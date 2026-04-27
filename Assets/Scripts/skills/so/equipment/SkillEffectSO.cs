

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬/룬/장비에 공통적으로 사용되는 범용 Effect 데이터.
/// 실제 실행 가능한 런타임 데이터는 EffectRuntimeData로 변환되어 사용된다.
/// </summary>
[CreateAssetMenu(fileName = "SkillEffectSO", menuName = "BS/Skills/Effect/SkillEffectSO")]
public class SkillEffectSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string effectId;
    [SerializeField] private string displayName;
    [TextArea]
    [SerializeField] private string description;

    [Header("Classification")]
    [SerializeField] private ElementType elementType = ElementType.None;
    [SerializeField] private List<string> tags = new();

    [Header("Trigger")]
    [SerializeField] private SkillEffectTriggerType triggerType = SkillEffectTriggerType.OnHit;
    [SerializeField] private SkillEffectTargetType targetType = SkillEffectTargetType.Target;

    [Header("Value")]
    [SerializeField] private float value;
    [SerializeField, Min(0f)] private float duration;
    [SerializeField, Range(0f, 1f)] private float chance = 1f;
    [SerializeField, Min(1)] private int maxStack = 1;

    public string EffectId => effectId;
    public string DisplayName => displayName;
    public string Description => description;

    public ElementType ElementType => elementType;
    public IReadOnlyList<string> Tags => tags;

    public SkillEffectTriggerType TriggerType => triggerType;
    public SkillEffectTargetType TargetType => targetType;

    public float Value => value;
    public float Duration => duration;
    public float Chance => chance;
    public int MaxStack => maxStack;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(effectId))
        {
            effectId = name;
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = name;
        }

        if (duration < 0f)
        {
            duration = 0f;
        }

        if (chance < 0f)
        {
            chance = 0f;
        }
        else if (chance > 1f)
        {
            chance = 1f;
        }

        if (maxStack < 1)
        {
            maxStack = 1;
        }
    }
#endif
}