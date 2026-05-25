using System.Collections.Generic;
using UnityEngine;
using Character;
using Effect;

/// <summary>
/// ProjectileEntity의 충돌/히트 처리를 담당하는 컴포넌트.
/// 충돌 감지, owner 필터링, 중복 타격 방지, CombatDamageService 호출 연결까지만 담당한다.
/// 실제 데미지 계산은 CombatDamageService가 수행한다.
/// </summary>
public class ProjectileHitHandler : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Collider2D hitCollider;

    [Header("Debug")]
    [SerializeField] private bool initialized;
    [SerializeField] private bool consumeOnHit = true;
    [SerializeField] private bool ignoreOwner = true;

    private ProjectileEntity ownerEntity;
    private ProjectileRuntimeData runtimeData;
    private CharacterManager ownerCharacter;
    private readonly HashSet<Collider2D> hitTargets = new();

    public bool IsInitialized => initialized;
    public bool ConsumeOnHit => consumeOnHit;
    public bool IgnoreOwner => ignoreOwner;

    private void Reset()
    {
        hitCollider = GetComponent<Collider2D>();
    }

    public void Initialize(ProjectileEntity entity, ProjectileRuntimeData data)
    {
        if (entity == null)
        {
            Debug.LogError("ProjectileHitHandler.Initialize failed: entity is null.", this);
            return;
        }

        if (data == null)
        {
            Debug.LogError("ProjectileHitHandler.Initialize failed: ProjectileRuntimeData is null.", this);
            return;
        }

        ownerEntity = entity;
        runtimeData = data;
        initialized = true;
        hitTargets.Clear();

        if (data.hit != null)
        {
            consumeOnHit = data.hit.deactivateAfterFirstHit;
        }

        if (runtimeData.owner != null)
        {
            ownerCharacter =
                runtimeData.owner.GetComponent<CharacterManager>();

            if (ownerCharacter == null)
            {
                ownerCharacter =
                    runtimeData.owner.GetComponentInParent<CharacterManager>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || ownerEntity == null || runtimeData == null)
        {
            return;
        }

        if (other == null)
        {
            return;
        }

        if (!IsTargetLayer(other.gameObject.layer))
        {
            return;
        }

        if (ignoreOwner && IsOwnerCollider(other))
        {
            return;
        }

        if (hitTargets.Contains(other))
        {
            return;
        }

        CharacterManager targetCharacter =
            other.GetComponentInParent<CharacterManager>();

        if (targetCharacter == null)
        {
            return;
        }

        bool hasDamage = runtimeData.hit != null
            && runtimeData.hit.damageProfile != null;
        bool hasBuffEffects = runtimeData.hit != null
            && runtimeData.hit.buffEffects != null
            && runtimeData.hit.buffEffects.Length > 0;
        bool hasDebuffEffects = runtimeData.hit != null
            && runtimeData.hit.debuffEffects != null
            && runtimeData.hit.debuffEffects.Length > 0;

        if (!hasDamage && !hasBuffEffects && !hasDebuffEffects)
        {
            return;
        }

        CharacterDamageRequest request = hasDamage
            ? BuildDamageRequest(targetCharacter)
            : null;

        if (hasDamage && request == null)
        {
            return;
        }

        hitTargets.Add(other);

        if (request != null)
        {
            ownerCharacter?.ApplyDamage(request);
        }

        ApplyAdditionalEffects(targetCharacter);

        if (consumeOnHit)
        {
            ownerEntity.Despawn();
        }
    }

    private bool IsTargetLayer(int layer)
    {
        if (runtimeData == null || runtimeData.hit == null)
        {
            return true;
        }

        return (runtimeData.hit.targetLayerMask.value & (1 << layer)) != 0;
    }

    private bool IsOwnerCollider(Collider2D other)
    {
        if (runtimeData == null || runtimeData.owner == null)
        {
            return false;
        }

        return other.transform.root == runtimeData.owner.transform.root;
    }

    private void ApplyAdditionalEffects(CharacterManager targetCharacter)
    {
        if (targetCharacter == null || runtimeData == null)
        {
            return;
        }

        EffectManager effectManager =
            targetCharacter.GetComponent<EffectManager>();

        if (effectManager == null)
        {
            effectManager =
                targetCharacter.GetComponentInChildren<EffectManager>();
        }

        if (effectManager == null)
        {
            return;
        }

        ApplyEffects(
            effectManager,
            targetCharacter,
            runtimeData.hit.buffEffects,
            EffectCategoryType.Buff);

        ApplyEffects(
            effectManager,
            targetCharacter,
            runtimeData.hit.debuffEffects,
            EffectCategoryType.Debuff);
    }

    private void ApplyEffects(
        EffectManager effectManager,
        CharacterManager targetCharacter,
        SkillProjectileHitEffectEntry[] effects,
        EffectCategoryType defaultCategoryType)
    {
        if (effectManager == null || effects == null || effects.Length == 0)
        {
            return;
        }

        foreach (SkillProjectileHitEffectEntry effectEntry in effects)
        {
            if (effectEntry == null || effectEntry.effectSo == null)
            {
                continue;
            }

            Effect.EffectRuntimeData effectRuntimeData =
                CreateEffectRuntimeData(
                    effectEntry.effectSo,
                    targetCharacter);

            if (effectRuntimeData == null)
            {
                continue;
            }

            EffectCategoryType categoryType = effectEntry.categoryType != EffectCategoryType.Neutral
                ? effectEntry.categoryType
                : defaultCategoryType;

            effectManager.AddEffect(
                effectRuntimeData,
                effectEntry.lifetimeType,
                effectEntry.duration,
                categoryType);
        }
    }

    private Effect.EffectRuntimeData CreateEffectRuntimeData(
        EffectSO effectSo,
        CharacterManager targetCharacter)
    {
        if (effectSo == null)
        {
            return null;
        }

        if (effectSo is StatModifierEffectSO statModifierEffect)
        {
            if (targetCharacter == null)
            {
                return null;
            }

            return new StatModifierEffectRuntime(
                statModifierEffect,
                EffectSourceType.Skill,
                GetEffectSourceId(),
                targetCharacter);
        }

        if (effectSo is HealEffectSO healEffect)
        {
            if (targetCharacter == null)
            {
                return null;
            }

            return healEffect.CreateRuntimeData(targetCharacter);
        }

        return null;
    }

    private string GetEffectSourceId()
    {
        if (runtimeData != null && runtimeData.projectilePrefab != null)
        {
            return runtimeData.projectilePrefab.name;
        }

        if (ownerEntity != null)
        {
            return ownerEntity.name;
        }

        return gameObject.name;
    }

    private CharacterDamageRequest BuildDamageRequest(CharacterManager targetCharacter)
    {
        if (targetCharacter == null
            || runtimeData == null
            || runtimeData.hit == null
            || runtimeData.hit.damageProfile == null)
        {
            return null;
        }

        return new CharacterDamageRequest
        {
            attacker = runtimeData.owner != null
                ? runtimeData.owner.gameObject
                : null,

            target = targetCharacter.gameObject,

            attackDamagePercent =
                runtimeData.hit.damageProfile.attackDamagePercent,

            flatBonusDamage =
                runtimeData.hit.damageProfile.flatBonusDamage
        };
    }
}