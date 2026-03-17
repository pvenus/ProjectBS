using UnityEngine;

/// <summary>
/// SanctuarySkill
/// - Spawns a temporary sanctuary zone at the caster position.
/// - Allies inside the zone are periodically healed.
/// - Intended as a healer/support area skill.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
/// </summary>
[CreateAssetMenu(fileName = "SanctuarySkill", menuName = "BS/Skills/Sanctuary Skill")]
public class SanctuarySkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask allyMask = ~0;

    [Header("Sanctuary")]
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float healPerTick = 2f;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Visual")]
    [SerializeField] private int sortingOrder = 30;
    [SerializeField] private Color ringColor = new Color(0.35f, 1f, 0.55f, 0.65f);

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public float ZoneRadius => radius;
    public float Duration => duration;
    public float TickInterval => tickInterval;
    public float HealPerTick => healPerTick;

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    public bool Execute(Transform caster)
    {
        if (caster == null) return false;

        castVfx?.Play(caster, Vector3.up);

        SpawnSanctuary(caster.position);
        return true;
    }

    public bool Execute(Transform caster, Transform target)
    {
        Vector3 pos = (target != null) ? target.position : (caster != null ? caster.position : Vector3.zero);
        return Execute(caster, pos);
    }

    public bool Execute(Transform caster, Vector3 point)
    {
        if (caster == null) return false;

        Vector3 dir = point - caster.position;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.up;

        castVfx?.Play(caster, dir.normalized);

        SpawnSanctuary(point);
        return true;
    }

    private void SpawnSanctuary(Vector3 worldPos)
    {
        GameObject go = new GameObject("SanctuaryZone");
        go.transform.position = worldPos;

        var zone = go.AddComponent<SanctuaryZoneMono>();
        zone.Initialize(
            allyMask,
            Mathf.Max(0.1f, radius),
            Mathf.Max(0.05f, duration),
            Mathf.Max(0.05f, tickInterval),
            Mathf.Max(0f, healPerTick),
            sortingOrder,
            ringColor,
            debugLog,
            impactVfx
        );
    }

    private class SanctuaryZoneMono : MonoBehaviour
    {
        private LayerMask _allyMask;
        private float _radius;
        private float _duration;
        private float _tickInterval;
        private float _healPerTick;
        private int _sortingOrder;
        private Color _ringColor;
        private bool _debugLog;
        private VFX_RangedHit _impactVfx;

        private float _lifeTimer;
        private float _tickTimer;
        private SpriteRenderer _ringRenderer;
        private float _startAlpha;

        public void Initialize(
            LayerMask allyMask,
            float radius,
            float duration,
            float tickInterval,
            float healPerTick,
            int sortingOrder,
            Color ringColor,
            bool debugLog,
            VFX_RangedHit impactVfx)
        {
            _allyMask = allyMask;
            _radius = radius;
            _duration = duration;
            _tickInterval = tickInterval;
            _healPerTick = healPerTick;
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
                HealAlliesInZone();
            }

            UpdateVisual();

            if (_lifeTimer <= 0f)
                Destroy(gameObject);
        }

        private void HealAlliesInZone()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _radius, _allyMask);
            if (hits == null || hits.Length == 0)
                return;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D c = hits[i];
                if (c == null) continue;
                if (c.isTrigger) continue;

                var stat = c.GetComponentInParent<StatMono>();
                if (stat == null)
                    continue;

                bool healed = TryHealStat(stat, _healPerTick);

                if (healed)
                    _impactVfx?.Play(stat.transform);

                if (healed && _debugLog)
                    Debug.Log($"[SanctuarySkill] Healed {stat.name} by {_healPerTick:0.##}");
            }
        }

        private static bool TryHealStat(StatMono stat, float amount)
        {
            if (stat == null) return false;

            var type = stat.GetType();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

            string[] methodNames = { "Heal", "RestoreHp", "RestoreHP", "RecoverHp", "RecoverHP", "AddHp", "AddHP" };
            for (int i = 0; i < methodNames.Length; i++)
            {
                var mFloat = type.GetMethod(methodNames[i], flags, null, new System.Type[] { typeof(float) }, null);
                if (mFloat != null)
                {
                    try
                    {
                        object result = mFloat.Invoke(stat, new object[] { amount });
                        if (result is bool b) return b;
                        return true;
                    }
                    catch { }
                }

                var mInt = type.GetMethod(methodNames[i], flags, null, new System.Type[] { typeof(int) }, null);
                if (mInt != null)
                {
                    try
                    {
                        object result = mInt.Invoke(stat, new object[] { Mathf.RoundToInt(amount) });
                        if (result is bool b) return b;
                        return true;
                    }
                    catch { }
                }
            }

            var currentHpField = type.GetField("currentHp", flags) ?? type.GetField("CurrentHp", flags);
            var maxHpField = type.GetField("maxHp", flags) ?? type.GetField("MaxHp", flags);
            if (currentHpField != null && maxHpField != null && currentHpField.FieldType == typeof(float) && maxHpField.FieldType == typeof(float))
            {
                try
                {
                    float currentHp = (float)currentHpField.GetValue(stat);
                    float maxHp = (float)maxHpField.GetValue(stat);
                    currentHp = Mathf.Min(maxHp, currentHp + amount);
                    currentHpField.SetValue(stat, currentHp);
                    return true;
                }
                catch { }
            }

            return false;
        }

        private void BuildVisual()
        {
            var ring = new GameObject("SanctuaryRing");
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.zero;

            _ringRenderer = ring.AddComponent<SpriteRenderer>();
            _ringRenderer.sprite = CreateRingSprite(96, 96);
            _ringRenderer.sortingOrder = _sortingOrder;
            _ringRenderer.color = _ringColor;
            _startAlpha = _ringColor.a;

            // Ring sprite radius in world units = (96 * 0.45) / 16 = 2.7
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
            float thickness = 2.5f;

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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.35f, 1f, 0.55f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, _radius > 0f ? _radius : 2.5f);
        }
#endif
    }
}
