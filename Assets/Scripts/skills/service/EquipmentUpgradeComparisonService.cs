using System.Collections.Generic;
using System.Text;
using Skill;
using String;
using Effect;
using UnityEngine;

public class EquipmentUpgradeComparisonService
{
    private readonly EquipmentUpgradeStatComparisonResolver statResolver = new();
    private readonly EquipmentUpgradeEffectComparisonResolver effectResolver = new();

    public string BuildComparisonText(
        EquipmentSkillSO skillSo,
        int currentLevel,
        int nextLevel)
    {
        if (skillSo == null || skillSo.UpgradeTableSo == null)
        {
            return string.Empty;
        }

        EquipmentUpgradeRuntimeData currentData =
            EquipmentUpgradeRuntimeData.FromEntries(
                currentLevel,
                skillSo.UpgradeTableSo.Entries,
                skillSo.EquipmentId);

        EquipmentUpgradeRuntimeData nextData =
            EquipmentUpgradeRuntimeData.FromEntries(
                nextLevel,
                skillSo.UpgradeTableSo.Entries,
                skillSo.EquipmentId);

        StringBuilder builder = new();

        string statText = statResolver.BuildComparisonText(
            skillSo,
            currentData?.statModifiers,
            nextData?.statModifiers);

        if (!string.IsNullOrWhiteSpace(statText))
        {
            builder.Append(statText);
        }

        string effectText = effectResolver.BuildComparisonText(
            currentData?.effectModifiers,
            nextData?.effectModifiers);

        if (!string.IsNullOrWhiteSpace(effectText))
        {
            builder.Append(effectText);
        }

        return builder.ToString();
    }
}
