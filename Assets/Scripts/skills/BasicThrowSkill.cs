using UnityEngine;

/// <summary>
/// BasicThrowSkill
/// - Basic ranged throw used by the healer / support baseline attack.
/// - Self-contained: finds nearest Npc target within range using enemyMask.
/// - Starts using VFX ScriptableObjects for cast / projectile / hit presentation.
/// </summary>
[CreateAssetMenu(menuName = "BS/Skills/Basic Throw Skill", fileName = "BasicThrowSkill")]
public class BasicThrowSkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Projectile")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileDistance = 10f;
    [SerializeField] private float projectileLifetime = 2.5f;
    [SerializeField] private int damagePerHit = 1;
    [SerializeField] private float projectileSpawnOffset = 0.6f;

    [Header("Visual")]
    [SerializeField] private int projectileSpriteSize = 16;
    [SerializeField] private int sortingOrder = 6;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedProjectile projectileVfx;
    [SerializeField] private VFX_RangedHit hitVfx;

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    /// <summary>
    /// Executes the throw from caster toward nearest NpcMono target.
    /// Returns true if fired.
    /// </summary>
    public bool Execute(Transform caster)
    {
        if (caster == null) return false;

        var target = FindNearestNpc(caster.position);
        if (target == null) return false;

        FireAt(caster, target.transform);
        return true;
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null || target == null) return false;

        FireAt(caster, target);
        return true;
    }

    private NpcMono FindNearestNpc(Vector2 origin)
    {
        float searchRange = Mathf.Max(0.1f, Range);
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, searchRange, enemyMask);
        float best = float.PositiveInfinity;
        NpcMono bestNpc = null;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            var npc = c.GetComponentInParent<NpcMono>();
            if (npc == null) continue;

            float d = ((Vector2)npc.transform.position - origin).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestNpc = npc;
            }
        }

        return bestNpc;
    }

    private void FireAt(Transform caster, Transform target)
    {
        Vector2 from = caster.position;
        Vector2 to = target.position;

        Vector2 dir = (to - from);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
        dir.Normalize();

        // Presentation VFX
        castVfx?.Play(caster, dir);
        projectileVfx?.Play(caster, target);

        Vector2 spawnPos = from + dir * projectileSpawnOffset;

        GameObject proj = new GameObject("BasicProjectile");
        proj.transform.position = spawnPos;

        // Keep a visible fallback sprite when projectile VFX is not assigned.
        if (projectileVfx == null)
        {
            var sr = proj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateProjectileSprite(projectileSpriteSize, projectileSpriteSize);
            sr.sortingOrder = sortingOrder;
        }

        var rb = proj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.22f;

        var projectile = proj.AddComponent<BasicThrowProjectileMono>();
        projectile.Initialize(
            caster,
            enemyMask,
            ignoreSameRoot,
            dir,
            projectileSpeed,
            projectileDistance,
            projectileLifetime,
            damagePerHit,
            hitVfx
        );
    }

    private class BasicThrowProjectileMono : MonoBehaviour
    {
        private Transform _caster;
        private LayerMask _enemyMask;
        private bool _ignoreSameRoot;
        private Vector2 _dir;
        private float _speed;
        private float _maxDist;
        private float _life;
        private int _damage;
        private VFX_RangedHit _hitVfx;
        private Vector2 _start;
        private bool _hit;

        public void Initialize(Transform caster, LayerMask enemyMask, bool ignoreSameRoot, Vector2 dir, float speed, float maxDist, float lifetime, int damage, VFX_RangedHit hitVfx)
        {
            _caster = caster;
            _enemyMask = enemyMask;
            _ignoreSameRoot = ignoreSameRoot;
            _dir = dir.normalized;
            _speed = Mathf.Max(0.1f, speed);
            _maxDist = Mathf.Max(0.1f, maxDist);
            _life = Mathf.Max(0.05f, lifetime);
            _damage = Mathf.Max(1, damage);
            _hitVfx = hitVfx;
            _start = transform.position;
        }

        private void Update()
        {
            transform.position += (Vector3)(_dir * (_speed * Time.deltaTime));

            _life -= Time.deltaTime;
            if (_life <= 0f || Vector2.Distance(_start, transform.position) >= _maxDist)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hit) return;
            if (other == null || other.isTrigger) return;

            Transform otherRoot = other.transform.root;
            Transform casterRoot = _caster != null ? _caster.root : null;

            if (_ignoreSameRoot && casterRoot != null && otherRoot == casterRoot)
                return;

            int otherLayerBit = 1 << other.gameObject.layer;
            if ((_enemyMask.value & otherLayerBit) == 0)
                return;

            var stat = other.GetComponentInParent<StatMono>();
            if (stat != null)
            {
                stat.TakeDamage(_damage);
                _hitVfx?.Play(other.transform);
                _hit = true;
                Destroy(gameObject);
                return;
            }

            var npc = other.GetComponentInParent<NpcMono>();
            if (npc != null)
            {
                npc.TakeDamage(_damage);
                _hitVfx?.Play(npc.transform);
                _hit = true;
                Destroy(gameObject);
            }
        }
    }

    private static Sprite CreateProjectileSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color c = new Color(0.75f, 1f, 0.75f, 1f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}