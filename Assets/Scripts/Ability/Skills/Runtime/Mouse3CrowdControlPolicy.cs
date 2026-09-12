using Stat;

namespace Skill
{
    public static class Mouse3CrowdControlPolicy
    {
        public static float ResolveResistanceMultiplier(Character.CharacterManager target)
        {
            if (target == null) return 0f;
            float ratio = target.GetStatValue(StatType.StatusResistance);
            float percent = target.GetStatValue(StatType.StatusResistancePercent) * .01f;
            return UnityEngine.Mathf.Clamp01(1f - UnityEngine.Mathf.Clamp01(ratio + percent));
        }

        public static float ResolveDuration(Character.CharacterManager target, float authoredDuration) =>
            UnityEngine.Mathf.Max(0f, authoredDuration) * ResolveResistanceMultiplier(target);

        public static float ResolveDistance(Character.CharacterManager target, float authoredDistance) =>
            UnityEngine.Mathf.Max(0f, authoredDistance) * ResolveResistanceMultiplier(target);
    }
}
