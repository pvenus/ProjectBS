

using UnityEngine;

/// <summary>
/// owner를 중심으로 원형 궤도를 도는 투사체 이동.
///
/// 핵심 개념:
/// - 여러 개의 투사체를 각각 따로 생성하되,
///   각 투사체는 자신의 spawnOrder 와 maxProjectileCount 를 기준으로
///   원 위의 시작 좌표를 스스로 계산한다.
/// - maxProjectileCount 가 바뀌면(예: 업그레이드로 최대 개수 증가/감소),
///   각 투사체는 다음 Tick 에 이를 감지하고 자신의 시작 각도를 다시 계산한 뒤
///   레이아웃을 리셋하여 새로운 원 배치로 자연스럽게 재정렬된다.
///
/// 주의:
/// - 이 클래스는 이동 로직 객체이며 MonoBehaviour 가 아니다.
/// - 실제로 움직이는 대상은 SkillProjectileMovementContext.targetTransform 이다.
/// - 따라가야 하는 대상은 SkillProjectileMovementContext.owner 이다.
/// </summary>
public class SkillProjectileOrbitMovement : ISkillProjectileMovement
{
    [System.Serializable]
    public class OrbitMovementDto
    {
        [Header("Orbit")]
        public float orbitRadius = 1.5f;
        public float orbitAngularSpeed = 180f;
        public bool clockwise;

        [Header("Layout")]
        public int spawnOrder;
        public int maxProjectileCount = 1;
        public bool snapOnInitialize = true;
        public bool resetPhaseWhenLayoutChanges = true;

        [Header("Behavior")]
        public bool endWhenOwnerMissing = true;

        [Header("Optional Radial Pulse")]
        public bool useRadialPulse;
        public float radialPulseAmplitude;
        public float radialPulseFrequency;
    }

    private OrbitMovementDto _dto;
    private SkillProjectileMovementContext _context;

    private Transform _targetTransform;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;
    private Vector2 _currentDirection = Vector2.right;

    private bool _initialized;
    private bool _hasReachedEnd;
    private int _lastResolvedMaxProjectileCount;

    /// <summary>
    /// 외부 시스템(예: 업그레이드 반영부)에서 런타임 최대 투사체 개수를 직접 주입할 때 사용.
    /// 0 이하이면 DTO 값을 그대로 사용한다.
    /// </summary>
    private int _runtimeMaxProjectileCountOverride;

    public bool IsInitialized => _initialized;
    public int CurrentResolvedMaxProjectileCount => ResolveCurrentMaxProjectileCount();

    public void Initialize(object dto)
    {
        if (dto is OrbitMovementDto typed)
        {
            Initialize(typed);
        }
        else
        {
            Debug.LogError("Invalid DTO type for SkillProjectileOrbitMovement");
        }
    }

    public void Initialize(OrbitMovementDto dto)
    {
        _dto = dto ?? new OrbitMovementDto();
        _dto.orbitRadius = Mathf.Max(0f, _dto.orbitRadius);
        _dto.maxProjectileCount = Mathf.Max(1, _dto.maxProjectileCount);
        _dto.radialPulseAmplitude = Mathf.Max(0f, _dto.radialPulseAmplitude);
        _dto.radialPulseFrequency = Mathf.Max(0f, _dto.radialPulseFrequency);

        _targetTransform = _context.targetTransform;
        _hasReachedEnd = false;
        _initialized = true;

        _lastResolvedMaxProjectileCount = ResolveCurrentMaxProjectileCount();
        ResetOrbitLayout(_dto.snapOnInitialize);
    }

