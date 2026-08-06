

using UnityEngine;

/// <summary>
/// MovementController
///
/// 순수 이동 전용 컨트롤러.
/// - Rigidbody2D velocity 제어만 담당한다.
/// - 어디로 이동할지 결정하는 AI/전술 로직은 포함하지 않는다.
/// - 플레이어 / NPC / 파티원 공통으로 재사용할 수 있다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : MonoBehaviour
{
    public enum MoveMode
    {
        None,
        Direction,
        Target
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float arriveDistance = 0.08f;
    [SerializeField] private bool stopOnDisable = true;

    private Rigidbody2D _rb;
    private MoveMode _moveMode = MoveMode.None;
    private Vector2 _moveDirection = Vector2.zero;
    private Vector2 _targetPosition = Vector2.zero;
    private bool _hasReachedTarget = false;

    public float MoveSpeed => moveSpeed;
    public float ArriveDistance => arriveDistance;
    public MoveMode CurrentMode => _moveMode;
    public Vector2 CurrentVelocity => _rb != null ? _rb.linearVelocity : Vector2.zero;
    public Vector2 CurrentDirection => _moveDirection;
    public Vector2 CurrentTargetPosition => _targetPosition;
    public bool IsMoving => _moveMode != MoveMode.None && CurrentVelocity.sqrMagnitude > 0.0001f;
    public bool HasReachedTarget => _moveMode == MoveMode.Target && _hasReachedTarget;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        moveSpeed = Mathf.Max(0f, moveSpeed);
        arriveDistance = Mathf.Max(0.001f, arriveDistance);
    }

    private void FixedUpdate()
    {
        switch (_moveMode)
        {
            case MoveMode.Direction:
                TickDirectionMove();
                break;

            case MoveMode.Target:
                TickTargetMove();
                break;

            default:
                if (_rb != null)
                    _rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    public void SetArriveDistance(float distance)
    {
        arriveDistance = Mathf.Max(0.001f, distance);
    }

    /// <summary>
    /// 방향 기반 지속 이동.
    /// </summary>
    public void MoveByDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            Stop();
            return;
        }

        _moveDirection = direction.normalized;
        _moveMode = MoveMode.Direction;
        _hasReachedTarget = false;
    }

    /// <summary>
    /// 목표 지점까지 이동.
    /// 도착하면 자동 정지한다.
    /// </summary>
    public void MoveTo(Vector2 targetPosition)
    {
        _targetPosition = targetPosition;
        _moveMode = MoveMode.Target;
        _hasReachedTarget = false;
    }

    /// <summary>
    /// 외부에서 현재 목표 위치를 바꾸고 싶을 때 사용.
    /// 이미 Target 모드가 아니어도 Target 모드로 진입한다.
    /// </summary>
    public void SetTargetPosition(Vector2 targetPosition)
    {
        MoveTo(targetPosition);
    }

    public float GetDistanceToTarget()
    {
        if (_moveMode != MoveMode.Target || _rb == null)
            return 0f;

        return Vector2.Distance(_rb.position, _targetPosition);
    }

    public bool IsWithinDistance(Vector2 targetPosition, float distance)
    {
        if (_rb == null)
            return false;

        return Vector2.Distance(_rb.position, targetPosition) <= Mathf.Max(0f, distance);
    }

    public void Stop()
    {
        _moveMode = MoveMode.None;
        _moveDirection = Vector2.zero;
        _hasReachedTarget = false;

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    private void TickDirectionMove()
    {
        if (_rb == null)
            return;

        _rb.linearVelocity = _moveDirection * moveSpeed;
    }

    private void TickTargetMove()
    {
        if (_rb == null)
            return;

        Vector2 currentPosition = _rb.position;
        Vector2 delta = _targetPosition - currentPosition;

        if (delta.magnitude <= arriveDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            _moveDirection = Vector2.zero;
            _hasReachedTarget = true;
            _moveMode = MoveMode.None;
            return;
        }

        _moveDirection = delta.normalized;
        _rb.linearVelocity = _moveDirection * moveSpeed;
        _hasReachedTarget = false;
    }

    private void OnDisable()
    {
        if (stopOnDisable)
            Stop();
    }
}