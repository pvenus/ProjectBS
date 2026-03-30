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

    [Header("Movement")]
    [SerializeField] private PathingArchetype archetype = PathingArchetype.Normal;
    [SerializeField] private float repathInterval = 0.35f;
    [SerializeField] private float waypointTolerance = 0.12f;
    [SerializeField] private float turnResponsiveness = 14f;
    [SerializeField] private float chaseRange = 1000f;
    [SerializeField] private float holdExitDistanceBuffer = 0.4f;
    [SerializeField] private float rangeControlEnterBuffer = 1.5f;


    [Header("Flying Straight Move")]
    [SerializeField] private float flyingDespawnBacktrackDistance = 3f;
    [SerializeField] private float flyingForwardLifetimeDistance = 40f;

    [Header("Wander")]
    [SerializeField] private bool enableWanderWhenNoTarget = true;
    [SerializeField] private float wanderRadius = 2.5f;
    [SerializeField] private float wanderInterval = 2.0f;
    [SerializeField] private float pauseBetweenWanders = 0.35f;

    [Header("Grid A*")]
    [SerializeField] private Vector2 gridCenter = Vector2.zero;
    [SerializeField] private Vector2 gridWorldSize = new Vector2(40f, 40f);
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private LayerMask obstacleMask;

    private Rigidbody2D _rb;
    private readonly List<Vector2> _path = new();
    private int _pathIndex;
    private float _repathTimer;
    private Vector2 _chaseDir = Vector2.right;
    private Vector2Int _lastGoalCell = new Vector2Int(int.MinValue, int.MinValue);

    private Vector2 _wanderDir = Vector2.right;
    private float _wanderTimer;
    private float _pauseTimer;
    private bool _isPausing;

    private Vector2 _spawnPosition;
    private Vector2 _flyingMoveDirection = Vector2.right;
    private bool _hasFlyingMoveDirection;
    private bool _isHoldingAtRange;

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

        if (canChase)
        {
            if (shouldHoldPosition)
            {
                _path.Clear();
                _pathIndex = 0;
            }
            else
            {
                // Direct movement mode does not use cached path data.
                _path.Clear();
                _pathIndex = 0;
            }
        }
        else if (enableWanderWhenNoTarget)
        {
            UpdateWanderState();
        }
    }

    private void FixedUpdate()
    {
        Transform target = targeting != null ? targeting.GetCurrentTarget() : null;
        bool canChase = target != null && Vector2.Distance(transform.position, target.position) <= chaseRange;
        bool shouldHoldPosition = canChase && ShouldHoldPositionNearTarget(target, false) && !IsFlyingArchetype();

        if (canChase)
        {
            if (shouldHoldPosition)
            {
                _chaseDir = Vector2.zero;
                return;
            }

            if (IsFlyingArchetype())
            {
                MoveFlyingStraight();
                CheckFlyingAutoDespawn();
                return;
            }

            if (UsesDirectRangeChase())
            {
                MoveWithRangeControl(target);
                return;
            }

            MoveStraightTowardTarget(target);
            return;
        }

        if (!enableWanderWhenNoTarget || _isPausing)
            return;

        Vector2 wanderDelta = _wanderDir * (GetMoveSpeed() * Time.fixedDeltaTime);
        _rb.MovePosition(_rb.position + wanderDelta);
    }
    private bool UsesDirectRangeChase()
    {
        if (movementProfile != null)
            return movementProfile.IsRanged() || movementProfile.IsSiege();

        return archetype == PathingArchetype.Ranged || archetype == PathingArchetype.Siege;
    }

    private bool UsesPathChase()
    {
        return false;
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
        if (_rb == null)
            return;

        if (!_hasFlyingMoveDirection || _flyingMoveDirection.sqrMagnitude <= 0.0001f)
            InitializeFlyingDirection();

        Vector2 dir = _flyingMoveDirection.sqrMagnitude <= 0.0001f ? Vector2.right : _flyingMoveDirection.normalized;
        _rb.MovePosition(_rb.position + dir * (GetMoveSpeed() * Time.fixedDeltaTime));
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

    public void RepathToCurrentTarget()
    {
        _path.Clear();
        _pathIndex = 0;
    }


    private float GetStopDistanceForCurrentArchetype()
    {
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

    private void MoveStraightTowardTarget(Transform target)
    {
        if (_rb == null || target == null)
            return;

        Vector2 pos = _rb.position;
        Vector2 toTarget = (Vector2)target.position - pos;
        float dist = toTarget.magnitude;

        if (dist <= 0.03f)
        {
            _chaseDir = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget / Mathf.Max(dist, 0.0001f);
        _chaseDir = dir;
        _rb.MovePosition(pos + dir * (GetMoveSpeed() * Time.fixedDeltaTime));
    }

    // (removed GetDesiredChaseGoal)


    private void MoveWithRangeControl(Transform target)
    {
        if (_rb == null || target == null)
            return;

        float stopDistance = GetStopDistanceForCurrentArchetype();
        if (stopDistance <= 0.01f)
            return;

        Vector2 pos = _rb.position;
        Vector2 targetPos = target.position;
        Vector2 toTarget = targetPos - pos;
        float dist = toTarget.magnitude;

        if (dist <= 0.0001f)
        {
            _chaseDir = Vector2.zero;
            return;
        }

        float enterDistance = stopDistance + Mathf.Max(0.05f, rangeControlEnterBuffer);

        // Far away: just go straight toward the target.
        if (dist > enterDistance)
        {
            Vector2 dirFar = toTarget / dist;
            _chaseDir = dirFar;
            _rb.MovePosition(pos + dirFar * (GetMoveSpeed() * Time.fixedDeltaTime));
            return;
        }

        // Near preferred range: only move radially to fix distance.
        float error = dist - stopDistance;
        if (Mathf.Abs(error) <= 0.05f)
        {
            _chaseDir = Vector2.zero;
            return;
        }

        Vector2 dir = toTarget / dist;
        Vector2 moveDir = error > 0f ? dir : -dir;
        _chaseDir = moveDir;

        float speedScale = Mathf.Clamp01(Mathf.Abs(error) / Mathf.Max(0.1f, rangeControlEnterBuffer));
        float speed = GetMoveSpeed() * Mathf.Max(0.35f, speedScale);
        _rb.MovePosition(pos + moveDir * (speed * Time.fixedDeltaTime));
    }

    private bool TryFindPathAStar(Vector2 startWorld, Vector2 goalWorld, out List<Vector2> worldPath)
    {
        worldPath = new List<Vector2>();

        if (!WorldToGrid(startWorld, out Vector2Int start))
            return false;
        if (!WorldToGrid(goalWorld, out Vector2Int goal))
            return false;

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();
        List<Vector2Int> open = new();
        HashSet<Vector2Int> closed = new();

        open.Add(start);
        gScore[start] = 0f;

        while (open.Count > 0)
        {
            int bestIndex = 0;
            Vector2Int current = open[0];
            float currentF = GetScore(gScore, current) + Heuristic(current, goal);
            for (int i = 1; i < open.Count; i++)
            {
                Vector2Int c = open[i];
                float f = GetScore(gScore, c) + Heuristic(c, goal);
                if (f < currentF)
                {
                    currentF = f;
                    current = c;
                    bestIndex = i;
                }
            }

            if (current == goal)
            {
                ReconstructWorldPath(cameFrom, current, worldPath);
                return worldPath.Count > 0;
            }

            open.RemoveAt(bestIndex);
            closed.Add(current);

            for (int ny = -1; ny <= 1; ny++)
            {
                for (int nx = -1; nx <= 1; nx++)
                {
                    if (nx == 0 && ny == 0)
                        continue;

                    Vector2Int next = new Vector2Int(current.x + nx, current.y + ny);
                    if (!IsInsideGrid(next))
                        continue;
                    if (closed.Contains(next))
                        continue;
                    if (!IsWalkable(next) && next != goal)
                        continue;

                    float stepCost = (nx == 0 || ny == 0) ? 1f : 1.4142135f;
                    float tentative = GetScore(gScore, current) + stepCost;

                    if (!gScore.ContainsKey(next) || tentative < gScore[next])
                    {
                        cameFrom[next] = current;
                        gScore[next] = tentative;
                        if (!open.Contains(next))
                            open.Add(next);
                    }
                }
            }
        }

        return false;
    }

    private void ReconstructWorldPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current, List<Vector2> outPath)
    {
        List<Vector2Int> reverse = new();
        reverse.Add(current);

        while (cameFrom.TryGetValue(current, out Vector2Int prev))
        {
            current = prev;
            reverse.Add(current);
        }

        reverse.Reverse();

        for (int i = 0; i < reverse.Count; i++)
        {
            Vector2Int g = reverse[i];
            outPath.Add(GridToWorld(g.x, g.y));
        }
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        int min = Mathf.Min(dx, dy);
        int max = Mathf.Max(dx, dy);
        return 1.4142135f * min + (max - min);
    }

    private float GetScore(Dictionary<Vector2Int, float> scores, Vector2Int key)
    {
        return scores.TryGetValue(key, out float v) ? v : float.PositiveInfinity;
    }

    private bool IsInsideGrid(Vector2Int g)
    {
        Vector2 half = gridWorldSize * 0.5f;
        Vector2 min = gridCenter - half;
        Vector2 max = gridCenter + half;
        Vector2 world = GridToWorld(g.x, g.y);
        return world.x >= min.x && world.x <= max.x && world.y >= min.y && world.y <= max.y;
    }

    private bool WorldToGrid(Vector2 world, out Vector2Int g)
    {
        Vector2 half = gridWorldSize * 0.5f;
        Vector2 min = gridCenter - half;
        Vector2 local = world - min;

        int gx = Mathf.FloorToInt(local.x / Mathf.Max(0.01f, cellSize));
        int gy = Mathf.FloorToInt(local.y / Mathf.Max(0.01f, cellSize));
        g = new Vector2Int(gx, gy);
        return IsInsideGrid(g);
    }

    private Vector2 GridToWorld(int gx, int gy)
    {
        Vector2 half = gridWorldSize * 0.5f;
        Vector2 min = gridCenter - half;
        return new Vector2(
            min.x + (gx + 0.5f) * cellSize,
            min.y + (gy + 0.5f) * cellSize
        );
    }

    private bool IsWalkable(Vector2Int g)
    {
        Vector2 center = GridToWorld(g.x, g.y);
        Vector2 size = new Vector2(cellSize * 0.9f, cellSize * 0.9f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, obstacleMask);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null)
                continue;
            if (c.isTrigger)
                continue;
            if (c.attachedRigidbody != null && c.attachedRigidbody.gameObject == gameObject)
                continue;

            Transform target = targeting != null ? targeting.GetCurrentTarget() : null;
            if (target != null && (c.transform == target || c.transform.IsChildOf(target)))
                continue;

            return false;
        }

        return true;
    }

    public void ForceRepath()
    {
        _repathTimer = 0f;
        _lastGoalCell = new Vector2Int(int.MinValue, int.MinValue);
        _isHoldingAtRange = false;
    }

    public void SetArchetype(PathingArchetype newArchetype)
    {
        archetype = newArchetype;
        _isHoldingAtRange = false;
        _spawnPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        _hasFlyingMoveDirection = false;
        InitializeFlyingDirection();
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

    public List<Vector2> GetCurrentPath()
    {
        return _path;
    }

    public int GetCurrentPathIndex()
    {
        return _pathIndex;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.35f);
        Gizmos.DrawWireCube(gridCenter, gridWorldSize);

        if (_path != null && _path.Count > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0.1f, 0.8f);
            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.DrawLine(_path[i], _path[i + 1]);
            }
        }

        if (Application.isPlaying && IsFlyingArchetype())
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.9f);
            Vector3 start = _spawnPosition;
            Vector3 dir = _flyingMoveDirection.sqrMagnitude <= 0.0001f ? Vector2.right : _flyingMoveDirection.normalized;
            Gizmos.DrawLine(start, start + dir * flyingForwardLifetimeDistance);
        }
    }
#endif
}
