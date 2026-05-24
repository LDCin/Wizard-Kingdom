using UnityEngine;

namespace SOs
{
    /// <summary>
    /// Base data cho mọi item shop. Mỗi category có subclass riêng vì
    /// layout/sprite khác nhau (xem SpellItemData, BackgroundItemData, WizardItemData).
    ///
    /// Quy tắc giá:
    /// - price = 0 → item miễn phí (default / random) → UI ẩn cụm giá.
    /// - price > 0 → UI luôn hiển thị giá kể cả sau khi user đã sở hữu.
    ///
    /// Thứ tự hiển thị: do thứ tự trong list của ShopCatalog quyết định.
    /// Designer drag reorder trong catalog Inspector.
    /// </summary>
    public abstract class ShopItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        public ShopCategory category;
        [Min(0)] public int price;

        [Tooltip("Sprite nhỏ hiển thị trên tay NPC chủ shop khi item này đang được browse.")]
        public Sprite handSprite;
    }
}
