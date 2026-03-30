using UnityEngine;

/// <summary>
/// BasicDoubleThrowSkill
/// - Fires two sequential projectiles at the same target.
/// - Intended as the ranged DPS basic attack.
/// - Compatible with SkillBrainMono reflection-based Execute(Transform, Transform) usage.
/// - Uses VFX ScriptableObjects for cast / projectile / hit presentation.
/// </summary>
[CreateAssetMenu(fileName = "BasicDoubleThrowSkill", menuName = "BS/Skills/Basic Double Throw Skill")]
public class BasicDoubleThrowSkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Projectile")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float spawnOffset = 0.55f;
    [SerializeField] private float secondShotDelay = 0.08f;

    [Header("Damage")]
    [SerializeField] private float damage = 1f;

    [Header("Visual")]
    [SerializeField] private int sortingOrder = 20;
    [SerializeField] private Color projectileColor = new Color(1f, 0.95f, 0.45f, 1f);

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedProjectile projectileVfx;
    [SerializeField] private VFX_RangedHit hitVfx;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public new BattleSkillCategory Category => BattleSkillCategory.Attack;
    public new BattleSkillTargetType TargetType => BattleSkillTargetType.Enemy;
    public new BattleSkillTacticalNeed TacticalNeed => BattleSkillTacticalNeed.OffensivePressure;

    public float Damage => damage;

    public float EvaluateBrainScore(BrainContext context, int roleBias = 0)
    {
        float score = 18f + roleBias;

        score += context.nearbyEnemyCount * 2f;

        if (context.role == Role.DPS)
            score += 10f;

        if (context.partyState == StateMono.PartyState.Aggressive)
            score += 6f;

        if (context.selfHp01 < 0.3f)
            score -= 6f;

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
                Debug.Log($"[BasicDoubleThrowSkill] Execute failed: no target found for caster={caster.name}");
            return false;
        }

        return Execute(caster, target);
    }
    /*
    도발 : 적대감, 유인, 진형 유지
    적대감 -> 의미? 작은 개념?
    타겟 전환, 어그로, 
    강제, 진입 유도, 
    # 도발
    적군 / 방어 / 단일 / 타겟팅 / 컨티뉴스
    # 관통샷
    적군 / 공격 / 다중 / 타겟팅 / 인스턴트 / 관통
    # 범위 도트 데미지
    적군 / 공격 / 다중 / 논타겟팅 / 도트 / 지역
    # 단일 힐
    아군 / 회복 / 단일 / 타겟팅 / 인스턴트
    # 범위 힐
    아군 / 회복 / 다중 / 논타겟팅 / 도트 / 지역
    # 가드
    셀프 / 방어 / 버프 / 타겟팅 / 컨티뉴스 
    # 넉백
    적군 / 제어 / 다중 / 논타겟팅 / 인스턴트 / 지역
    */

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

        // Presentation VFX for cast
        castVfx?.Play(caster, dir);

        // First shot immediately.
        FireProjectile(caster, dir);

        // Second shot shortly after, toward the same target.
        var delayed = new GameObject("DoubleThrowDelayedShot");
        var delayedShot = delayed.AddComponent<DelayedSecondShotMono>();
        delayedShot.Initialize(this, caster, target, Mathf.Max(0.01f, secondShotDelay), debugLog);

        if (debugLog)
            Debug.Log($"[BasicDoubleThrowSkill] Fire double throw caster={caster.name} target={target.name} dmg={damage:0.##} delay={secondShotDelay:0.##}");

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
            if (hit == null)
                continue;
            if (hit.isTrigger)
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
            Debug.Log($"[BasicDoubleThrowSkill] FindBestTarget caster={caster.name} radius={Radius:0.##} result={bestName}");
        }

        return bestTarget;
    }

    private void FireProjectile(Transform caster, Vector2 dir)
    {
        Vector2 spawnPos = (Vector2)caster.position + dir * spawnOffset;

        // Presentation VFX for the flying projectile.
        if (projectileVfx != null)
        {
            var tempTarget = new GameObject("DoubleThrowVfxTarget");
            tempTarget.transform.position = (Vector2)spawnPos + dir * maxDistance;
            projectileVfx.Play(caster, tempTarget.transform);
            Destroy(tempTarget, 0.05f);
        }

        var go = new GameObject("DoubleThrowProjectile");
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
        col.radius = 0.16f;

        var proj = go.AddComponent<DoubleThrowProjectileMono>();
        proj.Initialize(caster, enemyMask, ignoreSameRoot, dir, speed, maxDistance, lifeTime, damage, debugLog, hitVfx);
    }

    private Sprite CreateProjectileSprite()
    {
        var tex = new Texture2D(10, 10, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color32[10 * 10];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        // Simple bright diamond-like projectile
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                float dx = Mathf.Abs(x - 4.5f);
                float dy = Mathf.Abs(y - 4.5f);
                if (dx + dy <= 4.5f)
                    pixels[y * 10 + x] = new Color32(255, 255, 255, 255);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 10, 10), new Vector2(0.5f, 0.5f), 16f);
    }

    private class DoubleThrowProjectileMono : MonoBehaviour
    {
        private Transform _caster;
        private LayerMask _enemyMask;
        private bool _ignoreSameRoot;
        private Vector2 _dir;
        private float _speed;
        private float _maxDist;
        private float _life;
        private float _damage;
        private bool _debugLog;
        private VFX_RangedHit _hitVfx;
        private Vector2 _start;
        private float _t;
        private bool _hit;

        public void Initialize(Transform caster, LayerMask enemyMask, bool ignoreSameRoot, Vector2 dir, float speed, float maxDist, float lifeTime, float damage, bool debugLog, VFX_RangedHit hitVfx)
        {
            _caster = caster;
            _enemyMask = enemyMask;
            _ignoreSameRoot = ignoreSameRoot;
            _dir = dir.normalized;
            _speed = Mathf.Max(0.1f, speed);
            _maxDist = Mathf.Max(0.1f, maxDist);
            _life = Mathf.Max(0.05f, lifeTime);
            _damage = Mathf.Max(0f, damage);
            _debugLog = debugLog;
            _hitVfx = hitVfx;
            _start = transform.position;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            transform.position += (Vector3)(_dir * (_speed * Time.deltaTime));

            float traveled = Vector2.Distance(_start, transform.position);
            if (_t >= _life || traveled >= _maxDist)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hit) return;
            if (other == null || other.isTrigger) return;

            Transform otherRoot = other.transform.root;
            Transform casterRoot = _caster != null ? _caster.root : null;

            if (_ignoreSameRoot && casterRoot != null && otherRoot == casterRoot)
            {
                if (_debugLog)
                    Debug.Log($"[DoubleThrow] Ignore same root target={other.name}");
                return;
            }

            int otherLayerBit = 1 << other.gameObject.layer;
            if ((_enemyMask.value & otherLayerBit) == 0)
            {
                if (_debugLog)
                    Debug.Log($"[DoubleThrow] Ignore non-enemy layer target={other.name} layer={LayerMask.LayerToName(other.gameObject.layer)}");
                return;
            }

            if (_debugLog)
                Debug.Log($"[DoubleThrow] trigger with {other.name} trigger={other.isTrigger}");

            // Prefer StatMono damage path.
            var stat = other.GetComponentInParent<StatMono>();
            if (stat != null)
            {
                stat.TakeDamage(_damage);
                _hitVfx?.Play(other.transform);
                _hit = true;

                if (_debugLog)
                    Debug.Log($"[BasicDoubleThrowSkill] Hit StatMono target={stat.name} dmg={_damage:0.##}");

                Destroy(gameObject);
                return;
            }

            // Legacy fallback for enemies using NpcMono HP.
            var npc = other.GetComponentInParent<NpcMono>();
            if (npc != null)
            {
                npc.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(_damage)));
                _hitVfx?.Play(npc.transform);
                _hit = true;

                if (_debugLog)
                    Debug.Log($"[BasicDoubleThrowSkill] Hit NpcMono target={npc.name} dmg={_damage:0.##}");

                Destroy(gameObject);
                return;
            }

            if (_debugLog)
                Debug.Log($"[DoubleThrow] Enemy-layer collision had no StatMono/NpcMono target={other.name}");
        }
    }
    public class DelayedSecondShotMono : MonoBehaviour
    {
        private BasicDoubleThrowSkill _owner;
        private Transform _caster;
        private Transform _target;
        private float _delay;
        private float _t;
        private bool _debugLog;

        public void Initialize(BasicDoubleThrowSkill owner, Transform caster, Transform target, float delay, bool debugLog)
        {
            _owner = owner;
            _caster = caster;
            _target = target;
            _delay = Mathf.Max(0.01f, delay);
            _debugLog = debugLog;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            if (_t < _delay)
                return;

            if (_owner != null && _caster != null && _target != null)
            {
                Vector2 from = _caster.position;
                Vector2 to = _target.position;
                Vector2 dir = to - from;
                if (dir.sqrMagnitude < 0.0001f)
                    dir = Vector2.right;
                dir.Normalize();

                _owner.FireProjectile(_caster, dir);

                if (_debugLog)
                    Debug.Log($"[BasicDoubleThrowSkill] Fire delayed second shot caster={_caster.name} target={_target.name}");
            }

            Destroy(gameObject);
        }
    }
}
