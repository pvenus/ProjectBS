using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class SkillExecutorMono : MonoBehaviour, ISkillExecutor
{


    [SerializeField] private bool debugLog = false;

    [Header("Range Check")]
    [SerializeField] private bool useClosestPointForRangeCheck = true;
    [SerializeField] private float skillRangeCheckInset = 0.5f;

    [Header("Fallback")]
    [SerializeField] private bool enableBasicAttackFallback = true;
    [Header("Skill Loadout")]
    [SerializeField] private SkillLoadoutMono skillLoadout;

    private bool _hasPendingRequest;
    private SkillExecutionRequest _pendingRequest;
    private readonly Dictionary<ScriptableObject, float> _cooldowns = new Dictionary<ScriptableObject, float>();
    private venus.eldawn.party.AnimationMono _animationMono;
    private EquipmentSkillResolver _equipmentSkillResolver;
    private ProjectileFactory _projectileFactory;
    private void Awake()
    {
        if (skillLoadout == null)
            skillLoadout = GetComponent<SkillLoadoutMono>();

        _animationMono = GetComponentInChildren<venus.eldawn.party.AnimationMono>();
        _equipmentSkillResolver = new EquipmentSkillResolver();
        _projectileFactory = new ProjectileFactory();
    }

	public bool Execute(SkillBrainOutput output, Transform caster)
	{
		if (output.skill == null)
		{
			Debug.LogWarning("[SkillExecutorMonoExecutor] Output skill is null.");
			return false;
		}

		ExecuteBrainOutput(output, caster);
		return true;
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

        if (TryExecuteSkillRequest(req, req.Skill, failedReason: null))
            return true;

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

        return TryExecuteSkillRequest(failedRequest, basicAttackSkill, reason);
    }
    private bool TryExecuteSkillRequest(SkillExecutionRequest request, ScriptableObject skill, string failedReason)
    {
        if (request.Caster == null || skill == null)
            return false;

        bool used = false;

        if (request.UseTarget && request.Target != null)
        {
            if (!IsInSkillRange(skill, request.Caster, request.Target))
            {
                if (debugLog)
                {
                    string prefix = string.IsNullOrEmpty(failedReason) ? string.Empty : $" fallback reason={failedReason}";
                    Debug.Log($"[SkillExecutor] range block{prefix} skill={skill.name} caster={request.Caster.name} target={request.Target.name}");
                }
                return false;
            }

            used = ExecuteEquipmentSkill(request, skill, request.Target, false);

            if (debugLog)
            {
                string modeLabel = string.IsNullOrEmpty(failedReason) ? "target execute" : $"basic fallback target reason={failedReason}";
                Debug.Log($"[SkillExecutor] {modeLabel} skill={skill.name} used={used} caster={request.Caster.name} target={request.Target.name}");
            }
        }
        else if (request.UsePoint)
        {
            used = ExecuteEquipmentSkill(request, skill, null, true);

            if (debugLog)
            {
                string modeLabel = string.IsNullOrEmpty(failedReason) ? "point execute" : $"basic fallback point reason={failedReason}";
                Debug.Log($"[SkillExecutor] {modeLabel} skill={skill.name} used={used} caster={request.Caster.name} point={request.TargetPoint}");
            }
        }
        else
        {
            used = ExecuteEquipmentSkill(request, skill, null, false);

            if (debugLog)
            {
                string modeLabel = string.IsNullOrEmpty(failedReason) ? "self execute" : $"basic fallback self reason={failedReason}";
                Debug.Log($"[SkillExecutor] {modeLabel} skill={skill.name} used={used} caster={request.Caster.name}");
            }
        }

        if (!used)
            return false;

        TryPlayBasicAttackAnimation(skill);

        float cooldown = GetResolvedCooldown(skill, request.Caster);
        if (cooldown > 0f)
            _cooldowns[skill] = cooldown;

        return true;
    }

    public ScriptableObject GetBasicAttackSkill()
    {
        if (skillLoadout == null)
            return null;

        EquipmentSkillLoadoutEntry entry = skillLoadout.BasicAttack;
        return entry != null ? entry.SkillSo : null;
    }

    private bool ExecuteEquipmentSkill(SkillExecutionRequest request, ScriptableObject skill, Transform explicitTarget, bool usePoint)
    {
        if (request.Caster == null || skill == null)
            return false;

        if (!(skill is EquipmentSkillSO equipmentSkill))
            return false;

        if (_equipmentSkillResolver == null)
            _equipmentSkillResolver = new EquipmentSkillResolver();

        if (_projectileFactory == null)
            _projectileFactory = new ProjectileFactory();

        EquipmentSkillLoadoutEntry entry = FindLoadoutEntry(skill);
        EquipmentSkillRuntimeData runtime = entry != null ? entry.RuntimeData : null;
        if (runtime == null)
        {
            EquipmentSkillInstanceData instanceData = entry != null ? entry.BuildInstanceData() : new EquipmentSkillInstanceData();
            runtime = _equipmentSkillResolver.Resolve(equipmentSkill, instanceData);
        }

        if (runtime == null)
            return false;

        Vector2 spawnPosition = request.Caster.position;
        Vector2 direction = ResolveExecutionDirection(request, explicitTarget);
        GameObject targetObject = explicitTarget != null ? explicitTarget.gameObject : null;

        ProjectileRuntimeData projectileData = _equipmentSkillResolver.ResolveProjectileRuntime(
            runtime,
            request.Caster.gameObject,
            targetObject,
            spawnPosition,
            direction);

        if (projectileData == null)
            return false;

        ProjectileEntity projectilePrefab = projectileData.projectilePrefab != null
            ? projectileData.projectilePrefab
            : runtime.projectilePrefab;

        if (projectilePrefab == null)
            return false;

        _projectileFactory.SpawnOriented(projectilePrefab, projectileData);
        return true;
    }

    private EquipmentSkillLoadoutEntry FindLoadoutEntry(ScriptableObject skill)
    {
        if (skillLoadout == null || skill == null)
            return null;

        EquipmentSkillLoadoutEntry[] entries = skillLoadout.GetAllEntries();
        if (entries == null || entries.Length == 0)
            return null;

        for (int i = 0; i < entries.Length; i++)
        {
            EquipmentSkillLoadoutEntry entry = entries[i];
            if (entry != null && entry.SkillSo == skill)
                return entry;
        }

        return null;
    }

    private Vector2 ResolveExecutionDirection(SkillExecutionRequest request, Transform explicitTarget)
    {
        if (request.Caster == null)
            return Vector2.right;

        Vector2 origin = request.Caster.position;

        if (explicitTarget != null)
        {
            Vector2 toTarget = (Vector2)explicitTarget.position - origin;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }

        if (request.UsePoint)
        {
            Vector2 toPoint = request.TargetPoint - (Vector3)origin;
            if (toPoint.sqrMagnitude > 0.0001f)
                return toPoint.normalized;
        }

        return request.Caster.right;
    }

    private void TryPlayBasicAttackAnimation(ScriptableObject skill)
    {
        if (!IsBasicAttackSkill(skill))
            return;

        if (_animationMono == null)
            _animationMono = GetComponentInChildren<venus.eldawn.party.AnimationMono>();

        if (_animationMono == null)
            return;

        _animationMono.PlayAttack();

        if (debugLog)
            Debug.Log($"[SkillExecutor] basic attack animation played skill={skill.name} caster={name}");
    }

    private bool IsBasicAttackSkill(ScriptableObject skill)
    {
        if (skill == null)
            return false;

        ScriptableObject basicAttackSkill = GetBasicAttackSkill();
        return basicAttackSkill != null && basicAttackSkill == skill;
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

        if (skill is EquipmentSkillSO equipmentSkill)
            return Mathf.Max(0f, equipmentSkill.CastSo.Cooldown);

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

    public bool IsInSkillRange(ScriptableObject skill, Transform caster, Transform target)
    {
        if (skill == null || caster == null || target == null)
            return false;

        float rawRange = GetSkillRange(skill);
        if (rawRange <= 0f)
            return true;

        float usableRange = Mathf.Max(0f, rawRange - Mathf.Max(0f, skillRangeCheckInset));
        float dist = GetRangeCheckDistance(caster, target);
        bool inRange = dist <= usableRange;

        if (debugLog)
        {
            Debug.Log($"[SkillExecutor] range check skill={skill.name} dist={dist:0.00} rawRange={rawRange:0.00} usableRange={usableRange:0.00} inRange={inRange} caster={caster.name} target={target.name}");
        }

        return inRange;
    }

    private float GetSkillRange(ScriptableObject skill)
    {
        if (skill == null)
            return 0f;

        if (skill is EquipmentSkillSO equipmentSkill)
            return Mathf.Max(0f, equipmentSkill.CastSo.Range);

        var t = skill.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        string[] propNames = { "Range", "range" };
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

        string[] fieldNames = { "range", "Range" };
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

    private float GetSkillCooldown(ScriptableObject skill)
    {
        if (skill == null)
            return 0f;

        if (skill is EquipmentSkillSO equipmentSkill)
            return Mathf.Max(0f, equipmentSkill.CastSo.Cooldown);

        return GetCooldownFromSkill(skill);
    }

    private float GetRangeCheckDistance(Transform caster, Transform target)
    {
        if (!useClosestPointForRangeCheck)
            return Vector3.Distance(caster.position, target.position);

        Vector3 from = GetRangeReferencePoint(caster, target.position);
        Vector3 to = GetRangeReferencePoint(target, from);
        return Vector3.Distance(from, to);
    }

    private Vector3 GetRangeReferencePoint(Transform source, Vector3 fallbackTargetPoint)
    {
        if (source == null)
            return fallbackTargetPoint;

        Collider2D col2D = source.GetComponentInChildren<Collider2D>();
        if (col2D != null)
            return col2D.ClosestPoint(fallbackTargetPoint);

        Collider col3D = source.GetComponentInChildren<Collider>();
        if (col3D != null)
            return col3D.ClosestPoint(fallbackTargetPoint);

        return source.position;
    }

    private float GetResolvedCooldown(ScriptableObject skill, Transform caster)
    {
        float baseCooldown = GetSkillCooldown(skill);
        if (skill == null)
            return Mathf.Max(0f, baseCooldown);

        if (caster == null)
            return Mathf.Max(0f, baseCooldown);

        SkillUpgradeMono upgradeMono = caster.GetComponentInParent<SkillUpgradeMono>();
        if (upgradeMono == null)
            return Mathf.Max(0f, baseCooldown);

        SkillUpgradeMono.SkillUpgradeData upgrade = upgradeMono.GetUpgradeData(skill);
        float resolvedCooldown = baseCooldown + upgrade.cooldownAdd;
        return Mathf.Max(0f, resolvedCooldown);
    }
}