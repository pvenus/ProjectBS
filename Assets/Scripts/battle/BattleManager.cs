using Session;
using UnityEngine;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        private BattleSession battleSession;
        private bool isInitialPrefabSpawned;

        public BattleSession BattleSession => battleSession;

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateVictoryRule();
        }

        private void OnDestroy()
        {
            UnsubscribeBattleSpawnManager();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Initialize()
        {
            GameSession gameSession =
                GameSession.Instance;

            if (gameSession == null)
            {
                Debug.LogError(
                    "[BattleManager] GameSession not found.");

                return;
            }

            battleSession =
                gameSession.BattleSession;

            if (battleSession == null)
            {
                Debug.LogError(
                    "[BattleManager] BattleSession not found.");

                return;
            }

            EnsureBattleRuntime();
            SpawnInitialPrefabs();
        }

        private void EnsureBattleRuntime()
        {
            if (battleSession.BattleSO == null)
            {
                Debug.LogError(
                    "[BattleManager] BattleSO not found.");

                return;
            }

            battleSession.BattleRuntime =
                CreateBattleRuntime(
                    battleSession.BattleSO);
        }

        private BattleRuntime CreateBattleRuntime(
            BattleSO battleSO)
        {
            return new BattleRuntime
            {
                battleId = battleSO.BattleId,
                battleName = battleSO.BattleName,
                victoryRule = battleSO.VictoryRule,
                survivalTimeSeconds = battleSO.SurvivalTimeSeconds,
                rewardExperience = battleSO.RewardExperience,
                relicDropPool = battleSO.RelicDropPool,
                normalRelicDropChance = battleSO.NormalRelicDropChance,
                bossRelicDropChance = battleSO.BossRelicDropChance,
                backgroundPrefab = battleSO.BackgroundPrefab,
                // monsterSpawnerPrefab assignment removed
                bossKilled = false,
                remainingEnemyCount = 0,
                isCompleted = false,
                elapsedTime = 0f
            };
        }
        private void SpawnInitialPrefabs()
        {
            if (isInitialPrefabSpawned)
            {
                return;
            }

            BattleRuntime battleRuntime =
                battleSession.BattleRuntime;

            if (battleRuntime == null)
            {
                Debug.LogError(
                    "[BattleManager] BattleRuntime not found.");

                return;
            }

            SpawnPrefab(
                battleRuntime.backgroundPrefab,
                "Background");

            StartBattleSpawnSequence();

            isInitialPrefabSpawned = true;
        }

        private void StartBattleSpawnSequence()
        {
            if (battleSession == null ||
                battleSession.BattleSO == null)
            {
                Debug.LogError("[BattleManager] BattleSO not found.");
                return;
            }

            SpawnSequenceSO spawnSequence =
                battleSession.BattleSO.SpawnSequence;

            if (spawnSequence == null)
            {
                Debug.LogError(
                    "[BattleManager] SpawnSequence not assigned. BattleSO must use the new spawn system.");
                return;
            }

            BattleSpawnManager spawnManager =
                EnsureBattleSpawnManager();

            if (spawnManager == null)
            {
                Debug.LogError("[BattleManager] BattleSpawnManager not found.");
                return;
            }

            UnsubscribeBattleSpawnManager();
            spawnManager.OnSequenceFinished += HandleSpawnSequenceFinished;
            spawnManager.PlaySequence(spawnSequence);
        }

        private BattleSpawnManager EnsureBattleSpawnManager()
        {
            if (BattleSpawnManager.Instance != null)
            {
                return BattleSpawnManager.Instance;
            }

            GameObject spawnManagerObject =
                new GameObject("BattleSpawnManager");
            spawnManagerObject.transform.SetParent(transform, false);

            return spawnManagerObject.AddComponent<BattleSpawnManager>();
        }

        private GameObject SpawnPrefab(
            GameObject prefab,
            string objectName)
        {
            if (prefab == null)
            {
                return null;
            }

            GameObject spawnedObject =
                Instantiate(
                    prefab,
                    transform);

            spawnedObject.name =
                objectName;

            return spawnedObject;
        }

        private void UpdateVictoryRule()
        {
            if (battleSession == null
                || battleSession.BattleRuntime == null)
            {
                return;
            }

            BattleRuntime battleRuntime =
                battleSession.BattleRuntime;

            battleRuntime.elapsedTime += Time.deltaTime;

            switch (battleRuntime.victoryRule)
            {
                case BattleVictoryRule.KillBoss:
                    CheckBossKillVictory();
                    break;

                case BattleVictoryRule.ClearAllEnemies:
                    CheckClearAllEnemiesVictory();
                    break;

                case BattleVictoryRule.SurviveTime:
                    CheckSurviveTimeVictory();
                    break;
            }
        }

        private void CheckBossKillVictory()
        {
            if (battleSession.BattleRuntime.isCompleted)
            {
                return;
            }

            if (battleSession.BattleRuntime.bossKilled)
            {
                CompleteBattle();
            }
        }

        private void CheckClearAllEnemiesVictory()
        {
            if (battleSession.BattleRuntime.isCompleted)
            {
                return;
            }

            if (battleSession.BattleSO != null &&
                battleSession.BattleSO.SpawnSequence != null)
            {
                return;
            }

            if (battleSession.BattleRuntime.remainingEnemyCount <= 0)
            {
                CompleteBattle();
            }
        }

        private void HandleSpawnSequenceFinished()
        {
            UnsubscribeBattleSpawnManager();

            if (battleSession == null ||
                battleSession.BattleRuntime == null)
            {
                return;
            }

            if (battleSession.BattleRuntime.victoryRule == BattleVictoryRule.ClearAllEnemies)
            {
                CompleteBattle();
            }
        }

        private void UnsubscribeBattleSpawnManager()
        {
            if (BattleSpawnManager.Instance != null)
            {
                BattleSpawnManager.Instance.OnSequenceFinished -= HandleSpawnSequenceFinished;
            }
        }

        private void CheckSurviveTimeVictory()
        {
            BattleRuntime runtime =
                battleSession.BattleRuntime;

            if (runtime.isCompleted)
            {
                return;
            }

            if (runtime.elapsedTime >= runtime.survivalTimeSeconds)
            {
                CompleteBattle();
            }
        }

        private void CompleteBattle()
        {
            if (battleSession == null
                || battleSession.BattleRuntime == null)
            {
                return;
            }

            if (battleSession.BattleRuntime.isCompleted)
            {
                return;
            }

            battleSession.BattleRuntime.isCompleted = true;

            EndBattle();
        }

        public void EndBattle()
        {
            if (battleSession == null)
            {
                Debug.LogError(
                    "[BattleManager] BattleSession is null.");

                return;
            }

            battleSession.EndBattle();
        }
    }
}
