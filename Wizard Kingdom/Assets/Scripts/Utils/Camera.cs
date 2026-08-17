using DG.Tweening;
using UnityEngine;

namespace Utils
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private float _shakeDuration = 0.1f;
        [SerializeField] private float _shakeStrength = 0.08f;

        private void OnEnable()
        {
            Observer.Subscribe<RewardPayload>(ObserverEvent.EnemyDied, Shake);
        }

        private void OnDisable()
        {
            Observer.Unsubscribe<RewardPayload>(ObserverEvent.EnemyDied, Shake);
        }

        public void Shake(RewardPayload reward)
        {
            transform.DOShakePosition(_shakeDuration, _shakeStrength);
        }
    }
}
