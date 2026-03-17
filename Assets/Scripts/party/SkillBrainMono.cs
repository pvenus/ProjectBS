using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillBrainMono
/// - Reads StateMono and nearby combat context.
/// - Evaluates BattleSkillBase skills from SkillLoadoutMono.
/// - Sends the chosen execution request to SkillExecutorMono.
///
/// Notes:
/// - Brain is responsible for decision making only.
/// - Actual skill execution is routed through SkillExecutorMono.
/// </summary>
public class SkillBrainMono : MonoBehaviour
{
    [Header("Role")]
    [SerializeField] private Role role = Role.DPS;
    [Header("Perception")]
    [Tooltip("Which layers count as enemies for decision making.")]
    [SerializeField] private LayerMask enemyMask = ~0;
    [Tooltip("How often (seconds) the brain evaluates decisions.")]
    [SerializeField] private float thinkInterval = 1.0f;
    [Tooltip("General radius to estimate enemy density.")]
    [SerializeField] private float enemyCheckRadius = 3.0f;
    [Header("Allies")]
    [Tooltip("Which layers count as allies for healer/support logic.")]
    [SerializeField] private LayerMask allyMask = ~0;
    [Header("Skill Loadout")]
    [SerializeField] private SkillLoadoutMono skillLoadout;
    [Header("Debug")]
    [SerializeField] private bool debugBrain = false;

    // Cached modules
    private StateMono _state;
    private SkillExecutorMono _executor;
    private SkillBrainOutput _lastOutput;
    private BrainContext _lastContext;
    private BrainDecisionState _lastDecisionState;

    // Timers
    private float _thinkTimer;
    private bool _skillUsedThisTick;


    private void Awake()
    {
        _state = GetComponent<StateMono>();
        _executor = GetComponent<SkillExecutorMono>();

        if (skillLoadout == null)
            skillLoadout = GetComponent<SkillLoadoutMono>();

        // Small jitter so all agents don't decide on the same frame.
        _thinkTimer = UnityEngine.Random.Range(0f, thinkInterval);
    }

    private void Update()
    {
        _thinkTimer -= Time.deltaTime;
        if (_thinkTimer > 0f) return;
        _thinkTimer = Mathf.Max(0.03f, thinkInterval);

        _skillUsedThisTick = false;
        _lastContext = BuildBrainContext();
        _lastDecisionState = BrainDecisionState.None;
        _lastOutput = SkillBrainOutput.None;

        _lastOutput = EvaluateSkillOutput(_lastContext, out _lastDecisionState);
        _skillUsedThisTick = DispatchOutput(_lastOutput);
    }

    private SkillBrainOutput EvaluateSkillOutput(BrainContext context, out BrainDecisionState decisionState)
    {
        decisionState = BrainDecisionState.None;

        BattleSkillBase[] activeSkills = GetActiveBattleSkills();
        if (activeSkills.Length == 0)
        {
            decisionState = CreateDecisionState(BrainPhase.Decide, TacticalNeed.None, "NoActiveBattleSkills", 0f, null, Vector3.zero);
            return SkillBrainOutput.None;
        }

        float bestScore = float.NegativeInfinity;
        SkillBrainOutput bestOutput = SkillBrainOutput.None;
        BrainDecisionState bestState = BrainDecisionState.None;

        for (int i = 0; i < activeSkills.Length; i++)
        {
            BattleSkillBase skill = activeSkills[i];
            if (skill == null)
                continue;

            if (!TryEvaluateGenericSkill(skill, context, out SkillBrainOutput output, out BrainDecisionState state))
            {
                if (debugBrain)
                    Debug.Log($"[SkillBrain][Skill] skip skill={skill.DisplayName} role={context.role} reason=TryEvaluateGenericSkill returned false");
                continue;
            }

            if (!output.HasSkill)
            {
                if (debugBrain)
                    Debug.Log($"[SkillBrain][Skill] skip skill={skill.DisplayName} role={context.role} reason=No output generated");
                continue;
            }

            if (debugBrain)
            {
                string targetLabel = output.targetMode switch
                {
                    SkillOutputTargetMode.Self => name,
                    SkillOutputTargetMode.Target => output.target != null ? output.target.name : "null",
                    SkillOutputTargetMode.Point => output.point.ToString(),
                    _ => "none"
                };

                Debug.Log($"[SkillBrain][Skill] candidate skill={skill.DisplayName} score={output.score:0.00} need={state.need} phase={state.phase} target={targetLabel} reason={output.reason}");
            }

            if (output.score > bestScore)
            {
                bestScore = output.score;
                bestOutput = output;
                bestState = state;
            }
        }

        if (debugBrain)
        {
            Debug.Log($"[SkillBrain][Skill] result hasSkill={bestOutput.HasSkill} bestScore={bestScore:0.00} bestLabel={bestState.label} role={context.role}");
        }
        if (bestOutput.HasSkill)
        {
            decisionState = bestState;
            return bestOutput;
        }

        decisionState = CreateDecisionState(BrainPhase.Decide, TacticalNeed.None, "NoSkillSelected", 0f, null, Vector3.zero);
        return SkillBrainOutput.None;
    }

