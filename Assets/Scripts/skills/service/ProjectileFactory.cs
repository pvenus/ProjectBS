using System.Collections;
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
        if (ShouldSpawnWithInterval(runtimeData, count))
        {
            GetRunner().StartCoroutine(
                SpawnRoutine(
                    prefab,
                    runtimeData,
                    null,
                    false));

            return null;
        }

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
        if (ShouldSpawnWithInterval(runtimeData, count))
        {
            GetRunner().StartCoroutine(
                SpawnRoutine(
                    prefab,
                    runtimeData,
                    parent,
                    false));

            return null;
        }

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
        if (ShouldSpawnWithInterval(runtimeData, count))
        {
            GetRunner().StartCoroutine(
                SpawnRoutine(
                    prefab,
                    runtimeData,
                    null,
                    true));

            return null;
        }

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
    private bool ShouldSpawnWithInterval(ProjectileRuntimeData runtimeData, int count)
    {
        return runtimeData != null
            && count > 1
            && runtimeData.damageProfile != null
            && runtimeData.damageProfile.projectileSpawnInterval > 0f;
    }

    private IEnumerator SpawnRoutine(
        ProjectileEntity prefab,
        ProjectileRuntimeData runtimeData,
        Transform parent,
        bool oriented)
    {
        int count = Mathf.Max(1, runtimeData.projectileCount);
        float interval = Mathf.Max(
            0f,
            runtimeData.damageProfile.projectileSpawnInterval);

        Quaternion rotation = Quaternion.identity;

        if (oriented)
        {
            Vector2 direction = runtimeData.NormalizedDirection;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0f, 0f, angle);
        }

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);

            ProjectileEntity instance = parent != null
                ? Object.Instantiate(prefab, instanceData.spawnPosition, rotation, parent)
                : Object.Instantiate(prefab, instanceData.spawnPosition, rotation);

            instance.transform.localScale =
                Vector3.one * Mathf.Max(0.01f, instanceData.projectileScale);

            instance.Initialize(instanceData);

            if (i < count - 1 && interval > 0f)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private ProjectileFactoryRunner GetRunner()
    {
        ProjectileFactoryRunner runner =
            Object.FindFirstObjectByType<ProjectileFactoryRunner>();

        if (runner != null)
        {
            return runner;
        }

        GameObject runnerObject = new GameObject("ProjectileFactoryRunner");
        Object.DontDestroyOnLoad(runnerObject);

        return runnerObject.AddComponent<ProjectileFactoryRunner>();
    }

    private class ProjectileFactoryRunner : MonoBehaviour
    {
    }

    private Vector2 ResolveSpawnPosition(ProjectileRuntimeData source, int spawnOrder)
    {
        if (source == null)
        {
            return Vector2.zero;
        }

        float radius = Mathf.Max(0f, source.projectileSpawnRadius);
        int count = Mathf.Max(1, source.projectileCount);

        if (radius <= 0f || count <= 1)
        {
            return source.spawnPosition;
        }

        Vector2[] positions = BuildRandomRadiusPatternPositions(
            source.spawnPosition,
            radius,
            count);

        int index = Mathf.Clamp(
            spawnOrder,
            0,
            positions.Length - 1);

        return positions[index];
    }

    private Vector2[] BuildRandomRadiusPatternPositions(
        Vector2 center,
        float radius,
        int count)
    {
        count = Mathf.Max(1, count);
        radius = Mathf.Max(0f, radius);

        Vector2[] positions = new Vector2[count];

        if (radius <= 0f || count <= 1)
        {
            for (int i = 0; i < count; i++)
            {
                positions[i] = center;
            }

            return positions;
        }

        int innerCount = Mathf.Clamp(
            Mathf.RoundToInt(count * 0.25f),
            0,
            count);

        if (count >= 3)
        {
            innerCount = Mathf.Max(1, innerCount);
        }

        int outerCount = count - innerCount;
        int writeIndex = 0;

        FillRingPositions(
            positions,
            ref writeIndex,
            center,
            radius * 0.35f,
            innerCount,
            0f,
            28f);

        FillRingPositions(
            positions,
            ref writeIndex,
            center,
            radius,
            outerCount,
            360f / Mathf.Max(1, count) * 0.5f,
            20f);

        ShufflePositions(positions);

        return positions;
    }

    private void FillRingPositions(
        Vector2[] positions,
        ref int writeIndex,
        Vector2 center,
        float distance,
        int count,
        float angleOffset,
        float angleJitter)
    {
        if (positions == null || count <= 0)
        {
            return;
        }

        float step = 360f / Mathf.Max(1, count);

        for (int i = 0; i < count && writeIndex < positions.Length; i++)
        {
            float angle = angleOffset
                + step * i
                + Random.Range(-angleJitter, angleJitter);

            float distanceJitter = Random.Range(0.85f, 1f);
            Vector2 direction = DirectionFromAngle(angle);

            positions[writeIndex] =
                center + direction * distance * distanceJitter;

            writeIndex++;
        }
    }

    private Vector2 DirectionFromAngle(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad));
    }

    private void ShufflePositions(Vector2[] positions)
    {
        if (positions == null || positions.Length <= 1)
        {
            return;
        }

        for (int i = positions.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            Vector2 temp = positions[i];
            positions[i] = positions[j];
            positions[j] = temp;
        }
    }

    private ProjectileRuntimeData CreateInstanceRuntimeData(ProjectileRuntimeData source, int spawnOrder)
    {
        var data = new ProjectileRuntimeData
        {
            owner = source.owner,
            target = source.target,
            spawnPosition = ResolveSpawnPosition(source, spawnOrder),
            direction = source.direction,
            lifetime = source.lifetime,

            projectileCount = source.projectileCount,
            projectileScale = source.projectileScale,
            spawnOrder = spawnOrder,
            projectileSpawnInterval = source.projectileSpawnInterval,
            projectileSpawnRadius = source.projectileSpawnRadius,

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