using System;
using System.Collections.Generic;
using UnityEngine;
using Skill;
/// <summary>
/// 업그레이드 테이블에서 사용하는 레벨별 정의 데이터 (SO용).
/// 특정 장비 레벨에 도달했을 때 어떤 modifier가 적용되는지 정의한다.
/// </summary>
[Serializable]
public class EquipmentUpgradeEntry
{
    [Header("Level")]
    [SerializeField] private int level = 1;

    [Header("Modifiers")]
    [SerializeField] private List<SkillStatModifierRuntimeData> statModifiers = new();

    public int Level => level;
    public IReadOnlyList<SkillStatModifierRuntimeData> StatModifiers => statModifiers;

    public bool HasAnyModifier => statModifiers != null && statModifiers.Count > 0;
}