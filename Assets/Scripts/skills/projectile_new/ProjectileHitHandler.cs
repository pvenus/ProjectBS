using System.Collections.Generic;
using UnityEngine;
using Status.Service;

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
    private CombatDamageService damageService;
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

        damageService = new CombatDamageService();
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

        if (ignoreOwner && IsOwnerCollider(other))
        {
            return;
        }

        if (hitTargets.Contains(other))
        {
            return;
        }

        StatMono targetStat = other.GetComponentInParent<StatMono>();
        if (targetStat == null)
        {
            return;
        }

        if (runtimeData.damageProfile == null)
        {
            return;
        }

        DamageRequest request = BuildDamageRequest(targetStat);
        if (request == null)
        {
            return;
        }

        hitTargets.Add(other);
        damageService.Apply(request);

        if (consumeOnHit)
        {
            ownerEntity.Despawn();
        }
    }

    private bool IsOwnerCollider(Collider2D other)
    {
        if (runtimeData == null || runtimeData.owner == null)
        {
            return false;
        }

        return other.transform.root == runtimeData.owner.transform.root;
    }

    private DamageRequest BuildDamageRequest(StatMono targetStat)
    {
        if (targetStat == null || runtimeData == null || runtimeData.damageProfile == null)
        {
            return null;
        }

        var request = new DamageRequest
        {
            target = targetStat.gameObject,
            damage = new DamageAmountDto
            {
                baseDamage = runtimeData.damageProfile.baseDamage,
                flatBonusDamage = runtimeData.damageProfile.flatBonusDamage
            },
            element = new ElementContextDto
            {
                elementType = runtimeData.damageProfile.elementType,
                heatGain = runtimeData.damageProfile.heatGain,
                heatCoefficient = runtimeData.damageProfile.heatCoefficient,
                canTriggerOverheat = runtimeData.damageProfile.canTriggerOverheat
            },
            modifiers = new DamageModifierDto
            {
                isCritical = false,
                criticalMultiplier = runtimeData.damageProfile.criticalMultiplier
            }
        };

        return request;
    }
}