using UnityEngine;

public interface ISkillProjectileMovement
{
    /// <summary>
    /// 공통 초기화 (각 구현체에서 내부적으로 DTO 캐스팅)
    /// </summary>
    void Initialize(object dto);

    /// <summary>
    /// 매 프레임 이동 처리
    /// </summary>
    void TickMovement(float deltaTime);

    /// <summary>
    /// 이동 종료 여부
    /// </summary>
    bool HasReachedEnd();

    /// <summary>
    /// 초기 상태로 리셋
    /// </summary>
    void ResetMovement();

    /// <summary>
    /// 현재 이동 방향
    /// </summary>
    Vector2 GetDirection();

    /// <summary>
    /// 현재 위치
    /// </summary>
    Vector2 GetPosition();
}
