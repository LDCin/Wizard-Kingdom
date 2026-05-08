using UnityEngine;

namespace UI
{
    public class PausePanel : Panel
    {
        public void ReturnToMenu()
        {
            Time.timeScale = 1;
            Close();
            SceneLoader.LoadScene("Menu", "Panel - Menu");
        }

        public void Restart()
        {
            Close();
            Time.timeScale = 1;
            SceneLoader.LoadScene("Game", "Panel - Game");
        }

        public void Continue()
        {
            Time.timeScale = 1;
            Close();
        }
    }
}