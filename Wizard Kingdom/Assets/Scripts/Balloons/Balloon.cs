using UnityEngine;

namespace Balloons
{
    public class Balloon : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private Symbol _symbol;
        public Symbol Symbol => _symbol;

        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;

        [Header("Rope")]
        [SerializeField] private Transform _ropeTransform;
        [SerializeField] private Transform _ropeStartPoint;
        [SerializeField] private float _ropeBaseLength = 1f;
        [SerializeField] private float _ropeAngleOffset = 0f;

        private Transform _ropeTarget;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }
        }

        private void LateUpdate()
        {
            RefreshRope();
        }

        public void InitBalloonData(
            Symbol symbol,
            Sprite sprite,
            RuntimeAnimatorController runtimeAnimatorController)
        {
            _symbol = symbol;

            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = sprite;
            }

            if (_animator != null)
            {
                _animator.runtimeAnimatorController = runtimeAnimatorController;
            }
        }

        public void SetupRope(Transform ropeTarget)
        {
            _ropeTarget = ropeTarget;

            if (_ropeTransform != null)
            {
                _ropeTransform.gameObject.SetActive(_ropeTarget != null);
            }

            RefreshRope();
        }

        public void ClearRope()
        {
            _ropeTarget = null;

            if (_ropeTransform != null)
            {
                _ropeTransform.gameObject.SetActive(false);
            }
        }

        private void RefreshRope()
        {
            if (_ropeTransform == null || _ropeTarget == null)
            {
                return;
            }

            Vector3 startPos = _ropeStartPoint != null ? _ropeStartPoint.position : transform.position;
            Vector3 endPos = _ropeTarget.position;

            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;

            if (distance <= 0.0001f)
            {
                return;
            }

            _ropeTransform.position = startPos;

            Quaternion lookRotation = Quaternion.FromToRotation(Vector3.down, direction.normalized);
            _ropeTransform.rotation = lookRotation * Quaternion.Euler(0f, 0f, _ropeAngleOffset);

            Vector3 localScale = _ropeTransform.localScale;
            localScale.y = distance / _ropeBaseLength;
            _ropeTransform.localScale = localScale;
        }

        public void Pop()
        {
            gameObject.SetActive(false);
        }
    }
}