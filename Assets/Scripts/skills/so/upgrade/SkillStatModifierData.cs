

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
    [Header("Target")]
    public SkillStatModifierType modifierType = SkillStatModifierType.BaseDamage;

    [Header("Operation")]
    public SkillStatModifierOperationType operationType = SkillStatModifierOperationType.Flat;

    [Header("Value")]
    public float value;


    public static SkillStatModifierData Create(
        SkillStatModifierType type,
        SkillStatModifierOperationType op,
        float val,
        string source = null)
    {
        return new SkillStatModifierData
        {
            modifierType = type,
            operationType = op,
            value = val
        };
    }

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