using UnityEngine;

/// <summary>
/// GuardSkill (Tank defensive stance)
/// - Reduces incoming damage for a duration
/// - Optionally protects nearby allies (future extension)
/// - Uses score-based evaluation like TauntSkill
/// - Uses VFX ScriptableObjects for cast / impact presentation
/// </summary>
[CreateAssetMenu(fileName = "GuardSkill", menuName = "BS/Skills/GuardSkill")]
public class GuardSkill : BattleSkillBase
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
    [SerializeField] private float duration = 3f;
    [SerializeField, Range(0f, 1f)] private float damageReduction = 0.5f; // 50% reduction

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Scoring")]
    [SerializeField] private float lowSelfHpWeight = 50f;
    [SerializeField] private float enemyDensityWeight = 10f;

    public float Duration => duration;
    public float DamageReduction => damageReduction;

    [System.Serializable]
    public struct SkillContext
    {
        [Range(0f, 1f)] public float selfHp01;
        [Min(0)] public int enemyCountNear;
    }

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    public float EvaluateScore(in SkillContext ctx)
    {
        float selfDanger = 1f - Mathf.Clamp01(ctx.selfHp01);
        float density01 = Mathf.Clamp01(ctx.enemyCountNear / 10f);

        float score = BasePriority;
        score += selfDanger * lowSelfHpWeight;
        score += density01 * enemyDensityWeight;

        return score;
    }

    /// <summary>
    /// Applies guard state to owner.
    /// Owner will get a GuardReceiverMono to handle actual damage reduction.
    /// </summary>
    public void Execute(Transform owner)
    {
        if (owner == null) return;

        // Guard is a self-buff, so both cast and impact are shown on the owner.
        castVfx?.Play(owner, Vector3.up);
        impactVfx?.Play(owner);

        var receiver = owner.GetComponent<GuardReceiverMono>();
        if (receiver == null)
            receiver = owner.gameObject.AddComponent<GuardReceiverMono>();

        receiver.Apply(duration, damageReduction);
    }
}

/// <summary>
/// Runtime component that actually reduces damage.
/// Attach dynamically when GuardSkill executes.
/// </summary>
public class GuardReceiverMono : MonoBehaviour
{
    private float _expiresAt;
    private float _damageReduction;

    public float DamageMultiplier => (Time.time < _expiresAt) ? (1f - _damageReduction) : 1f;

    public void Apply(float duration, float reduction)
    {
        _damageReduction = Mathf.Clamp01(reduction);
        _expiresAt = Time.time + duration;
    }
}
