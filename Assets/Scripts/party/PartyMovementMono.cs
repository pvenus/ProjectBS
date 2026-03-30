using UnityEngine;

/// <summary>
/// PartyMovementMono
///
/// New movement concept: independent party member movement.
///
/// Modes:
/// - SafePosition      : reposition away from the threat
/// - ProtectPosition   : hold a closer tactical position against the threat
/// - AggressivePosition: push toward the threat
///
/// Each party member moves independently.
/// There is no leader-follow behavior; the player can switch control between members.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PartyMovementMono : MonoBehaviour
{
    public enum PositionMode
    {
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
    private PerceptionMono _perception;

    private Vector2 _targetPos;
    private Vector2 _jitterDir;
    private float _jitter01;

    private bool _isMovementControlledByPlayer = false;
    private Vector2 _manualMoveInput = Vector2.zero;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _perception = GetComponent<PerceptionMono>();

        // Stable per-agent randomization so party members don't collapse to one point.
        float ang = Random.Range(0f, Mathf.PI * 2f);
        _jitterDir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        _jitter01 = Random.Range(0.35f, 1f);
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
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
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
        if (_isMovementControlledByPlayer)
        {
            HandleManualMovement();
            return;
        }

        if (_phase == MovePhase.Move)
            MoveTo(_targetPos);
        else
            _rb.linearVelocity = Vector2.zero;
    }

    private void AutoSelectMode()
    {
        // VERY simple heuristic placeholder:
        // - If no enemy perceived => Safe
        // - If many enemies => Protect
        // - Otherwise => Aggressive
        // This is independent per member and can later be replaced by your AI engine.
        if (_perception == null || _perception.ClosestEnemy == null)
        {
            mode = PositionMode.SafePosition;
            return;
        }

        int n = _perception.EnemyCount;
        if (n >= 4)
            mode = PositionMode.ProtectPosition;
        else
            mode = PositionMode.AggressivePosition;
    }

    private Vector2 ComputeTargetPosition(PositionMode m)
    {
        Vector2 selfPos = _rb != null ? _rb.position : (Vector2)transform.position;

        // Determine a threat direction based on closest enemy.
        // If no enemy exists, keep a stable fallback direction.
        Vector2 threatDir = _jitterDir.sqrMagnitude > 0.001f ? _jitterDir.normalized : Vector2.right;
        Vector2 enemyPos = selfPos + threatDir;

        if (_perception != null && _perception.ClosestEnemy != null)
        {
            enemyPos = _perception.ClosestEnemy.position;
            Vector2 toEnemy = enemyPos - selfPos;
            if (toEnemy.sqrMagnitude > 0.0001f)
                threatDir = toEnemy.normalized;
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
                anchor = selfPos;
                areaRadius = 0f;
                break;
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
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        _rb.linearVelocity = _manualMoveInput * moveSpeed;
    }

    private void MoveTo(Vector2 target)
    {
        Vector2 pos = _rb.position;
        Vector2 delta = target - pos;

        if (delta.magnitude <= arriveDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = delta.normalized;
        _rb.linearVelocity = dir * moveSpeed;
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