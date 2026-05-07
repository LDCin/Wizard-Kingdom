using System.Collections;
using System.Collections.Generic;
using Balloons;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Enemies
{
    public class EnemyPool : MonoBehaviour
    {
        [SerializeField] private List<EnemyData> _enemyDataList = new();
        [SerializeField] private Enemy _enemyPrefab;
        private readonly Dictionary<string, Queue<Enemy>> _enemyDict = new();
        [SerializeField] private string _enemyDataLabel = "Enemy Data";
        [SerializeField] private int numberOfEachEnemy = 10;
        [SerializeField] private BalloonPool _balloonPoolPrefab;
        private BalloonPool _balloonPool;
        public bool IsReady { get; private set; }
        
        private void OnEnable()
        {
            Enemy.OnReturnEnemyToPool += ReturnEnemyToPool;
        }
        private void OnDisable()
        {
            Enemy.OnReturnEnemyToPool -= ReturnEnemyToPool;
        }

        private IEnumerator Start()
        {
            _balloonPool = Instantiate(_balloonPoolPrefab, transform);

            yield return new WaitUntil(() => _balloonPool.IsReady);

            yield return LoadEnemyData();
        }
        private IEnumerator LoadEnemyData()
        {
            IsReady = false;

            var enemyLoadHandle = Addressables.LoadAssetsAsync<EnemyData>(
                _enemyDataLabel,
                loadedEnemy =>
                {
                    _enemyDataList.Add(loadedEnemy);
                    Debug.Log("Load Data Of Enemy: " + loadedEnemy.enemyName);
                },
                true
            );

            yield return enemyLoadHandle;

            if (enemyLoadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Load Enemy Data Successfully!");

                InitEnemy();

                IsReady = true;

                Debug.Log("EnemyPool is ready!");
            }
            else
            {
                Debug.LogError("Load Enemy Data Failed with label: " + _enemyDataLabel);
            }
        }

        private void InitEnemy()
        {
            foreach (var enemyData in _enemyDataList)
            {
                if (!_enemyDict.ContainsKey(enemyData.enemyName))
                {
                    _enemyDict.Add(enemyData.enemyName, new Queue<Enemy>());
                }

                for (int numberOfEnemy = 0; numberOfEnemy < numberOfEachEnemy; numberOfEnemy++)
                {
                    Enemy newEnemy = CreateEnemy(enemyData);
                    _enemyDict[enemyData.enemyName].Enqueue(newEnemy);
                }
            }
        }
        private Enemy CreateEnemy(EnemyData enemyData)
        {
            Enemy newEnemy = Instantiate(_enemyPrefab, transform);

            newEnemy.InitEnemyData(
                enemyData.enemyName,
                enemyData.sprite,
                enemyData.runtimeAnimatorController,
                enemyData.goldReward,
                enemyData.scoreReward,
                enemyData.moveSpeed
            );

            newEnemy.gameObject.SetActive(false);

            return newEnemy;
        }
        private List<Balloon> GetRandomBalloonsForEnemy(EnemyData enemyData)
        {
            List<Symbol> tempSymbols = new(enemyData.possibleBalloonSymbols);
            List<Balloon> result = new();

            int count = enemyData.balloonSpawnCount;

            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, tempSymbols.Count);

                Symbol selectedSymbol = tempSymbols[randomIndex];
                Balloon balloon = _balloonPool.GetBalloon(selectedSymbol);

                result.Add(balloon);

                tempSymbols.RemoveAt(randomIndex);
            }

            return result;
        }

        public Enemy GetEnemyByName(string enemyName)
        {
            EnemyData enemyData = _enemyDataList.Find(data => data.enemyName == enemyName);

            if (enemyData == null)
            {
                Debug.LogError("EnemyData not found: " + enemyName);
                return null;
            }

            Queue<Enemy> enemyQueue = _enemyDict[enemyName];

            Enemy enemy;

            if (enemyQueue.Count > 0)
            {
                enemy = enemyQueue.Dequeue();
            }
            else
            {
                enemy = CreateEnemy(enemyData);
            }

            List<Balloon> balloons = GetRandomBalloonsForEnemy(enemyData);

            enemy.ResetEnemyState();
            enemy.SetupBalloons(balloons);
            
            return enemy;
        }
        
        public string GetRandomEnemyName()
        {
            int randomIndex = Random.Range(0, _enemyDataList.Count);
            return _enemyDataList[randomIndex].enemyName;
        }

        public void ReturnEnemyToPool(Enemy enemy)
        {
            enemy.ReturnBalloonsToPool(_balloonPool);

            enemy.gameObject.SetActive(false);

            _enemyDict[enemy.EnemyName].Enqueue(enemy);
        }
    }
}