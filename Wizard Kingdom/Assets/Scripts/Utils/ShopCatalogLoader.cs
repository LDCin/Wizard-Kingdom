using System;
using System.Collections;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Utils
{
    /// <summary>
    /// Helper load ShopCatalog asset qua Addressables.
    /// 1 catalog duy nhất chứa toàn bộ items, mỗi shop panel lấy list theo category mình cần.
    /// </summary>
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
