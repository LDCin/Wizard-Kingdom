using UnityEngine;
using Utils;

namespace Particles
{
    public static class ParticleEvent
    {
        public static void RequestParticle(ParticleType particleType, Vector3 position)
        {
            Observer.Publish(ObserverEvent.ParticleRequested, new ParticleRequestPayload(particleType, position));
        }
    }
}
