using System.Collections;
using Managers;
using SOs;
using UnityEngine;
using Utils;

namespace BackgroundSystem
{
    public class BackgroundLoader : MonoBehaviour
    {
        [SerializeField] private Transform _backgroundRoot;

        private GameObject _currentInstance;
        private bool _isCatalogReady;

        private void Awake()
        {
            if (_backgroundRoot == null) _backgroundRoot = transform;
        }

        private void OnEnable()
        {
            Observer.Subscribe<GameModeData>(ObserverEvent.ModeLoaded, HandleModeLoaded);
        }

        private void Start()
        {
            StartCoroutine(LoadAndApplyBackground());
        }

        private void OnDisable()
        {
            Observer.Unsubscribe<GameModeData>(ObserverEvent.ModeLoaded, HandleModeLoaded);
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

