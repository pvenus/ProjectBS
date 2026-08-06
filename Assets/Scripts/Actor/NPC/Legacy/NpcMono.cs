using Character;
using Stat;
using UnityEngine;

/// <summary>
/// Core NPC shell.
/// - Owns HP / drop / visual / knockback compatibility
/// - Delegates targeting to NpcTargeting
/// - Delegates movement/pathing to NpcPathing
/// - Delegates archetype sync to NpcMovementProfile
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(NpcTargeting))]
[RequireComponent(typeof(NpcPathing))]
public class NpcMono : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NpcTargeting targeting;
    [SerializeField] private NpcPathing pathing;
    [SerializeField] private NpcMovementProfile movementProfile;
    [SerializeField] private SkillExecutorMono skillExecutor;
    [SerializeField] private CharacterManager characterManager;

    [Header("Knockback")]
    [Tooltip("If > 0, knockback overrides pathing briefly.")]
    [SerializeField] private float knockbackDuration = 0.18f;

    [Tooltip("Velocity damping applied each FixedUpdate while knocked back (0.85 = decays fast).")]
    [Range(0.5f, 0.99f)]
    [SerializeField] private float knockbackDamping = 0.85f;

    [Tooltip("Max knockback speed clamp (world units/sec). Prevents huge impulses from flinging NPCs.")]
    [SerializeField] private float knockbackMaxSpeed = 18f;

    [Header("Collision")]
    [SerializeField] private float repathOnCollisionCooldown = 0.15f;

    [Header("Basic Attack")]
    [SerializeField] private bool useBasicAttackOnly = false;
    [SerializeField] private float basicAttackRequestInterval = 0.15f;
    [SerializeField] private float basicAttackRange = 2.5f;

    [Header("Drop")]
    [Tooltip("Optional coin prefab. If null, a simple coin GameObject will be created.")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinDropMin = 1;
    [SerializeField] private int coinDropMax = 3;
    [Range(0f, 1f)]
    [SerializeField] private float coinDropChance = 0.85f;
    [SerializeField] private float coinScatterRadius = 0.6f;

    [Header("Visual")]
    [SerializeField] private bool useSortByY = true;
    [SerializeField] private int sortingOrderOffset = 0;
    [SerializeField] private float sortingPrecision = 100f;

    private Rigidbody2D _rb;
    private CircleCollider2D _cc;
    private SpriteRenderer _bodyRenderer;

    private float _repathCollisionTimer;
    private float _knockTimer;
    private Vector2 _knockVel;
    private bool _pathingWasEnabledBeforeKnockback;
    private float _basicAttackRequestTimer;

    private void Reset()
    {
        CacheComponents();
        ConfigurePhysics();
    }

    private void OnValidate()
    {
        CacheComponents();

        
        knockbackDuration = Mathf.Max(0.01f, knockbackDuration);
        knockbackMaxSpeed = Mathf.Max(0.1f, knockbackMaxSpeed);
        repathOnCollisionCooldown = Mathf.Max(0.01f, repathOnCollisionCooldown);
        basicAttackRequestInterval = Mathf.Max(0.01f, basicAttackRequestInterval);
        basicAttackRange = Mathf.Max(0.01f, basicAttackRange);
    }

    private void Awake()
    {
        CacheComponents();

        UpdateSortingOrder();

        ConfigurePhysics();

        if (movementProfile != null)
            movementProfile.ApplyProfile();
    }

    private void Update()
    {
        _repathCollisionTimer -= Time.deltaTime;
        _basicAttackRequestTimer -= Time.deltaTime;

        if (useBasicAttackOnly && !IsDead())
            TryRequestBasicAttack();
    }

    private void LateUpdate()
    {
        UpdateSortingOrder();
    }

    private void FixedUpdate()
    {
        if (_rb == null)
            return;

        if (_knockTimer <= 0f)
            return;

        _knockTimer -= Time.fixedDeltaTime;

        Vector2 pos = _rb.position;
        Vector2 delta = _knockVel * Time.fixedDeltaTime;
        _rb.MovePosition(pos + delta);

        _knockVel *= Mathf.Clamp01(knockbackDamping);

        if (_knockTimer > 0f)
            return;

        _knockTimer = 0f;
        _knockVel = Vector2.zero;

        if (pathing != null)
        {
            pathing.enabled = _pathingWasEnabledBeforeKnockback;
            pathing.ForceRepath();
        }
    }

    private void CacheComponents()
    {
        

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _cc = GetComponent<CircleCollider2D>();
        if (_cc == null) _cc = gameObject.AddComponent<CircleCollider2D>();

        if (_bodyRenderer == null)
            _bodyRenderer = GetComponentInChildren<SpriteRenderer>();

        if (targeting == null)
            targeting = GetComponent<NpcTargeting>();
        if (pathing == null)
            pathing = GetComponent<NpcPathing>();
        if (movementProfile == null)
            movementProfile = GetComponent<NpcMovementProfile>();
        if (skillExecutor == null)
            skillExecutor = GetComponent<SkillExecutorMono>();
        if (characterManager == null)
            characterManager = GetComponent<CharacterManager>();
        if (characterManager == null)
            characterManager = GetComponentInParent<CharacterManager>();
    }

    private void ConfigurePhysics()
    {
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (_cc != null)
        {
            _cc.isTrigger = false;
            //_cc.radius = 0.42f;
        }
    }

    /// <summary>
    /// Apply an instantaneous knockback velocity that overrides pathing briefly.
    /// Existing skills can keep calling this on NpcMono.
    /// </summary>
    public void ApplyKnockback(Vector2 velocity)
    {
        float max = Mathf.Max(0.1f, knockbackMaxSpeed);
        if (velocity.sqrMagnitude > max * max)
            velocity = velocity.normalized * max;

        _knockVel = velocity;
        _knockTimer = Mathf.Max(_knockTimer, Mathf.Max(0.05f, knockbackDuration));

        if (pathing != null)
        {
            _pathingWasEnabledBeforeKnockback = pathing.enabled;
            pathing.enabled = false;
        }
    }

    // Compatibility entry points for TauntSkill / SendMessage callers.
    public void SetForcedTarget(Transform t)
    {
        targeting?.ForceTarget(t, 0.25f);
    }

    public void OnTaunted(float duration)
    {
        Transform forced = targeting != null ? targeting.GetForcedTarget() : null;
        if (targeting != null && forced != null)
            targeting.ForceTarget(forced, duration);
    }

    public void ClearForcedTarget(Transform t)
    {
        targeting?.ClearForcedTarget(t);
    }

    private void TryRequestBasicAttack()
    {
        if (_basicAttackRequestTimer > 0f)
            return;

        if (skillExecutor == null || targeting == null)
            return;

        ScriptableObject basicSkill = skillExecutor.GetBasicAttackSkill();
        if (basicSkill == null)
            return;

        Transform currentTarget = targeting.GetCurrentTarget();
        if (currentTarget == null)
            return;

        bool inRange = skillExecutor.IsInSkillRange(basicSkill, transform, currentTarget);
        if (!inRange)
            return;

        float requestRange = basicAttackRange;
        if (basicSkill is BattleSkillBase battleSkill)
            requestRange = Mathf.Max(0.01f, battleSkill.Range);

        Debug.Log($"[NpcMono] basic attack request pass skill={basicSkill.name} requestRange={requestRange:0.00} target={currentTarget.name}");

        SkillExecutionRequest req = new SkillExecutionRequest
        {
            Skill = basicSkill,
            Caster = transform,
            Target = currentTarget,
            UseTarget = true
        };

        skillExecutor.SetRequest(req);
        _basicAttackRequestTimer = basicAttackRequestInterval;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_repathCollisionTimer > 0f)
            return;

        _repathCollisionTimer = repathOnCollisionCooldown;

        if (_knockTimer > 0f)
            _knockVel *= 0.6f;

        if (pathing != null && pathing.enabled)
            pathing.ForceRepath();
    }

    public void TakeDamage(int amount)
    {
        int finalAmount = Mathf.Max(0, amount);

        var mark = GetComponent<MarkTargetSkill.MarkTargetReceiverMono>();
        if (mark != null && mark.IsMarked)
            finalAmount = Mathf.Max(1, Mathf.RoundToInt(finalAmount * mark.GetDamageMultiplier()));

        if (characterManager == null)
        {
            CacheComponents();
        }

        if (characterManager == null)
        {
            return;
        }

        characterManager.TakeDamage(finalAmount);

        if (IsDead())
        {
            DropCoins();
        }
    }

    public int GetHp()
    {
        if (characterManager == null)
        {
            CacheComponents();
        }

        if (characterManager == null)
        {
            return 0;
        }

        return Mathf.RoundToInt(
            characterManager.GetStatValue(StatType.Hp));
    }

    public int GetMaxHp()
    {
        if (characterManager == null)
        {
            CacheComponents();
        }

        if (characterManager == null)
        {
            return 1;
        }

        return Mathf.Max(
            1,
            Mathf.RoundToInt(
                characterManager.GetStatValue(StatType.MaxHp)));
    }

    private bool IsDead()
    {
        if (characterManager == null)
        {
            CacheComponents();
        }

        if (characterManager == null
            || characterManager.RuntimeData == null)
        {
            return false;
        }

        return characterManager.RuntimeData.isDead;
    }

    private void UpdateSortingOrder()
    {
        if (!useSortByY)
            return;

        if (_bodyRenderer == null)
            _bodyRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_bodyRenderer == null)
            return;

        _bodyRenderer.sortingOrder = sortingOrderOffset + Mathf.RoundToInt(-transform.position.y * sortingPrecision);
    }

    private void DropCoins()
    {
        if (Random.value > coinDropChance) return;

        int min = Mathf.Max(0, coinDropMin);
        int max = Mathf.Max(min, coinDropMax);
        int count = Random.Range(min, max + 1);

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * coinScatterRadius;
            Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0f);

            if (coinPrefab != null)
            {
                Instantiate(coinPrefab, pos, Quaternion.identity);
            }
            else
            {
                var coin = new GameObject("Coin");
                coin.transform.position = pos;
                coin.AddComponent<ProbCoin>();
            }
        }
    }
}
