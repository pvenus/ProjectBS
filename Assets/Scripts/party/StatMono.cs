using UnityEngine;

/// <summary>
/// StatMono
/// Generic stat container for characters (player, party members, monsters).
/// Designed for extensibility: HP, Mana, Damage and future stats.
/// Other systems (AI, skills, UI) should read values from here.
/// </summary>
public class StatMono : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float currentHp = 100f;

    [Header("Mana")]
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float currentMana = 50f;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;

    [Header("Contact Damage (DoT)")]
    [Tooltip("Enemies in this mask will be counted for contact damage.")]
    [SerializeField] private LayerMask enemyMask = ~0;

    [Tooltip("Overlap radius used to count enemies touching this character.")]
    [SerializeField] private float contactRadius = 0.55f;

    [Tooltip("Damage applied per enemy per tick (final damage = overlapCount * damagePerEnemyPerTick).")]
    [SerializeField] private float damagePerEnemyPerTick = 1f;

    [Tooltip("Minimum time between damage ticks.")]
    [SerializeField] private float contactTickInterval = 0.35f;

    [Header("HUD (Debug)")]
    [SerializeField] private bool showHpHud = true;
    [SerializeField] private bool showDamagePopups = true;
    [SerializeField] private Vector3 hudOffset = new Vector3(0f, 0.9f, 0f);
    [SerializeField] private int hudFontSize = 48;
    [SerializeField] private float hudCharacterSize = 0.05f;
    [SerializeField] private float damagePopupLife = 0.6f;
    [SerializeField] private float damagePopupRiseSpeed = 1.2f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private float _contactTickTimer;

    // HUD
    private TextMesh _hpText;
    private float _lastDamage;

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;

    public float Hp01
    {
        get
        {
            if (maxHp <= 0f) return 0f;
            return currentHp / maxHp;
        }
    }

    public float Damage => damage;

    public bool IsDead => currentHp <= 0f;

    private void Awake()
    {
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        _contactTickTimer = Random.Range(0f, Mathf.Max(0.01f, contactTickInterval));
        EnsureHud();
        UpdateHudText();
    }

    private void Update()
    {
        TickContactDamage();
        UpdateHudText();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _lastDamage = amount;
        if (showDamagePopups)
            SpawnDamagePopup(amount);

        currentHp -= amount;
        currentHp = Mathf.Max(0, currentHp);

        if (debugLog)
            Debug.Log($"[StatMono] {name} took {amount} damage. HP={currentHp}/{maxHp}");

        if (currentHp <= 0)
        {
            OnDeath();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHp += amount;
        currentHp = Mathf.Min(maxHp, currentHp);

        if (debugLog)
            Debug.Log($"[StatMono] {name} healed {amount}. HP={currentHp}/{maxHp}");
    }

    public bool ConsumeMana(float amount)
    {
        if (currentMana < amount)
            return false;

        currentMana -= amount;

        if (debugLog)
            Debug.Log($"[StatMono] {name} used {amount} mana. Mana={currentMana}/{maxMana}");

        return true;
    }

    public void RestoreMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Min(maxMana, currentMana);
    }

    // -----------------
    // Contact Damage (DoT)
    // -----------------

    private void TickContactDamage()
    {
        if (IsDead) return;

        _contactTickTimer -= Time.deltaTime;
        if (_contactTickTimer > 0f) return;
        _contactTickTimer = Mathf.Max(0.05f, contactTickInterval);

        int overlap = CountEnemiesInRadius(contactRadius);
        if (overlap <= 0) return;

        float raw = Mathf.Max(0f, overlap * Mathf.Max(0f, damagePerEnemyPerTick));
        if (raw <= 0f) return;

        TakeDamage(raw);

        if (debugLog)
            Debug.Log($"[StatMono] ContactDoT overlap={overlap} dmg={raw:0.##} HP={currentHp:0.##}/{maxHp:0.##}");
    }

    private int CountEnemiesInRadius(float r)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, r, enemyMask);
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            // Ignore self / children
            if (c.transform == transform || c.transform.IsChildOf(transform))
                continue;

            count++;
        }

        return count;
    }

    // -----------------
    // HUD (Debug)
    // -----------------

    private void EnsureHud()
    {
        if (!showHpHud) return;
        if (_hpText != null) return;

        var go = new GameObject("StatHud");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = hudOffset;

        _hpText = go.AddComponent<TextMesh>();
        _hpText.text = "";
        _hpText.anchor = TextAnchor.MiddleCenter;
        _hpText.alignment = TextAlignment.Center;
        _hpText.fontSize = hudFontSize;
        _hpText.characterSize = hudCharacterSize;
        _hpText.color = Color.white;

        var mr = _hpText.GetComponent<MeshRenderer>();
        mr.sortingOrder = 200;
    }

    private void UpdateHudText()
    {
        if (!showHpHud) return;
        if (_hpText == null) return;

        _hpText.text = $"HP {currentHp:0}/{maxHp:0}";

        float hp01 = Hp01;
        if (hp01 < 0.25f) _hpText.color = new Color(1f, 0.35f, 0.35f, 1f);
        else if (hp01 < 0.6f) _hpText.color = new Color(1f, 0.85f, 0.35f, 1f);
        else _hpText.color = Color.white;
    }

    private void SpawnDamagePopup(float dmg)
    {
        var go = new GameObject("DmgPopup");
        go.transform.position = transform.position + hudOffset + new Vector3(Random.Range(-0.15f, 0.15f), 0f, 0f);

        var tm = go.AddComponent<TextMesh>();
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = Mathf.Max(18, hudFontSize - 10);
        tm.characterSize = Mathf.Max(0.02f, hudCharacterSize * 0.85f);
        tm.text = $"{dmg:0}";
        tm.color = new Color(1f, 0.9f, 0.2f, 1f);

        var mr = tm.GetComponent<MeshRenderer>();
        mr.sortingOrder = 210;

        var pop = go.AddComponent<DamagePopupMono>();
        pop.Initialize(damagePopupLife, damagePopupRiseSpeed);
    }

    private class DamagePopupMono : MonoBehaviour
    {
        private float _life;
        private float _rise;
        private float _t;
        private TextMesh _tm;

        public void Initialize(float life, float riseSpeed)
        {
            _life = Mathf.Max(0.05f, life);
            _rise = Mathf.Max(0.1f, riseSpeed);
            _tm = GetComponent<TextMesh>();
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / _life);

            transform.position += Vector3.up * (_rise * Time.deltaTime);

            if (_tm != null)
            {
                var c = _tm.color;
                c.a = Mathf.Lerp(1f, 0f, k);
                _tm.color = c;
            }

            if (_t >= _life)
                Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
#endif

    private void OnDeath()
    {
        if (debugLog)
            Debug.Log($"[StatMono] {name} died");

        // Future hooks:
        // - Drop items
        // - Notify AI systems
        // - Play animation
    }
}
