using Character;
using Stat;
using UnityEngine;
using Party;

/// <summary>
/// PartyMovementMono
///
/// New movement concept: independent party member movement.
///
/// Modes:
/// - PatrolAroundTower : patrol around the nearest tower
/// - SafePosition      : reposition away from the threat
/// - ProtectPosition   : hold a closer tactical position against the threat
/// - AggressivePosition: push toward the threat
///
/// Each party member moves independently.
/// There is no leader-follow behavior; the player can switch control between members.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementController))]
public class PartyMovementMono : MonoBehaviour
{
    public enum PositionMode
    {
        PatrolAroundTower,
        FormationHold,
        ReturnToDestination,
        SafePosition,
        ProtectPosition,
        AggressivePosition
    }

    public enum MovePhase
    {
        ComputePosition,
        Move,
        Stay
    }

    [Header("Mode")]
    [SerializeField] private PositionMode mode = PositionMode.ProtectPosition;

    [Tooltip("If true, chooses mode automatically from simple heuristics (can be replaced by your AI engine later).")]
    [SerializeField] private bool autoMode = false;

    [Header("Tower Patrol")]
    [SerializeField] private float engageDistance = 5f;

    private const float MinCombatAwarenessDistance = 16f;
    private const float AggressiveRepathInterval = 0.2f;
    private const float ReturnModeRecheckInterval = 0.2f;
    private static readonly Vector2 DefaultDestinationPosition = Vector2.zero;
    private const float ReturnDestinationMoveRange = 2f;
    [SerializeField] private float patrolDistance = 2.4f;
    [SerializeField] private float patrolAreaRadius = 1.0f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("How close is considered 'arrived' to the target position.")]
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Tactical Radius")]
    [Tooltip("How far this member is allowed to reposition from its current location in one decision.")]
    [SerializeField] private float positionRadius = 3.0f;

    [Header("Safe Position")]
    [Tooltip("Preferred retreat distance when seeking a safe position.")]
    [SerializeField] private float safeDistance = 2.2f;
    [Tooltip("Random radius around the safe anchor point.")]
    [SerializeField] private float safeAreaRadius = 0.9f;

    [Header("Protect Position")]
    [Tooltip("Preferred combat distance when holding a protective position.")]
    [SerializeField] private float protectDistance = 1.8f;
    [Tooltip("Random radius around the protect anchor point.")]
    [SerializeField] private float protectAreaRadius = 0.8f;

    [Header("Aggressive Position")]
    private const float AggressiveDistance = 1f;
    private const float AggressiveAreaRadius = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool debugDraw = false;

    [Header("Cycle")]
    [SerializeField] private float decisionInterval = 3f;
    private const float PositionJitterStrength = 0.2f;

    [Header("Separation")]
    private const float SeparationRadius = 0.75f;
    private const float SeparationStrength = 0.25f;


    [Tooltip("Layer mask used to find nearby party members for separation.")]
    [SerializeField] private LayerMask partyMask = ~0;

    [Tooltip("Layer mask used to avoid walls and screen-side blockers.")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Party Anchor")]
    [Tooltip("Maximum distance this member's target position can be from the party anchor. 0 disables anchor clamping.")]
    [SerializeField] private float maxDistanceFromPartyAnchor = 3.5f;

    [Header("Formation Hold")]
    [SerializeField] private float formationRadius = 2.2f;
    [SerializeField] private float formationSlotSpacing = 1.1f;

    private MovePhase _phase = MovePhase.ComputePosition;
    private float _phaseTimer = 0f;

    private Rigidbody2D _rb;
    private MovementController _movementController;
    private PerceptionMono _perception;
    private AnimationMono _animationMono;
    private CharacterManager _characterManager;

    // private Transform _cachedTower;
    private PartyMovementPositionService _positionService;
    private PartyMovementExecutionService _executionService;
    private FormationHoldModeService _formationHoldModeService;
    private ReturnToDestinationModeService _returnToDestinationModeService;
    private AggressivePositionModeService _aggressivePositionModeService;
    private SafePositionModeService _safePositionModeService;
    private ProtectPositionModeService _protectPositionModeService;
    private PatrolAroundTowerModeService _patrolAroundTowerModeService;

    private Vector2 _targetPos;
    private Vector2 _jitterDir;
    private float _jitter01;

    private float _debugLogTimer = 0f;
    private float _aggressiveRepathTimer = 0f;
    private float _returnModeRecheckTimer = 0f;

    private bool _isMovementControlledByPlayer = false;
    private Vector2 _manualMoveInput = Vector2.zero;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _movementController = GetComponent<MovementController>();
        if (_movementController == null)
            _movementController = gameObject.AddComponent<MovementController>();
        _perception = GetComponent<PerceptionMono>();
        _animationMono = GetComponentInChildren<AnimationMono>();
        _characterManager = GetComponent<CharacterManager>();

        if (_characterManager == null)
            _characterManager = GetComponentInParent<CharacterManager>();

        // Stable per-agent randomization so party members don't collapse to one point.
        float ang = Random.Range(0f, Mathf.PI * 2f);
        _jitterDir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        _jitter01 = Random.Range(0.35f, 1f);
        _positionService = new PartyMovementPositionService(this);
        _executionService = new PartyMovementExecutionService(this);
        _formationHoldModeService = new FormationHoldModeService();
        _returnToDestinationModeService = new ReturnToDestinationModeService();
        _aggressivePositionModeService = new AggressivePositionModeService();
        _safePositionModeService = new SafePositionModeService();
        _protectPositionModeService = new ProtectPositionModeService();
        _patrolAroundTowerModeService = new PatrolAroundTowerModeService();
        SyncMovementControllerConfig();
    }

    public void SetMode(PositionMode newMode)
    {
        mode = newMode;
    }

    public PositionMode GetMode() => mode;

    public void SetMovementControlByPlayer(bool enabled)
    {
        _isMovementControlledByPlayer = enabled;

        if (!enabled)
        {
            _manualMoveInput = Vector2.zero;
            _phase = MovePhase.ComputePosition;
            _phaseTimer = 0f;
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
        }
        else
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
        }
    }

    public bool IsMovementControlledByPlayer() => _isMovementControlledByPlayer;

    public void SetManualMoveInput(Vector2 input)
    {
        _manualMoveInput = Vector2.ClampMagnitude(input, 1f);
    }

    private void Update()
    {
        if (_isMovementControlledByPlayer)
            return;

        _phaseTimer += Time.deltaTime;
        _debugLogTimer += Time.deltaTime;
        _aggressiveRepathTimer += Time.deltaTime;
        _returnModeRecheckTimer += Time.deltaTime;

        switch (_phase)
        {
            case MovePhase.ComputePosition:

                if (autoMode)
                    AutoSelectMode();

                // Decide target position from this member's current tactical context
                _targetPos = ComputeTargetPosition(mode);
                _aggressiveRepathTimer = 0f;
                _returnModeRecheckTimer = 0f;
                LogMovementDebugIfNeeded("ComputePosition");

                _phase = MovePhase.Move;
                break;

            case MovePhase.Move:

                if (mode == PositionMode.ReturnToDestination &&
                    autoMode &&
                    _returnModeRecheckTimer >= ReturnModeRecheckInterval)
                {
                    PositionMode resolvedMode = ResolveAutoMode();
                    _returnModeRecheckTimer = 0f;

                    if (resolvedMode != PositionMode.ReturnToDestination)
                    {
                        mode = resolvedMode;
                        _phase = MovePhase.ComputePosition;
                        _phaseTimer = 0f;
                        break;
                    }
                }

                if (mode == PositionMode.AggressivePosition &&
                    _aggressiveRepathTimer >= AggressiveRepathInterval)
                {
                    _targetPos = ComputeTargetPosition(mode);
                    _aggressiveRepathTimer = 0f;
                    LogMovementDebugIfNeeded("AggressiveRepath");
                }

                float dist = Vector2.Distance(transform.position, _targetPos);

                if (dist <= arriveDistance)
                {
                    _phase = MovePhase.Stay;
                    _phaseTimer = 0f;
                }
                break;

            case MovePhase.Stay:

                if (mode == PositionMode.ReturnToDestination && autoMode)
                {
                    PositionMode resolvedMode = ResolveAutoMode();

                    if (resolvedMode != PositionMode.ReturnToDestination)
                    {
                        mode = resolvedMode;
                        _phase = MovePhase.ComputePosition;
                        _phaseTimer = 0f;
                        break;
                    }
                }

                if (mode == PositionMode.AggressivePosition)
                {
                    _phase = MovePhase.ComputePosition;
                    _phaseTimer = 0f;
                    break;
                }
                // After interval re-evaluate position
                if (_phaseTimer >= decisionInterval)
                {
                    _phase = MovePhase.ComputePosition;
                    _phaseTimer = 0f;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_animationMono != null && _animationMono.IsPlayingAttack())
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
            return;
        }

        if (IsMovementBlocked())
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
            return;
        }

        if (_isMovementControlledByPlayer)
        {
            HandleManualMovement();
            return;
        }

        if (_phase == MovePhase.Move)
            MoveTo(_targetPos);
        else
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
        }
    }

    private bool IsMovementBlocked()
    {
        if (_characterManager == null)
        {
            _characterManager = GetComponent<CharacterManager>();

            if (_characterManager == null)
                _characterManager = GetComponentInParent<CharacterManager>();
        }

        return _characterManager != null && !_characterManager.CanMove;
    }

    private PositionMode ResolveAutoMode()
    {
        Transform closestEnemy = _perception != null
            ? _perception.ClosestEnemy
            : null;

        if (closestEnemy != null)
        {
            return PositionMode.AggressivePosition;
        }

        PartyManager partyManager = PartyManager.Instance;

        if (partyManager != null)
        {
            PartyAnchorService.PartyAnchorData anchorData =
                partyManager.GetPartyAnchorData();

            if (anchorData.HasZone)
            {
                return PositionMode.AggressivePosition;
            }
        }

        return PositionMode.ReturnToDestination;
    }

    private void AutoSelectMode()
    {
        mode = ResolveAutoMode();
    }

    private Vector2 ComputeTargetPosition(PositionMode m)
    {
        if (_positionService == null)
        {
            _positionService = new PartyMovementPositionService(this);
        }

        Vector2 selfPos = _rb != null
            ? _rb.position
            : (Vector2)transform.position;

        PartyAnchorService.PartyAnchorData anchorData = default;
        PartyManager partyManager = PartyManager.Instance;

        if (partyManager != null)
        {
            anchorData = partyManager.GetPartyAnchorData();
        }

        Vector2 partyAnchorPosition = anchorData.MemberCount > 0
            ? anchorData.AnchorPosition
            : selfPos;

        if (m == PositionMode.ReturnToDestination)
        {
            if (_returnToDestinationModeService == null)
            {
                _returnToDestinationModeService = new ReturnToDestinationModeService();
            }

            return _returnToDestinationModeService.ResolvePosition(
                new ReturnToDestinationModeService.Context
                {
                    DestinationPosition = DefaultDestinationPosition,
                    Time = Time.time,
                    MoveRange = ReturnDestinationMoveRange
                });
        }

        if (m == PositionMode.AggressivePosition)
        {
            if (_aggressivePositionModeService == null)
            {
                _aggressivePositionModeService = new AggressivePositionModeService();
            }

            Vector2 threatDirection = _jitterDir;

            if (_perception != null && _perception.ClosestEnemy != null)
            {
                threatDirection =
                    ((Vector2)_perception.ClosestEnemy.position - selfPos).normalized;
            }

            return _aggressivePositionModeService.ResolvePosition(
                new AggressivePositionModeService.Context
                {
                    SelfPosition = selfPos,
                    PartyAnchorPosition = partyAnchorPosition,
                    ThreatDirection = threatDirection,
                    AggressiveDistance = AggressiveDistance
                });
        }

        if (m == PositionMode.SafePosition)
        {
            if (_safePositionModeService == null)
            {
                _safePositionModeService = new SafePositionModeService();
            }

            Vector2 threatDirection = _jitterDir;

            if (_perception != null && _perception.ClosestEnemy != null)
            {
                threatDirection =
                    ((Vector2)_perception.ClosestEnemy.position - selfPos).normalized;
            }

            return _safePositionModeService.ResolvePosition(
                new SafePositionModeService.Context
                {
                    SelfPosition = selfPos,
                    ThreatDirection = threatDirection,
                    SafeDistance = safeDistance
                });
        }

        if (m == PositionMode.ProtectPosition)
        {
            if (_protectPositionModeService == null)
            {
                _protectPositionModeService = new ProtectPositionModeService();
            }

            Vector2 threatDirection = _jitterDir;

            if (_perception != null && _perception.ClosestEnemy != null)
            {
                threatDirection =
                    ((Vector2)_perception.ClosestEnemy.position - selfPos).normalized;
            }

            return _protectPositionModeService.ResolvePosition(
                new ProtectPositionModeService.Context
                {
                    SelfPosition = selfPos,
                    ThreatDirection = threatDirection,
                    ProtectDistance = protectDistance
                });
        }

        if (m == PositionMode.PatrolAroundTower)
        {
            if (_patrolAroundTowerModeService == null)
            {
                _patrolAroundTowerModeService = new PatrolAroundTowerModeService();
            }

            Transform tower = _positionService.FindNearestTower(
                selfPos,
                debugDraw,
                name);

            return _patrolAroundTowerModeService.ResolvePosition(
                new PatrolAroundTowerModeService.Context
                {
                    SelfPosition = selfPos,
                    Tower = tower,
                    JitterDirection = _jitterDir,
                    PatrolDistance = patrolDistance
                });
        }

        if (m == PositionMode.FormationHold)
        {
            if (_formationHoldModeService == null)
            {
                _formationHoldModeService = new FormationHoldModeService();
            }

            int formationIndex = GetFormationIndex();

            return _formationHoldModeService.ResolvePosition(
                new FormationHoldModeService.Context
                {
                    SelfPosition = selfPos,
                    PartyAnchorPosition = partyAnchorPosition,
                    PartyForwardDirection = anchorData.ForwardDirection,
                    JitterDirection = _jitterDir,
                    ObstacleMask = obstacleMask,
                    FormationRadius = formationRadius,
                    SlotSpacing = formationSlotSpacing,
                    FormationIndex = formationIndex,
                    MemberCount = Mathf.Max(1, anchorData.MemberCount)
                });
        }

        return _positionService.ComputeTargetPosition(
            new PartyMovementPositionService.Context
            {
                Mode = m,
                SelfPos = selfPos,
                PartyAnchorPosition = partyAnchorPosition,
                MaxDistanceFromParty = maxDistanceFromPartyAnchor,
                OwnerTransform = transform,
                OwnerName = name,
                Perception = _perception,
                JitterDir = _jitterDir,
                Jitter01 = _jitter01,
                EngageDistance = engageDistance,
                PatrolDistance = patrolDistance,
                PatrolAreaRadius = patrolAreaRadius,
                PositionRadius = positionRadius,
                SafeDistance = safeDistance,
                SafeAreaRadius = safeAreaRadius,
                ProtectDistance = protectDistance,
                ProtectAreaRadius = protectAreaRadius,
                AggressiveDistance = AggressiveDistance,
                AggressiveAreaRadius = AggressiveAreaRadius,
                PositionJitterStrength = PositionJitterStrength,
                SeparationRadius = SeparationRadius,
                SeparationStrength = SeparationStrength,
                PartyMask = partyMask,
                ObstacleMask = obstacleMask,
                DebugDraw = debugDraw
            });
    }
    private int GetFormationIndex()
    {
        PartyManager partyManager = PartyManager.Instance;

        if (partyManager == null)
        {
            return 0;
        }

        var members = partyManager.GetMovementMembers();
        if (members == null)
        {
            return 0;
        }

        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == this)
            {
                return i;
            }
        }

        return 0;
    }


    private void HandleManualMovement()
    {
        if (IsMovementBlocked())
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
            return;
        }

        if (_manualMoveInput.sqrMagnitude <= 0.0001f)
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
            return;
        }

        MoveInDirection(_manualMoveInput);
        UpdateMovementAnimation(_manualMoveInput.normalized);
    }

    private void MoveTo(Vector2 target)
    {
        if (_executionService == null)
        {
            _executionService = new PartyMovementExecutionService(this);
        }

        Vector2 currentPosition = _rb != null
            ? _rb.position
            : (Vector2)transform.position;

        Vector2 delta = target - currentPosition;
        float resolvedArriveDistance = Mathf.Max(0.01f, arriveDistance);

        if (delta.sqrMagnitude <= resolvedArriveDistance * resolvedArriveDistance)
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
            return;
        }

        _executionService.MoveTowardPoint(
            _movementController,
            target,
            GetMoveSpeed(),
            arriveDistance);

        UpdateMovementAnimation(delta.normalized);
    }

    private void UpdateMovementAnimation(Vector2 moveDirection)
    {
        if (_animationMono == null)
            return;

        if (_animationMono.IsPlayingAttack())
            return;

        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            _animationMono.PlayIdle();
            return;
        }

        _animationMono.SetDirectionFromVector(moveDirection.normalized);
        _animationMono.PlayMove();
    }


    private void MoveInDirection(Vector2 direction, float speedMultiplier = 1f)
    {
        if (_executionService == null)
        {
            _executionService = new PartyMovementExecutionService(this);
        }

        _executionService.MoveInDirection(
            _movementController,
            direction,
            GetMoveSpeed(),
            arriveDistance,
            speedMultiplier);
    }

    private void StopMovement()
    {
        if (_executionService == null)
        {
            _executionService = new PartyMovementExecutionService(this);
        }

        _executionService.StopMovement(_movementController);
    }

    private float GetMoveSpeed()
    {
        if (_characterManager == null)
        {
            _characterManager = GetComponent<CharacterManager>();

            if (_characterManager == null)
                _characterManager = GetComponentInParent<CharacterManager>();
        }

        if (_characterManager == null)
        {
            return moveSpeed;
        }

        float statMoveSpeed =
            _characterManager.GetStatValue(StatType.MoveSpeed);

        if (statMoveSpeed <= 0f)
        {
            return moveSpeed;
        }

        return statMoveSpeed;
    }

    private void SyncMovementControllerConfig(float speedMultiplier = 1f)
    {
        if (_movementController == null)
            return;

        _movementController.SetMoveSpeed(GetMoveSpeed() * Mathf.Max(0f, speedMultiplier));
        _movementController.SetArriveDistance(arriveDistance);
    }

    private void LogMovementDebugIfNeeded(string reason)
    {
        if (!debugDraw)
        {
            return;
        }

        if (_debugLogTimer < 0.5f)
        {
            return;
        }

        _debugLogTimer = 0f;

        PartyAnchorService.PartyAnchorData anchorData = default;
        PartyManager partyManager = PartyManager.Instance;

        if (partyManager != null)
        {
            anchorData = partyManager.GetPartyAnchorData();
        }

        string closestEnemyName = _perception != null && _perception.ClosestEnemy != null
            ? _perception.ClosestEnemy.name
            : "null";

        float targetDistance = Vector2.Distance(
            transform.position,
            _targetPos);

        Debug.Log(
            $"[PartyMovementDebug] {name} Reason={reason} " +
            $"Mode={mode} Phase={_phase} " +
            $"Target={_targetPos} TargetDist={targetDistance:F2} " +
            $"ClosestEnemy={closestEnemyName} " +
            $"EnemyCount={(_perception != null ? _perception.EnemyCount : 0)} " +
            $"Anchor={anchorData.AnchorPosition} " +
            $"PartyCenter={anchorData.PartyCenterPosition} " +
            $"ZoneCenter={anchorData.ZoneCenterPosition} " +
            $"MemberCount={anchorData.MemberCount} " +
            $"HasZone={anchorData.HasZone}",
            this);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, positionRadius);

        Vector2 gizmoTarget = _targetPos;

        if (gizmoTarget == Vector2.zero)
        {
            gizmoTarget = Application.isPlaying
                ? ComputeTargetPosition(mode)
                : (Vector2)transform.position;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(gizmoTarget, 0.08f);
        Gizmos.DrawLine(transform.position, gizmoTarget);

        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(gizmoTarget, arriveDistance);

        Gizmos.color = new Color(1f, 1f, 0f, 0.7f);
        Gizmos.DrawWireSphere(gizmoTarget, 0.35f);

        Vector2 selfPos = transform.position;
        Vector2 threatDir = _jitterDir.sqrMagnitude > 0.001f ? _jitterDir.normalized : Vector2.right;
        if (_perception != null && _perception.ClosestEnemy != null)
        {
            Vector2 enemyPos = _perception.ClosestEnemy.position;
            Vector2 toEnemy = enemyPos - selfPos;
            if (toEnemy.sqrMagnitude > 0.0001f)
                threatDir = toEnemy.normalized;
        }

        Vector2 anchor = selfPos;
        float areaRadius = 0f;

        switch (mode)
        {
            case PositionMode.ReturnToDestination:
                anchor = DefaultDestinationPosition;
                areaRadius = arriveDistance;
                Gizmos.color = new Color(0.2f, 1f, 0.2f, 1f);
                break;
            case PositionMode.FormationHold:
                PartyManager partyManager = PartyManager.Instance;
                if (partyManager != null)
                {
                    PartyAnchorService.PartyAnchorData anchorData = partyManager.GetPartyAnchorData();
                    anchor = anchorData.MemberCount > 0
                        ? anchorData.AnchorPosition
                        : selfPos;
                }
                else
                {
                    anchor = selfPos;
                }
                areaRadius = formationRadius;
                Gizmos.color = new Color(1f, 0.5f, 1f, 1f);
                break;
            case PositionMode.PatrolAroundTower:
                if (_positionService == null)
                {
                    _positionService = new PartyMovementPositionService(this);
                }

                Transform tower = _positionService.FindNearestTower(
                    _rb != null ? _rb.position : (Vector2)transform.position,
                    debugDraw,
                    name);
                if (tower != null)
                {
                    Vector2 towerPos = tower.position;
                    Vector2 baseDir = (selfPos - towerPos);
                    if (baseDir.sqrMagnitude <= 0.0001f)
                        baseDir = _jitterDir.sqrMagnitude > 0.001f ? _jitterDir : Vector2.right;
                    baseDir.Normalize();
                    anchor = towerPos + baseDir * patrolDistance;
                }
                else
                {
                    anchor = selfPos;
                }
                areaRadius = patrolAreaRadius;
                Gizmos.color = new Color(0.5f, 0.8f, 1f, 1f);
                break;
            case PositionMode.SafePosition:
                anchor = selfPos - threatDir * safeDistance;
                areaRadius = safeAreaRadius;
                Gizmos.color = new Color(0.3f, 0.9f, 1f, 1f);
                break;
            case PositionMode.ProtectPosition:
                anchor = selfPos + threatDir * protectDistance;
                areaRadius = protectAreaRadius;
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 1f);
                break;
            case PositionMode.AggressivePosition:
                anchor = gizmoTarget != Vector2.zero
                    ? gizmoTarget
                    : selfPos + threatDir * AggressiveDistance;
                areaRadius = AggressiveAreaRadius;
                Gizmos.color = new Color(1f, 0.35f, 0.35f, 1f);
                break;
        }

        Gizmos.DrawWireSphere(anchor, areaRadius);

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawLine(anchor, gizmoTarget);

        Gizmos.color = new Color(0.7f, 1f, 0.7f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, SeparationRadius);

        if (maxDistanceFromPartyAnchor > 0f)
        {
            PartyManager partyManager = PartyManager.Instance;

            if (partyManager != null)
            {
                PartyAnchorService.PartyAnchorData anchorData = partyManager.GetPartyAnchorData();

                if (anchorData.MemberCount > 0)
                {
                    Gizmos.color = new Color(1f, 0.5f, 1f, 0.65f);
                    Gizmos.DrawWireSphere(anchorData.AnchorPosition, maxDistanceFromPartyAnchor);
                    Gizmos.DrawSphere(anchorData.AnchorPosition, 0.08f);
                }
            }
        }

        if (gizmoTarget != Vector2.zero)
        {
            Vector2 moveDir = (gizmoTarget - (Vector2)transform.position);

            if (moveDir.sqrMagnitude > 0.0001f)
            {
                moveDir.Normalize();

                Gizmos.color = Color.red;
                Gizmos.DrawRay(
                    transform.position,
                    moveDir * 0.75f);
            }
        }
    }
}