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
        public static UserData CreateDefault()
        {
            return new UserData
            {
                stats = new StatsData
                {
                    highScores = new Dictionary<string, int>(),
                    currentCoin = UserDataDefaults.StartingCoin
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
        public int currentCoin;
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
    }

    [System.Serializable]
    public class SettingsData
    {
        public string id;
        public bool bgmEnabled = true;
        public bool sfxEnabled = true;
        public bool vibrationEnabled = true;
    }

    public static class UserDataDefaults
    {
        public const string DefaultBackgroundId = GameConfig.UserDataDefaults.DefaultBackgroundId;
        public const string DefaultWizardId = GameConfig.UserDataDefaults.DefaultWizardId;
        public const int StartingCoin = 0;
    }
}
