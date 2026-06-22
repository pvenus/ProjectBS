using UnityEngine;
using Skills.Dto.Move;

public class SkillProjectileLinearMovement : ISkillProjectileMovement
{
    // DTO는 외부 입력 설정값을 보관하고, 계산/변경되는 상태만 내부 필드가 소유한다.
    private LinearProjectileMoveDto _dto;
    private SkillProjectileMovementContext _context;
    private Vector2 _direction = Vector2.right;
    private bool _initialized;

    public Transform MovingTransform => _context.projectileTransform;
    public bool IsInitialized => _initialized;

    public void Initialize(object dto)
    {
        if (dto is LinearProjectileMoveDto typed)
        {
            Initialize(typed);
        }
        else
        {
            Debug.LogError("Invalid DTO type for SkillProjectileLinearMovement");
        }
    }

    public void SetContext(SkillProjectileMovementContext context)
    {
        _context = context;
    }

    public void Initialize(LinearProjectileMoveDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("LinearProjectileMoveDto is null");
            return;
        }

        _dto = dto;
        if (_context.projectileTransform == null)
        {
            Debug.LogError("SkillProjectileLinearMovement moving projectile transform is null");
            return;
        }

        Vector2 delta = _dto.targetPosition - _dto.startPosition;
        _direction = delta.sqrMagnitude <= 0.0001f
            ? Vector2.right
            : delta.normalized;
        _initialized = true;

        _context.projectileTransform.position = _dto.startPosition;
    }

    public void TickMovement(float deltaTime)
    {
        if (!_initialized || _dto == null || _context.projectileTransform == null)
            return;

        float step = Mathf.Max(0f, _dto.speed) * deltaTime;
        if (step <= 0f)
            return;

        _context.projectileTransform.position += (Vector3)(_direction * step);
    }

    public bool HasReachedEnd()
    {
        return false;
    }

    public void ResetMovement()
    {
        _dto = null;
        _context = default;
        _direction = Vector2.right;
        _initialized = false;
    }

    public Vector2 GetDirection()
    {
        return _direction;
    }

    public Vector2 GetPosition()
    {
        return _context.projectileTransform != null ? (Vector2)_context.projectileTransform.position : Vector2.zero;
    }
}