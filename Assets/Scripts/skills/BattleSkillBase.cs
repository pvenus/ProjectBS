using UnityEngine;
using UnityEngine.Serialization;

public enum BattleSkillCategory
{
    None,
    Attack,
    Defense,
    Heal,
    Control,
    Buff,
    Debuff,
    Utility
}

public enum BattleSkillTargetType
{
    None,
    Self,
    Ally,
    Enemy,
    Point
}

public enum BattleSkillTacticalNeed
{
    None,
    SelfDefense,
    AllySupport,
    AreaControl,
    OffensivePressure,
    Utility
}

public abstract class BattleSkillBase : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string skillId;
    [SerializeField] private string displayName;

    [Header("Brain Meta")]
    [SerializeField] private BattleSkillCategory category = BattleSkillCategory.None;
    [SerializeField] private BattleSkillTargetType targetType = BattleSkillTargetType.None;
    [SerializeField] private BattleSkillTacticalNeed tacticalNeed = BattleSkillTacticalNeed.None;
    [SerializeField] private float basePriority = 0f;

    [Header("Common Combat Values")]
    [FormerlySerializedAs("range")]
    [SerializeField] private float brainRange = 0f;
    [FormerlySerializedAs("radius")]
    [SerializeField] private float brainRadius = 0f;
    [FormerlySerializedAs("cooldown")]
    [SerializeField] private float brainCooldown = 0f;

    public string SkillId => string.IsNullOrWhiteSpace(skillId) ? name : skillId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public BattleSkillCategory Category => category;
    public BattleSkillTargetType TargetType => targetType;
    public BattleSkillTacticalNeed TacticalNeed => tacticalNeed;
    public float BasePriority => basePriority;
    public float Range => brainRange;
    public float Radius => brainRadius;
    public float Cooldown => brainCooldown;

    public virtual float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return basePriority + roleBias;
    }
}
