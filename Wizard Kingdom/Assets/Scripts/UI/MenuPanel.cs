using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MenuPanel : Panel
    {
        public static event Action<string> OnPlayGame;
        [SerializeField] private string _arcadeModeKey = GameConfig.GameMode.Arcade;
        [SerializeField] private string _timeAttackModeKey = GameConfig.GameMode.TimeAttack;

        public void ArcadeGameMode()
        {
            OnPlayGame?.Invoke(_arcadeModeKey);
            SceneLoader.LoadScene(GameConfig.Scene.Game, GameConfig.Panel.Game, GameConfig.Panel.DrawAreaFree);
        }

        public void TimeAttackGameMode()
        {
            OnPlayGame?.Invoke(_timeAttackModeKey);
            SceneLoader.LoadScene(GameConfig.Scene.Game, GameConfig.Panel.Game, GameConfig.Panel.DrawAreaFree);
        }

        public void Shop()
        {
            UIManager.Instance.ClosePanel(GameConfig.Panel.Menu);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Shop);
        }

        public void MoreNitrome()
        {

        }

        public void RemoveAds()
        {

        }
        public void Setting()
        {
            UIManager.Instance.ClosePanel(GameConfig.Panel.Menu);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Setting);
        }
    }
}
