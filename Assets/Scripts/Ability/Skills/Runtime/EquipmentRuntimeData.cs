using UnityEngine;
using Skill;
[System.Serializable]
public class EquipmentSkillInstanceData
{
    [Header("Identity")]
    public string equipmentId;

    [Header("Progression")]
    public int currentLevel = 1;
    public int upgradeLevel = 0;
}

[System.Serializable]
public class EquipmentSkillRuntimeData
{
    [Header("Source")]
    public EquipmentSkillSO sourceEquipment;
    public EquipmentSkillInstanceData instanceData;

    [Header("Resolved Identity")]
    public int resolvedLevel;
    public float resolvedRange;

    public int resolvedBurstCount = 1;
    public float resolvedBurstInterval;

    public int resolvedProjectileCount;
    public float resolvedProjectileSpreadAngle;
    public float resolvedProjectileArrangementValue;
    public float resolvedProjectileScale;
    public float resolvedRendererScale = 1f;

    [Header("Resolved Runtime Context")]
    public ResolvedVisualContextDto visualContext;
    public EquipmentUpgradeRuntimeData upgradeRuntimeData;
    public Skill.SkillComboProfile comboProfile;
    public Skill.ResolvedMouse3SkillProfile resolvedMouse3Profile;
}

namespace Skill
{
    [System.Serializable]
    public sealed class ResolvedMouse3SkillProfile
    {
        public string crowdControlKind;
        public float distance;
        public float duration;
        public float stopRadius;
        public bool collisionSafe;
        public float normalRatio;
        public float eliteRatio;
        public float bossRatio;
        public float bossHardCap;
        public float fanAngle;
        public int burstCount = 1;
        public float burstInterval;
        public float nextBasicForwardRatio;
        public float nextBasicRangeRatio;
        public float nextBasicInputGrace;
        public float gatherDistance;
        public float gatherDuration;
        public float stunNormalDuration;
        public float stunEliteDuration;
        public float stunBossDuration;
        public bool Enabled => !string.IsNullOrWhiteSpace(crowdControlKind);
    }
}
