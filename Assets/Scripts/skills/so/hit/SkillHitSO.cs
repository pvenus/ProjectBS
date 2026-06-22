using UnityEngine;
using Skills.Dto;
using Effect;
using System.Collections.Generic;
using Skill;
using Effect.Helper;

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
    [SerializeField] private bool deactivateAfterFirstHit;

    [Header("Target")]
    [SerializeField] private LayerMask targetLayerMask = ~0;

    public int MaxHitCount => maxHitCount;
    public bool IgnoreSameRoot => ignoreSameRoot;
    public bool UseRepeatInterval => useRepeatInterval;
    public float RepeatInterval => repeatInterval;

    public bool UseHitWindow => useHitWindow;
    public float HitStartTime => hitStartTime;
    public bool DeactivateAfterFirstHit => deactivateAfterFirstHit;
    public LayerMask TargetLayerMask => targetLayerMask;

    public SkillDamageSO DamageSo => damageSo;

    [Header("Damage")]
    [SerializeField] private SkillDamageSO damageSo;
    [SerializeField, Min(0f)] private float firstHitBaseDamage;

    public float FirstHitBaseDamage => Mathf.Max(0f, firstHitBaseDamage);
    public bool HasFirstHitBaseDamage => firstHitBaseDamage > 0f;

    [Header("Additional Effects")]
    [Tooltip("히트 성공 시 대상에게 추가로 적용할 버프 효과 목록")]
    [SerializeField] private SkillProjectileHitEffectEntry[] buffEffects;

    [Tooltip("히트 성공 시 대상에게 추가로 적용할 디버프 효과 목록")]
    [SerializeField] private SkillProjectileHitEffectEntry[] debuffEffects;

    public SkillProjectileHitEffectEntry[] BuffEffects => buffEffects;
    public SkillProjectileHitEffectEntry[] DebuffEffects => debuffEffects;

    [Header("Split Multi-Hit Damage")]
    [SerializeField] private bool useSplitMultiHitDamage;
    [SerializeField, Min(1)] private int splitHitCount = 4;
    [SerializeField, Min(0f)] private float splitHitInterval = 0.1f;

    public bool UseSplitMultiHitDamage => useSplitMultiHitDamage;
    public int SplitHitCount => splitHitCount;
    public float SplitHitInterval => splitHitInterval;

    public SkillDamageProfileDto CreateDamageProfileDto()
    {
        return damageSo != null
            ? damageSo.CreateDto()
            : null;
    }

    public SkillProjectileHitDto CreateDto()
    {
        return CreateDto(
            CreateDamageProfileDto());
    }

    public SkillProjectileHitDto CreateDto(
        SkillDamageProfileDto resolvedDamageProfile,
        IReadOnlyList<EffectUpgradeModifierData> effectUpgradeModifiers = null)
    {
        return new SkillProjectileHitDto
        {
            maxHitCount = Mathf.Max(1, maxHitCount),
            ignoreSameRoot = ignoreSameRoot,
            useRepeatInterval = useRepeatInterval,
            repeatInterval = Mathf.Max(0f, repeatInterval),
            hitStartTime = Mathf.Max(0f, hitStartTime),
            deactivateAfterFirstHit = deactivateAfterFirstHit,
            targetLayerMask = targetLayerMask,
            damageProfile = resolvedDamageProfile,
            buffEffects = CopyEffectEntries(
                buffEffects,
                EffectCategoryType.Buff,
                effectUpgradeModifiers),
            debuffEffects = CopyEffectEntries(
                debuffEffects,
                EffectCategoryType.Debuff,
                effectUpgradeModifiers),
            splitHitCount = Mathf.Max(1, splitHitCount),
            splitHitInterval = Mathf.Max(0f, splitHitInterval),
        };
    }

    private SkillProjectileHitEffectEntry[] CopyEffectEntries(
        SkillProjectileHitEffectEntry[] source,
        EffectCategoryType defaultCategoryType,
        IReadOnlyList<EffectUpgradeModifierData> effectUpgradeModifiers)
    {
        if (source == null || source.Length == 0)
        {
            return null;
        }

        SkillProjectileHitEffectEntry[] result =
            new SkillProjectileHitEffectEntry[source.Length];

        for (int i = 0; i < source.Length; i++)
        {
            SkillProjectileHitEffectEntry entry = source[i];

            if (entry == null)
            {
                continue;
            }

            SkillProjectileHitEffectEntry copiedEntry =
                EffectUpgradeApplyHelper.CreateResolvedEntry(
                    entry,
                    effectUpgradeModifiers);

            copiedEntry.categoryType =
                copiedEntry.categoryType == EffectCategoryType.Neutral
                    ? defaultCategoryType
                    : copiedEntry.categoryType;

            result[i] = copiedEntry;
        }

        return result;
    }
}
