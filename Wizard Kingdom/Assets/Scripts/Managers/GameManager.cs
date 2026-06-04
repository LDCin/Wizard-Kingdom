using System;
using System.Collections;
using System.Collections.Generic;
using BackgroundSystem;
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

        public static event Action<GesturePattern, SpellItemData> OnSpellCastRequested;
        public static event Action OnGameOver;
        public static event Action<int, int> OnUpdateScoreAndGold;
        public static event Action<GameModeData> OnModeLoaded;
        public static event Action<float, float> OnTimeChanged;
        public static event Action OnTimeExpired;
        [SerializeField] private EnemySpawner _enemySpawnerPrefab;
        [SerializeField] private Wizard _wizard;
        private EnemySpawner _currentEnemySpawner;
        [SerializeField] private Recognizer _recognizer;
        [SerializeField] private ParticlePool _particlePool;
        [SerializeField] private SpellCaster _spellCaster;
        // [SerializeField] private List<string> _spawnEnemyNameList;
        // [SerializeField] private float _delayTime = 1f;
        private GameModeData _currentModeData;
        private readonly HashSet<Enemy> _activeEnemies = new();
        private Coroutine _timeAttackCoroutine;
        private float _remainingTime;
        private float _totalTime;
        private int _comboPopCount;
        private bool _comboActive;
        private bool _isGameOver;

        [SerializeField] private float _startSpawnDelayTime = 5f;
        [SerializeField] private int _score;
        [SerializeField] private int _highScore;
        [SerializeField] private int _gold;
        [SerializeField] private List<GestureSpellBinding> _gestureSpellBindings = new();
        private StateMachine _stateMachine;
        private IState _playState;
        private IState _pauseState;
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
        [SerializeField] private bool _wizardDead = true;
        private Coroutine _startGameCoroutine;

        #region Analysis And Design Properties

        public int Score => _score;
        public int Gold => _gold;
        public float RemainingTime => _remainingTime;
        public EnemySpawner EnemySpawner => _currentEnemySpawner;
        public IReadOnlyCollection<Enemy> ActiveEnemy => _activeEnemies;
        public GameModeData CurrentGameMode => _currentModeData;
        public IReadOnlyList<string> OwnedSpellList => DataManager.Instance?.Data?.inventory?.ownedSpells;
        public IReadOnlyList<string> OwnedWizardList => DataManager.Instance?.Data?.inventory?.ownedWizards;
        public IReadOnlyList<string> OwnedBackgroundList => DataManager.Instance?.Data?.inventory?.ownedBackgrounds;
        public string CurrentWizard => DataManager.Instance?.GetEquippedWizard();
        public string CurrentBackground => DataManager.Instance?.GetEquippedBackground();

        #endregion

        #region Runtime Extension Properties

        public string CurrentModeKey => _currentModeData != null ? _currentModeData.modeName : null;
        public GameModeData CurrentModeData => _currentModeData;
        public float TotalTime => _totalTime;
        public bool IsGameOver => _isGameOver;
        public StateMachine StateMachine => _stateMachine;
        public IState PauseState => _pauseState;
        public IReadOnlyCollection<Enemy> ActiveEnemies => _activeEnemies;

        #endregion

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
            Wizard.OnDead += ChangeWizardState;
            GestureResultHandler.OnDrawSymbol += HandleComboStart;
            GestureResultHandler.OnDrawSymbol += HandleSkillGesture;
            GestureResultHandler.OnDrawSymbol += HandleRecognizedSymbol;
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
            Wizard.OnDead -= ChangeWizardState;
            GestureResultHandler.OnDrawSymbol -= HandleComboStart;
            GestureResultHandler.OnDrawSymbol -= HandleSkillGesture;
            GestureResultHandler.OnDrawSymbol -= HandleRecognizedSymbol;
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

            OnModeLoaded?.Invoke(_currentModeData);
            ChangeToPlayState();
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
        private void ChangeWizardState()
        {
            _wizardDead = true;
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
            _wizard?.Init();
            _spellCaster?.ResetSpellUses();
            InitGameStat();
            InitializeSceneDataAdapters();
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
        private IEnumerator GameOverRoutine()
        {
            StopSpawnEnemy();
            TryVibrateGameOver();
            OnGameOver?.Invoke();
            yield return new WaitUntil(() => _wizardDead);

            DataManager.Instance?.UpdateHighScoreAndGold(_currentModeData != null ? _currentModeData.modeName : null, _score, _gold);

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

                OnSpellCastRequested?.Invoke(binding.pattern, binding.spell);
                return;
            }
        }

        #region Analysis And Design Methods

        public void StartGame(string modeKey)
        {
            OnPlayGameSelected(modeKey);
        }
        public void startGame(string modeKey) => StartGame(modeKey);

        public void UpdateScoreAndGold(int newScore, int newGold)
        {
            _score += newScore;
            _gold += newGold;
            OnUpdateScoreAndGold?.Invoke(_score, _gold);

            if (_currentEnemySpawner != null)
            {
                _currentEnemySpawner.OnScoreChanged(_score);
            }
        }
        public void updateScoreAndGold(int newScore, int newGold)
        {
            UpdateScoreAndGold(newScore, newGold);
        }

        public void PauseGame()
        {
            ChangeToPauseState();
        }
        public void pauseGame() => PauseGame();

        public void ContinueGame()
        {
            ChangeToPlayState();
        }
        public void continueGame() => ContinueGame();

        public void RestartGame()
        {
            _isNewGame = true;
            ChangeToPlayState();
        }
        public void restartGame() => RestartGame();

        public void GameOver()
        {
            StartCoroutine(GameOverRoutine());
        }
        public void gameOver() => GameOver();

        public void HandleRecognizedSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            Enemy[] enemies = new Enemy[_activeEnemies.Count];
            _activeEnemies.CopyTo(enemies);

            foreach (Enemy enemy in enemies)
            {
                if (enemy == null) continue;

                if (enemy.Data != null)
                {
                    enemy.Data.CheckSymbol(enemy, symbol);
                }
                else
                {
                    enemy.CheckSymbol(symbol);
                }
            }
        }
        public void handleRecognizedSymbol(string symbol) => HandleRecognizedSymbol(symbol);

        public bool BuyItem(ShopItemData item, bool equipAfterPurchase = false)
        {
            if (item == null || DataManager.Instance == null) return false;
            if (IsOwned(item)) return false;
            if (item.price > 0 && !DataManager.Instance.SpendGold(item.price)) return false;

            bool added = item switch
            {
                SpellItemData spell => DataManager.Instance.AddSpell(spell.id),
                WizardItemData wizard => DataManager.Instance.AddWizard(wizard.id),
                BackgroundItemData background => DataManager.Instance.AddBackground(background.id),
                _ => false
            };

            if (added && equipAfterPurchase)
            {
                UseItem(item);
            }

            return added;
        }
        public bool buyItem(ShopItemData item, bool equipAfterPurchase = false)
        {
            return BuyItem(item, equipAfterPurchase);
        }

        public bool UseItem(ShopItemData item)
        {
            if (item == null || DataManager.Instance == null) return false;

            return item switch
            {
                WizardItemData wizard => DataManager.Instance.EquipWizard(wizard.id),
                BackgroundItemData background => DataManager.Instance.EquipBackground(background.id),
                SpellItemData spell => DataManager.Instance.OwnsSpell(spell.id),
                _ => false
            };
        }
        public bool useItem(ShopItemData item) => UseItem(item);

        public void ToggleBGM()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.ChangeBGMState(!DataManager.Instance.BgmEnabled);
        }
        public void toggleBGM() => ToggleBGM();

        public void ToggleSFX()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.ChangeSFXState(!DataManager.Instance.SfxEnabled);
        }
        public void toggleSFX() => ToggleSFX();

        public void ToggleVibration()
        {
            if (DataManager.Instance == null) return;
            DataManager.Instance.ChangeVibrationState(!DataManager.Instance.VibrationEnabled);
        }
        public void toggleVibration() => ToggleVibration();

        public void ResetData()
        {
            DataManager.Instance?.ResetUserData();
        }
        public void resetData() => ResetData();

        public void OpenSetting()
        {
            StartCoroutine(OpenSettingRoutine());
        }

        public void openSetting() => OpenSetting();

        #endregion

        private void InitializeSceneDataAdapters()
        {
            BackgroundLoader backgroundLoader = FindObjectOfType<BackgroundLoader>();
            backgroundLoader?.Init();

            WizardLoader wizardLoader = FindObjectOfType<WizardLoader>();
            wizardLoader?.Init();
        }

        private IEnumerator OpenSettingRoutine()
        {
            if (UIManager.Instance == null) yield break;

            UIManager.Instance.ClosePanel(GameConfig.Panel.Menu);
            yield return UIManager.Instance.LoadPanel(GameConfig.Panel.Setting);

            if (UIManager.Instance.GetPanel(GameConfig.Panel.Setting) is SettingPanel settingPanel)
            {
                settingPanel.GetData();
                settingPanel.Open();
            }
            else
            {
                UIManager.Instance.OpenPanel(GameConfig.Panel.Setting);
            }
        }

        private static bool IsOwned(ShopItemData item)
        {
            return item switch
            {
                SpellItemData spell => DataManager.Instance.OwnsSpell(spell.id),
                WizardItemData wizard => DataManager.Instance.OwnsWizard(wizard.id),
                BackgroundItemData background => DataManager.Instance.OwnsBackground(background.id),
                _ => false
            };
        }

        private bool IsGestureMatch(string a, string b)
        {
            return string.Equals(a, b, StringComparison.Ordinal);
        }

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

