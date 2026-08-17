using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UI;
using Utils;

namespace UI
{
    public class IntroPanel : Panel
    {
        [Header("UI")]
        [SerializeField] private RectTransform introContainer;
        [SerializeField] private RectTransform gameContentContainer;
        [SerializeField] private Image hourglass;
        [SerializeField] private CanvasGroup tapToStartGroup;

        [Header("Background")]
        [SerializeField] private Image introBackground;
        [SerializeField] private Image gameBackground;
        [SerializeField] private Color targetBackgroundColor;
        [SerializeField] private float backgroundColorChangeTime = 0.8f;

        [Header("Start Effect")]
        [SerializeField] private Image wizardImage;
        [SerializeField] private Sprite wizardOpenEyesSprite;
        [SerializeField] private CanvasGroup whiteFlashGroup;
        [SerializeField] private GameObject[] flyingItems;

        [SerializeField] private float startZoomScale = 0.85f;
        [SerializeField] private float startZoomDuration = 0.6f;
        [SerializeField] private float whiteFlashDuration = 0.45f;
        [SerializeField] private Ease startZoomEase = Ease.InOutQuart;

        private bool canTap = false;
        private bool tapped = false;
        private Tween tapBlinkTween;

        void Start()
        {
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

            SetImageAlpha(hourglass, 0f);

            SetCanvasGroupAlpha(tapToStartGroup, 0f);
            SetFlyingItemsActive(false);
            PlayIntro();
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

            if (introContainer != null)
            {
                seq.InsertCallback(revealStartTime, () =>
                {
                    introContainer.gameObject.SetActive(false);
                    SetFlyingItemsActive(true);
                });
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
                SceneManager.LoadScene(GameConfig.Scene.Menu);
                UIManager.Instance.OpenPanel(GameConfig.Panel.Menu);
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

        void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = alpha;
        }

        void SetFlyingItemsActive(bool isActive)
        {
            if (flyingItems == null)
                return;

            foreach (GameObject flyingItem in flyingItems)
            {
                if (flyingItem == null)
                    continue;

                flyingItem.SetActive(isActive);
            }
        }
    }
}
