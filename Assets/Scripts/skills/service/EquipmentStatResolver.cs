using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 스킬의 수치 계산 전담 Resolver.
/// 장비 기본값 + 룬 modifier + 강화 modifier를 조합해 최종 런타임 수치를 계산한다.
/// </summary>
public class EquipmentStatResolver
{
    public List<SkillStatModifierRuntimeData> CombineStatModifiers(
        IEnumerable<SkillStatModifierRuntimeData> first,
        IEnumerable<SkillStatModifierRuntimeData> second)
    {
        var result = new List<SkillStatModifierRuntimeData>();

        AddModifiers(result, first);
        AddModifiers(result, second);

        return result;
    }

    public float ApplyStatModifiers(
        float baseValue,
        SkillStatModifierType modifierType,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        float value = baseValue;

        if (modifiers == null)
        {
            return value;
        }

        foreach (SkillStatModifierRuntimeData modifier in modifiers)
        {
            if (modifier == null || modifier.modifierType != modifierType)
            {
                continue;
            }

            value = modifier.Apply(value);
        }

        return value;
    }

    public int ResolveProjectileCount(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        int baseValue = GetProjectileCount(equipmentSo);
        float resolved = ApplyStatModifiers(
            baseValue,
            SkillStatModifierType.ProjectileCount,
            modifiers);

        return Mathf.Max(1, Mathf.RoundToInt(resolved));
    }

    public float ResolveProjectileScale(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        float baseValue = GetProjectileScale(equipmentSo);
        float resolved = ApplyStatModifiers(
            baseValue,
            SkillStatModifierType.ProjectileScale,
            modifiers);

        return Mathf.Max(0.01f, resolved);
    }

    public float ResolveProjectileLifetime(
        EquipmentSkillRuntimeData runtime,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        if (runtime == null)
        {
            return 3f;
        }

        float baseLifetime = runtime.instanceData != null && runtime.instanceData.projectileLifetimeOverride > 0f
            ? runtime.instanceData.projectileLifetimeOverride
            : GetProjectileLifetime(runtime.sourceEquipment);

        float resolved = ApplyStatModifiers(
            baseLifetime,
            SkillStatModifierType.Lifetime,
            modifiers);

        return Mathf.Max(0.01f, resolved);
    }

    public float ResolveBaseDamage(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        float baseValue = GetBaseDamage(equipmentSo);
        float resolved = ApplyStatModifiers(
            baseValue,
            SkillStatModifierType.Damage,
            modifiers);

        return Mathf.Max(0f, resolved);
    }

    public DamageType GetDamageType(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.DamageType;
        }

        return DamageType.Normal;
    }

    public float GetBaseDamage(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(0f, equipmentSo.BaseProfileSo.BaseDamage);
        }

        return 10f;
    }

    public float GetFlatBonusDamage(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.FlatBonusDamage;
        }

        return 0f;
    }

    public bool GetCanCritical(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.CanCritical;
        }

        return true;
    }

    public float GetCriticalMultiplier(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(1f, equipmentSo.BaseProfileSo.CriticalMultiplier);
        }

        return 1.5f;
    }

    public bool GetIgnoreDefense(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.IgnoreDefense;
        }

        return false;
    }

    public int GetProjectileCount(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(1, equipmentSo.BaseProfileSo.ProjectileCount);
        }

        return 1;
    }

    public float GetProjectileScale(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(0.01f, equipmentSo.BaseProfileSo.ProjectileScale);
        }

        return 1f;
    }

    public float GetProjectileLifetime(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(0.01f, equipmentSo.BaseProfileSo.ProjectileLifetime);
        }

        return 3f;
    }

    public float GetProjectileSpawnOffset(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.ProjectileSpawnOffset;
        }

        return 0f;
    }

    private void AddModifiers(
        List<SkillStatModifierRuntimeData> target,
        IEnumerable<SkillStatModifierRuntimeData> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (SkillStatModifierRuntimeData modifier in source)
        {
            if (modifier != null)
            {
                target.Add(modifier);
            }
        }
    }
}