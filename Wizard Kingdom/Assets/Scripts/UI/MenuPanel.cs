using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MenuPanel : Panel
    {
        public static event Action OnPlayGame;
        public void ArcadeGameMode()
        {
            OnPlayGame?.Invoke();
            SceneLoader.LoadScene("Game", "Panel - Game", "Panel - Draw Area Free");
        }

        public void TimeAttackGameMode()
        {
            OnPlayGame?.Invoke();
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