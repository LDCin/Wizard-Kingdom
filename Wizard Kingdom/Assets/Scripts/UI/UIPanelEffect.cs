using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class UIPanelEffect : UIEffect
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private bool canInteractWhileFading = true;
        [SerializeField] private Vector2 openOffset = new(0f, -3000f);

        public override float ClosePanelDuration => duration;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (panel == null)
            {
                panel = transform as RectTransform;
            }
        }

        public override void ShowEffect(float delayTime)
        {
            base.ShowEffect(delayTime);

            if (panel == null)
            {
                return;
            }

            panel.DOKill();

            Vector2 endPosition = panel.anchoredPosition;
            panel.anchoredPosition = endPosition + openOffset;
            panel.DOAnchorPos(endPosition, duration)
                .SetEase(Ease.OutCubic)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent)
                .OnComplete(FinishShowEffect);

            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.DOKill();
            canvasGroup.interactable = canInteractWhileFading;
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, duration)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent);
        }

        public override void FinishShowEffect()
        {
            base.FinishShowEffect();

            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
            }
        }

        public override void HideEffect(float delayTime)
        {
            base.HideEffect(delayTime);

            if (panel != null)
            {
                panel.DOKill();
                panel.DOAnchorPos(panel.anchoredPosition + openOffset, duration)
                    .SetEase(Ease.OutCubic)
                    .SetDelay(delayTime)
                    .SetUpdate(deltaTimeIndependent);
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(0f, duration)
                    .SetDelay(delayTime)
                    .SetUpdate(deltaTimeIndependent);
            }
        }
    }
}
