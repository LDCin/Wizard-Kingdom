using System.Collections;
using System.Collections.Generic;
using Balloons;
using SOs;
using UnityEngine;
using Enemies;

namespace ObjectPool
{
    public class EnemyPool : ObjectPool<Enemy, EnemyData, string>
    {
        [Header("Balloon Pool")]
        [SerializeField] private BalloonPool _balloonPoolPrefab;

        private BalloonPool _balloonPool;

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

            yield return InitializeAsync();
        }

        protected override string GetKeyFromData(EnemyData data)
        {
            return data.enemyName;
        }

        protected override string GetKeyFromItem(Enemy enemy)
        {
            return enemy.EnemyName;
        }

        protected override void ApplyDataToItem(Enemy enemy, EnemyData data)
        {
            enemy.InitEnemyData(
                data.enemyName,
                data.sprite,
                data.runtimeAnimatorController,
                data.goldReward,
                data.scoreReward,
                data.moveSpeed
            );
        }

        public Enemy GetEnemyByName(string enemyName)
        {
            EnemyData enemyData = GetData(enemyName);

            if (enemyData == null)
            {
                Debug.LogError("EnemyData not found: " + enemyName);
                return null;
            }

            Enemy enemy = Get(enemyName);

            if (enemy == null)
            {
                return null;
            }

            List<Balloon> balloons = GetRandomBalloonsForEnemy(enemyData);

            enemy.ResetEnemyState();
            enemy.SetupBalloons(balloons);

            return enemy;
        }

        public string GetRandomEnemyName()
        {
            EnemyData randomEnemyData = GetRandomData();

            if (randomEnemyData == null)
            {
                return string.Empty;
            }

            return randomEnemyData.enemyName;
        }

        public void ReturnEnemyToPool(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            enemy.ReturnBalloonsToPool(_balloonPool);

            Return(enemy);
        }

        private List<Balloon> GetRandomBalloonsForEnemy(EnemyData enemyData)
        {
            List<Symbol> tempSymbols = new(enemyData.possibleBalloonSymbols);
            List<Balloon> result = new();

            int count = Mathf.Min(enemyData.balloonSpawnCount, tempSymbols.Count);

            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, tempSymbols.Count);

                Symbol selectedSymbol = tempSymbols[randomIndex];
                Balloon balloon = _balloonPool.GetBalloon(selectedSymbol);

                if (balloon != null)
                {
                    result.Add(balloon);
                }

                tempSymbols.RemoveAt(randomIndex);
            }

            return result;
        }
    }
}