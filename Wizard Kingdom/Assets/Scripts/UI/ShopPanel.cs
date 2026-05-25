using Managers;
using Utils;

namespace UI
{
    public class ShopPanel : Panel
    {
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

        public void OpenSpellCategory() => OpenAndClose(GameConfig.Panel.ShopSpell);
        public void OpenBackgroundCategory() => OpenAndClose(GameConfig.Panel.ShopBackground);
        public void OpenWizardCategory() => OpenAndClose(GameConfig.Panel.ShopWizard);

        public void CloseShop() {
            UIManager.Instance.OpenPanel(GameConfig.Panel.Menu);
            UIManager.Instance.ClosePanel(GameConfig.Panel.Shop);
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
