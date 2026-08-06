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

    public void Initialize(
        ProjectileEntity projectile,
        ProjectileRuntimeData runtimeData)
    {
        ownerProjectile = projectile;
        ownerRuntimeData = runtimeData;
    }

    public bool TrySpawnChildSkill(
        SpawnSkillTiming timing)
    {
        if (timing != SpawnSkillTiming.OnHit)
        {
            return false;
        }

        return SpawnChildSkillOnce(
            ResolveSpawnSkill());
    }

    private bool SpawnChildSkillOnce(
        EquipmentSkillSO spawnSkill)
    {
        if (!CanSpawnChildSkill(spawnSkill))
        {
            return false;
        }

        EquipmentSkillRuntimeData childRuntime = skillResolver.Resolve(
            spawnSkill,
            null);

        if (childRuntime == null)
        {
            return false;
        }

        Transform spawnTransform = ownerProjectile != null
            ? ownerProjectile.transform
            : transform;

        Vector2 spawnPosition = spawnTransform.position;

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
        EquipmentSkillSO spawnSkill)
    {
        return ownerProjectile != null &&
               ownerRuntimeData != null &&
               spawnSkill != null;
    }

    private EquipmentSkillSO ResolveSpawnSkill()
    {
        return ownerRuntimeData != null && ownerRuntimeData.hit != null
            ? ownerRuntimeData.hit.spawnSkill
            : null;
    }
}