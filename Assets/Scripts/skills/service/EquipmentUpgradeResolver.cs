

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 등급 업그레이드 해석 전담 Resolver.
/// SO의 업그레이드 테이블과 현재 등급을 기반으로 Runtime 데이터를 생성한다.
/// </summary>
public class EquipmentUpgradeResolver
{
    public EquipmentUpgradeRuntimeData Resolve(
        EquipmentSkillSO equipmentSo,
        EquipmentSkillInstanceData instanceData)
    {
        if (equipmentSo == null || equipmentSo.UpgradeTableSo == null)
        {
            return EquipmentUpgradeRuntimeData.Empty();
        }

        EquipmentGrade grade = instanceData != null
            ? instanceData.currentGrade
            : equipmentSo.BaseProfileSo != null ? equipmentSo.BaseProfileSo.BaseGrade : EquipmentGrade.Common;

        List<EquipmentUpgradeEntry> entries = equipmentSo.UpgradeTableSo.GetEntriesUpToGrade(grade);

        return EquipmentUpgradeRuntimeData.FromEntries(
            grade,
            entries,
            equipmentSo.UpgradeTableSo.UpgradeTableId);
    }

    public List<SkillStatModifierRuntimeData> ExtractModifiers(
        EquipmentUpgradeRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.statModifiers == null)
        {
            return new List<SkillStatModifierRuntimeData>();
        }

        return runtimeData.statModifiers;
    }
}