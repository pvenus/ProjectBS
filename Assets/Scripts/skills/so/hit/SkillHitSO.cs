using System;
using UnityEngine;
using Effect;
using Skill;

[CreateAssetMenu(
    fileName = "SkillHit",
    menuName = "BS/Skills/Hit/Skill Hit SO",
    order = 20)]
public class SkillHitSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string hitId;

    [Header("Hit Policy")]
    [SerializeField, Min(1)] private int maxHitCount = 1;
    [SerializeField] private bool ignoreSameRoot = true;
    [SerializeField] private bool useRepeatInterval;
    [SerializeField, Min(0f)] private float repeatInterval = 0.25f;

    [Header("Hit Timing")]
    [SerializeField, Min(0f)] private float hitStartTime;
    [SerializeField] private bool deactivateAfterFirstHit;

    [Header("Target")]
    [SerializeField] private LayerMask targetLayerMask = ~0;

    [Header("Damage Profile")]
    [SerializeField] private SkillHitDamageProfile damage = new();

    [Header("Skill Spawn Profile")]
    [SerializeField] private SkillHitSpawnProfile skillSpawn = new();

    [Header("Effect Profile")]
    [SerializeField] private SkillHitEffectProfile effects = new();

    [Header("Split Multi-Hit Profile")]
    [SerializeField] private SkillHitSplitProfile split = new();

    public string HitId => hitId;
    public int MaxHitCount => maxHitCount;
    public bool IgnoreSameRoot => ignoreSameRoot;
    public bool UseRepeatInterval => useRepeatInterval;
    public float RepeatInterval => repeatInterval;

    public float HitStartTime => hitStartTime;
    public bool DeactivateAfterFirstHit => deactivateAfterFirstHit;
    public LayerMask TargetLayerMask => targetLayerMask;

    // Expose SkillHitDamageProfile values as pass-throughs matching SkillDamageSO API
    public DamageType DamageType => damage.DamageType;
    public float BaseDamage => damage.BaseDamage;
    public float FirstHitBaseDamage => damage.FirstHitBaseDamage;
    public float AttackPercentDamage => damage.AttackPercentDamage;
    public bool CanCritical => damage.CanCritical;
    public bool IgnoreDefense => damage.IgnoreDefense;
    // Add additional pass-throughs if SkillDamageSO had more fields
    public EquipmentSkillSO SpawnSkill => skillSpawn.SpawnSkill;
    public EffectEntrySO[] BuffEffects => effects.BuffEffects;
    public EffectEntrySO[] DebuffEffects => effects.DebuffEffects;
    public bool UseSplitMultiHitDamage => split.UseSplitMultiHitDamage;
    public int SplitHitCount => split.SplitHitCount;
    public float SplitHitInterval => split.SplitHitInterval;
}

[Serializable]
public class SkillHitDamageProfile
{
    [SerializeField] private DamageType damageType;
    [SerializeField] private float baseDamage;
    [SerializeField] private float firstHitBaseDamage;
    [SerializeField] private float attackPercentDamage;
    [SerializeField] private bool canCritical;
    [SerializeField] private bool ignoreDefense;
    // Add additional fields from SkillDamageSO here as needed

    public DamageType DamageType => damageType;
    public float BaseDamage => baseDamage;
    public float FirstHitBaseDamage => firstHitBaseDamage;
    public float AttackPercentDamage => attackPercentDamage;
    public bool CanCritical => canCritical;
    public bool IgnoreDefense => ignoreDefense;
    // Add additional properties as needed
}

[Serializable]
public class SkillHitSpawnProfile
{
    [SerializeField] private EquipmentSkillSO spawnSkill;

    public EquipmentSkillSO SpawnSkill => spawnSkill;
}

[Serializable]
public class SkillHitEffectProfile
{
    [Tooltip("히트 성공 시 대상에게 추가로 적용할 버프 효과 목록")]
    [SerializeField] private EffectEntrySO[] buffEffects;

    [Tooltip("히트 성공 시 대상에게 추가로 적용할 디버프 효과 목록")]
    [SerializeField] private EffectEntrySO[] debuffEffects;

    public EffectEntrySO[] BuffEffects => buffEffects;
    public EffectEntrySO[] DebuffEffects => debuffEffects;
}

[Serializable]
public class SkillHitSplitProfile
{
    [SerializeField] private bool useSplitMultiHitDamage;
    [SerializeField, Min(1)] private int splitHitCount = 4;
    [SerializeField, Min(0f)] private float splitHitInterval = 0.1f;

    public bool UseSplitMultiHitDamage => useSplitMultiHitDamage;
    public int SplitHitCount => Mathf.Max(1, splitHitCount);
    public float SplitHitInterval => Mathf.Max(0f, splitHitInterval);
}
