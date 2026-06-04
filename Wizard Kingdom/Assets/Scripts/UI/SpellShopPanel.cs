using System.Collections.Generic;
using Managers;
using SOs;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpellShopPanel : ShopItemPanelBase<SpellItemData>
    {
        [Header("Spell preview layers")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _itemImage;

        [Header("Background sprites (shared cho mọi spell)")]
        [SerializeField] private Sprite _lockedBackgroundSprite;
        [SerializeField] private Sprite _unlockedBackgroundSprite;

        #region Analysis And Design Properties

        public Image OutSpellImage => _itemImage;

        #endregion

        protected override IReadOnlyList<SpellItemData> GetItemsFrom(ShopCatalog catalog) => catalog.spells;
        protected override ShopCategory Category => ShopCategory.Spell;

        protected override bool HasEquip => false;

        protected override void RenderItem(SpellItemData item, bool owned)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.sprite = owned ? _unlockedBackgroundSprite : _lockedBackgroundSprite;
                _backgroundImage.enabled = _backgroundImage.sprite != null;
            }

            if (_itemImage != null)
            {
                _itemImage.sprite = owned ? item.unlockedItemSprite : item.lockedItemSprite;
                _itemImage.enabled = _itemImage.sprite != null;
            }
        }

        protected override void RenderEmpty()
        {
            if (_backgroundImage != null) _backgroundImage.enabled = false;
            if (_itemImage != null) _itemImage.enabled = false;
        }

        protected override bool IsOwned(SpellItemData item)
            => DataManager.Instance != null && DataManager.Instance.OwnsSpell(item.id);

        protected override void AddToInventory(SpellItemData item)
            => DataManager.Instance.AddSpell(item.id);

        protected override bool IsEquipped(SpellItemData item) => false;

        protected override void Equip(SpellItemData item) { /* spell không có equip */ }
    }
}
