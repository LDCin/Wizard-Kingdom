using System.Collections.Generic;
using Balloons;
using Enemies;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy", order = 1)]
    public class EnemyData : ScriptableObject
    {
        public string id;
        public string enemyName;
        public Sprite sprite;
        public RuntimeAnimatorController runtimeAnimatorController;
        public EnemyType enemyType;

        [Header("Balloon")]
        public List<Symbol> possibleBalloonSymbols = new();
        public List<BalloonData> balloonDataList = new();
        public int balloonSpawnCount = 1;

        public int goldReward;
        public int scoreReward;
        public float moveSpeed;

        #region Analysis And Design Methods

        public EnemyData Get()
        {
            possibleBalloonSymbols ??= new List<Symbol>();
            balloonDataList ??= new List<BalloonData>();
            return this;
        }

        public EnemyData Set()
        {
            possibleBalloonSymbols ??= new List<Symbol>();
            balloonDataList ??= new List<BalloonData>();
            return this;
        }

        public Enemy Init(Enemy enemy, List<Balloon> balloons)
        {
            if (enemy == null) return null;
            enemy.Init(this, balloons);
            return enemy;
        }

        public bool CheckSymbol(Enemy enemy, string symbol)
        {
            if (enemy == null) return false;
            enemy.CheckSymbol(symbol);
            return true;
        }

        public EnemyData Destroy(Enemy enemy = null)
        {
            enemy?.Destroy();
            return this;
        }

        public EnemyData InvadeCastle(Enemy enemy = null)
        {
            enemy?.InvadeCastle();
            return this;
        }

        public EnemyData get() => Get();
        public EnemyData set() => Set();
        public Enemy init(Enemy enemy, List<Balloon> balloons) => Init(enemy, balloons);
        public bool checkSymbol(Enemy enemy, string symbol) => CheckSymbol(enemy, symbol);
        public EnemyData destroy(Enemy enemy = null) => Destroy(enemy);
        public EnemyData invadeCastle(Enemy enemy = null) => InvadeCastle(enemy);

        #endregion
    }
}
