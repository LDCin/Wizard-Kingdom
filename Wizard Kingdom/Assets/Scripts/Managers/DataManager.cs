using System;
using System.IO;
using System.Collections.Generic;
using Data;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Managers
{
    public class DataManager : Singleton<DataManager>
    {
        private const string FileName = "userdata.json";
        public static event Action<int> OnCoinChanged;
        public static event Action<string, int> OnHighScoreChanged;
        public static event Action<string> OnEquippedBackgroundChanged;
        public static event Action<string> OnEquippedWizardChanged;
        public static event Action<string> OnBackgroundPurchased;
        public static event Action<string> OnWizardPurchased;
        public static event Action<string> OnSpellPurchased;
        public static event Action OnSettingsChanged;

        private UserData _data;
        private string _filePath;
        private JsonSerializerSettings _jsonSettings;
        public UserData Data => _data;

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [SerializeField] private EditorUserData _editorData = new EditorUserData();
#endif

        public override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            _filePath = Path.Combine(Application.persistentDataPath, FileName);
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };

            Load();
#if UNITY_EDITOR
            LoadEditorDataFromUserData();
#endif
        }
        private void Load()
        {
            if (!File.Exists(_filePath))
            {
                _data = UserData.CreateDefault();
                Save();
                return;
            }

            try
            {
                string json = File.ReadAllText(_filePath);
                _data = JsonConvert.DeserializeObject<UserData>(json, _jsonSettings)
                        ?? UserData.CreateDefault();

                _data.stats ??= new StatsData();
                _data.stats.highScores ??= new System.Collections.Generic.Dictionary<string, int>();
                _data.inventory ??= new InventoryData();
                _data.inventory.ownedBackgrounds ??= new System.Collections.Generic.List<string>();
                _data.inventory.ownedWizards ??= new System.Collections.Generic.List<string>();
                _data.inventory.ownedSpells ??= new System.Collections.Generic.List<string>();
                _data.settings ??= new SettingsData();
            }
            catch (Exception e)
            {
                Debug.LogError($"DataManager: lỗi khi load '{_filePath}': {e.Message}. Reset về default.");
                _data = UserData.CreateDefault();
                Save();
            }
        }

        private void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_data, _jsonSettings);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"DataManager: lỗi khi save '{_filePath}': {e.Message}");
            }
        }

        public int GetCoin() => _data.stats.currentCoin;

        public void AddCoin(int amount)
        {
            if (amount == 0) return;
            _data.stats.currentCoin = Mathf.Max(0, _data.stats.currentCoin + amount);
            Save();
            OnCoinChanged?.Invoke(_data.stats.currentCoin);
        }

        public bool TrySpendCoin(int amount)
        {
            if (amount < 0) return false;
            if (_data.stats.currentCoin < amount) return false;

            _data.stats.currentCoin -= amount;
            Save();
            OnCoinChanged?.Invoke(_data.stats.currentCoin);
            return true;
        }

        public int GetHighScore(string modeKey)
        {
            if (string.IsNullOrEmpty(modeKey)) return 0;
            return _data.stats.highScores.TryGetValue(modeKey, out int v) ? v : 0;
        }

        public bool TrySetHighScore(string modeKey, int score)
        {
            if (string.IsNullOrEmpty(modeKey)) return false;
            int current = GetHighScore(modeKey);
            if (score <= current) return false;

            _data.stats.highScores[modeKey] = score;
            Save();
            OnHighScoreChanged?.Invoke(modeKey, score);
            return true;
        }

        public bool OwnsBackground(string id) => _data.inventory.ownedBackgrounds.Contains(id);
        public bool OwnsWizard(string id) => _data.inventory.ownedWizards.Contains(id);
        public bool OwnsSpell(string id) => _data.inventory.ownedSpells.Contains(id);

        public bool AddBackground(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsBackground(id)) return false;
            _data.inventory.ownedBackgrounds.Add(id);
            Save();
            OnBackgroundPurchased?.Invoke(id);
            return true;
        }

        public bool AddWizard(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsWizard(id)) return false;
            _data.inventory.ownedWizards.Add(id);
            Save();
            OnWizardPurchased?.Invoke(id);
            return true;
        }

        public bool AddSpell(string id)
        {
            if (string.IsNullOrEmpty(id) || OwnsSpell(id)) return false;
            _data.inventory.ownedSpells.Add(id);
            Save();
            OnSpellPurchased?.Invoke(id);
            return true;
        }

        public string GetEquippedBackground() => _data.inventory.equippedBackground;
        public string GetEquippedWizard() => _data.inventory.equippedWizard;

        public bool EquipBackground(string id)
        {
            if (!OwnsBackground(id)) return false;
            if (_data.inventory.equippedBackground == id) return false;

            _data.inventory.equippedBackground = id;
            Save();
            OnEquippedBackgroundChanged?.Invoke(id);
            return true;
        }

        public bool EquipWizard(string id)
        {
            if (!OwnsWizard(id)) return false;
            if (_data.inventory.equippedWizard == id) return false;

            _data.inventory.equippedWizard = id;
            Save();
            OnEquippedWizardChanged?.Invoke(id);
            return true;
        }

        public bool BgmEnabled => _data.settings.bgmEnabled;
        public bool SfxEnabled => _data.settings.sfxEnabled;
        public bool VibrationEnabled => _data.settings.vibrationEnabled;

        public void SetBgmEnabled(bool value)
        {
            if (_data.settings.bgmEnabled == value) return;
            _data.settings.bgmEnabled = value;
            Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetSfxEnabled(bool value)
        {
            if (_data.settings.sfxEnabled == value) return;
            _data.settings.sfxEnabled = value;
            Save();
            OnSettingsChanged?.Invoke();
        }

        public void SetVibrationEnabled(bool value)
        {
            if (_data.settings.vibrationEnabled == value) return;
            _data.settings.vibrationEnabled = value;
            Save();
            OnSettingsChanged?.Invoke();
        }

        /// <summary>
        /// Reset toàn bộ user data về default. Phát events để UI subscribe có thể refresh.
        /// </summary>
        public void ResetUserData()
        {
            _data = UserData.CreateDefault();
            Save();

            // Báo cho UI biết mọi thứ đã đổi
            OnCoinChanged?.Invoke(_data.stats.currentCoin);
            OnEquippedBackgroundChanged?.Invoke(_data.inventory.equippedBackground);
            OnEquippedWizardChanged?.Invoke(_data.inventory.equippedWizard);
            OnSettingsChanged?.Invoke();

            Debug.Log($"DataManager: reset user data tại {_filePath}");
        }

#if UNITY_EDITOR
        public void LoadEditorDataFromUserData()
        {
            if (_data == null)
            {
                _data = UserData.CreateDefault();
            }

            _editorData.currentCoin = _data.stats.currentCoin;

            _editorData.highScores = new List<HighScoreEntry>();
            foreach (KeyValuePair<string, int> entry in _data.stats.highScores)
            {
                _editorData.highScores.Add(new HighScoreEntry
                {
                    modeKey = entry.Key,
                    score = entry.Value
                });
            }

            _editorData.ownedBackgrounds = new List<string>(_data.inventory.ownedBackgrounds);
            _editorData.ownedWizards = new List<string>(_data.inventory.ownedWizards);
            _editorData.ownedSpells = new List<string>(_data.inventory.ownedSpells);
            _editorData.equippedBackground = _data.inventory.equippedBackground;
            _editorData.equippedWizard = _data.inventory.equippedWizard;

            _editorData.bgmEnabled = _data.settings.bgmEnabled;
            _editorData.sfxEnabled = _data.settings.sfxEnabled;
            _editorData.vibrationEnabled = _data.settings.vibrationEnabled;
        }

        public void ApplyEditorDataToUserData(bool triggerEvents = true)
        {
            if (_data == null)
            {
                _data = UserData.CreateDefault();
            }

            _data.stats.currentCoin = Mathf.Max(0, _editorData.currentCoin);
            _data.stats.highScores = new Dictionary<string, int>();

            if (_editorData.highScores != null)
            {
                foreach (HighScoreEntry entry in _editorData.highScores)
                {
                    if (string.IsNullOrEmpty(entry.modeKey))
                    {
                        continue;
                    }

                    _data.stats.highScores[entry.modeKey] = entry.score;
                }
            }

            _data.inventory.ownedBackgrounds = _editorData.ownedBackgrounds != null
                ? new List<string>(_editorData.ownedBackgrounds)
                : new List<string>();
            _data.inventory.ownedWizards = _editorData.ownedWizards != null
                ? new List<string>(_editorData.ownedWizards)
                : new List<string>();
            _data.inventory.ownedSpells = _editorData.ownedSpells != null
                ? new List<string>(_editorData.ownedSpells)
                : new List<string>();
            _data.inventory.equippedBackground = _editorData.equippedBackground;
            _data.inventory.equippedWizard = _editorData.equippedWizard;

            _data.settings.bgmEnabled = _editorData.bgmEnabled;
            _data.settings.sfxEnabled = _editorData.sfxEnabled;
            _data.settings.vibrationEnabled = _editorData.vibrationEnabled;

            Save();

            if (triggerEvents)
            {
                OnCoinChanged?.Invoke(_data.stats.currentCoin);
                OnEquippedBackgroundChanged?.Invoke(_data.inventory.equippedBackground);
                OnEquippedWizardChanged?.Invoke(_data.inventory.equippedWizard);
                OnSettingsChanged?.Invoke();

                foreach (KeyValuePair<string, int> entry in _data.stats.highScores)
                {
                    OnHighScoreChanged?.Invoke(entry.Key, entry.Value);
                }
            }
        }

        public void SaveUserData()
        {
            Save();
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
            UnityEditor.EditorUtility.RevealInFinder(_filePath);
        }
#endif
    }
}
