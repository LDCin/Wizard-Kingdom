using System.Collections.Generic;
using UnityEngine;

namespace SOs
{
    public enum GameModeType
    {
        Arcade,
        TimeAttack
    }

    [CreateAssetMenu(fileName = "New Game Mode Data", menuName = "Game/Game Mode Data")]
    public class GameModeData : ScriptableObject
    {
        public GameModeType modeType = GameModeType.Arcade;
        public string modeName;
        public string fixedBackgroundId;
        public List<DifficultyTier> difficultyTiers = new();
        public bool hasTime;
        [Min(0f)] public float playTime = 60f;
    }

    [System.Serializable]
    public class DifficultyTier
    {
        [Min(0)] public int scoreThreshold;
        [Min(0f)] public float commonSpawnDelay = 1f;
        public List<EnemySpawnEntry> enemies = new();
    }

    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyData enemyData;
        [Min(0f)] public float extraSpawnDelay = 0f;
    }
}
