using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MenuPanel : Panel
    {
        public static event Action<string> OnPlayGame;

        #region Analysis And Design Properties

        [SerializeField] private Button _subArcadeMode;
        [SerializeField] private Button _subTimeAttackMode;
        [SerializeField] private Button _subSetting;
        [SerializeField] private Button _subShopPanel;
        [SerializeField] private string _arcadeModeKey = GameConfig.GameMode.Arcade;
        [SerializeField] private string _timeAttackModeKey = GameConfig.GameMode.TimeAttack;

        public Button SubArcadeMode => _subArcadeMode;
        public Button SubTimeAttackMode => _subTimeAttackMode;
        public Button SubSetting => _subSetting;
        public Button SubShopPanel => _subShopPanel;

        #endregion

        #region Analysis And Design Methods

        public void ArcadeGameMode()
        {
            PlayGameMode(_arcadeModeKey);
        }

        public void TimeAttackGameMode()
        {
            PlayGameMode(_timeAttackModeKey);
        }

        public void arcadeGameMode() => ArcadeGameMode();
        public void timeAttackGameMode() => TimeAttackGameMode();

        public void Shop()
        {
            UIManager.Instance.ClosePanel(GameConfig.Panel.Menu);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Shop);
        }

        public void shop() => Shop();

        public void Setting()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OpenSetting();
                return;
            }

            UIManager.Instance.ClosePanel(GameConfig.Panel.Menu);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Setting);
        }

        public void setting() => Setting();

        #endregion

        private void PlayGameMode(string modeKey)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame(modeKey);
            }
            else
            {
                OnPlayGame?.Invoke(modeKey);
            }

            SceneLoader.LoadScene(GameConfig.Scene.Game, GameConfig.Panel.Game, GameConfig.Panel.DrawAreaFree);
        }

        public void MoreNitrome()
        {

        }

        public void RemoveAds()
        {

        }
    }
}
