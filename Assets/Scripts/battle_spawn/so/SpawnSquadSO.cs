using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

[Serializable]
public sealed class SpawnSquadGroup
{
    [SerializeField] private int order;
    [SerializeField] private CharacterSO character;
    [SerializeField] private SpawnPattern pattern;

    [SerializeField] private Vector2 localOffset;
    [SerializeField] private float localRotation;

    [Min(0f)]
    [SerializeField] private float slotInterval;

    [Min(1)]
    [SerializeField] private int quantity = 1;

    public SpawnSquadGroup(
        int order, 
        CharacterSO character, 
        SpawnPattern pattern, 
        Vector2 localOffset, 
        float localRotation, 
        float slotInterval,
        int quantity = 1)
    {
        this.order = order;
        this.character = character;
        this.pattern = pattern;
        this.localOffset = localOffset;
        this.localRotation = localRotation;
        this.slotInterval = slotInterval;
        this.quantity = quantity;
    }

    public int Order => order;
    public CharacterSO Character => character;
    public SpawnPattern Pattern => pattern;
    public Vector2 LocalOffset => localOffset;
    public float LocalRotation => localRotation;
    public float SlotInterval => slotInterval;
    public int Quantity => quantity;
}

[CreateAssetMenu(fileName = "SpawnSquad", menuName = "Battle Spawn/Content/Squad")]
public sealed class SpawnSquadSO : SpawnContentSO
{
    [Min(0f)]
    [SerializeField] private float groupInterval;

    [SerializeField]
    private List<SpawnSquadGroup> groups = new List<SpawnSquadGroup>();

    public float GroupInterval => groupInterval;
    public IReadOnlyList<SpawnSquadGroup> Groups => groups;

    public void Initialize(string id, float groupIntervalVal, List<SpawnSquadGroup> groupsVal)
    {
        base.Initialize(id, 0f);
        this.groupInterval = groupIntervalVal;
        this.groups = groupsVal ?? new List<SpawnSquadGroup>();
    }

    // --- 레거시 호환용 ---
    [SerializeField] private SpawnPattern legacyPattern;
    [SerializeField] private CharacterSO legacyNpc;
    public SpawnPattern Pattern => legacyPattern;
    public CharacterSO Npc => legacyNpc;
    public void Initialize(string id, SpawnPattern pattern, CharacterSO npc, float spawnDelay = 0f)
    {
        base.Initialize(id, spawnDelay);
        this.legacyPattern = pattern;
        this.legacyNpc = npc;
    }
}
