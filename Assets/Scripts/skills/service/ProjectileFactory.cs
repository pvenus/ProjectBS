using System.Collections;
using UnityEngine;
using Skill;
using Skills.Dto.Move;
/// <summary>
/// ProjectileEntity 생성 전용 팩토리.
/// 프리팹 Instantiate 없이 GameObject를 코드에서 생성한다.
/// ProjectileEntity가 자신이 관장하는 컴포넌트를 Awake에서 보장한다.
/// </summary>
public class ProjectileFactory
{
    /// <summary>
    /// 투사체 GameObject를 생성하고 런타임 데이터를 주입한다.
    /// </summary>
    public ProjectileEntity Spawn(ProjectileRuntimeData runtimeData)
    {
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
                    runtimeData,
                    null,
                    false));

            return null;
        }

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);

            ProjectileEntity instance = CreateProjectileInstance(
                instanceData,
                Quaternion.identity,
                null);
            ConfigureSpawnedProjectile(instance, instanceData);
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
    public ProjectileEntity Spawn(ProjectileRuntimeData runtimeData, Transform parent)
    {
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
                    runtimeData,
                    parent,
                    false));

            return null;
        }

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);

            ProjectileEntity instance = CreateProjectileInstance(
                instanceData,
                Quaternion.identity,
                parent);
            ConfigureSpawnedProjectile(instance, instanceData);
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
    public ProjectileEntity SpawnOriented(ProjectileRuntimeData runtimeData)
    {
        if (runtimeData == null)
        {
            Debug.LogError("ProjectileFactory.SpawnOriented failed: runtimeData is null.");
            return null;
        }


        ProjectileEntity firstInstance = null;
        int count = Mathf.Max(1, runtimeData.projectileCount);
        if (ShouldSpawnWithInterval(runtimeData, count))
        {
            GetRunner().StartCoroutine(
                SpawnRoutine(
                    runtimeData,
                    null,
                    true));

            return null;
        }

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);
            Quaternion instanceRotation = ResolveSpawnRotation(instanceData);
            ProjectileEntity instance = CreateProjectileInstance(
                instanceData,
                instanceRotation,
                null);
            ConfigureSpawnedProjectile(instance, instanceData);
            instance.Initialize(instanceData);

            if (firstInstance == null)
            {
                firstInstance = instance;
            }
        }


        return firstInstance;
    }
    private ProjectileEntity CreateProjectileInstance(
        ProjectileRuntimeData runtimeData,
        Quaternion rotation,
        Transform parent)
    {
        GameObject projectileObject = new GameObject("ProjectileEntity");

        if (parent != null)
        {
            projectileObject.transform.SetParent(parent, false);
        }

        projectileObject.transform.position = runtimeData.spawnPosition;
        projectileObject.transform.rotation = rotation;

        return projectileObject.AddComponent<ProjectileEntity>();
    }

    private void ConfigureSpawnedProjectile(
        ProjectileEntity instance,
        ProjectileRuntimeData runtimeData)
    {
        if (instance == null || runtimeData == null)
        {
            return;
        }

        float scale = Mathf.Max(0.01f, runtimeData.projectileScale);
        instance.transform.localScale = Vector3.one * scale;
    }

    private Quaternion ResolveSpawnRotation(ProjectileRuntimeData runtimeData)
    {
        if (runtimeData == null || runtimeData.moveRuntime == null)
        {
            return Quaternion.identity;
        }

        if (!runtimeData.moveRuntime.applyDirectionRotation)
        {
            return Quaternion.identity;
        }

        Vector2 direction = runtimeData.NormalizedDirection;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Quaternion.identity;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += runtimeData.moveRuntime.rotationOffset;

        return Quaternion.Euler(0f, 0f, angle);
    }
    private bool ShouldSpawnWithInterval(ProjectileRuntimeData runtimeData, int count)
    {
        return runtimeData != null
            && count > 1
            && runtimeData.projectileSpawnInterval > 0f;
    }

    private IEnumerator SpawnRoutine(
        ProjectileRuntimeData runtimeData,
        Transform parent,
        bool oriented)
    {
        int count = Mathf.Max(1, runtimeData.projectileCount);
        float interval = Mathf.Max(
            0f,
            runtimeData.projectileSpawnInterval);

        // Removed shared rotation calculation. Each instance will have its own rotation.

        for (int i = 0; i < count; i++)
        {
            var instanceData = CreateInstanceRuntimeData(runtimeData, i);
            Quaternion instanceRotation = oriented
                ? ResolveSpawnRotation(instanceData)
                : Quaternion.identity;

            ProjectileEntity instance = CreateProjectileInstance(
                instanceData,
                instanceRotation,
                parent);

            ConfigureSpawnedProjectile(instance, instanceData);

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

        ProjectileArrangementType arrangement =
            ResolveProjectileArrangement(source);

        switch (arrangement)
        {
            case ProjectileArrangementType.Line:
                return ResolveLineSpawnPosition(
                    source,
                    spawnOrder);

            case ProjectileArrangementType.Circle:
                return ResolveCircleSpawnPosition(
                    source,
                    spawnOrder);

            case ProjectileArrangementType.Spread:
            case ProjectileArrangementType.Single:
            default:
                return ResolveLegacyRadiusSpawnPosition(
                    source,
                    spawnOrder);
        }
    }

    private Vector2 ResolveLineSpawnPosition(
        ProjectileRuntimeData source,
        int spawnOrder)
    {
        if (source == null)
        {
            return Vector2.zero;
        }

        int count = Mathf.Max(1, source.projectileCount);
        float spacing = ResolveArrangementValue(source);

        if (count <= 1 || spacing <= 0f)
        {
            return source.spawnPosition;
        }

        Vector2 direction =
            source.direction.sqrMagnitude <= 0.0001f
                ? Vector2.right
                : source.direction.normalized;

        Vector2 perpendicular =
            new Vector2(-direction.y, direction.x);

        int safeOrder = Mathf.Clamp(
            spawnOrder,
            0,
            count - 1);

        float centerOffset = (count - 1) * 0.5f;
        float offset = (safeOrder - centerOffset) * spacing;

        return source.spawnPosition + perpendicular * offset;
    }

    private Vector2 ResolveCircleSpawnPosition(
        ProjectileRuntimeData source,
        int spawnOrder)
    {
        if (source == null)
        {
            return Vector2.zero;
        }

        float radius = ResolveArrangementValue(source);

        if (radius <= 0f)
        {
            radius = Mathf.Max(0f, source.projectileSpawnRadius);
        }

        int count = Mathf.Max(1, source.projectileCount);

        if (radius <= 0f || count <= 1)
        {
            return source.spawnPosition;
        }

        float angle = 360f / count * Mathf.Clamp(spawnOrder, 0, count - 1);
        Vector2 direction = DirectionFromAngle(angle);

        return source.spawnPosition + direction * radius;
    }

    private Vector2 ResolveLegacyRadiusSpawnPosition(
        ProjectileRuntimeData source,
        int spawnOrder)
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

    private float ResolveArrangementValue(
        ProjectileRuntimeData source)
    {
        if (source == null)
        {
            return 0f;
        }

        return Mathf.Max(0f, source.projectileArrangementValue);
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

    private Vector2 ResolveProjectileDirection(
        ProjectileRuntimeData source,
        int spawnOrder)
    {
        if (source == null)
        {
            return Vector2.right;
        }

        Vector2 baseDirection =
            source.direction.sqrMagnitude <= 0.0001f
                ? Vector2.right
                : source.direction.normalized;

        if (source.moveRuntime != null
            && source.moveRuntime.MoveType == ProjectileMoveType.Homing)
        {
            return -baseDirection;
        }

        ProjectileArrangementType arrangement =
            ResolveProjectileArrangement(source);

        switch (arrangement)
        {
            case ProjectileArrangementType.Spread:
                return ResolveSpreadProjectileDirection(
                    baseDirection,
                    source,
                    spawnOrder);

            case ProjectileArrangementType.Line:
            case ProjectileArrangementType.Circle:
            case ProjectileArrangementType.Single:
            default:
                return baseDirection;
        }
    }

    private ProjectileArrangementType ResolveProjectileArrangement(
        ProjectileRuntimeData source)
    {
        if (source == null)
        {
            return ProjectileArrangementType.Single;
        }

        if (source.projectileArrangement != ProjectileArrangementType.Single)
        {
            return source.projectileArrangement;
        }

        int projectileCount = Mathf.Max(1, source.projectileCount);
        float spreadAngle = Mathf.Max(0f, source.projectileSpreadAngle);

        if (projectileCount > 1 && spreadAngle > 0f)
        {
            return ProjectileArrangementType.Spread;
        }

        return ProjectileArrangementType.Single;
    }

    private Vector2 ResolveSpreadProjectileDirection(
        Vector2 baseDirection,
        ProjectileRuntimeData source,
        int spawnOrder)
    {
        int projectileCount = Mathf.Max(1, source.projectileCount);
        float spreadAngle = ResolveSpreadAngle(source);

        if (projectileCount <= 1 || spreadAngle <= 0f)
        {
            return baseDirection;
        }

        float angle = EvaluateSpreadAngle(
            spawnOrder,
            projectileCount,
            spreadAngle);

        return RotateDirection(baseDirection, angle);
    }

    private float ResolveSpreadAngle(
        ProjectileRuntimeData source)
    {
        if (source == null)
        {
            return 0f;
        }

        if (source.projectileArrangement == ProjectileArrangementType.Spread &&
            source.projectileArrangementValue > 0f)
        {
            int projectileCount = Mathf.Max(1, source.projectileCount);

            return source.projectileArrangementValue *
                   Mathf.Max(0, projectileCount - 1);
        }

        return Mathf.Max(0f, source.projectileSpreadAngle);
    }

    private Vector2 RotateDirection(
        Vector2 direction,
        float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos);
    }

    private ProjectileRuntimeData CreateInstanceRuntimeData(ProjectileRuntimeData source, int spawnOrder)
    {
        var spawnPosition = ResolveSpawnPosition(source, spawnOrder);
        var data = new ProjectileRuntimeData
        {
            owner = source.owner,
            target = source.target,
            spawnPosition = spawnPosition,
            direction = ResolveProjectileDirection(
                source,
                spawnOrder),
            targetingType = source.targetingType,
            lifetime = source.lifetime,

            projectileCount = source.projectileCount,
            projectileSpreadAngle = source.projectileSpreadAngle,
            projectileArrangement = source.projectileArrangement,
            projectileArrangementValue = source.projectileArrangementValue,
            projectileScale = source.projectileScale,
            spawnOrder = spawnOrder,
            projectileSpawnInterval = source.projectileSpawnInterval,
            projectileSpawnRadius = source.projectileSpawnRadius,

            moveRuntime = CreateInstanceMoveRuntimeDto(
                source,
                spawnOrder,
                spawnPosition),
            hit = source.hit,
            damageProfile = source.damageProfile,
            visualContext = source.visualContext,
            spawnSkillSo = source.spawnSkillSo,

            spawnClip = source.spawnClip,
            hitClip = source.hitClip,
            despawnClip = source.despawnClip,
            material = source.material,
            color = source.color,
            projectileVisualType = source.projectileVisualType,
            useAnimatorTriggers = source.useAnimatorTriggers
        };

        return data;
    }

    private SkillMoveRuntimeDto CreateInstanceMoveRuntimeDto(
        ProjectileRuntimeData source,
        int spawnOrder,
        Vector2 spawnPosition)
    {
        if (source == null || source.moveRuntime == null)
        {
            return source?.moveRuntime;
        }

        if (source.moveRuntime is LinearProjectileMoveDto linear)
        {
            return CreateInstanceLinearMoveDto(
                source,
                linear,
                spawnOrder,
                spawnPosition);
        }

        if (source.moveRuntime is WarpProjectileMoveDto warp)
        {
            return CreateInstanceWarpMoveDto(
                warp,
                spawnPosition);
        }

        return source.moveRuntime;
    }

    private WarpProjectileMoveDto CreateInstanceWarpMoveDto(
        WarpProjectileMoveDto sourceMove,
        Vector2 spawnPosition)
    {
        if (sourceMove == null)
        {
            return null;
        }

        return new WarpProjectileMoveDto
        {
            targetPosition = spawnPosition
        };
    }

    private LinearProjectileMoveDto CreateInstanceLinearMoveDto(
        ProjectileRuntimeData source,
        LinearProjectileMoveDto sourceMove,
        int spawnOrder,
        Vector2 spawnPosition)
    {
        LinearProjectileMoveDto move = CloneLinearMoveDto(sourceMove);
        move.startPosition = spawnPosition;

        if (source.targetingType == TargetingType.AutoTargetDirection ||
            source.targetingType == TargetingType.Directional)
        {
            move.targetPosition = ResolveDirectionBasedDestination(
                source,
                move,
                spawnPosition);
        }
        else if (source.targetingType == TargetingType.Position)
        {
            move.targetPosition = sourceMove.targetPosition;
        }

        int projectileCount = Mathf.Max(1, source.projectileCount);
        float spreadAngle = Mathf.Max(0f, source.projectileSpreadAngle);

        if (projectileCount <= 1 || spreadAngle <= 0f)
        {
            return move;
        }

        Vector2 baseDirection = ResolveLinearBaseDirection(
            move,
            source,
            spawnPosition);

        Vector2 spreadDirection = RotateDirection(
            baseDirection,
            EvaluateSpreadAngle(
                spawnOrder,
                projectileCount,
                spreadAngle));

        float distance = source.targetingType == TargetingType.AutoTargetDirection ||
            source.targetingType == TargetingType.Directional
                ? Vector2.Distance(
                    spawnPosition,
                    ResolveDirectionBasedDestination(source, move, spawnPosition))
                : ResolveLinearDistance(
                    move,
                    spawnPosition);

        move.targetPosition = spawnPosition + spreadDirection * distance;

        return move;
    }

    private LinearProjectileMoveDto CloneLinearMoveDto(LinearProjectileMoveDto source)
    {
        if (source == null)
        {
            return null;
        }

        return new LinearProjectileMoveDto
        {
            startPosition = source.startPosition,
            targetPosition = source.targetPosition,
            speed = source.speed,
            applyDirectionRotation = source.applyDirectionRotation,
            rotationOffset = source.rotationOffset
        };
    }
    private Vector2 ResolveDirectionBasedDestination(
        ProjectileRuntimeData source,
        LinearProjectileMoveDto move,
        Vector2 spawnPosition)
    {
        Vector2 direction = source != null && source.direction.sqrMagnitude > 0.0001f
            ? source.direction.normalized
            : Vector2.right;

        float targetDistance = source != null
            ? Vector2.Distance(spawnPosition, move.targetPosition)
            : 0f;

        float lifetimeDistance = 0f;

        if (source != null && move != null && source.lifetime > 0f && move.speed > 0f)
        {
            lifetimeDistance = source.lifetime * move.speed;
        }

        float distance = Mathf.Max(targetDistance, lifetimeDistance);

        if (distance <= 0.0001f)
        {
            distance = 1f;
        }

        return spawnPosition + direction * distance;
    }
    private Vector2 ResolveLinearBaseDirection(
        LinearProjectileMoveDto move,
        ProjectileRuntimeData source,
        Vector2 spawnPosition)
    {
        if (move == null)
        {
            return ResolveProjectileDirection(source, 0);
        }

        Vector2 toTarget = move.targetPosition - spawnPosition;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            return toTarget.normalized;
        }

        return ResolveProjectileDirection(source, 0);
    }

    private float ResolveLinearDistance(
        LinearProjectileMoveDto move,
        Vector2 spawnPosition)
    {
        if (move == null)
        {
            return 0f;
        }

        float targetDistance = Vector2.Distance(
            spawnPosition,
            move.targetPosition);

        if (targetDistance > 0.0001f)
        {
            return targetDistance;
        }

        return Vector2.Distance(
            move.startPosition,
            move.targetPosition);
    }

    private float EvaluateSpreadAngle(
        int spawnOrder,
        int projectileCount,
        float spreadAngle)
    {
        int safeCount = Mathf.Max(1, projectileCount);

        if (safeCount <= 1)
        {
            return 0f;
        }

        int safeOrder = Mathf.Clamp(
            spawnOrder,
            0,
            safeCount - 1);

        float step = spreadAngle / Mathf.Max(1, safeCount - 1);
        float startAngle = -spreadAngle * 0.5f;

        return startAngle + step * safeOrder;
    }
}