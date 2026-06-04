using Balloons;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "New Balloon Data", menuName = "Balloons/Balloon Data", order = 1)]
    public class BalloonData : ScriptableObject
    {
        public string id;
        public Symbol symbol;
        public Sprite sprite;
        public RuntimeAnimatorController runtimeAnimatorController;

        #region Analysis And Design Methods

        public BalloonData Get() => this;
        public BalloonData Set() => this;

        public Balloon Init(Balloon balloon)
        {
            if (balloon == null) return null;
            balloon.Init(this);
            return balloon;
        }

        public BalloonData Destroy(Balloon balloon = null)
        {
            balloon?.Destroy();
            return this;
        }

        public BalloonData get() => Get();
        public BalloonData set() => Set();
        public Balloon init(Balloon balloon) => Init(balloon);
        public BalloonData destroy(Balloon balloon = null) => Destroy(balloon);

        #endregion
    }
}
