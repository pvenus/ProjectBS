using UnityEngine;

namespace Skill
{
    /// <summary>
    /// 장비 = 스킬의 기본 정체성 데이터를 정의하는 Profile SO.
    /// 장비의 등급, 룬 슬롯, 공격 원형, 투사체 생성 리소스처럼
    /// 장비 자체에 귀속되는 기본값을 한곳에 모아 관리한다.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentBaseProfileSO", menuName = "BS/Skills/Equipment/EquipmentBaseProfileSO")]
    public class EquipmentBaseProfileSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private SkillType skillType = SkillType.Active;
        [SerializeField] private EffectType effectType = EffectType.Projectile;
        [SerializeField] private AttackArchetype attackArchetype = AttackArchetype.None;
        [SerializeField] private bool skipAttackAnimation;

        [Header("Projectile Resource")]
        [SerializeField] private ProjectileEntity projectilePrefab;
        [SerializeField] private float projectileSpawnOffset = 0f;

        [Header("Projectile Base Stats")]
        [SerializeField, Min(1)] private int projectileCount = 1;
        [SerializeField] private ProjectileArrangementType projectileArrangement = ProjectileArrangementType.Single;
        [SerializeField, Min(0f)] private float projectileArrangementValue = 0f;
        [SerializeField, Min(0f)] private float projectileSpreadAngle = 0f;
        [SerializeField] private float projectileScale = 1f;
        [SerializeField] private float projectileLifetime = 3f;

        [Header("Projectile Spawn Sequence")]
        [SerializeField, Min(0f)] private float projectileSpawnInterval = 0f;
        [SerializeField, Min(0f)] private float projectileSpawnRadius = 0f;

        [Header("Brain Meta")]
        [SerializeField] private BattleSkillCategory category = BattleSkillCategory.None;
        [SerializeField] private BattleSkillTargetType targetType = BattleSkillTargetType.None;
        [SerializeField] private BattleSkillTacticalNeed tacticalNeed = BattleSkillTacticalNeed.None;
        [SerializeField] private float basePriority = 0f;

        public BattleSkillCategory Category => category;
        public BattleSkillTargetType TargetType => targetType;
        public BattleSkillTacticalNeed TacticalNeed => tacticalNeed;
        public float BasePriority => basePriority;


        public SkillType SkillType => skillType;
        public EffectType EffectType => effectType;
        public AttackArchetype AttackArchetype => attackArchetype;
        public bool SkipAttackAnimation => skipAttackAnimation;
        public ProjectileEntity ProjectilePrefab => projectilePrefab;
        public float ProjectileSpawnOffset => projectileSpawnOffset;
        public int ProjectileCount => Mathf.Max(1, projectileCount);
        public ProjectileArrangementType ProjectileArrangement => projectileArrangement;
        public float ProjectileArrangementValue => Mathf.Max(0f, projectileArrangementValue);
        public float ProjectileSpreadAngle => Mathf.Max(0f, projectileSpreadAngle);
        public float ProjectileScale => Mathf.Max(0.01f, projectileScale);
        public float ProjectileLifetime => Mathf.Max(0.01f, projectileLifetime);
        public float ProjectileSpawnInterval => Mathf.Max(0f, projectileSpawnInterval);
        public float ProjectileSpawnRadius => Mathf.Max(0f, projectileSpawnRadius);
    }
}