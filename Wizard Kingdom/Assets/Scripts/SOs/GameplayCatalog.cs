using System.Collections.Generic;
using UnityEngine;

namespace SOs
{
    /// <summary>
    /// Catalog gom toàn bộ wizard/background data dùng trong gameplay.
    /// Tách khỏi ShopCatalog vì 2 mục đích khác nhau:
    /// - ShopCatalog: data hiển thị shop (sprite preview, price...)
    /// - GameplayCatalog: data load lên scene (animator override, sprite render...)
    /// Cả 2 link với nhau qua field id string.
    /// </summary>
    [CreateAssetMenu(fileName = "Gameplay Catalog", menuName = "Gameplay/Gameplay Catalog")]
    public class GameplayCatalog : ScriptableObject
    {
        public List<WizardData> wizards = new();
        public List<BackgroundData> backgrounds = new();

        public WizardData FindWizard(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var w in wizards)
            {
                if (w != null && w.id == id) return w;
            }
            return null;
        }

        public BackgroundData FindBackground(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var b in backgrounds)
            {
                if (b != null && b.id == id) return b;
            }
            return null;
        }
    }
}
