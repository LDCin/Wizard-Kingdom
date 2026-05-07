using UnityEngine;

namespace Balloons
{
    public class Balloon : MonoBehaviour
    {
        [SerializeField] private Symbol _symbol;
        public Symbol Symbol => _symbol;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        public void InitBalloonData(
            Symbol symbol,
            Sprite sprite,
            RuntimeAnimatorController runtimeAnimatorController
        )
        {
            _symbol = symbol;
            _spriteRenderer.sprite = sprite;
            _animator.runtimeAnimatorController = runtimeAnimatorController;
        }

        public void ResetBalloon()
        {
            gameObject.SetActive(true);
        }

        public void Pop()
        {
            gameObject.SetActive(false);
        }
    }
}