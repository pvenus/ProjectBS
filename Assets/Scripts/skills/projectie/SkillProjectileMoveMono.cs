using UnityEngine;

/// <summary>
/// Projectile 이동 전용 게이트웨이 Mono.
/// - Unity 생명주기(Update)를 담당한다.
/// - 내부에서 SkillProjectileMoveController를 생성/보관한다.
/// - 실제 이동 계산은 각 movement 구현체에 위임한다.
/// </summary>
public class SkillProjectileMoveMono : MonoBehaviour
{
    [Header("Auto Play")]
    [SerializeField] private bool playOnStart;
    [SerializeField] private SkillMoveSO moveConfig;
    [SerializeField] private Transform targetTransformOverride;
    [SerializeField] private Vector2 startPositionOverride;
    [SerializeField] private Vector2 targetPositionOverride = Vector2.right;

    private SkillProjectileMoveController _moveController;

    public bool IsReady => _moveController != null;
    public bool IsInitialized => _moveController != null && _moveController.IsInitialized;
    public SkillProjectileMoveController.ProjectileMoveType MoveType =>
        _moveController != null ? _moveController.MoveType : SkillProjectileMoveController.ProjectileMoveType.None;

    public SkillMoveSO CurrentMoveConfig => moveConfig;

    private void Awake()
    {
        BuildController();
    }

    private void Start()
    {
        if (!playOnStart || moveConfig == null)
            return;

        Transform runtimeTarget = targetTransformOverride != null ? targetTransformOverride : transform;
        Vector2 runtimeStart = startPositionOverride.sqrMagnitude <= 0.0001f
            ? (Vector2)transform.position
            : startPositionOverride;
        Vector2 runtimeTargetPosition = targetPositionOverride.sqrMagnitude <= 0.0001f
            ? runtimeStart
            : targetPositionOverride;

        Initialize(moveConfig, runtimeTarget, runtimeStart, runtimeTargetPosition);
    }
    public void Initialize(SkillMoveSO config, Transform targetTransform, Vector2 startPosition, Vector2 targetPosition)
    {
        if (config == null)
        {
            Debug.LogError("SkillMoveSO is null", this);
            return;
        }

        moveConfig = config;

        SkillProjectileMoveDto moveDto = config.CreateDto(targetTransform, startPosition, targetPosition);

        var controllerDto = new SkillProjectileMoveControllerDto
        {
            moveType = ConvertMoveType(moveDto.moveType),
            linearMovement = moveDto.moveType == SkillProjectileMoveDto.MoveType.Linear
                ? new SkillProjectileLinearMovementDto
                {
                    targetTransform = moveDto.targetTransform,
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

            applyDirectionRotation = moveDto.applyDirectionRotation,
            rotationTarget = moveDto.rotationTarget,
            rotationOffset = moveDto.rotationOffset
        };

        Initialize(controllerDto);
    }

    private void Update()
    {
        if (_moveController == null || !_moveController.IsInitialized)
            return;

        _moveController.TickMovement(Time.deltaTime);
    }

    /// <summary>
    /// 기본 movement/controller 조합을 생성한다.
    /// 현재는 linear movement를 사용한다.
    /// </summary>
    public void BuildController()
    {
        _moveController = new SkillProjectileMoveController(
            new SkillProjectileLinearMovement(),
            new SkillProjectileWarpMovement());
    }

    /// <summary>
    /// 런타임 DTO로 이동 컨트롤러를 초기화한다.
    /// </summary>
    public void Initialize(SkillProjectileMoveControllerDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileMoveControllerDto is null", this);
            return;
        }

        if (_moveController == null)
        {
            BuildController();
        }

        if (dto.moveType == SkillProjectileMoveController.ProjectileMoveType.Linear && dto.linearMovement != null)
        {
            dto.linearMovement.targetTransform = transform;
        }
        else if (dto.moveType == SkillProjectileMoveController.ProjectileMoveType.Warp && dto.warpMovement != null)
        {
            dto.warpMovement.targetTransform = transform;
        }

        _moveController.Initialize(dto);
    }

    /// <summary>
    /// 이동 종료 여부를 반환한다.
    /// </summary>
    public bool HasReachedEnd()
    {
        if (_moveController == null)
            return false;

        return _moveController.HasReachedEnd();
    }

    /// <summary>
    /// 현재 이동 방향을 반환한다.
    /// </summary>
    public Vector2 GetDirection()
    {
        if (_moveController == null)
            return Vector2.right;

        return _moveController.GetDirection();
    }

    /// <summary>
    /// 현재 이동 위치를 반환한다.
    /// </summary>
    public Vector2 GetPosition()
    {
        if (_moveController == null)
            return transform.position;

        return _moveController.GetPosition();
    }

    /// <summary>
    /// 현재 이동 상태를 초기화한다.
    /// </summary>
    public void ResetMovement()
    {
        if (_moveController == null)
            return;

        _moveController.ResetController();
    }

    private SkillProjectileMoveController.ProjectileMoveType ConvertMoveType(SkillProjectileMoveDto.MoveType moveType)
    {
        return moveType switch
        {
            SkillProjectileMoveDto.MoveType.Linear => SkillProjectileMoveController.ProjectileMoveType.Linear,
            SkillProjectileMoveDto.MoveType.Warp => SkillProjectileMoveController.ProjectileMoveType.Warp,
            _ => SkillProjectileMoveController.ProjectileMoveType.None
        };
    }
}