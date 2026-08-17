using System.Collections.Generic;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Shop Catalog", menuName = "Shop/Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        public List<SpellItemData> spells = new();
        public List<BackgroundItemData> backgrounds = new();
        public List<WizardItemData> wizards = new();
    }
}
