using UnityEngine;
using Skills.Dto;

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
    [SerializeField] private SkillDamageSO damageSo;

    public SkillDamageSO DamageSo => damageSo;
    public bool ApplyDamage => damageSo != null;

    [Header("Split Multi-Hit Damage")]
    [SerializeField] private bool useSplitMultiHitDamage;
    [SerializeField, Min(1)] private int splitHitCount = 4;
    [SerializeField, Min(0f)] private float splitHitInterval = 0.1f;

    [Header("Knockback")]
    [SerializeField] private bool useKnockback;
    [SerializeField, Min(0f)] private float knockbackForce;

    public bool UseSplitMultiHitDamage => useSplitMultiHitDamage;
    public int SplitHitCount => splitHitCount;
    public float SplitHitInterval => splitHitInterval;
    public bool UseKnockback => useKnockback;
    public float KnockbackForce => knockbackForce;

    private void ResolveValues(SkillUpgradeMono.SkillUpgradeData upgradeData, out SkillDamageProfileDto resolvedDamageProfile, out float resolvedKnockbackForce)
    {
        resolvedDamageProfile = damageSo != null ? damageSo.CreateDto() : null;
        resolvedKnockbackForce = Mathf.Max(0f, knockbackForce);

        if (resolvedDamageProfile != null)
        {
            resolvedDamageProfile.baseDamage = Mathf.Max(0f, resolvedDamageProfile.baseDamage + upgradeData.damageAdd);
        }

        resolvedKnockbackForce = Mathf.Max(0f, resolvedKnockbackForce + upgradeData.knockbackForceAdd);
    }
    public SkillProjectileHitDto CreateDto(SkillUpgradeMono.SkillUpgradeData upgradeData)
    {
        ResolveValues(upgradeData, out SkillDamageProfileDto resolvedDamageProfile, out float resolvedKnockbackForce);
        return CreateDto(resolvedDamageProfile, resolvedKnockbackForce);
    }

    public SkillProjectileHitDto CreateDto(SkillDamageProfileDto resolvedDamageProfile, float resolvedKnockbackForce)
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
            damageProfile = resolvedDamageProfile,
            applyDamage = resolvedDamageProfile != null,
            useSplitMultiHitDamage = useSplitMultiHitDamage,
            splitHitCount = Mathf.Max(1, splitHitCount),
            splitHitInterval = Mathf.Max(0f, splitHitInterval),
            useKnockback = useKnockback,
            knockbackForce = Mathf.Max(0f, resolvedKnockbackForce)
        };
    }
}
