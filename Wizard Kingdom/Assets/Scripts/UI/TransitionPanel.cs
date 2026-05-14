using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace UI
{
    public class TransitionPanel : Panel
    {
        [SerializeField] private Image backgroundFillImage;
        [SerializeField] private float duration = 0.8f;

        public void ShowTransition(Action onComplete)
        {
            gameObject.SetActive(true);
            backgroundFillImage.fillAmount = 0f;
            backgroundFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;

            backgroundFillImage.DOFillAmount(1f, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }

        public void HideTransition(Action onComplete)
        {
            gameObject.SetActive(true);
            backgroundFillImage.fillAmount = 1f;
            backgroundFillImage.fillOrigin = (int)Image.OriginVertical.Bottom;

            backgroundFillImage.DOFillAmount(0f, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }
}