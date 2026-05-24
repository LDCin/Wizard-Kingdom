using System;
using System.Collections;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    public static class ShopCatalogLoader
    {
        public static IEnumerator Load(
            string address,
            Action<ShopCatalog, AsyncOperationHandle<ShopCatalog>> onLoaded)
        {
            var handle = Addressables.LoadAssetAsync<ShopCatalog>(address);
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"ShopCatalogLoader: load failed cho address '{address}'.");
                onLoaded?.Invoke(null, handle);
                yield break;
            }

            onLoaded?.Invoke(handle.Result, handle);
        }

        public static void Release(AsyncOperationHandle<ShopCatalog> handle)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
    }
}
