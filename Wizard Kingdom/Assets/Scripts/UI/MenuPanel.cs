using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MenuPanel : Panel
    {
        public static event Action<string> OnPlayGame;
        [SerializeField] private string _arcadeModeKey = "ArcadeMode";
        [SerializeField] private string _timeAttackModeKey = "TimeAttackMode";

        public void ArcadeGameMode()
        {
            OnPlayGame?.Invoke(_arcadeModeKey);
            SceneLoader.LoadScene("Game", "Panel - Game", "Panel - Draw Area Free");
        }

        public void TimeAttackGameMode()
        {
            // CHANGED: gửi key, không gửi SO
            OnPlayGame?.Invoke(_timeAttackModeKey);
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
