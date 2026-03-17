using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// NPC for top-down 2D.
/// - Wanders randomly by default
/// - If player is within chaseRange, uses A* (grid-based) to chase the player
/// - Moves using Rigidbody2D.MovePosition so it collides with map walls/obstacles
/// - Has simple HP display (TextMesh)
///
/// Notes:
/// - For best results, tag the player GameObject as "Player".
/// - A* uses Physics2D overlap checks to mark blocked cells.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class NpcMono : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private float chaseRange = 1000f;

    // --- Taunt / forced target support ---
    private Transform _forcedTarget;
    private float _forcedUntil;

    private Transform GetCurrentTarget()
    {
        if (_forcedTarget != null && Time.time < _forcedUntil)
            return _forcedTarget;
        return player;
    }

    // Called by TauntSkill via SendMessage
    public void SetForcedTarget(Transform t)
    {
        _forcedTarget = t;
        // If duration arrives later via OnTaunted, keep a short default.
        _forcedUntil = Mathf.Max(_forcedUntil, Time.time + 0.25f);
    }

    // Called by TauntSkill via SendMessage (duration seconds)
    public void OnTaunted(float duration)
    {
        _forcedUntil = Mathf.Max(_forcedUntil, Time.time + Mathf.Max(0.05f, duration));
    }

    public void ClearForcedTarget(Transform t)
    {
        if (_forcedTarget == t)
            _forcedTarget = null;
    }
    /// <summary>
    /// Apply an instantaneous knockback velocity that overrides normal movement briefly.
    /// Skills (e.g., push) should call this instead of relying on AddForce,
    /// because this NPC moves via MovePosition.
    /// </summary>
    public void ApplyKnockback(Vector2 velocity)
    {
        float max = Mathf.Max(0.1f, knockbackMaxSpeed);
        if (velocity.sqrMagnitude > max * max)
            velocity = velocity.normalized * max;

        _knockVel = velocity;
        _knockTimer = Mathf.Max(_knockTimer, Mathf.Max(0.05f, knockbackDuration));
    }
    [SerializeField] private float repathInterval = 0.35f;
    [SerializeField] private float waypointTolerance = 0.35f;
    [SerializeField] private float turnResponsiveness = 14f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float minWanderTime = 0.6f;
    [SerializeField] private float maxWanderTime = 1.6f;
    [SerializeField] private float pauseChance = 0.15f;

    [Header("Knockback")]
    [Tooltip("If > 0, knockback overrides normal movement for this duration.")]
    [SerializeField] private float knockbackDuration = 0.18f;

    [Tooltip("Velocity damping applied each FixedUpdate while knocked back (0.85 = decays fast).")]
    [Range(0.5f, 0.99f)]
    [SerializeField] private float knockbackDamping = 0.85f;

    [Tooltip("Max knockback speed clamp (world units/sec). Prevents huge impulses from flinging NPCs.")]
    [SerializeField] private float knockbackMaxSpeed = 18f;

    [Header("A* Grid")]
    [Tooltip("World-space center of the grid. Default assumes map centered at (0,0).")]
    [SerializeField] private Vector2 gridCenter = Vector2.zero;
    [Tooltip("World-space size covered by the grid.")]
    [SerializeField] private Vector2 gridWorldSize = new Vector2(30f, 18f);
    [Tooltip("Cell size in world units. Smaller = better paths but slower.")]
    [SerializeField] private float cellSize = 1f;
    [Tooltip("Colliders in these layers are considered obstacles.")]
    [SerializeField] private LayerMask obstacleMask = ~0; // Everything

    [Header("Collision")]
    [SerializeField] private float repickCooldown = 0.15f;

    [Header("HP")]
    [SerializeField] private int maxHp = 30;

    [Header("Drop")]
    [Tooltip("Optional coin prefab. If null, a simple coin GameObject will be created.")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinDropMin = 1;
    [SerializeField] private int coinDropMax = 3;
    [Range(0f, 1f)]
    [SerializeField] private float coinDropChance = 0.85f;
    [SerializeField] private float coinScatterRadius = 0.6f;

    [Header("Visual")]
    [SerializeField] private int spriteSize = 48;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private CircleCollider2D _cc;

    private Vector2 _moveDir;
    private float _wanderTimer;
    private float _repickTimer;
    private bool _isPausing;
    // Knockback state (overrides normal movement)
    private float _knockTimer;
    private Vector2 _knockVel;

    private int _hp;
    private TextMesh _hpText;

    // A* path state
    private readonly List<Vector2> _path = new List<Vector2>();
    private int _pathIndex;
    private float _repathTimer;
    private Vector2 _chaseDir;
    private Vector2Int _lastGoalCell = new Vector2Int(int.MinValue, int.MinValue);

    private void Reset()
    {
        // Force-add required components (covers cases where RequireComponent didn't retroactively apply)
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _cc = GetComponent<CircleCollider2D>();
        if (_cc == null) _cc = gameObject.AddComponent<CircleCollider2D>();

        ConfigurePhysics();
    }

    private void OnValidate()
    {
        // Keep visible in Edit Mode.
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && _sr.sprite == null)
        {
            _sr.sprite = CreateNpcSprite(spriteSize, spriteSize);
        }
    }

    private void Awake()
    {
        // Force-add required components at runtime.
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _cc = GetComponent<CircleCollider2D>();
        if (_cc == null) _cc = gameObject.AddComponent<CircleCollider2D>();

        if (_sr.sprite == null)
        {
            _sr.sprite = CreateNpcSprite(spriteSize, spriteSize);
        }

        ConfigurePhysics();

        _hp = Mathf.Max(1, maxHp);
        CreateHpText();
        UpdateHpText();

        // Auto-find player if not assigned
        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }

        PickNewWander();
    }

    private void ConfigurePhysics()
    {
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (_cc != null)
        {
            _cc.isTrigger = false;
            _cc.radius = 0.42f;
        }
    }

    private void Update()
    {
        _repickTimer -= Time.deltaTime;

        var target = GetCurrentTarget();
        bool canChase = (target != null) && (Vector2.Distance(transform.position, target.position) <= chaseRange);

        if (canChase)
        {
            _repathTimer -= Time.deltaTime;
            if (_repathTimer <= 0f)
            {
                _repathTimer = repathInterval;
                RepathToPlayer();
            }
        }
        else
        {
            // Wander state
            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0f)
            {
                PickNewWander();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;
        // Knockback overrides normal movement so pushes are visible even with MovePosition.
        if (_knockTimer > 0f)
        {
            _knockTimer -= Time.fixedDeltaTime;

            Vector2 pos = _rb.position;
            Vector2 delta = _knockVel * Time.fixedDeltaTime;
            _rb.MovePosition(pos + delta);

            // Decay knockback velocity so it eases out.
            _knockVel *= Mathf.Clamp01(knockbackDamping);

            // While knocked, do not run chase/wander MovePosition this frame.
            return;
        }

        var target = GetCurrentTarget();
        bool canChase = (target != null) && (Vector2.Distance(transform.position, target.position) <= chaseRange);

        if (canChase && _path.Count > 0 && _pathIndex < _path.Count)
        {
            Vector2 pos = _rb.position;

            // If we're already very close to the current waypoint, advance until we find a meaningful target.
            float tolSqr = waypointTolerance * waypointTolerance;
            while (_pathIndex < _path.Count)
            {
                Vector2 toWp = _path[_pathIndex] - pos;
                if (toWp.sqrMagnitude > tolSqr) break;
                _pathIndex++;
            }

            if (_pathIndex >= _path.Count) return;

            Vector2 wpTarget = _path[_pathIndex];
            Vector2 desiredDir = (wpTarget - pos);
            if (desiredDir.sqrMagnitude > 0.0001f) desiredDir.Normalize();

            // Smooth turning to avoid left-right jitter when path updates frequently.
            _chaseDir = Vector2.Lerp(_chaseDir, desiredDir, turnResponsiveness * Time.fixedDeltaTime);
            if (_chaseDir.sqrMagnitude > 0.0001f) _chaseDir.Normalize();

            Vector2 delta = _chaseDir * (moveSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(pos + delta);
        }
        else
        {
            // Wander move
            if (_isPausing) return;
            Vector2 delta = _moveDir * (moveSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(_rb.position + delta);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // On first contact, re-pick direction quickly.
        if (_repickTimer > 0f) return;
        _repickTimer = repickCooldown;

        // If chasing, force a repath soon.
        if (player != null)
        {
            _repathTimer = 0f;
        }

        // In wander mode, bounce a bit.
        if (collision.contactCount > 0)
        {
            Vector2 n = collision.GetContact(0).normal;
            _moveDir = Vector2.Reflect(_moveDir, n).normalized;
        }

        // If we're being knocked back, damp velocity on collision.
        if (_knockTimer > 0f)
        {
            _knockVel *= 0.6f;
        }
    }

    private void PickNewWander()
    {
        _isPausing = (Random.value < pauseChance);

        if (_isPausing)
        {
            _moveDir = Vector2.zero;
        }
        else
        {
            _moveDir = Random.insideUnitCircle;
            if (_moveDir.sqrMagnitude < 0.0001f) _moveDir = Vector2.up;
            _moveDir.Normalize();
        }

        _wanderTimer = Random.Range(minWanderTime, maxWanderTime);
    }

    private void RepathToPlayer()
    {
        var target = GetCurrentTarget();
        if (target == null) return;

        // Avoid tiny path changes every interval (which can cause wobble).
        // Only recompute when the target moves into a new grid cell.
        if (WorldToGrid(target.position, out var goalCell))
        {
            if (goalCell == _lastGoalCell && _path.Count > 0 && _pathIndex < _path.Count)
            {
                return;
            }
            _lastGoalCell = goalCell;
        }

        Vector2 start = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 goal = target.position;

        _path.Clear();
        _pathIndex = 0;

        if (TryFindPathAStar(start, goal, out var newPath))
        {
            _path.AddRange(newPath);
            _pathIndex = 0;
        }
        else
        {
            // If no path, just move directly (will be blocked by physics)
            _path.Clear();
            _path.Add(goal);
            _pathIndex = 0;
        }
    }

    // -------------------------
    // A* Implementation
    // -------------------------

    private bool TryFindPathAStar(Vector2 startWorld, Vector2 goalWorld, out List<Vector2> worldPath)
    {
        worldPath = new List<Vector2>();

        if (!WorldToGrid(startWorld, out var start) || !WorldToGrid(goalWorld, out var goal))
            return false;

        int w = Mathf.Max(1, Mathf.RoundToInt(gridWorldSize.x / cellSize));
        int h = Mathf.Max(1, Mathf.RoundToInt(gridWorldSize.y / cellSize));

        // Early out if goal cell is blocked
        if (!IsWalkable(goal))
            return false;

        var open = new List<Node>();
        var openMap = new Dictionary<int, Node>();
        var closed = new HashSet<int>();

        Node startNode = new Node(start.x, start.y, g: 0, h: Heuristic(start, goal), parentKey: -1);
        open.Add(startNode);
        openMap[Key(start.x, start.y)] = startNode;

        int safety = 0;
        int safetyMax = w * h;

        while (open.Count > 0)
        {
            // Find node with lowest f (simple list scan)
            int bestIdx = 0;
            float bestF = open[0].F;
            for (int i = 1; i < open.Count; i++)
            {
                float f = open[i].F;
                if (f < bestF)
                {
                    bestF = f;
                    bestIdx = i;
                }
            }

            Node current = open[bestIdx];
            open.RemoveAt(bestIdx);
            openMap.Remove(Key(current.x, current.y));

            int cKey = Key(current.x, current.y);
            if (closed.Contains(cKey))
                continue;
            closed.Add(cKey);

            if (current.x == goal.x && current.y == goal.y)
            {
                // Reconstruct path
                var back = new List<Vector2Int>();
                back.Add(new Vector2Int(current.x, current.y));

                int pk = current.parentKey;
                while (pk != -1)
                {
                    if (!cameFrom.TryGetValue(pk, out var prev)) break;
                    back.Add(new Vector2Int(prev.x, prev.y));
                    pk = prev.parentKey;
                }

                back.Reverse();

                // Convert to world centers
                for (int i = 0; i < back.Count; i++)
                {
                    worldPath.Add(GridToWorld(back[i].x, back[i].y));
                }
                return true;
            }

            // Store parent chain
            cameFrom[cKey] = current;

            // Expand neighbors (4-dir; safer with grid obstacles)
            for (int ni = 0; ni < 4; ni++)
            {
                int nx = current.x + ((ni == 0) ? 1 : (ni == 1) ? -1 : 0);
                int ny = current.y + ((ni == 2) ? 1 : (ni == 3) ? -1 : 0);

                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;

                var nPos = new Vector2Int(nx, ny);
                int nKey = Key(nx, ny);
                if (closed.Contains(nKey)) continue;

                if (!IsWalkable(nPos)) continue;

                float g2 = current.g + 1f;
                float h2 = Heuristic(nPos, goal);
                var next = new Node(nx, ny, g2, h2, parentKey: cKey);

                // If it's already in open with a lower g, skip
                if (openMap.TryGetValue(nKey, out var existing) && existing.g <= g2)
                    continue;

                open.Add(next);
                openMap[nKey] = next;
            }

            // safety
            safety++;
            if (safety > safetyMax) break;
        }

        return false;
    }

    // Stores visited nodes for reconstruction (key -> node)
    private readonly Dictionary<int, Node> cameFrom = new Dictionary<int, Node>();

    private struct Node
    {
        public int x;
        public int y;
        public float g;
        public float h;
        public int parentKey;
        public float F => g + h;

        public Node(int x, int y, float g, float h, int parentKey)
        {
            this.x = x;
            this.y = y;
            this.g = g;
            this.h = h;
            this.parentKey = parentKey;
        }
    }

    private static int Key(int x, int y) => (x << 16) ^ y;

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private bool WorldToGrid(Vector2 world, out Vector2Int grid)
    {
        // Grid origin (bottom-left)
        Vector2 half = gridWorldSize * 0.5f;
        Vector2 origin = gridCenter - half;

        float rx = (world.x - origin.x) / cellSize;
        float ry = (world.y - origin.y) / cellSize;

        int gx = Mathf.FloorToInt(rx);
        int gy = Mathf.FloorToInt(ry);

        int w = Mathf.Max(1, Mathf.RoundToInt(gridWorldSize.x / cellSize));
        int h = Mathf.Max(1, Mathf.RoundToInt(gridWorldSize.y / cellSize));

        if (gx < 0 || gy < 0 || gx >= w || gy >= h)
        {
            grid = default;
            return false;
        }

        grid = new Vector2Int(gx, gy);
        return true;
    }

    private Vector2 GridToWorld(int gx, int gy)
    {
        Vector2 half = gridWorldSize * 0.5f;
        Vector2 origin = gridCenter - half;
        return new Vector2(
            origin.x + (gx + 0.5f) * cellSize,
            origin.y + (gy + 0.5f) * cellSize
        );
    }

    private bool IsWalkable(Vector2Int g)
    {
        Vector2 center = GridToWorld(g.x, g.y);
        Vector2 size = new Vector2(cellSize * 0.9f, cellSize * 0.9f);

        // OverlapBoxAll lets us ignore triggers and ourselves.
        var hits = Physics2D.OverlapBoxAll(center, size, 0f, obstacleMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;
            if (c.attachedRigidbody != null && c.attachedRigidbody.gameObject == gameObject) continue;

            // Ignore target collider so the goal cell can be occupied (player OR forced target).
            var target = GetCurrentTarget();
            if (target != null && (c.transform == target || c.transform.IsChildOf(target)))
                continue;

            return false; // blocked
        }

        return true;
    }

    // -------------------------
    // HP
    // -------------------------

    public void TakeDamage(int amount)
    {
        int finalAmount = Mathf.Max(0, amount);

        var mark = GetComponent<MarkTargetSkill.MarkTargetReceiverMono>();
        if (mark != null && mark.IsMarked)
        {
            finalAmount = Mathf.Max(1, Mathf.RoundToInt(finalAmount * mark.GetDamageMultiplier()));
        }

        _hp -= finalAmount;
        UpdateHpText();

        if (_hp <= 0)
        {
            DropCoins();
            Destroy(gameObject);
        }
    }

    private void CreateHpText()
    {
        GameObject textObj = new GameObject("HP_Text");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        _hpText = textObj.AddComponent<TextMesh>();
        _hpText.anchor = TextAnchor.MiddleCenter;
        _hpText.alignment = TextAlignment.Center;
        _hpText.characterSize = 0.2f;
        _hpText.fontSize = 32;
        _hpText.color = Color.red;
    }

    private void UpdateHpText()
    {
        if (_hpText != null)
        {
            _hpText.text = _hp.ToString();
        }
    }

    // -------------------------
    // Visual
    // -------------------------

    private static Sprite CreateNpcSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        // Blue-ish body with darker border
        Color fill = new Color(0.35f, 0.65f, 1f, 1f);
        Color border = new Color(0.10f, 0.18f, 0.30f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = (x == 0 || y == 0 || x == w - 1 || y == h - 1 || x == 1 || y == 1 || x == w - 2 || y == h - 2);
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Visualize chase range and grid bounds.
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.35f);
        Gizmos.DrawWireCube(gridCenter, gridWorldSize);

        // Draw current path
        if (_path != null && _path.Count > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0.1f, 0.8f);
            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.DrawLine(_path[i], _path[i + 1]);
            }
        }
    }
#endif

    private void DropCoins()
    {
        if (Random.value > coinDropChance) return;

        int min = Mathf.Max(0, coinDropMin);
        int max = Mathf.Max(min, coinDropMax);
        int count = Random.Range(min, max + 1);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * coinScatterRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);

            if (coinPrefab != null)
            {
                Instantiate(coinPrefab, pos, Quaternion.identity);
            }
            else
            {
                var coin = new GameObject("Coin");
                coin.transform.position = pos;
                coin.AddComponent<ProbCoin>();
            }
        }
    }
}
