using System.Collections.Generic;
using UnityEngine;
using Skill;
/// <summary>
/// 장비 스킬의 수치 계산 전담 Resolver.
/// 장비 기본값 + 룬 modifier + 강화 modifier를 조합해 최종 런타임 수치를 계산한다.
/// Damage는 SkillHitSO 내부의 SkillDamageSO 기준으로 계산한다.
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

    public float ResolveProjectileSpreadAngle(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        float baseValue = GetProjectileSpreadAngle(equipmentSo);
        float resolved = ApplyStatModifiers(
            baseValue,
            SkillStatModifierType.ProjectileSpreadAngle,
            modifiers);

        return Mathf.Max(0f, resolved);
    }

    public int ResolveBurstCount(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        int baseValue = equipmentSo != null && equipmentSo.CastSo != null
            ? Mathf.Max(1, equipmentSo.CastSo.BurstCount)
            : 1;

        return Mathf.Max(1, baseValue);
    }

    public float ResolveBurstInterval(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        float baseValue = equipmentSo != null && equipmentSo.CastSo != null
            ? equipmentSo.CastSo.BurstInterval
            : 0f;

        return Mathf.Max(0f, baseValue);
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
        SkillDamageSO damageSo,
        IEnumerable<SkillStatModifierRuntimeData> modifiers)
    {
        float baseValue = GetBaseDamage(damageSo);
        float resolved = ApplyStatModifiers(
            baseValue,
            SkillStatModifierType.Damage,
            modifiers);

        return Mathf.Max(0f, resolved);
    }

    public DamageType GetDamageType(EquipmentSkillSO equipmentSo)
    {
        return GetDamageType(GetDamageSo(equipmentSo));
    }

    public DamageType GetDamageType(SkillHitSO hitSo)
    {
        return GetDamageType(GetDamageSo(hitSo));
    }

    public DamageType GetDamageType(SkillDamageSO damageSo)
    {
        return damageSo != null
            ? damageSo.DamageType
            : DamageType.Normal;
    }

    public float GetBaseDamage(EquipmentSkillSO equipmentSo)
    {
        return GetBaseDamage(GetDamageSo(equipmentSo));
    }

    public float GetBaseDamage(SkillHitSO hitSo)
    {
        return GetBaseDamage(GetDamageSo(hitSo));
    }

    public float GetBaseDamage(SkillDamageSO damageSo)
    {
        return damageSo != null
            ? Mathf.Max(0f, damageSo.BaseDamage)
            : 0f;
    }
    public bool GetCanCritical(EquipmentSkillSO equipmentSo)
    {
        return GetCanCritical(GetDamageSo(equipmentSo));
    }
    public bool GetCanCritical(SkillHitSO hitSo)
    {
        return GetCanCritical(GetDamageSo(hitSo));
    }
    public bool GetCanCritical(SkillDamageSO damageSo)
    {
        return damageSo != null && damageSo.CanCritical;
    }

    public bool GetIgnoreDefense(EquipmentSkillSO equipmentSo)
    {
        return GetIgnoreDefense(GetDamageSo(equipmentSo));
    }
    public bool GetIgnoreDefense(SkillHitSO hitSo)
    {
        return GetIgnoreDefense(GetDamageSo(hitSo));
    }
    public bool GetIgnoreDefense(SkillDamageSO damageSo)
    {
        return damageSo != null && damageSo.IgnoreDefense;
    }

    public float GetAttackPercentDamage(EquipmentSkillSO equipmentSo)
    {
        return GetAttackPercentDamage(GetDamageSo(equipmentSo));
    }
    public float GetAttackPercentDamage(SkillHitSO hitSo)
    {
        return GetAttackPercentDamage(GetDamageSo(hitSo));
    }
    public float GetAttackPercentDamage(SkillDamageSO damageSo)
    {
        return damageSo != null
            ? Mathf.Max(0f, damageSo.AttackPercentDamage)
            : 0f;
    }

    public SkillDamageSO GetDamageSo(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo == null ||
            equipmentSo.HitSos == null ||
            equipmentSo.HitSos.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < equipmentSo.HitSos.Length; i++)
        {
            SkillDamageSO damageSo = GetDamageSo(equipmentSo.HitSos[i]);

            if (damageSo != null)
            {
                return damageSo;
            }
        }

        return null;
    }

    public SkillDamageSO GetDamageSo(SkillHitSO hitSo)
    {
        return hitSo != null
            ? hitSo.DamageSo
            : null;
    }

    public int GetProjectileCount(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(1, equipmentSo.BaseProfileSo.ProjectileCount);
        }

        return 1;
    }

    public float GetProjectileSpreadAngle(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(
                0f,
                equipmentSo.BaseProfileSo.ProjectileSpreadAngle);
        }

        return 0f;
    }

    public ProjectileArrangementType GetProjectileArrangement(
        EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return equipmentSo.BaseProfileSo.ProjectileArrangement;
        }

        return ProjectileArrangementType.Single;
    }

    public float GetProjectileArrangementValue(
        EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(
                0f,
                equipmentSo.BaseProfileSo.ProjectileArrangementValue);
        }

        return 0f;
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

    public float GetProjectileSpawnInterval(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(
                0f,
                equipmentSo.BaseProfileSo.ProjectileSpawnInterval);
        }

        return 0f;
    }

    public float GetProjectileSpawnRadius(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(
                0f,
                equipmentSo.BaseProfileSo.ProjectileSpawnRadius);
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