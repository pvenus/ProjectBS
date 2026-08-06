using System;
using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 장비 = 스킬의 기본 정체성 데이터를 정의하는 Profile SO.
    ///
    /// Identity는 모든 스킬이 공통으로 가진다.
    /// Projectile 관련 값은 투사체 스킬에서만 의미가 있으므로 별도 단위로 묶는다.
    /// JSON 생성 시 projectile 값을 생략해도 기본값으로 안전하게 동작하도록 한다.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentBaseProfileSO", menuName = "BS/Skills/Equipment/EquipmentBaseProfileSO")]
    public class EquipmentBaseProfileSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string baseProfileId;
        [SerializeField] private SkillType skillType = SkillType.Active;
        [SerializeField] private SkillComponentType skillComponentType = SkillComponentType.Projectile;

        [Header("Projectile Defaults")]
        [SerializeField, Min(1)] private int projectileCount = 1;
        [SerializeField] private float projectileScale = 1f;
        [SerializeField] private float projectileColliderRadius = 0.5f;
        [SerializeField] private float projectileLifetime = 3f;

        [Header("Projectile")]
        [SerializeField] private ProjectileArrangementProfile projectile = new();

        [Header("Projectile Spawn Sequence")]
        [SerializeField] private ProjectileSpawnProfile projectileSpawn = new();
        [Header("Brain Meta")]
        [SerializeField] private BattleSkillBrainMetaProfile brainMeta = new();

        public BattleSkillCategory Category => brainMeta != null
            ? brainMeta.Category
            : BattleSkillCategory.None;

        public BattleSkillTargetType TargetType => brainMeta != null
            ? brainMeta.TargetType
            : BattleSkillTargetType.None;

        public BattleSkillTacticalNeed TacticalNeed => brainMeta != null
            ? brainMeta.TacticalNeed
            : BattleSkillTacticalNeed.None;

        public float BasePriority => brainMeta != null
            ? brainMeta.BasePriority
            : 0f;

        public string BaseProfileId => baseProfileId;
        public SkillType SkillType => skillType;
        public SkillComponentType SkillComponentType => skillComponentType;

        public float ProjectileSpawnOffset => projectileSpawn != null
            ? projectileSpawn.SpawnOffset
            : 0f;

        public int ProjectileCount => Mathf.Max(1, projectileCount);
        public float ProjectileScale => Mathf.Max(0.01f, projectileScale);
        public float ProjectileColliderRadius => Mathf.Max(0.01f, projectileColliderRadius);
        public float ProjectileLifetime => Mathf.Max(0.01f, projectileLifetime);

        public ProjectileArrangementType ProjectileArrangement => projectile != null
            ? projectile.Arrangement
            : ProjectileArrangementType.Spread;

        public float ProjectileArrangementValue => projectile != null
            ? projectile.ArrangementValue
            : 0f;

        public float ProjectileSpreadAngle => projectile != null
            ? projectile.SpreadAngle
            : 0f;

        public float ProjectileSpawnInterval => projectileSpawn != null
            ? projectileSpawn.Interval
            : 0f;

        public float ProjectileSpawnRadius => projectile != null
            ? projectile.Radius
            : 0f;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string baseProfileId,
            SkillType skillType,
            SkillComponentType skillComponentType,
            int projectileCount,
            float projectileScale,
            float projectileColliderRadius,
            float projectileLifetime)
        {
            this.baseProfileId = baseProfileId;
            this.skillType = skillType;
            this.skillComponentType = skillComponentType;
            this.projectileCount = projectileCount;
            this.projectileScale = projectileScale;
            this.projectileColliderRadius = projectileColliderRadius;
            this.projectileLifetime = projectileLifetime;
        }

        public void ApplyEditorProjectileArrangement(
            ProjectileArrangementType arrangement,
            float arrangementValue,
            float spreadAngle,
            float radius)
        {
            projectile.ApplyEditorData(
                arrangement,
                arrangementValue,
                spreadAngle,
                radius);
        }

        public void ApplyEditorProjectileSpawn(
            float spawnOffset,
            float interval)
        {
            projectileSpawn.ApplyEditorData(
                spawnOffset,
                interval);
        }

        public void ApplyEditorBrainMeta(
            BattleSkillCategory category,
            BattleSkillTargetType targetType,
            BattleSkillTacticalNeed tacticalNeed,
            float basePriority)
        {
            brainMeta.ApplyEditorData(
                category,
                targetType,
                tacticalNeed,
                basePriority);
        }
#endif
    }

    [Serializable]
    public class BattleSkillBrainMetaProfile
    {
        [SerializeField] private BattleSkillCategory category = BattleSkillCategory.None;
        [SerializeField] private BattleSkillTargetType targetType = BattleSkillTargetType.None;
        [SerializeField] private BattleSkillTacticalNeed tacticalNeed = BattleSkillTacticalNeed.None;
        [SerializeField] private float basePriority = 0f;

        public BattleSkillCategory Category => category;
        public BattleSkillTargetType TargetType => targetType;
        public BattleSkillTacticalNeed TacticalNeed => tacticalNeed;
        public float BasePriority => basePriority;

#if UNITY_EDITOR
        public void ApplyEditorData(
            BattleSkillCategory category,
            BattleSkillTargetType targetType,
            BattleSkillTacticalNeed tacticalNeed,
            float basePriority)
        {
            this.category = category;
            this.targetType = targetType;
            this.tacticalNeed = tacticalNeed;
            this.basePriority = basePriority;
        }
#endif
    }

    [Serializable]
    public class ProjectileArrangementProfile
    {
        [Header("Arrangement")]
        [SerializeField] private ProjectileArrangementType arrangement = ProjectileArrangementType.Spread;
        [SerializeField, Min(0f)] private float arrangementValue = 0f;
        [SerializeField, Min(0f)] private float spreadAngle = 0f;
        [SerializeField, Min(0f)] private float radius = 0f;

        public ProjectileArrangementType Arrangement => arrangement;
        public float ArrangementValue => Mathf.Max(0f, arrangementValue);
        public float SpreadAngle => Mathf.Max(0f, spreadAngle);
        public float Radius => Mathf.Max(0f, radius);

#if UNITY_EDITOR
        public void ApplyEditorData(
            ProjectileArrangementType arrangement,
            float arrangementValue,
            float spreadAngle,
            float radius)
        {
            this.arrangement = arrangement;
            this.arrangementValue = arrangementValue;
            this.spreadAngle = spreadAngle;
            this.radius = radius;
        }
#endif
    }

    [Serializable]
    public class ProjectileSpawnProfile
    {
        [SerializeField] private float spawnOffset = 0f;
        [SerializeField, Min(0f)] private float interval = 0f;

        public float SpawnOffset => spawnOffset;
        public float Interval => Mathf.Max(0f, interval);

#if UNITY_EDITOR
        public void ApplyEditorData(
            float spawnOffset,
            float interval)
        {
            this.spawnOffset = spawnOffset;
            this.interval = interval;
        }
#endif
    }
}