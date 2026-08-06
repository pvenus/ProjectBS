using System;
using System.Collections.Generic;
using UnityEngine;
using Effect;
/// <summary>
/// 현재 장비의 레벨별 업그레이드 상태를 런타임에서 해석한 결과 데이터.
/// Resolver는 이 데이터를 기반으로 장비 기본 성능에 레벨별 modifier를 적용한다.
/// </summary>
[Serializable]
public class EquipmentUpgradeRuntimeData
{
    [Header("State")]
    public int currentLevel;

    [Header("Level Upgrade Infos")]
    public List<EquipmentLevelUpgradeRuntimeData> levelUpgradeInfos = new();

    [Header("Resolved Modifiers")]
    public List<SkillStatModifierData> statModifiers = new();

    public List<EffectUpgradeModifierData> effectModifiers = new();

    public bool HasAnyModifier =>
        (statModifiers != null && statModifiers.Count > 0) ||
        (effectModifiers != null && effectModifiers.Count > 0);

    public static EquipmentUpgradeRuntimeData Empty()
    {
        return new EquipmentUpgradeRuntimeData();
    }

    public static EquipmentUpgradeRuntimeData FromEntries(
        int level,
        IEnumerable<EquipmentUpgradeEntry> entries,
        string sourceId = null)
    {
        var runtime = new EquipmentUpgradeRuntimeData
        {
            currentLevel = level
        };

        runtime.ApplyEntries(entries, sourceId);
        return runtime;
    }

    public void ApplyEntries(IEnumerable<EquipmentUpgradeEntry> entries, string sourceId = null)
    {
        levelUpgradeInfos.Clear();
        statModifiers.Clear();
        effectModifiers.Clear();

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

            int entryLevel = entry.Level;
            List<SkillStatModifierData> copiedStatModifiers =
                UpgradeHelper.CopyModifiers(entry.StatModifiers, sourceId);

            List<EffectUpgradeModifierData> copiedEffectModifiers =
                UpgradeHelper.CopyEffectModifiers(entry.EffectModifiers);

            levelUpgradeInfos.Add(
                new EquipmentLevelUpgradeRuntimeData
                {
                    level = entryLevel,
                    statModifiers = copiedStatModifiers,
                    effectModifiers = copiedEffectModifiers
                });

            if (entryLevel > currentLevel)
            {
                continue;
            }

            statModifiers.AddRange(copiedStatModifiers);
            effectModifiers.AddRange(copiedEffectModifiers);
        }
    }
}

[Serializable]
public class EquipmentLevelUpgradeRuntimeData
{
    public int level;
    public List<SkillStatModifierData> statModifiers = new();
    public List<EffectUpgradeModifierData> effectModifiers = new();
}