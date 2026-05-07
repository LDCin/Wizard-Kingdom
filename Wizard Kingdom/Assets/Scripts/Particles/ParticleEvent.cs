using System;
using UnityEngine;

namespace Particles
{
    public static class ParticleEvent
    {
        public static event Action<ParticleType, Vector3> OnParticleRequested;

        public static void RequestParticle(ParticleType particleType, Vector3 position)
        {
            OnParticleRequested?.Invoke(particleType, position);
        }
    }
}