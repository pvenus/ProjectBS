using System;
using System.Collections.Generic;
using Battle.UI.PartyHud;
using Battle.UI.StrategicBoard;
using Character;
using Item;
using Presentation;
using Skill;
using Stat;
using UnityEngine;

namespace Battle.UI.Testing
{
    [DisallowMultipleComponent]
    public sealed class BattleUiDataSetupTester : MonoBehaviour
    {
        private const int MaxConfiguredPartyMembers = PartyHudView.MaxPartyMemberCount;
        private const int DefaultStrategicSlotLimit = 4;
        private const int MaxActiveSkillSlots = 4;
        private const string BasicAttackSlotKey = "basic_attack";
        private const string ActiveSlotPrefix = "active_";
        private const string PassiveSlotPrefix = "passive_";
        private const string TestStatusText = "UI TEST";

        [Header("Configured Test Data")]
        [Tooltip("Up to four CharacterSO assets. Null entries are skipped with a warning.")]
        [SerializeField] private List<CharacterSO> characters = new();

        [Tooltip("StrategicSkillItemSO assets are applied in list order, up to the board slot count.")]
        [SerializeField] private List<StrategicSkillItemSO> strategicSkills = new();

        [Header("Strategic Gauge")]
        [Min(0)]
        [SerializeField] private int currentGauge;
        [Min(0)]
        [SerializeField] private int maxGauge = 100;
        [SerializeField] private float chargePerSecond;

        [Header("Optional Explicit View References")]
        [SerializeField] private PartyHudView partyHudView;
        [SerializeField] private Transform partyBoardRoot;
        [SerializeField] private StrategicBoardView strategicBoardView;

        [Header("Safe Auto Discovery")]
        [Tooltip("When an explicit reference is absent, search only loaded scene objects.")]
        [SerializeField] private bool findViewsAutomatically = true;

        [Header("Phase 2 Placeholder")]
        [Tooltip("Default false. Start invokes only the Phase 2 placeholder; it does not inject battle data.")]
        [SerializeField] private bool injectOnStart;

        private readonly CharacterPresentationResolver characterPresentationResolver = new();

