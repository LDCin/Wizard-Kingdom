using Balloons;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "New Balloon Data", menuName = "Balloons/Balloon Data", order = 1)]
    public class BalloonData : ScriptableObject
    {
        public Symbol symbol;
        public Sprite sprite;
        public RuntimeAnimatorController runtimeAnimatorController;
    }
}