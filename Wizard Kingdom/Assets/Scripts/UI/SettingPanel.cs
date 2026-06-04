using Managers;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SettingPanel : Panel
    {
        [System.Serializable]
        private class ToggleSprites
        {
            public Button targetButton;

            [Header("ON state")]
            public Sprite onNormal;
            public Sprite onPressed;

            [Header("OFF state")]
            public Sprite offNormal;
            public Sprite offPressed;

            public void Apply(bool isOn)
            {
                if (targetButton == null || targetButton.image == null) return;

                targetButton.image.sprite = isOn ? onNormal : offNormal;

                var spriteState = targetButton.spriteState;
                spriteState.pressedSprite = isOn ? onPressed : offPressed;
                targetButton.spriteState = spriteState;
            }
        }

        [Header("Toggle visuals")]
        [SerializeField] private ToggleSprites _bgmVisual;
        [SerializeField] private ToggleSprites _sfxVisual;
        [SerializeField] private ToggleSprites _vibrationVisual;

        #region Analysis And Design Properties

        public Button SubBGM => _bgmVisual != null ? _bgmVisual.targetButton : null;
        public Button SubSFX => _sfxVisual != null ? _sfxVisual.targetButton : null;
        public Button SubVibration => _vibrationVisual != null ? _vibrationVisual.targetButton : null;

        #endregion

        [Header("Reset (triple-tap)")]
        [Tooltip("Số click liên tiếp cần để reset data.")]
        [SerializeField] private int _resetClickThreshold = 3;
        [Tooltip("Thời gian (giây) reset counter nếu user không click tiếp.")]
        [SerializeField] private float _resetClickTimeout = 1f;

        private int _resetClickCount;
        private float _lastResetClickTime;

        private void OnEnable()
        {
            DataManager.OnSettingsChanged += RefreshAllVisuals;
            _resetClickCount = 0;
            RefreshAllVisuals();
        }

        private void OnDisable()
        {
            DataManager.OnSettingsChanged -= RefreshAllVisuals;
        }

        private void RefreshAllVisuals()
        {
            var dm = DataManager.Instance;
            if (dm == null) return;

            _bgmVisual.Apply(dm.BgmEnabled);
            _sfxVisual.Apply(dm.SfxEnabled);
            _vibrationVisual.Apply(dm.VibrationEnabled);
        }

        public void ToggleBgm()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ToggleBGM();
                return;
            }

            var dm = DataManager.Instance;
            dm?.ChangeBGMState(!dm.BgmEnabled);
        }

        public void ToggleSfx()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ToggleSFX();
                return;
            }

            var dm = DataManager.Instance;
            dm?.ChangeSFXState(!dm.SfxEnabled);
        }

        private void ToggleVibrationInternal()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ToggleVibration();
                return;
            }

            var dm = DataManager.Instance;
            dm?.ChangeVibrationState(!dm.VibrationEnabled);
        }

        private void ResetDataInternal()
        {
            float now = Time.unscaledTime;

            if (now - _lastResetClickTime > _resetClickTimeout)
            {
                _resetClickCount = 0;
            }

            _resetClickCount++;
            _lastResetClickTime = now;

            int remaining = _resetClickThreshold - _resetClickCount;

            if (_resetClickCount >= _resetClickThreshold)
            {
                _resetClickCount = 0;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetData();
                }
                else
                {
                    DataManager.Instance?.ResetUserData();
                }
                Debug.Log("SettingPanel: user data reset.");
            }
            else
            {
                Debug.Log($"SettingPanel: press {remaining} time to reset data.");
            }
        }

        #region Analysis And Design Methods

        public void ToggleBGM() => ToggleBgm();
        public void ToggleSFX() => ToggleSfx();
        public void ToggleVibration() => ToggleVibrationInternal();
        public void ResetData() => ResetDataInternal();

        public SettingsData GetData()
        {
            SettingsData settingsData = DataManager.Instance?.Data?.Get().settings.GetData();
            RefreshAllVisuals();
            return settingsData;
        }

        public SettingsData getData() => GetData();

        #endregion

        public void CloseSetting()
        {
            UIManager.Instance.ClosePanel(GameConfig.Panel.Setting);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Menu);
        }
    }
}
