

using System.Collections.Generic;

/// <summary>
/// 장비에 장착된 룬 해석 전담 Resolver.
/// 장착된 RuneSO 목록을 Runtime 데이터로 변환하고,
/// modifier / effect 목록을 추출한다.
/// </summary>
public class EquipmentRuneResolver
{
    public RuneRuntimeSetData Resolve(EquipmentSkillInstanceData instanceData)
    {
        if (instanceData == null || instanceData.equippedRunes == null || instanceData.equippedRunes.Count == 0)
        {
            return RuneRuntimeSetData.Empty();
        }

        return RuneRuntimeSetData.FromRunes(instanceData.equippedRunes);
    }

    public List<SkillStatModifierRuntimeData> ExtractModifiers(RuneRuntimeSetData runtimeSet)
    {
        if (runtimeSet == null || runtimeSet.statModifiers == null)
        {
            return new List<SkillStatModifierRuntimeData>();
        }

        return runtimeSet.statModifiers;
    }

    public List<SkillEffectSO> ExtractEffects(RuneRuntimeSetData runtimeSet)
    {
        if (runtimeSet == null || runtimeSet.effects == null)
        {
            return new List<SkillEffectSO>();
        }

        return runtimeSet.effects;
    }

    public EffectRuntimeSetData ResolveEffectRuntimeSet(RuneRuntimeSetData runtimeSet)
    {
        List<SkillEffectSO> effects = ExtractEffects(runtimeSet);
        if (effects == null || effects.Count == 0)
        {
            return EffectRuntimeSetData.Empty();
        }

        return EffectRuntimeSetData.FromEffects(effects);
    }
}