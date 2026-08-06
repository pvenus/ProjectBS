

using Character;
using UnityEngine;

namespace Battle
{
    [CreateAssetMenu(
        fileName = "StrategicSkillCostConfig",
        menuName = "Battle/Strategic Skill Cost Config"
    )]
    public class StrategicSkillCostConfigSO : ScriptableObject
    {
        [Header("Gauge")]
        public int maxGauge = 100;
        public int initialGauge = 0;

        [Header("Monster Kill Gain")]
        public int normalMonsterKillGain = 1;
        public int eliteMonsterKillGain = 8;
        public int bossMonsterKillGain = 20;

        [Header("Passive Gain")]
        public bool usePassiveGain;
        public float passiveGainInterval = 1f;
        public int passiveGainAmount = 5;

        public int GetCharacterKillGain(CharacterType characterType)
        {
            switch (characterType)
            {
                case CharacterType.Npc:
                    return normalMonsterKillGain;

                case CharacterType.Boss:
                    return bossMonsterKillGain;

                default:
                    return 0;
            }
        }
    }
}