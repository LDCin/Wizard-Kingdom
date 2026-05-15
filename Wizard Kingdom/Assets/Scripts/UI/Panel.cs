using System;
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
        private float _targetBackgroundAlpha = -1;

        public UILayer UILayer => uiLayer;
        public void Open()
        {
            PlayOpenEffect();
        }

        public void PlayOpenEffect()
        {
            gameObject.SetActive(true);
            if (_hasEffect && _panelBoard != null)
            {
                RectTransform rect = _panelBoard.transform as RectTransform;
                
                rect.DOKill();

                Vector2 endPos = rect.anchoredPosition;
                rect.anchoredPosition = endPos + new Vector2(0f, -500f);

                rect.DOAnchorPos(endPos, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true);

                if (_panelBackground != null)
                {
                    if (_targetBackgroundAlpha == -1)
                    {
                        _targetBackgroundAlpha = _panelBackground.color.a;
                    }
                    Color color = _panelBackground.color;
                    color.a = 0;
                    _panelBackground.color = color;
                    _panelBackground.DOFade(_targetBackgroundAlpha, 0.5f).SetUpdate(true);
                }
            }
        }
        public void PlayCloseEffect()
        {
            if (_hasEffect && _panelBoard != null)
            {
                RectTransform rect = _panelBoard.transform as RectTransform;
                
                rect.DOKill();

                Vector2 endPos = rect.anchoredPosition + new Vector2(0f, -1500f);

                rect.DOAnchorPos(endPos, 0.35f).SetEase(Ease.OutCubic).SetUpdate(true).OnComplete(() =>
                {
                    if (_destroyOnClose)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        gameObject.SetActive(false);
                        rect.anchoredPosition = endPos + new Vector2(0f, 1500f);
                    }
                });
                
                // if (_panelBackground != null)
                // {
                //     if (_targetBackgroundAlpha == -1)
                //     {
                //         _targetBackgroundAlpha = _panelBackground.color.a;
                //     }
                //     Color color = _panelBackground.color;
                //     color.a = 0;
                //     _panelBackground.color = color;
                //     _panelBackground.DOFade(0, 0.5f).SetUpdate(true);
                // }
            }
            else
            {
                if (_destroyOnClose)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public void Close()
        {
            PlayCloseEffect();
        }
    }
}