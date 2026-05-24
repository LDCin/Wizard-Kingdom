using System.Collections.Generic;
using Managers;
using SOs;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Shop panel cho Background. Items lấy từ ShopCatalog.backgrounds.
    /// Khung preview chỉ có 1 ảnh full-fill. Bar phụ luôn hiển thị nameSprite.
    /// Có equip — BUY / SELECT / ✓.
    /// </summary>
    public class BackgroundShopPanel : ShopItemPanelBase<BackgroundItemData>
    {
        [Header("Background preview")]
        [SerializeField] private Image _itemImage;

        [Header("Bar phụ — name sprite (luôn hiển thị)")]
        [SerializeField] private Image _nameImage;

        protected override IReadOnlyList<BackgroundItemData> GetItemsFrom(ShopCatalog catalog) => catalog.backgrounds;

        protected override bool HasEquip => true;

        protected override void RenderItem(BackgroundItemData item, bool owned)
        {
            if (_itemImage != null)
            {
                _itemImage.sprite = owned ? item.unlockedItemSprite : item.lockedItemSprite;
                _itemImage.enabled = _itemImage.sprite != null;
            }

            if (_nameImage != null)
            {
                _nameImage.sprite = item.nameSprite;
                _nameImage.enabled = item.nameSprite != null;
            }
        }

        protected override void RenderEmpty()
        {
            if (_itemImage != null) _itemImage.enabled = false;
            if (_nameImage != null) _nameImage.enabled = false;
        }

        protected override bool IsOwned(BackgroundItemData item)
            => DataManager.Instance != null && DataManager.Instance.OwnsBackground(item.id);

        protected override void AddToInventory(BackgroundItemData item)
            => DataManager.Instance.AddBackground(item.id);

        protected override bool IsEquipped(BackgroundItemData item)
            => DataManager.Instance != null && DataManager.Instance.GetEquippedBackground() == item.id;

        protected override void Equip(BackgroundItemData item)
            => DataManager.Instance.EquipBackground(item.id);
    }
}
