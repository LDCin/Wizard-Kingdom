using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UISlideEffect : UIEffect
    {
        [SerializeField] private RectTransform panel;
        [SerializeField] private Image panelBackground;
        [SerializeField] private Vector2 openOffset = new(0f, -3000f);
        [SerializeField] private float openDuration = 0.35f;
        [SerializeField] private float closeDuration = 0.35f;
        [SerializeField] private float backgroundFadeDuration = 0.5f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private Vector2 originalAnchoredPosition;
        private float targetBackgroundAlpha = -1f;
        private bool originalPositionSet;

        public override float ClosePanelDuration => closeDuration;

        private void Awake()
        {
            if (panel == null)
            {
                panel = transform as RectTransform;
            }

            CacheOriginalValues();
        }

        public override void ShowEffect(float delayTime)
        {
            base.ShowEffect(delayTime);

            if (panel == null)
            {
                return;
            }

            CacheOriginalValues();
            panel.DOKill();

            panel.anchoredPosition = originalAnchoredPosition + openOffset;
            panel.DOAnchorPos(originalAnchoredPosition, openDuration)
                .SetEase(ease)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent)
                .OnComplete(FinishShowEffect);

            if (panelBackground == null)
            {
                return;
            }

            panelBackground.DOKill();
            Color color = panelBackground.color;
            color.a = 0f;
            panelBackground.color = color;
            panelBackground.DOFade(targetBackgroundAlpha, backgroundFadeDuration)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent);
        }

        public override void HideEffect(float delayTime)
        {
            base.HideEffect(delayTime);

            if (panel == null)
            {
                return;
            }

            CacheOriginalValues();
            panel.DOKill();

            panel.DOAnchorPos(originalAnchoredPosition + openOffset, closeDuration)
                .SetEase(ease)
                .SetDelay(delayTime)
                .SetUpdate(deltaTimeIndependent)
                .OnComplete(() => panel.anchoredPosition = originalAnchoredPosition);
        }

        private void CacheOriginalValues()
        {
            if (panel != null && !originalPositionSet)
            {
                originalAnchoredPosition = panel.anchoredPosition;
                originalPositionSet = true;
            }

            if (panelBackground != null && targetBackgroundAlpha < 0f)
            {
                targetBackgroundAlpha = panelBackground.color.a;
            }
        }
    }
}
