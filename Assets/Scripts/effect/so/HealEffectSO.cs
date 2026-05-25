

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
        public float maxHpPercent = 0.08f;

        public float flatHealAmount;

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