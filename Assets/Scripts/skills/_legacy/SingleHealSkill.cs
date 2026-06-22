using UnityEngine;
using System.Reflection;

/// <summary>
/// SingleHealSkill
/// - Heals the lowest-HP ally in range or a specified ally target.
/// - Uses reflection as a temporary bridge for different HP receiver implementations.
/// - Uses VFX ScriptableObjects for cast / impact presentation.
/// </summary>
[CreateAssetMenu(fileName = "SingleHealSkill", menuName = "BS/Skills/Single Heal Skill")]
public class SingleHealSkill : BattleSkillBase
{
    [Header("Target")]
    [SerializeField] private LayerMask allyMask = ~0;

    [Header("Heal")]
    [SerializeField] private float healAmount = 5f;

    [Header("VFX")]
    [SerializeField] private VFX_RangedCast castVfx;
    [SerializeField] private VFX_RangedHit impactVfx;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    public float HealAmount => healAmount;

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    public bool Execute(Transform caster)
    {
        if (caster == null) return false;

        Transform target = FindLowestHpAlly(caster);
        if (target == null)
            return false;

        Vector3 dir = (target.position - caster.position);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.up;

        castVfx?.Play(caster, dir.normalized);

        bool healed = HealTarget(target, healAmount);
        if (healed)
            impactVfx?.Play(target);

        return healed;
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null || target == null)
            return false;

        float dist = Vector2.Distance(caster.position, target.position);
        if (dist > Range)
            return false;

        Vector3 dir = (target.position - caster.position);
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.up;

        castVfx?.Play(caster, dir.normalized);

        bool healed = HealTarget(target, healAmount);
        if (healed)
            impactVfx?.Play(target);

        return healed;
    }

    private Transform FindLowestHpAlly(Transform caster)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(caster.position, Mathf.Max(0.1f, Range), allyMask);

        Transform bestTarget = null;
        float bestHpRatio = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            Transform t = c.transform;
            if (t == caster || t.IsChildOf(caster))
                continue;

            var stat = t.GetComponentInParent<StatMono>();
            if (stat == null)
                continue;

            float hpRatio = GetHpRatio(stat);
            if (hpRatio >= 0.999f)
                continue;

            if (hpRatio < bestHpRatio)
            {
                bestHpRatio = hpRatio;
                bestTarget = stat.transform;
            }
        }

        return bestTarget;
    }

    private float GetHpRatio(StatMono stat)
    {
        if (stat == null) return 1f;

        var type = stat.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var hp01Prop = type.GetProperty("Hp01", flags);
        if (hp01Prop != null && hp01Prop.PropertyType == typeof(float))
        {
            try { return Mathf.Clamp01((float)hp01Prop.GetValue(stat)); } catch { }
        }

        float currentHp = ReadFloat(type, stat, flags, "currentHp", "CurrentHp", "hp");
        float maxHp = ReadFloat(type, stat, flags, "maxHp", "MaxHp", "maxHP");

        if (maxHp > 0f)
            return Mathf.Clamp01(currentHp / maxHp);

        return 1f;
    }

    private bool HealTarget(Transform target, float amount)
    {
        if (target == null) return false;

        var stat = target.GetComponentInParent<StatMono>();
        if (stat == null)
            return false;

        var type = stat.GetType();
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Preferred: call a heal-like method if it exists.
        string[] methodNames = { "Heal", "RestoreHp", "RestoreHP", "RecoverHp", "RecoverHP", "AddHp", "AddHP" };
        for (int i = 0; i < methodNames.Length; i++)
        {
            var m = type.GetMethod(methodNames[i], flags, null, new System.Type[] { typeof(float) }, null);
            if (m != null)
            {
                try
                {
                    object result = m.Invoke(stat, new object[] { amount });
                    if (debugLog)
                        Debug.Log($"[SingleHealSkill] Healed {target.name} by method {methodNames[i]} amount={amount:0.##}");
                    if (result is bool b) return b;
                    return true;
                }
                catch { }
            }

            var mInt = type.GetMethod(methodNames[i], flags, null, new System.Type[] { typeof(int) }, null);
            if (mInt != null)
            {
                try
                {
                    object result = mInt.Invoke(stat, new object[] { Mathf.RoundToInt(amount) });
                    if (debugLog)
                        Debug.Log($"[SingleHealSkill] Healed {target.name} by method {methodNames[i]} amount={amount:0.##}");
                    if (result is bool b) return b;
                    return true;
                }
                catch { }
            }
        }

        // Fallback: directly modify currentHp if the fields exist.
        var currentHpField = type.GetField("currentHp", flags) ?? type.GetField("CurrentHp", flags);
        var maxHpField = type.GetField("maxHp", flags) ?? type.GetField("MaxHp", flags);
        if (currentHpField != null && maxHpField != null && currentHpField.FieldType == typeof(float) && maxHpField.FieldType == typeof(float))
        {
            try
            {
                float currentHp = (float)currentHpField.GetValue(stat);
                float maxHp = (float)maxHpField.GetValue(stat);
                currentHp = Mathf.Min(maxHp, currentHp + amount);
                currentHpField.SetValue(stat, currentHp);

                if (debugLog)
                    Debug.Log($"[SingleHealSkill] Healed {target.name} by field fallback amount={amount:0.##}");
                return true;
            }
            catch { }
        }

        return false;
    }

    private float ReadFloat(System.Type type, object instance, BindingFlags flags, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            var f = type.GetField(names[i], flags);
            if (f != null && f.FieldType == typeof(float))
            {
                try { return (float)f.GetValue(instance); } catch { }
            }

            var p = type.GetProperty(names[i], flags);
            if (p != null && p.PropertyType == typeof(float))
            {
                try { return (float)p.GetValue(instance); } catch { }
            }
        }

        return 0f;
    }
}
