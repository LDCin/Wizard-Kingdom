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

        #region Analysis And Design Properties

        [SerializeField] private SpriteAssetNumberText _totalGold;
        [SerializeField] private SpriteAssetNumberText _highScore;
        [SerializeField] private SpriteAssetNumberText _score;
        [SerializeField] private UnityEngine.UI.Button _subShop;
        [SerializeField] private UnityEngine.UI.Button _subRestartGame;
        [SerializeField] private UnityEngine.UI.Button _subShare;
        [SerializeField] private UnityEngine.UI.Button _subMenu;

        public SpriteAssetNumberText OutTotalGold => _totalGold;
        public SpriteAssetNumberText OutHighScore => _highScore;
        public SpriteAssetNumberText OutScore => _score;
        public UnityEngine.UI.Button SubShop => _subShop;
        public UnityEngine.UI.Button SubRestartGame => _subRestartGame;
        public UnityEngine.UI.Button SubShare => _subShare;
        public UnityEngine.UI.Button SubMenu => _subMenu;

        #endregion

        public void OnEnable()
        {
            _totalGold.SetValue(DataManager.Instance.GetGold());

            string modeKey = GameManager.Instance.CurrentModeKey;
            _highScore.SetValue(DataManager.Instance.GetHighScore(modeKey));

            _score.SetValue(GameManager.Instance.Score);
        }
        #region Analysis And Design Methods

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
        public void RestartGame() => Restart();
        public void BackToMenu(){
            OnBackToMenu?.Invoke();
            SceneLoader.LoadScene(GameConfig.Scene.Menu, GameConfig.Panel.Menu);
        }

        public void Share()
        {
            Application.OpenURL(GameConfig.Links.ShareLink);
        }

        #endregion

        public void MoreNitrome(){

        }
        public void RemoveAds(){

        }
    }
}