        private void Start()
        {
            if (injectOnStart)
            {
                InjectConfiguredDataAtBattleEntry();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            TrimList(characters, MaxConfiguredPartyMembers, nameof(characters));

            int strategicLimit = strategicBoardView != null && strategicBoardView.Slots != null
                ? strategicBoardView.Slots.Count
                : DefaultStrategicSlotLimit;
            TrimList(strategicSkills, strategicLimit, nameof(strategicSkills));

            currentGauge = Mathf.Max(0, currentGauge);
            maxGauge = Mathf.Max(0, maxGauge);
        }
#endif

        public void Configure(
            IReadOnlyList<CharacterSO> configuredCharacters,
            IReadOnlyList<StrategicSkillItemSO> configuredStrategicSkills,
            int configuredCurrentGauge,
            int configuredMaxGauge,
            float configuredChargePerSecond)
        {
            characters = CopyUpTo(
                configuredCharacters,
                MaxConfiguredPartyMembers);

            int strategicLimit = strategicBoardView != null
                && strategicBoardView.Slots != null
                    ? strategicBoardView.Slots.Count
                    : DefaultStrategicSlotLimit;
            strategicSkills = CopyUpTo(
                configuredStrategicSkills,
                strategicLimit);

            currentGauge = Mathf.Max(0, configuredCurrentGauge);
            maxGauge = Mathf.Max(0, configuredMaxGauge);
            chargePerSecond = configuredChargePerSecond;
        }

        [ContextMenu("Apply Configured Data")]
        public void ApplyConfiguredData()
        {
            if (!EnsurePlayMode(nameof(ApplyConfiguredData)))
            {
                return;
            }

            int appliedPartyMembers = ApplyPartyDataInternal();
            int appliedStrategicSlots = ApplyStrategicDataInternal();
            int displayedCurrentGauge = maxGauge > 0
                ? Mathf.Clamp(currentGauge, 0, maxGauge)
                : 0;
            int displayedMaxGauge = Mathf.Max(0, maxGauge);

            Debug.Log(
                $"[BattleUiDataSetupTester] Applied party={appliedPartyMembers}, strategicSlots={appliedStrategicSlots}, gauge={displayedCurrentGauge}/{displayedMaxGauge}, chargePerSecond={chargePerSecond:0.##}.",
                this);
        }

        [ContextMenu("Apply Party Data")]
        public void ApplyPartyData()
        {
            if (!EnsurePlayMode(nameof(ApplyPartyData)))
            {
                return;
            }

            int appliedPartyMembers = ApplyPartyDataInternal();
            Debug.Log(
                $"[BattleUiDataSetupTester] Applied party={appliedPartyMembers}.",
                this);
        }

        [ContextMenu("Apply Strategic Data")]
        public void ApplyStrategicData()
        {
            if (!EnsurePlayMode(nameof(ApplyStrategicData)))
            {
                return;
            }

            int appliedStrategicSlots = ApplyStrategicDataInternal();
            int displayedCurrentGauge = maxGauge > 0
                ? Mathf.Clamp(currentGauge, 0, maxGauge)
                : 0;
            int displayedMaxGauge = Mathf.Max(0, maxGauge);
            Debug.Log(
                $"[BattleUiDataSetupTester] Applied strategicSlots={appliedStrategicSlots}, gauge={displayedCurrentGauge}/{displayedMaxGauge}, chargePerSecond={chargePerSecond:0.##}.",
                this);
        }

        [ContextMenu("Clear Test Data")]
        public void ClearTestData()
        {
            if (!EnsurePlayMode(nameof(ClearTestData)))
            {
                return;
            }

            PartyHudView resolvedPartyHud = ResolvePartyHudView();
            if (resolvedPartyHud != null)
            {
                resolvedPartyHud.Clear();
            }
            else
            {
                List<PartyHudMemberView> memberViews = ResolvePartyMemberViews();
                for (int index = 0; index < memberViews.Count; index++)
                {
                    memberViews[index].Render(null, true);
                }
            }

            StrategicBoardView resolvedStrategicBoard = ResolveStrategicBoardView();
            if (resolvedStrategicBoard != null)
            {
                IReadOnlyList<StrategicSkillSlotView> slots = resolvedStrategicBoard.Slots;
                if (slots != null)
                {
                    for (int index = 0; index < slots.Count; index++)
                    {
                        if (slots[index] != null)
                        {
                            slots[index].ClearContent();
                        }
                    }
                }

                resolvedStrategicBoard.SetGauge(0, 0);
                resolvedStrategicBoard.SetChargePerSecond(0f);
                resolvedStrategicBoard.RefreshSlotResourceStates(0);
            }

            Debug.Log(
                "[BattleUiDataSetupTester] Cleared test party data, strategic slots, and gauge presentation.",
                this);
        }

        [ContextMenu("Phase 2/Inject Configured Data At Battle Entry (Placeholder)")]
        public void InjectConfiguredDataAtBattleEntry()
        {
            Debug.Log(
                "[PLACEHOLDER] BattleUiDataSetupTester.InjectConfiguredDataAtBattleEntry called; Battle entry hook pending Phase 1 Play Mode verification.",
                this);
        }

        private int ApplyPartyDataInternal()
        {
            List<PartyHudMemberData> members = BuildPartyMemberData();
            PartyHudView resolvedPartyHud = ResolvePartyHudView();
            if (resolvedPartyHud != null)
            {
                resolvedPartyHud.Render(new PartyHudViewData(members));
                return Mathf.Min(members.Count, MaxConfiguredPartyMembers);
            }

            List<PartyHudMemberView> memberViews = ResolvePartyMemberViews();
            if (memberViews.Count == 0)
            {
                Debug.LogWarning(
                    "[BattleUiDataSetupTester] No PartyHudView or scene PartyHudMemberView was found. Party data was not applied.",
                    this);
                return 0;
            }

            int appliedCount = Mathf.Min(members.Count, memberViews.Count);
            for (int index = 0; index < memberViews.Count; index++)
            {
                PartyHudMemberData data = index < appliedCount ? members[index] : null;
                memberViews[index].Render(data, true);
            }

            if (members.Count > memberViews.Count)
            {
                Debug.LogWarning(
                    $"[BattleUiDataSetupTester] Party data has {members.Count} members but PartyBoard has only {memberViews.Count} member views. Applied the first {appliedCount}.",
                    this);
            }

            return appliedCount;
        }

        private List<PartyHudMemberData> BuildPartyMemberData()
        {
            var result = new List<PartyHudMemberData>(MaxConfiguredPartyMembers);
            if (characters == null)
            {
                return result;
            }

            int configuredCount = Mathf.Min(characters.Count, MaxConfiguredPartyMembers);
            if (characters.Count > MaxConfiguredPartyMembers)
            {
                Debug.LogWarning(
                    $"[BattleUiDataSetupTester] Character list exceeds {MaxConfiguredPartyMembers}; extra entries are ignored.",
                    this);
            }

            for (int index = 0; index < configuredCount; index++)
            {
                CharacterSO character = characters[index];
                if (character == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character entry {index} is null and was skipped.",
                        this);
                    continue;
                }

                CharacterPresentationData presentation =
                    characterPresentationResolver.ResolveData(
                        character,
                        PresentationContext.Preview);
                string displayName = presentation?.Identity?.DisplayName;
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = !string.IsNullOrWhiteSpace(character.CharacterId)
                        ? character.CharacterId
                        : character.name;
                }

                Sprite portrait = presentation?.Identity?.Icon;
                if (portrait == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has no portrait in CharacterPresentationResolver; using null.",
                        character);
                }

