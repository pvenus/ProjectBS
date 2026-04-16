

using UnityEngine;

/// <summary>
/// MovementMono
///
/// 이동 관련 컨트롤러들을 한 번에 생성하고 접근하기 위한 메인 허브 컴포넌트.
/// 프리팹마다 MovementController / KnockbackController 를 개별로 직접 붙이지 않아도,
/// 이 컴포넌트 하나만 붙이면 필요한 컨트롤러를 자동으로 보장한다.
/// </summary>
[DisallowMultipleComponent]
public class MovementMono : MonoBehaviour
{
    [Header("Auto Create")]
    [SerializeField] private bool ensureMovementController = true;
    [SerializeField] private bool ensureKnockbackController = true;

    [Header("Reference")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private KnockbackController knockbackController;
    [SerializeField] private Rigidbody2D targetRigidbody;

    [Header("Debug")]
    [SerializeField] private bool debugLog;

    public MovementController MovementController => movementController;
    public KnockbackController KnockbackController => knockbackController;
    public Rigidbody2D TargetRigidbody => targetRigidbody;

    private void Reset()
    {
        CacheOrCreateControllers();
    }

    private void Awake()
    {
        CacheOrCreateControllers();
    }

    public void EnsureControllers()
    {
        CacheOrCreateControllers();
    }

    public void StopAllMotion(bool stopKnockback = true)
    {
        if (movementController != null)
            movementController.Stop();

        if (stopKnockback && knockbackController != null)
            knockbackController.StopKnockback(restoreMovement: false);
    }

    public bool IsMoving()
    {
        return movementController != null && movementController.IsMoving;
    }

    public bool IsKnockingBack()
    {
        return knockbackController != null && knockbackController.IsKnockingBack;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration = -1f)
    {
        if (knockbackController == null)
            return;

        if (duration > 0f)
            knockbackController.ApplyKnockback(direction, force, duration);
        else
            knockbackController.ApplyKnockback(direction.normalized * Mathf.Max(0f, force));
    }

    private void CacheOrCreateControllers()
    {
        if (targetRigidbody == null)
            targetRigidbody = GetComponent<Rigidbody2D>();

        if (ensureMovementController)
        {
            if (movementController == null)
                movementController = GetComponent<MovementController>();

            if (movementController == null)
                movementController = gameObject.AddComponent<MovementController>();
        }
        else if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }

        if (ensureKnockbackController)
        {
            if (knockbackController == null)
                knockbackController = GetComponent<KnockbackController>();

            if (knockbackController == null)
                knockbackController = gameObject.AddComponent<KnockbackController>();
        }
        else if (knockbackController == null)
        {
            knockbackController = GetComponent<KnockbackController>();
        }

        if (debugLog)
        {
            Debug.Log(
                $"[MovementMono] Ready / movementController={(movementController != null)} / knockbackController={(knockbackController != null)} / rigidbody={(targetRigidbody != null)}",
                this
            );
        }
    }
}