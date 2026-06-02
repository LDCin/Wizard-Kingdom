using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Data;
using Newtonsoft.Json;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Managers
{
    public class DataManager : Singleton<DataManager>
    {
        public static event Action<int> OnCoinChanged;
        public static event Action<string, int> OnHighScoreChanged;
        public static event Action<string> OnEquippedBackgroundChanged;
        public static event Action<string> OnEquippedWizardChanged;
        public static event Action<string> OnBackgroundPurchased;
        public static event Action<string> OnWizardPurchased;
        public static event Action<string> OnSpellPurchased;
        public static event Action OnSettingsChanged;

        private sealed class DataSlot
        {
            public string key;
            public string filePath;
            public Type type;
            public Func<object> defaultFactory;
            public object data;
        }

        private readonly Dictionary<Type, DataSlot> _dataSlotsByType = new Dictionary<Type, DataSlot>();
        private readonly Dictionary<string, DataSlot> _dataSlotsByKey = new Dictionary<string, DataSlot>();
        private readonly Dictionary<string, AsyncOperationHandle<GameModeData>> _gameModeHandles
            = new Dictionary<string, AsyncOperationHandle<GameModeData>>();

        private JsonSerializerSettings _jsonSettings;
        private AsyncOperationHandle<GameplayCatalog> _gameplayCatalogHandle;
        private bool _hasGameplayCatalogHandle;
        private GameplayCatalog _gameplayCatalog;
        private AsyncOperationHandle<ShopCatalog> _shopCatalogHandle;
        private bool _hasShopCatalogHandle;
        private ShopCatalog _shopCatalog;
        public UserData Data => GetData<UserData>();

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [SerializeField] private EditorUserData _editorData = new EditorUserData();
#endif

        public override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };

            RegisterData("user", "userdata.json", UserData.CreateDefault);

#if UNITY_EDITOR
            LoadEditorDataFromUserData();
#endif
        }

        public void RegisterData<T>(string key, string fileName, Func<T> defaultFactory) where T : class
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Data key is required.", nameof(key));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
            if (defaultFactory == null) throw new ArgumentNullException(nameof(defaultFactory));

            Type type = typeof(T);
            if (_dataSlotsByType.ContainsKey(type))
            {
                return;
            }

            var slot = new DataSlot
            {
                key = key,
                filePath = Path.Combine(Application.persistentDataPath, fileName),
                type = type,
                defaultFactory = () => defaultFactory()
            };

            _dataSlotsByType[type] = slot;
            _dataSlotsByKey[key] = slot;
            LoadSlot(slot);
        }

        public bool HasData<T>() where T : class
        {
            return _dataSlotsByType.ContainsKey(typeof(T));
        }

        public T GetData<T>() where T : class
        {
            if (_dataSlotsByType.TryGetValue(typeof(T), out DataSlot slot))
            {
                return slot.data as T;
            }

            return null;
        }

        public T GetDataByKey<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (_dataSlotsByKey.TryGetValue(key, out DataSlot slot))
            {
                return slot.data as T;
            }

            return null;
        }

        public void SaveData<T>() where T : class
        {
            if (_dataSlotsByType.TryGetValue(typeof(T), out DataSlot slot))
            {
                SaveSlot(slot);
            }
        }

        public void ResetData<T>() where T : class
        {
            if (!_dataSlotsByType.TryGetValue(typeof(T), out DataSlot slot))
            {
                return;
            }

            slot.data = slot.defaultFactory();
            SaveSlot(slot);
        }

        public void LoadGameplayCatalog(Action<GameplayCatalog> onLoaded)
        {
            if (_gameplayCatalog != null)
            {
                onLoaded?.Invoke(_gameplayCatalog);
                return;
            }

            StartCoroutine(LoadGameplayCatalogRoutine(onLoaded));
        }

        public void LoadShopCatalog(Action<ShopCatalog> onLoaded)
        {
            if (_shopCatalog != null)
            {
                onLoaded?.Invoke(_shopCatalog);
                return;
            }

            StartCoroutine(LoadShopCatalogRoutine(onLoaded));
        }

        public void LoadGameModeData(string modeKey, Action<GameModeData> onLoaded)
        {
            if (string.IsNullOrWhiteSpace(modeKey))
            {
                onLoaded?.Invoke(null);
                return;
            }

            if (_gameModeHandles.TryGetValue(modeKey, out AsyncOperationHandle<GameModeData> cachedHandle)
                && cachedHandle.IsValid()
                && cachedHandle.Status == AsyncOperationStatus.Succeeded
                && cachedHandle.Result != null)
            {
                onLoaded?.Invoke(cachedHandle.Result);
                return;
            }

            StartCoroutine(LoadGameModeDataRoutine(modeKey, onLoaded));
        }

        public WizardData FindWizardData(string id)
        {
            return _gameplayCatalog != null ? _gameplayCatalog.FindWizard(id) : null;
        }

        public BackgroundData FindBackgroundData(string id)
        {
            return _gameplayCatalog != null ? _gameplayCatalog.FindBackground(id) : null;
        }

        public EnemyData FindEnemyData(GameModeData modeData, string enemyName)
        {
            if (modeData == null || string.IsNullOrWhiteSpace(enemyName)) return null;

            foreach (DifficultyTier tier in modeData.difficultyTiers)
            {
                if (tier == null || tier.enemies == null) continue;
                foreach (EnemySpawnEntry entry in tier.enemies)
                {
                    if (entry == null || entry.enemyData == null) continue;
                    if (string.Equals(entry.enemyData.enemyName, enemyName, StringComparison.Ordinal))
                    {
                        return entry.enemyData;
                    }
                }
            }

            return null;
        }

        private IEnumerator LoadGameplayCatalogRoutine(Action<GameplayCatalog> onLoaded)
        {
            _gameplayCatalogHandle = Addressables.LoadAssetAsync<GameplayCatalog>(GameConfig.Addressables.GameplayCatalog);
            yield return _gameplayCatalogHandle;

            if (_gameplayCatalogHandle.Status != AsyncOperationStatus.Succeeded || _gameplayCatalogHandle.Result == null)
            {
                Debug.LogError($"DataManager: failed to load GameplayCatalog at '{GameConfig.Addressables.GameplayCatalog}'.");
                _hasGameplayCatalogHandle = false;
                _gameplayCatalog = null;
                onLoaded?.Invoke(null);
                yield break;
            }

            _hasGameplayCatalogHandle = true;
            _gameplayCatalog = _gameplayCatalogHandle.Result;
            onLoaded?.Invoke(_gameplayCatalog);
        }

        private IEnumerator LoadShopCatalogRoutine(Action<ShopCatalog> onLoaded)
        {
            _shopCatalogHandle = Addressables.LoadAssetAsync<ShopCatalog>(GameConfig.Addressables.ShopCatalog);
            yield return _shopCatalogHandle;

            if (_shopCatalogHandle.Status != AsyncOperationStatus.Succeeded || _shopCatalogHandle.Result == null)
            {
                Debug.LogError($"DataManager: failed to load ShopCatalog at '{GameConfig.Addressables.ShopCatalog}'.");
                _hasShopCatalogHandle = false;
                _shopCatalog = null;
                onLoaded?.Invoke(null);
                yield break;
            }

            _hasShopCatalogHandle = true;
            _shopCatalog = _shopCatalogHandle.Result;
            onLoaded?.Invoke(_shopCatalog);
        }

        private IEnumerator LoadGameModeDataRoutine(string modeKey, Action<GameModeData> onLoaded)
        {
            AsyncOperationHandle<GameModeData> handle = Addressables.LoadAssetAsync<GameModeData>(modeKey);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"DataManager: failed to load GameModeData with key '{modeKey}'.");
                onLoaded?.Invoke(null);
                yield break;
            }

            _gameModeHandles[modeKey] = handle;
            onLoaded?.Invoke(handle.Result);
        }

        private void OnDestroy()
        {
            if (_hasGameplayCatalogHandle && _gameplayCatalogHandle.IsValid())
            {
                Addressables.Release(_gameplayCatalogHandle);
            }

            if (_hasShopCatalogHandle && _shopCatalogHandle.IsValid())
            {
                Addressables.Release(_shopCatalogHandle);
            }

            foreach (KeyValuePair<string, AsyncOperationHandle<GameModeData>> pair in _gameModeHandles)
            {
                if (pair.Value.IsValid())
                {
                    Addressables.Release(pair.Value);
                }
            }
            _gameModeHandles.Clear();
        }

        private void LoadSlot(DataSlot slot)
        {
            if (!File.Exists(slot.filePath))
            {
                slot.data = slot.defaultFactory();
                SaveSlot(slot);
                return;
            }

            try
            {
                string json = File.ReadAllText(slot.filePath);
                slot.data = JsonConvert.DeserializeObject(json, slot.type, _jsonSettings)
                           ?? slot.defaultFactory();
                EnsureDataValidity(slot);
            }
            catch (Exception e)
            {
                Debug.LogError($"DataManager: load failed '{slot.filePath}': {e.Message}. Reset to default.");
                slot.data = slot.defaultFactory();
                SaveSlot(slot);
            }
        }

        private void SaveSlot(DataSlot slot)
        {
            try
            {
                EnsureDataValidity(slot);
                string json = JsonConvert.SerializeObject(slot.data, slot.type, _jsonSettings);
                File.WriteAllText(slot.filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"DataManager: save failed '{slot.filePath}': {e.Message}");
            }
        }

        private static void EnsureDataValidity(DataSlot slot)
        {
            if (slot.data is UserData userData)
            {
                EnsureUserDataValidity(userData);
            }
        }

        private static void EnsureUserDataValidity(UserData userData)
        {
            if (userData == null)
            {
                return;
            }

            userData.stats ??= new StatsData();
            userData.stats.highScores ??= new Dictionary<string, int>();
            userData.inventory ??= new InventoryData();
            userData.inventory.ownedBackgrounds ??= new List<string>();
            userData.inventory.ownedWizards ??= new List<string>();
            userData.inventory.ownedSpells ??= new List<string>();
            userData.settings ??= new SettingsData();
        }

        public int GetCoin() => Data.stats.currentCoin;

        public void AddCoin(int amount)
        {
            if (amount == 0) return;
            Data.stats.currentCoin = Mathf.Max(0, Data.stats.currentCoin + amount);
            SaveData<UserData>();
            OnCoinChanged?.Invoke(Data.stats.currentCoin);
        }

        public bool TrySpendCoin(int amount)
        {
            if (amount < 0) return false;
            if (Data.stats.currentCoin < amount) return false;

            Data.stats.currentCoin -= amount;
            SaveData<UserData>();
            OnCoinChanged?.Invoke(Data.stats.currentCoin);
            return true;
        }

        public int GetHighScore(string modeKey)
        {
            if (string.IsNullOrEmpty(modeKey)) return 0;
            return Data.stats.highScores.TryGetValue(modeKey, out int v) ? v : 0;
        }

        public bool TrySetHighScore(string modeKey, int score)
        {
            if (string.IsNullOrEmpty(modeKey)) return false;
            int current = GetHighScore(modeKey);
            if (score <= current) return false;

            Data.stats.highScores[modeKey] = score;
            SaveData<UserData>();
            OnHighScoreChanged?.Invoke(modeKey, score);
            return true;
        }

        public bool OwnsBackground(string id) => Data.inventory.ownedBackgrounds.Contains(id);
        public bool OwnsWizard(string id) => Data.inventory.ownedWizards.Contains(id);
        public bool OwnsSpell(string id) => Data.inventory.ownedSpells.Contains(id);

        public bool AddBackground(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsBackground(id)) return false;
            Data.inventory.ownedBackgrounds.Add(id);
            SaveData<UserData>();
            OnBackgroundPurchased?.Invoke(id);
            return true;
        }

        public bool AddWizard(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsWizard(id)) return false;
            Data.inventory.ownedWizards.Add(id);
            SaveData<UserData>();
            OnWizardPurchased?.Invoke(id);
            return true;
        }

        public bool AddSpell(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsSpell(id)) return false;
            Data.inventory.ownedSpells.Add(id);
            SaveData<UserData>();
            OnSpellPurchased?.Invoke(id);
            return true;
        }

        public string GetEquippedBackground() => Data.inventory.equippedBackground;
        public string GetEquippedWizard() => Data.inventory.equippedWizard;

        public bool EquipBackground(string id)
        {
            if (!OwnsBackground(id)) return false;
            if (Data.inventory.equippedBackground == id) return false;

            Data.inventory.equippedBackground = id;
            SaveData<UserData>();
            OnEquippedBackgroundChanged?.Invoke(id);
            return true;
        }

        public bool EquipWizard(string id)
        {
            if (!OwnsWizard(id)) return false;
            if (Data.inventory.equippedWizard == id) return false;

            Data.inventory.equippedWizard = id;
            SaveData<UserData>();
            OnEquippedWizardChanged?.Invoke(id);
            return true;
        }

        public bool BgmEnabled => Data.settings.bgmEnabled;
        public bool SfxEnabled => Data.settings.sfxEnabled;
        public bool VibrationEnabled => Data.settings.vibrationEnabled;

        public void SetBgmEnabled(bool value)
        {
            if (Data.settings.bgmEnabled == value) return;
            Data.settings.bgmEnabled = value;
            SaveData<UserData>();
            OnSettingsChanged?.Invoke();
        }

        public void SetSfxEnabled(bool value)
        {
            if (Data.settings.sfxEnabled == value) return;
            Data.settings.sfxEnabled = value;
            SaveData<UserData>();
            OnSettingsChanged?.Invoke();
        }

        public void SetVibrationEnabled(bool value)
        {
            if (Data.settings.vibrationEnabled == value) return;
            Data.settings.vibrationEnabled = value;
            SaveData<UserData>();
            OnSettingsChanged?.Invoke();
        }

        public void ResetUserData()
        {
            ResetData<UserData>();

            OnCoinChanged?.Invoke(Data.stats.currentCoin);
            OnEquippedBackgroundChanged?.Invoke(Data.inventory.equippedBackground);
            OnEquippedWizardChanged?.Invoke(Data.inventory.equippedWizard);
            OnSettingsChanged?.Invoke();

            if (_dataSlotsByType.TryGetValue(typeof(UserData), out DataSlot userDataSlot))
            {
                Debug.Log($"DataManager: reset user data at {userDataSlot.filePath}");
            }
        }

