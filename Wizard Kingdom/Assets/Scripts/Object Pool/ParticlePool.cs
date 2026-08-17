using System.Collections;
using Particles;
using SOs;
using UnityEngine;
using Utils;

namespace ObjectPool
{
    public class ParticlePool : ObjectPool<Particle, ParticleData, ParticleType>
    {
        private void OnEnable()
        {
            Observer.Subscribe<ParticleRequestPayload>(ObserverEvent.ParticleRequested, PlayParticle);
        }

        private void OnDisable()
        {
            Observer.Unsubscribe<ParticleRequestPayload>(ObserverEvent.ParticleRequested, PlayParticle);
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

        private void PlayParticle(ParticleRequestPayload payload)
        {
            if (!IsReady)
            {
                Debug.LogWarning("ParticlePool is not ready yet.");
                return;
            }

            Particle particle = Get(payload.ParticleType);

            if (particle == null)
            {
                return;
            }

            particle.Play(payload.Position);

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
