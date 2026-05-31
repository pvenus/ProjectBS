using UnityEngine;
using Item;

namespace Battle
{
    [CreateAssetMenu(
        fileName = "BattleSO",
        menuName = "Battle/BattleSO")]
    public class BattleSO : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField]
        private string battleId;

        [SerializeField]
        private string battleName;

        [Header("Prefab")]
        [SerializeField]
        private GameObject backgroundPrefab;

        [SerializeField]
        private GameObject monsterSpawnerPrefab;

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

        public GameObject MonsterSpawnerPrefab => monsterSpawnerPrefab;

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