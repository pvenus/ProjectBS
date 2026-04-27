using System.Collections.Generic;

/// <summary>
/// 장비 업그레이드 런타임 변환용 헬퍼.
/// Entry/SO는 순수 데이터로 두고, 런타임 modifier 복사/변환은 여기서 처리한다.
/// </summary>
public static class UpgradeHelper
{
    public static List<SkillStatModifierRuntimeData> CopyModifiers(
        IReadOnlyList<SkillStatModifierRuntimeData> source,
        string sourceId = null)
    {
        var result = new List<SkillStatModifierRuntimeData>();

        if (source == null || source.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < source.Count; i++)
        {
            SkillStatModifierRuntimeData modifier = source[i];
            if (modifier == null)
            {
                continue;
            }

            result.Add(CopyModifier(modifier, sourceId));
        }

        return result;
    }

    public static SkillStatModifierRuntimeData CopyModifier(
        SkillStatModifierRuntimeData source,
        string sourceId = null)
    {
        if (source == null)
        {
            return null;
        }

        return SkillStatModifierRuntimeData.Create(
            source.modifierType,
            source.operationType,
            source.value,
            string.IsNullOrWhiteSpace(sourceId) ? source.sourceId : sourceId);
    }

    public static List<SkillStatModifierRuntimeData> CollectModifiersUpToGrade(
        IEnumerable<EquipmentUpgradeEntry> entries,
        EquipmentGrade currentGrade,
        string sourceId = null)
    {
        var result = new List<SkillStatModifierRuntimeData>();

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

            if (entry.Grade > currentGrade)
            {
                continue;
            }

            result.AddRange(CopyModifiers(entry.StatModifiers, sourceId));
        }

        return result;
    }
}