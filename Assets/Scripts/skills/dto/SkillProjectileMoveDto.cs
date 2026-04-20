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
        Warp = 2,
        Hover = 3,
        Orbit = 4
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

    // =========================
    // Hover / Follow Movement
    // =========================

    /// <summary>
    /// Hover/Follow 시 사용할 오프셋 (owner 기준)
    /// </summary>
    public Vector2 followOffset = Vector2.zero;

    /// <summary>
    /// 따라가는 속도 (보간 속도)
    /// </summary>
    public float followLerpSpeed = 12f;

    /// <summary>
    /// 초기 생성 시 즉시 위치를 맞출지 여부
    /// </summary>
    public bool snapOnInitialize = true;

    /// <summary>
    /// hover(부유) 연출 사용 여부
    /// </summary>
    public bool useHoverMotion = true;

    /// <summary>
    /// hover 진폭
    /// </summary>
    public float hoverAmplitude = 0.15f;

    /// <summary>
    /// hover 주파수
    /// </summary>
    public float hoverFrequency = 2.5f;

    /// <summary>
    /// hover 방향 축
    /// </summary>
    public Vector2 hoverAxis = Vector2.up;

    /// <summary>
    /// owner가 사라지면 이동 종료 여부
    /// </summary>
    public bool endWhenOwnerMissing = true;

    // =========================
    // Orbit Movement
    // =========================

    /// <summary>
    /// 궤도 반지름 (owner 기준)
    /// </summary>
    public float orbitRadius = 1.5f;

    /// <summary>
    /// 각속도 (deg/sec)
    /// </summary>
    public float orbitAngularSpeed = 180f;

    /// <summary>
    /// 시계 방향 회전 여부
    /// </summary>
    public bool clockwise = false;

    /// <summary>
    /// 현재 투사체의 순서 (0 ~ max-1)
    /// </summary>
    public int spawnOrder = 0;

    /// <summary>
    /// 최대 투사체 개수 (업그레이드로 변할 수 있음)
    /// </summary>
    public int maxProjectileCount = 1;

    /// <summary>
    /// 레이아웃 변경 시 위상 리셋 여부
    /// </summary>
    public bool resetPhaseWhenLayoutChanges = true;

    /// <summary>
    /// 반지름을 진동시키는 효과 사용 여부
    /// </summary>
    public bool useRadialPulse = false;

    /// <summary>
    /// 반지름 진동 크기
    /// </summary>
    public float radialPulseAmplitude = 0f;

    /// <summary>
    /// 반지름 진동 주파수
    /// </summary>
    public float radialPulseFrequency = 0f;

    public Vector2 GetDirection()
    {
        Vector2 delta = targetPosition - startPosition;
        if (delta.sqrMagnitude <= 0.0001f)
            return Vector2.right;

        return delta.normalized;
    }
}