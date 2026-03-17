using UnityEngine;

/// <summary>
/// BasicPushSkill
/// - Cone-shaped close-range melee attack.
/// - Applies damage and knockback (push) to enemies in the cone.
/// - Designed to be used as a tank basic attack or fallback melee skill.
/// - Uses VFX ScriptableObjects for cast / hit presentation.
///
/// Notes:
/// - Uses LayerMask to select targets.
/// - Damage is applied via StatMono.TakeDamage if present.
/// - Knockback prefers NpcMono.ApplyKnockback when available, otherwise falls back to Rigidbody2D / transform push.
/// </summary>
[CreateAssetMenu(menuName = "BS/Skills/Basic Push Skill", fileName = "BasicPushSkill")]
public class BasicPushSkill : BattleSkillBase
{
    [Header("Targeting")]
    [Tooltip("Targets on these layers can be hit by this skill.")]
    [SerializeField] private LayerMask enemyMask = ~0;

    [Tooltip("Cone angle in degrees (centered around caster forward direction).")]
    [Range(10f, 180f)]
    [SerializeField] private float coneAngle = 70f;

    [Tooltip("If true, uses caster.right as forward. If false, uses (target - caster) when a target is provided.")]
    [SerializeField] private bool preferCasterRightAsForward = true;

    [Header("Damage")]
    [SerializeField] private float damage = 1f;

    [Header("Knockback")]
    [Tooltip("Impulse strength applied to hit targets.")]
    [SerializeField] private float knockbackImpulse = 6.5f;

    [Tooltip("Vertical lift impulse added to knockback (0 for top-down flat push).")]
    [SerializeField] private float knockbackUpImpulse = 0f;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit hitVfx;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public float ConeAngle => coneAngle;

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    /// <summary>
    /// Execute using caster orientation (caster.right) as forward.
    /// </summary>
    public bool Execute(Transform caster)
    {
        if (caster == null) return false;
        Vector2 forward = GetForward(caster, null);
        castVfx?.Play(caster, forward);
        return DoHit(caster, forward);
    }

    /// <summary>
    /// Execute and optionally aim toward a target.
    /// If preferCasterRightAsForward is true, target is ignored.
    /// </summary>
    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null) return false;
        Vector2 forward = GetForward(caster, target);
        castVfx?.Play(caster, forward);
        return DoHit(caster, forward);
    }

    private Vector2 GetForward(Transform caster, Transform target)
    {
        if (preferCasterRightAsForward)
        {
            Vector2 f = caster.right;
            if (f.sqrMagnitude < 0.0001f) f = Vector2.right;
            return f.normalized;
        }

        if (target != null)
        {
            Vector2 dir = (Vector2)target.position - (Vector2)caster.position;
            if (dir.sqrMagnitude < 0.0001f) dir = caster.right;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            return dir.normalized;
        }

        // Fallback
        Vector2 fb = caster.right;
        if (fb.sqrMagnitude < 0.0001f) fb = Vector2.right;
        return fb.normalized;
    }

    private bool DoHit(Transform caster, Vector2 forward)
    {
        Vector2 origin = caster.position;

        // Broad-phase: circle
        float hitRange = Mathf.Max(0.05f, Range);
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, hitRange, enemyMask);
        if (hits == null || hits.Length == 0)
        {
            if (debugLog) Debug.Log($"[BasicPushSkill] No targets in range (r={Range:0.##})");
            return false;
        }

        float half = coneAngle * 0.5f;
        int hitCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            // Ignore self / children
            if (c.transform == caster || c.transform.IsChildOf(caster))
                continue;

            // Direction to candidate
            Vector2 to = (Vector2)c.transform.position - origin;
            float dist = to.magnitude;
            if (dist <= 0.0001f) continue;
            if (dist > Range) continue;

            Vector2 dir = to / dist;
            float ang = Vector2.Angle(forward, dir);
            if (ang > half)
                continue;

            // Apply damage (supports both StatMono and legacy NpcMono)
            bool didDamage = false;

            var stat = c.GetComponentInParent<StatMono>();
            if (stat != null)
            {
                stat.TakeDamage(damage);   // StatMono는 float 데미지
                didDamage = true;
            }
            else
            {
                var npcDamage = c.GetComponentInParent<NpcMono>();
                if (npcDamage != null)
                {
                    int dmgInt = Mathf.Max(1, Mathf.RoundToInt(damage)); // NpcMono는 int 데미지
                    npcDamage.TakeDamage(dmgInt);
                    didDamage = true;
                }
            }

            if (didDamage)
                hitVfx?.Play(c.transform);

            if (debugLog && didDamage)
                Debug.Log($"[BasicPushSkill] Damage {damage:0.##} to {c.name}");

            // Apply knockback
            Vector2 impulse = dir * knockbackImpulse + Vector2.up * knockbackUpImpulse;
            bool appliedKnockback = false;
            string bodyTypeText = "None";

            var npc = c.GetComponentInParent<NpcMono>();
            if (npc != null)
            {
                npc.ApplyKnockback(impulse);
                appliedKnockback = true;
                bodyTypeText = "NpcMono.ApplyKnockback";
            }
            else
            {
                var rb = c.GetComponentInParent<Rigidbody2D>();
                if (rb != null)
                {
                    bodyTypeText = rb.bodyType.ToString();

                    if (rb.bodyType == RigidbodyType2D.Dynamic)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                        rb.AddForce(impulse, ForceMode2D.Impulse);
                        appliedKnockback = true;
                    }
                    else
                    {
                        c.transform.position += (Vector3)(impulse * 0.08f);
                        appliedKnockback = true;
                    }
                }
                else
                {
                    c.transform.position += (Vector3)(impulse * 0.08f);
                    appliedKnockback = true;
                }
            }

            if (debugLog && appliedKnockback)
                Debug.Log($"[BasicPushSkill] Knockback applied {impulse} to {c.name} bodyType={bodyTypeText}");

            hitCount++;
        }

        if (debugLog) Debug.Log($"[BasicPushSkill] Hit {hitCount} targets (range={Range:0.##}, cone={coneAngle:0.#})");
        return hitCount > 0;
    }
}