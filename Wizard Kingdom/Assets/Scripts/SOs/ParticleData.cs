using Particles;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "ParticleData", menuName = "SOs/Particle Data")]
    public class ParticleData : ScriptableObject
    {
        public ParticleType particleType;
        public ParticleSystem particlePrefab;
    }
}