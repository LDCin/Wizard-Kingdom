using System;
using System.Collections.Generic;
using Utils;

namespace Data
{
    [System.Serializable]
    public class UserData
    {
        public string id;
        public StatsData stats = new();
        public InventoryData inventory = new();
        public SettingsData settings = new();

        #region Analysis And Design Methods

        public UserData Get()
        {
            Set();
            stats.GetData();
            inventory.GetData();
            settings.GetData();
            return this;
        }

        public UserData Set()
        {
            stats ??= new StatsData();
            inventory ??= new InventoryData();
            settings ??= new SettingsData();
            return this;
        }

        public UserData Set(Action<UserData> updateData)
        {
            Set();
            updateData?.Invoke(this);
            return this;
        }

        public UserData get() => Get();
        public UserData set() => Set();
        public UserData set(Action<UserData> updateData) => Set(updateData);

        #endregion

        public static UserData CreateDefault()
        {
            return new UserData
            {
                stats = new StatsData
                {
                    highScores = new Dictionary<string, int>(),
                    currentGold = UserDataDefaults.StartingGold
                },
                inventory = new InventoryData
                {
                    ownedBackgrounds = new List<string> { UserDataDefaults.DefaultBackgroundId },
                    ownedWizards = new List<string> { UserDataDefaults.DefaultWizardId },
                    ownedSpells = new List<string>(),
                    equippedBackground = UserDataDefaults.DefaultBackgroundId,
                    equippedWizard = UserDataDefaults.DefaultWizardId
                },
                settings = new SettingsData
                {
                    bgmEnabled = true,
                    sfxEnabled = true,
                    vibrationEnabled = true
                }
            };
        }
    }

    [System.Serializable]
    public class StatsData
    {
        public string id;
        public Dictionary<string, int> highScores = new();
        public int currentGold;

        #region Analysis And Design Methods

        public StatsData GetData()
        {
            highScores ??= new Dictionary<string, int>();
            return this;
        }

        public bool UpdateData(string modeKey, int score, int gold)
        {
            GetData();

            bool changed = false;
            int newGold = Math.Max(0, currentGold + gold);
            if (newGold != currentGold)
            {
                currentGold = newGold;
                changed = true;
            }

            if (!string.IsNullOrEmpty(modeKey))
            {
                highScores.TryGetValue(modeKey, out int currentHighScore);
                if (score > currentHighScore)
                {
                    highScores[modeKey] = score;
                    changed = true;
                }
            }

            return changed;
        }

        public StatsData getData() => GetData();
        public bool updateData(string modeKey, int score, int gold) => UpdateData(modeKey, score, gold);

        #endregion
    }

    [System.Serializable]
    public class InventoryData
    {
        public string id;
        public List<string> ownedBackgrounds = new();
        public List<string> ownedWizards = new();
        public List<string> ownedSpells = new();

        public string equippedBackground;
        public string equippedWizard;

        #region Analysis And Design Methods

        public InventoryData GetData()
        {
            ownedBackgrounds ??= new List<string>();
            ownedWizards ??= new List<string>();
            ownedSpells ??= new List<string>();
            return this;
        }

        public InventoryData UpdateData()
        {
            return GetData();
        }

        public InventoryData getData() => GetData();
        public InventoryData updateData() => UpdateData();

        #endregion
    }

    [System.Serializable]
    public class SettingsData
    {
        public string id;
        public bool bgmEnabled = true;
        public bool sfxEnabled = true;
        public bool vibrationEnabled = true;

        #region Analysis And Design Methods

        public SettingsData GetData() => this;

        public SettingsData UpdateData(
            bool? bgmEnabled = null,
            bool? sfxEnabled = null,
            bool? vibrationEnabled = null)
        {
            if (bgmEnabled.HasValue) this.bgmEnabled = bgmEnabled.Value;
            if (sfxEnabled.HasValue) this.sfxEnabled = sfxEnabled.Value;
            if (vibrationEnabled.HasValue) this.vibrationEnabled = vibrationEnabled.Value;
            return this;
        }

        public SettingsData getData() => GetData();
        public SettingsData updateData(
            bool? bgmEnabled = null,
            bool? sfxEnabled = null,
            bool? vibrationEnabled = null)
        {
            return UpdateData(bgmEnabled, sfxEnabled, vibrationEnabled);
        }

        #endregion
    }

    public static class UserDataDefaults
    {
        public const string DefaultBackgroundId = GameConfig.UserDataDefaults.DefaultBackgroundId;
        public const string DefaultWizardId = GameConfig.UserDataDefaults.DefaultWizardId;
        public const int StartingGold = 0;
    }
}
