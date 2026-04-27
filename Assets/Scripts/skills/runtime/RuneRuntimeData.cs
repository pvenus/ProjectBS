using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 룬 하나를 런타임에서 해석한 결과 데이터.
/// 룬 원형, 속성, 수치 보정, Effect 목록을 함께 보관한다.
/// </summary>
[Serializable]
public class RuneRuntimeData
{
    [Header("Source")]
    public RuneSO sourceRune;
    public string runeId;
    public string displayName;

    [Header("Identity")]
    public ElementType elementType = ElementType.None;
    public EquipmentGrade grade = EquipmentGrade.Common;

    [Header("Resolved Modifiers")]
    public List<SkillStatModifierRuntimeData> statModifiers = new();

    [Header("Resolved Effects")]
    public List<SkillEffectSO> effects = new();

    public bool HasRune => sourceRune != null;
    public bool HasAnyModifier => statModifiers != null && statModifiers.Count > 0;
    public bool HasAnyEffect => effects != null && effects.Count > 0;

    public static RuneRuntimeData Empty()
    {
        return new RuneRuntimeData();
    }

    public static RuneRuntimeData FromRune(RuneSO rune)
    {
        var runtime = new RuneRuntimeData();
        runtime.ApplyRune(rune);
        return runtime;
    }

    public void ApplyRune(RuneSO rune)
    {
        sourceRune = rune;

        if (rune == null)
        {
            runeId = string.Empty;
            displayName = string.Empty;
            elementType = ElementType.None;
            grade = EquipmentGrade.Common;
            statModifiers.Clear();
            effects.Clear();
            return;
        }

        runeId = rune.RuneId;
        displayName = rune.DisplayName;
        elementType = rune.ElementType;
        grade = rune.Grade;

        statModifiers = rune.StatModifiers != null
            ? new List<SkillStatModifierRuntimeData>(rune.StatModifiers)
            : new List<SkillStatModifierRuntimeData>();

        effects = rune.Effects != null
            ? new List<SkillEffectSO>(rune.Effects)
            : new List<SkillEffectSO>();
    }
}
/// <summary>
/// 장착된 여러 룬을 런타임에서 해석한 결과 묶음.
/// Resolver는 이 데이터를 기반으로 최종 스탯과 Effect를 조립한다.
/// </summary>
[Serializable]
public class RuneRuntimeSetData
{
    [Header("Runes")]
    public List<RuneRuntimeData> runes = new();

    [Header("Flattened Result")]
    public List<SkillStatModifierRuntimeData> statModifiers = new();
    public List<SkillEffectSO> effects = new();

    public bool HasAnyRune => runes != null && runes.Count > 0;
    public bool HasAnyModifier => statModifiers != null && statModifiers.Count > 0;
    public bool HasAnyEffect => effects != null && effects.Count > 0;

    public static RuneRuntimeSetData Empty()
    {
        return new RuneRuntimeSetData();
    }

    public static RuneRuntimeSetData FromRunes(IEnumerable<RuneSO> sourceRunes)
    {
        var set = new RuneRuntimeSetData();
        set.ApplyRunes(sourceRunes);
        return set;
    }

    public void ApplyRunes(IEnumerable<RuneSO> sourceRunes)
    {
        runes.Clear();
        statModifiers.Clear();
        effects.Clear();

        if (sourceRunes == null)
        {
            return;
        }

        foreach (RuneSO rune in sourceRunes)
        {
            if (rune == null)
            {
                continue;
            }

            RuneRuntimeData runeRuntime = RuneRuntimeData.FromRune(rune);
            runes.Add(runeRuntime);

            if (runeRuntime.statModifiers != null)
            {
                statModifiers.AddRange(runeRuntime.statModifiers);
            }

            if (runeRuntime.effects != null)
            {
                effects.AddRange(runeRuntime.effects);
            }
        }
    }
}