using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UI;

namespace UI
{
    public class IntroPanel : Panel
    {
        [System.Serializable]
        public class FlyInItem
        {
            public RectTransform rect;
            public float delay;

            [HideInInspector] public Vector3 finalLocalPosition;
        }

        [Header("Scene")]
        public string mainMenuSceneName = "Menu";

        [Header("UI")]
        public RectTransform canvasRoot;
        public RectTransform introContainer;
        public CanvasGroup introCanvasGroup;
        public RectTransform gameContentContainer;
        public Image hourglass;
        public CanvasGroup tapToStartGroup;

        [Header("Background")]
        public Image introBackground;
        public Image gameBackground;
        public Color targetBackgroundColor;
        public float backgroundColorChangeTime = 0.8f;

        [Header("Intro Fade")]
        public float introFadeOutTime = 1.0f;

        [Header("Fly In Objects")]
        public FlyInItem[] flyInItems;

        [Header("Fly In Settings")]
        public float flyStartY = -1000f;
        public float flyDuration = 1.5f;
        public Ease flyEase = Ease.OutQuart;

        [Header("Start Effect")]
        public Image wizardImage;
        public Sprite wizardOpenEyesSprite;
        public CanvasGroup whiteFlashGroup;

        public float startZoomScale = 0.85f;
        public float startZoomDuration = 0.6f;
        public float whiteFlashDuration = 0.45f;
        public Ease startZoomEase = Ease.InOutQuart;

        private float screenHeight;
        private bool canTap = false;
        private bool tapped = false;
        private Tween tapBlinkTween;

        void Start()
        {
            screenHeight = canvasRoot.rect.height;

            introContainer.anchoredPosition = Vector2.zero;
            gameContentContainer.anchoredPosition = Vector2.zero;
            gameContentContainer.localScale = Vector3.one;

            introContainer.SetAsLastSibling();

            if (whiteFlashGroup != null)
            {
                whiteFlashGroup.transform.SetAsLastSibling();
                whiteFlashGroup.alpha = 0f;
                whiteFlashGroup.blocksRaycasts = false;
            }

            if (introCanvasGroup != null)
            {
                introCanvasGroup.alpha = 1f;
                introCanvasGroup.blocksRaycasts = true;
            }

            SetImageAlpha(hourglass, 0f);

            if (tapToStartGroup != null)
            {
                tapToStartGroup.alpha = 1f;
            }

            PrepareFlyInObjects();
            PlayIntro();
        }

        void PrepareFlyInObjects()
        {
            foreach (FlyInItem item in flyInItems)
            {
                if (item.rect == null)
                    continue;

                item.finalLocalPosition = item.rect.localPosition;
                item.rect.localPosition = item.finalLocalPosition + new Vector3(0f, flyStartY, 0f);
            }
        }

        void PlayIntro()
        {
            Sequence seq = DOTween.Sequence();

            seq.Append(hourglass.DOFade(1f, 0.8f));
            seq.AppendInterval(1.5f);
            seq.Append(hourglass.DOFade(0f, 0.5f));

            if (introBackground != null)
            {
                seq.Append(introBackground.DOColor(targetBackgroundColor, backgroundColorChangeTime));
            }

            if (gameBackground != null)
            {
                seq.Join(gameBackground.DOColor(targetBackgroundColor, backgroundColorChangeTime));
            }

            float revealStartTime = seq.Duration();

            if (introCanvasGroup != null)
            {
                seq.Insert(
                    revealStartTime,
                    introCanvasGroup.DOFade(0f, introFadeOutTime)
                        .SetEase(Ease.InOutSine)
                );

                seq.InsertCallback(revealStartTime + introFadeOutTime, () =>
                {
                    introCanvasGroup.blocksRaycasts = false;
                    introContainer.gameObject.SetActive(false);
                });
            }
            else
            {
                seq.InsertCallback(revealStartTime, () =>
                {
                    introContainer.gameObject.SetActive(false);
                });
            }

            foreach (FlyInItem item in flyInItems)
            {
                if (item.rect == null)
                    continue;

                seq.Insert(
                    revealStartTime + item.delay,
                    item.rect.DOLocalMove(item.finalLocalPosition, flyDuration)
                        .SetEase(flyEase)
                );
            }

            seq.OnComplete(() =>
            {
                canTap = true;
                PlayTapToStartBlink();
            });
        }

        void PlayTapToStartBlink()
        {
            if (tapToStartGroup == null)
                return;

            tapToStartGroup.alpha = 1f;

            tapBlinkTween = tapToStartGroup
                .DOFade(0.25f, 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        void Update()
        {
            if (!canTap || tapped)
                return;

            if (PlayerTapped())
            {
                PlayStartEffect();
            }
        }

        bool PlayerTapped()
        {
            if (Input.GetMouseButtonDown(0))
                return true;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                return true;

            return false;
        }

        void PlayStartEffect()
        {
            tapped = true;
            canTap = false;

            if (tapBlinkTween != null)
            {
                tapBlinkTween.Kill();
            }

            if (wizardImage != null && wizardOpenEyesSprite != null)
            {
                wizardImage.sprite = wizardOpenEyesSprite;
            }

            if (whiteFlashGroup != null)
            {
                whiteFlashGroup.blocksRaycasts = true;
                whiteFlashGroup.transform.SetAsLastSibling();
                whiteFlashGroup.alpha = 0f;
            }

            Sequence startSeq = DOTween.Sequence();

            if (tapToStartGroup != null)
            {
                startSeq.Insert(
                    0f,
                    tapToStartGroup.DOFade(0f, 0.2f)
                );
            }

            startSeq.Insert(
                0f,
                gameContentContainer.DOScale(startZoomScale, startZoomDuration)
                    .SetEase(startZoomEase)
            );

            if (whiteFlashGroup != null)
            {
                startSeq.Insert(
                    0f,
                    whiteFlashGroup.DOFade(1f, whiteFlashDuration)
                        .SetEase(Ease.InOutSine)
                );
            }

            startSeq.OnComplete(() =>
            {
                SceneManager.LoadScene(mainMenuSceneName);
                UIManager.Instance.OpenPanel("Panel - Menu");
                Close();
            });
        }

        void SetImageAlpha(Image image, float alpha)
        {
            if (image == null)
                return;

            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }
}