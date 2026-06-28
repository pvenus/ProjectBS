using System.Collections;
using Character;
using Effect;
using Effect.Helper;
using Skill;
using UnityEngine;

namespace Skill.Service.Helper
{
    public sealed class SkillUseContext
    {
        public EquipmentSkillRuntimeData Runtime { get; set; }
        public Transform Caster { get; set; }
        public Transform Target { get; set; }
        public bool UsePoint { get; set; }
        public Vector2 TargetPoint { get; set; }
        public MonoBehaviour CoroutineRunner { get; set; }

        public GameObject CasterObject =>
            Caster != null ? Caster.gameObject : null;

        public GameObject TargetObject =>
            Target != null ? Target.gameObject : null;
    }

    /// <summary>
    /// 스킬 사용 시 공통 실행 로직을 모은 헬퍼.
    ///
    /// ActiveSkillService 같은 외부 스킬 실행 코드는 이 객체를 통해
    /// 투사체 생성과 시전자 자기 효과 적용을 처리한다.
    /// ProjectileEntity 내부 컴포넌트 동작은 ProjectileEntity.Initialize 이후 각 컴포넌트가 담당한다.
    /// </summary>
    public static class SkillUseHelper
    {
        private static ProjectileFactory projectileFactory;
        private static readonly EquipmentSkillResolver skillResolver = new();
        private static readonly EffectResolver effectResolver = new();

        private static ProjectileFactory GetProjectileFactory()
        {
            if (projectileFactory == null)
            {
                projectileFactory = new ProjectileFactory();
            }

            return projectileFactory;
        }

        public static bool UseSkill(
            SkillUseContext context)
        {
            if (context == null ||
                context.Runtime == null ||
                context.Caster == null)
            {
                return false;
            }

            switch (ResolveSkillComponentType(context.Runtime))
            {
                case SkillComponentType.Spawn:
                    return UseSpawnSkillAndSelfEffects(context);

                case SkillComponentType.Projectile:
                default:
                    return UseProjectileSkillAndSelfEffects(context);
            }
        }

        private static SkillComponentType ResolveSkillComponentType(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null ||
                runtime.sourceEquipment == null ||
                runtime.sourceEquipment.BaseProfileSo == null)
            {
                return SkillComponentType.Projectile;
            }

            return runtime.sourceEquipment.BaseProfileSo.SkillComponentType;
        }

        private static bool UseProjectileSkillAndSelfEffects(
            SkillUseContext context)
        {
            return UseSkillProjectilesAndSelfEffects(
                context.Runtime,
                context.Caster,
                context.Target,
                context.UsePoint,
                context.TargetPoint);
        }

        private static bool UseSpawnSkillAndSelfEffects(
            SkillUseContext context)
        {
            ApplyCastSelfEffects(
                context.Runtime,
                context.CasterObject);

            if (context == null ||
                context.Runtime == null ||
                context.Runtime.sourceEquipment == null ||
                context.Caster == null ||
                context.Runtime.sourceEquipment.SpawnSkillSo == null)
            {
                return false;
            }

            if (context.CoroutineRunner == null)
            {
                return UseSpawnSkillImmediately(context);
            }

            context.CoroutineRunner.StartCoroutine(
                UseSpawnSkillRoutine(context));

            return true;
        }

