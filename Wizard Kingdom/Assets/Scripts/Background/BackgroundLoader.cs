using System.Collections;
using Managers;
using SOs;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace BackgroundSystem
{
    public class BackgroundLoader : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundRoot;

        private AsyncOperationHandle<GameplayCatalog> _catalogHandle;
        private bool _hasCatalogHandle;
        private GameObject _currentInstance;
        private GameplayCatalog _catalog;

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
            StartCoroutine(LoadAndApplyBackground());
        }

        private void OnDisable()
        {
            GameManager.OnModeLoaded -= HandleModeLoaded;
        }

        private void OnDestroy()
        {
            if (_hasCatalogHandle)
            {
                GameplayCatalogLoader.Release(_catalogHandle);
                _hasCatalogHandle = false;
            }
        }

        private IEnumerator LoadAndApplyBackground()
        {
            yield return GameplayCatalogLoader.Load((catalog, handle) =>
            {
                _catalogHandle = handle;
                _hasCatalogHandle = true;
                _catalog = catalog;

                if (_catalog == null) return;
                ApplyBackground(GameManager.Instance != null ? GameManager.Instance.CurrentModeData : null);
            });
        }

        private void HandleModeLoaded(GameModeData modeData)
        {
            if (_catalog == null) return;
            ApplyBackground(modeData);
        }

        private void ApplyBackground(GameModeData modeData)
        {
            string id = ResolveBackgroundId(modeData);
            if (string.IsNullOrEmpty(id)) return;

            BackgroundData data = _catalog.FindBackground(id);
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

            SpawnBackground(data.prefab);
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

        private void SpawnBackground(GameObject prefab)
        {
            if (_currentInstance != null)
            {
                Destroy(_currentInstance);
                _currentInstance = null;
            }
            _currentInstance = Instantiate(prefab, _backgroundRoot);
        }
    }
}
