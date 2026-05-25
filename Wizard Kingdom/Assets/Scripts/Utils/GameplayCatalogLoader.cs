using System;
using System.Collections;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    /// <summary>
    /// Helper load GameplayCatalog asset qua Addressables.
    /// Dùng pattern giống ShopCatalogLoader: caller giữ handle để release đúng lúc.
    /// </summary>
    public static class GameplayCatalogLoader
    {
        public const string CatalogAddress = "Gameplay Catalog";

        public static IEnumerator Load(
            Action<GameplayCatalog, AsyncOperationHandle<GameplayCatalog>> onLoaded)
        {
            var handle = Addressables.LoadAssetAsync<GameplayCatalog>(CatalogAddress);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"GameplayCatalogLoader: load failed cho address '{CatalogAddress}'.");
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
