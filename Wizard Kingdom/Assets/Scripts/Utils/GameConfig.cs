namespace Utils
{
    public static class GameConfig
    {
        public static class Scene
        {
            public const string Menu = "Menu";
            public const string Game = "Game";
            public const string GameOver = "Game Over";
        }

        public static class Panel
        {
            public const string Transition = Addressables.PanelTransition;
            public const string Menu = Addressables.PanelMenu;
            public const string Game = Addressables.PanelGame;
            public const string DrawAreaFree = Addressables.PanelDrawAreaFree;
            public const string Pause = Addressables.PanelPause;
            public const string GameOver = Addressables.PanelGameOver;
            public const string Shop = Addressables.PanelShop;
            public const string Setting = Addressables.PanelSetting;
            public const string ShopSpell = Addressables.PanelShopSpell;
            public const string ShopBackground = Addressables.PanelShopBackground;
            public const string ShopWizard = Addressables.PanelShopWizard;
        }

        public static class Tags
        {
            public const string Ground = "Ground";
            public const string Fire = "Fire";
        }

        public static class AnimatorParams
        {
            public const string Idle = "Idle";
            public const string Spell = "Spell";
            public const string Snap = "Snap";
            public const string Dead = "Dead";
            public const string Fall = "Fall";
            public const string Victory = "Victory";
        }

        public static class GameMode
        {
            public const string Arcade = Addressables.GameModeArcade;
            public const string TimeAttack = Addressables.GameModeTimeAttack;
        }

        public static class Links
        {
            public const string Facebook = "facebook.com";
        }

        public static class Addressables
        {
            public const string GameplayCatalog = "Gameplay Catalog";
            public const string ShopCatalog = "Shop Catalog";
            public const string GameModeArcade = "ArcadeMode";
            public const string GameModeTimeAttack = "TimeAttackMode";
            public const string PanelTransition = "Panel - Transition";
            public const string PanelMenu = "Panel - Menu";
            public const string PanelGame = "Panel - Game";
            public const string PanelDrawAreaFree = "Panel - Draw Area Free";
            public const string PanelPause = "Panel - Pause";
            public const string PanelGameOver = "Panel - Game Over";
            public const string PanelShop = "Panel - Shop";
            public const string PanelSetting = "Panel - Setting";
            public const string PanelShopSpell = "Panel - Shop Spell";
            public const string PanelShopBackground = "Panel - Shop Background";
            public const string PanelShopWizard = "Panel - Shop Wizard";
        }

        public static class UserDataDefaults
        {
            public const string DefaultBackgroundId = "background_classic";
            public const string DefaultWizardId = "wizard_default";
        }
    }
}