    private bool TryEvaluateGenericSkill(BattleSkillBase skill, BrainContext context, out SkillBrainOutput output, out BrainDecisionState decisionState)
    {
        output = SkillBrainOutput.None;
        decisionState = BrainDecisionState.None;

        if (skill == null)
            return false;

        float score = CalculateGenericSkillScore(skill, context);
        if (float.IsNaN(score) || float.IsInfinity(score))
            return false;
        if (debugBrain)
        {
            Debug.Log($"[SkillBrain][Generic] base-eval skill={skill.DisplayName} category={skill.Category} targetType={skill.TargetType} tacticalNeed={skill.TacticalNeed} baseScore={score:0.00} selfHp={context.selfHp01:0.00} lowestAllyHp={context.lowestAllyHp01:0.00} enemies={context.nearbyEnemyCount}");
        }

        TacticalNeed need = ConvertTacticalNeed(skill.TacticalNeed);
        string label = $"Generic_{skill.DisplayName}";

        switch (skill.TargetType)
        {
            case BattleSkillTargetType.Self:
            {
                decisionState = CreateDecisionState(BrainPhase.Act, need, label, score, transform, transform.position);
                output = CreateSelfOutput(skill, score, $"Generic loadout selected {skill.DisplayName}");
                return true;
            }

            case BattleSkillTargetType.Enemy:
            {
                float acquireRange = Mathf.Max(0.1f, skill.Range > 0f ? skill.Range : enemyCheckRadius);
                Transform target = FindClosestEnemy(acquireRange);
                if (target == null)
                    return false;

                decisionState = CreateDecisionState(BrainPhase.Act, need, label, score, target, target.position);
                output = CreateTargetOutput(skill, target, score, $"Generic loadout selected {skill.DisplayName} (target={target.name})");
                if (debugBrain)
                    Debug.Log($"[SkillBrain][Generic] enemy-target resolved skill={skill.DisplayName} target={target.name} range={acquireRange:0.00} finalScore={score:0.00}");
                return true;
            }

            case BattleSkillTargetType.Ally:
            {
                float allyRange = Mathf.Max(0.1f, skill.Range > 0f ? skill.Range : enemyCheckRadius * 3f);
                Transform target = FindLowestHpAlly(allyRange, out float lowestHp01);
                if (target == null)
                    return false;

                float allyScore = score + (1f - lowestHp01) * 10f;
                decisionState = CreateDecisionState(BrainPhase.Act, need, label, allyScore, target, target.position);
                output = CreateTargetOutput(skill, target, allyScore, $"Generic loadout selected {skill.DisplayName} (target={target.name})");
                if (debugBrain)
                    Debug.Log($"[SkillBrain][Generic] ally-target resolved skill={skill.DisplayName} target={target.name} allyHp={lowestHp01:0.00} finalScore={allyScore:0.00}");
                return true;
            }

            case BattleSkillTargetType.Point:
            {
                Vector3 point = ResolvePointSkillPosition(skill, out float pointScoreBonus);
                float pointScore = score + pointScoreBonus;
                decisionState = CreateDecisionState(BrainPhase.Act, need, label, pointScore, null, point);
                output = CreatePointOutput(skill, point, pointScore, $"Generic loadout selected {skill.DisplayName}");
                if (debugBrain)
                    Debug.Log($"[SkillBrain][Generic] point-target resolved skill={skill.DisplayName} point={point} pointBonus={pointScoreBonus:0.00} finalScore={pointScore:0.00}");
                return true;
            }
        }

        return false;
    }

