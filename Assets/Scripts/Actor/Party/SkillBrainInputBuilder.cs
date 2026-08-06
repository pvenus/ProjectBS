using UnityEngine;

/// <summary>
/// Builds the summarized BrainContext used by SkillBrainMono.
/// This class should only gather and shape perception/context input data.
/// It must not contain any decision or execution logic.
/// </summary>
public static class SkillBrainInputBuilder
{
    public static BrainContext Build(
        Transform self,
        Role role,
        StateMono state,
        LayerMask enemyMask,
        LayerMask allyMask,
        float enemyCheckRadius)
    {
        float selfHp01 = ReadHpRatio(self);
        float lowestAllyHp01 = 1f;
        Transform lowestAlly = FindLowestHpAlly(self, allyMask, enemyCheckRadius * 3f, out lowestAllyHp01);

        // Build only the summarized perception/context input used by the brain.
        return new BrainContext
        {
            role = role,
            partyState = state != null ? state.CurrentState : StateMono.PartyState.Normal,
            selfHp01 = selfHp01,
            nearbyEnemyCount = CountUnits(self, enemyMask, enemyCheckRadius),
            nearbyAllyCount = CountUnits(self, allyMask, enemyCheckRadius * 2f),
            lowestAllyHp01 = lowestAllyHp01,
            hasHealTarget = lowestAlly != null
        };
    }

    private static int CountUnits(Transform self, LayerMask mask, float radius)
    {
        if (self == null)
            return 0;

        Collider2D[] hits = Physics2D.OverlapCircleAll(self.position, radius, mask);
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null || c.isTrigger)
                continue;

            Transform t = c.transform;
            if (t == self || t.IsChildOf(self))
                continue;

            count++;
        }

        return count;
    }

    private static Transform FindLowestHpAlly(Transform self, LayerMask allyMask, float radius, out float lowestHp01)
    {
        lowestHp01 = 1f;

        if (self == null)
            return null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(self.position, radius, allyMask);
        Transform best = null;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null || c.isTrigger)
                continue;

            Transform t = c.transform;
            if (t == self || t.IsChildOf(self))
                continue;

            float hp01 = ReadHpRatio(t);
            if (hp01 >= 0.999f)
                continue;

            if (best == null || hp01 < lowestHp01)
            {
                best = t;
                lowestHp01 = hp01;
            }
        }

        return best;
    }

    private static float ReadHpRatio(Transform unit)
    {
        if (unit == null)
            return 1f;

        StatMono stat = unit.GetComponent<StatMono>();
        if (stat == null)
            stat = unit.GetComponentInParent<StatMono>();

        if (stat == null)
            return 1f;

        return GetHpRatioFromStat(stat);
    }

    private static float GetHpRatioFromStat(StatMono stat)
    {
        if (stat == null)
            return 1f;

        var type = stat.GetType();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

        var hp01Prop = type.GetProperty("Hp01", flags);
        if (hp01Prop != null && hp01Prop.PropertyType == typeof(float))
        {
            try
            {
                return Mathf.Clamp01((float)hp01Prop.GetValue(stat));
            }
            catch
            {
            }
        }

        float currentHp = 0f;
        float maxHp = 0f;

        var currentHpField = type.GetField("currentHp", flags)
                          ?? type.GetField("CurrentHp", flags)
                          ?? type.GetField("hp", flags)
                          ?? type.GetField("Hp", flags);
        if (currentHpField != null && currentHpField.FieldType == typeof(float))
        {
            try
            {
                currentHp = (float)currentHpField.GetValue(stat);
            }
            catch
            {
            }
        }

        var maxHpField = type.GetField("maxHp", flags)
                      ?? type.GetField("MaxHp", flags)
                      ?? type.GetField("maxHP", flags)
                      ?? type.GetField("HPMax", flags);
        if (maxHpField != null && maxHpField.FieldType == typeof(float))
        {
            try
            {
                maxHp = (float)maxHpField.GetValue(stat);
            }
            catch
            {
            }
        }

        if (maxHp > 0f)
            return Mathf.Clamp01(currentHp / maxHp);

        return 1f;
    }
}