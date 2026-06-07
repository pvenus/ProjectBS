

using Skill;
using UnityEngine;
using Character;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "ChanceOnHitSkillEffect",
        menuName = "BS/Effect/Chance On Hit Skill")]
    public class ChanceOnHitSkillEffectSO : EffectSO
    {
        [Header("Trigger")]
        public float chance = 1f;
        public bool requireCriticalHit;

        [Header("Skill")]
        public EquipmentSkillSO skillSo;

        [Tooltip("0 이하면 기본 타겟 사용")]
        public float rangeOverride = -1f;

        public EffectRuntimeData CreateRuntimeData(
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter)
        {
            return new ChanceOnHitSkillEffectRuntime(
                this,
                targetCharacter,
                sourceCharacter);
        }
    }
}