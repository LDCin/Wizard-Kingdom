using UnityEngine;

namespace Enemies
{
    public class EnemyExplosionEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _explosionParticle;

        private void OnEnable()
        {
            Enemy.OnEnemyExplode += PlayExplosion;
        }

        private void OnDisable()
        {
            Enemy.OnEnemyExplode -= PlayExplosion;
        }

        private void PlayExplosion(Vector3 position)
        {
            if (_explosionParticle == null)
            {
                Debug.LogError("Explosion Particle is missing.");
                return;
            }

            _explosionParticle.transform.position = position;
            _explosionParticle.Play();
        }
    }
}