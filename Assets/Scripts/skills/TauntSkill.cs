using System;
using UnityEngine;

/// <summary>
/// Taunt skill (Tank).
///
/// Goal (step-1): build the "different choices" foundation.
/// - EvaluateScore(): returns a score given the current context + personality.
/// - Execute(): applies a taunt effect to enemies in range.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
/// </summary>
[CreateAssetMenu(menuName = "BS/Skills/TauntSkill", fileName = "TauntSkill")]
public class TauntSkill : BattleSkillBase
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
    [SerializeField] private float radius = 4.0f;
    [SerializeField] private float duration = 2.0f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Scoring")]
    [Tooltip("How much ally low HP increases taunt priority.")]
    [SerializeField] private float allyLowHpWeight = 40f;

    [Tooltip("How much enemy density increases taunt priority.")]
    [SerializeField] private float enemyDensityWeight = 6f;

    [Tooltip("If tank is healthy, taunt is safer -> priority up.")]
    [SerializeField] private float selfHpWeight = 15f;

    [Tooltip("Aggressive personality prefers taunt more.")]
    [SerializeField] private float aggressionWeight = 10f;

    // Runtime state per-owner is not stored in ScriptableObject.
    // The caller should track cooldown per unit.

    public float TauntRadius => radius;
    public float Duration => duration;

    /// <summary>
    /// Skill evaluation context (keep minimal for now).
    /// You can expand this later (boss presence, hazard density, objective HP, etc.).
    /// </summary>
    [Serializable]
    public struct SkillContext
    {
        [Range(0f, 1f)] public float lowestAllyHp01; // 0 = dead, 1 = full
        [Range(0f, 1f)] public float selfHp01;
        [Min(0)] public int enemyCountNear;
    }

    /// <summary>
    /// Personality knobs for decision variance.
    /// (Start small. You can add fear, loyalty, recklessness, etc.)
    /// </summary>
    [Serializable]
    public struct Personality
    {
        [Range(0f, 1f)] public float aggression; // 0 = cautious, 1 = aggressive
    }

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    /// <summary>
    /// Returns a score for choosing Taunt right now.
    /// Caller can compare against other skills and pick max.
    /// </summary>
    public float EvaluateScore(in SkillContext ctx, in Personality p)
    {
        // Ally in danger? More taunt.
        float allyDanger = 1f - Mathf.Clamp01(ctx.lowestAllyHp01); // 0 safe -> 1 danger

        // Tank health also matters.
        float selfHealthy = Mathf.Clamp01(ctx.selfHp01); // 0 weak -> 1 healthy

        // Enemy density in the vicinity (normalize by a soft cap).
        float density01 = Mathf.Clamp01(ctx.enemyCountNear / 10f);

        float score = BasePriority;
        score += allyDanger * allyLowHpWeight;
        score += density01 * enemyDensityWeight;
        score += selfHealthy * selfHpWeight;
        score += Mathf.Clamp01(p.aggression) * aggressionWeight;

        return score;
    }

    /// <summary>
    /// Applies taunt effect to nearby enemies.
    /// 
    /// The owner should call this only when cooldown is ready.
    /// </summary>
    public void Execute(Transform owner)
    {
        if (owner == null) return;

        castVfx?.Play(owner, Vector3.up);

        float tauntRange = Mathf.Max(0.1f, Radius > 0f ? Radius : radius);
        var hits = Physics2D.OverlapCircleAll(owner.position, tauntRange, enemyMask);
        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;

            // Avoid taunting self/party by accident if masks overlap.
            if (c.transform == owner || c.transform.IsChildOf(owner))
                continue;

            // Preferred path: enemy has a TauntReceiverMono (in this file) to store state.
            var tr = c.GetComponent<TauntReceiverMono>();
            if (tr != null)
            {
                tr.Apply(owner, duration);
                impactVfx?.Play(c.transform);
                continue;
            }

            // Loose coupling: if enemy implements SetForcedTarget(Transform), it will receive.
            c.SendMessage("SetForcedTarget", owner, SendMessageOptions.DontRequireReceiver);

            // Also send duration if you want (optional receiver method).
            c.SendMessage("OnTaunted", duration, SendMessageOptions.DontRequireReceiver);
            impactVfx?.Play(c.transform);
        }
    }

    // Debug gizmo (optional)
    public void DrawGizmos(Transform owner)
    {
        if (owner == null) return;
        float tauntRange = Mathf.Max(0.1f, Radius > 0f ? Radius : radius);
        Gizmos.DrawWireSphere(owner.position, tauntRange);
    }
}

/// <summary>
/// Optional enemy-side helper.
/// If you add this component to enemies, TauntSkill will work immediately.
///
/// Later you can replace this with a more integrated enemy AI target system.
/// </summary>
public class TauntReceiverMono : MonoBehaviour
{
    private Transform _forcedTarget;
    private float _expiresAt;

    public Transform ForcedTarget => (_forcedTarget != null && Time.time < _expiresAt) ? _forcedTarget : null;

    public void Apply(Transform target, float duration)
    {
        _forcedTarget = target;
        _expiresAt = Time.time + Mathf.Max(0.05f, duration);

        // If the enemy has its own AI, it can optionally listen to these.
        SendMessage("SetForcedTarget", target, SendMessageOptions.DontRequireReceiver);
    }

    private void Update()
    {
        if (_forcedTarget == null) return;
        if (Time.time >= _expiresAt)
        {
            // Optional: tell AI to clear.
            SendMessage("ClearForcedTarget", _forcedTarget, SendMessageOptions.DontRequireReceiver);
            _forcedTarget = null;
        }
    }
}