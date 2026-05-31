

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    [CreateAssetMenu(
        fileName = "SkillPoolOverride",
        menuName = "Character/Skill Pool Override")]
    public class SkillPoolOverrideSO : ScriptableObject
    {
        public List<SkillPoolOverrideEntry> overrides = new();
    }

    [Serializable]
    public class SkillPoolOverrideEntry
    {
        [Header("Target Slot")]
        public string slotKey;

        [Header("Override Skill")]
        public EquipmentSkillSO skillSo;
    }
}