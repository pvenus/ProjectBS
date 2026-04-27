using UnityEngine;

/// <summary>
/// ProjectileEntity 생성 전용 팩토리.
/// 현재는 Instantiate 기반의 최소 구현이며,
/// 이후 풀링 / 멀티샷 / 분열탄 / 패턴 발사 확장 지점으로 사용한다.
/// </summary>
public class ProjectileFactory
{
    /// <summary>
    /// 지정된 프리팹으로 투사체를 생성하고 런타임 데이터를 주입한다.
    /// </summary>
    public ProjectileEntity Spawn(ProjectileEntity prefab, ProjectileRuntimeData runtimeData)
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectileFactory.Spawn failed: prefab is null.");
            return null;
        }

        if (runtimeData == null)
        {
            Debug.LogError("ProjectileFactory.Spawn failed: runtimeData is null.");
            return null;
        }

        ProjectileEntity firstInstance = null;
        int count = Mathf.Max(1, runtimeData.projectileCount);

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);

            ProjectileEntity instance = Object.Instantiate(prefab, instanceData.spawnPosition, Quaternion.identity);
            instance.transform.localScale = Vector3.one * Mathf.Max(0.01f, instanceData.projectileScale);
            instance.Initialize(instanceData);

            if (firstInstance == null)
            {
                firstInstance = instance;
            }
        }

        return firstInstance;
    }

    /// <summary>
    /// 부모 Transform 아래에 투사체를 생성하고 런타임 데이터를 주입한다.
    /// </summary>
    public ProjectileEntity Spawn(ProjectileEntity prefab, ProjectileRuntimeData runtimeData, Transform parent)
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectileFactory.Spawn failed: prefab is null.");
            return null;
        }

        if (runtimeData == null)
        {
            Debug.LogError("ProjectileFactory.Spawn failed: runtimeData is null.");
            return null;
        }

        ProjectileEntity firstInstance = null;
        int count = Mathf.Max(1, runtimeData.projectileCount);

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);

            ProjectileEntity instance = Object.Instantiate(prefab, instanceData.spawnPosition, Quaternion.identity, parent);
            instance.transform.localScale = Vector3.one * Mathf.Max(0.01f, instanceData.projectileScale);
            instance.Initialize(instanceData);

            if (firstInstance == null)
            {
                firstInstance = instance;
            }
        }

        return firstInstance;
    }

    /// <summary>
    /// 발사 방향을 기준으로 회전을 적용해 투사체를 생성한다.
    /// 2D 게임에서 Sprite 방향 정렬이 필요한 경우 사용한다.
    /// </summary>
    public ProjectileEntity SpawnOriented(ProjectileEntity prefab, ProjectileRuntimeData runtimeData)
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectileFactory.SpawnOriented failed: prefab is null.");
            return null;
        }

        if (runtimeData == null)
        {
            Debug.LogError("ProjectileFactory.SpawnOriented failed: runtimeData is null.");
            return null;
        }

        Vector2 direction = runtimeData.NormalizedDirection;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        ProjectileEntity firstInstance = null;
        int count = Mathf.Max(1, runtimeData.projectileCount);

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);

            ProjectileEntity instance = Object.Instantiate(prefab, instanceData.spawnPosition, rotation);
            instance.transform.localScale = Vector3.one * Mathf.Max(0.01f, instanceData.projectileScale);
            instance.Initialize(instanceData);

            if (firstInstance == null)
            {
                firstInstance = instance;
            }
        }


        return firstInstance;
    }
    private ProjectileRuntimeData CreateInstanceRuntimeData(ProjectileRuntimeData source, int spawnOrder)
    {
        var data = new ProjectileRuntimeData
        {
            owner = source.owner,
            target = source.target,
            spawnPosition = source.spawnPosition,
            direction = source.direction,
            lifetime = source.lifetime,

            projectileCount = source.projectileCount,
            projectileScale = source.projectileScale,
            spawnOrder = spawnOrder,

            move = source.move,
            hit = source.hit,
            damageProfile = source.damageProfile,
            visualContext = source.visualContext,

            spawnClip = source.spawnClip,
            hitClip = source.hitClip,
            despawnClip = source.despawnClip,
            sprite = source.sprite,
            material = source.material,
            color = source.color,
            useAnimatorTriggers = source.useAnimatorTriggers
        };

        return data;
    }
}