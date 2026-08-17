using UnityEngine;
using Utils;

namespace UI
{
    public class PausePanel : Panel
    {
        public void ReturnToMenu()
        {
            Observer.Publish(ObserverEvent.BackToMenu);
            SceneLoader.LoadScene(GameConfig.Scene.Menu, GameConfig.Panel.Menu);
            Close();
        }

        public void Restart()
        {
            Observer.Publish(ObserverEvent.RestartGame);
            SceneLoader.LoadScene(GameConfig.Scene.Game, GameConfig.Panel.Game, GameConfig.Panel.DrawAreaFree);
            Close();
        }

        public void Continue()
        {
            Observer.Publish(ObserverEvent.ContinueGame);
            Close();
        }
    }
}
