using UnityEngine;

/// <summary>
/// 신규 투사체 런타임 엔티티의 최상위 허브 Mono.
/// 이 객체는 계산을 직접 하지 않고,
/// 이미 Resolver에서 완성된 ProjectileRuntimeData를 받아
/// 이동 / 히트 / 수명 컴포넌트에 전달하는 역할만 담당한다.
/// </summary>
public class ProjectileEntity : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ProjectileMovement movement;
    [SerializeField] private ProjectileHitHandler hitHandler;
    [SerializeField] private ProjectileLifetime lifetime;
    [SerializeField] private ProjectileVisual visual;

    [Header("Runtime State")]
    [SerializeField] private bool initialized;

    private ProjectileRuntimeData runtimeData;

    public bool IsInitialized => initialized;
    public ProjectileRuntimeData RuntimeData => runtimeData;
    public ProjectileVisual Visual => visual;

    private void Reset()
    {
        movement = GetComponent<ProjectileMovement>();
        hitHandler = GetComponent<ProjectileHitHandler>();
        lifetime = GetComponent<ProjectileLifetime>();
        visual = GetComponent<ProjectileVisual>();
    }

    private void Awake()
    {
        if (movement == null)
        {
            movement = GetComponent<ProjectileMovement>();
        }

        if (hitHandler == null)
        {
            hitHandler = GetComponent<ProjectileHitHandler>();
        }

        if (lifetime == null)
        {
            lifetime = GetComponent<ProjectileLifetime>();
        }

        if (visual == null)
        {
            visual = GetComponent<ProjectileVisual>();
        }
    }

    /// <summary>
    /// Resolver / Factory가 생성한 최종 런타임 데이터를 주입한다.
    /// </summary>
    public void Initialize(ProjectileRuntimeData data)
    {
        if (data == null)
        {
            Debug.LogError("ProjectileRuntimeData is null.", this);
            return;
        }

        runtimeData = data;
        initialized = true;

        transform.position = data.spawnPosition;

        if (movement != null)
        {
            movement.Initialize(this, data);
        }

        if (hitHandler != null)
        {
            hitHandler.Initialize(this, data);
        }

        if (lifetime != null)
        {
            lifetime.Initialize(this, data);
        }

        if (visual != null)
        {
            visual.Initialize(this, data);
        }
    }

    /// <summary>
    /// 투사체를 종료한다.
    /// 현재는 단순 Destroy 기반으로 처리한다.
    /// 이후 풀링 구조가 들어오면 여기서 반환 처리로 교체할 수 있다.
    /// </summary>
    public void Despawn()
    {
        initialized = false;
        runtimeData = null;
        if (visual != null)
        {
            visual.OnDespawn();
        }
        Destroy(gameObject);
    }

    public Vector2 GetDirection()
    {
        if (runtimeData == null)
        {
            return Vector2.right;
        }

        return runtimeData.NormalizedDirection;
    }

    public GameObject GetOwner()
    {
        return runtimeData != null ? runtimeData.owner : null;
    }

    public GameObject GetTarget()
    {
        return runtimeData != null ? runtimeData.target : null;
    }
}