using Managers;
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
            var dm = DataManager.Instance;
            dm.SetBgmEnabled(!dm.BgmEnabled);
        }

        public void ToggleSfx()
        {
            var dm = DataManager.Instance;
            dm.SetSfxEnabled(!dm.SfxEnabled);
        }

        public void ToggleVibration()
        {
            var dm = DataManager.Instance;
            dm.SetVibrationEnabled(!dm.VibrationEnabled);
        }

        public void ResetData()
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
                DataManager.Instance.ResetUserData();
                Debug.Log("SettingPanel: user data reset.");
            }
            else
            {
                Debug.Log($"SettingPanel: press {remaining} time to reset data.");
            }
        }

        public void CloseSetting()
        {
            UIManager.Instance.ClosePanel(GameConfig.Panel.Setting);
            UIManager.Instance.OpenPanel(GameConfig.Panel.Menu);
        }
    }
}
