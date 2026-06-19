using Skills.Dto;
using Effect;
using UnityEngine;

[System.Serializable]
public class SkillProjectileHitEffectEntry
{
    public EffectSO effectSo;

    public EffectLifetimeType lifetimeType =
        EffectLifetimeType.CombatTimed;

    public EffectCategoryType categoryType =
        EffectCategoryType.Neutral;

    /// <summary>
    /// 지속 시간 기반 효과일 경우 사용.
    /// 0 이하이면 Effect 기본값 사용.
    /// </summary>
    public float duration = -1f;    

    /// <summary>
    /// 반복 적용 제한.
    /// 0 이하이면 무제한.
    /// </summary>
    public int maxApplyCount = 1;
}

[System.Serializable]
public class SkillProjectileHitDto
{
    public int maxHitCount = 1;
    public bool ignoreSameRoot = true;
    public bool useRepeatInterval;
    public float repeatInterval = 0.25f;

    public bool useHitWindow;
    public float hitStartTime;
    public bool deactivateAfterFirstHit;

    public LayerMask targetLayerMask = ~0;

    public float projectileColliderRadius = 0.5f;

    public SkillDamageProfileDto damageProfile;

    public float firstHitBaseDamage;

    public bool applyDamage = true;

    public SkillProjectileHitEffectEntry[] buffEffects;
    public SkillProjectileHitEffectEntry[] debuffEffects;

    public bool useSplitMultiHitDamage;
    public int splitHitCount = 1;
    public float splitHitInterval;
}