

using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "AttackBleedEffectSO",
        menuName = "Effect/Attack Bleed Effect")]
    public class AttackBleedEffectSO : EffectSO
    {
        [Header("Chance")]
        [Range(0f, 100f)]
        public float chancePercent = 10f;

        [Header("Bleed")]
        [Tooltip("공격력의 몇 %를 초당 출혈 데미지로 적용할지 설정합니다.")]
        [Min(0f)]
        public float attackRatioPercent = 10f;

        public AttackBleedEffectRuntime CreateRuntimeData(
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter)
        {
            if (targetCharacter == null || sourceCharacter == null)
            {
                return null;
            }

            return new AttackBleedEffectRuntime(
                this,
                targetCharacter,
                sourceCharacter);
        }
    }
}