        private static IEnumerator UseSpawnSkillRoutine(
            SkillUseContext context)
        {
            if (context == null ||
                context.Runtime == null ||
                context.Caster == null)
            {
                yield break;
            }

            SpawnSkillSO spawnSkill = ResolveSpawnSkill(context.Runtime);
            int spawnCount = ResolveSpawnCount(spawnSkill);
            float spawnInterval = ResolveSpawnInterval(spawnSkill);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnOneCharacter(
                    context,
                    spawnSkill,
                    i,
                    spawnCount);

                if (i < spawnCount - 1 && spawnInterval > 0f)
                {
                    yield return new WaitForSeconds(spawnInterval);
                }
            }
        }

        private static bool UseSpawnSkillImmediately(
            SkillUseContext context)
        {
            if (context == null ||
                context.Runtime == null ||
                context.Caster == null)
            {
                return false;
            }

            SpawnSkillSO spawnSkill = ResolveSpawnSkill(context.Runtime);
            int spawnCount = ResolveSpawnCount(spawnSkill);
            bool spawnedAny = false;

            for (int i = 0; i < spawnCount; i++)
            {
                spawnedAny = SpawnOneCharacter(
                    context,
                    spawnSkill,
                    i,
                    spawnCount) || spawnedAny;
            }

            return spawnedAny;
        }

        private static bool SpawnOneCharacter(
            SkillUseContext context,
            SpawnSkillSO spawnSkill,
            int index,
            int count)
        {
            if (context == null ||
                context.Caster == null ||
                spawnSkill == null)
            {
                return false;
            }

            CharacterSO characterSo = ResolveSpawnCharacterSo(spawnSkill);

            if (characterSo == null)
            {
                return false;
            }

            float spawnLifeTime = ResolveSpawnLifeTime(spawnSkill);
            float spawnRadius = ResolveSpawnRadius(spawnSkill);

            Vector3 spawnPosition = ResolveSpawnPosition(
                context.Caster.position,
                index,
                count,
                spawnRadius);

            GameObject spawnedObject = Character.Helper.CharacterBuilder.CreateOrBuildPlayerObject(
                null,
                characterSo.name,
                null,
                spawnPosition,
                context.Caster.rotation,
                null,
                true);

            if (spawnedObject == null)
            {
                return false;
            }

            CharacterManager spawnedCharacter = ResolveCharacterManager(
                spawnedObject);

            InitializeSpawnedCharacter(
                spawnedCharacter,
                characterSo);

            if (IsCharacterSpawn(spawnSkill) && spawnLifeTime > 0f)
            {
                Object.Destroy(
                    spawnedObject,
                    spawnLifeTime);
            }

            return true;
        }

        private static SpawnSkillSO ResolveSpawnSkill(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null ||
                runtime.sourceEquipment == null)
            {
                return null;
            }

            return runtime.sourceEquipment.SpawnSkillSo;
        }

        private static CharacterSO ResolveSpawnCharacterSo(
            SpawnSkillSO spawnSkill)
        {
            if (spawnSkill == null)
            {
                return null;
            }

            return spawnSkill.CharacterSO;
        }


        private static int ResolveSpawnCount(
            SpawnSkillSO spawnSkill)
        {
            if (spawnSkill == null)
            {
                return 1;
            }

            return Mathf.Max(
                1,
                spawnSkill.SpawnCount);
        }

        private static float ResolveSpawnInterval(
            SpawnSkillSO spawnSkill)
        {
            if (spawnSkill == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                spawnSkill.SpawnInterval);
        }

        private static float ResolveSpawnLifeTime(
            SpawnSkillSO spawnSkill)
        {
            if (spawnSkill == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                spawnSkill.SpawnLifeTime);
        }


        private static bool IsCharacterSpawn(
            SpawnSkillSO spawnSkill)
        {
            return ResolveSpawnCharacterSo(spawnSkill) != null;
        }

        private static float ResolveSpawnRadius(
            SpawnSkillSO spawnSkill)
        {
            return 0.75f;
        }



        private static Vector3 ResolveSpawnPosition(
            Vector3 origin,
            int index,
            int count,
            float radius)
        {
            if (count <= 1 || radius <= 0f)
            {
                return origin;
            }

            float angle = 360f / count * index;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0f) * radius;

            return origin + offset;
        }

        private static void InitializeSpawnedCharacter(
            CharacterManager spawnedCharacter,
            CharacterSO characterSo)
        {
            if (spawnedCharacter == null || characterSo == null)
            {
                return;
            }

            spawnedCharacter.InitializeFromSO(characterSo);
        }


        public static bool FireProjectiles(
            EquipmentSkillRuntimeData runtime,
            GameObject caster,
            GameObject target,
            Vector2 spawnPosition,
            Vector2 direction,
            Vector2 targetPoint,
            bool usePoint)
        {
            if (runtime == null || caster == null)
            {
                return false;
            }

            ProjectileRuntimeData[] projectileDatas =
                skillResolver.ResolveProjectileRuntime(
                    runtime,
                    caster,
                    target,
                    spawnPosition,
                    direction,
                    targetPoint);

            if (projectileDatas == null || projectileDatas.Length == 0)
            {
                return false;
            }

            ProjectileFactory factory = GetProjectileFactory();

            if (factory == null)
            {
                return false;
            }

            bool firedAny = false;

            for (int i = 0; i < projectileDatas.Length; i++)
            {
                ProjectileRuntimeData projectileData = projectileDatas[i];

                if (projectileData == null)
                {
                    continue;
                }

                if (usePoint && projectileData.moveRuntime != null)
                {
                    projectileData.spawnPosition = targetPoint;
                    projectileData.moveRuntime.targetPosition = targetPoint;
                }

                ProjectileEntity projectile = factory.SpawnOriented(
                    projectileData);

                if (projectile != null)
                {
                    firedAny = true;
                }
            }

            return firedAny;
        }

        public static void ApplyCastSelfEffects(
            EquipmentSkillRuntimeData runtime,
            GameObject caster)
        {
            SkillCastSO castSo = ResolveCastSo(runtime);

            if (castSo == null || caster == null)
            {
                return;
            }

            EffectEntrySO[] selfEffects = castSo.SelfEffects;

            if (selfEffects == null || selfEffects.Length == 0)
            {
                return;
            }

            EffectManager effectManager = ResolveEffectManager(caster);

            CharacterManager casterCharacter = caster.GetComponent<CharacterManager>();

            if (casterCharacter == null)
            {
                casterCharacter = caster.GetComponentInParent<CharacterManager>();
            }

            if (casterCharacter == null)
            {
                casterCharacter = caster.GetComponentInChildren<CharacterManager>();
            }

            if (effectManager == null)
            {
                return;
            }

            EffectEntryRuntime[] resolvedEffects =
                effectResolver.ResolveEntries(
                    selfEffects,
                    caster,
                    caster,
                    EffectCategoryType.Buff,
                    runtime.upgradeRuntimeData?.effectModifiers);

            EffectApplyHelper.ApplyEffects(
                effectManager,
                resolvedEffects);
        }

        public static bool UseSkillProjectilesAndSelfEffects(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            if (runtime == null || caster == null)
            {
                return false;
            }

            Vector2 resolvedTargetPoint = ResolveTargetPoint(
                runtime,
                caster,
                target,
                usePoint,
                targetPoint);

            Vector2 spawnPosition = caster.position;
            Vector2 direction = ResolveDirection(
                spawnPosition,
                caster,
                target,
                true,
                resolvedTargetPoint);

            ApplyCastSelfEffects(
                runtime,
                caster.gameObject);

            return FireProjectiles(
                runtime,
                caster.gameObject,
                target != null ? target.gameObject : null,
                spawnPosition,
                direction,
                resolvedTargetPoint,
                usePoint);
        }

        public static Vector2 ResolveTargetPoint(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            SkillCastSO castSo = ResolveCastSo(runtime);

            if (castSo == null)
            {
                return usePoint
                    ? targetPoint
                    : caster != null
                        ? (Vector2)caster.position
                        : Vector2.zero;
            }

            TargetingType targetingType = castSo.TargetingType;

            if (targetingType == TargetingType.Self ||
                targetingType == TargetingType.None)
            {
                return caster != null
                    ? (Vector2)caster.position
                    : Vector2.zero;
            }

            if (usePoint)
            {
                return targetPoint;
            }

            if (target != null)
            {
                return target.position;
            }

            return caster != null
                ? (Vector2)caster.position
                : Vector2.zero;
        }

        public static Vector2 ResolveDirection(
            Vector2 spawnPosition,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            Vector2 direction = caster != null
                ? (Vector2)caster.right
                : Vector2.right;

            if (target != null)
            {
                Vector2 toTarget = (Vector2)target.position - spawnPosition;

                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    direction = toTarget.normalized;
                }
            }
            else if (usePoint)
            {
                Vector2 toPoint = targetPoint - spawnPosition;

                if (toPoint.sqrMagnitude > 0.0001f)
                {
                    direction = toPoint.normalized;
                }
            }

            return direction;
        }

        private static SkillCastSO ResolveCastSo(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null || runtime.sourceEquipment == null)
            {
                return null;
            }

            return runtime.sourceEquipment.CastSo;
        }



        private static EffectManager ResolveEffectManager(GameObject caster)
        {
            if (caster == null)
            {
                return null;
            }

            EffectManager effectManager = caster.GetComponent<EffectManager>();

            if (effectManager != null)
            {
                return effectManager;
            }

            effectManager = caster.GetComponentInParent<EffectManager>();

            if (effectManager != null)
            {
                return effectManager;
            }

            return caster.GetComponentInChildren<EffectManager>();
        }

        private static CharacterManager ResolveCharacterManager(GameObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            CharacterManager manager = obj.GetComponent<CharacterManager>();

            if (manager != null)
            {
                return manager;
            }

            manager = obj.GetComponentInParent<CharacterManager>();

            if (manager != null)
            {
                return manager;
            }

            return obj.GetComponentInChildren<CharacterManager>();
        }

    }
}