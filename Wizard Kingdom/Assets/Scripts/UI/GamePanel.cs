using System;
using Managers;
using SOs;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class GamePanel : Panel
    {
        public static event Action OnPauseGame;
        [SerializeField] private SpriteAssetNumberText _arcadeScoreText;
        [SerializeField] private SpriteAssetNumberText _timeAttackScoreText;
        [SerializeField] private SpriteAssetNumberText _goldText;
        private void OnEnable()
        {
            GameManager.OnUpdateScoreAndGold += UpdateScoreAndGoldText;
            GameManager.OnModeLoaded += HandleModeLoaded;

            if (GameManager.Instance != null)
            {
                ApplyModeUI(GameManager.Instance.CurrentModeData);
                UpdateScoreAndGoldText(GameManager.Instance.Score, GameManager.Instance.Gold);
            }
        }
        private void OnDisable()
        {
            GameManager.OnUpdateScoreAndGold -= UpdateScoreAndGoldText;
            GameManager.OnModeLoaded -= HandleModeLoaded;
        }
        public void PauseGame()
        {
            OnPauseGame?.Invoke();
            UIManager.Instance.OpenPanel(GameConfig.Panel.Pause);
        }
        public void UpdateScoreAndGoldText(int newScore, int newGold)
        {
            if (_arcadeScoreText != null) _arcadeScoreText.SetValue(newScore);
            if (_timeAttackScoreText != null) _timeAttackScoreText.SetValue(newScore);
            _goldText.SetValue(newGold);
        }

        private void HandleModeLoaded(GameModeData modeData)
        {
            ApplyModeUI(modeData);
        }

        private void ApplyModeUI(GameModeData modeData)
        {
            bool showArcade = modeData == null || modeData.modeType == GameModeType.Arcade;
            if (_arcadeScoreText != null) _arcadeScoreText.gameObject.SetActive(showArcade);
            if (_timeAttackScoreText != null) _timeAttackScoreText.gameObject.SetActive(!showArcade);
        }
    }
}