    private float CalculateGenericSkillScore(BattleSkillBase skill, BrainContext context)
    {
        if (skill == null)
            return float.NegativeInfinity;

        int roleBias = GetRoleBias(skill, context.role);
        float score = skill.EvaluateBrainScore(context, roleBias);

        SurvivalState survival = ResolveSurvivalState(context);
        AllySupportState allySupport = ResolveAllySupportState(context);
        FieldControlState fieldControl = ResolveFieldControlState(context);

        // Keep a small amount of numeric detail so the current tuning does not change too abruptly.
        float selfMissingHp = Mathf.Clamp01(1f - context.selfHp01);
        float allyMissingHp = Mathf.Clamp01(1f - context.lowestAllyHp01);

        switch (skill.TacticalNeed)
        {
            case BattleSkillTacticalNeed.SelfDefense:
                score += survival switch
                {
                    SurvivalState.Safe => 0f,
                    SurvivalState.Pressured => 16f,
                    SurvivalState.Critical => 32f,
                    _ => 0f
                };
                score += selfMissingHp * 8f;
                break;

            case BattleSkillTacticalNeed.AllySupport:
                score += allySupport switch
                {
                    AllySupportState.Stable => context.hasHealTarget ? 0f : -12f,
                    AllySupportState.Wounded => 18f,
                    AllySupportState.Emergency => 34f,
                    _ => 0f
                };
                score += allyMissingHp * 8f;
                break;

            case BattleSkillTacticalNeed.AreaControl:
                score += fieldControl switch
                {
                    FieldControlState.Sparse => 0f,
                    FieldControlState.Contested => 16f,
                    FieldControlState.Overrun => 30f,
                    _ => 0f
                };
                if (context.role == Role.Tank)
                    score += 4f;
                break;

            case BattleSkillTacticalNeed.OffensivePressure:
                score += fieldControl switch
                {
                    FieldControlState.Sparse => 4f,
                    FieldControlState.Contested => 10f,
                    FieldControlState.Overrun => 14f,
                    _ => 0f
                };
                if (context.partyState == StateMono.PartyState.Aggressive)
                    score += 4f;
                if (context.role == Role.DPS)
                    score += 4f;
                break;

            case BattleSkillTacticalNeed.Utility:
                score += 1f;
                break;
        }

        if (skill.TargetType == BattleSkillTargetType.Ally && !context.hasHealTarget)
            score -= 100f;

        if (debugBrain)
        {
            Debug.Log($"[SkillBrain][Score] skill={skill.DisplayName} survival={survival} allySupport={allySupport} fieldControl={fieldControl} finalScore={score:0.00}");
        }

        return score;
    }

    private int GetRoleBias(BattleSkillBase skill, Role currentRole)
    {
        if (skill == null)
            return 0;

        int bias = 0;

        switch (currentRole)
        {
            case Role.Tank:
                if (skill.TacticalNeed == BattleSkillTacticalNeed.AreaControl) bias += 3;
                if (skill.TacticalNeed == BattleSkillTacticalNeed.OffensivePressure) bias += 1;
                break;

            case Role.Support:
                if (skill.TacticalNeed == BattleSkillTacticalNeed.AllySupport) bias += 5;
                if (skill.TacticalNeed == BattleSkillTacticalNeed.AreaControl) bias += 2;
                break;

            case Role.DPS:
                if (skill.TacticalNeed == BattleSkillTacticalNeed.OffensivePressure) bias += 5;
                if (skill.TacticalNeed == BattleSkillTacticalNeed.AreaControl) bias += 3;
                break;
        }

        return bias;
    }

    private TacticalNeed ConvertTacticalNeed(BattleSkillTacticalNeed need)
    {
        switch (need)
        {
            case BattleSkillTacticalNeed.SelfDefense: return TacticalNeed.SelfDefense;
            case BattleSkillTacticalNeed.AllySupport: return TacticalNeed.AllySupport;
            case BattleSkillTacticalNeed.AreaControl: return TacticalNeed.AreaControl;
            case BattleSkillTacticalNeed.OffensivePressure: return TacticalNeed.OffensivePressure;
            default: return TacticalNeed.None;
        }
    }

