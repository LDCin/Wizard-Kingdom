using UnityEngine;
using Enemies;
using Utils;

namespace Managers
{
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource _BGM;
        [SerializeField] private AudioSource _SFX;

        [SerializeField] private AudioClip _defaultSFX;
        // [SerializeField] private AudioClip _gameOverSound;
        [SerializeField] private AudioClip _balloonPopSound;
        [SerializeField] private AudioClip _enemyBreakSound;

        public override void Awake()
        {
            base.Awake();

            ApplySettingsFromUserData();
        }

        private void OnEnable()
        {
            DataManager.OnSettingsChanged += ApplySettingsFromUserData;
            Enemy.OnBalloonPop += PlayBalloonPopSound;
            Enemy.OnEnemyDie += HandleEnemyDie;
        }

        private void OnDisable()
        {
            DataManager.OnSettingsChanged -= ApplySettingsFromUserData;
            Enemy.OnBalloonPop -= PlayBalloonPopSound;
            Enemy.OnEnemyDie -= HandleEnemyDie;
        }

        private void ApplySettingsFromUserData()
        {
            var dm = DataManager.Instance;
            if (dm == null)
            {
                Debug.LogWarning("AudioManager: DataManager not found, defaulting audio enabled.");
                SetBgmEnabled(true);
                SetSfxEnabled(true);
                return;
            }

            SetBgmEnabled(dm.BgmEnabled);
            SetSfxEnabled(dm.SfxEnabled);
        }

        private void SetBgmEnabled(bool enabled)
        {
            if (_BGM == null)
            {
                Debug.Log("Not Found: BGM");
                return;
            }

            _BGM.loop = true;
            _BGM.mute = !enabled;
        }

        private void SetSfxEnabled(bool enabled)
        {
            if (_SFX == null)
            {
                Debug.Log("Not Found: SFX");
                return;
            }

            _SFX.mute = !enabled;
        }

        private void HandleEnemyDie(int scoreReward, int goldReward)
        {
            PlayEnemyBreakSound();
        }

        public void PlayBGM()
        {
            if (_BGM == null)
            {
                Debug.Log("Not Found: BGM");
                return;
            }

            _BGM.loop = true;
            _BGM.mute = false;

            var dm = DataManager.Instance;
            if (dm != null) dm.SetBgmEnabled(true);
        }

        public void StopBGM()
        {
            if (_BGM == null)
            {
                Debug.Log("Not Found: BGM");
                return;
            }

            _BGM.mute = true;

            var dm = DataManager.Instance;
            if (dm != null) dm.SetBgmEnabled(false);
        }

        public void StopSFX()
        {
            if (_SFX == null)
            {
                Debug.Log("Not Found: SFX");
                return;
            }

            _SFX.mute = true;

            var dm = DataManager.Instance;
            if (dm != null) dm.SetSfxEnabled(false);
        }

        public void PlaySFX()
        {
            if (_SFX == null)
            {
                Debug.Log("Not Found: SFX");
                return;
            }

            _SFX.mute = false;

            var dm = DataManager.Instance;
            if (dm != null) dm.SetSfxEnabled(true);
        }

        public void ChangeSFXState()
        {
            var dm = DataManager.Instance;
            if (dm == null)
            {
                Debug.LogWarning("AudioManager: DataManager not found.");
                return;
            }

            if (dm.SfxEnabled) StopSFX();
            else PlaySFX();
        }

        public void ChangeBGMState()
        {
            var dm = DataManager.Instance;
            if (dm == null)
            {
                Debug.LogWarning("AudioManager: DataManager not found.");
                return;
            }

            if (dm.BgmEnabled) StopBGM();
            else PlayBGM();
        }


        public void PlayBalloonPopSound()
        {
            if (_SFX == null || _balloonPopSound == null)
            {
                Debug.Log("Not Found: Balloon Sound");
                return;
            }

            _SFX.PlayOneShot(_balloonPopSound);
        }
        
        public void PlayEnemyBreakSound()
        {
            if (_SFX == null || _enemyBreakSound == null)
            {
                Debug.Log("Not Found: Enemy Break Sound");
                return;
            }

            _SFX.PlayOneShot(_enemyBreakSound);
        }
    }
}