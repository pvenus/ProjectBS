using System.Collections.Generic;
using UnityEngine;

using Character;

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
        [Tooltip("Monster character definition to spawn.")]
        public CharacterSO characterSO;

        [Tooltip("Relative selection weight. Higher = spawned more often.")]
        [Min(0f)] public float weight = 1f;

        [Tooltip("How many of this monster may be spawned in total. -1 = unlimited.")]
        public int maxSpawnCount = -1;


        [HideInInspector] public int spawnedCount;

        public bool CanSpawn()
        {
            if (characterSO == null || characterSO.prefab == null)
                return false;

            if (weight <= 0f)
                return false;

            return maxSpawnCount < 0 || spawnedCount < maxSpawnCount;
        }
    }

    [System.Serializable]
    public class SpawnPointEntry
    {
        [Tooltip("Spawn point transform. If empty, this spawner transform is used.")]
        public Transform point;

        [Tooltip("Relative selection weight. Higher = used more often.")]
        [Min(0f)] public float weight = 1f;

        public bool CanUse()
        {
            return weight > 0f;
        }
    }

    [System.Serializable]
    public class SpawnPhase
    {
        [Header("Time")]
        [Min(0f)] public float startTime;
        [Min(0f)] public float endTime = 60f;

        [Header("Timing")]
        [Min(0.05f)] public float spawnInterval = 5f;
        [Min(1)] public int spawnBurst = 1;

        [Header("Monster Pool")]
        public List<MonsterSpawnEntry> monsterPool = new List<MonsterSpawnEntry>();

        [Header("Spawn Points")]
        public List<SpawnPointEntry> spawnPoints = new List<SpawnPointEntry>();

        [HideInInspector] public float spawnTimer;

        public bool IsActive(float elapsedTime)
        {
            return elapsedTime >= startTime && elapsedTime < endTime;
        }

        public void ResetProgress()
        {
            spawnTimer = spawnInterval;

            if (monsterPool == null)
            {
                return;
            }

            for (int i = 0; i < monsterPool.Count; i++)
            {
                if (monsterPool[i] != null)
                {
                    monsterPool[i].spawnedCount = 0;
                }
            }
        }
    }

    [Header("Stage Spawn")]
    [SerializeField, Min(0f)] private float stageDuration = 180f;
    [SerializeField] private bool autoSpawn = true;
    [SerializeField] private List<SpawnPhase> phases = new List<SpawnPhase>();

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
    private float _elapsedTime;

    public IReadOnlyList<SpawnPhase> Phases => phases;
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
        ResetSpawnProgress();
    }

    private void Update()
    {
        CleanupDead();

        if (!autoSpawn)
        {
            return;
        }

        if (stageDuration > 0f && _elapsedTime >= stageDuration)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;
        UpdateActivePhases();
    }

    public void ResetSpawnProgress()
    {
        _elapsedTime = 0f;

        if (phases == null)
        {
            return;
        }

        for (int i = 0; i < phases.Count; i++)
        {
            if (phases[i] != null)
            {
                phases[i].ResetProgress();
            }
        }
    }

    public bool TrySpawnOne(SpawnPhase phase)
    {
        CleanupDead();

        if (phase == null)
        {
            return false;
        }

        if (respectAliveLimit && _alive.Count >= maxAliveCount)
        {
            return false;
        }

        MonsterSpawnEntry entry = PickMonsterEntry(phase);
        if (entry == null)
        {
            return false;
        }

        Transform point = PickSpawnPoint(phase);
        if (point == null)
        {
            return false;
        }

        Vector3 spawnPos = point.position;
        if (validateByOverlap && !IsSpawnPositionFree(spawnPos))
        {
            if (debugLog)
            {
                Debug.Log($"[NpcSpawnerMono] Spawn blocked at point={point.name} pos={spawnPos}", this);
            }

            return false;
        }

        GameObject spawned = Instantiate(entry.characterSO.prefab, spawnPos, point.rotation);
        if (spawned == null)
        {
            return false;
        }

        _alive.Add(spawned);
        entry.spawnedCount++;

        SetupSpawnedCharacter(
            spawned,
            entry);

        if (debugLog)
        {
            Debug.Log($"[NpcSpawnerMono] Spawned character={entry.characterSO.name} point={point.name} alive={_alive.Count}", this);
        }

        return true;
    }

    private void UpdateActivePhases()
    {
        if (phases == null || phases.Count == 0)
        {
            if (debugLog)
            {
                Debug.LogWarning($"[NpcSpawnerMono] Spawn phases are empty on {name}.", this);
            }

            return;
        }

        for (int i = 0; i < phases.Count; i++)
        {
            SpawnPhase phase = phases[i];
            if (phase == null || !phase.IsActive(_elapsedTime))
            {
                continue;
            }

            phase.spawnTimer -= Time.deltaTime;
            if (phase.spawnTimer > 0f)
            {
                continue;
            }

            phase.spawnTimer = phase.spawnInterval;
            SpawnPhaseBurst(phase);
        }
    }

    private int SpawnPhaseBurst(SpawnPhase phase)
    {
        CleanupDead();

        if (phase == null || phase.monsterPool == null || phase.monsterPool.Count == 0)
        {
            if (debugLog)
            {
                Debug.LogWarning($"[NpcSpawnerMono] Monster pool is empty on phase in {name}.", this);
            }

            return 0;
        }

        int spawned = 0;
        for (int i = 0; i < phase.spawnBurst; i++)
        {
            if (respectAliveLimit && _alive.Count >= maxAliveCount)
            {
                break;
            }

            if (!TrySpawnOne(phase))
            {
                break;
            }

            spawned++;
        }

        return spawned;
    }

    private void SetupSpawnedCharacter(
        GameObject spawned,
        MonsterSpawnEntry entry)
    {
        if (spawned == null || entry == null)
        {
            return;
        }

        CharacterManager characterManager =
            spawned.GetComponent<CharacterManager>();

        if (characterManager == null)
        {
            characterManager =
                spawned.GetComponentInChildren<CharacterManager>();
        }

        if (characterManager == null)
        {
            if (debugLog)
            {
                Debug.LogWarning(
                    $"[NpcSpawnerMono] CharacterManager not found on spawned npc={spawned.name}",
                    spawned);
            }

            return;
        }

        characterManager.InitializeFromSO(entry.characterSO);
    }


    private MonsterSpawnEntry PickMonsterEntry(SpawnPhase phase)
    {
        if (phase == null)
        {
            return null;
        }

        if (phase.monsterPool == null || phase.monsterPool.Count == 0)
            return null;

        float totalWeight = 0f;
        for (int i = 0; i < phase.monsterPool.Count; i++)
        {
            MonsterSpawnEntry entry = phase.monsterPool[i];
            if (entry == null || !entry.CanSpawn())
                continue;

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < phase.monsterPool.Count; i++)
        {
            MonsterSpawnEntry entry = phase.monsterPool[i];
            if (entry == null || !entry.CanSpawn())
                continue;

            cumulative += entry.weight;
            if (pick <= cumulative)
                return entry;
        }

        return null;
    }

    private Transform PickSpawnPoint(SpawnPhase phase)
    {
        if (phase == null || phase.spawnPoints == null || phase.spawnPoints.Count == 0)
        {
            return transform;
        }

        float totalWeight = 0f;
        for (int i = 0; i < phase.spawnPoints.Count; i++)
        {
            SpawnPointEntry entry = phase.spawnPoints[i];
            if (entry == null || !entry.CanUse())
            {
                continue;
            }

            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
        {
            return transform;
        }

        float pick = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < phase.spawnPoints.Count; i++)
        {
            SpawnPointEntry entry = phase.spawnPoints[i];
            if (entry == null || !entry.CanUse())
            {
                continue;
            }

            cumulative += entry.weight;
            if (pick <= cumulative)
            {
                return entry.point != null
                    ? entry.point
                    : transform;
            }
        }

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
