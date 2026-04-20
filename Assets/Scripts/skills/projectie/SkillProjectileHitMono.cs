using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skills.Dto;
using Status.Service;

/// <summary>
/// Projectile hit 전용 게이트웨이 Mono.
/// - Unity 충돌/스캔 결과를 입력으로 받는다.
/// - 내부 HitController를 생성/보관한다.
/// - 실제 hit 가능 여부 판단과 등록은 HitController에 위임한다.
/// - 데미지, VFX, 상태이상 같은 후처리는 여기서 하지 않는다.
/// </summary>
public class SkillProjectileHitMono : MonoBehaviour
{
    [Header("Hit Policy")]
    [SerializeField] private SkillHitSO hitConfig;

    [Header("Hit Detection")]
    [SerializeField] private LayerMask targetLayerMask;

    public struct HitRuntimeContext
    {
        public Transform owner;
        public SkillUpgradeMono.SkillUpgradeData upgradeData;
    }

    private HitRuntimeContext _context;

    public void SetContext(HitRuntimeContext context)
    {
        _context = context;
        ApplyContext();
    }

    private CircleCollider2D _circleCollider;
    private SkillDamageProfileDto _damageProfile;
    private bool _applyDamage = true;
    private bool _useHitWindow;
    private float _hitStartTime;
    private float _hitDuration = 0.1f;
    private bool _deactivateAfterFirstHit;
    private float _elapsedTime;
    private bool _hitWindowClosed;
    private bool _useSplitMultiHitDamage;
    private int _splitHitCount = 1;
    private float _splitHitInterval = 0f;
    private readonly Dictionary<Collider2D, Coroutine> _activeSplitDamageRoutines = new Dictionary<Collider2D, Coroutine>();

    private readonly CombatDamageService _damageService = new CombatDamageService();

