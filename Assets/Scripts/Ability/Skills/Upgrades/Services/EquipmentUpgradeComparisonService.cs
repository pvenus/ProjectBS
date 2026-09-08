using System.Collections.Generic;
using System.Text;
using Skill;
using String;
using Effect;
using UnityEngine;

public class EquipmentUpgradeComparisonService
{
    private const int MaxVisibleChangeRows = 3;
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
        builder.AppendLine($"[{ResolveSkillLabel(skillSo)}]");

        string statText = statResolver.BuildComparisonText(
            skillSo,
            currentData?.statModifiers,
            nextData?.statModifiers);

        if (!string.IsNullOrWhiteSpace(statText))
        {
            builder.Append(statText);
        }

        string effectText = effectResolver.BuildComparisonText(
            skillSo,
            currentData?.effectModifiers,
            nextData?.effectModifiers);

        if (!string.IsNullOrWhiteSpace(effectText))
        {
            builder.Append(effectText);
        }

        return BoundForCard(builder.ToString());
    }

    private static string BoundForCard(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        string[] rows = value.Split(
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries);
        int detailCount = Mathf.Max(0, rows.Length - 1);
        if (detailCount <= MaxVisibleChangeRows) return value.TrimEnd();

        StringBuilder bounded = new();
        bounded.AppendLine(rows[0]);
        for (int i = 1; i <= MaxVisibleChangeRows; i++)
            bounded.AppendLine(rows[i]);
        bounded.Append($"외 {detailCount - MaxVisibleChangeRows}개");
        return bounded.ToString();
    }

    private static string ResolveSkillLabel(EquipmentSkillSO skillSo)
    {
        if (skillSo == null) return "스킬";
        string displayName = StringManager.Instance != null
            ? skillSo.DisplayName
            : string.Empty;
        return string.IsNullOrWhiteSpace(displayName)
            ? "스킬"
            : displayName;
    }
}
