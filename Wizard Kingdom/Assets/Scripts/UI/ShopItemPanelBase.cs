using System.Collections;
using System.Collections.Generic;
using Managers;
using SOs;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public abstract class ShopItemPanelBase<TItem> : Panel where TItem : ShopItemData
    {
        protected const string CatalogAddress = GameConfig.Addressables.ShopCatalog;

        [SerializeField] private SpriteAssetNumberText _totalCoinText;
        [SerializeField] private GameObject _priceIcon;
        [SerializeField] private SpriteAssetNumberText _priceText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _equippedButton;
        [SerializeField] private Button _unlockedButton;
        [SerializeField] private ShopkeeperAnimator _shopkeeper;

        protected IReadOnlyList<TItem> _items;
        protected int _index;
        private AsyncOperationHandle<ShopCatalog> _catalogHandle;
        private bool _hasHandle;
        private bool _isNavigating;

        protected abstract IReadOnlyList<TItem> GetItemsFrom(ShopCatalog catalog);

        protected abstract bool HasEquip { get; }
        protected abstract void RenderItem(TItem item, bool owned);
        protected abstract void RenderEmpty();
        protected abstract bool IsOwned(TItem item);
        protected abstract void AddToInventory(TItem item);
        protected abstract bool IsEquipped(TItem item);
        protected abstract void Equip(TItem item);

        private void OnEnable()
        {
            DataManager.OnCoinChanged += OnCoinChanged;
            DataManager.OnBackgroundPurchased += OnInventoryChanged;
            DataManager.OnWizardPurchased += OnInventoryChanged;
            DataManager.OnSpellPurchased += OnInventoryChanged;
            DataManager.OnEquippedBackgroundChanged += OnInventoryChanged;
            DataManager.OnEquippedWizardChanged += OnInventoryChanged;

            RefreshCoin();
            StartCoroutine(LoadCatalogRoutine());
        }

        private void OnDisable()
        {
            DataManager.OnCoinChanged -= OnCoinChanged;
            DataManager.OnBackgroundPurchased -= OnInventoryChanged;
            DataManager.OnWizardPurchased -= OnInventoryChanged;
            DataManager.OnSpellPurchased -= OnInventoryChanged;
            DataManager.OnEquippedBackgroundChanged -= OnInventoryChanged;
            DataManager.OnEquippedWizardChanged -= OnInventoryChanged;

            ReleaseCatalog();
        }

        private IEnumerator LoadCatalogRoutine()
        {
            ReleaseCatalog();

            yield return ShopCatalogLoader.Load(CatalogAddress, (catalog, handle) =>
            {
                _catalogHandle = handle;
                _hasHandle = true;
                _items = catalog != null ? GetItemsFrom(catalog) : null;
                _index = 0;
                RefreshItem();
            });
        }

        private void ReleaseCatalog()
        {
            if (_hasHandle)
            {
                ShopCatalogLoader.Release(_catalogHandle);
                _hasHandle = false;
            }
            _items = null;
            _index = 0;
        }

        public void Prev()
        {
            if (_items == null || _items.Count == 0) return;
            _index = (_index - 1 + _items.Count) % _items.Count;
            _isNavigating = true;
            RefreshItem();
            _isNavigating = false;
        }

        public void Next()
        {
            if (_items == null || _items.Count == 0) return;
            _index = (_index + 1) % _items.Count;
            _isNavigating = true;
            RefreshItem();
            _isNavigating = false;
        }

        public void Buy()
        {
            if (!TryGetCurrentItem(out TItem item)) return;
            if (IsOwned(item)) return;

            if (item.price > 0 && !DataManager.Instance.TrySpendCoin(item.price)) return;

            AddToInventory(item);

            if (HasEquip) Equip(item);
        }

        public void EquipCurrent()
        {
            if (!TryGetCurrentItem(out TItem item)) return;
            if (!HasEquip) return;
            if (!IsOwned(item)) return;
            Equip(item);
        }

        public void BackToShop()
        {
            UIManager.Instance.OpenPanel(GameConfig.Panel.Shop);
            Close();
        }

        protected bool TryGetCurrentItem(out TItem item)
        {
            item = null;
            if (_items == null || _items.Count == 0) return false;
            if (_index < 0 || _index >= _items.Count) return false;
            item = _items[_index];
            return item != null;
        }

        private void OnCoinChanged(int _) => RefreshCoin();
        private void OnInventoryChanged(string _) => RefreshItem();

        private void RefreshCoin()
        {
            if (_totalCoinText == null) return;
            int coin = DataManager.Instance != null ? DataManager.Instance.GetCoin() : 0;
            _totalCoinText.SetValue(coin);
        }

        private void RefreshItem()
        {
            if (!TryGetCurrentItem(out TItem item))
            {
                RenderEmpty();
                HideNpcHand();
                HideAllActionButtons();
                SetPriceVisible(false);
                return;
            }

            bool owned = IsOwned(item);

            RenderItem(item, owned);
            RenderNpcHand(item);

            bool showPrice = item.price > 0;
            SetPriceVisible(showPrice);
            if (showPrice && _priceText != null) _priceText.SetValue(item.price);

            UpdateActionButton(item, owned);
        }

        private void SetPriceVisible(bool visible)
        {
            // Yellow bar luôn hiển thị (là một phần của Preview Frame sprite).
            // Chỉ ẩn nội dung bên trong: gold icon + price text.
            if (_priceIcon != null) _priceIcon.SetActive(visible);
            if (_priceText != null) _priceText.gameObject.SetActive(visible);
        }

        private void UpdateActionButton(TItem item, bool owned)
        {
            HideAllActionButtons();

            if (!owned)
            {
                SetActive(_buyButton, true);
                return;
            }

            if (!HasEquip)
            {
                SetActive(_unlockedButton, true);
                return;
            }

            if (IsEquipped(item))
                SetActive(_equippedButton, true);
            else
                SetActive(_equipButton, true);
        }

        private void RenderNpcHand(TItem item)
        {
            if (_shopkeeper == null) return;

            Sprite s = item != null ? item.handSprite : null;

            if (_isNavigating)
            {
                _shopkeeper.PlaySwapTo(s);
            }
            else
            {
                _shopkeeper.SetHandSpriteImmediate(s);
            }
        }

        private void HideNpcHand()
        {
            if (_shopkeeper == null) return;
            _shopkeeper.SetHandSpriteImmediate(null);
        }

        private void HideAllActionButtons()
        {
            SetActive(_buyButton, false);
            SetActive(_equipButton, false);
            SetActive(_equippedButton, false);
            SetActive(_unlockedButton, false);
        }

        private static void SetActive(Button btn, bool active)
        {
            if (btn != null) btn.gameObject.SetActive(active);
        }
    }
}
