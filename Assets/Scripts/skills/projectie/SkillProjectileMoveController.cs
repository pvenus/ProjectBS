using UnityEngine;

public class SkillProjectileMoveController
{
    public enum ProjectileMoveType
    {
        None = 0,
        Linear = 1,
        Warp = 2,
        Hover = 3,
        Orbit = 4
    }

    private ISkillProjectileMovement _linearMovement;
    private ISkillProjectileMovement _warpMovement;
    private ISkillProjectileMovement _hoverMovement;
    private ISkillProjectileMovement _orbitMovement;

    private ProjectileMoveType _moveType = ProjectileMoveType.None;
    private bool _initialized;
    private bool _applyDirectionRotation;
    private Transform _rotationTarget;
    private float _rotationOffset;

    public ProjectileMoveType MoveType => _moveType;
    public bool IsInitialized => _initialized;

    public SkillProjectileMoveController(
        ISkillProjectileMovement linearMovement,
        ISkillProjectileMovement warpMovement = null,
        ISkillProjectileMovement hoverMovement = null,
        ISkillProjectileMovement orbitMovement = null)
    {
        _linearMovement = linearMovement;
        _warpMovement = warpMovement;
        _hoverMovement = hoverMovement;
        _orbitMovement = orbitMovement;

        if (_linearMovement == null)
        {
            Debug.LogError("Linear movement component is null");
        }
    }

    public void Initialize(SkillProjectileMoveControllerDto dto)
    {
        Initialize(dto, default);
    }

