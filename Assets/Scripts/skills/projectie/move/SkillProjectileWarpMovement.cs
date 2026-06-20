using UnityEngine;
using Skills.Dto.Move;
public class SkillProjectileWarpMovement : ISkillProjectileMovement
{
    private SkillProjectileMovementContext _context;
    private SkillMoveRuntimeDto _dto;
    private bool _initialized;
    public Transform TargetTransform => _context.projectileTransform;
    public bool IsInitialized => _initialized;

    public void Initialize(object dto)
    {
        if (dto is WarpProjectileMoveDto typed)
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

    public void Initialize(WarpProjectileMoveDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("WarpProjectileMoveDto is null");
            return;
        }

        _dto = dto;

        if (_context.projectileTransform != null)
        {
            _context.projectileTransform.position = dto.targetPosition;
        }
        _initialized = true;
    }

    public void TickMovement(float deltaTime)
    {
        // Warp movement is completed at initialization.
    }

    public bool HasReachedEnd()
    {
        return _initialized;
    }

    public void ResetMovement()
    {
        _context = default;
        _dto = null;
        _initialized = false;
    }

    public Vector2 GetDirection()
    {
        {
            return Vector2.zero;
        }
    }

    public Vector2 GetPosition()
    {
        {
            return _context.projectileTransform != null
                ? (Vector2)_context.projectileTransform.position
                : (_dto is WarpProjectileMoveDto warpDto
                    ? warpDto.targetPosition
                    : Vector2.zero);
        }
    }
}
