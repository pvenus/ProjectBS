using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillHit",
    menuName = "BS/Skills/Hit/Skill Hit SO",
    order = 20)]
public class SkillHitSO : ScriptableObject
{
    [Header("Hit Policy")]
    [SerializeField, Min(1)] private int maxHitCount = 1;
    [SerializeField] private bool ignoreSameRoot = true;
    [SerializeField] private bool useRepeatInterval;

    [SerializeField, Min(0f)] private float repeatInterval = 0.25f;

    [Header("Hit Timing")]
    [SerializeField] private bool useHitWindow;
    [SerializeField, Min(0f)] private float hitStartTime;
    [SerializeField, Min(0f)] private float hitDuration = 0.1f;
    [SerializeField] private bool deactivateAfterFirstHit;

    public int MaxHitCount => maxHitCount;
    public bool IgnoreSameRoot => ignoreSameRoot;
    public bool UseRepeatInterval => useRepeatInterval;
    public float RepeatInterval => repeatInterval;

    public bool UseHitWindow => useHitWindow;
    public float HitStartTime => hitStartTime;
    public float HitDuration => hitDuration;
    public bool DeactivateAfterFirstHit => deactivateAfterFirstHit;

    [Header("Damage")]
    [SerializeField] private int damage;
    [SerializeField] private bool applyDamage = true;

    [Header("Split Multi-Hit Damage")]
    [SerializeField] private bool useSplitMultiHitDamage;
    [SerializeField, Min(1)] private int splitHitCount = 4;
    [SerializeField, Min(0f)] private float splitHitInterval = 0.1f;

    public int Damage => damage;
    public bool ApplyDamage => applyDamage;
    public bool UseSplitMultiHitDamage => useSplitMultiHitDamage;
    public int SplitHitCount => splitHitCount;
    public float SplitHitInterval => splitHitInterval;

    public SkillProjectileHitDto CreateDto()
    {
        return new SkillProjectileHitDto
        {
            maxHitCount = Mathf.Max(1, maxHitCount),
            ignoreSameRoot = ignoreSameRoot,
            useRepeatInterval = useRepeatInterval,
            repeatInterval = Mathf.Max(0f, repeatInterval),
            useHitWindow = useHitWindow,
            hitStartTime = Mathf.Max(0f, hitStartTime),
            hitDuration = Mathf.Max(0f, hitDuration),
            deactivateAfterFirstHit = deactivateAfterFirstHit,
            damage = damage,
            applyDamage = applyDamage,
            useSplitMultiHitDamage = useSplitMultiHitDamage,
            splitHitCount = Mathf.Max(1, splitHitCount),
            splitHitInterval = Mathf.Max(0f, splitHitInterval)
        };
    }

    public void ApplyTo(SkillProjectileHitMono hitMono)
    {
        if (hitMono == null)
        {
            Debug.LogError("SkillProjectileHitMono is null");
            return;
        }

        hitMono.Initialize(CreateDto());
    }
}
