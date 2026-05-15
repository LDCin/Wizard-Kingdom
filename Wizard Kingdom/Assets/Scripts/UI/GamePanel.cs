using System;
using UnityEngine;
using TMPro;

namespace UI
{
    public class GamePanel : Panel
    {
        public static event Action OnPauseGame;
        [SerializeField] private SpriteAssetScoreText _score;
        public void PauseGame()
        {
            OnPauseGame?.Invoke();
            UIManager.Instance.OpenPanel("Panel - Pause");
        }
    }
}