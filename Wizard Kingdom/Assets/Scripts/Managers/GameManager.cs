using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using GestureRecognizer;
using ObjectPool;
using Particles;
using Players;
using SOs;
using StateMachines;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [Serializable]
        public class GestureSpellBinding
        {
            public GesturePattern pattern;
            public SpellItemData spell;
        }

        public static event Action<GesturePattern, SpellItemData> OnSpellCastRequested;
        public static event Action OnGameOver;
        public static event Action<int, int> OnUpdateScoreAndGold;
        public static event Action<GameModeData> OnModeLoaded;
        public static event Action<float, float> OnTimeChanged;
        public static event Action OnTimeExpired;
        [SerializeField] private EnemySpawner _enemySpawnerPrefab;
        [SerializeField] private Player _player;
        private EnemySpawner _currentEnemySpawner;
        [SerializeField] private Recognizer _recognizer;
        [SerializeField] private ParticlePool _particlePool;
        [SerializeField] private SpellCaster _spellCaster;
        // [SerializeField] private List<string> _spawnEnemyNameList;
        // [SerializeField] private float _delayTime = 1f;
        private GameModeData _currentModeData;
        private AsyncOperationHandle<GameModeData> _currentModeHandle;
        private Coroutine _timeAttackCoroutine;
        private float _remainingTime;
        private float _totalTime;
        private int _comboPopCount;
        private bool _comboActive;
        private bool _isGameOver;

        public string CurrentModeKey => _currentModeData != null ? _currentModeData.modeName : null;
        public GameModeData CurrentModeData => _currentModeData;
        public float RemainingTime => _remainingTime;
        public float TotalTime => _totalTime;
        public bool IsGameOver => _isGameOver;

        [SerializeField] private float _startSpawnDelayTime = 5f;
        [SerializeField] private int _score;
        [SerializeField] private int _highScore;
        [SerializeField] private int _gold;
        [SerializeField] private List<GestureSpellBinding> _gestureSpellBindings = new();
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

            if (_spellCaster == null)
            {
                _spellCaster = GetComponentInChildren<SpellCaster>();
            }
        }

        private void OnEnable()
        {
            Enemy.OnEnemyReachCastle += ChangeToGameOverState;
            Enemy.OnEnemyDie += UpdateScoreAndGold;
            Enemy.OnBalloonPop += HandleBalloonPop;
            GamePanel.OnPauseGame += ChangeToPauseState;
            MenuPanel.OnPlayGame += OnPlayGameSelected;
            PausePanel.OnBackToMenu += ChangeToMenuState;
            PausePanel.OnContinueGame += ContinueGame;
            PausePanel.OnRestartGame += RestartGame;
            GameOverPanel.OnRestartGame += RestartGame;
            GameOverPanel.OnBackToMenu += ChangeToMenuState;
            Player.OnDead += ChangePlayerState;
            GestureResultHandler.OnDrawSymbol += HandleComboStart;
            GestureResultHandler.OnDrawSymbol += HandleSkillGesture;
            GestureResultHandler.OnRecognitionFinished += HandleComboEnd;
        }
        private void OnDisable()
        {
            Enemy.OnEnemyReachCastle -= ChangeToGameOverState;
            Enemy.OnEnemyDie -= UpdateScoreAndGold;
            Enemy.OnBalloonPop -= HandleBalloonPop;
            GamePanel.OnPauseGame -= ChangeToPauseState;
            MenuPanel.OnPlayGame -= OnPlayGameSelected;
            PausePanel.OnBackToMenu -= ChangeToMenuState;
            PausePanel.OnContinueGame -= ContinueGame;
            PausePanel.OnRestartGame -= RestartGame;
            GameOverPanel.OnRestartGame -= RestartGame;
            GameOverPanel.OnBackToMenu -= ChangeToMenuState;
            Player.OnDead -= ChangePlayerState;
            GestureResultHandler.OnDrawSymbol -= HandleComboStart;
            GestureResultHandler.OnDrawSymbol -= HandleSkillGesture;
            GestureResultHandler.OnRecognitionFinished -= HandleComboEnd;
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
            _isGameOver = false;
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
                OnModeLoaded?.Invoke(_currentModeData);
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
            _isGameOver = true;
            _stateMachine.ChangeState(_gameOverState);
        }
        private void ChangeToMenuState()
        {
            _isGameOver = false;
            _stateMachine.ChangeState(_menuState);
        }

        private void ChangeToPauseState()
        {
            _stateMachine.ChangeState(_pauseState);
        }
        private void ChangePlayerState()
        {
            _playerDead = true;
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
            _player?.ResetToIdleForNewGame();
            _spellCaster?.ResetSpellUses();
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
                        _totalTime = _currentModeData.playTime;
                        _remainingTime = _totalTime;
                        OnTimeChanged?.Invoke(_remainingTime, _totalTime);
                        _timeAttackCoroutine = StartCoroutine(TimeAttackCountdownRoutine());
                    }
                }
                else
                {
                    // _currentEnemySpawner.StartSpawn(_spawnEnemyNameList, _delayTime);
                    Debug.LogWarning("GameManager: _currentModeData not set.");
                }
            }
            _startGameCoroutine = null;
        }

        private IEnumerator TimeAttackCountdownRoutine()
        {
            while (_remainingTime > 0f)
            {
                _remainingTime -= Time.deltaTime;
                if (_remainingTime < 0f) _remainingTime = 0f;
                OnTimeChanged?.Invoke(_remainingTime, _totalTime);
                yield return null;
            }

            _timeAttackCoroutine = null;
            OnTimeExpired?.Invoke();
            ChangeToGameOverState();
        }

        public void AddTime(float seconds)
        {
            if (_currentModeData == null || !_currentModeData.hasTime) return;
            if (seconds <= 0f) return;

            _remainingTime = Mathf.Min(_remainingTime + seconds, _totalTime);
            OnTimeChanged?.Invoke(_remainingTime, _totalTime);
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

        private void HandleComboStart(string shapeName)
        {
            _comboPopCount = 0;
            _comboActive = _currentModeData != null && _currentModeData.hasTime;
        }

        private void HandleBalloonPop()
        {
            if (!_comboActive) return;
            _comboPopCount++;
        }

        private void HandleComboEnd()
        {
            if (!_comboActive) return;

            if (_comboPopCount >= 2)
            {
                AddTime(_comboPopCount);
            }

            _comboPopCount = 0;
            _comboActive = false;
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
            SceneLoader.LoadScene(GameConfig.Scene.GameOver, GameConfig.Panel.GameOver);
            Debug.Log("GAME OVER!");
        }

        private void HandleSkillGesture(string gestureId)
        {
            if (string.IsNullOrWhiteSpace(gestureId)) return;

            for (int i = 0; i < _gestureSpellBindings.Count; i++)
            {
                GestureSpellBinding binding = _gestureSpellBindings[i];
                if (binding == null || binding.pattern == null || binding.spell == null) continue;

                string patternId = binding.pattern.id;
                if (string.IsNullOrWhiteSpace(patternId)) continue;

                if (!IsGestureMatch(gestureId, patternId)) continue;

                if (DataManager.Instance != null && !DataManager.Instance.OwnsSpell(binding.spell.id))
                {
                    Debug.LogWarning($"Spell '{binding.spell.id}' is not owned.");
                    return;
                }

                OnSpellCastRequested?.Invoke(binding.pattern, binding.spell);
                return;
            }
        }

        private bool IsGestureMatch(string a, string b)
        {
            return string.Equals(a, b, StringComparison.Ordinal);
        }

        private readonly HashSet<Enemy> _activeEnemies = new();
        public IReadOnlyCollection<Enemy> ActiveEnemies => _activeEnemies;

        public void RegisterEnemy(Enemy enemy)
        {
            if (enemy == null) return;
            _activeEnemies.Add(enemy);
        }

        public void UnregisterEnemy(Enemy enemy)
        {
            if (enemy == null) return;
            _activeEnemies.Remove(enemy);
        }
    }
}