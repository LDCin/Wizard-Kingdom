using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Balloons;
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
        public static event Action<int> OnGoldChanged;
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

        #region Analysis And Design Configuration Methods

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
                onLoaded?.Invoke(cachedHandle.Result.Get());
                return;
            }

            StartCoroutine(LoadGameModeDataRoutine(modeKey, onLoaded));
        }

        public WizardData FindWizardData(string id)
        {
            return _gameplayCatalog != null ? _gameplayCatalog.FindWizard(id)?.Get() : null;
        }

        public BackgroundData FindBackgroundData(string id)
        {
            return _gameplayCatalog != null ? _gameplayCatalog.FindBackground(id)?.Get() : null;
        }

        public BalloonData FindBalloonData(string id)
        {
            return _gameplayCatalog != null ? _gameplayCatalog.FindBalloon(id)?.Get() : null;
        }

        public BalloonData FindBalloonData(Symbol symbol)
        {
            return _gameplayCatalog != null ? _gameplayCatalog.FindBalloon(symbol)?.Get() : null;
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
                        return entry.enemyData.Get();
                    }
                }
            }

            return null;
        }

        public void loadGameModeData(string modeKey, Action<GameModeData> onLoaded)
        {
            LoadGameModeData(modeKey, onLoaded);
        }

        public EnemyData findEnemyData(GameModeData modeData, string enemyName)
        {
            return FindEnemyData(modeData, enemyName);
        }

        public WizardData findWizardData(string id) => FindWizardData(id);
        public BackgroundData findBackgroundData(string id) => FindBackgroundData(id);
        public BalloonData findBalloonData(string id) => FindBalloonData(id);
        public BalloonData findBalloonData(Symbol symbol) => FindBalloonData(symbol);

        #endregion

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
            onLoaded?.Invoke(handle.Result.Get());
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

        #region Analysis And Design User Data Methods

        public int GetGold() => Data.Get().stats.currentGold;

        public void AddGold(int amount)
        {
            if (amount == 0) return;
            UpdateHighScoreAndGold(null, 0, amount);
        }

        public bool SpendGold(int amount)
        {
            if (amount < 0) return false;
            UserData userData = Data.Get();
            if (userData.stats.currentGold < amount) return false;

            userData.Set(data => data.stats.UpdateData(null, 0, -amount));
            SaveData<UserData>();
            OnGoldChanged?.Invoke(Data.stats.currentGold);
            return true;
        }

        public int GetHighScore(string modeKey)
        {
            if (string.IsNullOrEmpty(modeKey)) return 0;
            StatsData stats = Data.Get().stats.GetData();
            return stats.highScores.TryGetValue(modeKey, out int v) ? v : 0;
        }

        public bool TrySetHighScore(string modeKey, int score)
        {
            if (string.IsNullOrEmpty(modeKey)) return false;
            return UpdateHighScoreAndGold(modeKey, score, 0);
        }

        public bool SetHighScore(string modeKey, int score) => TrySetHighScore(modeKey, score);

        public bool UpdateHighScoreAndGold(string modeKey, int score, int gold)
        {
            UserData userData = Data.Get();
            int previousGold = userData.stats.currentGold;
            int previousHighScore = GetHighScore(modeKey);
            bool changed = false;

            userData.Set(data =>
            {
                changed = data.stats.UpdateData(modeKey, score, gold);
            });

            if (!changed) return false;

            SaveData<UserData>();

            if (Data.stats.currentGold != previousGold)
            {
                OnGoldChanged?.Invoke(Data.stats.currentGold);
            }

            int currentHighScore = GetHighScore(modeKey);
            if (!string.IsNullOrEmpty(modeKey) && currentHighScore > previousHighScore)
            {
                OnHighScoreChanged?.Invoke(modeKey, currentHighScore);
            }

            return true;
        }

        public bool OwnsBackground(string id)
        {
            return !string.IsNullOrEmpty(id)
                && Data.Get().inventory.GetData().ownedBackgrounds.Contains(id);
        }

        public bool OwnsWizard(string id)
        {
            return !string.IsNullOrEmpty(id)
                && Data.Get().inventory.GetData().ownedWizards.Contains(id);
        }

        public bool OwnsSpell(string id)
        {
            return !string.IsNullOrEmpty(id)
                && Data.Get().inventory.GetData().ownedSpells.Contains(id);
        }

        public bool AddBackground(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsBackground(id)) return false;
            Data.Set(data =>
            {
                data.inventory.GetData().ownedBackgrounds.Add(id);
                data.inventory.UpdateData();
            });
            SaveData<UserData>();
            ApplyShopItemDataState(FindShopItemData(ShopCategory.Background, id));
            OnBackgroundPurchased?.Invoke(id);
            return true;
        }

        public bool AddWizard(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsWizard(id)) return false;
            Data.Set(data =>
            {
                data.inventory.GetData().ownedWizards.Add(id);
                data.inventory.UpdateData();
            });
            SaveData<UserData>();
            ApplyShopItemDataState(FindShopItemData(ShopCategory.Wizard, id));
            OnWizardPurchased?.Invoke(id);
            return true;
        }

        public bool AddSpell(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsSpell(id)) return false;
            Data.Set(data =>
            {
                data.inventory.GetData().ownedSpells.Add(id);
                data.inventory.UpdateData();
            });
            SaveData<UserData>();
            ApplyShopItemDataState(FindShopItemData(ShopCategory.Spell, id));
            OnSpellPurchased?.Invoke(id);
            return true;
        }

        public string GetEquippedBackground() => Data.Get().inventory.GetData().equippedBackground;
        public string GetEquippedWizard() => Data.Get().inventory.GetData().equippedWizard;
        public string getEquippedBackground() => GetEquippedBackground();
        public string getEquippedWizard() => GetEquippedWizard();

        public bool EquipBackground(string id)
        {
            if (!OwnsBackground(id)) return false;
            if (Data.inventory.GetData().equippedBackground == id) return false;

            Data.Set(data =>
            {
                data.inventory.GetData().equippedBackground = id;
                data.inventory.UpdateData();
            });
            SaveData<UserData>();
            RefreshShopItemDataState(ShopCategory.Background);
            OnEquippedBackgroundChanged?.Invoke(id);
            return true;
        }

        public bool EquipWizard(string id)
        {
            if (!OwnsWizard(id)) return false;
            if (Data.inventory.GetData().equippedWizard == id) return false;

            Data.Set(data =>
            {
                data.inventory.GetData().equippedWizard = id;
                data.inventory.UpdateData();
            });
            SaveData<UserData>();
            RefreshShopItemDataState(ShopCategory.Wizard);
            OnEquippedWizardChanged?.Invoke(id);
            return true;
        }

        public bool BgmEnabled => Data.Get().settings.GetData().bgmEnabled;
        public bool SfxEnabled => Data.Get().settings.GetData().sfxEnabled;
        public bool VibrationEnabled => Data.Get().settings.GetData().vibrationEnabled;

        public void SetBgmEnabled(bool value)
        {
            if (Data.Get().settings.bgmEnabled == value) return;
            Data.Set(data => data.settings.UpdateData(bgmEnabled: value));
            SaveData<UserData>();
            OnSettingsChanged?.Invoke();
        }

        public void ChangeBGMState(bool value) => SetBgmEnabled(value);

        public void SetSfxEnabled(bool value)
        {
            if (Data.Get().settings.sfxEnabled == value) return;
            Data.Set(data => data.settings.UpdateData(sfxEnabled: value));
            SaveData<UserData>();
            OnSettingsChanged?.Invoke();
        }

        public void ChangeSFXState(bool value) => SetSfxEnabled(value);

        public void SetVibrationEnabled(bool value)
        {
            if (Data.Get().settings.vibrationEnabled == value) return;
            Data.Set(data => data.settings.UpdateData(vibrationEnabled: value));
            SaveData<UserData>();
            OnSettingsChanged?.Invoke();
        }

        public void ChangeVibrationState(bool value) => SetVibrationEnabled(value);

        public IReadOnlyList<ShopItemData> GetItemData()
        {
            List<ShopItemData> items = new List<ShopItemData>();
            foreach (ShopCategory category in Enum.GetValues(typeof(ShopCategory)))
            {
                items.AddRange(GetItemData(category));
            }

            return items;
        }

        public IReadOnlyList<ShopItemData> GetItemData(ShopCategory category)
        {
            if (_shopCatalog == null) return Array.Empty<ShopItemData>();

            Data.Get().inventory.GetData();
            List<ShopItemData> items = new List<ShopItemData>();

            switch (category)
            {
                case ShopCategory.Background:
                    AddShopItems(items, _shopCatalog.backgrounds);
                    break;
                case ShopCategory.Wizard:
                    AddShopItems(items, _shopCatalog.wizards);
                    break;
                case ShopCategory.Spell:
                    AddShopItems(items, _shopCatalog.spells);
                    break;
            }

            foreach (ShopItemData item in items)
            {
                ApplyShopItemDataState(item);
            }

            return items;
        }

        public IReadOnlyList<TItem> GetItemData<TItem>(ShopCategory category) where TItem : ShopItemData
        {
            IReadOnlyList<ShopItemData> items = GetItemData(category);
            List<TItem> typedItems = new List<TItem>();

            foreach (ShopItemData item in items)
            {
                if (item is TItem typedItem)
                {
                    typedItems.Add(typedItem);
                }
            }

            return typedItems;
        }

        public bool spendGold(int amount) => SpendGold(amount);
        public bool updateHighScoreAndGold(string modeKey, int score, int gold)
        {
            return UpdateHighScoreAndGold(modeKey, score, gold);
        }
        public bool addBackground(string id) => AddBackground(id);
        public bool addWizard(string id) => AddWizard(id);
        public bool addSpell(string id) => AddSpell(id);
        public bool equipBackground(string id) => EquipBackground(id);
        public bool equipWizard(string id) => EquipWizard(id);
        public void changeBGMState(bool value) => ChangeBGMState(value);
        public void changeSFXState(bool value) => ChangeSFXState(value);
        public void changeVibrationState(bool value) => ChangeVibrationState(value);
        public void resetUserData() => ResetUserData();
        public IReadOnlyList<ShopItemData> getItemData() => GetItemData();
        public IReadOnlyList<ShopItemData> getItemData(ShopCategory category) => GetItemData(category);

        public void ResetUserData()
        {
            ResetData<UserData>();

            OnGoldChanged?.Invoke(Data.stats.currentGold);
            OnEquippedBackgroundChanged?.Invoke(Data.inventory.equippedBackground);
            OnEquippedWizardChanged?.Invoke(Data.inventory.equippedWizard);
            OnSettingsChanged?.Invoke();

            if (_dataSlotsByType.TryGetValue(typeof(UserData), out DataSlot userDataSlot))
            {
                Debug.Log($"DataManager: reset user data at {userDataSlot.filePath}");
            }
        }

        #endregion

        private static void AddShopItems<TItem>(List<ShopItemData> target, IEnumerable<TItem> source)
            where TItem : ShopItemData
        {
            if (source == null) return;
            foreach (TItem item in source)
            {
                if (item != null)
                {
                    target.Add(item);
                }
            }
        }

        private ShopItemData FindShopItemData(ShopCategory category, string id)
        {
            if (_shopCatalog == null || string.IsNullOrEmpty(id)) return null;

            foreach (ShopItemData item in GetItemData(category))
            {
                if (item != null && string.Equals(item.id, id, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        private void RefreshShopItemDataState(ShopCategory category)
        {
            foreach (ShopItemData item in GetItemData(category))
            {
                ApplyShopItemDataState(item);
            }
        }

        private void ApplyShopItemDataState(ShopItemData item)
        {
            if (item == null) return;
            item.GetData().UpdateData(IsOwned(item), IsEquipped(item));
        }

        private bool IsOwned(ShopItemData item)
        {
            return item switch
            {
                BackgroundItemData background => OwnsBackground(background.id),
                WizardItemData wizard => OwnsWizard(wizard.id),
                SpellItemData spell => OwnsSpell(spell.id),
                _ => false
            };
        }

        private bool IsEquipped(ShopItemData item)
        {
            return item switch
            {
                BackgroundItemData background => GetEquippedBackground() == background.id,
                WizardItemData wizard => GetEquippedWizard() == wizard.id,
                _ => false
            };
        }

#if UNITY_EDITOR
        public void LoadEditorDataFromUserData()
        {
            if (Data == null)
            {
                RegisterData("user", "userdata.json", UserData.CreateDefault);
            }

            _editorData.currentGold = Data.stats.currentGold;

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

            Data.stats.currentGold = Mathf.Max(0, _editorData.currentGold);
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
                OnGoldChanged?.Invoke(Data.stats.currentGold);
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
            public int currentGold;
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
