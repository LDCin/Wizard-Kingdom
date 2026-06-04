using System.Collections;
using Managers;
using SOs;
using UnityEngine;

namespace BackgroundSystem
{
    public class BackgroundLoader : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundRoot;

        private GameObject _currentInstance;
        private bool _isCatalogReady;

        #region Analysis And Design Properties

        public GameObject OutPrefab => _currentInstance;

        #endregion

        private void Awake()
        {
            if (_backgroundRoot == null) _backgroundRoot = transform;
        }

        private void OnEnable()
        {
            GameManager.OnModeLoaded += HandleModeLoaded;
        }

        private void Start()
        {
            Init();
        }

        #region Analysis And Design Methods

        public void Init()
        {
            StartCoroutine(LoadAndApplyBackground());
        }

        #endregion

        private void OnDisable()
        {
            GameManager.OnModeLoaded -= HandleModeLoaded;
        }

        private IEnumerator LoadAndApplyBackground()
        {
            bool completed = false;
            DataManager.Instance.LoadGameplayCatalog(catalog =>
            {
                _isCatalogReady = catalog != null;
                completed = true;
            });

            yield return new WaitUntil(() => completed);

            if (!_isCatalogReady) yield break;
            ApplyBackground(GameManager.Instance != null ? GameManager.Instance.CurrentModeData : null);
        }

        private void HandleModeLoaded(GameModeData modeData)
        {
            if (!_isCatalogReady) return;
            ApplyBackground(modeData);
        }

        private void ApplyBackground(GameModeData modeData)
        {
            string id = ResolveBackgroundId(modeData);
            if (string.IsNullOrEmpty(id)) return;

            BackgroundData data = DataManager.Instance.FindBackgroundData(id);
            if (data == null)
            {
                Debug.LogWarning($"BackgroundLoader: not found BackgroundData for id '{id}' in GameplayCatalog.");
                return;
            }

            if (data.prefab == null)
            {
                Debug.LogWarning($"BackgroundLoader: BackgroundData '{id}' not set up prefab.");
                return;
            }

            SpawnBackground(data);
        }

        private string ResolveBackgroundId(GameModeData modeData)
        {
            if (modeData != null
                && modeData.modeType == GameModeType.TimeAttack
                && !string.IsNullOrEmpty(modeData.fixedBackgroundId))
            {
                return modeData.fixedBackgroundId;
            }

            return DataManager.Instance != null ? DataManager.Instance.GetEquippedBackground() : null;
        }

        private void SpawnBackground(BackgroundData data)
        {
            _currentInstance = data.Init(_backgroundRoot, _currentInstance);
        }
    }

    public class Background : BackgroundLoader
    {
    }
}
