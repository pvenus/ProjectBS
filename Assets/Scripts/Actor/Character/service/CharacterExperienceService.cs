using UnityEngine;
using Stat;

namespace Character
{
    /// <summary>
    /// Character experience gain service.
    ///
    /// CharacterManager exposes only GainExperience(baseExperience),
    /// and this service owns all bonus calculation related to experience gain.
    ///
    /// Applied stats:
    /// - ExpGain: additional experience percentage based on base experience
    /// </summary>
    public class CharacterExperienceService
    {
        public Result GainExperience(
            CharacterManager characterManager,
            float baseExperience)
        {
            Result result = new Result();

            if (characterManager == null
                || characterManager.RuntimeData == null
                || baseExperience <= 0f)
            {
                return result;
            }

            float expGainPercent =
                Mathf.Max(
                    0f,
                    characterManager.GetStatValue(StatType.ExpGain));

            float bonusExperience =
                baseExperience * (expGainPercent / 100f);

            result.baseExperience =
                Mathf.RoundToInt(baseExperience);


            result.percentBonusExperience =
                Mathf.RoundToInt(bonusExperience);

            result.totalExperience =
                Mathf.Max(
                    0,
                    result.baseExperience
                    + result.percentBonusExperience);

            if (result.totalExperience <= 0)
            {
                return result;
            }

            characterManager.AddStat(
                StatType.Experience,
                result.totalExperience);

            return result;
        }

        public struct Result
        {
            public int baseExperience;
            public int percentBonusExperience;
            public int totalExperience;
        }
    }
}