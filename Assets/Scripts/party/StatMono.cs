using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Stat;
using Party;

/// <summary>
/// StatMono
/// Generic stat container for characters (player, party members, monsters).
///
/// Responsibilities:
/// - HP / Mana / Damage data
/// - Damage / Heal / Mana consumption
/// - Optional contact damage tick
/// - Optional damage popup generation
///
/// Notes:
/// - Health is the single source of truth here.
/// - External systems can use either Hp or Health naming aliases.
/// </summary>
[DisallowMultipleComponent]
public class StatMono : MonoBehaviour
{

    [Header("Mana")]
    [SerializeField] private float maxMana = 50f;
    [SerializeField] private float currentMana = 50f;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;

    [Header("Contact Damage (DoT)")]
    [SerializeField] private bool enableContactDamage = false;
    [Tooltip("Enemies in this mask will be counted for contact damage.")]
    [SerializeField] private LayerMask enemyMask = ~0;

    [Tooltip("Overlap radius used to count enemies touching this character.")]
    [SerializeField] private float contactRadius = 0.55f;

    [Tooltip("Damage applied per enemy per tick (final damage = overlapCount * damagePerEnemyPerTick).")]
    [SerializeField] private float damagePerEnemyPerTick = 1f;

    [Tooltip("Minimum time between damage ticks.")]
    [SerializeField] private float contactTickInterval = 0.35f;


    [Header("Damage Popup")]
    [SerializeField] private bool showDamagePopups = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    [Header("Effect")]
    [SerializeField] private ShaderControllerMono shaderController;

    [Header("Death")]
    [SerializeField] private bool playDeathDissolveOnDeath = true;
    [SerializeField] private float deathDestroyDelay = 0.6f;

    private float _contactTickTimer;
    private bool _isDying;

    private CharacterManager _characterManager;


    public float MaxHp =>
        _characterManager != null
            ? _characterManager.GetStatValue(StatType.MaxHp)
            : 0f;

    public float CurrentHp =>
        _characterManager != null
            ? _characterManager.GetStatValue(StatType.Hp)
            : 0f;

    // Health naming aliases for external systems that use Health terminology.
    public float MaxHealth => MaxHp;
    public float CurrentHealth => CurrentHp;

    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;

    public float Hp01 => MaxHp > 0f ? CurrentHp / MaxHp : 0f;
    public float Damage => damage;
    public bool IsDead =>
        _isDying
        || (_characterManager != null
            && _characterManager.RuntimeData != null
            && CurrentHp <= 0f);

    private void Reset()
    {
        maxMana = Mathf.Max(0f, maxMana);
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        if (shaderController == null)
            shaderController = GetComponent<ShaderControllerMono>();
    }

    private void OnValidate()
    {
        maxMana = Mathf.Max(0f, maxMana);
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        contactRadius = Mathf.Max(0f, contactRadius);
        damagePerEnemyPerTick = Mathf.Max(0f, damagePerEnemyPerTick);
        contactTickInterval = Mathf.Max(0.01f, contactTickInterval);
        deathDestroyDelay = Mathf.Max(0f, deathDestroyDelay);
    }

    private void Awake()
    {
        maxMana = Mathf.Max(0f, maxMana);
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        _contactTickTimer = UnityEngine.Random.Range(0f, Mathf.Max(0.01f, contactTickInterval));

        if (shaderController == null)
            shaderController = GetComponent<ShaderControllerMono>();

        _characterManager =
            GetComponent<CharacterManager>();

        if (_characterManager == null)
            _characterManager =
                GetComponentInParent<CharacterManager>();

    }

    private void Start()
    {
    }

    private void Update()
    {
        if (enableContactDamage)
            TickContactDamage();
    }

