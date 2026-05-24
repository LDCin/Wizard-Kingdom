using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ShopkeeperAnimator : MonoBehaviour
    {
        private static readonly int SwapTrigger = Animator.StringToHash("Swap");

        [Header("Animator (NPC body)")]
        [SerializeField] private Animator _animator;

        [Header("Hand item — Image hiển thị sprite item user đang browse")]
        [SerializeField] private Image _handImage;

        private Sprite _pendingHandSprite;

        private void Reset()
        {
            _animator = GetComponent<Animator>();
        }
        public void SetHandSpriteImmediate(Sprite sprite)
        {
            _pendingHandSprite = null;
            ApplyHandSprite(sprite);
        }
        public void PlaySwapTo(Sprite sprite)
        {
            _pendingHandSprite = sprite;

            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetTrigger(SwapTrigger);
            }
            else
            {
                ApplyHandSprite(sprite);
                _pendingHandSprite = null;
            }
        }
        public void OnSwapMidpoint()
        {
            ApplyHandSprite(_pendingHandSprite);
            _pendingHandSprite = null;
        }

        private void ApplyHandSprite(Sprite sprite)
        {
            if (_handImage == null) return;
            _handImage.sprite = sprite;
            _handImage.enabled = sprite != null;
        }
    }
}
