using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles NPC pathfinding and chase movement.
/// - Simple direct chase movement for non-flying NPCs
/// - Flying archetypes choose a random valid target on spawn and move in a straight line
/// - Flying archetypes auto-despawn after passing away from their spawn-side lane
/// - Supports stop-distance behavior for melee / ranged / siege / flying types
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class NpcPathing : MonoBehaviour
{
    public enum PathingArchetype
    {
        Normal,
        Melee,
        Ranged,
        Siege,
        Flying
    }

    [Header("References")]
    [SerializeField] private NpcTargeting targeting;
    [SerializeField] private NpcMovementProfile movementProfile;
    [SerializeField] private SkillExecutorMono skillExecutor;
    [SerializeField] private MovementController movementController;
    [SerializeField] private venus.eldawn.party.AnimationMono animationMono;

    [Header("Movement")]
    [SerializeField] private PathingArchetype archetype = PathingArchetype.Normal;
    [SerializeField] private float chaseRange = 1000f;
    [SerializeField] private float holdExitDistanceBuffer = 0.4f;
    [SerializeField] private float rangeControlEnterBuffer = 1.5f;
    [SerializeField] private bool useDynamicStopJitter = true;
    [SerializeField, Range(0f, 1f)] private float stopDistanceInwardJitterRatio = 0.25f;
    [SerializeField, Range(0f, 89f)] private float stopAngleJitterDegrees = 18f;
    [SerializeField, Min(0.01f)] private float stopJitterRefreshInterval = 0.15f;


    [Header("Flying Straight Move")]
    [SerializeField] private float flyingDespawnBacktrackDistance = 3f;
    [SerializeField] private float flyingForwardLifetimeDistance = 40f;

    [Header("Wander")]
    [SerializeField] private bool enableWanderWhenNoTarget = true;
    [SerializeField] private float wanderInterval = 2.0f;
    [SerializeField] private float pauseBetweenWanders = 0.35f;


    private Rigidbody2D _rb;
    private Vector2 _wanderDir = Vector2.right;
    private float _wanderTimer;
    private float _pauseTimer;
    private bool _isPausing;

    private Vector2 _spawnPosition;
    private Vector2 _flyingMoveDirection = Vector2.right;
    private bool _hasFlyingMoveDirection;
    private bool _isHoldingAtRange;
    private Vector2 _currentDesiredPoint;
    private float _currentDesiredStopDistance;
    private Transform _lastStopJitterTarget;
    private float _stableAngleBiasDegrees;
    private float _stableDistanceBias01;
    private float _stableLateralBias;
    private float _decisionIntervalBias;
    private float _nextDecisionTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (targeting == null)
            targeting = GetComponent<NpcTargeting>();

        if (movementProfile == null)
            movementProfile = GetComponent<NpcMovementProfile>();

        if (skillExecutor == null)
            skillExecutor = GetComponent<SkillExecutorMono>();

        if (movementController == null)
            movementController = GetComponent<MovementController>();
        if (movementController == null)
            movementController = gameObject.AddComponent<MovementController>();

        if (animationMono == null)
            animationMono = GetComponentInChildren<venus.eldawn.party.AnimationMono>();

        SyncMovementControllerConfig();

        InitializeStableSpreadBias();
        ScheduleNextDecision(true);
        _spawnPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        _isHoldingAtRange = false;
        InitializeFlyingDirection();
        PickNewWanderDirection(true);
    }

    private void Update()
    {
        Transform target = targeting != null ? targeting.GetCurrentTarget() : null;
        bool canChase = target != null && Vector2.Distance(transform.position, target.position) <= chaseRange;
        bool shouldHoldPosition = canChase && ShouldHoldPositionNearTarget(target, true) && !IsFlyingArchetype();

        if (!canChase && enableWanderWhenNoTarget)
        {
            UpdateWanderState();
        }
    }

    private void FixedUpdate()
    {
        if (IsMovementLockedByAttack())
        {
            StopMovement();
            UpdateMovementAnimation();
            return;
        }

        Transform target = targeting != null ? targeting.GetCurrentTarget() : null;
        bool canChase = target != null && Vector2.Distance(transform.position, target.position) <= chaseRange;
        bool shouldHoldPosition = canChase && ShouldHoldPositionNearTarget(target, false) && !IsFlyingArchetype();

        if (canChase)
        {
            if (IsFlyingArchetype())
            {
                MoveFlyingStraight();
                CheckFlyingAutoDespawn();
                UpdateMovementAnimation();
                return;
            }

            if (UsesDirectRangeChase())
            {
                MoveWithRangeControl(target);
                UpdateMovementAnimation();
                return;
            }

            if (shouldHoldPosition)
            {
                StopMovement();
                UpdateMovementAnimation();
                return;
            }

            MoveStraightTowardTarget(target);
            UpdateMovementAnimation();
            return;
        }

        if (!enableWanderWhenNoTarget || _isPausing)
        {
            StopMovement();
            UpdateMovementAnimation();
            return;
        }

        MoveInDirection(_wanderDir);
        UpdateMovementAnimation();
    }
    private bool UsesDirectRangeChase()
    {
        if (movementProfile != null)
            return movementProfile.IsRanged() || movementProfile.IsSiege();

        return archetype == PathingArchetype.Ranged || archetype == PathingArchetype.Siege;
    }
    private void UpdateWanderState()
    {
        if (_isPausing)
        {
            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer <= 0f)
            {
                _isPausing = false;
                PickNewWanderDirection(false);
            }
            return;
        }

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            _isPausing = true;
            _pauseTimer = Mathf.Max(0.01f, pauseBetweenWanders);
        }
    }

    private void PickNewWanderDirection(bool immediate)
    {
        Vector2 random = Random.insideUnitCircle;
        if (random.sqrMagnitude <= 0.0001f)
            random = Vector2.right;

        _wanderDir = random.normalized;
        _wanderTimer = immediate ? Mathf.Max(0.25f, wanderInterval * 0.5f) : Mathf.Max(0.25f, wanderInterval);
        _isPausing = false;
        _pauseTimer = 0f;
    }

    private void InitializeFlyingDirection()
    {
        if (!IsFlyingArchetype())
            return;

        Transform target = targeting != null ? targeting.GetCurrentTarget() : null;
        if (target == null)
        {
            _flyingMoveDirection = Vector2.right;
            _hasFlyingMoveDirection = true;
            return;
        }

        Vector2 from = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 to = (Vector2)target.position - from;
        if (to.sqrMagnitude <= 0.0001f)
            to = Vector2.right;

        _flyingMoveDirection = to.normalized;
        _hasFlyingMoveDirection = true;
    }

    private void MoveFlyingStraight()
    {
        if (!_hasFlyingMoveDirection || _flyingMoveDirection.sqrMagnitude <= 0.0001f)
            InitializeFlyingDirection();

        Vector2 dir = _flyingMoveDirection.sqrMagnitude <= 0.0001f ? Vector2.right : _flyingMoveDirection.normalized;
        MoveInDirection(dir);
    }

    private void CheckFlyingAutoDespawn()
    {
        if (!IsFlyingArchetype())
            return;

        Vector2 currentPos = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 fromSpawn = currentPos - _spawnPosition;
        float forwardProgress = Vector2.Dot(fromSpawn, _flyingMoveDirection);

        if (forwardProgress <= -Mathf.Max(0.1f, flyingDespawnBacktrackDistance))
        {
            Destroy(gameObject);
            return;
        }

        if (forwardProgress >= Mathf.Max(1f, flyingForwardLifetimeDistance))
        {
            Destroy(gameObject);
        }
    }
    private float GetStopDistanceForCurrentArchetype()
    {
        float skillRange = GetCurrentSkillRange();
        if (skillRange > 0f)
            return skillRange;

        if (movementProfile != null)
            return movementProfile.GetStopDistance();

        switch (archetype)
        {
            case PathingArchetype.Flying:
                return 0.4f;
            case PathingArchetype.Ranged:
                return 3.5f;
            case PathingArchetype.Siege:
                return 4.5f;
            case PathingArchetype.Melee:
            default:
                return 0.6f;
        }
    }


    private float GetMoveSpeed()
    {
        if (movementProfile != null)
            return movementProfile.GetMoveSpeed();

        switch (archetype)
        {
            case PathingArchetype.Flying:
                return 3.1f;
            case PathingArchetype.Siege:
                return 1.55f;
            case PathingArchetype.Ranged:
                return 2.1f;
            case PathingArchetype.Melee:
                return 2.7f;
            default:
                return 2.2f;
        }
    }

    private bool IsFlyingArchetype()
    {
        if (movementProfile != null)
            return movementProfile.IsFlying();

        return archetype == PathingArchetype.Flying;
    }

    private bool ShouldHoldPositionNearTarget(Transform target, bool allowStateWrite)
    {
        if (target == null)
        {
            if (allowStateWrite)
                _isHoldingAtRange = false;
            return false;
        }

        float stopDistance = GetDesiredStopDistance(target);
        if (stopDistance <= 0f)
        {
            if (allowStateWrite)
                _isHoldingAtRange = false;
            return false;
        }

        float dist = Vector2.Distance(transform.position, target.position);
        float exitDistance = stopDistance + Mathf.Max(0f, holdExitDistanceBuffer);

        bool currentHoldState = _isHoldingAtRange;
        bool nextHoldState;

        if (currentHoldState)
            nextHoldState = dist <= exitDistance;
        else
            nextHoldState = dist <= stopDistance;

        if (allowStateWrite)
            _isHoldingAtRange = nextHoldState;

        return nextHoldState;
    }


    private float GetCurrentSkillRange()
    {
        BattleSkillBase skill = GetCurrentAttackSkill() as BattleSkillBase;
        if (skill == null)
            return 0f;

        return Mathf.Max(0f, skill.Range);
    }

    private ScriptableObject GetCurrentAttackSkill()
    {
        if (skillExecutor == null)
            return null;

        return skillExecutor.GetBasicAttackSkill();
    }

    private void MoveStraightTowardTarget(Transform target)
    {
        if (target == null)
            return;

        Vector2 pos = _rb.position;
        Vector2 toTarget = (Vector2)target.position - pos;
        float dist = toTarget.magnitude;

        if (dist <= 0.03f)
        {
            StopMovement();
            return;
        }

        MoveTowardPoint(target.position);
    }
    private void MoveWithRangeControl(Transform target)
    {
        if (_rb == null || target == null)
            return;

        float stopDistance = GetDesiredStopDistance(target);
        if (stopDistance <= 0.01f)
            stopDistance = GetStopDistanceForCurrentArchetype();
        if (stopDistance <= 0.01f)
            return;

        Vector2 pos = _rb.position;
        Vector2 targetPos = target.position;
        Vector2 desiredPoint = GetDesiredPoint(target);
        Vector2 toDesired = desiredPoint - pos;
        float distToDesired = toDesired.magnitude;

        Vector2 toTarget = targetPos - pos;
        float distToTarget = toTarget.magnitude;
        if (distToTarget <= 0.0001f)
            return;

        float enterDistance = stopDistance + Mathf.Max(0.05f, rangeControlEnterBuffer);
        bool nearPreferredBand = distToTarget <= enterDistance;

        // Far away: move toward the current desired point.
        if (!nearPreferredBand)
        {
            if (distToDesired <= 0.0001f)
            {
                StopMovement();
                return;
            }

            MoveTowardPoint(desiredPoint);
            return;
        }

        // Near the preferred ring: keep sliding around the target instead of freezing into one point.
        MoveAlongTargetRing(target, stopDistance, desiredPoint);
    }
    private void MoveAlongTargetRing(Transform target, float stopDistance, Vector2 desiredPoint)
    {
        if (_rb == null || target == null)
            return;

        Vector2 pos = _rb.position;
        Vector2 targetPos = target.position;
        Vector2 fromTarget = pos - targetPos;
        float currentRadius = fromTarget.magnitude;
        if (currentRadius <= 0.0001f)
            fromTarget = Vector2.right;
        else
            fromTarget /= currentRadius;

        Vector2 radial = fromTarget;
        Vector2 tangent = new Vector2(-radial.y, radial.x);
        float tangentSign = Mathf.Sign(_stableLateralBias == 0f ? 1f : _stableLateralBias);
        tangent *= tangentSign;

        float radialError = stopDistance - currentRadius;
        Vector2 radialCorrection = radial * Mathf.Clamp(radialError, -0.65f, 0.65f);

        Vector2 towardDesired = desiredPoint - pos;
        Vector2 desiredBlend = towardDesired.sqrMagnitude > 0.0001f ? towardDesired.normalized : Vector2.zero;

        Vector2 moveDir = (tangent * 1.15f + radialCorrection * 1.4f + desiredBlend * 0.45f).normalized;
        if (moveDir.sqrMagnitude <= 0.0001f)
        {
            StopMovement();
            return;
        }

        MoveInDirection(moveDir, 0.75f);
    }
    private void MoveTowardPoint(Vector2 targetPoint)
    {
        if (movementController == null)
            return;

        SyncMovementControllerConfig();
        movementController.MoveTo(targetPoint);
    }

    private void MoveInDirection(Vector2 direction, float speedMultiplier = 1f)
    {
        if (movementController == null)
            return;

        SyncMovementControllerConfig(speedMultiplier);
        movementController.MoveByDirection(direction);
    }

    private void StopMovement()
    {
        if (movementController != null)
            movementController.Stop();
    }

    private void SyncMovementControllerConfig(float speedMultiplier = 1f)
    {
        if (movementController == null)
            return;

        movementController.SetMoveSpeed(GetMoveSpeed() * Mathf.Max(0f, speedMultiplier));
        movementController.SetArriveDistance(0.03f);
    }

    private bool IsMovementLockedByAttack()
    {
        return animationMono != null && animationMono.IsPlayingAttack();
    }
    private void UpdateMovementAnimation()
    {
        if (animationMono == null || movementController == null)
            return;

        if (animationMono.IsPlayingAttack())
            return;

        if (movementController.IsMoving)
            animationMono.PlayMove();
        else
            animationMono.PlayIdle();
    }

    private float GetDesiredStopDistance(Transform target)
    {
        RefreshDynamicStopJitter(target);
        return _currentDesiredStopDistance > 0f ? _currentDesiredStopDistance : GetStopDistanceForCurrentArchetype();
    }

    private Vector2 GetDesiredPoint(Transform target)
    {
        RefreshDynamicStopJitter(target);
        return _currentDesiredPoint;
    }

    private void RefreshDynamicStopJitter(Transform target)
    {
        if (target == null)
        {
            ResetDynamicStopJitter();
            return;
        }

        float baseStopDistance = GetStopDistanceForCurrentArchetype();
        if (baseStopDistance <= 0.01f)
        {
            _currentDesiredStopDistance = 0f;
            _currentDesiredPoint = target.position;
            return;
        }

        bool isDynamicArchetype = UsesDirectRangeChase();
        bool targetChanged = _lastStopJitterTarget != target;
        bool decisionTimeReached = Time.time >= _nextDecisionTime;
        bool desiredPointInvalid = _currentDesiredStopDistance <= 0.01f;
        bool reachedDesiredPoint = _rb != null && Vector2.Distance(_rb.position, _currentDesiredPoint) <= 0.35f;

        bool needsRefresh = targetChanged
            || decisionTimeReached
            || desiredPointInvalid
            || reachedDesiredPoint;

        if (!isDynamicArchetype || !useDynamicStopJitter)
        {
            _currentDesiredStopDistance = baseStopDistance;
            Vector2 baseDirSimple = GetSpreadBaseDirection(target);
            _currentDesiredPoint = BuildDesiredPoint(target, baseDirSimple, _currentDesiredStopDistance);
            _lastStopJitterTarget = target;
            ScheduleNextDecision(false);
            return;
        }

        if (!needsRefresh)
            return;

        float inwardMin = Mathf.Max(0.1f, baseStopDistance * Mathf.Clamp01(1f - stopDistanceInwardJitterRatio));
        float inwardMax = Mathf.Max(inwardMin, baseStopDistance);

        float stableDesiredDistance = Mathf.Lerp(inwardMin, inwardMax, _stableDistanceBias01);
        float dynamicDesiredDistance = Random.Range(inwardMin, inwardMax);
        float desiredStopDistance = Mathf.Lerp(stableDesiredDistance, dynamicDesiredDistance, 0.35f);

        Vector2 baseDirDynamic = GetSpreadBaseDirection(target);
        float dynamicAngle = Random.Range(-stopAngleJitterDegrees, stopAngleJitterDegrees);
        float angle = _stableAngleBiasDegrees + dynamicAngle;
        Vector2 jitteredDir = RotateVector(baseDirDynamic, angle).normalized;
        if (jitteredDir.sqrMagnitude <= 0.0001f)
            jitteredDir = baseDirDynamic;

        jitteredDir = ApplyLocalSpacingBias(target, jitteredDir, stopDistanceInwardJitterRatio, desiredStopDistance);
        if (jitteredDir.sqrMagnitude <= 0.0001f)
            jitteredDir = baseDirDynamic;

        _currentDesiredStopDistance = Mathf.Min(baseStopDistance, desiredStopDistance);
        _currentDesiredPoint = BuildDesiredPoint(target, jitteredDir, _currentDesiredStopDistance);
        _lastStopJitterTarget = target;
        ScheduleNextDecision(false);
    }

    private void InitializeStableSpreadBias()
    {
        int seed = gameObject.GetInstanceID();
        uint hashA = (uint)(seed * 1103515245 + 12345);
        uint hashB = (uint)(seed * 214013 + 2531011);

        float tA = (hashA & 0xFFFF) / 65535f;
        float tB = (hashB & 0xFFFF) / 65535f;

        _stableAngleBiasDegrees = Mathf.Lerp(-stopAngleJitterDegrees, stopAngleJitterDegrees, tA);
        _stableDistanceBias01 = tB;
        _stableLateralBias = Mathf.Lerp(-0.95f, 0.95f, ((hashA >> 8) & 0xFFFF) / 65535f);
        _decisionIntervalBias = Mathf.Lerp(-0.06f, 0.08f, ((hashB >> 8) & 0xFFFF) / 65535f);
    }
    private void ScheduleNextDecision(bool immediate)
    {
        float baseInterval = Mathf.Max(0.01f, stopJitterRefreshInterval);
        float resolvedInterval = Mathf.Max(0.05f, baseInterval + _decisionIntervalBias + Random.Range(-0.02f, 0.04f));
        _nextDecisionTime = immediate ? Time.time : Time.time + resolvedInterval;
    }

    private Vector2 ApplyLocalSpacingBias(Transform target, Vector2 currentDir, float inwardRatio, float desiredStopDistance)
    {
        if (_rb == null || target == null)
            return currentDir;

        float baseStopDistance = GetStopDistanceForCurrentArchetype();
        if (baseStopDistance <= 0.01f)
            return currentDir;

        float spacingRadius = Mathf.Max(0.8f, baseStopDistance * (0.45f + inwardRatio * 0.6f));
        Collider2D[] nearby = Physics2D.OverlapCircleAll(_rb.position, spacingRadius);
        if (nearby == null || nearby.Length == 0)
            return currentDir;

        Vector2 targetPos = target.position;
        Vector2 myDesiredPoint = targetPos + currentDir * Mathf.Min(baseStopDistance, desiredStopDistance);
        Vector2 separation = Vector2.zero;
        int count = 0;

        for (int i = 0; i < nearby.Length; i++)
        {
            Collider2D col = nearby[i];
            if (col == null)
                continue;

            NpcPathing other = col.GetComponentInParent<NpcPathing>();
            if (other == null || other == this)
                continue;

            if (other.archetype != archetype)
                continue;

            Vector2 delta = _rb.position - (Vector2)other.transform.position;
            float dist = delta.magnitude;
            if (dist > spacingRadius)
                continue;

            if (dist > 0.0001f)
            {
                float bodyWeight = 1f - Mathf.Clamp01(dist / spacingRadius);
                separation += delta.normalized * bodyWeight;
                count++;
            }

            if (other._currentDesiredStopDistance > 0.01f)
            {
                Vector2 desiredDelta = myDesiredPoint - other._currentDesiredPoint;
                float desiredDist = desiredDelta.magnitude;
                float desiredSpacing = Mathf.Max(0.75f, baseStopDistance * 0.22f);
                if (desiredDist <= desiredSpacing)
                {
                    Vector2 pushDir = desiredDist > 0.0001f ? desiredDelta / desiredDist : new Vector2(-currentDir.y, currentDir.x);
                    float desiredWeight = 1f - Mathf.Clamp01(desiredDist / desiredSpacing);
                    separation += pushDir * (desiredWeight * 1.6f);
                    count++;
                }
            }
        }

        if (count == 0 || separation.sqrMagnitude <= 0.0001f)
            return currentDir;

        Vector2 tangent = new Vector2(-currentDir.y, currentDir.x) * Mathf.Sign(_stableLateralBias == 0f ? 1f : _stableLateralBias);
        Vector2 blended = (currentDir + separation.normalized * 1.15f + tangent * 0.35f).normalized;
        return blended.sqrMagnitude > 0.0001f ? blended : currentDir;
    }
    

    private Vector2 BuildDesiredPoint(Transform target, Vector2 radialDir, float stopDistance)
    {
        Vector2 targetPos = target != null ? (Vector2)target.position : Vector2.zero;
        Vector2 radial = radialDir.sqrMagnitude <= 0.0001f ? Vector2.right : radialDir.normalized;
        Vector2 tangent = new Vector2(-radial.y, radial.x);

        float lateralDistance = Mathf.Max(0.35f, stopDistance * 0.55f) * _stableLateralBias;
        return targetPos + radial * stopDistance + tangent * lateralDistance;
    }

    private Vector2 GetPreferredTargetDirection(Transform target)
    {
        Vector2 targetPos = target != null ? (Vector2)target.position : Vector2.zero;
        Vector2 fromTargetToSelf = (_rb != null ? _rb.position : (Vector2)transform.position) - targetPos;
        if (fromTargetToSelf.sqrMagnitude <= 0.0001f)
            return Vector2.right;

        return fromTargetToSelf.normalized;
    }

    private void ResetDynamicStopJitter()
    {
        _currentDesiredPoint = Vector2.zero;
        _currentDesiredStopDistance = 0f;
        _lastStopJitterTarget = null;
        ScheduleNextDecision(true);
    }

    private static Vector2 RotateVector(Vector2 v, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos);
    }

    public void ForceRepath()
    {
        _isHoldingAtRange = false;
        StopMovement();
        ResetDynamicStopJitter();
    }

    public void SetArchetype(PathingArchetype newArchetype)
    {
        archetype = newArchetype;
        InitializeStableSpreadBias();
        ScheduleNextDecision(true);
        _isHoldingAtRange = false;
        _spawnPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        _hasFlyingMoveDirection = false;
        InitializeFlyingDirection();
        SyncMovementControllerConfig();
        ResetDynamicStopJitter();
        ForceRepath();
    }

    public void SetMoveSpeed(float value)
    {
        // Movement speed is now sourced from NpcMovementProfile.
        // This method remains only for backward compatibility.
    }

    public void SetStopDistances(float melee, float ranged, float siege, float flying)
    {
        // Stop distances are now sourced from NpcMovementProfile.
        // This method remains only for backward compatibility.
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (Application.isPlaying && IsFlyingArchetype())
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.9f);
            Vector3 start = _spawnPosition;
            Vector3 dir = _flyingMoveDirection.sqrMagnitude <= 0.0001f ? Vector2.right : _flyingMoveDirection.normalized;
            Gizmos.DrawLine(start, start + dir * flyingForwardLifetimeDistance);
        }
    }
#endif
    private Vector2 GetSpreadBaseDirection(Transform target)
    {
        Vector2 radial = GetPreferredTargetDirection(target);
        if (radial.sqrMagnitude <= 0.0001f)
            radial = Vector2.right;

        Vector2 tangent = new Vector2(-radial.y, radial.x);
        Vector2 blended = (radial + tangent * _stableLateralBias).normalized;
        return blended.sqrMagnitude > 0.0001f ? blended : radial;
    }
}