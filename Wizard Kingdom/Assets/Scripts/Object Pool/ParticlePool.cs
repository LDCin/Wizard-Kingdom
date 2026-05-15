using System.Collections;
using Particles;
using SOs;
using UnityEngine;

namespace ObjectPool
{
    public class ParticlePool : ObjectPool<Particle, ParticleData, ParticleType>
    {
        private void OnEnable()
        {
            ParticleEvent.OnParticleRequested += PlayParticle;
        }

        private void OnDisable()
        {
            ParticleEvent.OnParticleRequested -= PlayParticle;
        }

        private IEnumerator Start()
        {
            yield return InitializeAsync();
        }

        protected override ParticleType GetKeyFromData(ParticleData data)
        {
            return data.particleType;
        }

        protected override ParticleType GetKeyFromItem(Particle item)
        {
            return item.ParticleType;
        }

        protected override void ApplyDataToItem(Particle item, ParticleData data)
        {
            item.Init(data.particleType, data.particlePrefab);
        }

        private void PlayParticle(ParticleType particleType, Vector3 position)
        {
            if (!IsReady)
            {
                Debug.LogWarning("ParticlePool is not ready yet.");
                return;
            }

            Particle particle = Get(particleType);

            if (particle == null)
            {
                return;
            }

            particle.Play(position);

            StartCoroutine(ReturnParticleAfterFinished(particle));
        }

        private IEnumerator ReturnParticleAfterFinished(Particle particle)
        {
            yield return null;

            while (particle != null && particle.IsAlive())
            {
                yield return null;
            }

            Return(particle);
        }

        protected override void OnReturn(Particle particle)
        {
            particle.StopAndClear();
            base.OnReturn(particle);
        }
    }
}