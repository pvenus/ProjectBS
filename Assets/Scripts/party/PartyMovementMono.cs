using UnityEngine;
using venus.eldawn.party;

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
    [SerializeField] private LayerMask towerMask;
    [SerializeField] private float engageDistance = 5f;
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
    [Tooltip("Preferred forward distance when taking an aggressive position.")]
    [SerializeField] private float aggressiveDistance = 2.6f;
    [Tooltip("Random radius around the aggressive anchor point.")]
    [SerializeField] private float aggressiveAreaRadius = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugDraw = false;

    [Header("Cycle")]
    [SerializeField] private float decisionInterval = 3f;
    [Tooltip("How strongly each unit spreads differently inside an area. 0 = no randomness.")]
    [SerializeField] private float positionJitterStrength = 1f;

    [Header("Separation")]
    [Tooltip("How far to search for nearby party members when pushing positions apart.")]
    [SerializeField] private float separationRadius = 1.2f;

    [Tooltip("How strongly this unit avoids nearby party members when choosing a target position.")]
    [SerializeField] private float separationStrength = 1.1f;

    [Tooltip("Layer mask used to find nearby party members for separation.")]
    [SerializeField] private LayerMask partyMask = ~0;

    private MovePhase _phase = MovePhase.ComputePosition;
    private float _phaseTimer = 0f;

    private Rigidbody2D _rb;
    private MovementController _movementController;
    private PerceptionMono _perception;
    private AnimationMono _animationMono;

    private Transform _cachedTower;

    private Vector2 _targetPos;
    private Vector2 _jitterDir;
    private float _jitter01;

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

        // Stable per-agent randomization so party members don't collapse to one point.
        float ang = Random.Range(0f, Mathf.PI * 2f);
        _jitterDir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        _jitter01 = Random.Range(0.35f, 1f);
        _cachedTower = FindInitialTower();
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

        switch (_phase)
        {
            case MovePhase.ComputePosition:

                if (autoMode)
                    AutoSelectMode();

                // Decide target position from this member's current tactical context
                _targetPos = ComputeTargetPosition(mode);

                _phase = MovePhase.Move;
                break;

            case MovePhase.Move:

                float dist = Vector2.Distance(transform.position, _targetPos);

                if (dist <= arriveDistance)
                {
                    _phase = MovePhase.Stay;
                    _phaseTimer = 0f;
                }
                break;

            case MovePhase.Stay:

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

    private void AutoSelectMode()
    {
        Transform closestEnemy = _perception != null ? _perception.ClosestEnemy : null;
        if (closestEnemy == null)
        {
            mode = PositionMode.PatrolAroundTower;
            return;
        }

        float enemyDistance = Vector2.Distance(transform.position, closestEnemy.position);
        if (enemyDistance > engageDistance)
        {
            mode = PositionMode.PatrolAroundTower;
            return;
        }

        int n = _perception != null ? _perception.EnemyCount : 0;
        if (n >= 4)
            mode = PositionMode.ProtectPosition;
        else
            mode = PositionMode.AggressivePosition;
    }

    private Vector2 ComputeTargetPosition(PositionMode m)
    {
        Vector2 selfPos = _rb != null ? _rb.position : (Vector2)transform.position;

        Transform closestEnemy = _perception != null ? _perception.ClosestEnemy : null;
        bool hasEnemyInRange = false;
        Vector2 threatDir = _jitterDir.sqrMagnitude > 0.001f ? _jitterDir.normalized : Vector2.right;

        if (closestEnemy != null)
        {
            Vector2 toEnemy = (Vector2)closestEnemy.position - selfPos;
            if (toEnemy.sqrMagnitude > 0.0001f)
            {
                float enemyDistance = toEnemy.magnitude;
                hasEnemyInRange = enemyDistance <= engageDistance;
                threatDir = toEnemy.normalized;
            }
        }

        if (m == PositionMode.PatrolAroundTower || !hasEnemyInRange)
        {
            return ComputeTowerPatrolPosition(selfPos);
        }

        Vector2 sideDir = new Vector2(-threatDir.y, threatDir.x);

        Vector2 anchor;
        float areaRadius;

        switch (m)
        {
            case PositionMode.SafePosition:
                anchor = selfPos - threatDir * safeDistance;
                areaRadius = safeAreaRadius;
                break;

            case PositionMode.ProtectPosition:
                anchor = selfPos + threatDir * protectDistance;
                areaRadius = protectAreaRadius;
                break;

            case PositionMode.AggressivePosition:
                anchor = selfPos + threatDir * aggressiveDistance;
                areaRadius = aggressiveAreaRadius;
                break;

            default:
                return ComputeTowerPatrolPosition(selfPos);
        }

        float jitterScale = Mathf.Max(0f, positionJitterStrength) * Mathf.Max(0f, areaRadius) * _jitter01;
        float sideSign = (_jitterDir.x >= 0f) ? 1f : -1f;
        float forwardBias = _jitterDir.y * 0.35f;

        Vector2 jitterOffset = (sideDir * sideSign + threatDir * forwardBias).normalized * jitterScale;
        Vector2 target = anchor + jitterOffset;

        Vector2 sep = ComputeSeparationOffset(target);
        target += sep;

        Vector2 offset = target - selfPos;
        float mag = offset.magnitude;
        if (mag > positionRadius)
            target = selfPos + offset.normalized * positionRadius;

        return target;
    }

    private Vector2 ComputeTowerPatrolPosition(Vector2 selfPos)
    {
        Transform tower = FindNearestTower();
        if (tower == null)
            return selfPos;

        Vector2 towerPos = tower.position;
        Vector2 baseDir = (selfPos - towerPos);
        if (baseDir.sqrMagnitude <= 0.0001f)
            baseDir = _jitterDir.sqrMagnitude > 0.001f ? _jitterDir : Vector2.right;

        baseDir.Normalize();
        Vector2 sideDir = new Vector2(-baseDir.y, baseDir.x);

        Vector2 anchor = towerPos + baseDir * patrolDistance;

        float jitterScale = Mathf.Max(0f, positionJitterStrength) * Mathf.Max(0f, patrolAreaRadius) * _jitter01;
        float sideSign = (_jitterDir.x >= 0f) ? 1f : -1f;
        float forwardBias = _jitterDir.y * 0.35f;

        Vector2 jitterOffset = (sideDir * sideSign + baseDir * forwardBias).normalized * jitterScale;
        Vector2 target = anchor + jitterOffset;

        Vector2 sep = ComputeSeparationOffset(target);
        target += sep;

        Vector2 offset = target - selfPos;
        float mag = offset.magnitude;
        if (mag > positionRadius)
            target = selfPos + offset.normalized * positionRadius;

        return target;
    }

    private Transform FindNearestTower()
    {
        if (_cachedTower != null)
            return _cachedTower;

        _cachedTower = FindInitialTower();
        return _cachedTower;
    }

    private Transform FindInitialTower()
    {
        Transform best = null;
        float bestDistance = float.MaxValue;
        Vector2 selfPos = _rb != null ? _rb.position : (Vector2)transform.position;

        TowerPropMono[] towers = FindObjectsByType<TowerPropMono>(FindObjectsSortMode.None);
        for (int i = 0; i < towers.Length; i++)
        {
            TowerPropMono tower = towers[i];
            if (tower == null)
                continue;

            float d = Vector2.Distance(selfPos, tower.transform.position);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = tower.transform;
            }
        }

        if (best != null)
            return best;

        int towerLayer = LayerMask.NameToLayer("Tower");
        if (towerLayer >= 0)
        {
            Transform[] allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            for (int i = 0; i < allTransforms.Length; i++)
            {
                Transform t = allTransforms[i];
                if (t == null)
                    continue;

                if (t.gameObject.layer != towerLayer)
                    continue;

                float d = Vector2.Distance(selfPos, t.position);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = t;
                }
            }
        }

        if (best == null && debugDraw)
        {
            Debug.LogWarning($"[PartyMovementMono] Could not find tower for {name}. Check TowerPropMono or layer named 'Tower'.", this);
        }

        return best;
    }

    private Vector2 ComputeSeparationOffset(Vector2 desiredTarget)
    {
        float radius = Mathf.Max(0.01f, separationRadius);
        float strength = Mathf.Max(0f, separationStrength);
        if (strength <= 0f)
            return Vector2.zero;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, partyMask);
        if (hits == null || hits.Length == 0)
            return Vector2.zero;

        Vector2 push = Vector2.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            Transform t = c.transform;
            if (t == transform || t.IsChildOf(transform))
                continue;

            // Only separate from other party members that also have PartyMovementMono
            var other = t.GetComponentInParent<PartyMovementMono>();
            if (other == null || other == this)
                continue;

            Vector2 otherPos = other.transform.position;
            Vector2 away = desiredTarget - otherPos;
            float d = away.magnitude;
            if (d <= 0.0001f)
            {
                // Stable fallback push if perfectly overlapped
                away = _jitterDir.sqrMagnitude > 0.001f ? _jitterDir : Vector2.right;
                d = 0.0001f;
            }

            if (d > radius)
                continue;

            float w = 1f - Mathf.Clamp01(d / radius);
            push += away.normalized * w;
            count++;
        }

        if (count <= 0 || push.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        push = push.normalized * strength;
        return push;
    }

    private void HandleManualMovement()
    {
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
        if (_movementController == null)
            return;

        Vector2 pos = _rb.position;
        Vector2 delta = target - pos;

        if (delta.magnitude <= arriveDistance)
        {
            StopMovement();
            UpdateMovementAnimation(Vector2.zero);
            return;
        }

        Vector2 direction = delta.normalized;
        MoveTowardPoint(target);
        UpdateMovementAnimation(direction);
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

    private void MoveTowardPoint(Vector2 targetPoint)
    {
        if (_movementController == null)
            return;

        SyncMovementControllerConfig();
        _movementController.MoveTo(targetPoint);
    }

    private void MoveInDirection(Vector2 direction, float speedMultiplier = 1f)
    {
        if (_movementController == null)
            return;

        SyncMovementControllerConfig(speedMultiplier);
        _movementController.MoveByDirection(direction);
    }

    private void StopMovement()
    {
        if (_movementController != null)
            _movementController.Stop();
    }

    private void SyncMovementControllerConfig(float speedMultiplier = 1f)
    {
        if (_movementController == null)
            return;

        _movementController.SetMoveSpeed(moveSpeed * Mathf.Max(0f, speedMultiplier));
        _movementController.SetArriveDistance(arriveDistance);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, positionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_targetPos, 0.08f);
        Gizmos.DrawLine(transform.position, _targetPos);

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
            case PositionMode.PatrolAroundTower:
                Transform tower = FindNearestTower();
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
                anchor = selfPos + threatDir * aggressiveDistance;
                areaRadius = aggressiveAreaRadius;
                Gizmos.color = new Color(1f, 0.35f, 0.35f, 1f);
                break;
        }

        Gizmos.DrawWireSphere(anchor, areaRadius);

        Gizmos.color = new Color(0.7f, 1f, 0.7f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}