                float maxHp = ResolveMaxHp(character);
                BuildSkillData(
                    character,
                    out List<PartyHudSkillSlotData> activeSkills,
                    out PartyHudSkillSlotData passiveSkill);

                result.Add(new PartyHudMemberData(
                    displayName,
                    portrait,
                    maxHp,
                    maxHp,
                    TestStatusText,
                    Color.gray,
                    activeSkills,
                    passiveSkill));
            }

            return result;
        }

        private float ResolveMaxHp(CharacterSO character)
        {
            IReadOnlyList<StatEntry> stats = character.BaseStats;
            if (stats != null)
            {
                for (int index = 0; index < stats.Count; index++)
                {
                    StatEntry stat = stats[index];
                    if (stat == null || stat.statType != StatType.MaxHp)
                    {
                        continue;
                    }

                    if (stat.value < 0f)
                    {
                        Debug.LogWarning(
                            $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has a negative MaxHp ({stat.value}); the display value is clamped to 0.",
                            character);
                    }

                    return Mathf.Max(0f, stat.value);
                }
            }

            Debug.LogWarning(
                $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has no BaseStats StatType.MaxHp entry; displaying 0 / 0.",
                character);
            return 0f;
        }

        private void BuildSkillData(
            CharacterSO character,
            out List<PartyHudSkillSlotData> activeSkills,
            out PartyHudSkillSlotData passiveSkill)
        {
            activeSkills = new List<PartyHudSkillSlotData>(MaxActiveSkillSlots);
            passiveSkill = null;

            var basicCandidates = new List<SkillCandidate>();
            var activeCandidates = new List<SkillCandidate>();
            var passiveCandidates = new List<SkillCandidate>();
            IReadOnlyList<CharacterSkillEntry> entries = character.Skills;

            if (entries == null)
            {
                Debug.LogWarning(
                    $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has no skill list.",
                    character);
                return;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                CharacterSkillEntry entry = entries[index];
                if (entry == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' skill entry {index} is null and was excluded.",
                        character);
                    continue;
                }

                if (entry.skillSo == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' slot '{entry.slotKey}' has no EquipmentSkillSO and was excluded.",
                        character);
                    continue;
                }

                if (string.Equals(entry.slotKey, BasicAttackSlotKey, StringComparison.Ordinal))
                {
                    basicCandidates.Add(new SkillCandidate(entry.skillSo, index, 0));
                }
                else if (TryGetNumberedSlotOrder(entry.slotKey, ActiveSlotPrefix, out int activeOrder))
                {
                    activeCandidates.Add(new SkillCandidate(entry.skillSo, index, activeOrder));
                }
                else if (TryGetNumberedSlotOrder(entry.slotKey, PassiveSlotPrefix, out int passiveOrder))
                {
                    passiveCandidates.Add(new SkillCandidate(entry.skillSo, index, passiveOrder));
                }
                else
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has unrecognized slotKey '{entry.slotKey}'. Only basic_attack, active_<positive number>, and passive_<positive number> are mapped.",
                        character);
                }
            }

            if (basicCandidates.Count == 0)
            {
                if (activeCandidates.Count > 0)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has active slots but no basic_attack. Active slots were excluded because PartyHudMemberView would otherwise promote an active skill into the basic slot.",
                        character);
                }
            }
            else
            {
                if (basicCandidates.Count > 1)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has multiple basic_attack entries; only the first is used.",
                        character);
                }

                activeSkills.Add(CreateSkillSlotData(basicCandidates[0].Skill, true));
                activeCandidates.Sort(SkillCandidate.Compare);
                int otherActiveLimit = MaxActiveSkillSlots - 1;
                int otherActiveCount = Mathf.Min(activeCandidates.Count, otherActiveLimit);
                for (int index = 0; index < otherActiveCount; index++)
                {
                    activeSkills.Add(CreateSkillSlotData(activeCandidates[index].Skill, false));
                }

                if (activeCandidates.Count > otherActiveLimit)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' exceeds the four active-slot display limit; extra active slots were excluded.",
                        character);
                }
            }

            if (passiveCandidates.Count > 0)
            {
                passiveCandidates.Sort(SkillCandidate.Compare);
                passiveSkill = CreateSkillSlotData(passiveCandidates[0].Skill, false, true);
                if (passiveCandidates.Count > 1)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Character '{CharacterLabel(character)}' has multiple passive slots; only the lowest numbered slot is used.",
                        character);
                }
            }
        }

        private PartyHudSkillSlotData CreateSkillSlotData(
            EquipmentSkillSO skill,
            bool isBasicAttack,
            bool isPassive = false)
        {
            Sprite icon = skill.Icon;
            if (icon == null)
            {
                Debug.LogWarning(
                    $"[BattleUiDataSetupTester] EquipmentSkillSO '{skill.name}' has no authoritative Icon; using null.",
                    skill);
            }

            return new PartyHudSkillSlotData(
                icon,
                0f,
                0f,
                isPassive ? PartyHudSkillState.Passive : PartyHudSkillState.Available,
                isBasicAttack);
        }

        private int ApplyStrategicDataInternal()
        {
            StrategicBoardView board = ResolveStrategicBoardView();
            if (board == null)
            {
                Debug.LogWarning(
                    "[BattleUiDataSetupTester] No StrategicBoardView was found. Strategic data was not applied.",
                    this);
                return 0;
            }

            IReadOnlyList<StrategicSkillSlotView> slots = board.Slots;
            int slotCount = slots?.Count ?? 0;
            int configuredCount = strategicSkills?.Count ?? 0;
            if (configuredCount > slotCount)
            {
                Debug.LogWarning(
                    $"[BattleUiDataSetupTester] Strategic skill list has {configuredCount} entries but the board has {slotCount} slots. Extra entries are ignored.",
                    this);
            }

            int appliedCount = 0;
            for (int index = 0; index < slotCount; index++)
            {
                StrategicSkillSlotView slot = slots[index];
                if (slot == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] StrategicBoardView.Slots[{index}] is null.",
                        board);
                    continue;
                }

                StrategicSkillItemSO item = index < configuredCount
                    ? strategicSkills[index]
                    : null;
                if (index >= configuredCount)
                {
                    slot.ClearContent();
                    continue;
                }

                if (item == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] Strategic skill entry {index} is null; the slot was cleared.",
                        this);
                    slot.ClearContent();
                    continue;
                }

                if (item.icon == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] StrategicSkillItemSO '{item.name}' has no icon; using null.",
                        item);
                }

                if (item.skillSo == null)
                {
                    Debug.LogWarning(
                        $"[BattleUiDataSetupTester] StrategicSkillItemSO '{item.name}' has no skillSo. The item SO remains the slot payload for display testing.",
                        item);
                }

                slot.SetContent(item.icon, item.gaugeCost, item);
                appliedCount++;
            }

            int safeMaxGauge = Mathf.Max(0, maxGauge);
            int safeCurrentGauge = safeMaxGauge > 0
                ? Mathf.Clamp(currentGauge, 0, safeMaxGauge)
                : 0;
            board.SetGauge(currentGauge, maxGauge);
            board.SetChargePerSecond(chargePerSecond);
            board.RefreshSlotResourceStates(safeCurrentGauge);

            return appliedCount;
        }

        private PartyHudView ResolvePartyHudView()
        {
            if (partyHudView != null)
            {
                return partyHudView;
            }

            return findViewsAutomatically ? FindLoadedSceneComponent<PartyHudView>() : null;
        }

        private List<PartyHudMemberView> ResolvePartyMemberViews()
        {
            Transform root = partyBoardRoot;
            if (root == null && findViewsAutomatically)
            {
                GameObject partyBoard = GameObject.Find("PartyBoard");
                root = partyBoard != null ? partyBoard.transform : null;
            }

            PartyHudMemberView[] found = root != null
                ? root.GetComponentsInChildren<PartyHudMemberView>(true)
                : findViewsAutomatically
                    ? FindLoadedSceneComponents<PartyHudMemberView>()
                    : Array.Empty<PartyHudMemberView>();

            var result = new List<PartyHudMemberView>(found.Length);
            for (int index = 0; index < found.Length; index++)
            {
                if (found[index] != null && found[index].gameObject.scene.IsValid())
                {
                    result.Add(found[index]);
                }
            }

            result.Sort(CompareHierarchyOrder);
            return result;
        }

        private StrategicBoardView ResolveStrategicBoardView()
        {
            if (strategicBoardView != null)
            {
                return strategicBoardView;
            }

            return findViewsAutomatically
                ? FindLoadedSceneComponent<StrategicBoardView>()
                : null;
        }

        private static bool EnsurePlayMode(string operation)
        {
            if (Application.isPlaying)
            {
                return true;
            }

            Debug.LogWarning(
                $"[BattleUiDataSetupTester] {operation} is Play Mode only. No scene or prefab data was changed.");
            return false;
        }

        private static T FindLoadedSceneComponent<T>() where T : Component
        {
            T[] components = FindLoadedSceneComponents<T>();
            return components.Length > 0 ? components[0] : null;
        }

        private static T[] FindLoadedSceneComponents<T>() where T : Component
        {
#if UNITY_2023_1_OR_NEWER
            T[] candidates = UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
#else
            T[] candidates = UnityEngine.Object.FindObjectsOfType<T>(true);
#endif
            var result = new List<T>(candidates.Length);
            for (int index = 0; index < candidates.Length; index++)
            {
                T candidate = candidates[index];
                if (candidate != null && candidate.gameObject.scene.IsValid())
                {
                    result.Add(candidate);
                }
            }

            return result.ToArray();
        }

        private static int CompareHierarchyOrder(
            PartyHudMemberView left,
            PartyHudMemberView right)
        {
            return string.CompareOrdinal(
                GetHierarchyOrderKey(left.transform),
                GetHierarchyOrderKey(right.transform));
        }

        private static string GetHierarchyOrderKey(Transform transform)
        {
            var indices = new Stack<int>();
            Transform current = transform;
            while (current != null)
            {
                indices.Push(current.GetSiblingIndex());
                current = current.parent;
            }

            return string.Join("/", indices);
        }

        private static bool TryGetNumberedSlotOrder(
            string slotKey,
            string prefix,
            out int order)
        {
            order = 0;
            if (string.IsNullOrWhiteSpace(slotKey)
                || !slotKey.StartsWith(prefix, StringComparison.Ordinal))
            {
                return false;
            }

            return int.TryParse(slotKey.Substring(prefix.Length), out order)
                && order > 0;
        }

        private static string CharacterLabel(CharacterSO character)
        {
            return !string.IsNullOrWhiteSpace(character.CharacterId)
                ? character.CharacterId
                : character.name;
        }

        private static List<T> CopyUpTo<T>(
            IReadOnlyList<T> source,
            int maxCount)
        {
            int safeMaxCount = Mathf.Max(0, maxCount);
            int count = source != null
                ? Mathf.Min(source.Count, safeMaxCount)
                : 0;
            var copy = new List<T>(count);

            for (int index = 0; index < count; index++)
            {
                copy.Add(source[index]);
            }

            return copy;
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
                $"[BattleUiDataSetupTester] Inspector field '{fieldName}' was trimmed to {maxCount} entries.",
                this);
        }
#endif

        private sealed class SkillCandidate
        {
            public EquipmentSkillSO Skill { get; }
            private int SourceIndex { get; }
            private int SlotOrder { get; }

            public SkillCandidate(
                EquipmentSkillSO skill,
                int sourceIndex,
                int slotOrder)
            {
                Skill = skill;
                SourceIndex = sourceIndex;
                SlotOrder = slotOrder;
            }

            public static int Compare(SkillCandidate left, SkillCandidate right)
            {
                int orderComparison = left.SlotOrder.CompareTo(right.SlotOrder);
                return orderComparison != 0
                    ? orderComparison
                    : left.SourceIndex.CompareTo(right.SourceIndex);
            }
        }
    }
}
