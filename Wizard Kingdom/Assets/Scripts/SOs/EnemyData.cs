using System.Collections.Generic;
using Balloons;
using Enemies;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy", order = 1)]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public Sprite sprite;
        public RuntimeAnimatorController runtimeAnimatorController;
        public EnemyType enemyType;

        [Header("Balloon")]
        public List<Symbol> possibleBalloonSymbols = new();
        public int balloonSpawnCount = 1;

        public int goldReward;
        public int scoreReward;
        public float moveSpeed;
    }
}