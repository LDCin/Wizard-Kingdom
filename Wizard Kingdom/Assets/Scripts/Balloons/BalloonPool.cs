using System.Collections;
using System.Collections.Generic;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Balloons
{
    public class BalloonPool : MonoBehaviour
    {
        [SerializeField] private Balloon _balloonPrefab;
        [SerializeField] private string _balloonDataLabel = "Balloon Data";
        [SerializeField] private int _numberOfEachBalloon = 20;

        private readonly List<BalloonData> _balloonDataList = new();
        private readonly Dictionary<Symbol, Queue<Balloon>> _balloonDict = new();
        private readonly Dictionary<Symbol, BalloonData> _balloonDataDict = new();

        private AsyncOperationHandle<IList<BalloonData>> _balloonLoadHandle;

        public bool IsReady { get; private set; }

        private void Start()
        {
            StartCoroutine(LoadBalloonData());
        }

        private IEnumerator LoadBalloonData()
        {
            IsReady = false;

            _balloonLoadHandle = Addressables.LoadAssetsAsync<BalloonData>(
                _balloonDataLabel,
                loadedBalloon =>
                {
                    _balloonDataList.Add(loadedBalloon);

                    if (!_balloonDataDict.ContainsKey(loadedBalloon.symbol))
                    {
                        _balloonDataDict.Add(loadedBalloon.symbol, loadedBalloon);
                    }

                    Debug.Log("Load Balloon Data: " + loadedBalloon.symbol);
                },
                true
            );

            yield return _balloonLoadHandle;

            if (_balloonLoadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Load Balloon Data Successfully!");

                InitBalloonPool();

                IsReady = true;

                Debug.Log("BalloonPool is ready!");
            }
            else
            {
                Debug.LogError("Load Balloon Data Failed with label: " + _balloonDataLabel);
            }
        }

        private void InitBalloonPool()
        {
            foreach (BalloonData balloonData in _balloonDataList)
            {
                if (!_balloonDict.ContainsKey(balloonData.symbol))
                {
                    _balloonDict.Add(balloonData.symbol, new Queue<Balloon>());
                }

                for (int i = 0; i < _numberOfEachBalloon; i++)
                {
                    Balloon balloon = CreateBalloon(balloonData);
                    _balloonDict[balloonData.symbol].Enqueue(balloon);
                }
            }
        }

        private Balloon CreateBalloon(BalloonData balloonData)
        {
            Balloon balloon = Instantiate(_balloonPrefab, transform);

            balloon.InitBalloonData(
                balloonData.symbol,
                balloonData.sprite,
                balloonData.runtimeAnimatorController
            );

            balloon.gameObject.SetActive(false);

            return balloon;
        }

        public Balloon GetBalloon(Symbol symbol)
        {
            Queue<Balloon> balloonQueue = _balloonDict[symbol];

            if (balloonQueue.Count > 0)
            {
                Balloon balloonFromPool = balloonQueue.Dequeue();
                return balloonFromPool;
            }

            BalloonData balloonData = _balloonDataDict[symbol];

            Balloon newBalloon = CreateBalloon(balloonData);
            return newBalloon;
        }

        public void ReturnBalloon(Balloon balloon)
        {
            if (balloon == null)
            {
                return;
            }

            balloon.gameObject.SetActive(false);
            balloon.transform.SetParent(transform);

            _balloonDict[balloon.Symbol].Enqueue(balloon);
        }

        private void OnDestroy()
        {
            if (_balloonLoadHandle.IsValid())
            {
                Addressables.Release(_balloonLoadHandle);
            }
        }
    }
}