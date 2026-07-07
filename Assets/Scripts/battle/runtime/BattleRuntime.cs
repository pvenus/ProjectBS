

using System;
using Item;
using UnityEngine;

namespace Battle
{
    [Serializable]
    public class BattleRuntime
    {
        public string battleId;
        public string battleName;

        public BattleVictoryRule victoryRule;
        public float survivalTimeSeconds;

        public float rewardExperience;

        public RelicPoolSO relicDropPool;
        public float normalRelicDropChance;
        public float bossRelicDropChance;

        public Sprite backgroundSprite;
        public GameObject monsterSpawnerPrefab;

        public bool bossKilled;
        public int remainingEnemyCount;

        public bool isCompleted;
        public float elapsedTime;
    }
}
