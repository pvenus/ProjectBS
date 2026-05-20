using Stat;
using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "StatModifierEffectSO",
        menuName = "Effect/Stat Modifier Effect SO")]
    public class StatModifierEffectSO : EffectSO
    {
        [Header("Stat Modifier")]
        public StatType targetStat = StatType.None;

        public StatModifierType modifierType =
            StatModifierType.Flat;

        public float value;
    }

    public enum StatModifierType
    {
        Flat = 0,
        Percent = 100,
        Multiply = 200,
    }
}
