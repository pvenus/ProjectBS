

using System;
using UnityEngine;

/// <summary>
/// 스탯 수정 연산 타입
/// </summary>
public enum SkillStatModifierOperationType
{
    Add = 0,
    Multiply = 1,
    Override = 2
}

/// <summary>
/// 수정 대상 스탯 타입
/// </summary>
public enum SkillStatModifierType
{
    Damage = 0,
    Cooldown = 1,
    Range = 2,
    ProjectileCount = 3,
    ProjectileScale = 4,
    Lifetime = 5
}

/// <summary>
/// 런타임에서 사용되는 스탯 수정 데이터
/// 장비/강화/룬에서 수집된 modifier를 최종 계산에 사용하기 위한 구조
/// </summary>
[Serializable]
public class SkillStatModifierRuntimeData
{
    [Header("Target")]
    public SkillStatModifierType modifierType = SkillStatModifierType.Damage;

    [Header("Operation")]
    public SkillStatModifierOperationType operationType = SkillStatModifierOperationType.Add;

    [Header("Value")]
    public float value;

    [Header("Source")]
    public string sourceId;

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
            value = val,
            sourceId = source
        };
    }

    public float Apply(float baseValue)
    {
        switch (operationType)
        {
            case SkillStatModifierOperationType.Add:
                return baseValue + value;

            case SkillStatModifierOperationType.Multiply:
                return baseValue * (1f + value);

            case SkillStatModifierOperationType.Override:
                return value;

            default:
                return baseValue;
        }
    }
}