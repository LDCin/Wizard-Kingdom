using UnityEngine;

namespace Player
{
    public class PlayerBodyAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private Player _player;

        private void Awake()
        {
            if (_player == null)
            {
                _player = GetComponentInParent<Player>();
            }
        }

        public void OnSnapAnimationFinished()
        {
            _player.OnSnapAnimationFinished();
        }
    }
}