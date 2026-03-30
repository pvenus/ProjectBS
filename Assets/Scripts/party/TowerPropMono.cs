

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defensive objective tower prop.
/// - Has HP / max HP
/// - Shows a world-space HP bar above the tower
/// - Can receive damage / healing
/// - Invokes destroy event when HP reaches 0
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class TowerPropMono : MonoBehaviour
{

    [Header("HP")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;
    [SerializeField] private bool destroyOnDeath = false;

    [Header("Contact Damage")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float contactRadius = 0.9f;
    [SerializeField] private float contactTickInterval = 0.35f;
    [SerializeField] private int damagePerEnemyPerTick = 1;
    [SerializeField] private bool debugContactDamage = false;

    [Header("Health Bar")]
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private Vector2 healthBarSize = new Vector2(1.8f, 0.22f);
    [SerializeField] private Vector2 healthBarBackgroundSize = new Vector2(1.95f, 0.30f);
    [SerializeField] private int healthBarSortingOrder = 50;
    [SerializeField] private bool hideHealthBarWhenFull = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onDead;

    private SpriteRenderer _towerRenderer;
    private GameObject _healthBarRoot;
    private SpriteRenderer _healthBarBackgroundRenderer;
    private SpriteRenderer _healthBarFillRenderer;
    private static Sprite _sharedWhiteSprite;

    private float _contactTickTimer = 0f;
    private int _lastRawDamage = 0;

    private void Awake()
    {
        _towerRenderer = GetComponent<SpriteRenderer>();

        maxHp = Mathf.Max(1f, maxHp);
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        EnsureHealthBarObjects();
        RefreshHealthBar();
    }


    private void Reset()
    {
        maxHp = 100f;
        currentHp = maxHp;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite == null)
            sr.sprite = GetSharedWhiteSprite();

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            gameObject.AddComponent<BoxCollider2D>();
    }

    private void Update()
    {
        TickContactDamage();
    }

    private void TickContactDamage()
    {
        if (currentHp <= 0f)
            return;

        _contactTickTimer -= Time.deltaTime;
        if (_contactTickTimer > 0f)
            return;

        _contactTickTimer = Mathf.Max(0.05f, contactTickInterval);

        int overlap = CountEnemiesInRadius(contactRadius);
        if (overlap <= 0)
            return;

        int raw = Mathf.Max(0, overlap * Mathf.Max(0, damagePerEnemyPerTick));
        if (raw <= 0)
            return;

        _lastRawDamage = raw;
        TakeDamage(raw);

        if (debugContactDamage)
        {
            Debug.Log($"[TowerContactDoT] tower={name} overlap={overlap} raw={raw} hp={currentHp}/{maxHp}");
        }
    }

    private int CountEnemiesInRadius(float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);
        if (hits == null || hits.Length == 0)
            return 0;

        int count = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            if (hit.transform == transform)
                continue;

            count++;
        }

        return count;
    }

    private void LateUpdate()
    {
        if (_healthBarRoot != null)
            _healthBarRoot.transform.position = transform.position + healthBarOffset;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || currentHp <= 0f)
            return;

        currentHp = Mathf.Max(0f, currentHp - amount);
        RefreshHealthBar();

        if (currentHp <= 0f)
            HandleDeath();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHp <= 0f)
            return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
        RefreshHealthBar();
    }

    public void SetMaxHp(float value, bool fillToFull = false)
    {
        maxHp = Mathf.Max(1f, value);
        currentHp = fillToFull ? maxHp : Mathf.Clamp(currentHp, 0f, maxHp);
        RefreshHealthBar();
    }

    public int GetLastRawDamage()
    {
        return _lastRawDamage;
    }

    public float GetCurrentHp()
    {
        return currentHp;
    }

    public float GetMaxHp()
    {
        return maxHp;
    }

    public float GetHpNormalized()
    {
        if (maxHp <= 0f)
            return 0f;

        return Mathf.Clamp01(currentHp / maxHp);
    }

    public bool IsDead()
    {
        return currentHp <= 0f;
    }

    private void HandleDeath()
    {
        RefreshHealthBar();
        onDead?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    private void EnsureHealthBarObjects()
    {
        if (_healthBarRoot != null)
            return;

        _healthBarRoot = new GameObject("HPBarRoot");
        _healthBarRoot.transform.SetParent(transform, false);
        _healthBarRoot.transform.position = transform.position + healthBarOffset;

        GameObject bg = new GameObject("HPBarBackground");
        bg.transform.SetParent(_healthBarRoot.transform, false);
        _healthBarBackgroundRenderer = bg.AddComponent<SpriteRenderer>();
        _healthBarBackgroundRenderer.sprite = GetSharedWhiteSprite();
        _healthBarBackgroundRenderer.color = new Color(0f, 0f, 0f, 0.75f);
        _healthBarBackgroundRenderer.sortingOrder = healthBarSortingOrder;
        bg.transform.localScale = new Vector3(healthBarBackgroundSize.x, healthBarBackgroundSize.y, 1f);

        GameObject fill = new GameObject("HPBarFill");
        fill.transform.SetParent(_healthBarRoot.transform, false);
        _healthBarFillRenderer = fill.AddComponent<SpriteRenderer>();
        _healthBarFillRenderer.sprite = GetSharedWhiteSprite();
        _healthBarFillRenderer.color = new Color(0.15f, 0.9f, 0.25f, 1f);
        _healthBarFillRenderer.sortingOrder = healthBarSortingOrder + 1;
        fill.transform.localPosition = Vector3.zero;
    }

    private void RefreshHealthBar()
    {
        if (_healthBarRoot == null)
            EnsureHealthBarObjects();

        if (_healthBarRoot == null || _healthBarFillRenderer == null || _healthBarBackgroundRenderer == null)
            return;

        float ratio = GetHpNormalized();
        bool visible = showHealthBar && (!hideHealthBarWhenFull || ratio < 0.999f);

        _healthBarRoot.SetActive(visible);
        if (!visible)
            return;

        _healthBarBackgroundRenderer.transform.localScale = new Vector3(healthBarBackgroundSize.x, healthBarBackgroundSize.y, 1f);

        float fillWidth = Mathf.Max(0.001f, healthBarSize.x * ratio);
        _healthBarFillRenderer.transform.localScale = new Vector3(fillWidth, healthBarSize.y, 1f);
        _healthBarFillRenderer.transform.localPosition = new Vector3(-(healthBarSize.x - fillWidth) * 0.5f, 0f, 0f);

        if (ratio > 0.6f)
            _healthBarFillRenderer.color = new Color(0.15f, 0.9f, 0.25f, 1f);
        else if (ratio > 0.3f)
            _healthBarFillRenderer.color = new Color(1f, 0.8f, 0.15f, 1f);
        else
            _healthBarFillRenderer.color = new Color(1f, 0.25f, 0.25f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }

    private static Sprite GetSharedWhiteSprite()
    {
        if (_sharedWhiteSprite != null)
            return _sharedWhiteSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        _sharedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sharedWhiteSprite;
    }
}