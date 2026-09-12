using Skill;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 신규 투사체 런타임 엔티티의 최상위 허브 Mono.
/// 이 객체는 계산을 직접 하지 않고,
/// 이미 Resolver에서 완성된 ProjectileRuntimeData를 받아
/// 이동 / 히트 / 수명 컴포넌트에 전달하는 역할만 담당한다.
/// </summary>
public class ProjectileEntity : MonoBehaviour
{
    private static readonly HashSet<ProjectileEntity> Live = new();
    [Header("Components")]
    [SerializeField] private ProjectileMovement movement;
    [SerializeField] private ProjectileHitHandler hitHandler;
    [SerializeField] private ProjectileLifetime lifetime;
    [SerializeField] private ProjectileVisual visual;
    [SerializeField] private ProjectileSpawner spawner;
    [SerializeField] private Transform scaler;

    [Header("Runtime State")]
    [SerializeField] private bool initialized;
    [SerializeField] private bool waitingForVisualCompletion;

    private ProjectileRuntimeData runtimeData;
    private float initializedAtTime;

    public bool IsInitialized => initialized;
    public ProjectileRuntimeData RuntimeData => runtimeData;
    public ProjectileVisual Visual => visual;
    public ProjectileSpawner Spawner => spawner;

    private void Reset()
    {
        movement = GetComponent<ProjectileMovement>();
        hitHandler = GetComponent<ProjectileHitHandler>();
        lifetime = GetComponent<ProjectileLifetime>();
        visual = GetComponent<ProjectileVisual>();
        spawner = GetComponent<ProjectileSpawner>();
        scaler = transform.Find("Scaler");
    }

    private void Awake()
    {
        if (movement == null)
        {
            movement = GetOrAddComponent<ProjectileMovement>();
        }

        if (hitHandler == null)
        {
            hitHandler = GetOrAddComponent<ProjectileHitHandler>();
        }

        if (lifetime == null)
        {
            lifetime = GetOrAddComponent<ProjectileLifetime>();
        }

        if (visual == null)
        {
            visual = GetOrAddComponent<ProjectileVisual>();
        }

        if (spawner == null)
        {
            spawner = GetOrAddComponent<ProjectileSpawner>();
        }
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component != null)
        {
            return component;
        }
        return gameObject.AddComponent<T>();
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
        LayeredProjectilePresentationProfileSO layeredProfile =
            data.sourceEquipment?.BaseVisualSo?.LayeredPresentationProfile;
        if (layeredProfile != null && layeredProfile.HasRequiredGroundField())
        {
            data.minimumVisualLifetime = Mathf.Max(
                data.minimumVisualLifetime,
                layeredProfile.ResolveMaximumLayerEnd());
        }
        Live.Add(this);
        initialized = true;
        waitingForVisualCompletion = false;
        initializedAtTime = Time.time;

        transform.position = data.spawnPosition;

        if (scaler != null)
        {
            float radius = data.hit != null
                ? Mathf.Max(0.01f, data.hit.projectileColliderRadius)
                : 1f;
            scaler.localScale = Vector3.one * radius;
        }

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

        if (spawner != null)
        {
            spawner.Initialize(this, data);
        }
    }

    /// <summary>
    /// 투사체를 종료한다.
    /// 현재는 단순 Destroy 기반으로 처리한다.
    /// 이후 풀링 구조가 들어오면 여기서 반환 처리로 교체할 수 있다.
    /// </summary>
    public void Despawn()
    {
        if (initialized && spawner != null)
        {
            spawner.TrySpawnChildSkill(SpawnSkillTiming.OnProjectileEnd);
        }

        initialized = false;
        Live.Remove(this);
        runtimeData = null;
        if (visual != null)
        {
            visual.OnDespawn();
        }
        Destroy(gameObject);
    }

    public static void DespawnVisualsFor(GameObject owner, string equipmentId)
    {
        if (owner == null || string.IsNullOrEmpty(equipmentId)) return;
        ProjectileEntity[] snapshot = new ProjectileEntity[Live.Count];
        Live.CopyTo(snapshot);
        for (int i = 0; i < snapshot.Length; i++)
        {
            ProjectileEntity entity = snapshot[i];
            ProjectileRuntimeData data = entity != null ? entity.runtimeData : null;
            if (data != null && data.owner == owner &&
                data.sourceEquipment != null &&
                string.Equals(data.sourceEquipment.EquipmentId, equipmentId,
                    System.StringComparison.Ordinal))
            {
                // Presentation hold cancellation is not a gameplay projectile-end receipt.
                entity.initialized = false;
                entity.Despawn();
            }
        }
    }

    private void OnDestroy()
    {
        Live.Remove(this);
    }

    /// <summary>
    /// 충돌/적용 횟수가 소진된 뒤 gameplay 충돌만 즉시 종료하고,
    /// 현재 클립의 남은 한 cycle을 표시한 다음 객체를 제거한다.
    /// </summary>
    public void CompleteCollisionAndDespawnAfterVisual()
    {
        if (waitingForVisualCompletion)
        {
            return;
        }

        waitingForVisualCompletion = true;
        initialized = false;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        // ProjectileLifetime의 즉시 Despawn이 남은 clip 재생을 자르지 않게 한다.
        if (lifetime != null)
        {
            lifetime.enabled = false;
        }

        float remainingVisualTime = visual != null
            ? visual.GetRemainingCurrentClipPlaybackTime()
            : 0f;

        // A playable may transiently report zero remaining time before its
        // first rendered evaluation. Keep the detached visual alive for the
        // authored spawn-to-stop duration regardless of graph timing.
        float elapsedSinceInitialize = Mathf.Max(0f, Time.time - initializedAtTime);
        float remainingMinimumTime = runtimeData != null
            ? Mathf.Max(0f, runtimeData.minimumVisualLifetime - elapsedSinceInitialize)
            : 0f;
        remainingVisualTime = Mathf.Max(remainingVisualTime, remainingMinimumTime);

        if (remainingVisualTime <= 0f)
        {
            Despawn();
            return;
        }

        // minimumVisualLifetime is authored to include the terminal frame's
        // dwell. Adding another rendered-frame grace period here makes only
        // the final sprite linger longer than the preceding samples.
        Destroy(gameObject, remainingVisualTime);
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
