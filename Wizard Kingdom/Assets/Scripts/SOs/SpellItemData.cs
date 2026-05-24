using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Spell Item", menuName = "Shop/Spell Item")]
    public class SpellItemData : ShopItemData
    {
        public Sprite lockedItemSprite;
        public Sprite unlockedItemSprite;
    }
}