    public void SetContext(SkillProjectileMovementContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 업그레이드 등으로 최대 투사체 개수가 바뀌었을 때 외부에서 호출하면,
    /// 다음 Tick 에 자동으로 새로운 레이아웃으로 재배치된다.
    /// </summary>
    public void SetRuntimeMaxProjectileCount(int maxProjectileCount)
    {
        _runtimeMaxProjectileCountOverride = Mathf.Max(0, maxProjectileCount);
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

        int resolvedMaxProjectileCount = ResolveCurrentMaxProjectileCount();
        if (resolvedMaxProjectileCount != _lastResolvedMaxProjectileCount)
        {
            _lastResolvedMaxProjectileCount = resolvedMaxProjectileCount;
            ResetOrbitLayout(_dto != null && _dto.resetPhaseWhenLayoutChanges);
        }

        float synchronizedTime = GetSynchronizedOrbitTime();
        Vector2 nextPosition = EvaluateOrbitPosition(synchronizedTime, resolvedMaxProjectileCount);
        Vector2 delta = nextPosition - _lastPosition;
        if (delta.sqrMagnitude > 0.000001f)
            _currentDirection = delta.normalized;

        _currentPosition = nextPosition;
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
        _lastPosition = Vector2.zero;
        _currentDirection = Vector2.right;
        _initialized = false;
        _hasReachedEnd = false;
        _lastResolvedMaxProjectileCount = 1;
        _runtimeMaxProjectileCountOverride = 0;
    }

    public Vector2 GetDirection()
    {
        return _currentDirection;
    }

    public Vector2 GetPosition()
    {
        return _targetTransform != null ? (Vector2)_targetTransform.position : _currentPosition;
    }

    private void ResetOrbitLayout(bool resetPhase)
    {
        if (_targetTransform == null)
            return;

        Vector2 startPosition = EvaluateOrbitPosition(GetSynchronizedOrbitTime(), _lastResolvedMaxProjectileCount);

        _currentPosition = startPosition;
        _lastPosition = startPosition;
        _targetTransform.position = startPosition;

        SkillProjectileVisualMono visualMono = _targetTransform.GetComponentInChildren<SkillProjectileVisualMono>(true);
        if (visualMono != null)
        {
            visualMono.RestartCurrentClip();
        }
    }

    private float GetSynchronizedOrbitTime()
    {
        return Time.time;
    }

    private Vector2 EvaluateOrbitPosition(float elapsedTime, int maxProjectileCount)
    {
        if (_context.owner == null)
            return GetPosition();

        float baseAngleDeg = EvaluateBaseAngleDeg(maxProjectileCount);
        float signedAngularSpeed = _dto.clockwise ? -_dto.orbitAngularSpeed : _dto.orbitAngularSpeed;
        float currentAngleDeg = baseAngleDeg + signedAngularSpeed * elapsedTime;
        float currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;

        float radius = _dto.orbitRadius;
        if (_dto.useRadialPulse && _dto.radialPulseAmplitude > 0f && _dto.radialPulseFrequency > 0f)
        {
            float wave = Mathf.Sin(elapsedTime * Mathf.PI * 2f * _dto.radialPulseFrequency);
            radius += _dto.radialPulseAmplitude * wave;
        }

        radius = Mathf.Max(0f, radius);

        Vector2 orbitOffset = new Vector2(Mathf.Cos(currentAngleRad), Mathf.Sin(currentAngleRad)) * radius;
        return (Vector2)_context.owner.position + orbitOffset;
    }

    private float EvaluateBaseAngleDeg(int maxProjectileCount)
    {
        int safeMax = Mathf.Max(1, maxProjectileCount);
        int safeOrder = Mathf.Clamp(_dto.spawnOrder, 0, Mathf.Max(0, safeMax - 1));
        float angleStep = 360f / safeMax;
        return angleStep * safeOrder;
    }

    private int ResolveCurrentMaxProjectileCount()
    {
        if (_runtimeMaxProjectileCountOverride > 0)
            return Mathf.Max(1, _runtimeMaxProjectileCountOverride);

        if (_dto == null)
            return 1;

        return Mathf.Max(1, _dto.maxProjectileCount);
    }
}