    public void Initialize(SkillProjectileMoveControllerDto dto, SkillProjectileMovementContext movementContext)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileMoveControllerDto is null");
            return;
        }

        _moveType = dto.moveType;
        _initialized = false;
        _applyDirectionRotation = dto.applyDirectionRotation;
        _rotationTarget = dto.rotationTarget;
        _rotationOffset = dto.rotationOffset;

        switch (_moveType)
        {
            case ProjectileMoveType.Linear:
                InitializeLinear(dto.linearMovement, movementContext);
                break;

            case ProjectileMoveType.Warp:
                InitializeWarp(dto.warpMovement, movementContext);
                break;

            case ProjectileMoveType.Hover:
                InitializeHover(dto.hoverMovement, movementContext);
                break;

            case ProjectileMoveType.Orbit:
                InitializeOrbit(dto.orbitMovement, movementContext);
                break;

            case ProjectileMoveType.None:
            default:
                Debug.LogWarning($"Unsupported move type: {_moveType}");
                break;
        }
    }

    public void TickMovement(float deltaTime)
    {
        if (!_initialized)
            return;

        switch (_moveType)
        {
            case ProjectileMoveType.Linear:
                _linearMovement?.TickMovement(deltaTime);
                break;

            case ProjectileMoveType.Warp:
                _warpMovement?.TickMovement(deltaTime);
                break;

            case ProjectileMoveType.Hover:
                _hoverMovement?.TickMovement(deltaTime);
                break;

            case ProjectileMoveType.Orbit:
                _orbitMovement?.TickMovement(deltaTime);
                break;
        }
        ApplyRotation();
    }

    public bool HasReachedEnd()
    {
        if (!_initialized)
            return false;

        return _moveType switch
        {
            ProjectileMoveType.Linear => _linearMovement != null && _linearMovement.HasReachedEnd(),
            ProjectileMoveType.Warp => _warpMovement != null && _warpMovement.HasReachedEnd(),
            ProjectileMoveType.Hover => _hoverMovement != null && _hoverMovement.HasReachedEnd(),
            ProjectileMoveType.Orbit => _orbitMovement != null && _orbitMovement.HasReachedEnd(),
            _ => false
        };
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

    public Transform GetRotationTarget()
    {
        return _rotationTarget;
    }

    public void ApplyRuntimeUpgrade(SkillUpgradeMono.SkillUpgradeData upgradeData)
    {
        switch (_moveType)
        {
            case ProjectileMoveType.Orbit:
                if (_orbitMovement is SkillProjectileOrbitMovement orbit)
                {
                    orbit.SetRuntimeMaxProjectileCount(Mathf.Max(1, Mathf.RoundToInt(upgradeData.projectileCountAdd)));
                }
                break;
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

        Transform target = _rotationTarget;
        if (target == null)
        {
            target = GetDefaultRotationTarget();
        }

        if (target == null)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        target.rotation = Quaternion.AngleAxis(angle + _rotationOffset, Vector3.forward);
    }

    public ISkillProjectileMovement GetCurrentMovement()
    {
        return _moveType switch
        {
            ProjectileMoveType.Linear => _linearMovement,
            ProjectileMoveType.Warp => _warpMovement,
            ProjectileMoveType.Hover => _hoverMovement,
            ProjectileMoveType.Orbit => _orbitMovement,
            _ => null
        };
    }

    private Transform GetDefaultRotationTarget()
    {
        return _moveType switch
        {
            ProjectileMoveType.Linear when _linearMovement is SkillProjectileLinearMovement linear => linear.TargetTransform,
            ProjectileMoveType.Warp when _warpMovement is SkillProjectileWarpMovement warp => warp.TargetTransform,
            _ => null
        };
    }

    public void ResetController()
    {
        switch (_moveType)
        {
            case ProjectileMoveType.Linear:
                _linearMovement?.ResetMovement();
                break;

            case ProjectileMoveType.Warp:
                _warpMovement?.ResetMovement();
                break;

            case ProjectileMoveType.Hover:
                _hoverMovement?.ResetMovement();
                break;

            case ProjectileMoveType.Orbit:
                _orbitMovement?.ResetMovement();
                break;
        }

        _applyDirectionRotation = false;
        _rotationTarget = null;
        _rotationOffset = 0f;
        _moveType = ProjectileMoveType.None;
        _initialized = false;
    }

    private void InitializeLinear(SkillProjectileLinearMovementDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_linearMovement == null)
        {
            Debug.LogError("Linear movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _linearMovement.SetContext(movementContext);
        _linearMovement.Initialize(dto);

        if (_linearMovement is SkillProjectileLinearMovement linear)
        {
            _initialized = linear.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }

    private void InitializeWarp(SkillProjectileWarpMovementDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_warpMovement == null)
        {
            Debug.LogError("Warp movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _warpMovement.SetContext(movementContext);
        _warpMovement.Initialize(dto);

        if (_warpMovement is SkillProjectileWarpMovement warp)
        {
            _initialized = warp.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }

    private void InitializeHover(SkillProjectileHoverMovement.HoverMovementDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_hoverMovement == null)
        {
            Debug.LogError("Hover movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _hoverMovement.SetContext(movementContext);
        _hoverMovement.Initialize(dto);

        if (_hoverMovement is SkillProjectileHoverMovement hover)
        {
            _initialized = true;
        }
        else
        {
            _initialized = true;
        }
    }

    private void InitializeOrbit(SkillProjectileOrbitMovement.OrbitMovementDto dto, SkillProjectileMovementContext movementContext)
    {
        if (_orbitMovement == null)
        {
            Debug.LogError("Orbit movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

        _orbitMovement.SetContext(movementContext);
        _orbitMovement.Initialize(dto);

        if (_orbitMovement is SkillProjectileOrbitMovement orbit)
        {
            _initialized = orbit.IsInitialized;
        }
        else
        {
            _initialized = true;
        }
    }
}

public class SkillProjectileMoveControllerDto
{
    public SkillProjectileMoveController.ProjectileMoveType moveType;
    public SkillProjectileLinearMovementDto linearMovement;
    public SkillProjectileWarpMovementDto warpMovement;
    public SkillProjectileHoverMovement.HoverMovementDto hoverMovement;
    public SkillProjectileOrbitMovement.OrbitMovementDto orbitMovement;

    public bool applyDirectionRotation;
    public Transform rotationTarget;
    public float rotationOffset;
}