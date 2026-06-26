using UnityEngine;

[CreateAssetMenu(fileName = "SpawnFormation", menuName = "Battle Spawn/Content/Formation")]
public sealed class SpawnFormationSO : SpawnContentSO
{
    [SerializeField] private SpawnSquadSO squad;
    [SerializeField] private SpawnPattern pattern;

    [Min(0f)]
    [SerializeField] private float slotInterval;

    [Min(1)]
    [SerializeField] private int quantity = 1;

    public SpawnSquadSO Squad => squad;
    public SpawnPattern Pattern => pattern;
    public float SlotInterval => slotInterval;
    public int Quantity => quantity;

    public void Initialize(string id, SpawnPattern patternVal, SpawnSquadSO squadVal, float slotIntervalVal, int quantityVal = 1)
    {
        base.Initialize(id, 0f);
        this.pattern = patternVal;
        this.squad = squadVal;
        this.slotInterval = slotIntervalVal;
        this.quantity = quantityVal;
    }

    // --- 레거시 호환용 ---
    [SerializeField] private SpawnPattern legacyPattern;
    public SpawnPattern LegacyPattern => legacyPattern;
    public void Initialize(string id, SpawnPattern patternVal, SpawnSquadSO squadVal, float spawnDelay = 0f)
    {
        base.Initialize(id, spawnDelay);
        this.legacyPattern = patternVal;
        this.squad = squadVal;
    }
}
