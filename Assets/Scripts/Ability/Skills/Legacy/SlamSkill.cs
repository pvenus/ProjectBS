using System;
using UnityEngine;

/// <summary>
/// Slam / Shockwave skill (Tank space-making tool).
///
/// - Knocks back enemies in a radius around the owner.
/// - Optional damage per enemy.
/// - Score-based evaluation so different personalities can choose it more/less.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
///
/// Loose coupling:
/// - Applies knockback via NpcMono.ApplyKnockback when available, otherwise Rigidbody2D / fallback transform movement.
/// - Sends optional messages if enemy implements them:
///   - TakeDamage(int)
///   - OnStunned(float)
/// </summary>
[CreateAssetMenu(fileName = "SlamSkill", menuName = "BS/Skills/SlamSkill")]
public class SlamSkill : BattleSkillBase
{
    [Header("Cost")]
    [SerializeField] private float manaCost = 0f;

    public float ManaCost => manaCost;

    /// <summary>
    /// IPartySkill entry point.
    /// SkillExecutorMono calls this.
    /// </summary>
    public void Execute(GameObject owner)
    {
        Execute(owner != null ? owner.transform : null);
    }
    
    [Header("Effect")]
    [SerializeField] private float radius = 3.2f;
    [SerializeField] private float knockbackForce = 8.5f;
    [SerializeField] private float upwardBias = 0.15f;
    [SerializeField] private int damage = 0;
    [SerializeField] private float stunSeconds = 0.0f;
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private bool debugLog = false;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Scoring")]
    [Tooltip("More overlap/density => slam more likely.")]
    [SerializeField] private float enemyDensityWeight = 16f;
    [Tooltip("If self HP is low, prefer slam to create space.")]
    [SerializeField] private float lowSelfHpWeight = 22f;
    [Tooltip("Aggressive personalities use slam offensively.")]
    [SerializeField] private float aggressionWeight = 8f;

    public float SlamRadius => radius;
    public float KnockbackForce => knockbackForce;

    [Serializable]
    public struct SkillContext
    {
        [Range(0f, 1f)] public float selfHp01;
        [Min(0)] public int enemyCountNear;
    }

    [Serializable]
    public struct Personality
    {
        [Range(0f, 1f)] public float aggression;
    }

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    public float EvaluateScore(in SkillContext ctx, in Personality p)
    {
        float density01 = Mathf.Clamp01(ctx.enemyCountNear / 10f);
        float selfDanger = 1f - Mathf.Clamp01(ctx.selfHp01);

        float score = BasePriority;
        score += density01 * enemyDensityWeight;
        score += selfDanger * lowSelfHpWeight;
        score += Mathf.Clamp01(p.aggression) * aggressionWeight;
        return score;
    }

    public void Execute(Transform owner)
    {
        if (owner == null) return;

        castVfx?.Play(owner, Vector3.up);

        float slamRange = Mathf.Max(0.1f, Radius > 0f ? Radius : radius);
        var hits = Physics2D.OverlapCircleAll(owner.position, slamRange, enemyMask);
        if (hits == null || hits.Length == 0) return;

        Vector2 origin = owner.position;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            // avoid self/party
            if (c.transform == owner || c.transform.IsChildOf(owner))
                continue;

            Vector2 dir = ((Vector2)c.transform.position - origin);
            if (dir.sqrMagnitude < 0.0001f)
                dir = UnityEngine.Random.insideUnitCircle;

            dir.Normalize();
            dir += Vector2.up * upwardBias;
            dir.Normalize();

            bool appliedKnockback = false;
            string bodyTypeText = "None";

            var npc = c.GetComponentInParent<NpcMono>();
            if (npc != null)
            {
                npc.ApplyKnockback(dir * knockbackForce);
                appliedKnockback = true;
                bodyTypeText = "NpcMono.ApplyKnockback";
            }
            else
            {
                var rb = c.attachedRigidbody;
                if (rb != null)
                {
                    bodyTypeText = rb.bodyType.ToString();

                    if (rb.bodyType == RigidbodyType2D.Dynamic)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                        appliedKnockback = true;
                    }
                    else
                    {
                        c.transform.position += (Vector3)(dir * (knockbackForce * 0.08f));
                        appliedKnockback = true;
                    }
                }
                else
                {
                    c.transform.position += (Vector3)(dir * (knockbackForce * 0.08f));
                    appliedKnockback = true;
                }
            }

            if (appliedKnockback)
                impactVfx?.Play(c.transform);

            if (debugLog)
                Debug.Log($"[SlamSkill] Knockback applied={appliedKnockback} dir={dir} force={knockbackForce:0.##} target={c.name} bodyType={bodyTypeText}");

            if (damage > 0)
                c.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            if (stunSeconds > 0f)
                c.SendMessage("OnStunned", stunSeconds, SendMessageOptions.DontRequireReceiver);
        }

        // VFX is now handled by the assigned VFX ScriptableObjects.
    }
}
