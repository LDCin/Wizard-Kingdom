using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectPool;

namespace Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPool _enemyPoolPrefab;
        [SerializeField] private List<GameObject> _spawnPointList = new();

        private EnemyPool _enemyPool;
        private Coroutine _spawnCoroutine;
        private bool _canSpawn;

        public void StartSpawn(List<string> spawnEnemyNameList, float delayTime)
        {
            StopSpawn();

            _canSpawn = true;
            _spawnCoroutine = StartCoroutine(SpawnRoutine(spawnEnemyNameList, delayTime));
        }

        public void StopSpawn()
        {
            _canSpawn = false;

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnRoutine(List<string> spawnEnemyNameList, float delayTime)
        {
            yield return new WaitUntil(() => _enemyPool != null && _enemyPool.IsReady);
            while (_canSpawn)
            {
                foreach (var spawnPoint in _spawnPointList)
                {
                    int idx = Random.Range(0, spawnEnemyNameList.Count - 1);
                    SpawnEnemyByName(spawnEnemyNameList[idx], spawnPoint);
                    yield return new WaitForSeconds(delayTime);
                }
            }
        }

        private IEnumerator Start()
        {
            _enemyPool = Instantiate(_enemyPoolPrefab, transform);

            yield return new WaitUntil(() => _enemyPool.IsReady);

            Debug.Log("Init Enemy Pool Successfully");

            // SpawnEnemyRandom();
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