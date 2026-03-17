using UnityEngine;

/// <summary>
/// Prototype coin drop.
/// - Visible via runtime-generated sprite
/// - Small bob/rotate animation
/// - Distance-based pickup (no collision/trigger)
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ProbCoin : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private int spriteSize = 24;
    [SerializeField] private float pixelsPerUnit = 16f;

    [Header("Motion")]
    [SerializeField] private float bobAmplitude = 0.08f;
    [SerializeField] private float bobSpeed = 3.5f;
    [SerializeField] private float spinSpeed = 180f;

    [Header("Pickup")]
    [Tooltip("If player comes within this distance, coin is collected (no collision/trigger).")]
    [SerializeField] private float pickupRadius = 3.0f;
    [Tooltip("Optional. If null, auto-find tag 'Player'.")]
    [SerializeField] private Transform player;

    [Header("Lifetime")]
    [Tooltip("0 = infinite")]
    [SerializeField] private float lifetime = 0f;

    private SpriteRenderer _sr;
    private Vector3 _basePos;
    private float _t;

    private void Reset()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        if (_sr.sprite == null)
        {
            _sr.sprite = CreateCoinSprite(spriteSize, spriteSize, pixelsPerUnit);
            _sr.sortingOrder = 3;
        }

        _basePos = transform.position;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        _t += Time.deltaTime;

        // Bob (visual only)
        float y = Mathf.Sin(_t * bobSpeed) * bobAmplitude;
        transform.position = _basePos + new Vector3(0f, y, 0f);

        // Spin (visual only)
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Distance-based pickup (no collision)
        if (player != null && pickupRadius > 0f)
        {
            float d = Vector2.Distance(transform.position, player.position);
            if (d <= pickupRadius)
            {
                var pm = player.GetComponent<PlayerMono>();
                if (pm != null)
                {
                    int expValue = 1;
                    pm.AddExp(expValue);
                }
                // TODO: later add currency/inventory increment here.
                Destroy(gameObject);
                return;
            }
        }

        // Optional lifetime cleanup
        if (lifetime > 0f && _t >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private static Sprite CreateCoinSprite(int w, int h, float ppu)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color fill = new Color(1f, 0.85f, 0.15f, 1f);
        Color border = new Color(0.35f, 0.25f, 0.05f, 1f);
        Color shine = new Color(1f, 0.95f, 0.6f, 1f);

        // Draw a simple coin circle
        float cx = (w - 1) * 0.5f;
        float cy = (h - 1) * 0.5f;
        float r = Mathf.Min(w, h) * 0.42f;

        for (int yy = 0; yy < h; yy++)
        {
            for (int xx = 0; xx < w; xx++)
            {
                float dx = xx - cx;
                float dy = yy - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                if (d > r)
                {
                    tex.SetPixel(xx, yy, new Color(0, 0, 0, 0));
                    continue;
                }

                bool isBorder = d >= r - 1.2f;
                if (isBorder)
                {
                    tex.SetPixel(xx, yy, border);
                }
                else
                {
                    bool isShine = (dx < -r * 0.15f && dy > r * 0.1f);
                    tex.SetPixel(xx, yy, isShine ? shine : fill);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
    }
}
