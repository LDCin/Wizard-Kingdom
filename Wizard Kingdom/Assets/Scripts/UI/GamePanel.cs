using UnityEngine;
using TMPro;

namespace UI
{
    public class GamePanel : Panel
    {
        [SerializeField] private SpriteAssetScoreText _score;
        public void PauseGame()
        {
            Time.timeScale = 0;
            UIManager.Instance.OpenPanel("Panel - Pause");
        }
    }
}