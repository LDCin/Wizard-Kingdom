using System;
using Managers;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class GamePanel : Panel
    {
        public static event Action OnPauseGame;
        [SerializeField] private SpriteAssetNumberText _scoreText;
        [SerializeField] private SpriteAssetNumberText _goldText;
        private void OnEnable()
        {
            GameManager.OnUpdateScoreAndGold += UpdateScoreAndGoldText;

            if (GameManager.Instance != null)
            {
                UpdateScoreAndGoldText(GameManager.Instance.Score, GameManager.Instance.Gold);
            }
        }
        private void OnDisable()
        {
            GameManager.OnUpdateScoreAndGold -= UpdateScoreAndGoldText;
        }
        public void PauseGame()
        {
            OnPauseGame?.Invoke();
            UIManager.Instance.OpenPanel(GameConfig.Panel.Pause);
        }
        public void UpdateScoreAndGoldText(int newScore, int newGold)
        {
            _scoreText.SetValue(newScore);
            _goldText.SetValue(newGold);
        }
    }
}