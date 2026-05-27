using System;
using Managers;
using UnityEngine;
using Utils;

namespace UI
{
    public class GameOverPanel : Panel
    {
        public static event Action OnRestartGame;
        public static event Action OnBackToMenu;
        [SerializeField] private SpriteAssetNumberText _totalCoin;
        [SerializeField] private SpriteAssetNumberText _highScore;
        [SerializeField] private SpriteAssetNumberText _score;

        public void OnEnable()
        {
            _totalCoin.SetValue(DataManager.Instance.GetCoin());

            string modeKey = GameManager.Instance.CurrentModeKey;
            _highScore.SetValue(DataManager.Instance.GetHighScore(modeKey));

            _score.SetValue(GameManager.Instance.Score);
        }
        public void Shop(){
            UIManager.Instance.ClosePanel(GameConfig.Panel.GameOver);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Shop);
        }
        public void Restart()
        {
            OnRestartGame?.Invoke();
            SceneLoader.LoadScene(GameConfig.Scene.Game, GameConfig.Panel.Game, GameConfig.Panel.DrawAreaFree);
            Close();
        }
        public void BackToMenu(){
            OnBackToMenu?.Invoke();
            SceneLoader.LoadScene(GameConfig.Scene.Menu, GameConfig.Panel.Menu);
        }

        public void Share()
        {
            Application.OpenURL(GameConfig.Links.ShareLink);
        }
        public void MoreNitrome(){

        }
        public void RemoveAds(){

        }
    }
}