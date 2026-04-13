using UnityEngine;

namespace Skills.Dto
{
    public enum SkillProjectileFireType
    {
        Forward,
        Targeting,
        Spread,
        Radial
    }

    [System.Serializable]
    public class SkillProjectileDto
    {
        public SkillMoveSO moveConfig;

        public float lifetime = 1f;
        public int projectileCount = 1;

        public SkillProjectileFireType fireType = SkillProjectileFireType.Forward;
    }
}