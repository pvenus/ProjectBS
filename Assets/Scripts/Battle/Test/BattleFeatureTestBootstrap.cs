using System.Collections.Generic;
using Battle.UI.Testing;
using Character;
using Item;
using Party;
using Session;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Battle.Testing
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class BattleFeatureTestBootstrap : MonoBehaviour
    {
        private const int MaxPartyMemberCount = 4;
        private const int MaxStrategicSkillCount = 4;

        [Header("Test Definitions")]
        [SerializeField] private BattleSO battle;
        [SerializeField] private List<CharacterSO> partyMembers = new();
        [SerializeField] private List<StrategicSkillItemSO> strategicSkills = new();

        [Header("Required Scene References")]
        [SerializeField] private GameSession gameSession;
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private ItemManager itemManager;
        [SerializeField] private StrategicSkillCostManager strategicSkillCostManager;

        [Header("Optional UI Test Reference")]
        [SerializeField] private BattleUiDataSetupTester battleUiDataSetupTester;

        [Header("Startup")]
        [SerializeField] private bool prepareOnAwake = true;
        [SerializeField] private string returnSceneName = "StageScene";
        [Tooltip("Explicit Inspector references take priority. When enabled, only missing scene references are searched in the loaded scene.")]
        [SerializeField] private bool findMissingReferencesAutomatically;

        private void Awake()
        {
            if (prepareOnAwake)
            {
                PrepareTestBattle();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TrimList(partyMembers, MaxPartyMemberCount, nameof(partyMembers));
            TrimList(
                strategicSkills,
                MaxStrategicSkillCount,
                nameof(strategicSkills));
        }
#endif

        [ContextMenu("Prepare Test Battle")]
        public void PrepareTestBattle()
        {
            ResolveMissingReferencesIfEnabled();

            if (!ValidateConfiguration())
            {
                return;
            }

            if (!PrepareSession())
            {
                return;
            }

            PrepareParty();
            PrepareStrategicSkills();
            ApplyConfiguredUiData();

            Debug.Log(
                "[BattleFeatureTestBootstrap] Direct battle state prepared. "
                + "BattleManager and PartyManager will initialize from the session during Start.",
                this);
        }

        private bool PrepareSession()
        {
            BattleSession battleSession = gameSession.BattleSession;
            if (battleSession == null)
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] GameSession.BattleSession is null.",
                    this);
                return false;
            }

            string activeSceneName = SceneManager.GetActiveScene().name;
            bool prepared = battleSession.TryPrepareDirectBattle(
                battle,
                activeSceneName,
                returnSceneName);

            if (!prepared)
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] Direct BattleSession preparation failed.",
                    this);
            }

            return prepared;
        }

        private void PrepareParty()
        {
            BattleSession battleSession = gameSession.BattleSession;
            battleSession.PartyRuntimeData ??= new PartyRuntimeData();
            battleSession.PartyRuntimeData.Clear();

            int addedCount = 0;
            int configuredPartyMemberCount = partyMembers?.Count ?? 0;
            for (int i = 0; i < configuredPartyMemberCount; i++)
            {
                CharacterSO character = partyMembers[i];
                if (character == null)
                {
                    Debug.LogWarning(
                        $"[BattleFeatureTestBootstrap] Party member entry {i} is null and was skipped.",
                        this);
                    continue;
                }

                battleSession.PartyRuntimeData.AddMember(
                    new CharacterRuntimeData
                    {
                        characterSO = character
                    });
                addedCount++;
            }

            Debug.Log(
                $"[BattleFeatureTestBootstrap] Party runtime prepared. members={addedCount}.",
                this);
        }

        private void PrepareStrategicSkills()
        {
            StageSession stageSession = gameSession.StageSession;
            if (stageSession == null)
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] GameSession.StageSession is null.",
                    this);
                return;
            }

            stageSession.Initialize(stageSession.RuntimeData);
            stageSession.StrategicSkillItemRuntimeData.Clear();

            strategicSkillCostManager.ResetGauge();

            int addedCount = 0;
            int configuredStrategicSkillCount = strategicSkills?.Count ?? 0;
            for (int i = 0; i < configuredStrategicSkillCount; i++)
            {
                StrategicSkillItemSO strategicSkill = strategicSkills[i];
                if (strategicSkill == null)
                {
                    Debug.LogWarning(
                        $"[BattleFeatureTestBootstrap] Strategic skill entry {i} is null and was skipped.",
                        this);
                    continue;
                }

                stageSession.StrategicSkillItemRuntimeData
                    .AddStrategicSkillItem(strategicSkill);
                itemManager.AddStrategicSkillItem(strategicSkill);
                addedCount++;
            }

            Debug.Log(
                "[BattleFeatureTestBootstrap] Strategic skill runtime prepared. "
                + $"items={addedCount}, gauge={strategicSkillCostManager.CurrentGauge}/"
                + $"{strategicSkillCostManager.MaxGauge}.",
                this);
        }

        private void ApplyConfiguredUiData()
        {
            if (battleUiDataSetupTester == null)
            {
                Debug.Log(
                    "[BattleFeatureTestBootstrap] Optional BattleUiDataSetupTester is not assigned. "
                    + "PartyBoard and StrategicBoard test data were not applied.",
                    this);
                return;
            }

            battleUiDataSetupTester.Configure(
                partyMembers,
                strategicSkills,
                strategicSkillCostManager.CurrentGauge,
                strategicSkillCostManager.MaxGauge,
                0f);
            battleUiDataSetupTester.ApplyConfiguredData();
        }

        private bool ValidateConfiguration()
        {
            bool isValid = true;

            if (battle == null)
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] BattleSO is required.",
                    this);
                isValid = false;
            }

            if (battle != null && battle.SpawnSequence == null)
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] BattleSO.SpawnSequence is required by BattleManager.",
                    battle);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(returnSceneName))
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] Return scene name is required.",
                    this);
                isValid = false;
            }

            int validPartyMemberCount = 0;
            int configuredPartyMemberCount = partyMembers?.Count ?? 0;
            for (int i = 0; i < configuredPartyMemberCount; i++)
            {
                if (partyMembers[i] != null)
                {
                    validPartyMemberCount++;
                }
            }

            if (validPartyMemberCount == 0)
            {
                Debug.LogError(
                    "[BattleFeatureTestBootstrap] At least one CharacterSO is required.",
                    this);
                isValid = false;
            }

            isValid &= ValidateRequiredReference(gameSession, nameof(gameSession));
            isValid &= ValidateRequiredReference(battleManager, nameof(battleManager));
            isValid &= ValidateRequiredReference(partyManager, nameof(partyManager));
            isValid &= ValidateRequiredReference(itemManager, nameof(itemManager));
            isValid &= ValidateRequiredReference(
                strategicSkillCostManager,
                nameof(strategicSkillCostManager));

            return isValid;
        }

        private bool ValidateRequiredReference(
            Object reference,
            string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            Debug.LogError(
                $"[BattleFeatureTestBootstrap] Required scene reference '{fieldName}' is missing.",
                this);
            return false;
        }

        private void ResolveMissingReferencesIfEnabled()
        {
            if (!findMissingReferencesAutomatically)
            {
                return;
            }

            gameSession ??= FindLoadedSceneComponent<GameSession>();
            battleManager ??= FindLoadedSceneComponent<BattleManager>();
            partyManager ??= FindLoadedSceneComponent<PartyManager>();
            itemManager ??= FindLoadedSceneComponent<ItemManager>();
            strategicSkillCostManager ??=
                FindLoadedSceneComponent<StrategicSkillCostManager>();
            battleUiDataSetupTester ??=
                FindLoadedSceneComponent<BattleUiDataSetupTester>();
        }

        private static T FindLoadedSceneComponent<T>() where T : Component
        {
#if UNITY_2023_1_OR_NEWER
            T[] candidates = Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
#else
            T[] candidates = Object.FindObjectsOfType<T>(true);
#endif
            for (int i = 0; i < candidates.Length; i++)
            {
                T candidate = candidates[i];
                if (candidate != null && candidate.gameObject.scene.IsValid())
                {
                    return candidate;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void TrimList<T>(List<T> list, int maxCount, string fieldName)
        {
            if (list == null || list.Count <= maxCount)
            {
                return;
            }

            list.RemoveRange(maxCount, list.Count - maxCount);
            Debug.LogWarning(
                $"[BattleFeatureTestBootstrap] Inspector field '{fieldName}' was trimmed to {maxCount} entries.",
                this);
        }
#endif
    }
}