    private Vector3 ResolvePointSkillPosition(BattleSkillBase skill, out float scoreBonus)
    {
        scoreBonus = 0f;

        if (skill == null)
            return transform.position;

        if (skill.TacticalNeed == BattleSkillTacticalNeed.AllySupport)
        {
            Vector3 point = FindBestPointForAllies(skill.Radius > 0f ? skill.Radius : skill.Range, out int allyCount);
            scoreBonus = allyCount * 3f;
            return point;
        }

        Vector3 enemyPoint = FindBestPointForEnemies(skill.Radius > 0f ? skill.Radius : skill.Range, out int enemyCount);
        scoreBonus = enemyCount * 3f;
        return enemyPoint;
    }

    private Vector3 FindBestPointForEnemies(float radius, out int bestCount)
    {
        bestCount = 0;
        float r = Mathf.Max(0.1f, radius);
        var enemies = Physics2D.OverlapCircleAll(transform.position, r * 3f, enemyMask);
        Vector3 bestPos = transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            var c = enemies[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            Vector3 pos = c.transform.position;
            int count = 0;
            var inside = Physics2D.OverlapCircleAll(pos, r, enemyMask);

            for (int j = 0; j < inside.Length; j++)
            {
                var cc = inside[j];
                if (cc == null) continue;
                if (cc.isTrigger) continue;
                if (cc.transform == transform || cc.transform.IsChildOf(transform))
                    continue;
                count++;
            }

            if (count > bestCount)
            {
                bestCount = count;
                bestPos = pos;
            }
        }

        return bestPos;
    }

    private Vector3 FindBestPointForAllies(float radius, out int bestCount)
    {
        bestCount = 0;
        float r = Mathf.Max(0.1f, radius);
        var allies = Physics2D.OverlapCircleAll(transform.position, r * 3f, allyMask);
        Vector3 bestPos = transform.position;

        for (int i = 0; i < allies.Length; i++)
        {
            var c = allies[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            Vector3 pos = c.transform.position;
            int count = 0;
            var inside = Physics2D.OverlapCircleAll(pos, r, allyMask);

            for (int j = 0; j < inside.Length; j++)
            {
                var cc = inside[j];
                if (cc == null) continue;
                if (cc.isTrigger) continue;
                if (cc.transform != transform && cc.transform.IsChildOf(transform))
                    continue;
                count++;
            }

            if (count > bestCount)
            {
                bestCount = count;
                bestPos = pos;
            }
        }

        return bestPos;
    }

    private BattleSkillBase[] GetActiveBattleSkills()
    {
        if (skillLoadout == null)
            return System.Array.Empty<BattleSkillBase>();

        ScriptableObject[] active = skillLoadout.GetActiveSkills();
        if (active == null || active.Length == 0)
            return System.Array.Empty<BattleSkillBase>();

        List<BattleSkillBase> result = new List<BattleSkillBase>(active.Length);
        for (int i = 0; i < active.Length; i++)
        {
            if (active[i] is BattleSkillBase battleSkill)
                result.Add(battleSkill);
        }

        return result.ToArray();
    }
    private Transform FindLowestHpAlly(float r, out float lowestHp01)
    {
        lowestHp01 = 1f;
        var hits = Physics2D.OverlapCircleAll(transform.position, r, allyMask);
        Transform best = null;
        float bestHp = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            Transform t = c.transform;
            if (t != transform && t.IsChildOf(transform))
                continue;

            var stat = t.GetComponentInParent<StatMono>();
            if (stat == null)
                continue;

            float hp01 = GetHpRatioFromStat(stat);
            if (hp01 >= 0.999f)
                continue;

            if (hp01 < bestHp)
            {
                bestHp = hp01;
                best = stat.transform;
            }
        }

        if (debugBrain)
            Debug.Log($"[SkillBrain][Support] FindLowestHpAlly result target={(best != null ? best.name : "null")} hp={(best != null ? bestHp.ToString("0.00") : "n/a")} searchRadius={r:0.00}");
        if (best != null)
            lowestHp01 = bestHp;

        return best;
    }

    private float GetHpRatioFromStat(StatMono stat)
    {
        if (stat == null) return 1f;

        var type = stat.GetType();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

        var hp01Prop = type.GetProperty("Hp01", flags);
        if (hp01Prop != null && hp01Prop.PropertyType == typeof(float))
        {
            try { return Mathf.Clamp01((float)hp01Prop.GetValue(stat)); } catch { }
        }

        float currentHp = 0f;
        float maxHp = 0f;

        var currentHpField = type.GetField("currentHp", flags) ?? type.GetField("CurrentHp", flags) ?? type.GetField("hp", flags);
        if (currentHpField != null && currentHpField.FieldType == typeof(float))
        {
            try { currentHp = (float)currentHpField.GetValue(stat); } catch { }
        }

        var maxHpField = type.GetField("maxHp", flags) ?? type.GetField("MaxHp", flags) ?? type.GetField("maxHP", flags);
        if (maxHpField != null && maxHpField.FieldType == typeof(float))
        {
            try { maxHp = (float)maxHpField.GetValue(stat); } catch { }
        }

        if (maxHp > 0f)
            return Mathf.Clamp01(currentHp / maxHp);

        return 1f;
    }


