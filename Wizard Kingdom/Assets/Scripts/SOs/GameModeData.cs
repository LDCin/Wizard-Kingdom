using System.Collections.Generic;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "New Game Mode Data", menuName = "Game/Game Mode Data")]
    public class GameModeData : ScriptableObject
    {
        public string modeName;

        [Header("Visual")]
        public GameObject theme;

        [Header("Base Difficulty")]
        public float baseSpawnInterval = 2f;
        public List<EnemySpawnOption> baseEnemies = new();

        [Header("Difficulty Milestones")]
        public List<DifficultyMilestone> difficultyMilestones = new();
    }

    [System.Serializable]
    public class DifficultyMilestone
    {
        public int requiredScore;

        [Tooltip("Thời gian giữa các lần spawn sau khi đạt mốc này")]
        public float spawnInterval = 1.5f;

        [Tooltip("Enemy được mở thêm ở mốc này")]
        public List<EnemySpawnOption> additionalEnemies = new();
    }

    [System.Serializable]
    public class EnemySpawnOption
    {
        public EnemyData enemyData;

        [Min(1)]
        public int spawnRatio = 1;

        [Tooltip("Sau khi enemy này được spawn, phải chờ bao nhiêu giây mới được spawn lại")]
        public float spawnCooldown = 0f;
    }
}