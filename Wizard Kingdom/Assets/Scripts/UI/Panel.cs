using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Panel : MonoBehaviour
    {
        [SerializeField] private bool _destroyOnClose = false;
        [SerializeField] private bool _hasEffect = false;
        [SerializeField] private UILayer uiLayer = UILayer.Overlay;
        [SerializeField] private GameObject _panelBoard;
        [SerializeField] private Image _panelBackground;

        private Coroutine _closeCoroutine;
        private float _targetBackgroundAlpha = -1f;

        public UILayer UILayer => uiLayer;

        public void Open()
        {
            if (_closeCoroutine != null)
            {
                StopCoroutine(_closeCoroutine);
                _closeCoroutine = null;
            }

            gameObject.SetActive(true);
            UpdateVisual();
            PlayOpenEffect();
        }

        public virtual void UpdateVisual()
        {
        }

        public void Close()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UIEffect effect = GetComponent<UIEffect>();
            if (effect != null && effect.UseCloseEffect)
            {
                if (_closeCoroutine != null)
                {
                    StopCoroutine(_closeCoroutine);
                }

                _closeCoroutine = StartCoroutine(CloseAfterEffect(effect));
                return;
            }

            PlayCloseEffect();
        }

        private IEnumerator CloseAfterEffect(UIEffect effect)
        {
            if (effect.DeltaTimeIndependent)
            {
                yield return new WaitForSecondsRealtime(effect.HidePanelDelayTime);
                effect.Close();
                yield return new WaitForSecondsRealtime(effect.ClosePanelDuration);
            }
            else
            {
                yield return new WaitForSeconds(effect.HidePanelDelayTime);
                effect.Close();
                yield return new WaitForSeconds(effect.ClosePanelDuration);
            }

            _closeCoroutine = null;
            CloseImmediately();
        }

        private void PlayOpenEffect()
        {
            if (GetComponent<UIEffect>() != null)
            {
                return;
            }

            if (!_hasEffect || _panelBoard == null)
            {
                return;
            }

            RectTransform rect = _panelBoard.transform as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.DOKill();

            Vector2 endPos = rect.anchoredPosition;
            rect.anchoredPosition = endPos + new Vector2(0f, -3000f);
            rect.DOAnchorPos(endPos, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);

            if (_panelBackground == null)
            {
                return;
            }

            if (_targetBackgroundAlpha < 0f)
            {
                _targetBackgroundAlpha = _panelBackground.color.a;
            }

            Color color = _panelBackground.color;
            color.a = 0f;
            _panelBackground.color = color;
            _panelBackground.DOFade(_targetBackgroundAlpha, 0.5f).SetUpdate(true);
        }

        private void PlayCloseEffect()
        {
            if (_hasEffect && _panelBoard != null)
            {
                RectTransform rect = _panelBoard.transform as RectTransform;
                if (rect != null)
                {
                    rect.DOKill();
                    Vector2 startPos = rect.anchoredPosition;
                    Vector2 endPos = startPos + new Vector2(0f, -3000f);

                    rect.DOAnchorPos(endPos, 0.35f)
                        .SetEase(Ease.OutCubic)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            CloseImmediately();
                            rect.anchoredPosition = startPos;
                        });
                    return;
                }
            }

            CloseImmediately();
        }

        private void CloseImmediately()
        {
            if (_destroyOnClose)
            {
                UIManager.Instance?.UnregisterPanel(name);
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