#if UNITY_EDITOR
        public void LoadEditorDataFromUserData()
        {
            if (Data == null)
            {
                RegisterData("user", "userdata.json", UserData.CreateDefault);
            }

            _editorData.currentCoin = Data.stats.currentCoin;

            _editorData.highScores = new List<HighScoreEntry>();
            foreach (KeyValuePair<string, int> entry in Data.stats.highScores)
            {
                _editorData.highScores.Add(new HighScoreEntry
                {
                    modeKey = entry.Key,
                    score = entry.Value
                });
            }

            _editorData.ownedBackgrounds = new List<string>(Data.inventory.ownedBackgrounds);
            _editorData.ownedWizards = new List<string>(Data.inventory.ownedWizards);
            _editorData.ownedSpells = new List<string>(Data.inventory.ownedSpells);
            _editorData.equippedBackground = Data.inventory.equippedBackground;
            _editorData.equippedWizard = Data.inventory.equippedWizard;

            _editorData.bgmEnabled = Data.settings.bgmEnabled;
            _editorData.sfxEnabled = Data.settings.sfxEnabled;
            _editorData.vibrationEnabled = Data.settings.vibrationEnabled;
        }

        public void ApplyEditorDataToUserData(bool triggerEvents = true)
        {
            if (Data == null)
            {
                RegisterData("user", "userdata.json", UserData.CreateDefault);
            }

            Data.stats.currentCoin = Mathf.Max(0, _editorData.currentCoin);
            Data.stats.highScores = new Dictionary<string, int>();

            if (_editorData.highScores != null)
            {
                foreach (HighScoreEntry entry in _editorData.highScores)
                {
                    if (string.IsNullOrEmpty(entry.modeKey))
                    {
                        continue;
                    }

                    Data.stats.highScores[entry.modeKey] = entry.score;
                }
            }

            Data.inventory.ownedBackgrounds = _editorData.ownedBackgrounds != null
                ? new List<string>(_editorData.ownedBackgrounds)
                : new List<string>();
            Data.inventory.ownedWizards = _editorData.ownedWizards != null
                ? new List<string>(_editorData.ownedWizards)
                : new List<string>();
            Data.inventory.ownedSpells = _editorData.ownedSpells != null
                ? new List<string>(_editorData.ownedSpells)
                : new List<string>();
            Data.inventory.equippedBackground = _editorData.equippedBackground;
            Data.inventory.equippedWizard = _editorData.equippedWizard;

            Data.settings.bgmEnabled = _editorData.bgmEnabled;
            Data.settings.sfxEnabled = _editorData.sfxEnabled;
            Data.settings.vibrationEnabled = _editorData.vibrationEnabled;

            SaveData<UserData>();

            if (triggerEvents)
            {
                OnCoinChanged?.Invoke(Data.stats.currentCoin);
                OnEquippedBackgroundChanged?.Invoke(Data.inventory.equippedBackground);
                OnEquippedWizardChanged?.Invoke(Data.inventory.equippedWizard);
                OnSettingsChanged?.Invoke();

                foreach (KeyValuePair<string, int> entry in Data.stats.highScores)
                {
                    OnHighScoreChanged?.Invoke(entry.Key, entry.Value);
                }
            }
        }

        public void SaveUserData()
        {
            SaveData<UserData>();
        }

        [Serializable]
        public class HighScoreEntry
        {
            public string modeKey;
            public int score;
        }

        [Serializable]
        public class EditorUserData
        {
            public string id;
            public int currentCoin;
            public List<HighScoreEntry> highScores = new List<HighScoreEntry>();
            public List<string> ownedBackgrounds = new List<string>();
            public List<string> ownedWizards = new List<string>();
            public List<string> ownedSpells = new List<string>();
            public string equippedBackground;
            public string equippedWizard;
            public bool bgmEnabled = true;
            public bool sfxEnabled = true;
            public bool vibrationEnabled = true;
        }

#endif

#if UNITY_EDITOR
        [ContextMenu("Reset User Data")]
        private void ResetUserDataMenu() => ResetUserData();

        [ContextMenu("Open Save Folder")]
        private void OpenSaveFolder()
        {
            if (_dataSlotsByType.TryGetValue(typeof(UserData), out DataSlot userDataSlot))
            {
                UnityEditor.EditorUtility.RevealInFinder(userDataSlot.filePath);
            }
        }
#endif
    }
}
