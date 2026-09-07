using UnityEngine;

/// <summary>
/// KnockbackController
///
/// 강제 이동 계열 중 넉백만 담당하는 공용 컨트롤러.
/// - 일반 이동(MovementController)과 분리한다.
/// - 넉백 중에는 MovementController를 잠시 정지시킨다.
/// - Rigidbody2D.MovePosition 기반으로 안정적으로 밀어낸다.
/// - NPC / 파티원 / 플레이어 공용으로 사용할 수 있다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Rigidbody2D targetRigidbody;
    [SerializeField] private MovementController movementController;

    [Header("Knockback")]
    [SerializeField, Min(0.01f)] private float defaultDuration = 0.18f;
    [SerializeField, Range(0f, 1f)] private float dampingPerStep = 0.85f;
    [SerializeField, Min(0.1f)] private float maxSpeed = 18f;
    [SerializeField] private bool restoreMovementAfterKnockback = true;
    [SerializeField] private bool stopMovementOnKnockbackStart = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog;

    private Vector2 _knockVelocity;
    private float _knockTimer;
    private bool _movementWasActiveBeforeKnockback;
    private bool _isKnockingBack;

    public bool IsKnockingBack => _isKnockingBack && _knockTimer > 0f;
    public Vector2 CurrentVelocity => _knockVelocity;
    public float RemainingTime => Mathf.Max(0f, _knockTimer);

    private void Reset()
    {
        targetRigidbody = GetComponent<Rigidbody2D>();
        movementController = GetComponent<MovementController>();
    }

    private void Awake()
    {
        if (targetRigidbody == null)
            targetRigidbody = GetComponent<Rigidbody2D>();

        if (movementController == null)
            movementController = GetComponent<MovementController>();

        defaultDuration = Mathf.Max(0.01f, defaultDuration);
        maxSpeed = Mathf.Max(0.1f, maxSpeed);
    }

    private void FixedUpdate()
    {
        if (targetRigidbody == null)
            return;

        if (_knockTimer <= 0f)
        {
            if (_isKnockingBack)
                EndKnockback();
            return;
        }

        _isKnockingBack = true;
        _knockTimer -= Time.fixedDeltaTime;

        Vector2 currentPosition = targetRigidbody.position;
        Vector2 delta = _knockVelocity * Time.fixedDeltaTime;
        targetRigidbody.MovePosition(currentPosition + delta);

        _knockVelocity *= Mathf.Clamp01(dampingPerStep);

        if (_knockTimer > 0f)
            return;

        EndKnockback();
    }

    /// <summary>
    /// 기본 duration으로 넉백을 적용한다.
    /// velocity는 초당 이동 속도 기준이다.
    /// </summary>
    public void ApplyKnockback(Vector2 velocity)
    {
        ApplyKnockback(velocity, defaultDuration);
    }

    /// <summary>
    /// 지정한 duration으로 넉백을 적용한다.
    /// velocity는 초당 이동 속도 기준이다.
    /// </summary>
    public void ApplyKnockback(Vector2 velocity, float duration)
    {
        if (targetRigidbody == null)
            return;

        float safeDuration = Mathf.Max(0.01f, duration);
        Vector2 clampedVelocity = ClampVelocity(velocity);

        if (clampedVelocity.sqrMagnitude <= 0.0001f)
            return;

        if (!_isKnockingBack)
            CacheMovementState();

        if (stopMovementOnKnockbackStart && movementController != null)
            movementController.Stop();

        _knockVelocity = clampedVelocity;
        _knockTimer = Mathf.Max(_knockTimer, safeDuration);
        _isKnockingBack = true;

        if (debugLog)
            Debug.Log($"[KnockbackController] ApplyKnockback / velocity={_knockVelocity} / duration={_knockTimer:0.000}", this);
    }

    /// <summary>
    /// 방향과 힘으로 넉백을 적용한다.
    /// force는 속도 크기로 사용된다.
    /// </summary>
    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (direction.sqrMagnitude <= 0.0001f || force <= 0f)
            return;

        ApplyKnockback(direction.normalized * force, duration);
    }

    /// <summary>
    /// 넉백을 즉시 종료한다.
    /// </summary>
    public void StopKnockback(bool restoreMovement = true)
    {
        _knockTimer = 0f;
        _knockVelocity = Vector2.zero;

        bool shouldRestore = restoreMovement && restoreMovementAfterKnockback;
        EndKnockback(shouldRestore);
    }

    private Vector2 ClampVelocity(Vector2 velocity)
    {
        float safeMaxSpeed = Mathf.Max(0.1f, maxSpeed);
        float sqrMax = safeMaxSpeed * safeMaxSpeed;

        if (velocity.sqrMagnitude > sqrMax)
            return velocity.normalized * safeMaxSpeed;

        return velocity;
    }

    private void CacheMovementState()
    {
        _movementWasActiveBeforeKnockback = movementController != null && movementController.IsMoving;
    }

    private void EndKnockback()
    {
        EndKnockback(restoreMovementAfterKnockback);
    }

    private void EndKnockback(bool restoreMovement)
    {
        _knockTimer = 0f;
        _knockVelocity = Vector2.zero;
        _isKnockingBack = false;

        if (restoreMovement && _movementWasActiveBeforeKnockback && movementController != null)
        {
            // 일반 이동 재개는 상위 로직이 다시 목표를 밀어넣는 구조를 가정한다.
            // 여기서는 Stop 상태만 해제하지 않고, 단순히 복구 가능 상태로만 둔다.
        }

        _movementWasActiveBeforeKnockback = false;

        if (debugLog)
            Debug.Log("[KnockbackController] EndKnockback", this);
    }
}
