

using UnityEngine;

/// <summary>
/// ProjectileEntity의 생명주기를 관리하는 컴포넌트.
/// 런타임 데이터의 lifetime 값을 기준으로 시간을 누적하고,
/// 수명이 끝나면 ProjectileEntity를 종료한다.
/// </summary>
public class ProjectileLifetime : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool initialized;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float elapsedTime;

    private ProjectileEntity owner;
    private ProjectileRuntimeData runtimeData;

    public bool IsInitialized => initialized;
    public float Lifetime => lifetime;
    public float ElapsedTime => elapsedTime;
    public float RemainingTime => Mathf.Max(0f, lifetime - elapsedTime);

    public void Initialize(ProjectileEntity ownerEntity, ProjectileRuntimeData data)
    {
        if (ownerEntity == null)
        {
            Debug.LogError("ProjectileLifetime.Initialize failed: ownerEntity is null.", this);
            return;
        }

        if (data == null)
        {
            Debug.LogError("ProjectileLifetime.Initialize failed: ProjectileRuntimeData is null.", this);
            return;
        }

        owner = ownerEntity;
        runtimeData = data;
        initialized = true;
        elapsedTime = 0f;
        lifetime = Mathf.Max(0f, data.lifetime);
    }

    private void Update()
    {
        if (!initialized || owner == null || runtimeData == null)
        {
            return;
        }

        if (lifetime <= 0f)
        {
            owner.Despawn();
            return;
        }

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifetime)
        {
            owner.Despawn();
        }
    }

    public void ResetLifetime(float newLifetime)
    {
        lifetime = Mathf.Max(0f, newLifetime);
        elapsedTime = 0f;
    }

    public void AddLifetime(float additionalTime)
    {
        lifetime = Mathf.Max(0f, lifetime + additionalTime);
    }
}