using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Effect;
using Effect.Helper;
using Battle.Prop;

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
    private readonly HashSet<Collider2D> overlapTargets = new();
    private Coroutine collectCoroutine;
    private Coroutine repeatHitCoroutine;
    private bool isCollectingInitialHits;
    private bool initialHitCollectionCompleted;
    private bool hasAppliedDamage;
    private int currentHitCount;

    public bool IsInitialized => initialized;
    public bool ConsumeOnHit => consumeOnHit;
    public bool IgnoreOwner => ignoreOwner;

    private void Reset()
    {
        hitCollider = GetComponent<Collider2D>();
    }

    private void Awake()
    {
        EnsureHitCollider();
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
        EnsureHitCollider();
        ConfigureHitCollider(data);
        initialized = true;
        hitTargets.Clear();
        pendingHitTargets.Clear();
        overlapTargets.Clear();
        initialHitCollectionCompleted = false;
        isCollectingInitialHits = false;
        hasAppliedDamage = false;
        currentHitCount = 0;
        ignoreOwner = data.hit != null && data.hit.ignoreSameRoot;

        if (collectCoroutine != null)
        {
            StopCoroutine(collectCoroutine);
            collectCoroutine = null;
        }

        if (repeatHitCoroutine != null)
        {
            StopCoroutine(repeatHitCoroutine);
            repeatHitCoroutine = null;
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
        StartRepeatHitIfNeeded();
    }

    private void EnsureHitCollider()
    {
        if (hitCollider != null)
        {
            return;
        }

        hitCollider = GetComponent<Collider2D>();

        if (hitCollider == null)
        {
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = 0.5f;
            hitCollider = circleCollider;
        }

        hitCollider.isTrigger = true;
    }

    private void ConfigureHitCollider(ProjectileRuntimeData data)
    {
        EnsureHitCollider();

        CircleCollider2D circleCollider = hitCollider as CircleCollider2D;

        if (circleCollider == null)
        {
            return;
        }

        circleCollider.radius = ResolveProjectileColliderRadius(data);
    }

    private float ResolveProjectileColliderRadius(ProjectileRuntimeData data)
    {
        if (data == null || data.hit == null)
        {
            return 0.5f;
        }

        return Mathf.Max(0.01f, data.hit.projectileColliderRadius);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanProcessCollider(other))
        {
            return;
        }

        if (runtimeData.hit.useRepeatInterval)
        {
            RegisterOverlapTarget(other);
            return;
        }

        if (isCollectingInitialHits && !initialHitCollectionCompleted)
        {
            RegisterPendingHit(other);
            return;
        }

        ProcessHit(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        overlapTargets.Remove(other);
    }

    private void StartRepeatHitIfNeeded()
    {
        if (runtimeData == null
            || runtimeData.hit == null
            || !runtimeData.hit.useRepeatInterval)
        {
            return;
        }

        float interval = Mathf.Max(
            0.05f,
            runtimeData.hit.repeatInterval);

        repeatHitCoroutine = StartCoroutine(
            RepeatHitRoutine(interval));
    }

    private IEnumerator RepeatHitRoutine(float interval)
    {
        WaitForSeconds wait = new WaitForSeconds(interval);

        while (initialized && ownerEntity != null && runtimeData != null)
        {
            yield return wait;
            ProcessOverlapHits();
        }
    }

    private void RegisterOverlapTarget(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        overlapTargets.Add(other);
    }

    private void ProcessOverlapHits()
    {
        if (overlapTargets.Count == 0)
        {
            return;
        }

        List<Collider2D> targets = new List<Collider2D>(overlapTargets);
        targets.RemoveAll(x => x == null);
        targets.Sort(CompareColliderDistanceToProjectile);

        overlapTargets.Clear();

        for (int i = 0; i < targets.Count; i++)
        {
            Collider2D target = targets[i];

            if (!CanProcessCollider(target, true))
            {
                continue;
            }

            overlapTargets.Add(target);
            ProcessHit(target, true, false);
        }
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
        return CanProcessCollider(other, false);
    }

    private bool CanProcessCollider(Collider2D other, bool ignoreHitHistory)
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

        if (!ignoreHitHistory && hitTargets.Contains(other))
        {
            return false;
        }

        return true;
    }

    private bool HasReachedMaxHitCount()
    {
        if (runtimeData == null || runtimeData.hit == null)
        {
            return false;
        }

        return runtimeData.hit.maxHitCount > 0
            && currentHitCount >= runtimeData.hit.maxHitCount;
    }

    private void ProcessHit(Collider2D other)
    {
        ProcessHit(other, false, consumeOnHit);
    }

    private void ProcessHit(
        Collider2D other,
        bool ignoreHitHistory,
        bool consumeAfterHit)
    {
        if (HasReachedMaxHitCount())
        {
            return;
        }
        if (!CanProcessCollider(other, ignoreHitHistory))
        {
            return;
        }

        BattlePropController targetProp =
            other.GetComponentInParent<BattlePropController>();

        if (targetProp != null)
        {
            targetProp.OnProjectileHit();

            if (!ignoreHitHistory)
            {
                hitTargets.Add(other);
            }

            currentHitCount++;

            if (HasReachedMaxHitCount())
            {
                ownerEntity.Despawn();
                return;
            }

            if (consumeAfterHit)
            {
                ownerEntity.Despawn();
            }

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

        if (!ignoreHitHistory)
        {
            hitTargets.Add(other);
        }

        currentHitCount++;

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

            hasAppliedDamage = true;
        }

        ApplyAdditionalEffects(targetCharacter);

        if (HasReachedMaxHitCount())
        {
            ownerEntity.Despawn();
            return;
        }

        if (consumeAfterHit)
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

        if (repeatHitCoroutine != null)
        {
            StopCoroutine(repeatHitCoroutine);
            repeatHitCoroutine = null;
        }

        pendingHitTargets.Clear();
        overlapTargets.Clear();
        isCollectingInitialHits = false;
        initialHitCollectionCompleted = false;
        hasAppliedDamage = false;
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
        if (runtimeData == null || runtimeData.hit == null)
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
            runtimeData.hit.buffEffects);

        ApplyEffects(
            effectManager,
            targetCharacter,
            runtimeData.hit.debuffEffects);
    }

    private void ApplyEffects(
        EffectManager effectManager,
        CharacterManager targetCharacter,
        EffectEntryRuntime[] effects)
    {
        if (effectManager == null
            || effects == null
            || effects.Length == 0)
        {
            return;
        }

        foreach (EffectEntryRuntime effectEntry in effects)
        {
            if (effectEntry == null
                || effectEntry.RuntimeData == null)
            {
                continue;
            }

            if (effectEntry.RuntimeData is TauntEffectRuntime tauntEffect)
            {
                tauntEffect.BindHitTarget(
                    targetCharacter,
                    transform);
            }

            EffectApplyHelper.ApplyEffect(
                effectManager,
                effectEntry);
        }
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

        float baseDamage = runtimeData.hit.damageProfile.baseDamage;

        bool useFirstHitDamage =
            runtimeData.hit.hitStartTime > 0f
            && !hasAppliedDamage
            && runtimeData.hit.damageProfile.firstHitBaseDamage > 0f;

        if (useFirstHitDamage)
        {
            baseDamage = runtimeData.hit.damageProfile.firstHitBaseDamage;
        }

        return new CharacterDamageRequest
        {
            attacker = runtimeData.owner != null
                ? runtimeData.owner.gameObject
                : null,

            target = targetCharacter.gameObject,

            baseDamage = baseDamage,

            attackDamagePercent =
                runtimeData.hit.damageProfile.attackDamagePercent
        };
    }
}
