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

    /// <summary>
    /// EffectSO 값을 업그레이드 등으로 덮어쓸 경우 사용.
    /// false이면 EffectSO 원본 값을 사용.
    /// </summary>
    public bool hasValueOverride;

    /// <summary>
    /// EffectSO의 value 필드 대체값.
    /// </summary>
    public float valueOverride;
}

[System.Serializable]
public class SkillProjectileHitDto
{
    public int maxHitCount = 1;
    public bool ignoreSameRoot = true;
    public bool useRepeatInterval;
    public float repeatInterval = 0.25f;

    public float hitStartTime;
    public bool deactivateAfterFirstHit;

    public LayerMask targetLayerMask = ~0;

    public float projectileColliderRadius = 0.5f;

    public SkillDamageProfileDto damageProfile;


    public SkillProjectileHitEffectEntry[] buffEffects;
    public SkillProjectileHitEffectEntry[] debuffEffects;

    public int splitHitCount = 1;
    public float splitHitInterval;
}