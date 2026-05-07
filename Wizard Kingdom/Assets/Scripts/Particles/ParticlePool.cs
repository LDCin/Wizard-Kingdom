using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Particles
{
    public class ParticlePool : MonoBehaviour
    {
        [System.Serializable]
        private class ParticlePoolData
        {
            public ParticleType particleType;
            public ParticleSystem particlePrefab;
            public int initialAmount = 10;
        }

        [SerializeField] private List<ParticlePoolData> _particlePoolDataList = new();

        private readonly Dictionary<ParticleType, Queue<ParticleSystem>> _particleDict = new();
        private readonly Dictionary<ParticleType, ParticleSystem> _particlePrefabDict = new();

        private void Awake()
        {
            InitPool();
        }

        private void OnEnable()
        {
            ParticleEvent.OnParticleRequested += PlayParticle;
        }

        private void OnDisable()
        {
            ParticleEvent.OnParticleRequested -= PlayParticle;
        }

        private void InitPool()
        {
            foreach (ParticlePoolData poolData in _particlePoolDataList)
            {
                if (_particleDict.ContainsKey(poolData.particleType))
                {
                    Debug.LogWarning("Duplicate particle type: " + poolData.particleType);
                    continue;
                }

                _particleDict.Add(poolData.particleType, new Queue<ParticleSystem>());
                _particlePrefabDict.Add(poolData.particleType, poolData.particlePrefab);

                for (int i = 0; i < poolData.initialAmount; i++)
                {
                    ParticleSystem particle = CreateParticle(poolData.particleType);
                    _particleDict[poolData.particleType].Enqueue(particle);
                }
            }
        }

        private ParticleSystem CreateParticle(ParticleType particleType)
        {
            ParticleSystem prefab = _particlePrefabDict[particleType];

            ParticleSystem particle = Instantiate(prefab, transform);
            particle.gameObject.SetActive(false);

            return particle;
        }

        private void PlayParticle(ParticleType particleType, Vector3 position)
        {
            if (!_particleDict.ContainsKey(particleType))
            {
                Debug.LogError("ParticlePool does not contain particle type: " + particleType);
                return;
            }

            ParticleSystem particle = GetParticle(particleType);

            particle.transform.position = position;
            particle.gameObject.SetActive(true);

            particle.Clear(true);
            particle.Play(true);

            StartCoroutine(ReturnParticleAfterFinished(particleType, particle));
        }

        private ParticleSystem GetParticle(ParticleType particleType)
        {
            Queue<ParticleSystem> particleQueue = _particleDict[particleType];

            if (particleQueue.Count > 0)
            {
                return particleQueue.Dequeue();
            }

            return CreateParticle(particleType);
        }

        private IEnumerator ReturnParticleAfterFinished(ParticleType particleType, ParticleSystem particle)
        {
            yield return null;

            while (particle != null && particle.IsAlive(true))
            {
                yield return null;
            }

            ReturnParticleToPool(particleType, particle);
        }

        private void ReturnParticleToPool(ParticleType particleType, ParticleSystem particle)
        {
            if (particle == null)
            {
                return;
            }

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.gameObject.SetActive(false);

            _particleDict[particleType].Enqueue(particle);
        }
    }
}