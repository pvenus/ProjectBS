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

        /// <summary>
        /// 이 투사체를 생성한 원본 스킬 SO.
        /// 런타임 업그레이드 재조회 시 기준으로 사용.
        /// </summary>
        public ProjectileSkill sourceSkill;

        /// <summary>
        /// 살아있는 동안 업그레이드 변화를 반영할지 여부.
        /// </summary>
        public bool useRuntimeUpgradeRefresh;

        /// <summary>
        /// 런타임 업그레이드 재조회 주기(초). 현재는 고정값으로 사용됨.
        /// </summary>
        public float runtimeUpgradeRefreshInterval;
    }
}