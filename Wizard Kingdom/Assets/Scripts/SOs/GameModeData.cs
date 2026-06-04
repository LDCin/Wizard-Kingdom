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
        public string id;
        public GameModeType modeType = GameModeType.Arcade;
        public string modeName;
        public string fixedBackgroundId;
        public List<DifficultyTier> difficultyTiers = new();
        public bool hasTime;
        [Min(0f)] public float playTime = 60f;

        #region Analysis And Design Methods

        public GameModeData Get()
        {
            difficultyTiers ??= new List<DifficultyTier>();
            foreach (DifficultyTier tier in difficultyTiers)
            {
                tier?.Get();
            }

            return this;
        }

        public GameModeData Set()
        {
            difficultyTiers ??= new List<DifficultyTier>();
            return this;
        }

        public GameModeData get() => Get();
        public GameModeData set() => Set();

        #endregion
    }

    [System.Serializable]
    public class DifficultyTier
    {
        [Min(0)] public int scoreThreshold;
        [Min(0f)] public float commonSpawnDelay = 1f;
        public List<EnemySpawnEntry> enemies = new();

        #region Analysis And Design Methods

        public DifficultyTier Get()
        {
            enemies ??= new List<EnemySpawnEntry>();
            foreach (EnemySpawnEntry entry in enemies)
            {
                entry?.Get();
            }

            return this;
        }

        public DifficultyTier Set()
        {
            enemies ??= new List<EnemySpawnEntry>();
            return this;
        }

        public DifficultyTier get() => Get();
        public DifficultyTier set() => Set();

        #endregion
    }

    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyData enemyData;
        [Min(0f)] public float extraSpawnDelay = 0f;

        #region Analysis And Design Methods

        public EnemySpawnEntry Get()
        {
            enemyData?.Get();
            return this;
        }

        public EnemySpawnEntry Set() => this;
        public EnemySpawnEntry get() => Get();
        public EnemySpawnEntry set() => Set();

        #endregion
    }
}
