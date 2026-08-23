using System;
using System.Collections.Generic;

namespace Skill
{
    [Serializable]
    public sealed class SkillClassificationPresentationData
    {
        private readonly string[] keys;

        public SkillType SkillType { get; }
        public SkillComponentType ComponentType { get; }
        public BattleSkillCategory Category { get; }
        public BattleSkillTargetType TargetType { get; }
        public BattleSkillTacticalNeed TacticalNeed { get; }
        public IReadOnlyList<string> Keys => keys;

        public SkillClassificationPresentationData(
            SkillType skillType,
            SkillComponentType componentType,
            BattleSkillCategory category,
            BattleSkillTargetType targetType,
            BattleSkillTacticalNeed tacticalNeed)
        {
            SkillType = skillType;
            ComponentType = componentType;
            Category = category;
            TargetType = targetType;
            TacticalNeed = tacticalNeed;
            keys = BuildKeys();
        }

        private string[] BuildKeys()
        {
            List<string> result = new()
            {
                SkillType.ToString(),
                ComponentType.ToString(),
            };
            AddWhenMeaningful(result, Category);
            AddWhenMeaningful(result, TargetType);
            AddWhenMeaningful(result, TacticalNeed);
            return result.ToArray();
        }

        private static void AddWhenMeaningful<T>(
            ICollection<string> target,
            T value)
            where T : struct, Enum
        {
            if (Convert.ToInt32(value) != 0)
            {
                target.Add(value.ToString());
            }
        }
    }
}
