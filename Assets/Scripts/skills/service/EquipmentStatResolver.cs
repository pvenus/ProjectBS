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

        SkillHitSO hitSo = null;
        if (equipmentSo.HitSos != null && equipmentSo.HitSos.Length > 0)
        {
            hitSo = equipmentSo.HitSos[0];
        }

        switch (modifierType)
        {
            case SkillStatModifierType.BaseDamage:
            {
                return hitSo != null
                    ? Mathf.Max(0f, hitSo.BaseDamage)
                    : 0f;
            }

            case SkillStatModifierType.AttackPercentDamage:
            {
                return hitSo != null
                    ? Mathf.Max(0f, hitSo.AttackPercentDamage)
                    : 0f;
            }

            case SkillStatModifierType.MaxHitCount:
            {
                return hitSo != null
                    ? Mathf.Max(1, hitSo.MaxHitCount)
                    : 1;
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

            case SkillStatModifierType.ProjectileColliderRadius:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(0.01f, equipmentSo.BaseProfileSo.ProjectileColliderRadius)
                    : 3f;

            case SkillStatModifierType.ProjectileSpawnRadius:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(0f, equipmentSo.BaseProfileSo.ProjectileSpawnRadius)
                    : 0f;

            case SkillStatModifierType.ProjectileSpawnInterval:
                return equipmentSo.BaseProfileSo != null
                    ? Mathf.Max(0f, equipmentSo.BaseProfileSo.ProjectileSpawnInterval)
                    : 0f;

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
            case SkillStatModifierType.MaxHitCount:
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
            if (modifier == null || modifier.ModifierType != modifierType)
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

    public float ResolveAttackPercentDamage(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveStat(
            equipmentSo,
            SkillStatModifierType.AttackPercentDamage,
            modifiers);
    }
    public int ResolveMaxHitCount(
        EquipmentSkillSO equipmentSo,
        IEnumerable<SkillStatModifierData> modifiers)
    {
        return ResolveIntStat(
            equipmentSo,
            SkillStatModifierType.MaxHitCount,
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
        if (equipmentSo == null || equipmentSo.HitSos == null || equipmentSo.HitSos.Length == 0)
        {
            return DamageType.Normal;
        }
        return equipmentSo.HitSos[0].DamageType;
    }

    public DamageType GetDamageType(SkillHitSO hitSo)
    {
        return hitSo != null ? hitSo.DamageType : DamageType.Normal;
    }

    public float GetBaseDamage(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo == null || equipmentSo.HitSos == null || equipmentSo.HitSos.Length == 0)
        {
            return 0f;
        }
        return Mathf.Max(0f, equipmentSo.HitSos[0].BaseDamage);
    }

    public float GetBaseDamage(SkillHitSO hitSo)
    {
        return hitSo != null ? Mathf.Max(0f, hitSo.BaseDamage) : 0f;
    }

    public bool GetCanCritical(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo == null || equipmentSo.HitSos == null || equipmentSo.HitSos.Length == 0)
        {
            return false;
        }
        return equipmentSo.HitSos[0].CanCritical;
    }
    public bool GetCanCritical(SkillHitSO hitSo)
    {
        return hitSo != null && hitSo.CanCritical;
    }

    public bool GetIgnoreDefense(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo == null || equipmentSo.HitSos == null || equipmentSo.HitSos.Length == 0)
        {
            return false;
        }
        return equipmentSo.HitSos[0].IgnoreDefense;
    }
    public bool GetIgnoreDefense(SkillHitSO hitSo)
    {
        return hitSo != null && hitSo.IgnoreDefense;
    }

    public float GetAttackPercentDamage(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo == null || equipmentSo.HitSos == null || equipmentSo.HitSos.Length == 0)
        {
            return 0f;
        }
        return Mathf.Max(0f, equipmentSo.HitSos[0].AttackPercentDamage);
    }
    public float GetAttackPercentDamage(SkillHitSO hitSo)
    {
        return hitSo != null ? Mathf.Max(0f, hitSo.AttackPercentDamage) : 0f;
    }
    public int GetMaxHitCount(EquipmentSkillSO equipmentSo)
    {
        if (equipmentSo == null || equipmentSo.HitSos == null || equipmentSo.HitSos.Length == 0)
        {
            return 1;
        }

        return Mathf.Max(1, equipmentSo.HitSos[0].MaxHitCount);
    }

    public int GetMaxHitCount(SkillHitSO hitSo)
    {
        return hitSo != null ? Mathf.Max(1, hitSo.MaxHitCount) : 1;
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