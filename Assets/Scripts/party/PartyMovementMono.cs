using UnityEngine;

/// <summary>
/// PartyMovementMono
///
/// New movement concept: "take a position" around the player.
///
/// Modes:
/// - SafePosition      : move to a safer spot near the player (typically behind / away from threat)
/// - ProtectPosition   : move to a protective spot near the player (between player and threat)
/// - AggressivePosition: move to an offensive spot near the player (toward threat but leashed)
///
/// All three modes always choose a target position within a radius around the player,
/// so "follow" is implicitly included.
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

    [Header("Leader")]
    [SerializeField] private Transform leader;

    [Header("Mode")]
    [SerializeField] private PositionMode mode = PositionMode.ProtectPosition;

    [Tooltip("If true, chooses mode automatically from simple heuristics (can be replaced by your AI engine later).")]
    [SerializeField] private bool autoMode = false;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Tooltip("How close is considered 'arrived' to the target position.")]
    [SerializeField] private float arriveDistance = 0.08f;

    [Header("Position Radius (around player)")]
    [Tooltip("All target positions are clamped within this radius around the player.")]
    [SerializeField] private float positionRadius = 3.0f;

    [Header("Safe Position")]
    [Tooltip("Preferred distance from player when seeking safe position.")]
    [SerializeField] private float safeDistance = 2.2f;
    [Tooltip("Random radius around the safe anchor point.")]
    [SerializeField] private float safeAreaRadius = 0.9f;

    [Header("Protect Position")]
    [Tooltip("How far from player to stand when protecting (between player and threat).")]
    [SerializeField] private float protectDistance = 1.8f;
    [Tooltip("Random radius around the protect anchor point.")]
    [SerializeField] private float protectAreaRadius = 0.8f;

    [Header("Aggressive Position")]
    [Tooltip("How far from player to stand when being aggressive (toward threat).")]
    [SerializeField] private float aggressiveDistance = 2.6f;
    [Tooltip("Random radius around the aggressive anchor point.")]
    [SerializeField] private float aggressiveAreaRadius = 1.0f;

    [Header("Leash")]
    [Tooltip("Hard limit: if we exceed this distance from the player, force a return to a safe spot.")]
    [SerializeField] private float maxLeashDistance = 7f;

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

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _perception = GetComponent<PerceptionMono>();

        // Stable per-agent randomization so party members don't collapse to one point.
        float ang = Random.Range(0f, Mathf.PI * 2f);
        _jitterDir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        _jitter01 = Random.Range(0.35f, 1f);
    }

    public void SetLeader(Transform t)
    {
        leader = t;
    }

    public void SetMode(PositionMode newMode)
    {
        mode = newMode;
    }

    public PositionMode GetMode() => mode;

    private void Update()
    {
        if (leader == null)
            return;

        _phaseTimer += Time.deltaTime;

        switch (_phase)
        {
            case MovePhase.ComputePosition:

                if (autoMode)
                    AutoSelectMode();

                // Decide target position relative to player
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
                    float playerDist = Vector2.Distance(transform.position, leader.position);

                    // If we drift too far from player force safe position next
                    if (playerDist > maxLeashDistance)
                        mode = PositionMode.SafePosition;

                    _phase = MovePhase.ComputePosition;
                    _phaseTimer = 0f;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (leader == null)
            return;

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
        Vector2 playerPos = leader.position;

        // Determine a threat direction based on closest enemy.
        // If no enemy, we use a stable fallback direction.
        Vector2 threatDir = Vector2.right;
        if (_perception != null && _perception.ClosestEnemy != null)
        {
            Vector2 enemyPos = _perception.ClosestEnemy.position;
            Vector2 toEnemy = (enemyPos - playerPos);
            if (toEnemy.sqrMagnitude > 0.0001f)
                threatDir = toEnemy.normalized;
        }

        // Perpendicular direction around the player/enemy line.
        Vector2 sideDir = new Vector2(-threatDir.y, threatDir.x);

        Vector2 anchor;
        float areaRadius;

        switch (m)
        {
            case PositionMode.SafePosition:
                // Safe = behind player relative to threat.
                anchor = playerPos - threatDir * safeDistance;
                areaRadius = safeAreaRadius;
                break;

            case PositionMode.ProtectPosition:
                // Protect = between player and threat.
                anchor = playerPos + threatDir * protectDistance;
                areaRadius = protectAreaRadius;
                break;

            case PositionMode.AggressivePosition:
                // Aggressive = further toward threat.
                anchor = playerPos + threatDir * aggressiveDistance;
                areaRadius = aggressiveAreaRadius;
                break;

            default:
                anchor = playerPos;
                areaRadius = 0f;
                break;
        }

        // Stable random offset so members spread inside the area instead of stacking on one point.
        float jitterScale = Mathf.Max(0f, positionJitterStrength) * Mathf.Max(0f, areaRadius) * _jitter01;

        // Mix side spread + slight forward/back offset so positions feel less grid-like.
        float sideSign = (_jitterDir.x >= 0f) ? 1f : -1f;
        float forwardBias = _jitterDir.y * 0.35f;

        Vector2 jitterOffset = (sideDir * sideSign + threatDir * forwardBias).normalized * jitterScale;
        Vector2 target = anchor + jitterOffset;

        // Separation: push the chosen target away from nearby party members
        Vector2 sep = ComputeSeparationOffset(target);
        target += sep;

        // Clamp within player's positionRadius so follow is implicit.
        Vector2 offset = target - playerPos;
        float mag = offset.magnitude;
        if (mag > positionRadius)
            target = playerPos + offset.normalized * positionRadius;

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
        if (leader == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(leader.position, positionRadius);

        // Show target point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_targetPos, 0.08f);
        Gizmos.DrawLine(transform.position, _targetPos);

        // Show current mode area around anchor
        Vector2 playerPos = leader.position;
        Vector2 threatDir = Vector2.right;
        if (_perception != null && _perception.ClosestEnemy != null)
        {
            Vector2 enemyPos = _perception.ClosestEnemy.position;
            Vector2 toEnemy = enemyPos - playerPos;
            if (toEnemy.sqrMagnitude > 0.0001f)
                threatDir = toEnemy.normalized;
        }

        Vector2 anchor = playerPos;
        float areaRadius = 0f;

        switch (mode)
        {
            case PositionMode.SafePosition:
                anchor = playerPos - threatDir * safeDistance;
                areaRadius = safeAreaRadius;
                Gizmos.color = new Color(0.3f, 0.9f, 1f, 1f);
                break;
            case PositionMode.ProtectPosition:
                anchor = playerPos + threatDir * protectDistance;
                areaRadius = protectAreaRadius;
                Gizmos.color = new Color(1f, 0.85f, 0.2f, 1f);
                break;
            case PositionMode.AggressivePosition:
                anchor = playerPos + threatDir * aggressiveDistance;
                areaRadius = aggressiveAreaRadius;
                Gizmos.color = new Color(1f, 0.35f, 0.35f, 1f);
                break;
        }

        Gizmos.DrawWireSphere(anchor, areaRadius);

        Gizmos.color = new Color(0.7f, 1f, 0.7f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
