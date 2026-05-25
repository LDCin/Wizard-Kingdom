using UnityEngine;

namespace BackgroundSystem
{
    public class CloudScroller : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] _clouds;
        [SerializeField] private float _speed = 1f;
        [Tooltip("Camera dùng để check ra khỏi màn hình. Để trống = Camera.main.")]
        [SerializeField] private Camera _camera;

        private float _halfSpriteWidth;

        private void Start()
        {
            if (_camera == null) _camera = Camera.main;

            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] != null && _clouds[i].sprite != null)
                {
                    _halfSpriteWidth = _clouds[i].bounds.size.x * 0.5f;
                    break;
                }
            }
            if (_halfSpriteWidth <= 0f) _halfSpriteWidth = 1f;
        }

        private void Update()
        {
            if (_clouds == null || _clouds.Length == 0 || _camera == null) return;

            float delta = _speed * Time.deltaTime;
            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] == null) continue;
                _clouds[i].transform.localPosition += new Vector3(delta, 0f, 0f);
            }

            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] == null) continue;
                WrapIfOutsideCamera(_clouds[i].transform);
            }
        }

        private void WrapIfOutsideCamera(Transform self)
        {
            float worldX = self.position.x;
            float halfCameraWidth = _camera.orthographicSize * _camera.aspect;
            float cameraCenterX = _camera.transform.position.x;
            float cameraRight = cameraCenterX + halfCameraWidth;
            float cameraLeft = cameraCenterX - halfCameraWidth;

            if (_speed > 0f && worldX - _halfSpriteWidth > cameraRight)
            {
                Transform leftmost = FindLeftmost();
                if (leftmost != null)
                {
                    float newX = leftmost.localPosition.x - _halfSpriteWidth * 2f;
                    self.localPosition = new Vector3(newX, self.localPosition.y, self.localPosition.z);
                }
            }
            else if (_speed < 0f && worldX + _halfSpriteWidth < cameraLeft)
            {
                Transform rightmost = FindRightmost();
                if (rightmost != null)
                {
                    float newX = rightmost.localPosition.x + _halfSpriteWidth * 2f;
                    self.localPosition = new Vector3(newX, self.localPosition.y, self.localPosition.z);
                }
            }
        }

        private Transform FindLeftmost()
        {
            Transform result = null;
            float minX = float.MaxValue;
            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] == null) continue;
                if (_clouds[i].transform.localPosition.x < minX)
                {
                    minX = _clouds[i].transform.localPosition.x;
                    result = _clouds[i].transform;
                }
            }
            return result;
        }

        private Transform FindRightmost()
        {
            Transform result = null;
            float maxX = float.MinValue;
            for (int i = 0; i < _clouds.Length; i++)
            {
                if (_clouds[i] == null) continue;
                if (_clouds[i].transform.localPosition.x > maxX)
                {
                    maxX = _clouds[i].transform.localPosition.x;
                    result = _clouds[i].transform;
                }
            }
            return result;
        }
    }
}