    private bool _useKnockback;
    private float _knockbackForce;

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_hitWindowClosed)
            return;

        if (_hitController == null)
            return;

        if (_hitController.HasReachedMaxHitCount)
            return;

        if (_useHitWindow)
        {
            if (_elapsedTime < _hitStartTime)
                return;

            if (_elapsedTime > _hitStartTime + _hitDuration)
                return;
        }

        Vector2 center = transform.TransformPoint(_circleCollider.offset);
        float radius = _circleCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        var hits = Physics2D.OverlapCircleAll(center, radius, targetLayerMask);

        bool hasProcessedAnyHitThisFrame = false;
        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            if (TryProcessHit(_context.owner != null ? _context.owner : transform, hit))
            {
                hasProcessedAnyHitThisFrame = true;

                if (_hitController.HasReachedMaxHitCount)
                    break;
            }
        }
        if (_deactivateAfterFirstHit && hasProcessedAnyHitThisFrame)
        {
            _hitWindowClosed = true;
        }
    }

    private SkillProjectileHitController _hitController;

    public bool IsReady => _hitController != null;
    public bool HasReachedMaxHitCount => _hitController != null && _hitController.HasReachedMaxHitCount;

    private void Awake()
    {
        if (hitConfig == null)
        {
            Debug.LogError("SkillHitSO (hitConfig) is required on SkillProjectileHitMono", this);
            enabled = false;
            return;
        }

        _circleCollider = GetComponent<CircleCollider2D>();
        if (_circleCollider == null)
        {
            Debug.LogError("CircleCollider2D is required on SkillProjectileHitMono", this);
            enabled = false;
            return;
        }
    }

    public void Initialize(SkillHitSO config)
    {
        if (config == null)
        {
            Debug.LogError("SkillHitSO is null", this);
            return;
        }

        hitConfig = config;
        ApplyContext();
    }

    public void ApplyUpgradeData(SkillUpgradeMono.SkillUpgradeData upgradeData)
    {
        _context.upgradeData = upgradeData;
        ApplyContext();
    }

    private void ApplyContext()
    {
        if (hitConfig == null)
            return;

        Initialize(hitConfig.CreateDto(_context.upgradeData));
    }

    /// <summary>
    /// 런타임 DTO로 컨트롤러를 재설정한다.
    /// </summary>
    public void Initialize(SkillProjectileHitDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileHitDto is null", this);
            return;
        }

        _useHitWindow = dto.useHitWindow;
        _hitStartTime = Mathf.Max(0f, dto.hitStartTime);
        _hitDuration = Mathf.Max(0f, dto.hitDuration);
        _deactivateAfterFirstHit = dto.deactivateAfterFirstHit;
        _elapsedTime = 0f;
        _hitWindowClosed = false;

        _damageProfile = dto.damageProfile;
        _applyDamage = dto.applyDamage;
        _useSplitMultiHitDamage = dto.useSplitMultiHitDamage;
        _splitHitCount = Mathf.Max(1, dto.splitHitCount);
        _splitHitInterval = Mathf.Max(0f, dto.splitHitInterval);

        _useKnockback = dto.useKnockback;
        _knockbackForce = Mathf.Max(0f, dto.knockbackForce);

        _hitController = new SkillProjectileHitController();
        _hitController.Configure(Mathf.Max(1, dto.maxHitCount), dto.ignoreSameRoot);

        if (dto.useRepeatInterval)
        {
            _hitController.UseHitIntervalPolicy(Mathf.Max(0f, dto.repeatInterval));
        }
        else
        {
            _hitController.UseHitOncePolicy();
        }

        _hitController.ResetState();
    }

    /// <summary>
    /// 현재 히트 처리에 포함되는 후처리.
    /// 현재는 데미지만 적용하지만, 이후 상태이상 / VFX / 락온 도트 같은 정책도 이 경로로 확장할 수 있다.
    /// </summary>
    private void ApplyDamage(Collider2D other)
    {
        if (other == null)
            return;

        var stat = other.GetComponentInParent<StatMono>();

        if (_applyDamage)
        {
            if (stat == null || _damageProfile == null)
                return;

            if (_useSplitMultiHitDamage)
            {
                StartSplitDamageRoutine(other, stat);
            }
            else
            {
                ApplySingleDamage(other, stat, _damageProfile);
            }
        }

        ApplyKnockback(other);
    }

    private void ApplySingleDamage(Collider2D other, StatMono stat, SkillDamageProfileDto damageProfile)
    {
        if (other == null || stat == null || damageProfile == null)
            return;

        Transform source = _context.owner != null ? _context.owner : transform;
        Vector2 hitPoint = other.bounds.center;

        DamageRequest request = damageProfile.CreateRequest(
            source != null ? source.gameObject : gameObject,
            stat.gameObject,
            hitPoint
        );

        _damageService.Apply(request);
    }

    private void ApplyKnockback(Collider2D other)
    {
        if (!_useKnockback)
            return;

        if (other == null)
            return;

        Transform source = _context.owner != null ? _context.owner : transform;
        Vector2 sourcePosition = source != null ? (Vector2)source.position : (Vector2)transform.position;
        Vector2 targetPosition = other.bounds.center;
        Vector2 direction = Vector2.zero;

        direction = targetPosition - sourcePosition;

        if (direction.sqrMagnitude <= 0.0001f)
            direction = (Vector2)transform.position - (Vector2)other.transform.position;

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        MovementMono movementMono = other.GetComponentInParent<MovementMono>();
        if (movementMono != null)
        {
            movementMono.ApplyKnockback(direction.normalized, _knockbackForce);
            return;
        }

        KnockbackController knockbackController = other.GetComponentInParent<KnockbackController>();
        if (knockbackController != null)
        {
            knockbackController.ApplyKnockback(direction.normalized * _knockbackForce);
        }
    }

    /// <summary>
    /// 총 데미지를 앞 타격이 훨씬 크게 보이도록 반감(halving) 기반으로 분할한다.
    /// - 첫 타격은 남은 데미지의 절반을 우선 가져간다.
    /// - 이후 타격도 남은 데미지를 절반씩 가져가되, 소수점은 버리고 최소 1을 보장한다.
    /// - 마지막 타격은 남은 모든 데미지를 가져가서 총합이 항상 totalDamage와 같도록 맞춘다.
    /// 
    /// 예)
    /// totalDamage = 10, splitCount = 4
    /// -> [5, 2, 2, 1] 같은 앞타 우선 내림차순 형태
    /// </summary>
    private int[] BuildLogSplitDamageSequence(int totalDamage, int splitCount)
    {
        totalDamage = Mathf.Max(0, totalDamage);
        splitCount = Mathf.Max(1, splitCount);

        int[] result = new int[splitCount];
        if (totalDamage == 0)
            return result;

        int remain = totalDamage;

        for (int i = 0; i < splitCount; i++)
        {
            int remainingSlots = splitCount - i;

            if (remainingSlots <= 1)
            {
                result[i] = remain;
                remain = 0;
                break;
            }

            int minRequiredForRest = remainingSlots - 1;
            int maxAssignable = Mathf.Max(1, remain - minRequiredForRest);

            int value = Mathf.FloorToInt(remain * 0.5f);
            value = Mathf.Max(1, value);
            value = Mathf.Min(value, maxAssignable);

            result[i] = value;
            remain -= value;
        }

        if (remain > 0)
        {
            result[0] += remain;
        }

        System.Array.Sort(result);
        System.Array.Reverse(result);
        return result;
    }

    private void StartSplitDamageRoutine(Collider2D targetCollider, StatMono stat)
    {
        if (targetCollider == null || stat == null)
            return;

        if (_activeSplitDamageRoutines.ContainsKey(targetCollider))
            return;

        Coroutine routine = StartCoroutine(ApplySplitDamageRoutine(targetCollider, stat));
        _activeSplitDamageRoutines[targetCollider] = routine;
    }

    private IEnumerator ApplySplitDamageRoutine(Collider2D targetCollider, StatMono stat)
    {
        int totalBaseDamage = _damageProfile != null ? Mathf.RoundToInt(_damageProfile.baseDamage) : 0;
        int[] splitDamages = BuildLogSplitDamageSequence(totalBaseDamage, _splitHitCount);

        for (int i = 0; i < splitDamages.Length; i++)
        {
            if (targetCollider == null || stat == null)
                break;

            if (!targetCollider.gameObject.activeInHierarchy)
                break;

            int damage = Mathf.Max(0, splitDamages[i]);
            if (damage > 0 && _damageProfile != null)
            {
                SkillDamageProfileDto splitProfile = new SkillDamageProfileDto
                {
                    skillId = _damageProfile.skillId,
                    damageType = _damageProfile.damageType,
                    elementType = _damageProfile.elementType,
                    baseDamage = damage,
                    flatBonusDamage = _damageProfile.flatBonusDamage,
                    heatCoefficient = _damageProfile.heatCoefficient,
                    heatGain = _damageProfile.heatGain,
                    canTriggerOverheat = _damageProfile.canTriggerOverheat,
                    canCritical = _damageProfile.canCritical,
                    criticalMultiplier = _damageProfile.criticalMultiplier,
                    ignoreDefense = _damageProfile.ignoreDefense
                };

                ApplySingleDamage(targetCollider, stat, splitProfile);
            }

            if (i < splitDamages.Length - 1 && _splitHitInterval > 0f)
            {
                yield return new WaitForSeconds(_splitHitInterval);
            }
            else
            {
                yield return null;
            }
        }

        if (targetCollider != null)
        {
            _activeSplitDamageRoutines.Remove(targetCollider);
        }
    }

    /// <summary>
    /// 외부에서 수집한 collider 후보가 hit 가능한지 확인한다.
    /// 상태는 변경하지 않는다.
    /// </summary>
    public bool CanHit(Transform owner, Collider2D other)
    {
        if (_hitController == null)
        {
            Debug.LogError("HitController is not initialized. Ensure hitConfig is set and Initialize was called.", this);
            return false;
        }

        if (owner == null || other == null)
            return false;

        return _hitController.CanHit(owner, other);
    }

    /// <summary>
    /// 외부에서 수집한 collider 후보를 hit로 등록한다.
    /// 내부적으로 CanHit 검사 후 성공 시 히트 상태를 기록한다.
    /// </summary>
    public bool TryRegisterHit(Transform owner, Collider2D other)
    {
        if (_hitController == null)
        {
            Debug.LogError("HitController is not initialized. Ensure hitConfig is set and Initialize was called.", this);
            return false;
        }

        if (owner == null || other == null)
            return false;

        return _hitController.TryRegisterHit(owner, other);
    }

    /// <summary>
    /// 검사 + 등록을 한 번에 수행하는 편의 메서드.
    /// </summary>
    public bool TryProcessHit(Transform owner, Collider2D other)
    {
        if (!TryRegisterHit(owner, other))
            return false;

        ApplyDamage(other);
        return true;
    }

    /// <summary>
    /// 현재 히트 상태를 초기화한다.
    /// 풀링 재사용이나 스킬 재시작 시 사용한다.
    /// </summary>
    public void ResetHitState()
    {
        if (_hitController == null)
        {
            Debug.LogError("HitController is not initialized. Ensure hitConfig is set and Initialize was called.", this);
            return;
        }

        foreach (var pair in _activeSplitDamageRoutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
            }
        }
        _activeSplitDamageRoutines.Clear();

        _splitHitCount = 1;
        _splitHitInterval = 0f;
        _useKnockback = false;
        _knockbackForce = 0f;
        _context = default;

        _elapsedTime = 0f;
        _hitWindowClosed = false;
        _hitController.ResetState();
        _damageProfile = null;
    }

    private void OnDisable()
    {
        foreach (var pair in _activeSplitDamageRoutines)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
            }
        }
        _activeSplitDamageRoutines.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        var circle = GetComponent<CircleCollider2D>();
        if (circle == null)
            return;

        Vector3 center = transform.TransformPoint(circle.offset);
        float radius = circle.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        Gizmos.DrawWireSphere(center, radius);
    }
}