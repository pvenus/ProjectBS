using UnityEngine;
using Skill;
using Skills.Dto.Move;

/// <summary>
/// ProjectileEntity의 실제 이동을 담당하는 컴포넌트.
/// ProjectileRuntimeData를 입력으로 받아,
/// 표준 SkillProjectileMoveController 기반 이동 시스템에 위임한다.
/// </summary>
public class ProjectileMovement : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool initialized;
    [SerializeField] private Vector2 direction = Vector2.right;

    private ProjectileEntity owner;
    private ProjectileRuntimeData runtimeData;
    private SkillProjectileMoveController moveController;

    public bool IsInitialized => initialized;
    public Vector2 Direction => direction;

    public void Initialize(ProjectileEntity ownerEntity, ProjectileRuntimeData data)
    {
        if (ownerEntity == null)
        {
            Debug.LogError("ProjectileMovement.Initialize failed: ownerEntity is null.", this);
            return;
        }

        if (data == null)
        {
            Debug.LogError("ProjectileMovement.Initialize failed: ProjectileRuntimeData is null.", this);
            return;
        }

        owner = ownerEntity;
        runtimeData = data;
        initialized = true;

        direction = data.NormalizedDirection;

        BuildMoveControllerIfNeeded();
        InitializeMoveController();
    }

    private void Update()
    {
        if (!initialized || owner == null || runtimeData == null)
        {
            return;
        }

        if (moveController != null)
        {
            moveController.TickMovement(Time.deltaTime);
        }
    }

    private void BuildMoveControllerIfNeeded()
    {
        if (moveController != null)
        {
            return;
        }

        ISkillProjectileMovement movement = CreateMovement(runtimeData.moveRuntime);

        if (movement == null)
        {
            Debug.LogWarning(
                $"Unsupported moveRuntime type: {(runtimeData.moveRuntime != null ? runtimeData.moveRuntime.GetType().Name : "null")}",
                this);
            return;
        }

        moveController = new SkillProjectileMoveController(movement);
    }

    private ISkillProjectileMovement CreateMovement(SkillMoveRuntimeDto moveDto)
    {
        return moveDto switch
        {
            LinearProjectileMoveDto => new SkillProjectileLinearMovement(),
            HoverProjectileMoveDto => new SkillProjectileHoverMovement(),
            WarpProjectileMoveDto => new SkillProjectileWarpMovement(),
            HomingProjectileMoveDto => new SkillProjectileHomingMovement(),
            OrbitProjectileMoveDto => new SkillProjectileOrbitMovement(),
            _ => null
        };
    }

    private void InitializeMoveController()
    {
        if (moveController == null)
        {
            return;
        }

        if (runtimeData.moveRuntime == null)
        {
            Debug.LogWarning("Projectile moveRuntime is null.", this);
            return;
        }

        SkillProjectileMovementContext movementContext = BuildMovementContext(runtimeData);
        PrepareRuntimeMoveDto(runtimeData, runtimeData.moveRuntime);
        moveController.Initialize(runtimeData.moveRuntime, movementContext);
    }

    private void PrepareRuntimeMoveDto(
        ProjectileRuntimeData data,
        SkillMoveRuntimeDto moveDto)
    {
        switch (moveDto)
        {
            case LinearProjectileMoveDto linearMoveDto:
                PrepareLinearRuntimeMoveDto(data, linearMoveDto);
                break;

            case WarpProjectileMoveDto warpMoveDto:
                PrepareWarpRuntimeMoveDto(data, warpMoveDto);
                break;
        }
    }

    private SkillProjectileMovementContext BuildMovementContext(ProjectileRuntimeData data)
    {
        Transform ownerTransform = data.owner != null ? data.owner.transform : null;
        Transform targetTransform = data.target != null ? data.target.transform : null;

        return new SkillProjectileMovementContext
        {
            owner = ownerTransform,
            projectileTransform = transform,
            targetTransform = targetTransform,
            targetLayerMask = data.hit.targetLayerMask,
            spawnPosition = data.spawnPosition
        };
    }

    private void PrepareLinearRuntimeMoveDto(
        ProjectileRuntimeData data,
        LinearProjectileMoveDto moveDto)
    {
        if (data == null || moveDto == null)
        {
            return;
        }

        moveDto.startPosition = data.spawnPosition;

        bool useRuntimeTargetPosition =
            data.targetingType == TargetingType.AutoTargetDirection ||
            data.targetingType == TargetingType.Directional ||
            data.targetingType == TargetingType.Position ||
            (data.projectileCount > 1 && data.projectileSpreadAngle > 0f);

        if (useRuntimeTargetPosition)
        {
            if ((moveDto.targetPosition - data.spawnPosition).sqrMagnitude <= 0.0001f)
            {
                moveDto.targetPosition = data.spawnPosition + data.NormalizedDirection;
            }

            return;
        }

        if (data.targetingType == TargetingType.AutoTarget)
        {
            Transform targetTransform = data.target != null ? data.target.transform : null;

            if (targetTransform != null)
            {
                moveDto.targetPosition = targetTransform.position;
            }

            if ((moveDto.targetPosition - data.spawnPosition).sqrMagnitude <= 0.0001f)
            {
                moveDto.targetPosition = data.spawnPosition + data.NormalizedDirection;
            }

            return;
        }

        if ((moveDto.targetPosition - data.spawnPosition).sqrMagnitude <= 0.0001f)
        {
            moveDto.targetPosition = data.spawnPosition + data.NormalizedDirection;
        }
    }

    private void PrepareWarpRuntimeMoveDto(
        ProjectileRuntimeData data,
        WarpProjectileMoveDto moveDto)
    {
        if (data == null || moveDto == null)
        {
            return;
        }

        if (data.targetingType == TargetingType.AutoTarget)
        {
            Transform targetTransform = data.target != null ? data.target.transform : null;

            if (targetTransform != null)
            {
                moveDto.targetPosition = targetTransform.position;
            }
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        if (newDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        direction = newDirection.normalized;
    }
}
