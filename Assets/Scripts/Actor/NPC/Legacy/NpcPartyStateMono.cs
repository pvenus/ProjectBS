using UnityEngine;
using System;

/// <summary>
/// Party member (ally) that uses a simple state machine:
/// - Follow: move toward a desired offset near the player
/// - Formation: hover/orbit near the player with small motion
/// - Wander: autonomous random movement for a short duration
///
/// Regardless of state, it auto-attacks by firing projectiles toward the nearest NPC target.
///
/// Notes:
/// - Player should be tagged as "Player" (or assign player Transform).
/// - Requires ProjectileMono class to exist in the project (defined in PlayerAttackMono.cs).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class NpcPartyState : MonoBehaviour
{
    public enum PartyState
    {
        Follow,
        Formation,
        Wander
    }

    public enum PartyRole
    {
        DPS,
        Tank
    }

    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Role")]
    [SerializeField] private PartyRole role = PartyRole.DPS;


    [Header("Tank Skill - Taunt")]
    [SerializeField] private TauntSkill tauntSkill;
    [SerializeField] private float tauntMinScoreToUse = 18f;
    [SerializeField, Range(0f, 1f)] private float personalityAggression = 0.55f;
    [SerializeField] private bool debugTaunt = false;

    [Header("Tank Skill - Guard")]
    [SerializeField] private GuardSkill guardSkill;
    [SerializeField] private float guardMinScoreToUse = 14f;
    [SerializeField] private float guardEnemyCheckRadius = 3.0f;
    [SerializeField] private bool debugGuard = false;

    [Header("Skills (Enemy)")]
    [SerializeField] private SlamSkill slamSkill;
    [SerializeField] private LayerMask slamTargetMask = ~0;
    [SerializeField] private float slamMinScoreToUse = 10f;
    [SerializeField, Range(0f, 1f)] private float slamAggression = 0.5f;

    [Header("Skill Cooldowns")]
    [SerializeField] private float slamCooldownJitter = 0.25f;

    // runtime
    private float _slamCooldownTimer;
    private float _stunnedUntil;

    [Header("Tank HP (for scoring)")]
    [SerializeField] private int maxHp = 1000;
    [SerializeField] private int hp = 1000;

    [Header("Contact Damage (DoT)")]
    [Tooltip("If enemies overlap this ally, damage is applied as: overlapCount * damagePerEnemyPerTick")] 
    [SerializeField] private int damagePerEnemyPerTick = 1;
    [Tooltip("Minimum time between damage ticks (DoT style).")]
    [SerializeField] private float contactTickInterval = 0.35f;
    [Tooltip("Overlap radius used to count enemies touching this ally.")]
    [SerializeField] private float contactRadius = 0.55f;

    [Header("HUD (Debug)")]
    [SerializeField] private bool showHpHud = true;
    [SerializeField] private bool showDamagePopups = true;
    [SerializeField] private Vector3 hudOffset = new Vector3(0f, 0.9f, 0f);
    [SerializeField] private int hudFontSize = 48;
    [SerializeField] private float hudCharacterSize = 0.05f;
    [SerializeField] private float damagePopupLife = 0.6f;
    [SerializeField] private float damagePopupRiseSpeed = 1.2f;

    [Header("State")]
    [SerializeField] private PartyState initialState = PartyState.Follow;
    [SerializeField] private float stateThinkInterval = 0.25f;

    [Header("Follow / Formation")]
    [Tooltip("Baseline offset from player (kept small). Actual spacing is controlled by orbit distances below.")]
    [SerializeField] private Vector2 baseOffset = new Vector2(0f, -0.2f);

    [SerializeField] private float followSpeed = 9f;
    [Tooltip("If we get extremely far (teleport etc.), snap back.")]
    [SerializeField] private float followSnapDistance = 10f;

    [Header("Orbit Spacing")]
    [Tooltip("Preferred orbit distance while following.")]
    [SerializeField] private float followOrbitDistance = 20.0f;
    [Tooltip("Preferred orbit distance while in formation.")]
    [SerializeField] private float formationOrbitDistance = 30.0f;
    [Tooltip("How much small bob/orbit motion to add on top of the orbit distance.")]
    [SerializeField] private float formationRadius = 0.55f;
    [SerializeField] private float formationLerp = 10f;

    [Tooltip("If farther than this, go into Follow.")]
    [SerializeField] private float farDistance = 50.0f;
    [Tooltip("If closer than this, go into Formation.")]
    [SerializeField] private float nearDistance = 30.0f;

    [Tooltip("How fast this party member's orbit angle drifts (prevents clumping).")]
    [SerializeField] private float orbitDriftSpeed = 0.35f;

    [Header("Wander")]
    [SerializeField] private float wanderSpeed = 2.6f;
    [SerializeField] private float wanderMinTime = 0.8f;
    [SerializeField] private float wanderMaxTime = 1.8f;
    [SerializeField] private float wanderChanceNearPlayer = 0.18f;

    [Header("Auto Attack")]
    [SerializeField] private float fireCooldown = 0.45f;
    [SerializeField] private float targetRange = 10f;
    [SerializeField] private LayerMask targetMask = ~0;

    [Header("Projectile")]
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileDistance = 10f;
    [SerializeField] private float projectileLifetime = 2.5f;
    [SerializeField] private int damagePerHit = 1;
    [SerializeField] private float projectileSpawnOffset = 0.6f;

    [Header("Visual")]
    [SerializeField] private int spriteSize = 52;

    private PartyState _state;
    private float _thinkTimer;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private CircleCollider2D _cc;

    private float _fireTimer;

    private float _tauntCooldownTimer;
    private float _debugHeartbeat;

    private float _guardCooldownTimer;
    private float _contactTickTimer;

    // Wander
    private Vector2 _wanderDir;
    private float _wanderTimer;

    private float _orbitPhase;

    // Stable orbit angle per member (creates separation between party members)
    private float _orbitAngle;

    // HUD
    private TextMesh _hpText;
    private int _lastRawDamage;
    private int _lastFinalDamage;

    private void Reset()
    {
        EnsureComponents();
        ConfigurePhysics();
    }

    private void OnValidate()
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null && _sr.sprite == null)
        {
            _sr.sprite = CreatePartySprite(spriteSize, spriteSize);
        }
    }

    private void Awake()
    {
        EnsureComponents();

        if (_sr.sprite == null)
        {
            _sr.sprite = CreatePartySprite(spriteSize, spriteSize);
        }

        ConfigurePhysics();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        _state = initialState;
        _thinkTimer = UnityEngine.Random.Range(0f, stateThinkInterval);

        _fireTimer = UnityEngine.Random.Range(0f, fireCooldown); // desync
        _orbitPhase = UnityEngine.Random.Range(0f, 100f);

        // Each member gets a stable orbit angle so they don't stack.
        _orbitAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        _tauntCooldownTimer = UnityEngine.Random.Range(0f, 0.25f);
        _guardCooldownTimer = UnityEngine.Random.Range(0f, 0.35f);
        _slamCooldownTimer = UnityEngine.Random.Range(0f, Mathf.Max(0.01f, slamCooldownJitter));
        _contactTickTimer = UnityEngine.Random.Range(0f, contactTickInterval);

        PickNewWander();

        EnsureHud();
        UpdateHudText();

        if (debugTaunt)
        {
            Debug.Log($"[Taunt] Awake OK name={name} role={role} tauntSkill={(tauntSkill != null)} mask={targetMask.value} radius={(tauntSkill!=null?tauntSkill.Radius:0f):0.0}");
        }

        _debugHeartbeat = 0f;
    }

    private void OnEnable()
    {
        if (debugTaunt)
            Debug.Log($"[Taunt] OnEnable name={name} enabled={enabled} activeInHierarchy={gameObject.activeInHierarchy}");
    }

    private void EnsureComponents()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();

        _cc = GetComponent<CircleCollider2D>();
        if (_cc == null) _cc = gameObject.AddComponent<CircleCollider2D>();
    }

    private void ConfigurePhysics()
    {
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _cc.isTrigger = false;
        _cc.radius = 0.4f;
    }

    private void Update()
    {
        if (debugTaunt)
        {
            _debugHeartbeat -= Time.unscaledDeltaTime;
            if (_debugHeartbeat <= 0f)
            {
                _debugHeartbeat = 1.0f;
                Debug.Log($"[Taunt] Heartbeat name={name} role={role} state={_state} cd={_tauntCooldownTimer:0.00} timeScale={Time.timeScale:0.00}");
            }
        }
        TickStateMachine();
        TickAutoAttack();
        TickSkills();
        TickContactDamage();
        UpdateHudText();

        if (IsStunned())
            return;

        TickEnemySkills();
    }
    // -----------------
    // HUD (Debug)
    // -----------------

    private void EnsureHud()
    {
        if (!showHpHud) return;
        if (_hpText != null) return;

        // Create a child TextMesh so we don't require any UI canvas.
        var go = new GameObject("HPHud");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = hudOffset;

        _hpText = go.AddComponent<TextMesh>();
        _hpText.text = "";
        _hpText.anchor = TextAnchor.MiddleCenter;
        _hpText.alignment = TextAlignment.Center;
        _hpText.fontSize = hudFontSize;
        _hpText.characterSize = hudCharacterSize;
        _hpText.color = Color.white;

        // Ensure it renders on top of sprites.
        var mr = _hpText.GetComponent<MeshRenderer>();
        mr.sortingOrder = 200;
    }

    private void UpdateHudText()
    {
        if (!showHpHud) return;
        if (_hpText == null) return;

        string extra = "";
        if (_lastRawDamage > 0)
        {
            // Example: raw 5 -> final 3 (guard)
            extra = $"  (-{_lastRawDamage}->{_lastFinalDamage})";
        }

        _hpText.text = $"HP {hp}/{maxHp}{extra}";

        // Color hint (low HP)
        float hp01 = (maxHp <= 0) ? 1f : Mathf.Clamp01((float)hp / maxHp);
        if (hp01 < 0.25f) _hpText.color = new Color(1f, 0.35f, 0.35f, 1f);
        else if (hp01 < 0.6f) _hpText.color = new Color(1f, 0.85f, 0.35f, 1f);
        else _hpText.color = Color.white;

        // If this is a tank, label it.
        if (role == PartyRole.Tank)
            _hpText.text = "TANK\n" + _hpText.text;
    }

    private void SpawnDamagePopup(int raw, int final)
    {
        if (!showDamagePopups) return;

        var go = new GameObject("DmgPopup");
        go.transform.position = transform.position + hudOffset + new Vector3(UnityEngine.Random.Range(-0.15f, 0.15f), 0f, 0f);

        var tm = go.AddComponent<TextMesh>();
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = Mathf.Max(18, hudFontSize - 10);
        tm.characterSize = Mathf.Max(0.02f, hudCharacterSize * 0.85f);

        // Show both values so you can confirm reduction.
        // Example: "5→3"
        tm.text = (raw == final) ? $"{final}" : $"{raw}→{final}";
        tm.color = (raw == final) ? new Color(1f, 0.9f, 0.2f, 1f) : new Color(0.35f, 0.85f, 1f, 1f);

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

    // -----------------
    // Skills (Tank: Taunt + Guard)
    // -----------------

    private void TickSkills()
    {
        if (debugTaunt)
        {
            Debug.Log($"[Taunt] TickSkills t={Time.unscaledTime:0.00} role={role} skill={(tauntSkill != null)} cd={_tauntCooldownTimer:0.00} pos={transform.position}");
        }
        if (role != PartyRole.Tank)
        {
            if (debugTaunt) Debug.Log("[Taunt] Skip: role is not Tank");
            return;
        }
        if (tauntSkill == null)
        {
            if (debugTaunt) Debug.Log("[Taunt] Skip: tauntSkill is null (assign TauntSkill asset in Inspector)");
            return;
        }

        _tauntCooldownTimer -= Time.deltaTime;
        if (_tauntCooldownTimer > 0f)
        {
            if (debugTaunt) Debug.Log($"[Taunt] Cooldown remaining: {_tauntCooldownTimer:0.00}");
            // Guard can tick even if taunt is on cooldown, so don't return yet.
        }
        else
        {
            // Build a minimal context for scoring.
            int enemyNear = CountEnemiesInRadius(tauntSkill.Radius);
            if (enemyNear <= 0)
            {
                if (debugTaunt) Debug.Log($"[Taunt] Skip: no enemies in radius {tauntSkill.Radius:0.0}. Check targetMask/layers.");
                // prevent spamming checks every frame when alone
                _tauntCooldownTimer = 0.25f;
            }
            else
            {
                // NOTE: We don't have ally HP system yet, so treat as safe for now.
                // Later: plug in Player HP + party members HP.
                var ctx = new TauntSkill.SkillContext
                {
                    lowestAllyHp01 = 1f,
                    selfHp01 = (maxHp <= 0) ? 1f : Mathf.Clamp01((float)hp / maxHp),
                    enemyCountNear = enemyNear
                };

                var p = new TauntSkill.Personality
                {
                    aggression = Mathf.Clamp01(personalityAggression)
                };

                float score = tauntSkill.EvaluateScore(ctx, p);
                if (debugTaunt)
                    Debug.Log($"[Taunt] score={score:0.0} min={tauntMinScoreToUse:0.0} enemyNear={enemyNear} selfHp={(maxHp<=0?1f:(float)hp/maxHp):0.00} agg={personalityAggression:0.00}");

                if (score < tauntMinScoreToUse)
                {
                    if (debugTaunt) Debug.Log("[Taunt] Skip: score below threshold");
                    _tauntCooldownTimer = 0.35f;
                }
                else
                {
                    if (debugTaunt) Debug.Log($"[Taunt] EXECUTE! radius={tauntSkill.Radius:0.0} duration={tauntSkill.Duration:0.0} cooldown={tauntSkill.Cooldown:0.0}");
                    // Execute taunt + simple VFX.
                    tauntSkill.Execute(transform);
                    SpawnTauntVfx(tauntSkill.Radius);

                    _tauntCooldownTimer = tauntSkill.Cooldown;
                }
            }
        }

        // --- Guard (defensive stance) ---
        if (guardSkill == null)
        {
            if (debugGuard) Debug.Log("[Guard] Skip: guardSkill is null (assign GuardSkill asset in Inspector)");
            return;
        }

        _guardCooldownTimer -= Time.deltaTime;
        if (_guardCooldownTimer > 0f)
        {
            if (debugGuard) Debug.Log($"[Guard] Cooldown remaining: {_guardCooldownTimer:0.00}");
            return;
        }

        int enemyNearGuard = CountEnemiesInRadius(guardEnemyCheckRadius);
        var gctx = new GuardSkill.SkillContext
        {
            selfHp01 = (maxHp <= 0) ? 1f : Mathf.Clamp01((float)hp / maxHp),
            enemyCountNear = enemyNearGuard
        };

        float gScore = guardSkill.EvaluateScore(gctx);
        if (debugGuard)
            Debug.Log($"[Guard] score={gScore:0.0} min={guardMinScoreToUse:0.0} enemyNear={enemyNearGuard} selfHp={(maxHp<=0?1f:(float)hp/maxHp):0.00}");

        if (gScore < guardMinScoreToUse)
        {
            if (debugGuard) Debug.Log("[Guard] Skip: score below threshold");
            _guardCooldownTimer = 0.35f;
            return;
        }

        if (debugGuard) Debug.Log($"[Guard] EXECUTE! duration={guardSkill.Duration:0.0} reduction={guardSkill.DamageReduction:0.00} cooldown={guardSkill.Cooldown:0.0}");
        guardSkill.Execute(gameObject);
        SpawnGuardVfx(guardSkill.Duration);

        _guardCooldownTimer = guardSkill.Cooldown;
    }

    private int CountEnemiesInRadius(float r)
    {
        // Reuse targetMask for enemies.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, r, targetMask);
        int count = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;
            // Count colliders that represent enemies. Prefer NpcMono, but allow plain colliders on enemy layer.
            if (c.GetComponentInParent<NpcMono>() == null && ((targetMask.value & (1 << c.gameObject.layer)) == 0))
                continue;
            count++;
        }
        if (debugTaunt)
            Debug.Log($"[Taunt] CountEnemiesInRadius r={r:0.0} -> {count}");
        return count;
    }

    private void SpawnTauntVfx(float r)
    {
        // Minimal VFX: a ring that expands/fades quickly.
        var go = new GameObject("TauntVFX");
        go.transform.position = transform.position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRingSprite(64, 64);
        sr.sortingOrder = 50;

        var v = go.AddComponent<TauntVfxMono>();
        v.Initialize(r);
    }

    private void TickStateMachine()
    {
        if (player == null) return;

        _thinkTimer -= Time.deltaTime;
        if (_thinkTimer <= 0f)
        {
            _thinkTimer = stateThinkInterval;
            Think();
        }

        if (_state == PartyState.Wander)
        {
            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0f)
            {
                // After wander, go back to follow/formation based on distance
                float d = Vector2.Distance(transform.position, player.position);
                _state = (d > farDistance) ? PartyState.Follow : PartyState.Formation;
            }
        }
    }

    private void Think()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        switch (_state)
        {
            case PartyState.Follow:
                if (dist <= nearDistance)
                    _state = PartyState.Formation;
                break;

            case PartyState.Formation:
                if (dist >= farDistance)
                    _state = PartyState.Follow;
                else if (UnityEngine.Random.value < (wanderChanceNearPlayer * 1.35f))
                {
                    _state = PartyState.Wander;
                    PickNewWander();
                }
                break;

            case PartyState.Wander:
                // let wander timer drive exit
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_rb == null || player == null) return;

        Vector2 pos = _rb.position;

        Vector2 desired;
        if (_state == PartyState.Follow)
        {
            // Keep some distance and separation around the player.
            _orbitAngle += orbitDriftSpeed * Time.fixedDeltaTime;
            Vector2 orbitDir = new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle));
            desired = (Vector2)player.position + baseOffset + orbitDir * followOrbitDistance;
            MoveToward(pos, desired, followSpeed);
        }
        else if (_state == PartyState.Formation)
        {
            // Orbit at a closer (but still separated) distance with small motion.
            _orbitAngle += orbitDriftSpeed * Time.fixedDeltaTime;

            _orbitPhase += Time.fixedDeltaTime;
            Vector2 orbitDir = new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle));
            Vector2 bob = new Vector2(Mathf.Cos(_orbitPhase * 2.1f), Mathf.Sin(_orbitPhase * 1.7f)) * formationRadius;

            desired = (Vector2)player.position + baseOffset + orbitDir * formationOrbitDistance + bob;

            Vector2 next = Vector2.Lerp(pos, desired, formationLerp * Time.fixedDeltaTime);
            _rb.MovePosition(next);
        }
        else // Wander
        {
            Vector2 delta = _wanderDir * (wanderSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(pos + delta);
        }

        // Snap protection if we get too far away (e.g., player teleports)
        Vector2 snapDesired = (Vector2)player.position + baseOffset + new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle)) * followOrbitDistance;
        if ((snapDesired - _rb.position).sqrMagnitude > followSnapDistance * followSnapDistance)
        {
            _rb.position = snapDesired;
        }

        if (IsStunned())
        {
            var rb2d = GetComponent<Rigidbody2D>();
            if (rb2d != null)
                rb2d.linearVelocity = Vector2.zero;
            return;
        }
    }

    private void MoveToward(Vector2 pos, Vector2 desired, float speed)
    {
        Vector2 to = desired - pos;
        float dist = to.magnitude;
        if (dist < 0.0001f) return;

        Vector2 dir = to / dist;
        Vector2 step = dir * (speed * Time.fixedDeltaTime);

        // Don't overshoot
        if (step.magnitude > dist) step = to;

        _rb.MovePosition(pos + step);
    }

    private void PickNewWander()
    {
        _wanderDir = UnityEngine.Random.insideUnitCircle;
        if (_wanderDir.sqrMagnitude < 0.0001f) _wanderDir = Vector2.up;
        _wanderDir.Normalize();

        _wanderTimer = UnityEngine.Random.Range(wanderMinTime, wanderMaxTime);
    }

    // -----------------
    // Auto attack
    // -----------------

    private void TickAutoAttack()
    {
        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;

        var target = FindNearestNpc();
        if (target == null) return;

        FireAt(target);
        _fireTimer = fireCooldown;
    }

    private NpcMono FindNearestNpc()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetRange, targetMask);
        float best = float.PositiveInfinity;
        NpcMono bestNpc = null;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            var npc = c.GetComponentInParent<NpcMono>();
            if (npc == null) continue;
            if (npc.gameObject == gameObject) continue;

            float d = ((Vector2)npc.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestNpc = npc;
            }
        }

        return bestNpc;
    }

    private void FireAt(NpcMono target)
    {
        Vector2 from = transform.position;
        Vector2 to = target.transform.position;
        Vector2 dir = (to - from);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
        dir.Normalize();

        Vector2 spawnPos = from + dir * projectileSpawnOffset;

        GameObject proj = new GameObject("PartyProjectile");
        proj.transform.position = spawnPos;

        var sr = proj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateProjectileSprite(16, 16);
        sr.sortingOrder = 6;

        var rb = proj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = proj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.22f;

        var projectile = proj.AddComponent<ProjectileMono>();
        projectile.Initialize(
            dir: dir,
            speed: projectileSpeed,
            maxDist: projectileDistance,
            lifetime: projectileLifetime,
            damage: damagePerHit
        );
    }

    // -----------------
    // Visual
    // -----------------

    private static Sprite CreatePartySprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color fill = new Color(0.35f, 1.0f, 0.55f, 1f);
        Color border = new Color(0.08f, 0.25f, 0.12f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = (x == 0 || y == 0 || x == w - 1 || y == h - 1 || x == 1 || y == 1 || x == w - 2 || y == h - 2);
                tex.SetPixel(x, y, isBorder ? border : fill);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    private static Sprite CreateProjectileSprite(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color c = new Color(0.75f, 1f, 0.75f, 1f);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, c);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, targetRange);

        if (player != null)
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif

    private class TauntVfxMono : MonoBehaviour
    {
        private float _life = 0.22f;
        private float _t;
        private float _targetRadius;
        private SpriteRenderer _sr;

        public void Initialize(float worldRadius)
        {
            _targetRadius = Mathf.Max(0.5f, worldRadius);
            _sr = GetComponent<SpriteRenderer>();
            transform.localScale = Vector3.one * 0.2f;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / _life);

            // scale from small -> target
            float s = Mathf.Lerp(0.2f, _targetRadius, k);
            transform.localScale = new Vector3(s, s, 1f);

            if (_sr != null)
            {
                var c = _sr.color;
                c.a = Mathf.Lerp(0.7f, 0f, k);
                _sr.color = c;
            }

            if (_t >= _life)
                Destroy(gameObject);
        }
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
        float thickness = 2.2f;

        Color ring = new Color(1f, 0.85f, 0.2f, 1f);

        for (int yy = 0; yy < h; yy++)
        {
            for (int xx = 0; xx < w; xx++)
            {
                float dx = xx - cx;
                float dy = yy - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                if (Mathf.Abs(d - r) <= thickness)
                    tex.SetPixel(xx, yy, ring);
                else
                    tex.SetPixel(xx, yy, new Color(0, 0, 0, 0));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    private void SpawnGuardVfx(float duration)
    {
        // Minimal VFX: a soft aura that fades out over 'duration'.
        var go = new GameObject("GuardVFX");
        go.transform.position = transform.position;
        go.transform.SetParent(transform, true);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRingSprite(64, 64);
        sr.sortingOrder = 49;
        sr.color = new Color(0.35f, 0.85f, 1f, 0.65f);

        var v = go.AddComponent<GuardVfxMono>();
        v.Initialize(duration, sr);
    }

    private class GuardVfxMono : MonoBehaviour
    {
        private float _life;
        private float _t;
        private SpriteRenderer _sr;

        public void Initialize(float life, SpriteRenderer sr)
        {
            _life = Mathf.Max(0.1f, life);
            _sr = sr;
            transform.localScale = new Vector3(2.2f, 2.2f, 1f);
        }

        private void Update()
        {
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / _life);

            if (_sr != null)
            {
                var c = _sr.color;
                c.a = Mathf.Lerp(0.65f, 0f, k);
                _sr.color = c;
            }

            if (_t >= _life)
                Destroy(gameObject);
        }
    }

    // -----------------
    // Contact Damage (DoT)
    // -----------------

    private void TickContactDamage()
    {
        if (hp <= 0) return;

        _contactTickTimer -= Time.deltaTime;
        if (_contactTickTimer > 0f) return;
        _contactTickTimer = Mathf.Max(0.05f, contactTickInterval);

        int overlap = CountEnemiesInRadius(contactRadius);
        if (overlap <= 0) return;

        int raw = Mathf.Max(0, overlap * Mathf.Max(0, damagePerEnemyPerTick));
        if (raw <= 0) return;

        int finalDamage = ApplyGuardReduction(raw);

        _lastRawDamage = raw;
        _lastFinalDamage = finalDamage;
        SpawnDamagePopup(raw, finalDamage);

        TakeDamage(finalDamage);

        if (debugTaunt || debugGuard)
            Debug.Log($"[ContactDoT] overlap={overlap} raw={raw} final={finalDamage} hp={hp}/{maxHp}");
    }

    private int ApplyGuardReduction(int dmg)
    {
        if (dmg <= 0) return 0;

        var guard = GetComponent<GuardReceiverMono>();
        float mult = (guard != null) ? guard.DamageMultiplier : 1f;
        int reduced = Mathf.CeilToInt(dmg * mult);
        return Mathf.Max(1, reduced);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        hp = Mathf.Max(0, hp - amount);
        UpdateHudText();

        if (hp <= 0)
        {
            Debug.Log($"[Party] {name} died");
            // For now just destroy the party member.
            Destroy(gameObject);
        }
    }
    // Called by SlamSkill via SendMessage
    public void OnStunned(float seconds)
    {
        _stunnedUntil = Mathf.Max(_stunnedUntil, Time.time + Mathf.Max(0.05f, seconds));
    }

    private bool IsStunned()
    {
        return Time.time < _stunnedUntil;
    }

    private void TickEnemySkills()
    {
        if (slamSkill == null) return;

        _slamCooldownTimer -= Time.deltaTime;
        if (_slamCooldownTimer > 0f) return;

        int near = CountTargetsInRadius(slamSkill.Radius);
        if (near <= 0)
        {
            _slamCooldownTimer = 0.25f;
            return;
        }

        var ctx = new SlamSkill.SkillContext
        {
            selfHp01 = 1f,           // (나중에 NPC HP 넣으면 연결)
            enemyCountNear = near
        };

        var p = new SlamSkill.Personality
        {
            aggression = Mathf.Clamp01(slamAggression)
        };

        float score = slamSkill.EvaluateScore(ctx, p);
        if (score < slamMinScoreToUse)
        {
            _slamCooldownTimer = 0.35f;
            return;
        }

        Debug.Log($"[EnemySlam] use! score={score} near={near}");
        
        slamSkill.Execute(transform);
        _slamCooldownTimer = slamSkill.Cooldown;
    }

    private int CountTargetsInRadius(float r)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, r, slamTargetMask);
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            if (c.transform == transform || c.transform.IsChildOf(transform))
                continue;

            count++;
        }

        return count;
    }
}