using UnityEngine;
using Skills.Dto.Move;
using Skill;

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
    [SerializeField] private Transform casterOverride;
    [SerializeField] private Transform targetTransformOverride;
    [SerializeField] private Vector2 startPositionOverride;
    [SerializeField] private Vector2 targetPositionOverride = Vector2.right;

    private SkillProjectileMoveController _moveController;

    public bool IsReady => _moveController != null;
    public bool IsInitialized => _moveController != null && _moveController.IsInitialized;
    public ProjectileMoveType MoveType =>
        _moveController != null ? _moveController.MoveType : ProjectileMoveType.Linear;

    public SkillMoveSO CurrentMoveConfig => moveConfig;

    public SkillProjectileMoveController GetController()
    {
        return _moveController;
    }

    private void Awake()
    {
        BuildController();
    }

    private void Start()
    {
        if (!playOnStart || moveConfig == null)
            return;

        Transform runtimeCaster = casterOverride != null ? casterOverride : transform;
        Transform runtimeTarget = targetTransformOverride != null ? targetTransformOverride : transform;
        Vector2 runtimeStart = startPositionOverride.sqrMagnitude <= 0.0001f
            ? (Vector2)transform.position
            : startPositionOverride;
        Vector2 runtimeTargetPosition = targetPositionOverride.sqrMagnitude <= 0.0001f
            ? runtimeStart
            : targetPositionOverride;
    }


    public void Initialize(
        LinearProjectileMoveDto moveDto,
        Transform caster,
        Transform targetTransform,
        Vector2 startPosition)
    {
        if (moveDto == null)
        {
            Debug.LogError("LinearProjectileMoveDto is null", this);
            return;
        }

        if (_moveController == null)
        {
            BuildController();
        }

        SkillProjectileMovementContext movementContext = new SkillProjectileMovementContext
        {
            owner = caster,
            projectileTransform = transform,
            targetTransform = targetTransform,
            spawnPosition = startPosition
        };

        _moveController.InitializeLinear(moveDto, transform, movementContext);
    }

    public void Initialize(SkillProjectileMoveDto moveDto, Transform caster, Transform targetTransform, Vector2 startPosition, Vector2 targetPosition)
    {
        if (moveDto == null)
        {
            Debug.LogError("SkillProjectileMoveDto is null", this);
            return;
        }

        SkillProjectileMovementContext movementContext = new SkillProjectileMovementContext
        {
            owner = caster,
            projectileTransform = transform,
            targetTransform = targetTransform,
            spawnPosition = startPosition
        };
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

}