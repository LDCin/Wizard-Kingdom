using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPool _enemyPoolPrefab;
        [SerializeField] private List<GameObject> _spawnPointList = new();

        private EnemyPool _enemyPool;

        private IEnumerator Start()
        {
            _enemyPool = Instantiate(_enemyPoolPrefab, transform);

            yield return new WaitUntil(() => _enemyPool.IsReady);

            Debug.Log("Init Enemy Pool Successfully");

            SpawnEnemyRandom();
        }

        private void SpawnEnemyByName(string enemyName, GameObject spawnPoint)
        {
            Enemy enemy = _enemyPool.GetEnemyByName(enemyName);

            if (enemy == null)
            {
                return;
            }

            enemy.transform.position = spawnPoint.transform.position;
            enemy.gameObject.SetActive(true);
        }

        private void SpawnEnemyRandom()
        {
            for (int i = 0; i < _spawnPointList.Count; i++)
            {
                string enemyName = _enemyPool.GetRandomEnemyName();

                Enemy enemy = _enemyPool.GetEnemyByName(enemyName);

                if (enemy == null)
                {
                    continue;
                }

                enemy.transform.position = _spawnPointList[i].transform.position;
                enemy.gameObject.SetActive(true);
            }
        }
    }
}