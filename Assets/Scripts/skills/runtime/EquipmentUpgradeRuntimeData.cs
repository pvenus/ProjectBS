using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 현재 장비의 등급 상태를 런타임에서 해석한 결과 데이터.
/// Resolver는 이 데이터를 기반으로 장비 기본 성능에 등급별 modifier를 적용한다.
/// </summary>
[Serializable]
public class EquipmentUpgradeRuntimeData
{
    [Header("State")]
    public EquipmentGrade currentGrade;

    [Header("Resolved Modifiers")]
    public List<SkillStatModifierRuntimeData> statModifiers = new();

    public bool HasAnyModifier => statModifiers != null && statModifiers.Count > 0;

    public static EquipmentUpgradeRuntimeData Empty()
    {
        return new EquipmentUpgradeRuntimeData();
    }

    public static EquipmentUpgradeRuntimeData FromEntries(
        EquipmentGrade grade,
        IEnumerable<EquipmentUpgradeEntry> entries,
        string sourceId = null)
    {
        var runtime = new EquipmentUpgradeRuntimeData
        {
            currentGrade = grade
        };

        runtime.ApplyEntries(entries, sourceId);
        return runtime;
    }

    public void ApplyEntries(IEnumerable<EquipmentUpgradeEntry> entries, string sourceId = null)
    {
        statModifiers.Clear();

        if (entries == null)
        {
            return;
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

            statModifiers.AddRange(
                UpgradeHelper.CopyModifiers(entry.StatModifiers, sourceId)
            );
        }
    }
}