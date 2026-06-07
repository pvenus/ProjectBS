

using UnityEngine;

/// <summary>
/// 호밍 투사체 이동 설정 DTO.
/// 세부 호밍 정책은 SkillProjectileHomingMovement 내부 기본값을 사용하고,
/// 외부에서는 이동 속도와 도착 판정 정도만 전달한다.
/// </summary>
[System.Serializable]
public class HomingMovementDto
{    
    public Transform targetTransform;
    /// <summary>
    /// 유도탄 이동 속도.
    /// </summary>
    public float speed = 12f;

    /// <summary>
    /// 타겟 도착 판정 거리.
    /// 0 이하이면 HomingMovement 내부 기본값을 사용한다.
    /// </summary>
    public float arrivalThreshold = 0.05f;
}