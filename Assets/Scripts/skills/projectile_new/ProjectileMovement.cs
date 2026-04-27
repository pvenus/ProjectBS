using UnityEngine;
using Skills.Dto;

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
            new SkillProjectileOrbitMovement());
    }

    private void InitializeMoveController()
    {
        SkillProjectileMoveControllerDto controllerDto = BuildMoveControllerDto(runtimeData);
        if (controllerDto == null)
        {
            return;
        }

        SkillProjectileMovementContext movementContext = BuildMovementContext(runtimeData);
        moveController.Initialize(controllerDto, movementContext);
    }

    private SkillProjectileMovementContext BuildMovementContext(ProjectileRuntimeData data)
    {
        Transform ownerTransform = data.owner != null ? data.owner.transform : null;
        Transform targetTransform = transform;

        return new SkillProjectileMovementContext
        {
            owner = ownerTransform,
            targetTransform = targetTransform,
            spawnPosition = data.spawnPosition
        };
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

        moveDto.targetTransform = data.target != null ? data.target.transform : null;
        moveDto.startPosition = data.spawnPosition;
        moveDto.targetPosition = moveDto.targetTransform != null
            ? (Vector2)moveDto.targetTransform.position
            : data.spawnPosition + data.NormalizedDirection;

        var controllerDto = new SkillProjectileMoveControllerDto
        {
            applyDirectionRotation = moveDto.applyDirectionRotation,
            rotationTarget = moveDto.rotationTarget,
            rotationOffset = moveDto.rotationOffset,

            linearMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Linear
                ? new SkillProjectileLinearMovementDto
                {
                    targetTransform = transform,
                    startPosition = moveDto.startPosition,
                    targetPosition = moveDto.targetPosition,
                    direction = moveDto.GetDirection(),
                    speed = moveDto.speed,
                    maxDistance = Vector2.Distance(moveDto.startPosition, moveDto.targetPosition),
                    arrivalThreshold = moveDto.arrivalThreshold
                }
                : null,

            warpMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Warp
                ? new SkillProjectileWarpMovementDto
                {
                    targetTransform = moveDto.targetTransform,
                    startPosition = moveDto.startPosition,
                    targetPosition = moveDto.targetPosition,
                    direction = moveDto.GetDirection(),
                    arrivalThreshold = moveDto.arrivalThreshold
                }
                : null,

            hoverMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Hover
                ? new SkillProjectileHoverMovement.HoverMovementDto
                {
                    followOffset = moveDto.followOffset,
                    followLerpSpeed = moveDto.followLerpSpeed,
                    snapOnInitialize = moveDto.snapOnInitialize,
                    useHoverMotion = moveDto.useHoverMotion,
                    hoverAmplitude = moveDto.hoverAmplitude,
                    hoverFrequency = moveDto.hoverFrequency,
                    hoverAxis = moveDto.hoverAxis,
                    endWhenOwnerMissing = moveDto.endWhenOwnerMissing
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
