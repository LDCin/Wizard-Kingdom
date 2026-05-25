using UnityEngine;

namespace SOs
{
    /// <summary>
    /// Gameplay data của một background. Mỗi background là 1 prefab độc lập
    /// để tự do về cấu trúc (số lượng sprite, animation, particle, layout...).
    /// id phải khớp với BackgroundItemData.id và id lưu trong UserData.inventory.
    /// </summary>
    [CreateAssetMenu(fileName = "Background Data", menuName = "Gameplay/Background Data")]
    public class BackgroundData : ScriptableObject
    {
        public string id;

        [Tooltip("Prefab chứa toàn bộ visual của map (sprites, animations, particles...). " +
                 "BackgroundLoader sẽ Instantiate prefab này dưới Background root khi user equip.")]
        public GameObject prefab;
    }
}
