

using Character;
using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "HealEffect",
        menuName = "Effect/Heal Effect")]
    public class HealEffectSO : EffectSO
    {
        [Header("Heal")]
        public bool useMaxHpPercent = true;

        [Range(0f, 10f)]
        public float maxHpPercent = 0f;

        public float flatHealAmount;

        [Header("Attack Scaling")]
        public bool useAttackScaling;

        [Range(0f, 20f)]
        public float attackPercentHeal = 0f;

        public bool clampToMaxHp = true;

        public EffectRuntimeData CreateRuntimeData(
            CharacterManager targetCharacter)
        {
            return new HealEffectRuntime(
                this,
                targetCharacter);
        }
    }
}