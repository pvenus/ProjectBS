using UnityEngine;

public class SkillProjectileMoveController
{
    public enum ProjectileMoveType
    {
        None = 0,
        Linear = 1,
        Warp = 2
    }

    private ISkillProjectileMovement _linearMovement;
    private ISkillProjectileMovement _warpMovement;

    private ProjectileMoveType _moveType = ProjectileMoveType.None;
    private bool _initialized;
    private bool _applyDirectionRotation;
    private Transform _rotationTarget;
    private float _rotationOffset;

    public ProjectileMoveType MoveType => _moveType;
    public bool IsInitialized => _initialized;

    public SkillProjectileMoveController(ISkillProjectileMovement linearMovement, ISkillProjectileMovement warpMovement = null)
    {
        _linearMovement = linearMovement;
        _warpMovement = warpMovement;

        if (_linearMovement == null)
        {
            Debug.LogError("Linear movement component is null");
        }
    }

    public void Initialize(SkillProjectileMoveControllerDto dto)
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
                InitializeLinear(dto.linearMovement);
                break;

            case ProjectileMoveType.Warp:
                InitializeWarp(dto.warpMovement);
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

    private ISkillProjectileMovement GetCurrentMovement()
    {
        return _moveType switch
        {
            ProjectileMoveType.Linear => _linearMovement,
            ProjectileMoveType.Warp => _warpMovement,
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
        }

        _applyDirectionRotation = false;
        _rotationTarget = null;
        _rotationOffset = 0f;
        _moveType = ProjectileMoveType.None;
        _initialized = false;
    }

    private void InitializeLinear(SkillProjectileLinearMovementDto dto)
    {
        if (_linearMovement == null)
        {
            Debug.LogError("Linear movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

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

    private void InitializeWarp(SkillProjectileWarpMovementDto dto)
    {
        if (_warpMovement == null)
        {
            Debug.LogError("Warp movement component is missing or does not implement ISkillProjectileMovement");
            return;
        }

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
}

public class SkillProjectileMoveControllerDto
{
    public SkillProjectileMoveController.ProjectileMoveType moveType;
    public SkillProjectileLinearMovementDto linearMovement;
    public SkillProjectileWarpMovementDto warpMovement;

    public bool applyDirectionRotation;
    public Transform rotationTarget;
    public float rotationOffset;
}