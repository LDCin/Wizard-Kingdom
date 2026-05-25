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
        [Tooltip("Parent transform để spawn prefab BG vào. Mặc định = transform của GameObject này.")]
        [SerializeField] private Transform _backgroundRoot;

        private AsyncOperationHandle<GameplayCatalog> _catalogHandle;
        private bool _hasCatalogHandle;
        private GameObject _currentInstance;

        private void Awake()
        {
            if (_backgroundRoot == null) _backgroundRoot = transform;
        }

        private void Start()
        {
            StartCoroutine(LoadAndApplyBackground());
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

                if (catalog == null) return;
                if (DataManager.Instance == null) return;

                string id = DataManager.Instance.GetEquippedBackground();
                BackgroundData data = catalog.FindBackground(id);

                if (data == null)
                {
                    Debug.LogWarning($"BackgroundLoader: không tìm thấy BackgroundData cho id '{id}' trong GameplayCatalog.");
                    return;
                }

                if (data.prefab == null)
                {
                    Debug.LogWarning($"BackgroundLoader: BackgroundData '{id}' chưa gán prefab.");
                    return;
                }

                SpawnBackground(data.prefab);
            });
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
