using System;
using System.Collections.Generic;
using UnityEngine;
using Effect;
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
    [SerializeField] private List<SkillStatModifierData> statModifiers = new();

    [SerializeField] private List<EffectUpgradeModifierData> effectModifiers = new();

    public int Level => level;
    public IReadOnlyList<SkillStatModifierData> StatModifiers => statModifiers;

    public IReadOnlyList<EffectUpgradeModifierData> EffectModifiers => effectModifiers;

    public bool HasAnyModifier =>
        (statModifiers != null && statModifiers.Count > 0) ||
        (effectModifiers != null && effectModifiers.Count > 0);

#if UNITY_EDITOR
    public void ApplyEditorData(
        int level,
        List<SkillStatModifierData> statModifiers,
        List<EffectUpgradeModifierData> effectModifiers)
    {
        this.level = Mathf.Max(1, level);
        this.statModifiers = statModifiers ?? new List<SkillStatModifierData>();
        this.effectModifiers = effectModifiers ?? new List<EffectUpgradeModifierData>();
    }
#endif
}