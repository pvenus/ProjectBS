using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Defensive objective tower prop.
/// - Has HP / max HP
/// - Shows a world-space HP bar above the tower
/// - Can receive damage / healing
/// - Invokes destroy event when HP reaches 0
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(StatMono))]
public class TowerPropMono : MonoBehaviour
{


    [Header("Contact Damage")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private float contactRadius = 0.9f;
    [SerializeField] private float contactTickInterval = 0.35f;
    [SerializeField] private int damagePerEnemyPerTick = 1;
    [SerializeField] private bool debugContactDamage = false;

    [Header("Events")]
    [SerializeField] private UnityEvent onDead;

    private StatMono _stat;

    private float _contactTickTimer = 0f;
    private int _lastRawDamage = 0;

    private void Awake()
    {
        // Ensure collider is not trigger (projectile hit requires non-trigger)
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = false;

        _stat = GetComponent<StatMono>();
    }


    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            col = gameObject.AddComponent<BoxCollider2D>();

        col.isTrigger = false;
    }

    private void Update()
    {
        TickContactDamage();
    }

    private void TickContactDamage()
    {
        if (IsDead())
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
            Debug.Log($"[TowerContactDoT] tower={name} overlap={overlap} raw={raw} hp={GetCurrentHp()}/{GetMaxHp()}");
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

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || IsDead())
            return;

        if (_stat == null)
            return;

        bool wasAlive = !IsDead();
        _stat.TakeDamage(amount);

        if (wasAlive && IsDead())
            HandleDeath();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || IsDead())
            return;

        if (_stat == null)
            return;

        _stat.Heal(amount);
    }

    public void SetMaxHp(float value, bool fillToFull = false)
    {
        if (_stat == null)
            return;

        _stat.SetMaxHealth(value, fillToFull);
    }

    public int GetLastRawDamage()
    {
        return _lastRawDamage;
    }

    public float GetCurrentHp()
    {
        return _stat != null ? _stat.CurrentHealth : 0f;
    }

    public float GetMaxHp()
    {
        return _stat != null ? _stat.MaxHealth : 0f;
    }

    public float GetHpNormalized()
    {
        if (_stat == null || _stat.MaxHealth <= 0f)
            return 0f;

        return Mathf.Clamp01(_stat.CurrentHealth / _stat.MaxHealth);
    }

    public bool IsDead()
    {
        return _stat == null || _stat.CurrentHealth <= 0f;
    }

    private void HandleDeath()
    {
        onDead?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
}