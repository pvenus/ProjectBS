using UnityEngine;

public class SkillProjectileLinearMovement : ISkillProjectileMovement
{
    private Transform _targetTransform;
    private Vector2 _start;
    private Vector2 _targetPosition;
    private Vector2 _direction = Vector2.right;
    private float _speed;
    private float _maxDistance;
    private float _arrivalThreshold = 0.01f;
    private bool _initialized;
    private bool _hasReachedEnd;

    public Transform TargetTransform => _targetTransform;
    public Vector2 StartPosition => _start;
    public Vector2 TargetPosition => _targetPosition;
    public Vector2 Direction => _direction;
    public float Speed => _speed;
    public float MaxDistance => _maxDistance;
    public bool IsInitialized => _initialized;

    public void Initialize(object dto)
    {
        if (dto is SkillProjectileLinearMovementDto typed)
        {
            Initialize(typed);
        }
        else
        {
            Debug.LogError("Invalid DTO type for SkillProjectileLinearMovement");
        }
    }

    public void Initialize(SkillProjectileLinearMovementDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileLinearMovementDto is null");
            return;
        }

        if (dto.targetTransform == null)
        {
            Debug.LogError("SkillProjectileLinearMovement targetTransform is null");
            return;
        }

        _targetTransform = dto.targetTransform;
        _start = dto.startPosition;
        _targetPosition = dto.targetPosition;

        Vector2 delta = _targetPosition - _start;
        _direction = delta.sqrMagnitude <= 0.0001f
            ? (dto.direction.sqrMagnitude <= 0.0001f ? Vector2.right : dto.direction.normalized)
            : delta.normalized;

        _speed = Mathf.Max(0f, dto.speed);
        _maxDistance = delta.magnitude > 0.0001f ? delta.magnitude : Mathf.Max(0f, dto.maxDistance);
        _arrivalThreshold = Mathf.Max(0.0001f, dto.arrivalThreshold);
        _hasReachedEnd = false;
        _initialized = true;

        _targetTransform.position = _start;
    }

    public void TickMovement(float deltaTime)
    {
        if (!_initialized || _targetTransform == null || _hasReachedEnd)
            return;

        Vector2 current = _targetTransform.position;
        Vector2 toTarget = _targetPosition - current;
        float remainingDistance = toTarget.magnitude;

        if (remainingDistance <= _arrivalThreshold)
        {
            _targetTransform.position = _targetPosition;
            _hasReachedEnd = true;
            return;
        }

        float step = _speed * deltaTime;
        if (step <= 0f)
            return;

        if (step >= remainingDistance)
        {
            _targetTransform.position = _targetPosition;
            _hasReachedEnd = true;
            return;
        }

        Vector2 moveDir = toTarget / remainingDistance;
        _direction = moveDir;
        _targetTransform.position = current + moveDir * step;
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
        _start = Vector2.zero;
        _targetPosition = Vector2.zero;
        _direction = Vector2.right;
        _speed = 0f;
        _maxDistance = 0f;
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
        return _targetTransform != null ? (Vector2)_targetTransform.position : _start;
    }

    public void SetDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        _direction = direction.normalized;
        _targetPosition = _start + (_direction * _maxDistance);
    }

    public void SetSpeed(float speed)
    {
        _speed = Mathf.Max(0f, speed);
    }

    public void SetMaxDistance(float maxDistance)
    {
        _maxDistance = Mathf.Max(0f, maxDistance);
        _targetPosition = _start + (_direction * _maxDistance);
    }
}

public class SkillProjectileLinearMovementDto
{
    public Transform targetTransform;
    public Vector2 startPosition;
    public Vector2 targetPosition;
    public Vector2 direction;
    public float speed;
    public float maxDistance;
    public float arrivalThreshold = 0.01f;
}