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

        moveController = new SkillProjectileMoveController(
            new SkillProjectileLinearMovement(),
            new SkillProjectileWarpMovement(),
            new SkillProjectileHoverMovement(),
            new SkillProjectileOrbitMovement(),
            new SkillProjectileHomingMovement());
    }

    private void InitializeMoveController()
    {
        SkillProjectileMovementContext movementContext = BuildMovementContext(runtimeData);

        if (runtimeData.moveRuntime is LinearProjectileMoveDto linearMoveDto)
        {
            PrepareLinearRuntimeMoveDto(runtimeData, linearMoveDto);
            moveController.InitializeLinear(linearMoveDto, transform, movementContext);
            return;
        }

        if (runtimeData.moveRuntime is HoverProjectileMoveDto hoverMoveDto)
        {
            moveController.InitializeHover(hoverMoveDto, movementContext);
            return;
        }

        SkillProjectileMoveControllerDto controllerDto = BuildMoveControllerDto(runtimeData);
        if (controllerDto == null)
        {
            return;
        }

        moveController.Initialize(controllerDto, movementContext);
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
    private SkillProjectileMoveControllerDto BuildMoveControllerDto(ProjectileRuntimeData data)
    {
        if (data == null)
        {
            return null;
        }

        SkillProjectileMoveDto moveDto = data.move;
        if (moveDto == null)
        {
            return null;
        }

        bool isWarpMovement =
            moveDto.moveType == SkillProjectileMoveDto.MoveType.Warp;

        bool useRuntimeTargetPosition =
            data.targetingType == TargetingType.AutoTargetDirection ||
            data.targetingType == TargetingType.Directional ||
            data.targetingType == TargetingType.Position ||
            (moveDto.moveType == SkillProjectileMoveDto.MoveType.Linear &&
             data.projectileCount > 1 &&
             data.projectileSpreadAngle > 0f);

        moveDto.startPosition = data.spawnPosition;

        if (useRuntimeTargetPosition)
        {
            // ProjectileFactory / EquipmentSkillResolver already calculated the final destination.
            // Do not bind this movement back to the enemy target transform, otherwise
            // direction-based or spread projectiles can converge to the same target point.
            moveDto.targetTransform = null;

            if (!isWarpMovement &&
                (moveDto.targetPosition - data.spawnPosition).sqrMagnitude <= 0.0001f)
            {
                moveDto.targetPosition = data.spawnPosition + data.NormalizedDirection;
            }
        }
        else if (data.targetingType == TargetingType.AutoTarget)
        {
            moveDto.targetTransform = data.target != null ? data.target.transform : null;
            if (moveDto.targetTransform != null)
            {
                moveDto.targetPosition = moveDto.targetTransform.position;
            }

            if (!isWarpMovement &&
                (moveDto.targetPosition - data.spawnPosition).sqrMagnitude <= 0.0001f)
            {
                moveDto.targetPosition = data.spawnPosition + data.NormalizedDirection;
            }
        }
        else
        {
            moveDto.targetTransform = null;
            // Do not overwrite moveDto.targetPosition from data.targetPosition here.

            if (!isWarpMovement &&
                (moveDto.targetPosition - data.spawnPosition).sqrMagnitude <= 0.0001f)
            {
                moveDto.targetPosition = data.spawnPosition + data.NormalizedDirection;
            }
        }

        var controllerDto = new SkillProjectileMoveControllerDto
        {
            applyDirectionRotation = moveDto.applyDirectionRotation,
            rotationOffset = moveDto.rotationOffset,

            warpMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Warp
                ? new SkillProjectileWarpMovementDto
                {
                    targetTransform = transform,
                    startPosition = moveDto.startPosition,
                    targetPosition = moveDto.targetPosition,
                    direction = moveDto.GetDirection(),
                    arrivalThreshold = moveDto.arrivalThreshold
                }
                : null,
            orbitMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Orbit
                ? new SkillProjectileOrbitMovement.OrbitMovementDto
                {
                    orbitRadius = moveDto.orbitRadius,
                    orbitAngularSpeed = moveDto.orbitAngularSpeed,
                    clockwise = moveDto.clockwise,
                    spawnOrder = runtimeData.spawnOrder,
                    maxProjectileCount = runtimeData.projectileCount,
                    snapOnInitialize = moveDto.snapOnInitialize,
                    resetPhaseWhenLayoutChanges = moveDto.resetPhaseWhenLayoutChanges,
                    endWhenOwnerMissing = moveDto.endWhenOwnerMissing,
                    useRadialPulse = moveDto.useRadialPulse,
                    radialPulseAmplitude = moveDto.radialPulseAmplitude,
                    radialPulseFrequency = moveDto.radialPulseFrequency
                }
                : null,

            homingMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Homing
                ? new HomingMovementDto
                {
                    targetTransform = transform,
                    speed = moveDto.speed,
                    arrivalThreshold = moveDto.arrivalThreshold
                }
                : null
        };

        controllerDto.moveType = moveDto.moveType;
        return controllerDto;
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
