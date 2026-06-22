using UnityEngine;

/// <summary>
/// PiercingShotSkill
/// - Fires a long straight projectile that can pierce through multiple enemies.
/// - Intended as a ranged DPS active skill for line clear.
/// - Compatible with SkillBrainMono reflection-based Execute(Transform, Transform).
/// - Uses VFX ScriptableObjects for cast / projectile / hit presentation.
/// </summary>
[CreateAssetMenu(fileName = "PiercingShotSkill", menuName = "BS/Skills/Piercing Shot Skill")]
public class PiercingShotSkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Projectile")]
    [SerializeField] private float speed = 16f;
    [SerializeField] private float maxDistance = 14f;
    [SerializeField] private float lifeTime = 2f; 
    [SerializeField] private float spawnOffset = 0.65f;
    [SerializeField] private float hitRadius = 0.18f;

    [Header("Damage")]
    [SerializeField] private float damage = 4f;
    [SerializeField] private int maxHits = 99;

    [Header("Visual")]
    [SerializeField] private int sortingOrder = 24;
    [SerializeField] private Color projectileColor = new Color(1f, 0.9f, 0.25f, 1f);

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedProjectile projectileVfx;
    [SerializeField] private VFX_RangedHit hitVfx;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public new BattleSkillCategory Category => BattleSkillCategory.Attack;
    public new BattleSkillTargetType TargetType => BattleSkillTargetType.Enemy;
    public new BattleSkillTacticalNeed TacticalNeed => BattleSkillTacticalNeed.OffensivePressure;

    public new float Range => maxDistance;
    public float Damage => damage;

    public float EvaluateBrainScore(BrainContext context, int roleBias = 0)
    {
        float score = 26f + roleBias;

        score += context.nearbyEnemyCount * 3f;

        if (context.role == Role.DPS)
            score += 12f;

        if (context.partyState == StateMono.PartyState.Aggressive)
            score += 8f;

        if (context.selfHp01 < 0.3f)
            score -= 8f;

        return score;
    }

    public bool Execute(Transform caster)
    {
        if (caster == null)
            return false;

        Transform target = FindBestTarget(caster);
        if (target == null)
        {
            if (debugLog)
                Debug.Log($"[PiercingShotSkill] Execute failed: no target found for caster={caster.name}");
            return false;
        }

        return Execute(caster, target);
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null || target == null)
            return false;

        Vector2 from = caster.position;
        Vector2 to = target.position;

        Vector2 dir = to - from;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector2.right;
        dir.Normalize();

        castVfx?.Play(caster, dir);
        FireProjectile(caster, dir);

        if (debugLog)
            Debug.Log($"[PiercingShotSkill] Fire caster={caster.name} target={target.name} dmg={damage:0.##}");

        return true;
    }

    private Transform FindBestTarget(Transform caster)
    {
        if (caster == null)
            return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.position, Radius, enemyMask);
        if (hits == null || hits.Length == 0)
            return null;

        Transform casterRoot = caster.root;
        Transform bestTarget = null;
        float bestDistSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.isTrigger)
                continue;

            Transform hitTransform = hit.transform;
            if (hitTransform == null)
                continue;

            Transform hitRoot = hitTransform.root;
            if (ignoreSameRoot && casterRoot != null && hitRoot == casterRoot)
                continue;

            StatMono stat = hit.GetComponentInParent<StatMono>();
            NpcMono npc = hit.GetComponentInParent<NpcMono>();
            if (stat == null && npc == null)
                continue;

            Vector2 delta = (Vector2)hitTransform.position - (Vector2)caster.position;
            float distSqr = delta.sqrMagnitude;
            if (distSqr < bestDistSqr)
            {
                bestDistSqr = distSqr;
                bestTarget = stat != null ? stat.transform : npc.transform;
            }
        }

        if (debugLog)
        {
            string bestName = bestTarget != null ? bestTarget.name : "null";
            Debug.Log($"[PiercingShotSkill] FindBestTarget caster={caster.name} radius={Radius:0.##} result={bestName}");
        }

        return bestTarget;
    }

    private void FireProjectile(Transform caster, Vector2 dir)
    {
        Vector2 spawnPos = (Vector2)caster.position + dir * spawnOffset;

        // Presentation VFX for the flying projectile.
        if (projectileVfx != null)
        {
            var tempTarget = new GameObject("PiercingShotVfxTarget");
            tempTarget.transform.position = (Vector2)spawnPos + dir * maxDistance;
            projectileVfx.Play(caster, tempTarget.transform);
            Destroy(tempTarget, 0.05f);
        }

        var go = new GameObject("PiercingShotProjectile");
        go.transform.position = spawnPos;

        // Keep a visible fallback sprite when projectileVfx is not assigned.
        if (projectileVfx == null)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateProjectileSprite();
            sr.sortingOrder = sortingOrder;
            sr.color = projectileColor;
        }

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = Mathf.Max(0.05f, hitRadius);

        var proj = go.AddComponent<PiercingProjectileMono>();
        proj.Initialize(caster, enemyMask, ignoreSameRoot, dir, speed, maxDistance, lifeTime, damage, maxHits, debugLog, hitVfx);
    }

    private Sprite CreateProjectileSprite()
    {
        const int w = 22;
        const int h = 8;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color32[w * h];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        int cy = h / 2;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool body = Mathf.Abs(y - cy) <= 1 && x >= 2 && x <= w - 4;
                bool tip = x > w - 4 && Mathf.Abs(y - cy) <= (w - 1 - x);
                bool tail = x < 2 && Mathf.Abs(y - cy) <= x;
                if (body || tip || tail)
                    pixels[y * w + x] = new Color32(255, 255, 255, 255);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0f, 0.5f), 16f);
    }

    private class PiercingProjectileMono : MonoBehaviour
    {
        private Transform _caster;
        private LayerMask _enemyMask;
        private bool _ignoreSameRoot;
        private Vector2 _dir;
        private float _speed;
        private float _maxDist;
        private float _life;
        private float _damage;
        private int _maxHits;
        private bool _debugLog;
        private VFX_RangedHit _hitVfx;
        private Vector2 _start;
        private float _t;
        private int _hitCount;
        private readonly System.Collections.Generic.HashSet<int> _hitIds = new System.Collections.Generic.HashSet<int>();

        public void Initialize(Transform caster, LayerMask enemyMask, bool ignoreSameRoot, Vector2 dir, float speed, float maxDist, float lifeTime, float damage, int maxHits, bool debugLog, VFX_RangedHit hitVfx)
        {
            _caster = caster;
            _enemyMask = enemyMask;
            _ignoreSameRoot = ignoreSameRoot;
            _dir = dir.normalized;
            _speed = Mathf.Max(0.1f, speed);
            _maxDist = Mathf.Max(0.1f, maxDist);
            _life = Mathf.Max(0.05f, lifeTime);
            _damage = Mathf.Max(0f, damage);
            _maxHits = Mathf.Max(1, maxHits);
            _debugLog = debugLog;
            _hitVfx = hitVfx;
            _start = transform.position;

            float rotZ = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        }

        private void Update()
        {
            _t += Time.deltaTime;
            transform.position += (Vector3)(_dir * (_speed * Time.deltaTime));

            float traveled = Vector2.Distance(_start, transform.position);
            if (_t >= _life || traveled >= _maxDist || _hitCount >= _maxHits)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
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
                int id = stat.gameObject.GetInstanceID();
                if (_hitIds.Contains(id)) return;

                _hitIds.Add(id);
                _hitCount++;
                stat.TakeDamage(_damage);
                _hitVfx?.Play(stat.transform);

                if (_debugLog)
                    Debug.Log($"[PiercingShotSkill] Hit StatMono target={stat.name} dmg={_damage:0.##} hitCount={_hitCount}");

                return;
            }

            var npc = other.GetComponentInParent<NpcMono>();
            if (npc != null)
            {
                int id = npc.gameObject.GetInstanceID();
                if (_hitIds.Contains(id)) return;

                _hitIds.Add(id);
                _hitCount++;
                npc.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(_damage)));
                _hitVfx?.Play(npc.transform);

                if (_debugLog)
                    Debug.Log($"[PiercingShotSkill] Hit NpcMono target={npc.name} dmg={_damage:0.##} hitCount={_hitCount}");
            }
        }
    }
}
