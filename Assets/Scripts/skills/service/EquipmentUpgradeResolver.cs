using System.Collections.Generic;
using Skill;
/// <summary>
/// 장비 업그레이드 해석 전담 Resolver.
/// 현재 등급 기반 업그레이드 로직은 제거되었으므로 빈 Runtime 데이터를 반환한다.
/// </summary>
public class EquipmentUpgradeResolver
{
    public EquipmentUpgradeRuntimeData Resolve(
        EquipmentSkillSO equipmentSo,
        EquipmentSkillInstanceData instanceData)
    {
        return EquipmentUpgradeRuntimeData.Empty();
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