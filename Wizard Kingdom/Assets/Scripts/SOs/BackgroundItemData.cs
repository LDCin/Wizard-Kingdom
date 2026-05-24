using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Background Item", menuName = "Shop/Background Item")]
    public class BackgroundItemData : ShopItemData
    {
        public Sprite lockedItemSprite;
        public Sprite unlockedItemSprite;
        public Sprite nameSprite;
    }
}
