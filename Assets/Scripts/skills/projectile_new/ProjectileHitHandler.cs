using System.Collections;
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
    private readonly List<Collider2D> pendingHitTargets = new();
    private Coroutine collectCoroutine;
    private bool isCollectingInitialHits;
    private bool initialHitCollectionCompleted;

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
        pendingHitTargets.Clear();
        initialHitCollectionCompleted = false;
        isCollectingInitialHits = false;

        if (collectCoroutine != null)
        {
            StopCoroutine(collectCoroutine);
            collectCoroutine = null;
        }

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

        StartInitialHitCollectionIfNeeded();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanProcessCollider(other))
        {
            return;
        }

        if (isCollectingInitialHits && !initialHitCollectionCompleted)
        {
            RegisterPendingHit(other);
            return;
        }

        ProcessHit(other);
    }

    private void StartInitialHitCollectionIfNeeded()
    {
        float collectDelay = runtimeData != null && runtimeData.hit != null
            ? Mathf.Max(0f, runtimeData.hit.hitStartTime)
            : 0f;

        if (collectDelay <= 0f)
        {
            initialHitCollectionCompleted = true;
            return;
        }

        isCollectingInitialHits = true;
        collectCoroutine = StartCoroutine(
            CollectInitialHitsAndProcess(collectDelay));
    }

    private IEnumerator CollectInitialHitsAndProcess(float collectDelay)
    {
        yield return new WaitForSeconds(collectDelay);

        isCollectingInitialHits = false;
        initialHitCollectionCompleted = true;
        collectCoroutine = null;

        ProcessPendingHitsByDistance();
    }

    private void RegisterPendingHit(Collider2D other)
    {
        if (other == null || pendingHitTargets.Contains(other))
        {
            return;
        }

        pendingHitTargets.Add(other);
    }

    private void ProcessPendingHitsByDistance()
    {
        if (pendingHitTargets.Count == 0)
        {
            return;
        }

        pendingHitTargets.RemoveAll(x => x == null);
        pendingHitTargets.Sort(CompareColliderDistanceToProjectile);

        for (int i = 0; i < pendingHitTargets.Count; i++)
        {
            Collider2D target = pendingHitTargets[i];

            if (!CanProcessCollider(target))
            {
                continue;
            }

            ProcessHit(target);

            if (consumeOnHit)
            {
                break;
            }
        }

        pendingHitTargets.Clear();
    }

    private int CompareColliderDistanceToProjectile(Collider2D a, Collider2D b)
    {
        Vector2 origin = transform.position;

        float distanceA = a != null
            ? ((Vector2)a.ClosestPoint(origin) - origin).sqrMagnitude
            : float.MaxValue;

        float distanceB = b != null
            ? ((Vector2)b.ClosestPoint(origin) - origin).sqrMagnitude
            : float.MaxValue;

        return distanceA.CompareTo(distanceB);
    }

    private bool CanProcessCollider(Collider2D other)
    {
        if (!initialized || ownerEntity == null || runtimeData == null)
        {
            return false;
        }

        if (other == null)
        {
            return false;
        }

        if (!IsTargetLayer(other.gameObject.layer))
        {
            return false;
        }

        if (ignoreOwner && IsOwnerCollider(other))
        {
            return false;
        }

        if (hitTargets.Contains(other))
        {
            return false;
        }

        return true;
    }

    private void ProcessHit(Collider2D other)
    {
        if (!CanProcessCollider(other))
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
            if (ownerCharacter != null)
            {
                ownerCharacter.ApplyDamage(request);
            }
            else
            {
                CharacterDamageService.ApplyWithoutOwner(request);
            }
        }

        ApplyAdditionalEffects(targetCharacter);

        if (consumeOnHit)
        {
            ownerEntity.Despawn();
        }
    }

    private void OnDisable()
    {
        if (collectCoroutine != null)
        {
            StopCoroutine(collectCoroutine);
            collectCoroutine = null;
        }

        pendingHitTargets.Clear();
        isCollectingInitialHits = false;
        initialHitCollectionCompleted = false;
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
        if (targetCharacter == null || runtimeData == null || runtimeData.hit == null)
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
                EffectResolveHelper.CreateRuntimeData(
                    effectEntry.effectSo,
                    EffectSourceType.Skill,
                    GetEffectSourceId(),
                    targetCharacter,
                    ResolveKnockbackSourceTransform(),
                    Vector2.zero);

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

    private Transform ResolveKnockbackSourceTransform()
    {
        if (runtimeData != null && runtimeData.owner != null)
        {
            return runtimeData.owner.transform;
        }

        return transform;
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

            baseDamage =
                runtimeData.hit.damageProfile.baseDamage,

            attackDamagePercent =
                runtimeData.hit.damageProfile.attackDamagePercent
        };
    }
}