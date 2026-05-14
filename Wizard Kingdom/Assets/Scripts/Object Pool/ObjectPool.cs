using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        private readonly Dictionary<TKey, Queue<TItem>> _poolDict = new();

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
                Debug.Log($"{GetType().Name}: Load data successfully!");

                InitPool();

                IsReady = true;

                Debug.Log($"{GetType().Name} is ready!");
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
                Debug.LogWarning($"{GetType().Name}: Duplicate data key: {key}");
                return;
            }

            _dataList.Add(data);

            if (!_poolDict.ContainsKey(key))
            {
                _poolDict.Add(key, new Queue<TItem>());
            }

            Debug.Log($"{GetType().Name}: Load data: {key}");
        }

        private void InitPool()
        {
            foreach (TData data in _dataList)
            {
                TKey key = GetKeyFromData(data);

                if (!_poolDict.ContainsKey(key))
                {
                    _poolDict.Add(key, new Queue<TItem>());
                }

                for (int i = 0; i < _numberOfEachItem; i++)
                {
                    TItem item = CreateItem(data);
                    _poolDict[key].Enqueue(item);
                }
            }
        }

        private TItem CreateItem(TData data)
        {
            TItem item = Instantiate(_prefab, transform);

            ApplyDataToItem(item, data);

            item.gameObject.SetActive(false);

            return item;
        }

        protected TItem Get(TKey key)
        {
            if (!_poolDict.TryGetValue(key, out Queue<TItem> queue))
            {
                Debug.LogError($"{GetType().Name}: Pool queue not found for key: {key}");
                return null;
            }

            if (queue.Count > 0)
            {
                TItem itemFromPool = queue.Dequeue();
                return itemFromPool;
            }

            TData data = GetData(key);

            if (data == null)
            {
                Debug.LogError($"{GetType().Name}: Data not found for key: {key}");
                return null;
            }

            TItem newItem = CreateItem(data);

            return newItem;
        }

        protected void Return(TItem item)
        {
            if (item == null)
            {
                return;
            }

            TKey key = GetKeyFromItem(item);

            if (!_poolDict.ContainsKey(key))
            {
                Debug.LogWarning($"{GetType().Name}: Pool queue missing when returning item. Key: {key}");
                _poolDict.Add(key, new Queue<TItem>());
            }

            OnReturn(item);

            _poolDict[key].Enqueue(item);
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
        }
    }
}