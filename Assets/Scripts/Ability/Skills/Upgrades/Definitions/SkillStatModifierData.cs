

using System;
using UnityEngine;
using Skill;
/// <summary>
/// 스탯 수정 연산 타입
/// </summary>


/// <summary>
/// 스킬 업그레이드/강화에서 사용하는 스탯 수정 데이터.
/// SO와 JSON에서 직렬화되는 modifier 정의에 가깝다.
/// </summary>
[Serializable]
public class SkillStatModifierData
{
    [SerializeField] private SkillStatModifierType modifierType = SkillStatModifierType.BaseDamage;
    [SerializeField] private SkillStatModifierOperationType operationType = SkillStatModifierOperationType.Flat;
    [SerializeField] private float value;

    public SkillStatModifierType ModifierType => modifierType;
    public SkillStatModifierOperationType OperationType => operationType;
    public float Value => value;

    #if UNITY_EDITOR
    public void ApplyEditorData(
        SkillStatModifierType modifierType,
        SkillStatModifierOperationType operationType,
        float value)
    {
        this.modifierType = modifierType;
        this.operationType = operationType;
        this.value = value;
    }
    #endif

    public float Apply(float baseValue)
    {
        switch (operationType)
        {
            case SkillStatModifierOperationType.Flat:
                return baseValue + value;
            case SkillStatModifierOperationType.Percent:
                return baseValue * (1f + value);
            case SkillStatModifierOperationType.Override:
                return value;
            default:
                return baseValue;
        }
    }
}