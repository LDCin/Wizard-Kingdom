using DG.Tweening;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFadeEffect : UIEffect
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private float closeDuration = 0.35f;
        [SerializeField] private bool canInteractWhileFading = true;

        private float targetAlpha = 1f;

        public override float ClosePanelDuration => closeDuration;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            targetAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        }

        public override void ShowEffect(float delayTime)
        {
            base.ShowEffect(delayTime);

            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.DOKill();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = canInteractWhileFading;
            canvasGroup.blocksRaycasts = canInteractWhileFading;

            canvasGroup.DOFade(targetAlpha, openDuration)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent)
                .OnComplete(FinishShowEffect);
        }

        public override void FinishShowEffect()
        {
            base.FinishShowEffect();

            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public override void HideEffect(float delayTime)
        {
            base.HideEffect(delayTime);

            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.DOKill();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup.DOFade(0f, closeDuration)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent);
        }
    }
}
