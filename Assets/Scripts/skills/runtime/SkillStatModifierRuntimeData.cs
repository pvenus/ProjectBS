

using System;
using UnityEngine;
using Skill;
/// <summary>
/// 스탯 수정 연산 타입
/// </summary>


/// <summary>
/// 런타임에서 사용되는 스탯 수정 데이터
/// 장비/강화/룬에서 수집된 modifier를 최종 계산에 사용하기 위한 구조
/// </summary>
[Serializable]
public class SkillStatModifierRuntimeData
{
    [Header("Target")]
    public SkillStatModifierType modifierType = SkillStatModifierType.BaseDamage;

    [Header("Operation")]
    public SkillStatModifierOperationType operationType = SkillStatModifierOperationType.Flat;

    [Header("Value")]
    public float value;


    public static SkillStatModifierRuntimeData Create(
        SkillStatModifierType type,
        SkillStatModifierOperationType op,
        float val,
        string source = null)
    {
        return new SkillStatModifierRuntimeData
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