    private Transform FindClosestEnemy(float r)
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, r, enemyMask);
        Transform closest = null;
        float closestDistSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            // Avoid counting self.
            if (c.transform == transform || c.transform.IsChildOf(transform))
                continue;

            float distSqr = (c.transform.position - transform.position).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closest = c.transform;
            }
        }

        return closest;
    }
    private BrainContext BuildBrainContext()
    {
        return SkillBrainInputBuilder.Build(
            transform,
            role,
            _state,
            enemyMask,
            allyMask,
            enemyCheckRadius);
    }

    private SurvivalState ResolveSurvivalState(BrainContext context)
    {
        if (context.selfHp01 <= 0.25f)
            return SurvivalState.Critical;

        if (context.selfHp01 <= 0.6f || context.nearbyEnemyCount >= 4)
            return SurvivalState.Pressured;

        return SurvivalState.Safe;
    }

    private AllySupportState ResolveAllySupportState(BrainContext context)
    {
        if (!context.hasHealTarget)
            return AllySupportState.Stable;

        if (context.lowestAllyHp01 <= 0.3f)
            return AllySupportState.Emergency;

        if (context.lowestAllyHp01 <= 0.7f)
            return AllySupportState.Wounded;

        return AllySupportState.Stable;
    }

    private FieldControlState ResolveFieldControlState(BrainContext context)
    {
        if (context.nearbyEnemyCount >= 6)
            return FieldControlState.Overrun;

        if (context.nearbyEnemyCount >= 3)
            return FieldControlState.Contested;

        return FieldControlState.Sparse;
    }

    private BrainDecisionState CreateDecisionState(
        BrainPhase phase,
        TacticalNeed need,
        string label,
        float priority,
        Transform target,
        Vector3 point)
    {
        return new BrainDecisionState
        {
            phase = phase,
            need = need,
            label = label,
            priority = priority,
            target = target,
            point = point
        };
    }
    private SkillBrainOutput CreateSelfOutput(ScriptableObject skill, float score, string reason)
    {
        return new SkillBrainOutput
        {
            skill = skill,
            targetMode = SkillOutputTargetMode.Self,
            target = transform,
            point = transform.position,
            score = score,
            reason = reason
        };
    }
    private SkillBrainOutput CreateTargetOutput(ScriptableObject skill, Transform target, float score, string reason)
    {
        return new SkillBrainOutput
        {
            skill = skill,
            targetMode = SkillOutputTargetMode.Target,
            target = target,
            point = (target != null) ? target.position : Vector3.zero,
            score = score,
            reason = reason
        };
    }
    private SkillBrainOutput CreatePointOutput(ScriptableObject skill, Vector3 point, float score, string reason)
    {
        return new SkillBrainOutput
        {
            skill = skill,
            targetMode = SkillOutputTargetMode.Point,
            target = null,
            point = point,
            score = score,
            reason = reason
        };
    }
    private bool DispatchOutput(SkillBrainOutput output)
    {
        if (!output.HasSkill || _executor == null)
            return false;

        bool used = _executor.ExecuteBrainOutput(output, transform);

        if (debugBrain)
        {
            string targetLabel = output.targetMode switch
            {
                SkillOutputTargetMode.Self => name,
                SkillOutputTargetMode.Target => output.target != null ? output.target.name : "null",
                SkillOutputTargetMode.Point => output.point.ToString(),
                _ => "none"
            };

            Debug.Log($"[SkillBrain] Output dispatch used={used} skill={(output.skill != null ? output.skill.name : "null")} mode={output.targetMode} target={targetLabel} score={output.score:0.00} reason={output.reason} state={_lastDecisionState.label} phase={_lastDecisionState.phase} need={_lastDecisionState.need} priority={_lastDecisionState.priority:0.00}");
        }

        return used;
    }
}