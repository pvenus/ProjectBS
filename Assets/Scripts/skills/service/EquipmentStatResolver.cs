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
    public List<SkillStatModifierData> CombineStatModifiers(
        IEnumerable<SkillStatModifierData> second)
    {
        var result = new List<SkillStatModifierData>();

        AddModifiers(result, second);

        return result;
    }

    public float GetBaseStatValue(
        EquipmentSkillSO equipmentSo,
        SkillStatModifierType modifierType)
    {
        if (equipmentSo == null)
        {
            return 0f;
        }

        switch (modifierType)
        {
            case SkillStatModifierType.BaseDamage:
            {
                SkillDamageSO damageSo = GetDamageSo(equipmentSo);
                return damageSo != null
                    ? Mathf.Max(0f, damageSo.BaseDamage)
                    : 0f;
            }

            case SkillStatModifierType.AttackPercentDamage:
            {
                SkillDamageSO damageSo = GetDamageSo(equipmentSo);
                return damageSo != null
                    ? Mathf.Max(0f, damageSo.AttackPercentDamage)
                    : 0f;
            }

            case SkillStatModifierType.Cooldown:
                return equipmentSo.CastSo != null
                    ? Mathf.Max(0f, equipmentSo.CastSo.Cooldown)
                    : 0f;

            case SkillStatModifierType.Range:
                return equipmentSo.CastSo != null
                    ? Mathf.Max(0f, equipmentSo.CastSo.Range)
                    : 0f;
                    
            case SkillStatModifierType.ProjectileCount:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(1, equipmentSo.BaseProfileSo.ProjectileCount)
                    : 1;

            case SkillStatModifierType.ProjectileSpreadAngle:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(0f, equipmentSo.BaseProfileSo.ProjectileSpreadAngle)
                    : 0f;

            case SkillStatModifierType.ProjectileScale:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(0.01f, equipmentSo.BaseProfileSo.ProjectileScale)
                    : 1f;

            case SkillStatModifierType.Lifetime:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(0.01f, equipmentSo.BaseProfileSo.ProjectileLifetime)
                    : 3f;

            default:
                return 0f;
        }
    }

    public float ResolveStatValue(
        EquipmentSkillSO equipmentSo,
        SkillStatModifierType modifierType,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        float baseValue = GetBaseStatValue(
            equipmentSo,
            modifierType);

        return ApplyStatModifiers(
            baseValue,
            modifierType,
            modifiers);
    }

    public float ResolveStat(
        EquipmentSkillSO equipmentSo,
        SkillStatModifierType modifierType,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        float resolved = ResolveStatValue(
            equipmentSo,
            modifierType,
            modifiers);

        return ClampResolvedStat(
            modifierType,
            resolved);
    }

    public int ResolveIntStat(
        EquipmentSkillSO equipmentSo,
        SkillStatModifierType modifierType,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        float resolved = ResolveStat(
            equipmentSo,
            modifierType,
            modifiers);

        return Mathf.RoundToInt(resolved);
    }

    private float ClampResolvedStat(
        SkillStatModifierType modifierType,
        float value)
    {
        switch (modifierType)
        {
            case SkillStatModifierType.ProjectileCount:
            case SkillStatModifierType.SplitHitCount:
                return Mathf.Max(1f, value);

            case SkillStatModifierType.ProjectileScale:
            case SkillStatModifierType.Lifetime:
                return Mathf.Max(0.01f, value);

            case SkillStatModifierType.ProjectileSpreadAngle:
            case SkillStatModifierType.ProjectileSpawnInterval:
            case SkillStatModifierType.ProjectileSpawnRadius:
            case SkillStatModifierType.ProjectileColliderRadius:
            case SkillStatModifierType.Cooldown:
            case SkillStatModifierType.Range:
            case SkillStatModifierType.BaseDamage:
            case SkillStatModifierType.AttackPercentDamage:
                return Mathf.Max(0f, value);

            default:
                return value;
        }
    }

    public float ApplyStatModifiers(
        float baseValue,
        SkillStatModifierType modifierType,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        float value = baseValue;

        if (modifiers == null)
        {
            return value;
        }

        foreach (SkillStatModifierData modifier in modifiers)
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
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveIntStat(
            equipmentSo,
            SkillStatModifierType.ProjectileCount,
            modifiers);
    }

    public float ResolveProjectileSpreadAngle(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.ProjectileSpreadAngle,
            modifiers);
    }

    public int ResolveBurstCount(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        int baseValue = equipmentSo != null && equipmentSo.CastSo != null
            ? Mathf.Max(1, equipmentSo.CastSo.BurstCount)
            : 1;

        return Mathf.Max(1, baseValue);
    }

    public float ResolveBurstInterval(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        float baseValue = equipmentSo != null && equipmentSo.CastSo != null
            ? equipmentSo.CastSo.BurstInterval
            : 0f;

        return Mathf.Max(0f, baseValue);
    }

    public float ResolveProjectileScale(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.ProjectileScale,
            modifiers);
    }

    public float ResolveProjectileLifetime(
        EquipmentSkillRuntimeData runtime,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            runtime?.sourceEquipment,
            SkillStatModifierType.Lifetime,
            modifiers);
    }

    public float ResolveBaseDamage(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.BaseDamage,
            modifiers);
    }

    public float ResolveBaseDamage(
        SkillDamageSO damageSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        float baseValue = GetBaseDamage(damageSo);
        float resolved = ApplyStatModifiers(
            baseValue,
            SkillStatModifierType.BaseDamage,
            modifiers);

        return Mathf.Max(0f, resolved);
    }

    public float ResolveAttackPercentDamage(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.AttackPercentDamage,
            modifiers);
    }

    public float ResolveCooldown(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.Cooldown,
            modifiers);
    }

    public float ResolveRange(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.Range,
            modifiers);
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

    public float GetProjectileColliderRadius(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo != null && equipmentSo.BaseProfileSo != null)
        {
            return Mathf.Max(
                0.01f,
                equipmentSo.BaseProfileSo.ProjectileColliderRadius);
        }

        return 0.5f;
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
        List<SkillStatModifierData> target,
        IEnumerable<SkillStatModifierData> source)
    {
        if (target == null || source == null)
        {
            return;
        }

        foreach (SkillStatModifierData modifier in source)
        {
            if (modifier != null)
            {
                target.Add(modifier);
            }
        }
    }
}