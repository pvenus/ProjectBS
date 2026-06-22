using UnityEngine;

/// <summary>
/// VolleyRainSkill
/// - Spawns an area at a target point.
/// - The area repeatedly damages enemies inside it, like a ranged rain / bombardment skill.
/// - Intended as the ranged DPS area-clear active skill.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
/// </summary>
[CreateAssetMenu(fileName = "VolleyRainSkill", menuName = "BS/Skills/Volley Rain Skill")]
public class VolleyRainSkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Area")]
    [SerializeField] private float duration = 3.0f;
    [SerializeField] private float tickInterval = 0.35f;

    [Header("Damage")]
    [SerializeField] private float damagePerTick = 2.0f;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Visual")]
    [SerializeField] private Color ringColor = new Color(1f, 0.75f, 0.25f, 0.7f);
    [SerializeField] private int sortingOrder = 35;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public new BattleSkillCategory Category => BattleSkillCategory.Attack;
    public new BattleSkillTargetType TargetType => BattleSkillTargetType.Point;
    public new BattleSkillTacticalNeed TacticalNeed => BattleSkillTacticalNeed.AreaControl;

    public float Duration => duration;
    public float TickInterval => tickInterval;
    public float DamagePerTick => damagePerTick;

    public float EvaluateBrainScore(BrainContext context, int roleBias = 0)
    {
        float score = 28f + roleBias;

        score += context.nearbyEnemyCount * 3.5f;

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
        if (caster == null) return false;

        castVfx?.Play(caster, Vector3.up);
        SpawnZone(caster.position);
        return true;
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null) return false;

        Vector3 pos = target != null ? target.position : caster.position;
        Vector3 dir = pos - caster.position;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.up;

        castVfx?.Play(caster, dir.normalized);
        SpawnZone(pos);
        return true;
    }

    public bool Execute(Transform caster, Vector3 point)
    {
        if (caster == null)
            return false;

        Vector3 dir = point - caster.position;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.up;

        castVfx?.Play(caster, dir.normalized);
        SpawnZone(point);
        return true;
    }

    private void SpawnZone(Vector3 pos)
    {
        GameObject go = new GameObject("VolleyRainZone");
        go.transform.position = pos;

        var zone = go.AddComponent<VolleyRainZoneMono>();
        zone.Initialize(
            enemyMask,
            Mathf.Max(0.1f, Radius),
            Mathf.Max(0.05f, duration),
            Mathf.Max(0.05f, tickInterval),
            Mathf.Max(0f, damagePerTick),
            sortingOrder,
            ringColor,
            debugLog,
            impactVfx
        );
    }

    private class VolleyRainZoneMono : MonoBehaviour
    {
        private LayerMask _enemyMask;
        private float _radius;
        private float _duration;
        private float _tickInterval;
        private float _damage;
        private int _sortingOrder;
        private Color _ringColor;
        private bool _debugLog;
        private VFX_RangedHit _impactVfx;

        private float _lifeTimer;
        private float _tickTimer;
        private SpriteRenderer _ringRenderer;
        private float _startAlpha;

        public void Initialize(
            LayerMask enemyMask,
            float radius,
            float duration,
            float tickInterval,
            float damage,
            int sortingOrder,
            Color ringColor,
            bool debugLog,
            VFX_RangedHit impactVfx)
        {
            _enemyMask = enemyMask;
            _radius = radius;
            _duration = duration;
            _tickInterval = tickInterval;
            _damage = damage;
            _sortingOrder = sortingOrder;
            _ringColor = ringColor;
            _debugLog = debugLog;
            _impactVfx = impactVfx;

            _lifeTimer = _duration;
            _tickTimer = 0f;

            BuildVisual();
        }

        private void Update()
        {
            _lifeTimer -= Time.deltaTime;
            _tickTimer -= Time.deltaTime;

            if (_tickTimer <= 0f)
            {
                _tickTimer = _tickInterval;
                DamageEnemies();
            }

            UpdateVisual();

            if (_lifeTimer <= 0f)
                Destroy(gameObject);
        }

        private void DamageEnemies()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _radius, _enemyMask);
            if (hits == null || hits.Length == 0)
                return;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D c = hits[i];
                if (c == null) continue;
                if (c.isTrigger) continue;

                var stat = c.GetComponentInParent<StatMono>();
                if (stat != null)
                {
                    stat.TakeDamage(_damage);
                    _impactVfx?.Play(stat.transform);

                    if (_debugLog)
                        Debug.Log($"[VolleyRainSkill] DOT StatMono {stat.name} dmg={_damage:0.##}");

                    continue;
                }

                var npc = c.GetComponentInParent<NpcMono>();
                if (npc != null)
                {
                    npc.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(_damage)));
                    _impactVfx?.Play(npc.transform);

                    if (_debugLog)
                        Debug.Log($"[VolleyRainSkill] DOT NpcMono {npc.name} dmg={_damage:0.##}");
                }
            }
        }

        private void BuildVisual()
        {
            var ring = new GameObject("VolleyRainRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.zero;

            _ringRenderer = ring.AddComponent<SpriteRenderer>();
            _ringRenderer.sprite = CreateRingSprite(96, 96);
            _ringRenderer.sortingOrder = _sortingOrder;
            _ringRenderer.color = _ringColor;
            _startAlpha = _ringColor.a;

            const float baseRadiusUnits = (96f * 0.45f) / 16f;
            float scale = _radius / Mathf.Max(0.001f, baseRadiusUnits);
            ring.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private void UpdateVisual()
        {
            if (_ringRenderer == null) return;

            float t = Mathf.Clamp01(1f - (_lifeTimer / Mathf.Max(0.001f, _duration)));
            var c = _ringRenderer.color;
            c.a = Mathf.Lerp(_startAlpha, 0f, t);
            _ringRenderer.color = c;
        }

        private static Sprite CreateRingSprite(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            float cx = (w - 1) * 0.5f;
            float cy = (h - 1) * 0.5f;
            float r = Mathf.Min(w, h) * 0.45f;
            float thickness = 3f;

            var pixels = new Color32[w * h];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(0, 0, 0, 0);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);

                    if (Mathf.Abs(d - r) <= thickness)
                        pixels[y * w + x] = new Color32(255, 255, 255, 255);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
