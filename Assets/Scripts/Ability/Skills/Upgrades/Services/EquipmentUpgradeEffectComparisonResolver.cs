using System.Collections.Generic;
using System.Text;
using Skill;
using Effect;
using UnityEngine;

public class EquipmentUpgradeEffectComparisonResolver
{
    public string BuildComparisonText(
        EquipmentSkillSO skillSo,
        IReadOnlyList<EffectUpgradeModifierData> currentModifiers,
        IReadOnlyList<EffectUpgradeModifierData> nextModifiers)
    {
        List<EffectUpgradeModifierKey> changedKeys = CollectChangedEffectModifierKeys(
            currentModifiers,
            nextModifiers);
        changedKeys.Sort((left, right) => left.CompareTo(right));

        if (changedKeys.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new();

        for (int i = 0; i < changedKeys.Count; i++)
        {
            EffectUpgradeModifierKey key = changedKeys[i];
            if (!TryResolveEffectBaseValue(skillSo, key, out float baseValue))
            {
                EffectUpgradeModifierData raw = FindLatestModifier(key, nextModifiers);
                string rawDelta = raw != null
                    ? FormatRawModifier(raw)
                    : "변경값 확인 불가";
                builder.AppendLine($"효과 연결 오류  {rawDelta}");
                Debug.LogWarning(
                    $"[SkillUpgradePreview] Unsupported effect preview field: "
                    + $"{key.targetEffectId}.{key.fieldType}; raw id is hidden from player UI.");
                continue;
            }

            float currentValue = ResolveEffectModifierValue(
                key,
                baseValue,
                currentModifiers);
            float nextValue = ResolveEffectModifierValue(
                key,
                baseValue,
                nextModifiers);

            if (Mathf.Approximately(currentValue, nextValue))
            {
                continue;
            }

            builder.AppendLine(
                $"{GetEffectModifierDisplayName(skillSo, key)}  "
                + $"{FormatEffectModifierValue(key.fieldType, currentValue)} → "
                + $"{FormatEffectModifierValue(key.fieldType, nextValue)}  "
                + $"({FormatEffectModifierDelta(key.fieldType, nextValue - currentValue)})");
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
                0f,
                currentModifiers);
            float nextValue = ResolveEffectModifierValue(
                key,
                0f,
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
        float baseValue,
        IReadOnlyList<EffectUpgradeModifierData> modifiers)
    {
        float value = baseValue;

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

            value = UpgradeModifierValueEvaluator.Apply(
                value,
                modifier.OperationType,
                modifier.Value);
        }

        return value;
    }

    private static EffectUpgradeModifierData FindLatestModifier(
        EffectUpgradeModifierKey key,
        IReadOnlyList<EffectUpgradeModifierData> modifiers)
    {
        if (modifiers == null) return null;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            EffectUpgradeModifierData modifier = modifiers[i];
            if (modifier != null
                && modifier.TargetEffectId == key.targetEffectId
                && modifier.FieldType == key.fieldType)
                return modifier;
        }
        return null;
    }

    private string GetEffectModifierDisplayName(
        EquipmentSkillSO skillSo,
        EffectUpgradeModifierKey key)
    {
        EffectEntrySO entry = FindEffectEntry(skillSo, key.targetEffectId);
        string effectName = entry?.EffectSO != null
            ? EffectDisplayNameResolver.Resolve(entry.EffectSO)
            : "확인 불가 효과";

        string fieldName;
        switch (key.fieldType)
        {
            case EffectModifierFieldType.Value:
                fieldName = "효과 수치"; break;
            case EffectModifierFieldType.Duration:
                fieldName = "지속시간"; break;
            case EffectModifierFieldType.Chance:
                fieldName = "확률"; break;
            case EffectModifierFieldType.Cooldown:
                fieldName = "재사용"; break;
            case EffectModifierFieldType.MaxApplyCount:
                fieldName = "최대 적용"; break;
            case EffectModifierFieldType.TickInterval:
                fieldName = "발동 간격"; break;
            case EffectModifierFieldType.Radius:
                fieldName = "범위"; break;
            default:
                fieldName = key.fieldType.ToString(); break;
        }
        return $"{effectName} {fieldName}";
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

    private string FormatEffectModifierDelta(EffectModifierFieldType fieldType, float delta)
    {
        string sign = delta >= 0f ? "+" : string.Empty;
        switch (fieldType)
        {
            case EffectModifierFieldType.Duration:
            case EffectModifierFieldType.Cooldown:
            case EffectModifierFieldType.TickInterval:
                return $"{sign}{delta:0.##}초";
            case EffectModifierFieldType.Chance:
                return $"{sign}{Mathf.RoundToInt(delta * 100f)}%";
            case EffectModifierFieldType.MaxApplyCount:
                return $"{sign}{Mathf.RoundToInt(delta)}";
            default:
                return $"{sign}{delta:0.##}";
        }
    }

    private static string FormatRawModifier(EffectUpgradeModifierData modifier)
    {
        string sign = modifier.Value >= 0f ? "+" : string.Empty;
        return $"raw {modifier.OperationType} {sign}{modifier.Value:0.##}";
    }

    private static bool TryResolveEffectBaseValue(
        EquipmentSkillSO skillSo,
        EffectUpgradeModifierKey key,
        out float value)
    {
        value = 0f;
        EffectEntrySO entry = FindEffectEntry(skillSo, key.targetEffectId);
        if (entry == null) return false;

        switch (key.fieldType)
        {
            case EffectModifierFieldType.Value:
                if (entry.HasValueOverride)
                {
                    value = entry.ValueOverride;
                    return true;
                }
                if (entry.EffectSO?.Config is StatModifierEffectConfig config)
                {
                    value = config.Value;
                    return true;
                }
                return false;
            case EffectModifierFieldType.Duration:
                value = entry.Duration;
                return true;
            case EffectModifierFieldType.MaxApplyCount:
                value = entry.MaxApplyCount;
                return true;
            default:
                return false;
        }
    }

    private static EffectEntrySO FindEffectEntry(EquipmentSkillSO skillSo, string effectId)
    {
        if (skillSo == null || string.IsNullOrWhiteSpace(effectId)) return null;
        EffectEntrySO found = FindInEntries(skillSo.CastSo?.SelfEffects, effectId)
            ?? FindInEntries(skillSo.CastSo?.PostMoveSelfEffects, effectId);
        if (found != null) return found;

        SkillHitSO[] hits = skillSo.HitSos;
        if (hits == null) return null;
        for (int i = 0; i < hits.Length; i++)
        {
            found = FindInEntries(hits[i]?.BuffEffects, effectId)
                ?? FindInEntries(hits[i]?.DebuffEffects, effectId);
            if (found != null) return found;
        }
        return null;
    }

    private static EffectEntrySO FindInEntries(EffectEntrySO[] entries, string effectId)
    {
        if (entries == null) return null;
        for (int i = 0; i < entries.Length; i++)
        {
            EffectEntrySO entry = entries[i];
            if (entry?.EffectSO != null && entry.EffectSO.EffectId == effectId)
                return entry;
        }
        return null;
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

        public int CompareTo(EffectUpgradeModifierKey other)
        {
            int id = string.CompareOrdinal(targetEffectId, other.targetEffectId);
            return id != 0 ? id : ((int)fieldType).CompareTo((int)other.fieldType);
        }
    }
}
