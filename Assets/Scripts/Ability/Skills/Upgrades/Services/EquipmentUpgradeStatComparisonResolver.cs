using System.Collections.Generic;
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
                $"{GetModifierDisplayName(modifierType)} {FormatModifierValue(modifierType, currentValue)} → {FormatModifierValue(modifierType, nextValue)}");
        }

        return builder.ToString();
    }

    private List<SkillStatModifierType> CollectChangedModifierTypes(
        EquipmentSkillSO skillSo,
        IReadOnlyList<SkillStatModifierData> currentModifiers,
        IReadOnlyList<SkillStatModifierData> nextModifiers)
    {
        List<SkillStatModifierType> result = new();
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.BaseDamage, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.AttackPercentDamage, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.Cooldown, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.Range, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.SplitHitCount, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.MaxHitCount, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.ProjectileCount, currentModifiers, nextModifiers, result);
        AddModifierTypeIfChanged(skillSo, SkillStatModifierType.ProjectileScale, currentModifiers, nextModifiers, result);
        return result;
    }

    private void AddModifierTypeIfChanged(
        EquipmentSkillSO skillSo,
        SkillStatModifierType modifierType,
        IReadOnlyList<SkillStatModifierData> currentModifiers,
        IReadOnlyList<SkillStatModifierData> nextModifiers,
        List<SkillStatModifierType> result)
    {
        float currentValue = statResolver.ResolveStat(
            skillSo,
            modifierType,
            currentModifiers);
        float nextValue = statResolver.ResolveStat(
            skillSo,
            modifierType,
            nextModifiers);

        if (!Mathf.Approximately(currentValue, nextValue))
        {
            result.Add(modifierType);
        }
    }

    private string GetModifierDisplayName(SkillStatModifierType modifierType)
    {
        return StringManager.Instance.Get(
            $"enum.{nameof(SkillStatModifierType)}.{modifierType}",
            "name");
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
}
