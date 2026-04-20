using UnityEngine;

/// <summary>
/// 시전자(owner)를 기준으로 지정된 오프셋 위치를 따라다니는 호버링 이동.
/// 필요하면 약간의 부유(sine) 연출도 줄 수 있다.
///
/// 기본 사용 목적:
/// - 시전자를 따라다니는 패시브 오브젝트
/// - 장판형 위성체
/// - 추종형 도트/오브
/// </summary>
public class SkillProjectileHoverMovement : ISkillProjectileMovement
{
    [System.Serializable]
    public class HoverMovementDto
    {
        [Header("Follow")]
        public Vector2 followOffset = Vector2.zero;
        public float followLerpSpeed = 12f;
        public bool snapOnInitialize = true;

        [Header("Hover")]
        public bool useHoverMotion = true;
        public float hoverAmplitude = 0.15f;
        public float hoverFrequency = 2.5f;
        public Vector2 hoverAxis = Vector2.up;

        [Header("Behavior")]
        public bool endWhenOwnerMissing = true;
    }

    private HoverMovementDto _dto;
    private SkillProjectileMovementContext _context;
    private bool _initialized;
    private bool _hasReachedEnd;
    private float _hoverTime;
    private Vector2 _currentDirection = Vector2.up;
    private Vector2 _lastPosition;
    private Transform _targetTransform;
    private Vector2 _currentPosition;

    public void Initialize(object dto)
    {
        if (dto is HoverMovementDto typed)
        {
            Initialize(typed);
        }
        else
        {
            Debug.LogError("Invalid DTO type for SkillProjectileHoverMovement");
        }
    }

    public void Initialize(HoverMovementDto dto)
    {
        _dto = dto ?? new HoverMovementDto();
        _dto.followLerpSpeed = Mathf.Max(0f, _dto.followLerpSpeed);
        _dto.hoverFrequency = Mathf.Max(0f, _dto.hoverFrequency);
        _hoverTime = 0f;
        _hasReachedEnd = false;
        _initialized = true;

        _targetTransform = _context.targetTransform;

        Vector2 startPosition = _targetTransform != null
            ? (Vector2)_targetTransform.position
            : _context.spawnPosition;

        if (_context.owner != null)
        {
            Vector2 target = EvaluateBaseFollowPosition();
            if (_dto.snapOnInitialize)
            {
                _currentPosition = target;
                if (_targetTransform != null)
                    _targetTransform.position = target;
                startPosition = target;
            }
            else
            {
                _currentPosition = startPosition;
            }
        }
        else
        {
            _currentPosition = startPosition;
        }

        _lastPosition = startPosition;
        _currentDirection = Vector2.up;
    }

    public void SetContext(SkillProjectileMovementContext context)
    {
        _context = context;
    }

    public void TickMovement(float deltaTime)
    {
        if (!_initialized || _hasReachedEnd)
            return;

        if (_context.owner == null)
        {
            if (_dto != null && _dto.endWhenOwnerMissing)
                _hasReachedEnd = true;
            return;
        }

        if (_targetTransform == null)
            return;

        _hoverTime += Mathf.Max(0f, deltaTime);

        Vector2 basePosition = EvaluateBaseFollowPosition();
        Vector2 hoverOffset = EvaluateHoverOffset();
        Vector2 desiredPosition = basePosition + hoverOffset;
        Vector2 currentPosition = _targetTransform != null ? (Vector2)_targetTransform.position : _currentPosition;

        Vector2 nextPosition;
        if (_dto.followLerpSpeed <= 0f)
        {
            nextPosition = desiredPosition;
        }
        else
        {
            float t = 1f - Mathf.Exp(-_dto.followLerpSpeed * Mathf.Max(0f, deltaTime));
            nextPosition = Vector2.Lerp(currentPosition, desiredPosition, t);
        }

        Vector2 delta = nextPosition - _lastPosition;
        if (delta.sqrMagnitude > 0.000001f)
            _currentDirection = delta.normalized;

        _currentPosition = nextPosition;
        if (_targetTransform != null)
            _targetTransform.position = nextPosition;

        _lastPosition = nextPosition;
    }

    public bool HasReachedEnd()
    {
        return _hasReachedEnd;
    }

    public void ResetMovement()
    {
        _dto = null;
        _context = default;
        _targetTransform = null;
        _currentPosition = Vector2.zero;
        _initialized = false;
        _hasReachedEnd = false;
        _hoverTime = 0f;
        _currentDirection = Vector2.up;
        _lastPosition = Vector2.zero;
    }

    public Vector2 GetDirection()
    {
        return _currentDirection;
    }

    public Vector2 GetPosition()
    {
        return _targetTransform != null ? (Vector2)_targetTransform.position : _currentPosition;
    }

    private Vector2 EvaluateBaseFollowPosition()
    {
        if (_context.owner == null)
            return GetPosition();

        Vector2 ownerPosition = _context.owner.position;
        Vector2 offset = _dto != null ? _dto.followOffset : Vector2.zero;
        return ownerPosition + offset;
    }

    private Vector2 EvaluateHoverOffset()
    {
        if (_dto == null || !_dto.useHoverMotion)
            return Vector2.zero;

        Vector2 axis = _dto.hoverAxis.sqrMagnitude > 0.0001f ? _dto.hoverAxis.normalized : Vector2.up;
        float wave = Mathf.Sin(_hoverTime * Mathf.PI * 2f * _dto.hoverFrequency);
        return axis * (_dto.hoverAmplitude * wave);
    }
}