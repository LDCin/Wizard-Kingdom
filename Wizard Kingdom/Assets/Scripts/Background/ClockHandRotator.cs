using Managers;
using UnityEngine;

namespace BackgroundSystem
{
    public class ClockHandRotator : MonoBehaviour
    {
        [Min(0.01f)]
        [SerializeField] private float _secondsPerRevolution = 60f;

        private float _remainingTime;
        private float _totalTime;

        private void OnEnable()
        {
            GameManager.OnTimeChanged += HandleTimeChanged;

            if (GameManager.Instance != null)
            {
                _totalTime = Mathf.Max(0.01f, GameManager.Instance.TotalTime);
                _remainingTime = Mathf.Clamp(GameManager.Instance.RemainingTime, 0f, _totalTime);
                _secondsPerRevolution = _totalTime;
                ApplyRotation();
            }
        }

        private void OnDisable()
        {
            GameManager.OnTimeChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(float remainingTime, float totalTime)
        {
            _totalTime = Mathf.Max(0.01f, totalTime);
            _remainingTime = Mathf.Clamp(remainingTime, 0f, _totalTime);
            _secondsPerRevolution = _totalTime;
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            float progress = 1f - Mathf.Clamp01(_remainingTime / _secondsPerRevolution);
            float angle = -360f * progress;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}