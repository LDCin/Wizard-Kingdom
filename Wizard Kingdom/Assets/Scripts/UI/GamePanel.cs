using System;
using System.Collections;
using Managers;
using SOs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class GamePanel : Panel
    {
        public static event Action OnPauseGame;
        [SerializeField] private SpriteAssetNumberText _arcadeScoreText;
        [SerializeField] private SpriteAssetNumberText _timeAttackScoreText;
        [SerializeField] private SpriteAssetNumberText _goldText;
        [Header("Spell Usage UI")]
        [SerializeField] private Image _spellIconImage;
        [SerializeField] private SpriteAssetNumberText _spellRemainingText;
        [SerializeField] private float _spellDisplayDuration = 1.5f;
        [SerializeField] private float _spellBlinkDuration = 1.0f;
        [SerializeField] private float _spellBlinkInterval = 0.15f;
        private Coroutine _spellDisplayCoroutine;
        private void OnEnable()
        {
            GameManager.OnUpdateScoreAndGold += UpdateScoreAndGoldText;
            GameManager.OnModeLoaded += HandleModeLoaded;
            SpellCaster.OnSpellUsageUpdated += ShowSpellUsage;

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
            SpellCaster.OnSpellUsageUpdated -= ShowSpellUsage;
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

        private void ShowSpellUsage(Sprite icon, int remainingUses)
        {
            if (_spellIconImage == null || _spellRemainingText == null) return;

            _spellIconImage.sprite = icon;
            _spellIconImage.enabled = icon != null;
            _spellRemainingText.SetValue(remainingUses);

            if (_spellDisplayCoroutine != null)
            {
                StopCoroutine(_spellDisplayCoroutine);
            }

            _spellDisplayCoroutine = remainingUses <= 0
                ? StartCoroutine(SpellBlinkRoutine())
                : StartCoroutine(SpellDisplayRoutine());
        }

        private IEnumerator SpellDisplayRoutine()
        {
            _spellIconImage.gameObject.SetActive(true);
            _spellRemainingText.gameObject.SetActive(true);

            if (_spellDisplayDuration > 0f)
            {
                yield return new WaitForSeconds(_spellDisplayDuration);
            }

            _spellIconImage.gameObject.SetActive(false);
            _spellRemainingText.gameObject.SetActive(false);
            _spellDisplayCoroutine = null;
        }

        private IEnumerator SpellBlinkRoutine()
        {
            _spellIconImage.gameObject.SetActive(true);
            _spellRemainingText.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < _spellBlinkDuration)
            {
                bool visible = Mathf.FloorToInt(elapsed / _spellBlinkInterval) % 2 == 0;
                _spellIconImage.gameObject.SetActive(visible && _spellIconImage.sprite != null);
                _spellRemainingText.gameObject.SetActive(visible);
                yield return new WaitForSeconds(_spellBlinkInterval);
                elapsed += _spellBlinkInterval;
            }

            _spellIconImage.gameObject.SetActive(false);
            _spellRemainingText.gameObject.SetActive(false);
            _spellDisplayCoroutine = null;
        }
    }
}