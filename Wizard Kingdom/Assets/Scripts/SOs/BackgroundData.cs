using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Background Data", menuName = "Gameplay/Background Data")]
    public class BackgroundData : ScriptableObject
    {
        public string id;
        public GameObject prefab;

        #region Analysis And Design Methods

        public BackgroundData Get() => this;
        public BackgroundData Set() => this;

        public GameObject Init(Transform parent = null, GameObject currentInstance = null)
        {
            if (currentInstance != null)
            {
                Object.Destroy(currentInstance);
            }

            return prefab != null ? Instantiate(prefab, parent) : null;
        }

        public BackgroundData get() => Get();
        public BackgroundData set() => Set();
        public GameObject init(Transform parent = null, GameObject currentInstance = null)
        {
            return Init(parent, currentInstance);
        }

        #endregion
    }
}
