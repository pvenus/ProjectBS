

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업그레이드 테이블에서 사용하는 등급별 정의 데이터 (SO용).
/// 특정 장비 등급에 도달했을 때 어떤 modifier가 적용되는지 정의한다.
/// </summary>
[Serializable]
public class EquipmentUpgradeEntry
{
    [Header("Grade")]
    [SerializeField] private EquipmentGrade grade = EquipmentGrade.Common;

    [Header("Modifiers")]
    [SerializeField] private List<SkillStatModifierRuntimeData> statModifiers = new();

    public EquipmentGrade Grade => grade;
    public IReadOnlyList<SkillStatModifierRuntimeData> StatModifiers => statModifiers;

    public bool HasAnyModifier => statModifiers != null && statModifiers.Count > 0;
}