using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple registered-point spawner.
///
/// Concept:
/// - Each spawner GameObject itself acts as a spawn point.
/// - Multiple spawners can be placed in the scene and controlled by a higher-level system.
/// - This spawner does not manage stage time or wave progression.
/// - It only receives/keeps a monster pool and keeps spawning from that pool.
///
/// Typical usage:
/// 1. Place multiple empty GameObjects in the scene as spawn points.
/// 2. Register those Transforms in this component.
/// 3. Fill the monster entries list for the current wave/group.
/// 4. Let this component keep spawning from the registered monster pool.
/// </summary>
public class NpcSpawnerMono : MonoBehaviour
{
    [System.Serializable]
    public class MonsterSpawnEntry
    {
        [Tooltip("Monster prefab to spawn.")]
        public GameObject prefab;

        [Tooltip("Relative selection weight. Higher = spawned more often.")]
        [Min(0f)] public float weight = 1f;

        [Tooltip("How many of this monster may be spawned in total. -1 = unlimited.")]
        public int maxSpawnCount = -1;

        [HideInInspector] public int spawnedCount;

        public bool CanSpawn()
        {
            if (prefab == null)
                return false;

            if (weight <= 0f)
                return false;

            return maxSpawnCount < 0 || spawnedCount < maxSpawnCount;
        }
    }

    [Header("Monster Pool")]
    [SerializeField] private List<MonsterSpawnEntry> monsterPool = new List<MonsterSpawnEntry>();

    [Header("Spawn Timing")]
    [SerializeField] private bool autoSpawn = true;
    [SerializeField, Min(0.05f)] private float spawnInterval = 1.5f;
    [SerializeField, Min(1)] private int spawnBurst = 1;

    [Header("Spawn Limits")]
    [SerializeField] private bool respectAliveLimit = true;
    [SerializeField, Min(1)] private int maxAliveCount = 30;

    [Header("Spawn Validation")]
    [SerializeField] private bool validateByOverlap = false;
    [SerializeField, Min(0.01f)] private float spawnCheckRadius = 0.4f;
    [SerializeField] private LayerMask blockedMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float gizmoRadius = 0.25f;

    private readonly List<GameObject> _alive = new List<GameObject>();
    private float _spawnTimer;

    public IReadOnlyList<MonsterSpawnEntry> MonsterPool => monsterPool;
    public int AliveCount
    {
        get
        {
            CleanupDead();
            return _alive.Count;
        }
    }

    private void Awake()
    {
        _spawnTimer = spawnInterval;
    }

    private void Update()
    {
        CleanupDead();

        if (!autoSpawn)
            return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer > 0f)
            return;

        _spawnTimer = spawnInterval;
        SpawnBurst();
    }

    public void SetMonsterPool(List<MonsterSpawnEntry> newPool, bool resetSpawnCounts = true)
    {
        monsterPool = newPool ?? new List<MonsterSpawnEntry>();

        if (resetSpawnCounts)
            ResetMonsterPoolProgress();
    }

    public void ResetMonsterPoolProgress()
    {
        if (monsterPool == null)
            return;

        for (int i = 0; i < monsterPool.Count; i++)
        {
            if (monsterPool[i] != null)
                monsterPool[i].spawnedCount = 0;
        }
    }

    public int SpawnBurst()
    {
        CleanupDead();

        if (monsterPool == null || monsterPool.Count == 0)
        {
            if (debugLog)
                Debug.LogWarning($"[NpcSpawnerMono] Monster pool is empty on {name}.", this);
            return 0;
        }

        int spawned = 0;
        for (int i = 0; i < spawnBurst; i++)
        {
            if (respectAliveLimit && _alive.Count >= maxAliveCount)
                break;

            if (!TrySpawnOne())
                break;

            spawned++;
        }

        return spawned;
    }

    public bool TrySpawnOne()
    {
        CleanupDead();

        if (respectAliveLimit && _alive.Count >= maxAliveCount)
            return false;

        MonsterSpawnEntry entry = PickMonsterEntry();
        if (entry == null)
            return false;

        Transform point = PickSpawnPoint();
        if (point == null)
            return false;

        Vector3 spawnPos = point.position;
        if (validateByOverlap && !IsSpawnPositionFree(spawnPos))
        {
            if (debugLog)
                Debug.Log($"[NpcSpawnerMono] Spawn blocked at point={point.name} pos={spawnPos}", this);
            return false;
        }

        GameObject spawned = Instantiate(entry.prefab, spawnPos, point.rotation);
        if (spawned == null)
            return false;

        _alive.Add(spawned);
        entry.spawnedCount++;

        if (debugLog)
        {
            Debug.Log($"[NpcSpawnerMono] Spawned prefab={entry.prefab.name} point={point.name} alive={_alive.Count}", this);
        }

        return true;
    }

    private MonsterSpawnEntry PickMonsterEntry()
    {
        if (monsterPool == null || monsterPool.Count == 0)
            return null;

        float totalWeight = 0f;
        for (int i = 0; i < monsterPool.Count; i++)
        {
            MonsterSpawnEntry entry = monsterPool[i];
            if (entry == null || !entry.CanSpawn())
                continue;

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < monsterPool.Count; i++)
        {
            MonsterSpawnEntry entry = monsterPool[i];
            if (entry == null || !entry.CanSpawn())
                continue;

            cumulative += entry.weight;
            if (pick <= cumulative)
                return entry;
        }

        return null;
    }

    private Transform PickSpawnPoint()
    {
        return transform;
    }

    private bool IsSpawnPositionFree(Vector3 pos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, spawnCheckRadius, blockedMask);
        if (hits == null || hits.Length == 0)
            return true;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.isTrigger)
                continue;

            return false;
        }

        return true;
    }

    private void CleanupDead()
    {
        for (int i = _alive.Count - 1; i >= 0; i--)
        {
            if (_alive[i] == null)
                _alive.RemoveAt(i);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}