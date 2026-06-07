using Skill;
using System.Collections.Generic;

namespace Character.Runtime.Skill
{
    /// <summary>
    /// Character skill runtime state.
    ///
    /// Owns the character's skill pool and any future
    /// runtime-only skill related data.
    /// </summary>
    [System.Serializable]
    public class CharacterSkillRuntimeData
    {
        public SkillPoolRuntimeData skillPool = new();

        /// <summary>
        /// Runtime cooldown state.
        /// Key = skillId.
        /// Value = cooldown end time.
        /// </summary>
        public Dictionary<string, float> cooldownEndTimes = new();
    }
}