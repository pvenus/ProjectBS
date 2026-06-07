using Skill;
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
    private SpawnSkillSO activeSpawnSkillSo;
    private int spawnedCount;
    private float elapsed;
    private bool isRunning;

    public void Initialize(
        ProjectileEntity projectile,
        ProjectileRuntimeData runtimeData)
    {
        ownerProjectile = projectile;
        ownerRuntimeData = runtimeData;

        StopSpawnTimer();

        if (ownerProjectile == null || ownerRuntimeData == null)
        {
            return;
        }

        StartSpawnTimer(ownerRuntimeData.spawnSkillSo);
    }

    private void Update()
    {
        TickSpawnTimer(Time.deltaTime);
    }

    private void TickSpawnTimer(float deltaTime)
    {
        if (!isRunning)
        {
            return;
        }

        if (!CanSpawnChildSkill(activeSpawnSkillSo))
        {
            StopSpawnTimer();
            return;
        }

        elapsed += deltaTime;

        float interval = Mathf.Max(0f, activeSpawnSkillSo.SpawnInterval);

        if (elapsed < interval)
        {
            return;
        }

        elapsed = 0f;

        if (SpawnChildSkillOnce(activeSpawnSkillSo))
        {
            spawnedCount++;
        }

        if (spawnedCount >= Mathf.Max(1, activeSpawnSkillSo.SpawnCount))
        {
            StopSpawnTimer();
        }
    }

    private void StartSpawnTimer(
        SpawnSkillSO spawnSkillSo)
    {
        if (!CanSpawnChildSkill(spawnSkillSo))
        {
            return; 
        }

        switch (spawnSkillSo.Timing)
        {
            case SpawnSkillTiming.OnCast:
                SpawnChildSkillOnce(spawnSkillSo);
                break;

            case SpawnSkillTiming.OnInterval:
                RegisterIntervalSpawn(spawnSkillSo);
                break;
        }
    }

    private void RegisterIntervalSpawn(
        SpawnSkillSO spawnSkillSo)
    {
        if (!CanSpawnChildSkill(spawnSkillSo))
        {
            return;
        }

        activeSpawnSkillSo = spawnSkillSo;
        spawnedCount = 0;
        elapsed = 0f;
        isRunning = true;

        // First spawn happens immediately on registration.
        if (SpawnChildSkillOnce(activeSpawnSkillSo))
        {
            spawnedCount = 1;
        }

        if (spawnedCount >= Mathf.Max(1, activeSpawnSkillSo.SpawnCount))
        {
            StopSpawnTimer();
        }
    }

    public bool TrySpawnChildSkill(
        SpawnSkillTiming timing)
    {
        if (ownerRuntimeData == null)
        {
            return false;
        }

        SpawnSkillSO spawnSkillSo = ownerRuntimeData.spawnSkillSo;

        if (!CanSpawnChildSkill(spawnSkillSo) || spawnSkillSo.Timing != timing)
        {
            return false;
        }

        switch (spawnSkillSo.Timing)
        {
            case SpawnSkillTiming.OnHit:
            case SpawnSkillTiming.OnProjectileEnd:
                return SpawnChildSkillOnce(spawnSkillSo);

            case SpawnSkillTiming.OnInterval:
                RegisterIntervalSpawn(spawnSkillSo);
                return true;

            case SpawnSkillTiming.OnCast:
                return SpawnChildSkillOnce(spawnSkillSo);

            case SpawnSkillTiming.None:
            default:
                return false;
        }
    }

    private bool SpawnChildSkillOnce(
        SpawnSkillSO spawnSkillSo)
    {
        if (!CanSpawnChildSkill(spawnSkillSo))
        {
            return false;
        }

        EquipmentSkillRuntimeData childRuntime = skillResolver.Resolve(
            spawnSkillSo.Skill,
            null);

        if (childRuntime == null)
        {
            return false;
        }

        Vector2 spawnPosition = ResolveChildSpawnPosition(
            spawnSkillSo.Position);

        Vector2 direction = ResolveChildDirection();

        ProjectileRuntimeData[] childProjectiles =
            skillResolver.ResolveProjectileRuntime(
                childRuntime,
                ownerRuntimeData.owner,
                ownerRuntimeData.target,
                spawnPosition,
                direction,
                spawnPosition);

        if (childProjectiles == null || childProjectiles.Length == 0)
        {
            return false;
        }

        bool spawnedAny = false;

        for (int i = 0; i < childProjectiles.Length; i++)
        {
            ProjectileRuntimeData childProjectile = childProjectiles[i];

            if (childProjectile == null)
            {
                continue;
            }

            ProjectileEntity prefab = childProjectile.projectilePrefab != null
                ? childProjectile.projectilePrefab
                : childRuntime.projectilePrefab;

            if (prefab == null)
            {
                continue;
            }

            ProjectileEntity projectile = Instantiate(
                prefab,
                childProjectile.spawnPosition,
                Quaternion.identity);

            projectile.Initialize(childProjectile);
            spawnedAny = true;
        }

        return spawnedAny;
    }

    private bool CanSpawnChildSkill(
        SpawnSkillSO spawnSkillSo)
    {
        return ownerProjectile != null &&
               ownerRuntimeData != null &&
               spawnSkillSo != null &&
               spawnSkillSo.Skill != null;
    }

    private void StopSpawnTimer()
    {
        activeSpawnSkillSo = null;
        spawnedCount = 0;
        elapsed = 0f;
        isRunning = false;
    }

    private Vector2 ResolveChildSpawnPosition(
        SpawnSkillPosition position)
    {
        switch (position)
        {
            case SpawnSkillPosition.Caster:
                if (ownerRuntimeData.owner != null)
                {
                    return ownerRuntimeData.owner.transform.position;
                }

                return ownerRuntimeData.spawnPosition;

            case SpawnSkillPosition.Target:
                if (ownerRuntimeData.target != null)
                {
                    return ownerRuntimeData.target.transform.position;
                }

                return transform.position;

            case SpawnSkillPosition.ProjectilePosition:
            case SpawnSkillPosition.HitPoint:
                return transform.position;

            default:
                return transform.position;
        }
    }

    private Vector2 ResolveChildDirection()
    {
        if (ownerRuntimeData.direction.sqrMagnitude > 0.0001f)
        {
            return ownerRuntimeData.direction.normalized;
        }

        Vector2 right = transform.right;

        if (right.sqrMagnitude > 0.0001f)
        {
            return right.normalized;
        }

        return Vector2.right;
    }
}