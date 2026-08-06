using System.Collections.Generic;
using System.Text;
using Skill;
using Effect;
using UnityEngine;

public class EquipmentUpgradeEffectComparisonResolver
{
    public string BuildComparisonText(
        IReadOnlyList<EffectUpgradeModifierData> currentModifiers,
        IReadOnlyList<EffectUpgradeModifierData> nextModifiers)
    {
        List<EffectUpgradeModifierKey> changedKeys = CollectChangedEffectModifierKeys(
            currentModifiers,
            nextModifiers);

        if (changedKeys.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();

        for (int i = 0; i < changedKeys.Count; i++)
        {
            EffectUpgradeModifierKey key = changedKeys[i];
            float currentValue = ResolveEffectModifierValue(
                key,
                currentModifiers);
            float nextValue = ResolveEffectModifierValue(
                key,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                continue;
            }

            builder.AppendLine(
                $"{GetEffectModifierDisplayName(key)} {FormatEffectModifierValue(key.fieldType, currentValue)} → {FormatEffectModifierValue(key.fieldType, nextValue)}");
        }

        return builder.ToString();
    }

    private List<EffectUpgradeModifierKey> CollectChangedEffectModifierKeys(
        IReadOnlyList<EffectUpgradeModifierData> currentModifiers,
        IReadOnlyList<EffectUpgradeModifierData> nextModifiers)
    {
        List<EffectUpgradeModifierKey> result = new();
        AddEffectModifierKeys(currentModifiers, result);
        AddEffectModifierKeys(nextModifiers, result);

        for (int i = result.Count - 1; i >= 0; i--)
        {
            EffectUpgradeModifierKey key = result[i];
            float currentValue = ResolveEffectModifierValue(
                key,
                currentModifiers);
            float nextValue = ResolveEffectModifierValue(
                key,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                result.RemoveAt(i);
            }
        }

        return result;
    }

    private void AddEffectModifierKeys(
        IReadOnlyList<EffectUpgradeModifierData> modifiers,
        List<EffectUpgradeModifierKey> result)
    {
        if (modifiers == null || result == null)
        {
            return;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            EffectUpgradeModifierData modifier = modifiers[i];
            if (modifier == null)
            {
                continue;
            }

            EffectUpgradeModifierKey key = new EffectUpgradeModifierKey(
                modifier.TargetEffectId,
                modifier.FieldType);

            if (!ContainsEffectModifierKey(result, key))
            {
                result.Add(key);
            }
        }
    }

    private bool ContainsEffectModifierKey(
        List<EffectUpgradeModifierKey> keys,
        EffectUpgradeModifierKey target)
    {
        if (keys == null)
        {
            return false;
        }

        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i].Equals(target))
            {
                return true;
            }
        }

        return false;
    }

    private float ResolveEffectModifierValue(
        EffectUpgradeModifierKey key,
        IReadOnlyList<EffectUpgradeModifierData> modifiers)
    {
        float value = 0f;

        if (modifiers == null)
        {
            return value;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            EffectUpgradeModifierData modifier = modifiers[i];
            if (modifier == null ||
                !string.Equals(modifier.TargetEffectId, key.targetEffectId) ||
                modifier.FieldType != key.fieldType)
            {
                continue;
            }

            value = ApplyEffectModifierValue(
                value,
                modifier);
        }

        return value;
    }

    private float ApplyEffectModifierValue(
        float currentValue,
        EffectUpgradeModifierData modifier)
    {
        switch (modifier.OperationType)
        {
            case SkillStatModifierOperationType.Flat:
                return currentValue + modifier.Value;
            case SkillStatModifierOperationType.Percent:
                return currentValue * (1f + modifier.Value);
            case SkillStatModifierOperationType.Override:
                return modifier.Value;
            default:
                return currentValue;
        }
    }

    private string GetEffectModifierDisplayName(
        EffectUpgradeModifierKey key)
    {
        switch (key.fieldType)
        {
            case EffectModifierFieldType.Value:
                return "Effect Value";
            case EffectModifierFieldType.Duration:
                return "Duration";
            case EffectModifierFieldType.Chance:
                return "Chance";
            case EffectModifierFieldType.Cooldown:
                return "Cooldown";
            case EffectModifierFieldType.MaxApplyCount:
                return "Max Apply Count";
            case EffectModifierFieldType.TickInterval:
                return "Tick Interval";
            case EffectModifierFieldType.Radius:
                return "Radius";
            default:
                return "Effect";
        }
    }

    private string FormatEffectModifierValue(
        EffectModifierFieldType fieldType,
        float value)
    {
        switch (fieldType)
        {
            case EffectModifierFieldType.Duration:
            case EffectModifierFieldType.Cooldown:
            case EffectModifierFieldType.TickInterval:
                return $"{value:0.##}초";
            case EffectModifierFieldType.Chance:
                return $"{Mathf.RoundToInt(value * 100f)}%";
            case EffectModifierFieldType.MaxApplyCount:
                return Mathf.RoundToInt(value).ToString();
            default:
                return value.ToString("0.##");
        }
    }

    private readonly struct EffectUpgradeModifierKey
    {
        public EffectUpgradeModifierKey(
            string targetEffectId,
            EffectModifierFieldType fieldType)
        {
            this.targetEffectId = targetEffectId ?? string.Empty;
            this.fieldType = fieldType;
        }

        public readonly string targetEffectId;
        public readonly EffectModifierFieldType fieldType;

        public bool Equals(EffectUpgradeModifierKey other)
        {
            return targetEffectId == other.targetEffectId &&
                   fieldType == other.fieldType;
        }
    }
}