using System;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Battle.Prop.SO;

namespace Battle
{
    [CreateAssetMenu(
        fileName = "BattleSO",
        menuName = "Battle/BattleSO")]
    public class BattleSO : ScriptableObject
    {
        [Serializable]
        public class TimedPropPlacement
        {
            [Min(0f)]
            public float spawnTimeSeconds;

            public BattlePropSO prop;

            public Vector3 position;

            public Quaternion rotation = Quaternion.identity;

            public string runtimeId;
        }
        [Header("Basic Info")]
        public string battleId;

        public string battleName;

        [Header("Prefab")]
        public GameObject backgroundPrefab;

        [Header("New Spawn System")]
        public SpawnSequenceSO spawnSequence;
        public SpawnUnitBinding[] spawnUnitBindings;

        [Header("Timed Props")]
        public List<TimedPropPlacement> timedPropPlacements = new();

        [Header("Victory Rule")]
        public BattleVictoryRule victoryRule;

        [Min(0f)]
        public float survivalTimeSeconds;

        [Header("Reward")]
        [Min(0f)]
        public float rewardExperience;

        [Header("Relic Drop")]
        public RelicPoolSO relicDropPool;

        [Range(0f, 100f)]
        public float normalRelicDropChance;

        [Range(0f, 100f)]
        public float bossRelicDropChance;

        public string BattleId => battleId;

        public string BattleName => battleName;

        public GameObject BackgroundPrefab => backgroundPrefab;

        public SpawnSequenceSO SpawnSequence => spawnSequence;

        public IReadOnlyList<SpawnUnitBinding> SpawnUnitBindings => spawnUnitBindings;

        public IReadOnlyList<TimedPropPlacement> TimedPropPlacements => timedPropPlacements;

        public BattleVictoryRule VictoryRule => victoryRule;

        public float SurvivalTimeSeconds => survivalTimeSeconds;

        public float RewardExperience => rewardExperience;

        public RelicPoolSO RelicDropPool => relicDropPool;

        public float NormalRelicDropChance => normalRelicDropChance;

        public float BossRelicDropChance => bossRelicDropChance;
    }

    public enum BattleVictoryRule
    {
        KillBoss,
        ClearAllEnemies,
        SurviveTime
    }
}
