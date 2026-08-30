using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Currency;
using Item;
using Session.SO;
using Battle;
using System;
using Progression;
using Progression.RandomGrowth;

namespace Session
{
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Sessions")]
        public StageSession StageSession;

        public BattleSession BattleSession;

        public ProgressionSession ProgressionSession { get; private set; }

        [Header("Start Profile")]
        [SerializeField] private StartProfileSO startProfile;
        [SerializeField, Min(0)] private int startProfileApplyDelayFrame = 1;

        private bool startProfileApplied;

        [Header("Debug")]
        [SerializeField] private bool enableBattleSceneTest;

        [SerializeField] private KeyCode battleTestKey = KeyCode.F1;
        [SerializeField] private KeyCode returnStageTestKey = KeyCode.F2;
        [SerializeField] private KeyCode skillUpgradeTestKey = KeyCode.F3;

        [SerializeField] private string battleSceneName = "BattleScene";

        private void Awake()
        {
            if (Instance != null
                && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            Initialize();
            StartCoroutine(ApplyStartProfileDelayed());
        }

        private void Update()
        {
            if (!enableBattleSceneTest)
            {
                return;
            }

            if (Input.GetKeyDown(battleTestKey))
            {
                if (BattleSession == null)
                {
                    Debug.LogError(
                        "[GameSession] BattleSession is null.");

                    return;
                }

                BattleSession.BeginBattle(
                    "debug_battle",
                    battleSceneName,
                    SceneManager.GetActiveScene().name);

                return;
            }

            if (Input.GetKeyDown(returnStageTestKey))
            {
                if (BattleSession == null)
                {
                    Debug.LogError(
                        "[GameSession] BattleSession is null.");

                    return;
                }

                if (!BattleSession.IsBattleActive)
                {
                    Debug.LogWarning(
                        "[GameSession][Debug] Cannot complete battle. "
                        + "No battle is active.");
                    return;
                }

                if (BattleSession.BattleRuntime == null)
                {
                    Debug.LogError(
                        "[GameSession][Debug] Cannot complete battle. "
                        + "BattleRuntime is null.");
                    return;
                }

                BattleSession.BattleRuntime.isCompleted = true;
                Debug.Log(
                    "[GameSession][Debug] Battle marked complete "
                    + "before returning to StageScene. "
                    + $"battleId={BattleSession.BattleId}, "
                    + $"stageNodeId={BattleSession.PendingStageNodeId}.");
                BattleSession.EndBattle();
            }

            if (Input.GetKeyDown(skillUpgradeTestKey))
            {
                OpenSkillUpgradeWindowForDebug();
            }
        }

        private void OpenSkillUpgradeWindowForDebug()
        {
            BattleManager battleManager = BattleManager.Instance;
            if (battleManager == null)
            {
                Debug.LogWarning(
                    "[GameSession][Debug] Cannot open skill upgrade UI. "
                    + "BattleManager is not available.");
                return;
            }

            battleManager.OpenSkillUpgradeForDebug();
        }

        private IEnumerator ApplyStartProfileDelayed()
        {
            int delayFrame = Mathf.Max(0, startProfileApplyDelayFrame);

            for (int i = 0; i < delayFrame; i++)
            {
                yield return null;
            }

            ApplyStartProfileIfNeeded();
        }

        private void ApplyStartProfileIfNeeded()
        {
            if (startProfileApplied || startProfile == null)
            {
                return;
            }

            Initialize();

            StageSession.CurrencyRuntimeData ??= new CurrencyRutimeData();
            StageSession.RelicRuntimeData ??= new RelicRuntimeData();

            StageSession.CurrencyRuntimeData.gold =
                Mathf.Max(
                    0,
                    startProfile.StartGold);

            if (startProfile.StartRelics != null)
            {
                for (int i = 0; i < startProfile.StartRelics.Count; i++)
                {
                    RelicSO relic = startProfile.StartRelics[i];

                    if (relic == null)
                    {
                        continue;
                    }

                    if (ItemManager.Instance != null)
                    {
                        ItemManager.Instance.AddRelic(relic);
                    }
                }
            }

            bool partyInitialized =
                BattleSession.TryInitializePartyMembers(
                    startProfile.StartPartyMembers);

            // PartyManager.Start can run before the delayed start profile is
            // applied. Re-run the normal party initialization path so the
            // newly created members receive stats and owned skill instances.
            if (partyInitialized && Party.PartyManager.Instance != null)
            {
                Party.PartyManager.Instance.SpawnParty();
            }

            startProfileApplied = true;
        }

        private void Initialize()
        {
            StageSession ??= new StageSession();
            BattleSession ??= new BattleSession();
            ProgressionSession ??= new Progression.ProgressionSession();
        }

        public ProgressionRunId BeginNewProgressionRun()
        {
            return BeginNewProgressionRun(
                new GuidRandomGrowthSessionIdentityFactory());
        }

        public ProgressionRunId BeginNewProgressionRun(
            IRandomGrowthSessionIdentityFactory identityFactory)
        {
            Initialize();

            if (identityFactory == null)
            {
                throw new ArgumentNullException(nameof(identityFactory));
            }

            ProgressionRunId runId = identityFactory.CreateRunId();
            if (!runId.IsValid)
            {
                throw new InvalidOperationException(
                    "The progression identity factory returned an invalid run ID.");
            }

            ProgressionSession.ResetForNewRun(runId);
            StageSession.ResetRandomGrowthForNewRun(runId);
            StageSession.ConfigureSafeGrowthRuntime(ProgressionSession);
            return runId;
        }

        public bool TryBeginStageBattle(
            BattleSO battleSO,
            string stageNodeId,
            Action beforeSceneLoad = null)
        {
            Initialize();

            if (battleSO == null)
            {
                Debug.LogError(
                    "[GameSession] Cannot begin stage battle. "
                    + "BattleSO is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(stageNodeId))
            {
                Debug.LogError(
                    "[GameSession] Cannot begin stage battle. "
                    + "stageNodeId is empty.");
                return false;
            }

            if (BattleSession.TryGetCompletedStageNodeId(out string pendingId))
            {
                Debug.LogError(
                    "[GameSession] Cannot begin stage battle while "
                    + "a previous completion is pending. "
                    + $"pendingNodeId={pendingId}.");
                return false;
            }

            return BattleSession.BeginBattle(
                battleSO,
                battleSceneName,
                SceneManager.GetActiveScene().name,
                stageNodeId,
                beforeSceneLoad);
        }
    }
}
