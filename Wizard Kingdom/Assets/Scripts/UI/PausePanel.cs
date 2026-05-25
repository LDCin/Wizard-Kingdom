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
        public void ReturnToMenu()
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
    }
}