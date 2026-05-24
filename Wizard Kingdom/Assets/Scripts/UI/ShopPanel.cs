using Managers;

namespace UI
{
    /// <summary>
    /// Panel chọn danh mục shop. Mỗi category có panel riêng được mở qua Addressables.
    /// Hiển thị tổng coin của user, auto refresh khi DataManager phát OnCoinChanged.
    /// </summary>
    public class ShopPanel : Panel
    {
        private const string SpellPanelName = "Panel - Shop Spell";
        private const string BackgroundPanelName = "Panel - Shop Background";
        private const string WizardPanelName = "Panel - Shop Wizard";

        [UnityEngine.Header("Header")]
        [UnityEngine.SerializeField] private SpriteAssetNumberText _totalCoinText;

        private void OnEnable()
        {
            DataManager.OnCoinChanged += OnCoinChanged;
            RefreshCoin();
        }

        private void OnDisable()
        {
            DataManager.OnCoinChanged -= OnCoinChanged;
        }

        public void OpenSpellCategory() => OpenAndClose(SpellPanelName);
        public void OpenBackgroundCategory() => OpenAndClose(BackgroundPanelName);
        public void OpenWizardCategory() => OpenAndClose(WizardPanelName);

        public void CloseShop() {
            UIManager.Instance.OpenPanel("Panel - Menu");
            UIManager.Instance.ClosePanel("Panel - Shop");
        }

        private void OpenAndClose(string panelName)
        {
            UIManager.Instance.OpenPanel(panelName);
            Close();
        }

        private void OnCoinChanged(int _) => RefreshCoin();

        private void RefreshCoin()
        {
            if (_totalCoinText == null) return;
            int coin = DataManager.Instance != null ? DataManager.Instance.GetCoin() : 0;
            _totalCoinText.SetValue(coin);
        }
    }
}
