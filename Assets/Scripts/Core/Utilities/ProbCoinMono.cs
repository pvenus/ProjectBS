using UnityEngine;
using Currency;
using Character;

/// <summary>
/// Prototype coin drop.
/// - Visible via runtime-generated sprite
/// - Small bob/rotate animation
/// - Collected only by CharacterManager whose CharacterSO type is Player
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
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
    [SerializeField] private int goldAmount = 1;

    [Header("Lifetime")]
    [Tooltip("0 = infinite")]
    [SerializeField] private float lifetime = 0f;

    private SpriteRenderer _sr;
    private CircleCollider2D _collider;
    private Rigidbody2D _rb;
    private Vector3 _basePos;
    private float _t;

    private void Reset()
    {
        ResolveComponents();
    }

    private void Awake()
    {
        ResolveComponents();

        if (_sr.sprite == null)
        {
            _sr.sprite = CreateCoinSprite(spriteSize, spriteSize, pixelsPerUnit);
            _sr.sortingOrder = 3;
        }

        _basePos = transform.position;
    }

    private void ResolveComponents()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
        }

        _collider = GetComponent<CircleCollider2D>();
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<CircleCollider2D>();
        }

        _collider.isTrigger = true;
        _collider.radius = 0.45f;

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
        }

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = true;
        _rb.gravityScale = 0f;
    }

    public void SetGoldAmount(int amount)
    {
        goldAmount = Mathf.Max(0, amount);
    }

    private void Update()
    {
        _t += Time.deltaTime;

        // Bob (visual only)
        float y = Mathf.Sin(_t * bobSpeed) * bobAmplitude;
        transform.position = _basePos + new Vector3(0f, y, 0f);

        // Spin (visual only)
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Optional lifetime cleanup
        if (lifetime > 0f && _t >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryCollect(collision.collider);
    }

    private void TryCollect(Collider2D collider)
    {
        if (collider == null)
        {
            return;
        }

        CharacterManager characterManager =
            collider.GetComponentInParent<CharacterManager>();

        if (characterManager == null
            || characterManager.RuntimeData == null
            || characterManager.RuntimeData.characterSO == null)
        {
            return;
        }

        if (characterManager.RuntimeData.characterSO.CharacterType
            != CharacterType.Player)
        {
            return;
        }

        CurrencyManager.Instance.AddGold(goldAmount);
        Destroy(gameObject);
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
