using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using GestureRecognizer;
using ObjectPool;
using Particles;
using Players;
using SOs; // ADDED: dùng GameModeData
using StateMachines;
using UI;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AddressableAssets; // ADDED: load GameModeData qua Addressable
using UnityEngine.ResourceManagement.AsyncOperations; // ADDED
using Utils;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public static event Action OnGameOver;
        public static event Action<int, int> OnUpdateScoreAndGold;
        [SerializeField] private EnemySpawner _enemySpawnerPrefab;
        private EnemySpawner _currentEnemySpawner;
        [SerializeField] private Recognizer _recognizer;
        [SerializeField] private ParticlePool _particlePool;
        // [SerializeField] private List<string> _spawnEnemyNameList;
        // [SerializeField] private float _delayTime = 1f;
        private GameModeData _currentModeData;
        private AsyncOperationHandle<GameModeData> _currentModeHandle;
        private Coroutine _timeAttackCoroutine;

        public string CurrentModeKey => _currentModeData != null ? _currentModeData.modeName : null;

        [SerializeField] private float _startSpawnDelayTime = 5f;
        [SerializeField] private int _score;
        [SerializeField] private int _highScore;
        [SerializeField] private int _gold;
        public int Score => _score;
        public int Gold => _gold;
        private StateMachine _stateMachine;
        public StateMachine StateMachine => _stateMachine;
        private IState _playState;
        private IState _pauseState;
        public IState PauseState => _pauseState;
        private IState _menuState;
        private IState _gameOverState;
        private bool _isNewGame = true;
        public bool IsNewGame
        {
            get => _isNewGame;
            set
            {
                _isNewGame = value;
            }
        }
        [SerializeField] private bool _playerDead = true;
        private Coroutine _startGameCoroutine;
        public override void Awake()
        {
            base.Awake();
            _stateMachine = new StateMachine();
            _playState = new PlayState(this);
            _pauseState = new PauseState(this);
            _menuState = new MenuState(this);
            _gameOverState = new GameOverState(this);
        }

        private void OnEnable()
        {
            Enemy.OnEnemyReachCastle += ChangeToGameOverState;
            Enemy.OnEnemyDie += UpdateScoreAndGold;
            GamePanel.OnPauseGame += ChangeToPauseState;
            MenuPanel.OnPlayGame += OnPlayGameSelected;
            PausePanel.OnBackToMenu += ChangeToMenuState;
            PausePanel.OnContinueGame += ContinueGame;
            PausePanel.OnRestartGame += RestartGame;
            GameOverPanel.OnRestartGame += RestartGame;
            GameOverPanel.OnBackToMenu += ChangeToMenuState;
            Player.OnDead += ChangePlayerState;
        }
        private void OnDisable()
        {
            Enemy.OnEnemyReachCastle -= ChangeToGameOverState;
            Enemy.OnEnemyDie -= UpdateScoreAndGold;
            GamePanel.OnPauseGame -= ChangeToPauseState;
            MenuPanel.OnPlayGame -= OnPlayGameSelected;
            PausePanel.OnBackToMenu -= ChangeToMenuState;
            PausePanel.OnContinueGame -= ContinueGame;
            PausePanel.OnRestartGame -= RestartGame;
            GameOverPanel.OnRestartGame -= RestartGame;
            GameOverPanel.OnBackToMenu -= ChangeToMenuState;
            Player.OnDead -= ChangePlayerState;
        }

        private void Start()
        {
            Debug.Log("Start GameManager");
            _stateMachine.ChangeState(_menuState);
            ParticlePool newParticlePool = Instantiate(_particlePool, transform);
            _particlePool = newParticlePool;
        }
        private void ChangeToPlayState()
        {
            _stateMachine.ChangeState(_playState);
        }
        private void OnPlayGameSelected(string modeKey)
        {
            StartCoroutine(LoadModeAndPlayRoutine(modeKey));
        }
        private IEnumerator LoadModeAndPlayRoutine(string modeKey)
        {
            ReleaseCurrentModeHandle();

            if (string.IsNullOrEmpty(modeKey))
            {
                Debug.LogWarning("GameManager: modeKey rỗng, không load được GameModeData.");
                yield break;
            }

            _currentModeHandle = Addressables.LoadAssetAsync<GameModeData>(modeKey);
            yield return _currentModeHandle;

            if (_currentModeHandle.Status == AsyncOperationStatus.Succeeded)
            {
                _currentModeData = _currentModeHandle.Result;
                ChangeToPlayState();
            }
            else
            {
                Debug.LogError($"GameManager: load GameModeData thất bại với key '{modeKey}'.");
                ReleaseCurrentModeHandle();
            }
        }
        private void ReleaseCurrentModeHandle()
        {
            if (_currentModeHandle.IsValid())
            {
                Addressables.Release(_currentModeHandle);
            }
            _currentModeData = null;
        }
        private void ChangeToGameOverState()
        {
            _stateMachine.ChangeState(_gameOverState);
        }
        private void ChangeToMenuState()
        {
            _stateMachine.ChangeState(_menuState);
        }

        private void ChangeToPauseState()
        {
            _stateMachine.ChangeState(_pauseState);
        }
        private void ChangePlayerState()
        {
            _playerDead = !_playerDead;
        }
        private void UpdateScoreAndGold(int newScore, int newGold)
        {
            _score += newScore;
            _gold += newGold;
            OnUpdateScoreAndGold?.Invoke(_score, _gold);

            if (_currentEnemySpawner != null)
            {
                _currentEnemySpawner.OnScoreChanged(_score);
            }
        }
        private void InitGameStat()
        {
            UpdateScoreAndGold(-_score, -_gold);
        }
        public void StartGame()
        {
            if (!_isNewGame) return;
            _isNewGame = false;
            _playerDead = false;
            InitGameStat();
            DestroyEnemySpawner();
            _currentEnemySpawner = Instantiate(_enemySpawnerPrefab, transform);
            _startGameCoroutine = StartCoroutine(StartGameRoutine());
        }
        public IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(_startSpawnDelayTime);
            if (_currentEnemySpawner != null)
            {
                if (_currentModeData != null)
                {
                    _currentEnemySpawner.StartSpawn(_currentModeData);

                    if (_currentModeData.hasTime)
                    {
                        _timeAttackCoroutine = StartCoroutine(TimeAttackCountdownRoutine(_currentModeData.playTime));
                    }
                }
                else
                {
                    // _currentEnemySpawner.StartSpawn(_spawnEnemyNameList, _delayTime);
                    Debug.LogWarning("GameManager: _currentModeData chưa được set.");
                }
            }
            _startGameCoroutine = null;
        }

        private IEnumerator TimeAttackCountdownRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _timeAttackCoroutine = null;
            ChangeToGameOverState();
        }
        private void ContinueGame()
        {
            ChangeToPlayState();
        }
        private void RestartGame()
        {
            _isNewGame = true;
            ChangeToPlayState();
        }

        public void StopSpawnEnemy()
        {
            if (_currentEnemySpawner != null)
            {
                _currentEnemySpawner.StopSpawn();
            }
        }

        public void DestroyEnemySpawner()
        {
            if (_startGameCoroutine != null)
            {
                StopCoroutine(_startGameCoroutine);
                _startGameCoroutine = null;
            }

            if (_timeAttackCoroutine != null)
            {
                StopCoroutine(_timeAttackCoroutine);
                _timeAttackCoroutine = null;
            }

            if (_currentEnemySpawner != null)
            {
                _currentEnemySpawner.StopSpawn();
                Destroy(_currentEnemySpawner.gameObject);
                _currentEnemySpawner = null;
            }
        }
        public void BackToMenu()
        {
            _isNewGame = true;
            DestroyEnemySpawner();
            ReleaseCurrentModeHandle();
        }
        public void GameOver()
        {
            StartCoroutine(GameOverRoutine());
        }
        private IEnumerator GameOverRoutine()
        {
            StopSpawnEnemy();
            OnGameOver?.Invoke();
            yield return new WaitUntil(() => _playerDead);

            if (_gold > 0)
            {
                DataManager.Instance.AddCoin(_gold);
            }

            if (_currentModeData != null)
            {
                DataManager.Instance.TrySetHighScore(_currentModeData.modeName, _score);
            }

            DestroyEnemySpawner();
            SceneLoader.LoadScene("Game Over", "Panel - Game Over");
            Debug.Log("GAME OVER!");
        }
    }
}