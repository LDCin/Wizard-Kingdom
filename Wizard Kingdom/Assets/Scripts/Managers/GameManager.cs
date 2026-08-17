using System;
using System.Collections;
using System.Collections.Generic;
using Enemies;
using GestureRecognizer;
using ObjectPool;
using Particles;
using Wizards;
using SOs;
using StateMachines;
using UI;
using UnityEngine;
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

        [SerializeField] private EnemySpawner _enemySpawnerPrefab;
        [SerializeField] private Wizard _wizard;
        private EnemySpawner _currentEnemySpawner;
        [SerializeField] private Recognizer _recognizer;
        [SerializeField] private ParticlePool _particlePool;
        [SerializeField] private SpellCaster _spellCaster;
        // [SerializeField] private List<string> _spawnEnemyNameList;
        // [SerializeField] private float _delayTime = 1f;
        private GameModeData _currentModeData;
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
        private StateMachine<GameStateType> _stateMachine;
        public StateMachine<GameStateType> StateMachine => _stateMachine;
        private bool _isNewGame = true;
        public bool IsNewGame
        {
            get => _isNewGame;
            set
            {
                _isNewGame = value;
            }
        }
        [SerializeField] private bool _wizardDead = true;
        private Coroutine _startGameCoroutine;
        public override void Awake()
        {
            base.Awake();
            _stateMachine = new StateMachine<GameStateType>();
            _stateMachine.RegisterState(GameStateType.Menu, new MenuState(this));
            _stateMachine.RegisterState(GameStateType.Play, new PlayState(this));
            _stateMachine.RegisterState(GameStateType.Pause, new PauseState(this));
            _stateMachine.RegisterState(GameStateType.GameOver, new GameOverState(this));

            if (_spellCaster == null)
            {
                _spellCaster = GetComponentInChildren<SpellCaster>();
            }
        }

        private void OnEnable()
        {
            Observer.Subscribe(ObserverEvent.EnemyReachedCastle, ChangeToGameOverState);
            Observer.Subscribe<RewardPayload>(ObserverEvent.EnemyDied, HandleEnemyDied);
            Observer.Subscribe(ObserverEvent.BalloonPopped, HandleBalloonPop);
            Observer.Subscribe(ObserverEvent.PauseGame, ChangeToPauseState);
            Observer.Subscribe<string>(ObserverEvent.PlayGame, OnPlayGameSelected);
            Observer.Subscribe(ObserverEvent.BackToMenu, ChangeToMenuState);
            Observer.Subscribe(ObserverEvent.ContinueGame, ContinueGame);
            Observer.Subscribe(ObserverEvent.RestartGame, RestartGame);
            Observer.Subscribe(ObserverEvent.WizardDead, ChangeWizardState);
            Observer.Subscribe<string>(ObserverEvent.DrawSymbol, HandleComboStart);
            Observer.Subscribe<string>(ObserverEvent.DrawSymbol, HandleSkillGesture);
            Observer.Subscribe(ObserverEvent.RecognitionFinished, HandleComboEnd);
        }
        private void OnDisable()
        {
            Observer.Unsubscribe(ObserverEvent.EnemyReachedCastle, ChangeToGameOverState);
            Observer.Unsubscribe<RewardPayload>(ObserverEvent.EnemyDied, HandleEnemyDied);
            Observer.Unsubscribe(ObserverEvent.BalloonPopped, HandleBalloonPop);
            Observer.Unsubscribe(ObserverEvent.PauseGame, ChangeToPauseState);
            Observer.Unsubscribe<string>(ObserverEvent.PlayGame, OnPlayGameSelected);
            Observer.Unsubscribe(ObserverEvent.BackToMenu, ChangeToMenuState);
            Observer.Unsubscribe(ObserverEvent.ContinueGame, ContinueGame);
            Observer.Unsubscribe(ObserverEvent.RestartGame, RestartGame);
            Observer.Unsubscribe(ObserverEvent.WizardDead, ChangeWizardState);
            Observer.Unsubscribe<string>(ObserverEvent.DrawSymbol, HandleComboStart);
            Observer.Unsubscribe<string>(ObserverEvent.DrawSymbol, HandleSkillGesture);
            Observer.Unsubscribe(ObserverEvent.RecognitionFinished, HandleComboEnd);
        }

        private void Start()
        {
            Debug.Log("Start GameManager");
            _stateMachine.ChangeState(GameStateType.Menu);
            ParticlePool newParticlePool = Instantiate(_particlePool, transform);
            _particlePool = newParticlePool;
        }
        private void ChangeToPlayState()
        {
            _isGameOver = false;
            _stateMachine.ChangeState(GameStateType.Play);
        }
        private void OnPlayGameSelected(string modeKey)
        {
            StartCoroutine(LoadModeAndPlayRoutine(modeKey));
        }
        private IEnumerator LoadModeAndPlayRoutine(string modeKey)
        {
            _currentModeData = null;

            if (string.IsNullOrEmpty(modeKey))
            {
                Debug.LogWarning("GameManager: modeKey r?ng, không load du?c GameModeData.");
                yield break;
            }

            bool completed = false;
            DataManager.Instance.LoadGameModeData(modeKey, modeData =>
            {
                _currentModeData = modeData;
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            if (_currentModeData == null)
            {
                Debug.LogError($"GameManager: load GameModeData th?t b?i v?i key '{modeKey}'.");
                yield break;
            }

            Observer.Publish(ObserverEvent.ModeLoaded, _currentModeData);
            ChangeToPlayState();
        }
        private void ChangeToGameOverState()
        {
            _isGameOver = true;
            _stateMachine.ChangeState(GameStateType.GameOver);
        }
        private void ChangeToMenuState()
        {
            _isGameOver = false;
            _stateMachine.ChangeState(GameStateType.Menu);
        }

        private void ChangeToPauseState()
        {
            _stateMachine.ChangeState(GameStateType.Pause);
        }
        private void ChangeWizardState()
        {
            _wizardDead = true;
        }
        private void HandleEnemyDied(RewardPayload reward)
        {
            UpdateScoreAndGold(reward.Score, reward.Gold);
        }

        private void UpdateScoreAndGold(int newScore, int newGold)
        {
            _score += newScore;
            _gold += newGold;
            Observer.Publish(ObserverEvent.ScoreAndGoldChanged, new ScoreAndGoldPayload(_score, _gold));

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
            _wizardDead = false;
            _wizard?.ResetToIdleForNewGame();
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
                        Observer.Publish(ObserverEvent.TimeChanged, new TimePayload(_remainingTime, _totalTime));
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
                Observer.Publish(ObserverEvent.TimeChanged, new TimePayload(_remainingTime, _totalTime));
                yield return null;
            }

            _timeAttackCoroutine = null;
            Observer.Publish(ObserverEvent.TimeExpired);
            ChangeToGameOverState();
        }

        public void AddTime(float seconds)
        {
            if (_currentModeData == null || !_currentModeData.hasTime) return;
            if (seconds <= 0f) return;

            _remainingTime = Mathf.Min(_remainingTime + seconds, _totalTime);
            Observer.Publish(ObserverEvent.TimeChanged, new TimePayload(_remainingTime, _totalTime));
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
            _currentModeData = null;
        }
        public void GameOver()
        {
            StartCoroutine(GameOverRoutine());
        }
        private IEnumerator GameOverRoutine()
        {
            StopSpawnEnemy();
            TryVibrateGameOver();
            Observer.Publish(ObserverEvent.GameOver);
            yield return new WaitUntil(() => _wizardDead);

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

        private void TryVibrateGameOver()
        {
            if (!Application.isMobilePlatform) return;
            var dm = DataManager.Instance;
            if (dm != null && !dm.VibrationEnabled) return;
            Handheld.Vibrate();
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

                Observer.Publish(ObserverEvent.SpellCastRequested, new SpellCastRequestPayload(binding.pattern, binding.spell));
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



