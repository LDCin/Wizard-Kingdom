using System;
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

        [NonSerialized] public bool isOwned;
        [NonSerialized] public bool isEquipped;

        #region Analysis And Design Methods

        public virtual ShopItemData GetData() => this;

        public virtual ShopItemData UpdateData(bool owned = false, bool equipped = false)
        {
            isOwned = owned;
            isEquipped = equipped;
            return this;
        }

        public ShopItemData getData() => GetData();
        public ShopItemData updateData(bool owned = false, bool equipped = false)
        {
            return UpdateData(owned, equipped);
        }

        #endregion
    }
}
