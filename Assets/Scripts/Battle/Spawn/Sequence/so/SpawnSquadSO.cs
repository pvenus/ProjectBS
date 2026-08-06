using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class SpawnSquadGroup
{
    [SerializeField] private int order;
    [SerializeField] private string spawnUnitKey;
    [SerializeField] private SpawnUnitRole spawnRole = SpawnUnitRole.Any;
    [SerializeField] private string patternId;
    [SerializeField] private string patternDisplayName;
    [SerializeField] private SpawnPatternKind patternKind;
    [SerializeReference] private SpawnPatternConfig patternConfig;

    [SerializeField] private Vector2 localOffset;
    [SerializeField] private float localRotation;

    [Min(0f)]
    [SerializeField] private float slotInterval;

    [Min(0)]
    [SerializeField] private int quantity;

    public SpawnSquadGroup(
        int order,
        string spawnUnitKey,
        SpawnUnitRole spawnRole,
        SpawnPatternData pattern,
        Vector2 localOffset,
        float localRotation,
        float slotInterval,
        int quantity = 0)
    {
        this.order = order;
        this.spawnUnitKey = spawnUnitKey;
        this.spawnRole = spawnRole;
        SetPattern(pattern);
        this.localOffset = localOffset;
        this.localRotation = localRotation;
        this.slotInterval = slotInterval;
        this.quantity = quantity;
    }

    public int Order => order;
    public string SpawnUnitKey => spawnUnitKey;
    public SpawnUnitRole SpawnRole => spawnRole;
    public string PatternId => patternId;
    public string PatternDisplayName => patternDisplayName;
    public SpawnPatternKind PatternKind => patternKind;
    public SpawnPatternConfig PatternConfig => patternConfig;
    public FixedSpawnPatternConfig FixedConfig => patternKind.IsFixedSlotKind() ? patternConfig as FixedSpawnPatternConfig : null;
    public RandomSpawnPatternConfig RandomConfig => patternKind.IsRandomAreaKind() ? patternConfig as RandomSpawnPatternConfig : null;
    public bool HasPattern => patternKind != SpawnPatternKind.None;
    public Vector2 LocalOffset => localOffset;
    public float LocalRotation => localRotation;
    public float SlotInterval => slotInterval;
    public int Quantity => quantity;

    public SpawnSquadGroup Clone()
    {
        return new SpawnSquadGroup(
            order,
            spawnUnitKey,
            spawnRole,
            ToPatternData(),
            localOffset,
            localRotation,
            slotInterval,
            quantity);
    }

    public void SetPattern(SpawnPatternData pattern)
    {
        if (pattern == null || !pattern.HasPattern)
        {
            patternId = string.Empty;
            patternDisplayName = string.Empty;
            patternKind = SpawnPatternKind.None;
            patternConfig = null;
            return;
        }

        patternId = pattern.PatternId;
        patternDisplayName = pattern.DisplayName;
        patternKind = pattern.PatternKind;
        patternConfig = CloneConfig(pattern);
    }

    public SpawnPatternData ToPatternData()
    {
        if (!HasPattern)
        {
            return SpawnPatternData.None();
        }

        if (patternKind.IsFixedSlotKind() && FixedConfig != null)
        {
            IReadOnlyList<SpawnPatternSlot> slots = FixedConfig.Slots;
            return new SpawnPatternData(
                patternId,
                patternDisplayName,
                patternKind,
                slots != null ? new List<SpawnPatternSlot>(slots) : new List<SpawnPatternSlot>());
        }

        if (patternKind.IsRandomAreaKind() && RandomConfig != null)
        {
            return new SpawnPatternData(patternId, patternDisplayName, patternKind, RandomConfig.Shape, RandomConfig.AreaSize);
        }

        return SpawnPatternData.None();
    }

    private static SpawnPatternConfig CloneConfig(SpawnPatternData pattern)
    {
        if (pattern.PatternKind.IsFixedSlotKind() && pattern.FixedConfig != null)
        {
            IReadOnlyList<SpawnPatternSlot> slots = pattern.FixedConfig.Slots;
            return new FixedSpawnPatternConfig(
                slots != null ? new List<SpawnPatternSlot>(slots) : new List<SpawnPatternSlot>());
        }

        if (pattern.PatternKind.IsRandomAreaKind() && pattern.RandomConfig != null)
        {
            return new RandomSpawnPatternConfig(pattern.RandomConfig.Shape, pattern.RandomConfig.AreaSize);
        }

        return null;
    }
}

[CreateAssetMenu(fileName = "SpawnSquad", menuName = "Battle Spawn/Content/Squad")]
public sealed class SpawnSquadSO : SpawnContentSO
{
    [SerializeField] private string formationPatternId;
    [SerializeField] private string formationPatternDisplayName;
    [SerializeField] private SpawnPatternKind formationPatternKind;
    [SerializeReference] private SpawnPatternConfig formationPatternConfig;

    [Min(0f)]
    [SerializeField] private float formationSlotInterval;

