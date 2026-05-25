using System;
using System.Collections;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    public static class GameplayCatalogLoader
    {
        public static IEnumerator Load(
            Action<GameplayCatalog, AsyncOperationHandle<GameplayCatalog>> onLoaded)
        {
            var handle = Addressables.LoadAssetAsync<GameplayCatalog>(GameConfig.Addressables.GameplayCatalog);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"GameplayCatalogLoader: load failed cho address '{GameConfig.Addressables.GameplayCatalog}'.");
                onLoaded?.Invoke(null, handle);
                yield break;
            }

            onLoaded?.Invoke(handle.Result, handle);
        }

        public static void Release(AsyncOperationHandle<GameplayCatalog> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}
