using System;
using Managers;
using UnityEngine;

namespace UI
{
    public class GameOverPanel : Panel
    {
        public static event Action OnRestartGame;
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
            UIManager.Instance.ClosePanel("Panel - Game Over");
            UIManager.Instance.OpenPanel("Panel - Shop");
        }
        public void Share(){
            Application.OpenURL("facebook.com");
        }
        public void Restart()
        {
            OnRestartGame?.Invoke();
            SceneLoader.LoadScene("Game", "Panel - Game", "Panel - Draw Area Free");
            Close();
        }
        public void BackToMenu(){
            SceneLoader.LoadScene("Menu");
        }
        public void MoreNitrome(){

        }
        public void RemoveAds(){

        }
    }
}