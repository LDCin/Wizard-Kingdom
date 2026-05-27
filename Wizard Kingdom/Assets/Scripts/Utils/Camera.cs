using System;
using DG.Tweening;
using Enemies;
using UnityEngine;

namespace Utils
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private float _shakeDuration = 0.1f;
        [SerializeField] private float _shakeStrength = 0.08f;

        private void OnEnable()
        {
            Enemy.OnEnemyDie += Shake;
        }

        private void OnDisable()
        {
            Enemy.OnEnemyDie -= Shake;
        }

        public void Shake(int score, int gold)
        {
            transform.DOShakePosition(_shakeDuration, _shakeStrength);
        }
    }
}