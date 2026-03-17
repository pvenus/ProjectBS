using UnityEngine;

/// <summary>
/// Top-down 2D player controller with collision.
/// - Reads WASD / Arrow keys (old Input Manager axes)
/// - Moves using Rigidbody2D.MovePosition so BoxCollider2D walls/obstacles block the player
/// - Generates a simple sprite at runtime so something is visible
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerMono : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Progression")]
    [SerializeField] private int level = 1;
    [SerializeField] private int exp = 0;
    [SerializeField] private int expToNext = 100;
    [Tooltip("Extra EXP required per level up.")]
    [SerializeField] private int expGrowthPerLevel = 3;

    [Header("Visual")]
    [SerializeField] private int spriteSize = 64;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    [Header("Party")]
    [SerializeField] private int partyMemberTankCount = 1;
    [SerializeField] private int partyMemberHealCount = 1;
    [SerializeField] private int partyMemberDPSCount = 1;
    [SerializeField] private Vector3 partySpawnOffset = new Vector3(1.5f, 0f, 0f);
    private GameObject _partyMemberTankPrefab;
    private GameObject _partyMemberHealPrefab;
    private GameObject _partyMemberDPSPrefab;

    private void Start()
    {
        // Force-add required components (covers cases where RequireComponent didn't retroactively apply)
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        var cc = GetComponent<CircleCollider2D>();
        if (cc == null) cc = gameObject.AddComponent<CircleCollider2D>();

        // This runs when the component is first added or reset in the editor.
        // Make sure required components are configured sensibly.
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (cc != null)
        {
            cc.isTrigger = false;
            cc.radius = 0.45f;
        }

        // Load party member prefab from Resources and spawn it
        _partyMemberTankPrefab = Resources.Load<GameObject>("PartyMemberTank");
        _partyMemberHealPrefab = Resources.Load<GameObject>("PartyMemberHeal");
        _partyMemberDPSPrefab = Resources.Load<GameObject>("PartyMemberDPS");

        if (_partyMemberTankPrefab == null && _partyMemberHealPrefab == null && _partyMemberDPSPrefab == null)
        {
            Debug.LogWarning("PartyMemberTnak prefab not found in Resources folder.");
        }

        if (_partyMemberTankPrefab != null)
            {
                for (int i = 0; i < partyMemberTankCount; i++)
                {
                    Vector3 spawnPos = transform.position + partySpawnOffset + new Vector3(i * 0.8f, 0f, 0f);
                    var obj = Instantiate(_partyMemberTankPrefab, spawnPos, Quaternion.identity);
                    var move = obj.GetComponent<PartyMovementMono>();
                    if (move != null)
                    {
                        move.SetLeader(transform);
                    }
                }
            }

            if (_partyMemberHealPrefab != null)
            {
                for (int i = 0; i < partyMemberHealCount; i++)
                {
                    Vector3 spawnPos = transform.position + partySpawnOffset + new Vector3(i * 0.8f, 0f, 0f);
                    var obj = Instantiate(_partyMemberHealPrefab, spawnPos, Quaternion.identity);
                    var move = obj.GetComponent<PartyMovementMono>();
                    if (move != null)
                    {
                        move.SetLeader(transform);
                    }
                }
            }

            if (_partyMemberDPSPrefab != null)
            {
                for (int i = 0; i < partyMemberDPSCount; i++)
                {
                    Vector3 spawnPos = transform.position + partySpawnOffset + new Vector3(i * 0.8f, 0f, 0f);
                    var obj = Instantiate(_partyMemberDPSPrefab, spawnPos, Quaternion.identity);
                    var move = obj.GetComponent<PartyMovementMono>();
                    if (move != null)
                    {
                        move.SetLeader(transform);
                    }
                }
            }
    }

    private void OnValidate()
    {
        // Ensure required components exist in Edit Mode as well.
        if (!Application.isPlaying)
        {
            if (GetComponent<SpriteRenderer>() == null) gameObject.AddComponent<SpriteRenderer>();
            if (GetComponent<Rigidbody2D>() == null) gameObject.AddComponent<Rigidbody2D>();
            if (GetComponent<CircleCollider2D>() == null) gameObject.AddComponent<CircleCollider2D>();
        }

        // Make the generated sprite appear even in Edit Mode (so you see something without pressing Play).
        // Note: this creates a runtime texture in memory (fine for prototyping).
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && _sr.sprite == null)
        {
            _sr.sprite = CreateSprite(spriteSize, spriteSize);
        }
    }

    private void Awake()
    {
        // Force-add required components at runtime too.
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        var cc = GetComponent<CircleCollider2D>();
        if (cc == null) cc = gameObject.AddComponent<CircleCollider2D>();
        cc.isTrigger = false;

        // If no sprite is assigned in the editor, generate one so the player is visible.
        if (_sr.sprite == null)
        {
            _sr.sprite = CreateSprite(spriteSize, spriteSize);
        }

        // Rigidbody2D setup for top-down movement.
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Optional: prevent the rigidbody from being pushed by other dynamics (for now)
        // You can remove this if you later add knockback/physics.
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        transform.position = Vector3.zero;
    }

    private void Update()
    {
        // Read input in Update.
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(x, y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        _moveInput = dir;
    }

    private void FixedUpdate()
    {
        // Apply movement in physics step.
        Vector2 delta = _moveInput * (moveSpeed * Time.fixedDeltaTime);
        _rb.MovePosition(_rb.position + delta);
    }

    private static Sprite CreateSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        // Draw a simple yellow square with a dark border.
        Color fill = new Color(1f, 0.92f, 0.2f, 1f);
        Color border = new Color(0.15f, 0.12f, 0.05f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = (x == 0 || y == 0 || x == w - 1 || y == h - 1 || x == 1 || y == 1 || x == w - 2 || y == h - 2);
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        }

        tex.Apply();

        // 16 pixels per unit gives a reasonably sized character in 2D.
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        exp += amount;
        while (exp >= expToNext)
        {
            exp -= expToNext;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        expToNext += expGrowthPerLevel;

        // TODO: later hook stats upgrades here (damage, speed, etc.)
        Debug.Log($"LEVEL UP! Lv {level} (Next EXP: {expToNext})");

        UILevelUpMono.Instance?.Open();
    }

    public int GetLevel() => level;
    public int GetExp() => exp;
    public int GetExpToNext() => expToNext;
}