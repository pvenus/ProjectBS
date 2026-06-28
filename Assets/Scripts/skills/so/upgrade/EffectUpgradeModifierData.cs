using System;
using Skill;
using UnityEngine;

namespace Effect
{
    /// <summary>
    /// Defines how a specific effect is modified by an equipment upgrade.
    /// </summary>
    [Serializable]
    public class EffectUpgradeModifierData
    {
        [SerializeField] private string targetEffectId;
        [SerializeField] private EffectModifierFieldType fieldType;
        [SerializeField] private SkillStatModifierOperationType operationType = SkillStatModifierOperationType.Flat;
        [SerializeField] private float value;

        public string TargetEffectId => targetEffectId;
        public EffectModifierFieldType FieldType => fieldType;
        public SkillStatModifierOperationType OperationType => operationType;
        public float Value => value;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string targetEffectId,
            EffectModifierFieldType fieldType,
            SkillStatModifierOperationType operationType,
            float value)
        {
            this.targetEffectId = targetEffectId;
            this.fieldType = fieldType;
            this.operationType = operationType;
            this.value = value;
        }
#endif
    }
}