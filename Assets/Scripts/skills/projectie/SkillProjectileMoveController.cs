using UnityEngine;
using Skill;
using Skills.Dto.Move;

public class SkillProjectileMoveController
{

    private readonly ISkillProjectileMovement _movement;

    private ProjectileMoveType _moveType = ProjectileMoveType.Linear;
    private bool _initialized;
    private bool _applyDirectionRotation;
    private float _rotationOffset;

    public ProjectileMoveType MoveType => _moveType;
    public bool IsInitialized => _initialized;

    public SkillProjectileMoveController(ISkillProjectileMovement movement)
    {
        _movement = movement;

        if (_movement == null)
        {
            Debug.LogError("Projectile movement component is null");
        }
    }

    public void Initialize(
        SkillMoveRuntimeDto moveDto,
        SkillProjectileMovementContext movementContext)
    {
        if (moveDto == null)
        {
            Debug.LogError("SkillProjectileMoveController.Initialize failed: moveDto is null");
            return;
        }

        switch (moveDto)
        {
            case LinearProjectileMoveDto linearMoveDto:
                InitializeLinear(linearMoveDto, movementContext);
                break;

            case WarpProjectileMoveDto warpMoveDto:
                InitializeWarp(warpMoveDto, movementContext);
                break;

            case HoverProjectileMoveDto hoverMoveDto:
                InitializeHover(hoverMoveDto, movementContext);
                break;

            case OrbitProjectileMoveDto orbitMoveDto:
                InitializeOrbit(orbitMoveDto, movementContext);
                break;

            case HomingProjectileMoveDto homingMoveDto:
                InitializeHoming(homingMoveDto, movementContext);
                break;

            default:
                Debug.LogWarning($"Unsupported move runtime dto type: {moveDto.GetType().Name}");
                break;
        }

        _applyDirectionRotation = moveDto.applyDirectionRotation;
        _rotationOffset = moveDto.rotationOffset;

        if (_applyDirectionRotation)
        {
            ApplyRotation();
        }        
    }

    public void InitializeLinear(
        LinearProjectileMoveDto moveDto,
        Transform movingTransform,
        SkillProjectileMovementContext movementContext = default)
    {
        if (moveDto == null)
        {
            Debug.LogError("SkillProjectileMoveController.InitializeLinear failed: moveDto is null");
            return;
        }

        if (movingTransform == null)
        {
            Debug.LogError("SkillProjectileMoveController.InitializeLinear failed: movingTransform is null");
            return;
        }

        _moveType = ProjectileMoveType.Linear;
        _initialized = false;

        movementContext.projectileTransform = movingTransform;
        InitializeLinear(moveDto, movementContext);
    }

    public void InitializeHover(
        HoverProjectileMoveDto moveDto,
        SkillProjectileMovementContext movementContext = default)
    {
        if (moveDto == null)
        {
            Debug.LogError("SkillProjectileMoveController.InitializeHover failed: moveDto is null");
            return;
        }

        _moveType = ProjectileMoveType.Hover;
        _initialized = false;
        InitializeHoverInternal(moveDto, movementContext);
    }


    public void TickMovement(float deltaTime)
    {
        if (!_initialized)
            return;

        _movement?.TickMovement(deltaTime);
        ApplyRotation();
    }

    public bool HasReachedEnd()
    {
        if (!_initialized)
            return false;

        return _movement != null && _movement.HasReachedEnd();
    }

    public Vector2 GetDirection()
    {
        ISkillProjectileMovement movement = GetCurrentMovement();
        return movement != null ? movement.GetDirection() : Vector2.right;
    }

    public Vector2 GetPosition()
    {
        ISkillProjectileMovement movement = GetCurrentMovement();
        return movement != null ? movement.GetPosition() : Vector2.zero;
    }

    public void ApplyRuntimeUpgrade(SkillUpgradeMono.SkillUpgradeData upgradeData)
    {
        if (_movement is SkillProjectileOrbitMovement orbit)
        {
            orbit.SetRuntimeMaxProjectileCount(
                Mathf.Max(1, Mathf.RoundToInt(upgradeData.projectileCountAdd)));
        }
    }

    private void ApplyRotation()
    {
        if (!_applyDirectionRotation)
            return;

        ISkillProjectileMovement movement = GetCurrentMovement();
        if (movement == null)
            return;

        Vector2 direction = movement.GetDirection();
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        Transform target = GetDefaultRotationTarget();

        if (target == null)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        target.rotation = Quaternion.AngleAxis(angle + _rotationOffset, Vector3.forward);
    }

    public ISkillProjectileMovement GetCurrentMovement()
    {
        return _movement;
    }

    private Transform GetDefaultRotationTarget()
    {
        return _movement switch
        {
            SkillProjectileLinearMovement linear => linear.MovingTransform,
            SkillProjectileWarpMovement warp => warp.TargetTransform,
            SkillProjectileHoverMovement hover => hover.TargetTransform,
            _ => null
        };
    }

    public void ResetController()
    {
        _movement?.ResetMovement();

        _applyDirectionRotation = false;
        _rotationOffset = 0f;
        _moveType = ProjectileMoveType.Linear;
        _initialized = false;
    }

    private void InitializeLinear(LinearProjectileMoveDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_movement == null)
        {
            Debug.LogError("Linear movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _moveType = ProjectileMoveType.Linear;
        _initialized = false;

        _movement.SetContext(movementContext);
        _movement.Initialize(dto);

        if (_movement is SkillProjectileLinearMovement linear)
        {
            _initialized = linear.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }

    private void InitializeHoverInternal(HoverProjectileMoveDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_movement == null)
        {
            Debug.LogError("Hover movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _movement.SetContext(movementContext);
        _movement.Initialize(dto);

        _initialized = true;
    }

    public void InitializeWarp(WarpProjectileMoveDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_movement == null)
        {
            Debug.LogError("Warp movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _moveType = ProjectileMoveType.Warp;
        _initialized = false;

        _movement.SetContext(movementContext);
        _movement.Initialize(dto);

        if (_movement is SkillProjectileWarpMovement warp)
        {
            _initialized = warp.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }

    public void InitializeOrbit(OrbitProjectileMoveDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_movement == null)
        {
            Debug.LogError("Orbit movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _moveType = ProjectileMoveType.Orbit;
        _initialized = false;

        _movement.SetContext(movementContext);
        _movement.Initialize(dto);

        if (_movement is SkillProjectileOrbitMovement orbit)
        {
            _initialized = orbit.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }

    public void InitializeHoming(HomingProjectileMoveDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_movement == null)
        {
            Debug.LogError("Homing movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _moveType = ProjectileMoveType.Homing;
        _initialized = false;

        _movement.SetContext(movementContext);
        _movement.Initialize(dto);

        if (_movement is SkillProjectileHomingMovement homing)
        {
            _initialized = homing.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }
}
