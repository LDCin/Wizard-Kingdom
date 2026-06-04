using System.Collections.Generic;
using Managers;
using SOs;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WizardShopPanel : ShopItemPanelBase<WizardItemData>
    {
        [Header("Full body (nửa trái)")]
        [SerializeField] private Image _fullBodyImage;

        [Header("Portrait (nửa phải)")]
        [SerializeField] private Image _portraitImage;
        [SerializeField] private Sprite _lockedPortraitSprite;

        [Header("Bar phụ — name sprite (chỉ hiển thị khi locked)")]
        [SerializeField] private Image _nameImage;

        #region Analysis And Design Properties

        public Image OutWizardImage => _fullBodyImage;
        public Image OutWizardName => _nameImage;

        #endregion

        protected override IReadOnlyList<WizardItemData> GetItemsFrom(ShopCatalog catalog) => catalog.wizards;
        protected override ShopCategory Category => ShopCategory.Wizard;

        protected override bool HasEquip => true;

        protected override void RenderItem(WizardItemData item, bool owned)
        {
            if (_fullBodyImage != null)
            {
                _fullBodyImage.sprite = owned ? item.unlockedFullBodySprite : item.lockedFullBodySprite;
                _fullBodyImage.enabled = _fullBodyImage.sprite != null;
            }

            if (_portraitImage != null)
            {
                _portraitImage.sprite = owned ? item.unlockedPortraitSprite : _lockedPortraitSprite;
                _portraitImage.enabled = _portraitImage.sprite != null;
            }

            if (_nameImage != null)
            {
                _nameImage.gameObject.SetActive(true);
                _nameImage.sprite = item.nameSprite;
                _nameImage.enabled = item.nameSprite != null;
                if (item.nameSprite != null) _nameImage.SetNativeSize();
            }
        }

        protected override void RenderEmpty()
        {
            if (_fullBodyImage != null) _fullBodyImage.enabled = false;
            if (_portraitImage != null) _portraitImage.enabled = false;
            if (_nameImage != null) _nameImage.enabled = false;
        }

        protected override bool IsOwned(WizardItemData item)
            => DataManager.Instance != null && DataManager.Instance.OwnsWizard(item.id);

        protected override void AddToInventory(WizardItemData item)
            => DataManager.Instance.AddWizard(item.id);

        protected override bool IsEquipped(WizardItemData item)
            => DataManager.Instance != null && DataManager.Instance.GetEquippedWizard() == item.id;

        protected override void Equip(WizardItemData item)
            => DataManager.Instance.EquipWizard(item.id);
    }
}
