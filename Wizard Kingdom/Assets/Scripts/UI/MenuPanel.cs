using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MenuPanel : Panel
    {
        private void OnEnable()
        {
            RectTransform rect = transform as RectTransform;

            rect.DOKill();

            Vector2 endPos = rect.anchoredPosition;
            rect.anchoredPosition = endPos + new Vector2(0f, -500f);

            rect.DOAnchorPos(endPos, 0.35f).SetEase(Ease.OutCubic);
        }

        public void ArcadeGameMode()
        {
            Close();
            SceneLoader.LoadScene("Game", "Panel - Game", "Panel - Draw Area Free");
        }

        public void TimeAttackGameMode()
        {
            Close();
            SceneLoader.LoadScene("Game", "Panel - Game", "Panel - Draw Area Free");
        }

        public void Shop()
        {
            UIManager.Instance.ClosePanel("Panel - Menu");
            // UIManager.Instance.OpenPanel("Panel - Shop");
        }

        public void MoreNitrome()
        {
            
        }

        public void RemoveAds()
        {
            
        }
    }
}