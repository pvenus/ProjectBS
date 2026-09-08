using System.Collections.Generic;
using System;
using System.Text;
using Skill;
using String;
using UnityEngine;
public class EquipmentUpgradeStatComparisonResolver
{
    private readonly EquipmentStatResolver statResolver = new();

    public string BuildComparisonText(
        EquipmentSkillSO skillSo,
        IReadOnlyList<SkillStatModifierData> currentModifiers,
        IReadOnlyList<SkillStatModifierData> nextModifiers)
    {
        if (skillSo == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new();
        List<SkillStatModifierType> changedTypes = CollectChangedModifierTypes(
            skillSo,
            currentModifiers,
            nextModifiers);

        for (int i = 0; i < changedTypes.Count; i++)
        {
            SkillStatModifierType modifierType = changedTypes[i];
            float currentValue = statResolver.ResolveStat(
                skillSo,
                modifierType,
                currentModifiers);
            float nextValue = statResolver.ResolveStat(
                skillSo,
                modifierType,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                continue;
            }

            builder.AppendLine(
                $"{GetModifierDisplayName(modifierType)}  "
                + $"{FormatModifierValue(modifierType, currentValue)} → "
                + $"{FormatModifierValue(modifierType, nextValue)}  "
                + $"({FormatDelta(modifierType, nextValue - currentValue)})");
        }

        return builder.ToString();
    }

    private List<SkillStatModifierType> CollectChangedModifierTypes(
        EquipmentSkillSO skillSo,
        IReadOnlyList<SkillStatModifierData> currentModifiers,
        IReadOnlyList<SkillStatModifierData> nextModifiers)
    {
        List<SkillStatModifierType> result = new();
        AddModifierTypes(currentModifiers, result);
        AddModifierTypes(nextModifiers, result);
        result.Sort((left, right) => ((int)left).CompareTo((int)right));
        for (int i = result.Count - 1; i >= 0; i--)
        {
            SkillStatModifierType type = result[i];
            float before = statResolver.ResolveStat(skillSo, type, currentModifiers);
            float after = statResolver.ResolveStat(skillSo, type, nextModifiers);
            if (Mathf.Approximately(before, after)) result.RemoveAt(i);
        }
        return result;
    }

    private static void AddModifierTypes(
        IReadOnlyList<SkillStatModifierData> modifiers,
        List<SkillStatModifierType> result)
    {
        if (modifiers == null) return;
        for (int i = 0; i < modifiers.Count; i++)
        {
            SkillStatModifierData modifier = modifiers[i];
            if (modifier != null && !result.Contains(modifier.ModifierType))
                result.Add(modifier.ModifierType);
        }
    }

    private string GetModifierDisplayName(SkillStatModifierType modifierType)
    {
        return StringManager.Instance != null
            ? StringManager.Instance.Get(
                $"enum.{nameof(SkillStatModifierType)}.{modifierType}",
                "name")
            : modifierType.ToString();
    }

    private string FormatModifierValue(
        SkillStatModifierType modifierType,
        float value)
    {
        switch (modifierType)
        {
            case SkillStatModifierType.AttackPercentDamage:
                return $"{Mathf.RoundToInt(value * 100f)}%";
            case SkillStatModifierType.Cooldown:
                return $"{value:0.##}초";
            case SkillStatModifierType.Range:
            case SkillStatModifierType.ProjectileScale:
                return value.ToString("0.##");
            case SkillStatModifierType.SplitHitCount:
            case SkillStatModifierType.MaxHitCount:
            case SkillStatModifierType.ProjectileCount:
                return Mathf.RoundToInt(value).ToString();
            default:
                return value.ToString("0.##");
        }
    }

    private string FormatDelta(SkillStatModifierType modifierType, float delta)
    {
        string sign = delta >= 0f ? "+" : string.Empty;
        switch (modifierType)
        {
            case SkillStatModifierType.AttackPercentDamage:
                return $"{sign}{Mathf.RoundToInt(delta * 100f)}%";
            case SkillStatModifierType.Cooldown:
            case SkillStatModifierType.Lifetime:
            case SkillStatModifierType.ProjectileSpawnInterval:
                return $"{sign}{delta:0.##}초";
            case SkillStatModifierType.SplitHitCount:
            case SkillStatModifierType.MaxHitCount:
            case SkillStatModifierType.ProjectileCount:
                return $"{sign}{Mathf.RoundToInt(delta)}";
            default:
                return $"{sign}{delta:0.##}";
        }
    }
}
