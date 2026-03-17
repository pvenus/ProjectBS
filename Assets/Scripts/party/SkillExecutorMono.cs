using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class SkillExecutorMono : MonoBehaviour
{

    [SerializeField] private bool debugLog = false;

    [Header("Fallback")]
    [SerializeField] private bool enableBasicAttackFallback = true;
    [Header("Skill Loadout")]
    [SerializeField] private SkillLoadoutMono skillLoadout;

    private bool _hasPendingRequest;
    private SkillExecutionRequest _pendingRequest;
    private readonly Dictionary<ScriptableObject, float> _cooldowns = new Dictionary<ScriptableObject, float>();
    private void Awake()
    {
        if (skillLoadout == null)
            skillLoadout = GetComponent<SkillLoadoutMono>();
    }

    public bool ExecuteBrainOutput(SkillBrainOutput output, Transform caster)
    {
        if (!output.HasSkill || caster == null)
            return false;

        SkillExecutionRequest req = new SkillExecutionRequest
        {
            Skill = output.skill,
            Caster = caster
        };

        switch (output.targetMode)
        {
            case SkillOutputTargetMode.Target:
                if (output.target == null)
                    return false;

                req.Target = output.target;
                req.UseTarget = true;
                break;

            case SkillOutputTargetMode.Point:
                req.TargetPoint = output.point;
                req.UsePoint = true;
                break;

            case SkillOutputTargetMode.Self:
            default:
                break;
        }

        if (debugLog)
        {
            string targetLabel = output.targetMode switch
            {
                SkillOutputTargetMode.Target => output.target != null ? output.target.name : "null",
                SkillOutputTargetMode.Point => output.point.ToString(),
                SkillOutputTargetMode.Self => caster.name,
                _ => "none"
            };

            Debug.Log($"[SkillExecutor] brain output received skill={output.skill.name} mode={output.targetMode} target={targetLabel} score={output.score:0.00} reason={output.reason}");
        }

        return SetRequest(req);
    }

    public bool SetRequest(SkillExecutionRequest req)
    {
        _pendingRequest = req;
        _hasPendingRequest = (req.Skill != null && req.Caster != null);

        if (debugLog && _hasPendingRequest)
            Debug.Log($"[SkillExecutor] request set skill={req.Skill.name} caster={req.Caster.name}");

        if (!_hasPendingRequest)
            return false;

        return TryExecutePending();
    }

    public void ClearRequest()
    {
        if (debugLog && _hasPendingRequest)
            Debug.Log($"[SkillExecutor] request cleared skill={_pendingRequest.Skill?.name}");

        _hasPendingRequest = false;
        _pendingRequest = default;
    }

    public bool HasPendingRequest => _hasPendingRequest;

    public bool TryExecutePending()
    {
        if (!_hasPendingRequest)
            return false;

        var req = _pendingRequest;
        if (req.Skill == null || req.Caster == null)
        {
            _hasPendingRequest = false;
            return false;
        }

        if (IsOnCooldown(req.Skill))
        {
            if (debugLog)
                Debug.Log($"[SkillExecutor] cooldown block skill={req.Skill.name} remain={GetRemainingCooldown(req.Skill):0.##}");

            return TryExecuteBasicFallback(req, "cooldown");
        }

        bool used = false;

        if (req.UseTarget && req.Target != null)
        {
            used = InvokeExecute(req.Skill, req.Caster, req.Target);
            if (debugLog)
                Debug.Log($"[SkillExecutor] target execute skill={req.Skill.name} used={used} caster={req.Caster.name} target={req.Target.name}");
        }
        else if (req.UsePoint)
        {
            Transform temp = CreateTempTarget(req.TargetPoint);
            used = InvokeExecute(req.Skill, req.Caster, temp);

            if (debugLog)
                Debug.Log($"[SkillExecutor] point execute skill={req.Skill.name} used={used} caster={req.Caster.name} point={req.TargetPoint}");
        }
        else
        {
            used = InvokeExecute(req.Skill, req.Caster, null);
            if (debugLog)
                Debug.Log($"[SkillExecutor] self execute skill={req.Skill.name} used={used} caster={req.Caster.name}");
        }

        if (used)
        {
            float cooldown = GetCooldownFromSkill(req.Skill);
            if (cooldown > 0f)
                _cooldowns[req.Skill] = cooldown;

            return true;
        }

        return TryExecuteBasicFallback(req, "execute-fail");
    }

    private bool TryExecuteBasicFallback(SkillExecutionRequest failedRequest, string reason)
    {
        if (!enableBasicAttackFallback || failedRequest.Caster == null)
            return false;

        ScriptableObject basicAttackSkill = GetBasicAttackSkill();
        if (basicAttackSkill == null)
            return false;

        if (basicAttackSkill == failedRequest.Skill)
            return false;

        if (IsOnCooldown(basicAttackSkill))
        {
            if (debugLog)
                Debug.Log($"[SkillExecutor] basic fallback blocked by cooldown skill={basicAttackSkill.name} remain={GetRemainingCooldown(basicAttackSkill):0.##}");
            return false;
        }

        bool used;
        if (failedRequest.UseTarget && failedRequest.Target != null)
        {
            used = InvokeExecute(basicAttackSkill, failedRequest.Caster, failedRequest.Target);
            if (debugLog)
                Debug.Log($"[SkillExecutor] basic fallback target reason={reason} skill={basicAttackSkill.name} used={used} caster={failedRequest.Caster.name} target={failedRequest.Target.name}");
        }
        else
        {
            used = InvokeExecute(basicAttackSkill, failedRequest.Caster, null);
            if (debugLog)
                Debug.Log($"[SkillExecutor] basic fallback self reason={reason} skill={basicAttackSkill.name} used={used} caster={failedRequest.Caster.name}");
        }

        if (!used)
            return false;

        float cooldown = GetCooldownFromSkill(basicAttackSkill);
        if (cooldown > 0f)
            _cooldowns[basicAttackSkill] = cooldown;

        return true;
    }
    public ScriptableObject GetBasicAttackSkill()
    {
        return skillLoadout != null ? skillLoadout.GetBasicAttack() : null;
    }

    private void Update()
    {
        UpdateCooldowns();

        if (!_hasPendingRequest)
            return;

        TryExecutePending();
    }

    private void UpdateCooldowns()
    {
        if (_cooldowns.Count == 0)
            return;

        List<ScriptableObject> keys = new List<ScriptableObject>(_cooldowns.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            ScriptableObject key = keys[i];
            _cooldowns[key] -= Time.deltaTime;

            if (_cooldowns[key] <= 0f)
                _cooldowns.Remove(key);
        }
    }

    private bool IsOnCooldown(ScriptableObject skill)
    {
        if (skill == null)
            return false;

        return _cooldowns.TryGetValue(skill, out float remain) && remain > 0f;
    }

    private float GetRemainingCooldown(ScriptableObject skill)
    {
        if (skill == null)
            return 0f;

        return _cooldowns.TryGetValue(skill, out float remain) ? Mathf.Max(0f, remain) : 0f;
    }

    private float GetCooldownFromSkill(ScriptableObject skill)
    {
        if (skill == null)
            return 0f;

        var t = skill.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        string[] propNames = { "Cooldown", "cooldown", "Cd", "cd" };
        for (int i = 0; i < propNames.Length; i++)
        {
            var p = t.GetProperty(propNames[i], flags);
            if (p != null && p.CanRead)
            {
                object v = p.GetValue(skill);
                if (v is float f) return Mathf.Max(0f, f);
                if (v is int ii) return Mathf.Max(0f, ii);
            }
        }

        string[] fieldNames = { "cooldown", "Cooldown", "cd", "Cd" };
        for (int i = 0; i < fieldNames.Length; i++)
        {
            var f = t.GetField(fieldNames[i], flags);
            if (f != null)
            {
                object v = f.GetValue(skill);
                if (v is float ff) return Mathf.Max(0f, ff);
                if (v is int ii) return Mathf.Max(0f, ii);
            }
        }

        return 0f;
    }

    private bool InvokeExecute(ScriptableObject skill, Transform caster, Transform target)
    {
        var t = skill.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var methods = t.GetMethods(flags);

        MethodInfo m2 = null;
        MethodInfo m1 = null;

        for (int i = 0; i < methods.Length; i++)
        {
            var m = methods[i];
            if (m.Name != "Execute")
                continue;

            var ps = m.GetParameters();

            if (ps.Length == 2 &&
                ps[0].ParameterType == typeof(Transform) &&
                ps[1].ParameterType == typeof(Transform))
            {
                m2 = m;
                break;
            }

            if (ps.Length == 1 &&
                ps[0].ParameterType == typeof(Transform))
            {
                m1 = m;
            }
        }

        if (m2 != null && target != null)
        {
            try
            {
                object r = m2.Invoke(skill, new object[] { caster, target });
                return r is bool b ? b : true;
            }
            catch
            {
                return false;
            }
        }

        if (m1 != null)
        {
            try
            {
                object r = m1.Invoke(skill, new object[] { caster });
                return r is bool b ? b : true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    private Transform CreateTempTarget(Vector3 worldPos)
    {
        var go = new GameObject("SkillExecutionPoint");
        go.transform.position = worldPos;
        Destroy(go, 0.05f);
        return go.transform;
    }
}