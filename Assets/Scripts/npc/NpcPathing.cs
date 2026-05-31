using Character;
using Npc.Service;
using Stat;
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
    [SerializeField] private AnimationMono animationMono;
    [SerializeField] private CharacterManager characterManager;

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

    private Vector2 _spawnPosition;
    private bool _isHoldingAtRange;
    private Vector2 _currentDesiredPoint;
    private float _currentDesiredStopDistance;
    private float _stableAngleBiasDegrees;
    private float _stableDistanceBias01;
    private float _stableLateralBias;
    private float _decisionIntervalBias;
    private readonly NpcRangePositioningService _rangePositioningService =
        new NpcRangePositioningService();
    private readonly NpcFlyingMovementService _flyingMovementService =
        new NpcFlyingMovementService();
    private readonly NpcWanderService _wanderService =
        new NpcWanderService();
    private readonly NpcMovementAnimationService _movementAnimationService =
        new NpcMovementAnimationService();

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
            animationMono = GetComponentInChildren<AnimationMono>();

        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();
        if (characterManager == null)
            characterManager = GetComponentInParent<CharacterManager>();

        SyncMovementControllerConfig();

        InitializeStableSpreadBias();
        _rangePositioningService.Reset();
        _spawnPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        _isHoldingAtRange = false;
        InitializeFlyingService();
        _wanderService.Reset();
        _movementAnimationService.Reset();
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
        if (IsMovementLocked())
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
                HandleFlyingMovement(target);
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

        NpcWanderService.Result wanderResult =
            _wanderService.Evaluate(CreateWanderContext());

        if (!wanderResult.shouldMove)
        {
            StopMovement();
            UpdateMovementAnimation();
            return;
        }

        MoveInDirection(wanderResult.moveDirection);
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
        _wanderService.Evaluate(CreateWanderContext());
    }

    private NpcWanderService.Context CreateWanderContext()
    {
        return new NpcWanderService.Context
        {
            deltaTime = Time.deltaTime,
            wanderDuration = wanderInterval,
            pauseDuration = pauseBetweenWanders,
            canWander = enableWanderWhenNoTarget
        };
    }

    private void InitializeFlyingService()
    {
        if (!IsFlyingArchetype())
            return;

        _flyingMovementService.Reset();
        _flyingMovementService.Initialize(CreateFlyingContext(targeting != null ? targeting.GetCurrentTarget() : null));
    }

    private void HandleFlyingMovement(Transform target)
    {
        NpcFlyingMovementService.Result result =
            _flyingMovementService.Evaluate(CreateFlyingContext(target));

        if (result.shouldDespawn)
        {
            Destroy(gameObject);
            return;
        }

        if (!result.hasMoveDirection)
        {
            StopMovement();
            return;
        }

        MoveInDirection(result.moveDirection);
    }

    private NpcFlyingMovementService.Context CreateFlyingContext(Transform target)
    {
        Vector2 currentPosition = _rb != null
            ? _rb.position
            : (Vector2)transform.position;

        return new NpcFlyingMovementService.Context
        {
            selfPosition = currentPosition,
            spawnPosition = _spawnPosition,
            configuredDirection = Vector2.right,
            target = target,
            useTargetDirectionOnInitialize = true,
            autoDespawnByDistance = true,
            autoDespawnDistance = Mathf.Max(1f, flyingForwardLifetimeDistance)
        };
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
        if (characterManager == null)
        {
            characterManager = GetComponent<CharacterManager>();

            if (characterManager == null)
                characterManager = GetComponentInParent<CharacterManager>();
        }

        if (characterManager != null)
        {
            float statMoveSpeed =
                characterManager.GetStatValue(StatType.MoveSpeed);

            if (statMoveSpeed > 0f)
            {
                return statMoveSpeed;
            }
        }

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

        float stopDistance = GetStopDistanceForCurrentArchetype();
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

        float stopDistance = GetStopDistanceForCurrentArchetype();
        if (stopDistance <= 0.01f)
            return;

        Vector2 pos = _rb.position;
        Vector2 targetPos = target.position;

        NpcRangePositioningService.Context context =
            new NpcRangePositioningService.Context
            {
                selfPosition = pos,
                targetPosition = targetPos,
                targetTransform = target,
                preferredRange = stopDistance,
                stopDistance = stopDistance,
                minDistance = Mathf.Max(0.1f, stopDistance * Mathf.Clamp01(1f - stopDistanceInwardJitterRatio)),
                maxDistance = stopDistance,
                decisionInterval = stopJitterRefreshInterval + _decisionIntervalBias,
                repathDistance = Mathf.Max(0.5f, stopDistance * 0.5f),
                ringMoveAngle = _stableAngleBiasDegrees,
                spacingRadius = Mathf.Max(0.8f, stopDistance * (0.45f + stopDistanceInwardJitterRatio * 0.6f)),
                spacingWeight = 1.15f,
                stopJitterRadius = Mathf.Max(0.35f, stopDistance * 0.55f) * Mathf.Abs(_stableLateralBias),
                useRingMovement = true,
                useLocalSpacing = true,
                useStopJitter = useDynamicStopJitter,
                time = Time.time
            };

        NpcRangePositioningService.Result result =
            _rangePositioningService.Evaluate(context);

        _currentDesiredStopDistance = result.desiredStopDistance;
        _currentDesiredPoint = result.desiredPoint;

        if (result.shouldStop)
        {
            _isHoldingAtRange = true;
            StopMovement();
            return;
        }

        _isHoldingAtRange = false;

        if (!result.hasDesiredPoint)
        {
            StopMovement();
            return;
        }

        Vector2 toTarget = targetPos - pos;
        float distToTarget = toTarget.magnitude;
        float enterDistance = stopDistance + Mathf.Max(0.05f, rangeControlEnterBuffer);

        if (distToTarget > enterDistance)
        {
            MoveTowardPoint(result.desiredPoint);
            return;
        }

        MoveAlongTargetRing(target, stopDistance, result.desiredPoint);
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

    private bool IsMovementLocked()
    {
        if (characterManager == null)
        {
            characterManager = GetComponent<CharacterManager>();

            if (characterManager == null)
                characterManager = GetComponentInParent<CharacterManager>();
        }

        if (characterManager != null && !characterManager.CanMove)
        {
            return true;
        }

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




    private void ResetRangePositioning()
    {
        _currentDesiredPoint = Vector2.zero;
        _currentDesiredStopDistance = 0f;
        _rangePositioningService.ForceRepath();
    }


    public void ForceRepath()
    {
        _isHoldingAtRange = false;
        StopMovement();
        ResetRangePositioning();
        _rangePositioningService.ForceRepath();
    }

    public void SetArchetype(PathingArchetype newArchetype)
    {
        archetype = newArchetype;
        InitializeStableSpreadBias();
        _isHoldingAtRange = false;
        _spawnPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        _flyingMovementService.Reset();
        InitializeFlyingService();
        SyncMovementControllerConfig();
        ResetRangePositioning();
        _rangePositioningService.Reset();
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
            Vector3 dir = _flyingMovementService.HasMoveDirection
                ? _flyingMovementService.MoveDirection
                : Vector2.right;
            Gizmos.DrawLine(start, start + dir * flyingForwardLifetimeDistance);
        }
    }
#endif
}