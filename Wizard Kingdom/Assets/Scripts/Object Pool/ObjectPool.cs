using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UPool = uPools.ObjectPool<UnityEngine.MonoBehaviour>;

namespace ObjectPool
{
    public abstract class ObjectPool<TItem, TData, TKey> : MonoBehaviour
        where TItem : MonoBehaviour
        where TData : ScriptableObject
    {
        [Header("Pool Settings")]
        [SerializeField] private TItem _prefab;
        [SerializeField] private string _dataLabel;
        [SerializeField] private int _numberOfEachItem = 10;

        private readonly List<TData> _dataList = new();
        private readonly Dictionary<TKey, UPool> _poolDict = new();
        private readonly HashSet<TItem> _activeItems = new();

        private AsyncOperationHandle<IList<TData>> _loadHandle;

        public bool IsReady { get; private set; }

        protected IReadOnlyList<TData> DataList => _dataList;

        protected IEnumerator InitializeAsync()
        {
            IsReady = false;

            _loadHandle = Addressables.LoadAssetsAsync<TData>(
                _dataLabel,
                OnDataLoaded,
                true
            );

            yield return _loadHandle;

            if (_loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                InitPool();

                IsReady = true;
            }
            else
            {
                Debug.LogError($"{GetType().Name}: Load data failed with label: {_dataLabel}");
            }
        }

        private void OnDataLoaded(TData data)
        {
            TKey key = GetKeyFromData(data);

            if (HasData(key))
            {
                return;
            }

            _dataList.Add(data);
        }

        private void InitPool()
        {
            foreach (TData data in _dataList)
            {
                TKey key = GetKeyFromData(data);

                if (_poolDict.ContainsKey(key))
                {
                    continue;
                }

                UPool pool = CreatePool(data);
                pool.Prewarm(_numberOfEachItem);
                _poolDict.Add(key, pool);
            }
        }

        private UPool CreatePool(TData data)
        {
            return new UPool(
                () => CreateItem(data),
                OnRentFromPool,
                OnReturnToPool,
                item => Destroy(item.gameObject)
            );
        }

        private MonoBehaviour CreateItem(TData data)
        {
            TItem item = Instantiate(_prefab, transform);

            ApplyDataToItem(item, data);

            item.gameObject.SetActive(false);

            return item;
        }

        protected TItem Get(TKey key)
        {
            if (!_poolDict.TryGetValue(key, out UPool pool))
            {
                Debug.LogError($"{GetType().Name}: Pool not found for key: {key}");
                return null;
            }

            TItem itemFromPool = pool.Rent() as TItem;
            if (itemFromPool == null)
            {
                Debug.LogError($"{GetType().Name}: Pool item type mismatch. Key: {key}");
                return null;
            }

            _activeItems.Add(itemFromPool);

            return itemFromPool;
        }

        protected void Return(TItem item)
        {
            if (item == null)
            {
                return;
            }

            if (!_activeItems.Remove(item))
            {
                return;
            }

            TKey key = GetKeyFromItem(item);

            if (!_poolDict.TryGetValue(key, out UPool pool))
            {
                Debug.LogWarning($"{GetType().Name}: Pool missing when returning item. Key: {key}");
                OnReturn(item);
                return;
            }

            pool.Return(item);
        }

        protected TData GetData(TKey key)
        {
            foreach (TData data in _dataList)
            {
                if (EqualityComparer<TKey>.Default.Equals(GetKeyFromData(data), key))
                {
                    return data;
                }
            }

            return null;
        }

        protected TData GetRandomData()
        {
            if (_dataList.Count <= 0)
            {
                Debug.LogError($"{GetType().Name}: Data list is empty.");
                return null;
            }

            int randomIndex = Random.Range(0, _dataList.Count);
            return _dataList[randomIndex];
        }

        private bool HasData(TKey key)
        {
            foreach (TData data in _dataList)
            {
                if (EqualityComparer<TKey>.Default.Equals(GetKeyFromData(data), key))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnRentFromPool(MonoBehaviour item)
        {
            if (item == null)
            {
                return;
            }

            item.transform.SetParent(transform);
        }

        private void OnReturnToPool(MonoBehaviour item)
        {
            if (item is TItem typedItem)
            {
                OnReturn(typedItem);
            }
        }

        protected virtual void OnReturn(TItem item)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(transform);
        }

        protected abstract TKey GetKeyFromData(TData data);
        protected abstract TKey GetKeyFromItem(TItem item);
        protected abstract void ApplyDataToItem(TItem item, TData data);

        protected virtual void OnDestroy()
        {
            if (_loadHandle.IsValid())
            {
                Addressables.Release(_loadHandle);
            }

            foreach (UPool pool in _poolDict.Values)
            {
                if (!pool.IsDisposed)
                {
                    pool.Dispose();
                }
            }

            _poolDict.Clear();
            _activeItems.Clear();
        }
    }
}
