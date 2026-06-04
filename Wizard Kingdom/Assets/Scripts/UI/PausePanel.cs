using System;
using UnityEngine;
using Utils;

namespace UI
{
    public class PausePanel : Panel
    {
        public static event Action OnContinueGame;
        public static event Action OnBackToMenu;
        public static event Action OnRestartGame;

        #region Analysis And Design Properties

        [SerializeField] private UnityEngine.UI.Button _subContinueGame;
        [SerializeField] private UnityEngine.UI.Button _subRestartGame;
        [SerializeField] private UnityEngine.UI.Button _subReturnToMenu;

        public UnityEngine.UI.Button SubContinueGame => _subContinueGame;
        public UnityEngine.UI.Button SubRestartGame => _subRestartGame;
        public UnityEngine.UI.Button SubReturnToMenu => _subReturnToMenu;

        #endregion
        private void HandleReturnToMenu()
        {
            OnBackToMenu?.Invoke();
            SceneLoader.LoadScene(GameConfig.Scene.Menu, GameConfig.Panel.Menu);
            Close();
        }

        public void Restart()
        {
            OnRestartGame?.Invoke();
            SceneLoader.LoadScene(GameConfig.Scene.Game, GameConfig.Panel.Game, GameConfig.Panel.DrawAreaFree);
            Close();
        }

        public void Continue()
        {
            OnContinueGame?.Invoke();
            Close();
        }

        #region Analysis And Design Methods

        public void ContinueGame() => Continue();
        public void RestartGame() => Restart();
        public void ReturnToMenu() => HandleReturnToMenu();
        public void continueGame() => ContinueGame();
        public void restartGame() => RestartGame();
        public void returnToMenu() => ReturnToMenu();

        #endregion
    }
}
