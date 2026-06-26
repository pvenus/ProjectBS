using System;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Battle.Prop.SO;
using Wave.SO;

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
        [SerializeField]
        private string battleId;

        [SerializeField]
        private string battleName;

        [Header("Prefab")]
        [SerializeField]
        private GameObject backgroundPrefab;

        [SerializeField]
        private StageWaveSO waveSO;

        [Header("New Spawn System")]
        [SerializeField]
        private SpawnSequenceSO spawnSequence;

        [Header("Timed Props")]
        [SerializeField]
        private List<TimedPropPlacement> timedPropPlacements = new();

        [Header("Victory Rule")]
        [SerializeField]
        private BattleVictoryRule victoryRule;

        [SerializeField]
        [Min(0f)]
        private float survivalTimeSeconds;

        [Header("Reward")]
        [SerializeField]
        [Min(0f)]
        private float rewardExperience;

        [Header("Relic Drop")]
        [SerializeField]
        private RelicPoolSO relicDropPool;

        [SerializeField]
        [Range(0f, 100f)]
        private float normalRelicDropChance;

        [SerializeField]
        [Range(0f, 100f)]
        private float bossRelicDropChance;

        public string BattleId => battleId;

        public string BattleName => battleName;

        public GameObject BackgroundPrefab => backgroundPrefab;

        public StageWaveSO WaveSO => waveSO;

        public SpawnSequenceSO SpawnSequence => spawnSequence;

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