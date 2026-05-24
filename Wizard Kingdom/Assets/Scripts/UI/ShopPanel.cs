namespace UI
{
    /// <summary>
    /// Panel chọn danh mục shop. Mỗi category có panel riêng được mở qua Addressables.
    /// </summary>
    public class ShopPanel : Panel
    {
        private const string SpellPanelName = "Panel - Shop Spell";
        private const string BackgroundPanelName = "Panel - Shop Background";
        private const string WizardPanelName = "Panel - Shop Wizard";

        public void OpenSpellCategory() => OpenAndClose(SpellPanelName);
        public void OpenBackgroundCategory() => OpenAndClose(BackgroundPanelName);
        public void OpenWizardCategory() => OpenAndClose(WizardPanelName);

        public void CloseShop() => Close();

        private void OpenAndClose(string panelName)
        {
            UIManager.Instance.OpenPanel(panelName);
            Close();
        }
    }
}
