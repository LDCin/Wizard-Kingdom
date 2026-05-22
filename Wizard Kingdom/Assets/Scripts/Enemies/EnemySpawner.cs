using System.Collections;
using System.Collections.Generic;
using Managers;
using ObjectPool;
using SOs; // ADDED: dùng GameModeData
using UnityEngine;

namespace Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyPool _enemyPoolPrefab;
        [SerializeField] private List<GameObject> _spawnPointList = new();

        private EnemyPool _enemyPool;
        private Coroutine _spawnCoroutine;
        private bool _canSpawn;

        private GameModeData _currentModeData;
        private int _currentTierIndex = -1;
        private float _currentCommonDelay = 0f;
        private readonly Dictionary<string, Coroutine> _enemyCoroutines = new();
        private int _lastSpawnPointIndex = -1;

        public void StartSpawn(List<string> spawnEnemyNameList, float delayTime)
        {
            StopSpawn();

            _canSpawn = true;
            _spawnCoroutine = StartCoroutine(SpawnRoutine(spawnEnemyNameList, delayTime));
        }

        public void StartSpawn(GameModeData modeData)
        {
            StopSpawn();

            if (modeData == null || modeData.difficultyTiers == null || modeData.difficultyTiers.Count == 0)
            {
                Debug.LogWarning("EnemySpawner: GameModeData rỗng hoặc không có tier nào.");
                return;
            }

            _currentModeData = modeData;
            _currentTierIndex = -1;
            _canSpawn = true;

            StartCoroutine(StartSpawnByModeRoutine());
        }

        public void StopSpawn()
        {
            _canSpawn = false;

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            foreach (var co in _enemyCoroutines.Values)
            {
                if (co != null) StopCoroutine(co);
            }
            _enemyCoroutines.Clear();
            _currentModeData = null;
            _currentTierIndex = -1;
            _lastSpawnPointIndex = -1;
        }

        public void OnScoreChanged(int score)
        {
            if (!_canSpawn || _currentModeData == null) return;

            int newTier = 0;
            for (int i = 0; i < _currentModeData.difficultyTiers.Count; i++)
            {
                if (score >= _currentModeData.difficultyTiers[i].scoreThreshold)
                    newTier = i;
                else
                    break;
            }

            if (newTier > _currentTierIndex)
            {
                AdvanceToTier(newTier);
            }
        }

        private IEnumerator SpawnRoutine(List<string> spawnEnemyNameList, float delayTime)
        {
            yield return new WaitUntil(() => _enemyPool != null && _enemyPool.IsReady);
            while (_canSpawn)
            {
                foreach (var spawnPoint in _spawnPointList)
                {
                    int idx = Random.Range(0, spawnEnemyNameList.Count);
                    SpawnEnemyByName(spawnEnemyNameList[idx], spawnPoint);
                    yield return new WaitForSeconds(delayTime);
                }
            }
        }
        private IEnumerator StartSpawnByModeRoutine()
        {
            yield return new WaitUntil(() => _enemyPool != null && _enemyPool.IsReady);

            if (_canSpawn)
            {
                AdvanceToTier(0);
            }
        }
        private void AdvanceToTier(int tierIndex)
        {
            if (_currentModeData == null) return;
            if (tierIndex < 0 || tierIndex >= _currentModeData.difficultyTiers.Count) return;

            _currentTierIndex = tierIndex;
            _currentCommonDelay = _currentModeData.difficultyTiers[tierIndex].commonSpawnDelay;

            for (int i = 0; i <= tierIndex; i++)
            {
                var tier = _currentModeData.difficultyTiers[i];
                foreach (var entry in tier.enemies)
                {
                    if (entry.enemyData == null) continue;
                    string enemyName = entry.enemyData.enemyName;
                    if (string.IsNullOrEmpty(enemyName)) continue;

                    if (_enemyCoroutines.ContainsKey(enemyName)) continue;

                    Coroutine co = StartCoroutine(SpawnEnemyRoutine(entry));
                    _enemyCoroutines.Add(enemyName, co);
                }
            }
        }

        private IEnumerator SpawnEnemyRoutine(EnemySpawnEntry entry)
        {
            while (_canSpawn)
            {
                float delay = _currentCommonDelay + entry.extraSpawnDelay;
                yield return new WaitForSeconds(delay);

                if (!_canSpawn) yield break;

                if (_spawnPointList.Count == 0) continue;
                if (entry.enemyData == null) continue;

                int pointIndex = PickNextSpawnPointIndex();
                _lastSpawnPointIndex = pointIndex;

                GameObject spawnPoint = _spawnPointList[pointIndex];
                SpawnEnemyByName(entry.enemyData.enemyName, spawnPoint);
            }
        }

        private int PickNextSpawnPointIndex()
        {
            int count = _spawnPointList.Count;
            if (count <= 1 || _lastSpawnPointIndex < 0)
            {
                return Random.Range(0, count);
            }

            int idx = Random.Range(0, count - 1);
            if (idx >= _lastSpawnPointIndex) idx++;
            return idx;
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
