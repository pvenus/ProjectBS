using UnityEngine;

/// <summary>
/// Projectile 이동 전용 DTO.
/// - 시작/목표 좌표 기반 이동을 기본으로 한다.
/// - 직선 이동과 순간 이동(워프)을 포함한다.
/// </summary>
[System.Serializable]
public class SkillProjectileMoveDto
{
    public enum MoveType
    {
        None = 0,
        Linear = 1,
        Warp = 2
    }

    /// <summary>
    /// 이동 방식.
    /// Linear: 시작점에서 목표점까지 속도 기반 이동.
    /// Warp: 시작 직후 목표 지점으로 즉시 이동.
    /// </summary>
    public MoveType moveType = MoveType.Linear;

    /// <summary>
    /// 실제 이동을 적용할 대상 Transform.
    /// 일반적으로 projectile 본체 Transform.
    /// </summary>
    public Transform targetTransform;

    /// <summary>
    /// 이동 시작 좌표.
    /// </summary>
    public Vector2 startPosition;

    /// <summary>
    /// 이동 목표 좌표.
    /// </summary>
    public Vector2 targetPosition;

    /// <summary>
    /// 직선 이동 시 속도.
    /// Warp에서는 무시될 수 있다.
    /// </summary>
    public float speed;

    /// <summary>
    /// 목표 지점 도착 판정 오차 범위.
    /// 너무 작은 값으로 인한 떨림 방지용.
    /// </summary>
    public float arrivalThreshold = 0.01f;

    /// <summary>
    /// 이동 방향 기반으로 회전을 적용할지 여부.
    /// </summary>
    public bool applyDirectionRotation;

    /// <summary>
    /// 회전을 적용할 대상 Transform.
    /// null이면 targetTransform 사용.
    /// </summary>
    public Transform rotationTarget;

    /// <summary>
    /// 방향 회전에 추가로 적용할 각도 오프셋.
    /// </summary>
    public float rotationOffset;

    public Vector2 GetDirection()
    {
        Vector2 delta = targetPosition - startPosition;
        if (delta.sqrMagnitude <= 0.0001f)
            return Vector2.right;

        return delta.normalized;
    }
}