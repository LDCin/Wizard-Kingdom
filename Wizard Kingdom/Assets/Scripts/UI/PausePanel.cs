using System;
using UnityEngine;

namespace UI
{
    public class PausePanel : Panel
    {
        public static event Action OnContinueGame;
        public static event Action OnBackToMenu;
        public static event Action OnRestartGame;
        public void ReturnToMenu()
        {
            OnBackToMenu?.Invoke();
            SceneLoader.LoadScene("Menu", "Panel - Menu");
            Close();
        }

        public void Restart()
        {
            OnRestartGame?.Invoke();
            SceneLoader.LoadScene("Game", "Panel - Game", "Panel - Draw Area Free");
            Close();
        }

        public void Continue()
        {
            OnContinueGame?.Invoke();
            Close();
        }
    }
}