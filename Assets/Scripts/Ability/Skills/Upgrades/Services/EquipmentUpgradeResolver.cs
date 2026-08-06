using System.Collections.Generic;
using Skill;
using Effect;
/// <summary>
/// 장비 업그레이드 해석 전담 Resolver.
/// EquipmentSkillSO의 업그레이드 테이블과 인스턴스 레벨을 조합해
/// 현재 적용 가능한 레벨별 업그레이드 Runtime 데이터를 생성한다.
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

        int level = ResolveLevel(instanceData);

        return EquipmentUpgradeRuntimeData.FromEntries(
            level,
            equipmentSo.UpgradeTableSo.Entries,
            equipmentSo.EquipmentId);
    }

    private static int ResolveLevel(EquipmentSkillInstanceData instanceData)
    {
        if (instanceData == null)
        {
            return 1;
        }

        return instanceData.currentLevel <= 0
            ? 1
            : instanceData.currentLevel;
    }

    public List<SkillStatModifierData> ExtractModifiers(
        EquipmentUpgradeRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.statModifiers == null)
        {
            return new List<SkillStatModifierData>();
        }

        return runtimeData.statModifiers;
    }

    public List<EffectUpgradeModifierData> ExtractEffectModifiers(
        EquipmentUpgradeRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.effectModifiers == null)
        {
            return new List<EffectUpgradeModifierData>();
        }

        return runtimeData.effectModifiers;
    }
}