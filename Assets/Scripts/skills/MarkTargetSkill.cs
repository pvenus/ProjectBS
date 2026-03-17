using UnityEngine;

/// <summary>
/// MarkTargetSkill
/// - Marks a single enemy target for a short duration.
/// - The marked target takes bonus damage from subsequent hits.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
/// - Compatible with SkillBrainMono reflection-based Execute(Transform, Transform).
/// </summary>
[CreateAssetMenu(fileName = "MarkTargetSkill", menuName = "BS/Skills/Mark Target Skill")]
public class MarkTargetSkill : BattleSkillBase
{
    [Header("Mark")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private float bonusDamageMultiplier = 1.35f;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public new BattleSkillCategory Category => BattleSkillCategory.Control;
    public new BattleSkillTargetType TargetType => BattleSkillTargetType.Enemy;
    public new BattleSkillTacticalNeed TacticalNeed => BattleSkillTacticalNeed.OffensivePressure;

    public float Duration => duration;
    public float BonusDamageMultiplier => bonusDamageMultiplier;

    public float EvaluateBrainScore(BrainContext context, int roleBias = 0)
    {
        float score = 22f + roleBias;

        score += context.nearbyEnemyCount * 2.5f;

        if (context.role == Role.DPS)
            score += 10f;

        if (context.partyState == StateMono.PartyState.Aggressive)
            score += 8f;

        if (context.selfHp01 < 0.35f)
            score -= 8f;

        return score;
    }

    public bool Execute(Transform caster)
    {
        if (caster == null)
            return false;

        Transform target = FindBestTarget(caster);
        if (target == null)
        {
            if (debugLog)
                Debug.Log($"[MarkTargetSkill] Execute failed: no target found for caster={caster.name}");
            return false;
        }

        return Execute(caster, target);
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null || target == null)
            return false;

        Vector3 dir = target.position - caster.position;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.up;

        castVfx?.Play(caster, dir.normalized);

        var marker = target.GetComponentInParent<MarkTargetReceiverMono>();
        if (marker == null)
            marker = target.gameObject.AddComponent<MarkTargetReceiverMono>();

        marker.ApplyMark(Mathf.Max(0.1f, duration), Mathf.Max(1f, bonusDamageMultiplier), debugLog);
        impactVfx?.Play(target);

        if (debugLog)
            Debug.Log($"[MarkTargetSkill] Mark applied target={target.name} duration={duration:0.##} bonusX={bonusDamageMultiplier:0.##}");

        return true;
    }

    private Transform FindBestTarget(Transform caster)
    {
        if (caster == null)
            return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.position, Radius);
        if (hits == null || hits.Length == 0)
            return null;

        Transform bestTarget = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.isTrigger)
                continue;

            Transform t = hit.transform;
            if (t == null)
                continue;

            if (t.root == caster.root)
                continue;

            var npc = hit.GetComponentInParent<NpcMono>();
            var stat = hit.GetComponentInParent<StatMono>();

            if (npc == null && stat == null)
                continue;

            float dist = (t.position - caster.position).sqrMagnitude;
            if (dist < bestDist)
            {
                bestDist = dist;
                bestTarget = stat != null ? stat.transform : npc.transform;
            }
        }

        if (debugLog)
        {
            string name = bestTarget != null ? bestTarget.name : "null";
            Debug.Log($"[MarkTargetSkill] FindBestTarget caster={caster.name} radius={Radius:0.##} result={name}");
        }

        return bestTarget;
    }

    /// <summary>
    /// Attach this to a target to make it receive bonus damage while marked.
    /// Other skills / projectiles can optionally query this component via GetDamageMultiplier().
    /// </summary>
    public class MarkTargetReceiverMono : MonoBehaviour
    {
        private float _timer;
        private float _multiplier = 1f;
        private bool _debugLog;

        public bool IsMarked => _timer > 0f;
        public float GetDamageMultiplier() => IsMarked ? _multiplier : 1f;

        public void ApplyMark(float duration, float multiplier, bool debugLog)
        {
            _timer = Mathf.Max(_timer, Mathf.Max(0.05f, duration));
            _multiplier = Mathf.Max(1f, multiplier);
            _debugLog = debugLog;

            if (_debugLog)
                Debug.Log($"[MarkTargetSkill] Receiver ApplyMark name={name} duration={_timer:0.##} mult={_multiplier:0.##}");
        }

        private void Update()
        {
            if (_timer <= 0f)
                return;

            _timer -= Time.deltaTime;
        }
    }
}
