using UnityEngine;

/// <summary>
/// JudgementMarkSkill
/// - Creates a damage-over-time zone.
/// - Enemies inside the zone receive periodic damage.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
/// </summary>
[CreateAssetMenu(fileName = "JudgementMarkSkill", menuName = "BS/Skills/Judgement Mark Skill")]
public class JudgementMarkSkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Zone")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Damage")]
    [SerializeField] private float damagePerTick = 2f;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Visual")]
    [SerializeField] private Color ringColor = new Color(1f, 0.2f, 0.6f, 0.7f);
    [SerializeField] private int sortingOrder = 35;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public float ZoneRadius => radius;
    public float Duration => duration;
    public float TickInterval => tickInterval;
    public float DamagePerTick => damagePerTick;

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
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

    private void SpawnZone(Vector3 pos)
    {
        GameObject go = new GameObject("JudgementMarkZone");
        go.transform.position = pos;

        var zone = go.AddComponent<JudgementZoneMono>();
        zone.Initialize(enemyMask, radius, duration, tickInterval, damagePerTick, sortingOrder, ringColor, debugLog, impactVfx);
    }


    private class JudgementZoneMono : MonoBehaviour
    {
        private LayerMask _enemyMask;
        private float _radius;
        private float _duration;
        private float _tickInterval;
        private float _damage;
        private bool _debugLog;
        private VFX_RangedHit _impactVfx;

        private float _lifeTimer;
        private float _tickTimer;

        private SpriteRenderer _ring;
        private float _startAlpha;

        public void Initialize(LayerMask mask, float r, float dur, float tick, float dmg, int order, Color color, bool debug, VFX_RangedHit impactVfx)
        {
            _enemyMask = mask;
            _radius = Mathf.Max(0.1f, r);
            _duration = Mathf.Max(0.1f, dur);
            _tickInterval = Mathf.Max(0.05f, tick);
            _damage = Mathf.Max(0f, dmg);
            _debugLog = debug;
            _impactVfx = impactVfx;

            _lifeTimer = _duration;
            _tickTimer = 0f;

            BuildVisual(order, color);
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
            var hits = Physics2D.OverlapCircleAll(transform.position, _radius, _enemyMask);

            for (int i = 0; i < hits.Length; i++)
            {
                var c = hits[i];
                if (c == null || c.isTrigger) continue;

                var stat = c.GetComponentInParent<StatMono>();
                if (stat != null)
                {
                    TryDamage(stat, _damage);
                    _impactVfx?.Play(stat.transform);

                    if (_debugLog)
                        Debug.Log($"[JudgementMark] DOT StatMono {stat.name} dmg={_damage}");

                    continue;
                }

                var npc = c.GetComponentInParent<NpcMono>();
                if (npc != null)
                {
                    npc.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(_damage)));
                    _impactVfx?.Play(npc.transform);

                    if (_debugLog)
                        Debug.Log($"[JudgementMark] DOT NpcMono {npc.name} dmg={_damage}");
                }
            }
        }


        private static void TryDamage(StatMono stat, float dmg)
        {
            var type = stat.GetType();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            var m = type.GetMethod("TakeDamage", flags);
            if (m != null)
            {
                try
                {
                    m.Invoke(stat, new object[] { dmg });
                    return;
                }
                catch { }
            }

            var hpField = type.GetField("currentHp", flags) ?? type.GetField("CurrentHp", flags);
            if (hpField != null && hpField.FieldType == typeof(float))
            {
                float hp = (float)hpField.GetValue(stat);
                hp -= dmg;
                hpField.SetValue(stat, hp);
            }
        }


        private void BuildVisual(int order, Color color)
        {
            GameObject ring = new GameObject("JudgementRing");
            ring.transform.SetParent(transform, false);

            _ring = ring.AddComponent<SpriteRenderer>();
            _ring.sprite = CreateRingSprite(96, 96);
            _ring.sortingOrder = order;
            _ring.color = color;

            _startAlpha = color.a;

            const float baseRadiusUnits = (96f * 0.45f) / 16f;
            float scale = _radius / baseRadiusUnits;
            ring.transform.localScale = new Vector3(scale, scale, 1f);
        }


        private void UpdateVisual()
        {
            if (_ring == null) return;

            float t = Mathf.Clamp01(1f - (_lifeTimer / _duration));

            var c = _ring.color;
            c.a = Mathf.Lerp(_startAlpha, 0f, t);
            _ring.color = c;
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
