using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "Background Data", menuName = "Gameplay/Background Data")]
    public class BackgroundData : ScriptableObject
    {
        public string id;
        public GameObject prefab;
    }
}
