using System.Collections.Generic;
using Effect;
/// <summary>
/// 장비 업그레이드 런타임 변환용 헬퍼.
/// Entry/SO는 순수 데이터로 두고, 런타임 modifier 복사/변환은 여기서 처리한다.
/// </summary>
public static class UpgradeHelper
{
    public static List<SkillStatModifierData> CopyModifiers(
        IReadOnlyList<SkillStatModifierData> source,
        string sourceId = null)
    {
        var result = new List<SkillStatModifierData>();

        if (source == null || source.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < source.Count; i++)
        {
            SkillStatModifierData modifier = source[i];
            if (modifier == null)
            {
                continue;
            }

            result.Add(CopyModifier(modifier, sourceId));
        }

        return result;
    }

    public static SkillStatModifierData CopyModifier(
        SkillStatModifierData source,
        string sourceId = null)
    {
        if (source == null)
        {
            return null;
        }

        SkillStatModifierData copied = new SkillStatModifierData();
        copied.ApplyEditorData(
            source.ModifierType,
            source.OperationType,
            source.Value);

        return copied;
    }

    public static List<EffectUpgradeModifierData> CopyEffectModifiers(
        IReadOnlyList<EffectUpgradeModifierData> source)
    {
        var result = new List<EffectUpgradeModifierData>();

        if (source == null || source.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < source.Count; i++)
        {
            EffectUpgradeModifierData modifier = source[i];
            if (modifier == null)
            {
                continue;
            }

            result.Add(CopyEffectModifier(modifier));
        }

        return result;
    }

    public static EffectUpgradeModifierData CopyEffectModifier(
        EffectUpgradeModifierData source)
    {
        if (source == null)
        {
            return null;
        }

        EffectUpgradeModifierData copied = new EffectUpgradeModifierData();
        copied.ApplyEditorData(
            source.TargetEffectId,
            source.FieldType,
            source.OperationType,
            source.Value);

        return copied;
    }

    public static List<SkillStatModifierData> CollectModifiersUpToLevel(
        IEnumerable<EquipmentUpgradeEntry> entries,
        int currentLevel,
        string sourceId = null)
    {
        var result = new List<SkillStatModifierData>();

        if (entries == null)
        {
            return result;
        }

        foreach (EquipmentUpgradeEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.Level > currentLevel)
            {
                continue;
            }

            result.AddRange(CopyModifiers(entry.StatModifiers, sourceId));
        }

        return result;
    }

    public static List<EffectUpgradeModifierData> CollectEffectModifiersUpToLevel(
        IEnumerable<EquipmentUpgradeEntry> entries,
        int currentLevel)
    {
        var result = new List<EffectUpgradeModifierData>();

        if (entries == null)
        {
            return result;
        }

        foreach (EquipmentUpgradeEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            if (entry.Level > currentLevel)
            {
                continue;
            }

            result.AddRange(CopyEffectModifiers(entry.EffectModifiers));
        }

        return result;
    }
}