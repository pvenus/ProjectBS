using Session;
using Party;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle
{
    public class BattleManager : MonoBehaviour
    {
        private enum SkillUpgradeOpenResult
        {
            Opened,
            AlreadyOpen,
            NoCandidates,
            MissingRequiredReference
        }

        public static BattleManager Instance { get; private set; }

        private BattleSession battleSession;
        private bool isInitialPrefabSpawned;
        private bool isSpawnSequenceFinished;
        private bool isWaitingForBattleEndUpgrade;
        private bool isDebugSkillUpgradeOpen;
        private bool shouldEndBattleAfterCurrentUpgrade;
        private BattleEndSkillUpgradePresenter battleEndSkillUpgradePresenter;

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
            battleEndSkillUpgradePresenter?.Dispose();

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
            SpawnSequenceSO spawnSequence = battleSO.SpawnSequence;
            Sprite backgroundSprite = battleSO.BackgroundSprite;

            Debug.Log(
                "[BattleManager] Initializing battle. "
                + $"battleId={battleSO.BattleId}, "
                + $"spawnSequence={(spawnSequence != null ? spawnSequence.SequenceId : "null")}, "
                + $"spawnUnitBindings={battleSO.SpawnUnitBindings?.Count ?? 0}, "
                + $"backgroundSprite={(backgroundSprite != null ? backgroundSprite.name : "null")}");

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
                backgroundSprite = backgroundSprite,
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

            SpawnBackground(
                battleRuntime);

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
            isSpawnSequenceFinished = false;
            spawnManager.OnSequenceFinished += HandleSpawnSequenceFinished;
            spawnManager.PlaySequence(
                spawnSequence,
                new SpawnUnitBindingResolver(battleSession.BattleSO.SpawnUnitBindings));
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

        private GameObject SpawnBackground(
            BattleRuntime battleRuntime)
        {
            if (battleRuntime == null)
            {
                return null;
            }

            if (battleRuntime.backgroundSprite != null)
            {
                GameObject backgroundObject =
                    new GameObject("Background");
                backgroundObject.transform.SetParent(transform, false);

                SpriteRenderer renderer =
                    backgroundObject.AddComponent<SpriteRenderer>();
                renderer.sprite = battleRuntime.backgroundSprite;
                renderer.sortingOrder = -1000;

                FitBackgroundToCamera(backgroundObject.transform, renderer);

                return backgroundObject;
            }

            return null;
        }

        private static void FitBackgroundToCamera(
            Transform backgroundTransform,
            SpriteRenderer renderer)
        {
            Camera camera = Camera.main;

            if (backgroundTransform == null ||
                renderer == null ||
                renderer.sprite == null ||
                camera == null ||
                !camera.orthographic)
            {
                return;
            }

            Vector2 spriteSize = renderer.sprite.bounds.size;
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return;
            }

            float cameraHeight = camera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * camera.aspect;
            float scale = Mathf.Max(
                cameraWidth / spriteSize.x,
                cameraHeight / spriteSize.y);

            backgroundTransform.localScale = new Vector3(scale, scale, 1f);

            Vector3 scaledCenter = renderer.sprite.bounds.center * scale;
            Vector3 cameraPosition = camera.transform.position;
            backgroundTransform.position = new Vector3(
                cameraPosition.x - scaledCenter.x,
                cameraPosition.y - scaledCenter.y,
                0f);
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
                if (!isSpawnSequenceFinished)
                {
                    return;
                }

                if (EnemyRegistry.Instance.ActiveEnemies.Count <= 0)
                {
                    CompleteBattle();
                }

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

            isSpawnSequenceFinished = true;
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

            OpenBattleEndUpgradeOrEndBattle();
        }

        private void OpenBattleEndUpgradeOrEndBattle()
        {
            if (isWaitingForBattleEndUpgrade)
            {
                if (isDebugSkillUpgradeOpen)
                {
                    shouldEndBattleAfterCurrentUpgrade = true;
                    Debug.Log(
                        "[BattleManager] Battle completed while the debug skill upgrade UI "
                        + "was open. Battle will end after the current selection.",
                        this);
                }

                return;
            }

            SkillUpgradeOpenResult result =
                TryOpenSkillUpgrade(HandleBattleEndUpgradeCompleted);

            switch (result)
            {
                case SkillUpgradeOpenResult.Opened:
                case SkillUpgradeOpenResult.AlreadyOpen:
                    return;
                case SkillUpgradeOpenResult.NoCandidates:
                    Debug.LogWarning(
                        "[BattleManager] No skill upgrade candidates were available. "
                        + "Ending battle without an upgrade.",
                        this);
                    EndBattle();
                    return;
                default:
                    Debug.LogWarning(
                        "[BattleManager] Skill upgrade UI was unavailable. "
                        + "Ending battle without an upgrade.",
                        this);
                    EndBattle();
                    return;
            }
        }

        private SkillUpgradeOpenResult TryOpenSkillUpgrade(
            System.Action completionCallback)
        {
            if (isWaitingForBattleEndUpgrade)
            {
                return SkillUpgradeOpenResult.AlreadyOpen;
            }

            if (TryOpenBattleEndUpgradeView(completionCallback))
            {
                return SkillUpgradeOpenResult.Opened;
            }

            UIEquipmentUpgradeMono upgradeUI =
                ResolveOrInstantiateBattleEndUpgradeUi();
            if (upgradeUI == null)
            {
                return SkillUpgradeOpenResult.MissingRequiredReference;
            }

            if (upgradeUI.IsOpen)
            {
                return SkillUpgradeOpenResult.AlreadyOpen;
            }

            bool completedBeforeOpenReturned = false;
            bool openCallReturned = false;
            System.Action guardedCompletion = () =>
            {
                if (!openCallReturned)
                {
                    completedBeforeOpenReturned = true;
                    return;
                }

                completionCallback?.Invoke();
            };

            isWaitingForBattleEndUpgrade = true;
            bool opened = upgradeUI.OpenWithCompletion(guardedCompletion);
            openCallReturned = true;
            if (!opened)
            {
                isWaitingForBattleEndUpgrade = false;

                return completedBeforeOpenReturned
                    ? SkillUpgradeOpenResult.NoCandidates
                    : SkillUpgradeOpenResult.MissingRequiredReference;
            }

            return SkillUpgradeOpenResult.Opened;
        }

        private bool TryOpenBattleEndUpgradeView(
            System.Action completionCallback)
        {
            PartyManager partyManager = PartyManager.Instance;
            if (partyManager == null
                || partyManager.Members == null
                || partyManager.Members.Count == 0)
            {
                return false;
            }

            battleEndSkillUpgradePresenter ??=
                new BattleEndSkillUpgradePresenter();

            isWaitingForBattleEndUpgrade = true;
            bool opened =
                battleEndSkillUpgradePresenter.Open(
                    partyManager.Members,
                    completionCallback);

            if (!opened)
            {
                isWaitingForBattleEndUpgrade = false;
            }

            return opened;
        }

        private UIEquipmentUpgradeMono ResolveOrInstantiateBattleEndUpgradeUi()
        {
            UIEquipmentUpgradeMono upgradeUI =
                FindObjectOfType<UIEquipmentUpgradeMono>(true);
            if (upgradeUI != null)
            {
                return upgradeUI;
            }

            UIEquipmentUpgradeMono upgradePrefab =
                Resources.Load<UIEquipmentUpgradeMono>("skill/Upgrade UI");

            return upgradePrefab != null
                ? InstantiateBattleEndUpgradeUi(upgradePrefab)
                : null;
        }

        private UIEquipmentUpgradeMono InstantiateBattleEndUpgradeUi(
            UIEquipmentUpgradeMono upgradePrefab)
        {
            Transform uiRoot = FindActiveSceneCanvasRoot();

            if (uiRoot == null)
            {
                Debug.LogWarning(
                    "[BattleManager] No active Canvas was found in the active scene. "
                    + "The skill upgrade UI will be instantiated without a parent.",
                    this);

                return Instantiate(upgradePrefab);
            }

            UIEquipmentUpgradeMono upgradeUI =
                Instantiate(upgradePrefab, uiRoot, false);
            upgradeUI.transform.SetAsLastSibling();

            Debug.Log(
                $"[BattleManager] Skill upgrade UI instantiated under Canvas '{uiRoot.name}'.",
                upgradeUI);

            return upgradeUI;
        }

        private Transform FindActiveSceneCanvasRoot()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            Canvas[] canvases =
                FindObjectsByType<Canvas>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            Canvas firstActiveCanvas = null;

            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null
                    || !canvas.isActiveAndEnabled
                    || canvas.gameObject.scene != activeScene)
                {
                    continue;
                }

                firstActiveCanvas ??= canvas;

                if (canvas.rootCanvas == canvas)
                {
                    return canvas.transform;
                }
            }

            return firstActiveCanvas != null
                ? firstActiveCanvas.transform
                : null;
        }

        private void HandleBattleEndUpgradeCompleted()
        {
            isWaitingForBattleEndUpgrade = false;
            isDebugSkillUpgradeOpen = false;
            shouldEndBattleAfterCurrentUpgrade = false;
            EndBattle();
        }

        public void OpenSkillUpgradeForDebug()
        {
            SkillUpgradeOpenResult result =
                TryOpenSkillUpgrade(HandleDebugSkillUpgradeCompleted);

            isDebugSkillUpgradeOpen =
                result == SkillUpgradeOpenResult.Opened;

            switch (result)
            {
                case SkillUpgradeOpenResult.Opened:
                    Debug.Log(
                        "[BattleManager][Debug] Skill upgrade UI opened. "
                        + "Battle will continue after a selection.",
                        this);
                    break;
                case SkillUpgradeOpenResult.AlreadyOpen:
                    Debug.LogWarning(
                        "[BattleManager][Debug] Skill upgrade UI is already open.",
                        this);
                    break;
                case SkillUpgradeOpenResult.NoCandidates:
                    Debug.LogWarning(
                        "[BattleManager][Debug] No skill upgrade candidates were available. "
                        + "Battle continues.",
                        this);
                    break;
                default:
                    Debug.LogWarning(
                        "[BattleManager][Debug] Skill upgrade UI could not be opened. "
                        + "Required popup or Resources references are unavailable. "
                        + "Battle continues.",
                        this);
                    break;
            }
        }

        private void HandleDebugSkillUpgradeCompleted()
        {
            isWaitingForBattleEndUpgrade = false;
            isDebugSkillUpgradeOpen = false;

            if (shouldEndBattleAfterCurrentUpgrade)
            {
                shouldEndBattleAfterCurrentUpgrade = false;
                Debug.Log(
                    "[BattleManager][Debug] Skill upgrade selected after battle completion. "
                    + "Ending battle through the normal completion path.",
                    this);
                EndBattle();
                return;
            }

            Debug.Log(
                "[BattleManager][Debug] Skill upgrade selected. Battle continues.",
                this);
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