    private void LateUpdate()
    {
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        if (amount <= 0f)
            return;

        if (showDamagePopups)
            SpawnDamagePopup(amount);

        shaderController?.PlayHitFlash();

        if (_characterManager != null)
        {
            _characterManager.TakeDamage(amount);
        }

        if (debugLog)
            Debug.Log($"[StatMono] {name} took {amount} damage. HP={CurrentHp}/{MaxHp}");

        if (CurrentHp <= 0f)
        {
            OnDeath();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

        if (amount <= 0f)
            return;

        if (_characterManager != null)
        {
            _characterManager.Heal(amount);
        }

        if (debugLog)
            Debug.Log($"[StatMono] {name} healed {amount}. HP={CurrentHp}/{MaxHp}");
    }

    public void SetMaxHealth(float value, bool fillToFull = false)
    {
        if (_characterManager == null)
            return;

        _characterManager.SetStat(
            StatType.MaxHp,
            Mathf.Max(1f, value));

        if (fillToFull)
        {
            _characterManager.SetStat(
                StatType.Hp,
                value);
        }
    }

    public void SetMaxHp(float value, bool fillToFull = false)
    {
        SetMaxHealth(value, fillToFull);
    }

    public bool ConsumeMana(float amount)
    {
        if (amount <= 0f)
            return true;

        if (currentMana < amount)
            return false;

        currentMana -= amount;

        if (debugLog)
            Debug.Log($"[StatMono] {name} used {amount} mana. Mana={currentMana}/{maxMana}");

        return true;
    }

    public void RestoreMana(float amount)
    {
        if (amount <= 0f)
            return;

        currentMana += amount;
        currentMana = Mathf.Min(maxMana, currentMana);
    }

    // -----------------
    // Contact Damage (DoT)
    // -----------------

    private void TickContactDamage()
    {
        if (!enableContactDamage)
            return;

        if (IsDead)
            return;

        _contactTickTimer -= Time.deltaTime;
        if (_contactTickTimer > 0f)
            return;

        _contactTickTimer = Mathf.Max(0.05f, contactTickInterval);

        int overlap = CountEnemiesInRadius(contactRadius);
        if (overlap <= 0)
            return;

        float raw = Mathf.Max(0f, overlap * Mathf.Max(0f, damagePerEnemyPerTick));
        if (raw <= 0f)
            return;

        TakeDamage(raw);

        if (debugLog)
            Debug.Log($"[StatMono] ContactDoT overlap={overlap} dmg={raw:0.##} HP={CurrentHp:0.##}/{MaxHp:0.##}");
    }

    private int CountEnemiesInRadius(float radius)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
        if (hits == null || hits.Length == 0)
            return 0;

        HashSet<StatMono> uniqueTargets = new HashSet<StatMono>();

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null)
                continue;

            if (c.isTrigger)
                continue;

            if (c.transform == transform || c.transform.IsChildOf(transform))
                continue;

            StatMono otherStat = c.GetComponentInParent<StatMono>();
            if (otherStat == null)
                continue;

            if (otherStat == this)
                continue;

            uniqueTargets.Add(otherStat);
        }

        return uniqueTargets.Count;
    }


    private void SpawnDamagePopup(float dmg)
    {
        if (DamagePupupManager.Instance == null)
        {
            return;
        }

        Vector3 popupPosition =
            transform.position
            + Vector3.up * 0.9f
            + new Vector3(
                UnityEngine.Random.Range(-0.15f, 0.15f),
                0f,
                0f);

        DamagePupupManager.Instance.ShowDamage(
            dmg,
            popupPosition);
    }


    private IEnumerator DestroyAfterDeathRoutine()
    {
        float delay = deathDestroyDelay;

        if (playDeathDissolveOnDeath && shaderController != null)
            delay = Mathf.Max(delay, 0.01f);

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!enableContactDamage)
            return;

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, contactRadius);
    }
#endif

    private void OnDeath()
    {
        if (_isDying)
            return;

        _isDying = true;
        enableContactDamage = false;

        if (debugLog)
            Debug.Log($"[StatMono] {name} died");

        AnimationMono animationMono = GetComponentInChildren<AnimationMono>();
        if (animationMono != null)
            animationMono.PlayDeath();

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
                colliders[i].enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (playDeathDissolveOnDeath && shaderController != null)
            shaderController.PlayDeathDissolve();

        StartCoroutine(DestroyAfterDeathRoutine());
    }
}
