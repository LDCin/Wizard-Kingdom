using UnityEngine;

namespace Particles
{
    public class Particle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        private ParticleType _particleType;

        public ParticleType ParticleType => _particleType;

        public void Init(ParticleType particleType, ParticleSystem particlePrefab)
        {
            _particleType = particleType;

            if (_particleSystem != null)
            {
                Destroy(_particleSystem.gameObject);
                _particleSystem = null;
            }

            _particleSystem = Instantiate(particlePrefab, transform);
            _particleSystem.transform.localPosition = Vector3.zero;
            _particleSystem.transform.localRotation = Quaternion.identity;
            _particleSystem.transform.localScale = Vector3.one;

            ParticleSystem.MainModule main = _particleSystem.main;
            main.playOnAwake = false;
        }

        public void Play(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);

            if (_particleSystem == null)
            {
                return;
            }

            _particleSystem.Clear(true);
            _particleSystem.Play(true);
        }

        public bool IsAlive()
        {
            return _particleSystem != null && _particleSystem.IsAlive(true);
        }

        public void StopAndClear()
        {
            if (_particleSystem == null)
            {
                return;
            }

            _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particleSystem.Clear(true);
        }
    }
}