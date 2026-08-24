using Skill;
using Skill.Service.Helper;
using UnityEngine;

/// <summary>
/// ProjectileEntity 내부 컴포넌트.
///
/// 부모 투사체의 RuntimeData.spawnSkillSo를 읽고,
/// 자신의 생명주기 동안 정해진 타이밍에 자식 스킬 투사체를 생성한다.
/// 외부 스킬 사용 로직은 ProjectileFactory로 부모 투사체를 생성만 하고,
/// 이 컴포넌트는 ProjectileEntity.Initialize(...) 이후 내부 동작만 담당한다.
/// </summary>
public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private ProjectileEntity ownerProjectile;

    private readonly EquipmentSkillResolver skillResolver = new();

    private ProjectileRuntimeData ownerRuntimeData;
    private SpawnSkillSO activeSpawnSkill;
    private int spawnedCount;
    private float elapsed;

    public void Initialize(
        ProjectileEntity projectile,
        ProjectileRuntimeData runtimeData)
    {
        ownerProjectile = projectile;
        ownerRuntimeData = runtimeData;
        StopIntervalSpawn();

        SpawnSkillSO spawnSkill = ResolveSpawnSkill();
        if (spawnSkill == null || spawnSkill.Skill == null)
        {
            return;
        }

        if (spawnSkill.Timing == SpawnSkillTiming.OnCast)
        {
            SpawnChildSkillOnce(spawnSkill);
        }
        else if (spawnSkill.Timing == SpawnSkillTiming.OnInterval)
        {
            StartIntervalSpawn(spawnSkill);
        }
    }

    private void Update()
    {
        if (activeSpawnSkill == null)
        {
            return;
        }

        elapsed += Time.deltaTime;
        if (elapsed < activeSpawnSkill.SpawnInterval)
        {
            return;
        }

        elapsed = 0f;
        if (SpawnChildSkillOnce(activeSpawnSkill))
        {
            spawnedCount++;
        }

        if (spawnedCount >= activeSpawnSkill.SpawnCount)
        {
            StopIntervalSpawn();
        }
    }

    public bool TrySpawnChildSkill(
        SpawnSkillTiming timing)
    {
        SpawnSkillSO spawnSkill = ResolveSpawnSkill();
        if (spawnSkill == null || spawnSkill.Timing != timing)
        {
            return false;
        }

        return SpawnChildSkillOnce(spawnSkill);
    }

    private bool SpawnChildSkillOnce(
        SpawnSkillSO spawnConfig)
    {
        if (!CanSpawnChildSkill(spawnConfig))
        {
            return false;
        }

        EquipmentSkillRuntimeData childRuntime = skillResolver.Resolve(
            spawnConfig.Skill,
            null);

        if (childRuntime == null)
        {
            return false;
        }

        Transform spawnTransform = ownerProjectile != null
            ? ownerProjectile.transform
            : transform;

        Vector2 spawnPosition = ResolveSpawnPosition(spawnConfig.Position);

        Transform targetTransform = ownerRuntimeData.target != null
            ? ownerRuntimeData.target.transform
            : null;

        SkillUseHelper.UseSkillProjectilesAndSelfEffects(
            childRuntime,
            spawnTransform,
            targetTransform,
            false,
            spawnPosition);

        return true;
    }

    private bool CanSpawnChildSkill(
        SpawnSkillSO spawnSkill)
    {
        return ownerProjectile != null &&
               ownerRuntimeData != null &&
               spawnSkill != null &&
               spawnSkill.Skill != null;
    }

    private SpawnSkillSO ResolveSpawnSkill()
    {
        return ownerRuntimeData != null
            ? ownerRuntimeData.spawnSkillSo
            : null;
    }

    private void StartIntervalSpawn(SpawnSkillSO spawnSkill)
    {
        activeSpawnSkill = spawnSkill;
        spawnedCount = 0;
        elapsed = 0f;

        if (SpawnChildSkillOnce(spawnSkill))
        {
            spawnedCount = 1;
        }

        if (spawnedCount >= spawnSkill.SpawnCount)
        {
            StopIntervalSpawn();
        }
    }

    private void StopIntervalSpawn()
    {
        activeSpawnSkill = null;
        spawnedCount = 0;
        elapsed = 0f;
    }

    private Vector2 ResolveSpawnPosition(SpawnSkillPosition position)
    {
        switch (position)
        {
            case SpawnSkillPosition.Caster:
                return ownerRuntimeData.owner != null
                    ? ownerRuntimeData.owner.transform.position
                    : ownerRuntimeData.spawnPosition;
            case SpawnSkillPosition.Target:
                return ownerRuntimeData.target != null
                    ? ownerRuntimeData.target.transform.position
                    : transform.position;
            case SpawnSkillPosition.HitPoint:
            case SpawnSkillPosition.ProjectilePosition:
            default:
                return transform.position;
        }
    }
}
