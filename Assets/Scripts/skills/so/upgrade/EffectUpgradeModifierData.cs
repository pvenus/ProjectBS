

using System;

namespace Skill
{
    /// <summary>
    /// 특정 Effect의 값을 업그레이드하기 위한 데이터.
    /// 예) 공격력 버프 수치 증가, 지속시간 증가, 발동 확률 증가
    /// </summary>
    [Serializable]
    public class EffectUpgradeModifierData
    {
        public string effectId;

        public EffectModifierFieldType fieldType;

        public SkillStatModifierOperationType operationType =
            SkillStatModifierOperationType.Flat;

        public float value;

        public static EffectUpgradeModifierData Create(
            string effectId,
            EffectModifierFieldType fieldType,
            SkillStatModifierOperationType operationType,
            float value)
        {
            return new EffectUpgradeModifierData
            {
                effectId = effectId,
                fieldType = fieldType,
                operationType = operationType,
                value = value
            };
        }
    }
}