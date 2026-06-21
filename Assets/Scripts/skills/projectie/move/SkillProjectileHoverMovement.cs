using UnityEngine;
using Skills.Dto.Move;

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
    private HoverProjectileMoveDto _dto;
    private SkillProjectileMovementContext _context;
    private bool _initialized;
    private Vector2 _direction;
    public Transform TargetTransform => _context.projectileTransform;

    public void Initialize(object dto)
    {
        if (dto is HoverProjectileMoveDto typed)
        {
            Initialize(typed);
        }
        else
        {
            Debug.LogError("Invalid DTO type for SkillProjectileHoverMovement");
        }
    }

    public void Initialize(HoverProjectileMoveDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("HoverProjectileMoveDto is null");
            return;
        }

        if (_context.projectileTransform == null)
        {
            Debug.LogError("SkillProjectileHoverMovement moving projectile transform is null");
            return;
        }

        _dto = dto;
        _initialized = true;

        if (_context.owner != null)
        {
            Vector2 ownerPosition = _context.owner.position;
            Vector2 targetPosition = _context.targetTransform.position;
            Vector2 direction = targetPosition - ownerPosition;

            _direction = direction.sqrMagnitude > Mathf.Epsilon
                ? direction.normalized
                : Vector2.right;
        }

        if (_context.owner != null)
        {
            Vector2 target = EvaluateBaseFollowPosition();
            _context.projectileTransform.position = target;
        }
    }

    public void SetContext(SkillProjectileMovementContext context)
    {
        _context = context;
    }

    public void TickMovement(float deltaTime)
    {
        if (!_initialized)
            return;

        if (_context.owner == null)
            return;

        if (_context.projectileTransform == null)
            return;

        _context.projectileTransform.position = EvaluateBaseFollowPosition();

    }

    public bool HasReachedEnd()
    {
        return false;
    }

    public void ResetMovement()
    {
        _dto = null;
        _context = default;
        _initialized = false;
        _direction = Vector2.zero;
    }

    public Vector2 GetDirection()
    {
        return _direction;
    }

    public Vector2 GetPosition()
    {
        return _context.projectileTransform.position;
    }

    private Vector2 EvaluateBaseFollowPosition()
    {
        if (_context.owner == null)
            return GetPosition();

        Vector2 ownerPosition = _context.owner.position;
        Vector2 offset = _dto != null ? _dto.followOffset : Vector2.zero;
        return ownerPosition + offset;
    }
}