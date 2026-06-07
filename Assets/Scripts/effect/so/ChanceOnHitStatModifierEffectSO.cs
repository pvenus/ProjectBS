using Character;
using Stat;
using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "ChanceOnHitStatModifierEffectSO",
        menuName = "Effect/Chance On Hit Stat Modifier Effect")]
    public class ChanceOnHitStatModifierEffectSO : EffectSO
    {
        [Header("Chance")]
        [Range(0f, 100f)]
        public float chancePercent = 10f;

        [Header("Stat Modifier")]
        public StatType statType =
            StatType.Attack;

        public StatModifierType valueType =
            StatModifierType.Percent;

        public float value = -15f;

        public ChanceOnHitStatModifierEffectRuntime CreateRuntimeData(
            CharacterManager targetCharacter)
        {
            if (targetCharacter == null)
            {
                return null;
            }

            return new ChanceOnHitStatModifierEffectRuntime(
                this,
                targetCharacter);
        }
    }
}