    [Min(1)]
    [SerializeField] private int formationQuantity = 1;

    [Min(0f)]
    [SerializeField] private float groupInterval;

    [Min(0f)]
    [SerializeField] private float slotInterval;

    [Min(1)]
    [SerializeField] private int quantity = 1;

    [SerializeField]
    private List<SpawnSquadGroup> groups = new List<SpawnSquadGroup>();

    public string FormationPatternId => formationPatternId;
    public string FormationPatternDisplayName => formationPatternDisplayName;
    public SpawnPatternKind FormationPatternKind => formationPatternKind;
    public SpawnPatternConfig FormationPatternConfig => formationPatternConfig;
    public FixedSpawnPatternConfig FormationFixedConfig => formationPatternKind.IsFixedSlotKind() ? formationPatternConfig as FixedSpawnPatternConfig : null;
    public RandomSpawnPatternConfig FormationRandomConfig => formationPatternKind.IsRandomAreaKind() ? formationPatternConfig as RandomSpawnPatternConfig : null;
    public SpawnPatternData FormationPattern => ToFormationPatternData();
    public bool HasFormationPattern => formationPatternKind != SpawnPatternKind.None;
    public float FormationSlotInterval => formationSlotInterval;
    public int FormationQuantity => formationQuantity;
    public float GroupInterval => groupInterval;
    public float SlotInterval => slotInterval;
    public int Quantity => quantity;
    public IReadOnlyList<SpawnSquadGroup> Groups => groups;

    public void Initialize(string id, float groupIntervalVal, List<SpawnSquadGroup> groupsVal)
    {
        Initialize(id, groupIntervalVal, 0f, 1, groupsVal);
    }

    public void Initialize(
        string id,
        float groupIntervalVal,
        float slotIntervalVal,
        int quantityVal,
        List<SpawnSquadGroup> groupsVal)
    {
        Initialize(
            id,
            SpawnPatternData.None(),
            0f,
            1,
            groupIntervalVal,
            slotIntervalVal,
            quantityVal,
            groupsVal);
    }

    public void Initialize(
        string id,
        SpawnPatternData formationPatternVal,
        float formationSlotIntervalVal,
        int formationQuantityVal,
        float groupIntervalVal,
        float slotIntervalVal,
        int quantityVal,
        List<SpawnSquadGroup> groupsVal)
    {
        base.Initialize(id, 0f);
        SetFormationPattern(formationPatternVal);
        this.formationSlotInterval = Mathf.Max(0f, formationSlotIntervalVal);
        this.formationQuantity = Mathf.Max(1, formationQuantityVal);
        this.groupInterval = groupIntervalVal;
        this.slotInterval = Mathf.Max(0f, slotIntervalVal);
        this.quantity = Mathf.Max(1, quantityVal);
        this.groups = groupsVal ?? new List<SpawnSquadGroup>();
    }

    public void SetFormationPattern(SpawnPatternData pattern)
    {
        if (pattern == null || !pattern.HasPattern)
        {
            formationPatternId = string.Empty;
            formationPatternDisplayName = string.Empty;
            formationPatternKind = SpawnPatternKind.None;
            formationPatternConfig = null;
            return;
        }

        formationPatternId = pattern.PatternId;
        formationPatternDisplayName = pattern.DisplayName;
        formationPatternKind = pattern.PatternKind;
        formationPatternConfig = CloneConfig(pattern);
    }

    public SpawnPatternData ToFormationPatternData()
    {
        if (!HasFormationPattern)
        {
            return SpawnPatternData.None();
        }

        if (formationPatternKind.IsFixedSlotKind() && FormationFixedConfig != null)
        {
            IReadOnlyList<SpawnPatternSlot> slots = FormationFixedConfig.Slots;
            return new SpawnPatternData(
                formationPatternId,
                formationPatternDisplayName,
                formationPatternKind,
                slots != null ? new List<SpawnPatternSlot>(slots) : new List<SpawnPatternSlot>());
        }

        if (formationPatternKind.IsRandomAreaKind() && FormationRandomConfig != null)
        {
            return new SpawnPatternData(formationPatternId, formationPatternDisplayName, formationPatternKind, FormationRandomConfig.Shape, FormationRandomConfig.AreaSize);
        }

        return SpawnPatternData.None();
    }

    private static SpawnPatternConfig CloneConfig(SpawnPatternData pattern)
    {
        if (pattern.PatternKind.IsFixedSlotKind() && pattern.FixedConfig != null)
        {
            IReadOnlyList<SpawnPatternSlot> slots = pattern.FixedConfig.Slots;
            return new FixedSpawnPatternConfig(
                slots != null ? new List<SpawnPatternSlot>(slots) : new List<SpawnPatternSlot>());
        }

        if (pattern.PatternKind.IsRandomAreaKind() && pattern.RandomConfig != null)
        {
            return new RandomSpawnPatternConfig(pattern.RandomConfig.Shape, pattern.RandomConfig.AreaSize);
        }

        return null;
    }
}
