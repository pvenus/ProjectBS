

using UnityEngine;

public class SkillProjectileWarpMovement : ISkillProjectileMovement
{
    private Transform _targetTransform;
    private SkillProjectileMovementContext _context;
    private Vector2 _start;
    private Vector2 _targetPosition;
    private Vector2 _direction = Vector2.right;
    private float _arrivalThreshold = 0.01f;
    private bool _initialized;
    private bool _hasReachedEnd;

    public Transform TargetTransform => _targetTransform;
    public Vector2 StartPosition => _start;
    public Vector2 TargetPosition => _targetPosition;
    public Vector2 Direction => _direction;
    public float ArrivalThreshold => _arrivalThreshold;
    public bool IsInitialized => _initialized;

    public void Initialize(object dto)
    {
        if (dto is SkillProjectileWarpMovementDto typed)
        {
            Initialize(typed);
        }
        else
        {
            Debug.LogError("Invalid DTO type for SkillProjectileWarpMovement");
        }
    }

    public void SetContext(SkillProjectileMovementContext context)
    {
        _context = context;
    }

    public void Initialize(SkillProjectileWarpMovementDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileWarpMovementDto is null");
            return;
        }

        if (dto.targetTransform == null)
        {
            Debug.LogError("SkillProjectileWarpMovement targetTransform is null");
            return;
        }

        _targetTransform = dto.targetTransform;
        _start = dto.startPosition;
        _targetPosition = dto.targetPosition;
        _arrivalThreshold = Mathf.Max(0.0001f, dto.arrivalThreshold);

        Vector2 delta = _targetPosition - _start;
        _direction = delta.sqrMagnitude <= 0.0001f
            ? (dto.direction.sqrMagnitude <= 0.0001f ? Vector2.right : dto.direction.normalized)
            : delta.normalized;

        _targetTransform.position = _targetPosition;
        _hasReachedEnd = true;
        _initialized = true;
    }

    public void TickMovement(float deltaTime)
    {
        // Warp movement is completed at initialization.
    }

    public bool HasReachedEnd()
    {
        if (!_initialized || _targetTransform == null)
            return false;

        if (_hasReachedEnd)
            return true;

        return Vector2.Distance(_targetTransform.position, _targetPosition) <= _arrivalThreshold;
    }

    public void ResetMovement()
    {
        _targetTransform = null;
        _context = default;
        _start = Vector2.zero;
        _targetPosition = Vector2.zero;
        _direction = Vector2.right;
        _arrivalThreshold = 0.01f;
        _initialized = false;
        _hasReachedEnd = false;
    }

    public Vector2 GetDirection()
    {
        return _direction;
    }

    public Vector2 GetPosition()
    {
        return _targetTransform != null ? (Vector2)_targetTransform.position : _targetPosition;
    }
}
