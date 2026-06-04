using Managers;
using Utils;

namespace UI
{
    public class ShopPanel : Panel
    {
        #region Analysis And Design Properties

        [UnityEngine.Header("Header")]
        [UnityEngine.SerializeField] private SpriteAssetNumberText _totalGoldText;
        [UnityEngine.SerializeField] private UnityEngine.UI.Button _subSpellShop;
        [UnityEngine.SerializeField] private UnityEngine.UI.Button _subBackgroundShop;
        [UnityEngine.SerializeField] private UnityEngine.UI.Button _subWizardShop;
        [UnityEngine.SerializeField] private UnityEngine.UI.Button _subMenu;

        public SpriteAssetNumberText OutGold => _totalGoldText;
        public UnityEngine.UI.Button SubSpellShop => _subSpellShop;
        public UnityEngine.UI.Button SubBackgroundShop => _subBackgroundShop;
        public UnityEngine.UI.Button SubWizardShop => _subWizardShop;
        public UnityEngine.UI.Button SubMenu => _subMenu;

        #endregion

        private void OnEnable()
        {
            DataManager.OnGoldChanged += OnGoldChanged;
            RefreshGold();
        }

        private void OnDisable()
        {
            DataManager.OnGoldChanged -= OnGoldChanged;
        }

        #region Analysis And Design Methods

        public void OpenSpellCategory() => OpenAndClose(GameConfig.Panel.ShopSpell);
        public void OpenBackgroundCategory() => OpenAndClose(GameConfig.Panel.ShopBackground);
        public void OpenWizardCategory() => OpenAndClose(GameConfig.Panel.ShopWizard);
        public void openSpellCategory() => OpenSpellCategory();
        public void openBackgroundCategory() => OpenBackgroundCategory();
        public void openWizardCategory() => OpenWizardCategory();

        public void CloseShop() {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                GameManager.Instance.BackToMenu();
            }

            UIManager.Instance.OpenPanel(GameConfig.Panel.Menu);
            UIManager.Instance.ClosePanel(GameConfig.Panel.Shop);
        }
        public void closeShop() => CloseShop();

        #endregion

        private void OpenAndClose(string panelName)
        {
            UIManager.Instance.OpenPanel(panelName);
            Close();
        }

        private void OnGoldChanged(int _) => RefreshGold();

        private void RefreshGold()
        {
            if (_totalGoldText == null) return;
            int gold = DataManager.Instance != null ? DataManager.Instance.GetGold() : 0;
            _totalGoldText.SetValue(gold);
        }
    }
}
