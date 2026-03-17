using UnityEngine;

/// <summary>
/// Boss monster wrapper.
///
/// Movement/AI is handled by the SAME script as normal monsters (NpcMono).
/// This component only:
/// - enforces that NpcMono + SpriteRenderer exist
/// - gives the boss a distinct look (bigger, tinted, optional crown child)
///
/// How to use:
/// 1) Create an enemy GameObject (or duplicate a normal monster).
/// 2) Ensure it has NpcMono (this script requires it).
/// 3) Add NpcBossMono.
/// 4) Press Reset on the component (or just play) to apply visuals.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(NpcMono))]
[RequireComponent(typeof(SpriteRenderer))]
public class NpcBossMono : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("World scale multiplier applied on Awake.")]
    [SerializeField] private float scaleMultiplier = 1.9f;

    [Tooltip("Tint to clearly separate boss from normal monsters.")]
    [SerializeField] private Color bossTint = new Color(0.9f, 0.25f, 0.35f, 1f);

    [Tooltip("If true, generate a simple boss sprite if no sprite is assigned.")]
    [SerializeField] private bool generateSpriteIfMissing = true;

    [Tooltip("Create a small crown marker above the boss.")]
    [SerializeField] private bool addCrownMarker = true;

    [Tooltip("Sorting order added on top of existing value.")]
    [SerializeField] private int sortingOrderBoost = 3;

    [Header("(Optional) Boss Feel")]
    [Tooltip("Extra contact damage feel: just a visual hint for now (no gameplay).")]
    [SerializeField] private bool pulseSlightly = true;

    [SerializeField] private float pulseSpeed = 2.2f;
    [SerializeField] private float pulseAmount = 0.04f;

    private SpriteRenderer _sr;
    private Vector3 _baseScale;

    private void Reset()
    {
        ApplyVisuals();
    }

    private void Awake()
    {
        ApplyVisuals();
    }

    private void Update()
    {
        if (!pulseSlightly) return;
        if (_baseScale == Vector3.zero) return;

        float k = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = _baseScale * k;
    }

    private void ApplyVisuals()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();

        // Make boss larger.
        _baseScale = transform.localScale;
        if (_baseScale == Vector3.zero) _baseScale = Vector3.one;
        _baseScale *= Mathf.Max(1f, scaleMultiplier);
        transform.localScale = _baseScale;

        // Distinct color.
        _sr.color = bossTint;

        // Render slightly above normal enemies.
        _sr.sortingOrder += sortingOrderBoost;

        // If no sprite set, create a simple diamond-ish boss sprite.
        if (generateSpriteIfMissing && _sr.sprite == null)
        {
            _sr.sprite = CreateBossSprite(96, 96);
        }

        // Add a crown marker child so you can tell it's a boss even in a crowd.
        if (addCrownMarker)
        {
            EnsureCrownMarker();
        }
    }

    private void EnsureCrownMarker()
    {
        const string crownName = "BossCrown";
        Transform existing = transform.Find(crownName);
        if (existing != null) return;

        var go = new GameObject(crownName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        go.transform.localScale = Vector3.one * 0.55f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCrownSprite(64, 64);
        sr.color = new Color(1f, 0.9f, 0.2f, 1f);
        sr.sortingOrder = _sr.sortingOrder + 1;
    }

    private static Sprite CreateBossSprite(int w, int h)
    {
        // Simple diamond + outline.
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        float cx = (w - 1) * 0.5f;
        float cy = (h - 1) * 0.5f;
        float r = Mathf.Min(w, h) * 0.34f;
        float outline = 2.2f;

        Color fill = new Color(1f, 1f, 1f, 1f);
        Color outl = new Color(0f, 0f, 0f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);
                float d = dx + dy; // diamond distance

                if (d <= r)
                {
                    // outline band
                    if (d >= (r - outline))
                        tex.SetPixel(x, y, outl);
                    else
                        tex.SetPixel(x, y, fill);
                }
                else
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    private static Sprite CreateCrownSprite(int w, int h)
    {
        // Tiny crown silhouette.
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color fill = Color.white;
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }

        // Simple 3-point crown
        int baseY = Mathf.RoundToInt(h * 0.25f);
        int topY = Mathf.RoundToInt(h * 0.75f);
        int midY = Mathf.RoundToInt(h * 0.6f);

        for (int x = 10; x < w - 10; x++)
        {
            // base bar
            for (int y = baseY; y < baseY + 8; y++)
                tex.SetPixel(x, y, fill);
        }

        // spikes
        DrawTriangle(tex, new Vector2(w * 0.25f, baseY + 8), new Vector2(w * 0.17f, midY), new Vector2(w * 0.33f, topY));
        DrawTriangle(tex, new Vector2(w * 0.5f, baseY + 8), new Vector2(w * 0.42f, midY), new Vector2(w * 0.58f, topY + 4));
        DrawTriangle(tex, new Vector2(w * 0.75f, baseY + 8), new Vector2(w * 0.67f, midY), new Vector2(w * 0.83f, topY));

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.3f), 64f);
    }

    private static void DrawTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 c)
    {
        // Very small helper: fill triangle using barycentric technique.
        int minX = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
        int maxX = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
        int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
        int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)));

        minX = Mathf.Clamp(minX, 0, tex.width - 1);
        maxX = Mathf.Clamp(maxX, 0, tex.width - 1);
        minY = Mathf.Clamp(minY, 0, tex.height - 1);
        maxY = Mathf.Clamp(maxY, 0, tex.height - 1);

        float area = Edge(a, b, c);
        if (Mathf.Abs(area) < 0.0001f) return;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                float w0 = Edge(b, c, p);
                float w1 = Edge(c, a, p);
                float w2 = Edge(a, b, p);

                bool hasNeg = (w0 < 0f) || (w1 < 0f) || (w2 < 0f);
                bool hasPos = (w0 > 0f) || (w1 > 0f) || (w2 > 0f);

                if (!(hasNeg && hasPos))
                    tex.SetPixel(x, y, Color.white);
            }
        }
    }

    private static float Edge(Vector2 a, Vector2 b, Vector2 c)
    {
        return (c.x - a.x) * (b.y - a.y) - (c.y - a.y) * (b.x - a.x);
    }
}
