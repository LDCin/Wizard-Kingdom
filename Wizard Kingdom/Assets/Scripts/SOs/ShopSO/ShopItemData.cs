using UnityEngine;

namespace SOs
{
    public abstract class ShopItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        public ShopCategory category;
        [Min(0)] public int price;
        public Sprite handSprite;
    }
}
