using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Wizard Item", menuName = "Shop/Wizard Item")]
    public class WizardItemData : ShopItemData
    {
        public Sprite lockedFullBodySprite;
        public Sprite unlockedFullBodySprite;
        public Sprite unlockedPortraitSprite;
        public Sprite nameSprite;
        public WizardData wizardData;
    }
}
