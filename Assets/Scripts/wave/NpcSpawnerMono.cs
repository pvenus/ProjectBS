using System.Collections.Generic;
using UnityEngine;

using Character;
using Character.Helper;
using Wave.SO;

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
    [Header("Stage Wave")]
    [SerializeField] private StageWaveSO stageWaveSo;
    [SerializeField] private bool autoSpawn = true;

    [Header("Spawn Limits")]
    [SerializeField] private bool respectAliveLimit = true;

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
    private readonly Dictionary<SpawnPhase, float> _phaseTimers = new Dictionary<SpawnPhase, float>();
    private readonly Dictionary<SpawnPhase, List<GameObject>> _phaseAlive = new Dictionary<SpawnPhase, List<GameObject>>();

    public StageWaveSO StageWaveSo => stageWaveSo;
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

        if (stageWaveSo != null && stageWaveSo.Duration > 0f && _elapsedTime >= stageWaveSo.Duration)
        {
            return;
        }

        _elapsedTime += Time.deltaTime;
        UpdateActivePhases();
    }

    public void ResetSpawnProgress()
    {
        _elapsedTime = 0f;
        _phaseTimers.Clear();
        _phaseAlive.Clear();

        if (stageWaveSo == null || stageWaveSo.Phases == null)
        {
            return;
        }

        for (int i = 0; i < stageWaveSo.Phases.Count; i++)
        {
            SpawnPhase phase = stageWaveSo.Phases[i];

            if (phase == null)
            {
                continue;
            }

            _phaseTimers[phase] = 0f;
            _phaseAlive[phase] = new List<GameObject>();
        }
    }

    public bool TrySpawnOne(SpawnPhase phase)
    {
        CleanupDead();

        if (phase == null)
        {
            return false;
        }

        if (respectAliveLimit && phase.MaxAliveCount > 0 && CountAliveInPhase(phase) >= phase.MaxAliveCount)
        {
            return false;
        }

        SpawnMonsterEntry entry = PickMonsterEntry(phase);
        GameObject prefab = ResolveCharacterPrefab(entry);

        if (entry == null || entry.characterSo == null)
        {
            return false;
        }

        Vector3 spawnPos = GetSpawnPosition();
        if (validateByOverlap && !IsSpawnPositionFree(spawnPos))
        {
            if (debugLog)
            {
                Debug.Log($"[NpcSpawnerMono] Spawn blocked pos={spawnPos}", this);
            }

            return false;
        }

        GameObject spawned = CharacterBuilder.CreateOrBuildNpcObject(
            prefab,
            entry.characterSo.name,
            transform,
            spawnPos,
            Quaternion.identity,
            "Enemy",
            null,
            true);

        if (spawned == null)
        {
            return false;
        }

        _alive.Add(spawned);
        RegisterPhaseAlive(phase, spawned);

        SetupSpawnedCharacter(
            spawned,
            entry.characterSo);

        if (debugLog)
        {
            Debug.Log($"[NpcSpawnerMono] Spawned character={entry.characterSo.name} pos={spawnPos} alive={_alive.Count}", this);
        }

        return true;
    }

    private void UpdateActivePhases()
    {
        if (stageWaveSo == null || stageWaveSo.Phases == null || stageWaveSo.Phases.Count == 0)
        {
            if (debugLog)
            {
                Debug.LogWarning($"[NpcSpawnerMono] StageWaveSO or phases are empty on {name}.", this);
            }

            return;
        }

        for (int i = 0; i < stageWaveSo.Phases.Count; i++)
        {
            SpawnPhase phase = stageWaveSo.Phases[i];
            if (phase == null || !phase.IsActive(_elapsedTime))
            {
                continue;
            }

            if (!_phaseTimers.ContainsKey(phase))
            {
                _phaseTimers[phase] = 0f;
            }

            _phaseTimers[phase] -= Time.deltaTime;
            if (_phaseTimers[phase] > 0f)
            {
                continue;
            }

            _phaseTimers[phase] = phase.SpawnInterval;
            SpawnPhaseBurst(phase);
        }
    }

    private int SpawnPhaseBurst(SpawnPhase phase)
    {
        CleanupDead();

        if (phase == null || phase.monsters == null || phase.monsters.Count == 0)
        {
            if (debugLog)
            {
                Debug.LogWarning($"[NpcSpawnerMono] Monster pool is empty on phase in {name}.", this);
            }

            return 0;
        }

        int spawned = 0;
        for (int i = 0; i < phase.SpawnCountPerTick; i++)
        {
            if (respectAliveLimit && phase.MaxAliveCount > 0 && CountAliveInPhase(phase) >= phase.MaxAliveCount)
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
        CharacterSO characterSo)
    {
        if (spawned == null || characterSo == null)
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

        characterManager.InitializeFromSO(characterSo);
    }

    private void RegisterPhaseAlive(SpawnPhase phase, GameObject spawned)
    {
        if (phase == null || spawned == null)
        {
            return;
        }

        if (!_phaseAlive.TryGetValue(phase, out List<GameObject> aliveList) || aliveList == null)
        {
            aliveList = new List<GameObject>();
            _phaseAlive[phase] = aliveList;
        }

        aliveList.Add(spawned);
    }

    private int CountAliveInPhase(SpawnPhase phase)
    {
        if (phase == null)
        {
            return 0;
        }

        if (!_phaseAlive.TryGetValue(phase, out List<GameObject> aliveList) || aliveList == null)
        {
            return 0;
        }

        for (int i = aliveList.Count - 1; i >= 0; i--)
        {
            if (aliveList[i] == null)
            {
                aliveList.RemoveAt(i);
            }
        }

        return aliveList.Count;
    }

    private GameObject ResolveCharacterPrefab(
        SpawnMonsterEntry entry)
    {
        if (entry == null || entry.characterSo == null)
        {
            return null;
        }

        return entry.characterSo.ResolvePrefab();
    }

    private SpawnMonsterEntry PickMonsterEntry(SpawnPhase phase)
    {
        if (phase == null || phase.monsters == null || phase.monsters.Count == 0)
        {
            return null;
        }

        int totalWeight = 0;
        for (int i = 0; i < phase.monsters.Count; i++)
        {
            SpawnMonsterEntry entry = phase.monsters[i];
            if (entry == null || entry.characterSo == null)
            {
                continue;
            }

            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int pick = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < phase.monsters.Count; i++)
        {
            SpawnMonsterEntry entry = phase.monsters[i];
            if (entry == null || entry.characterSo == null)
            {
                continue;
            }

            cumulative += entry.Weight;
            if (pick < cumulative)
            {
                return entry;
            }
        }

        return null;
    }

    private Vector3 GetSpawnPosition()
    {
        if (stageWaveSo == null)
        {
            return transform.position;
        }

        return stageWaveSo.GetRandomSpawnPosition(_elapsedTime)
            + stageWaveSo.GetRandomGroupOffset();
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

        foreach (List<GameObject> aliveList in _phaseAlive.Values)
        {
            if (aliveList == null)
            {
                continue;
            }

            for (int i = aliveList.Count - 1; i >= 0; i--)
            {
                if (aliveList[i] == null)
                {
                    aliveList.RemoveAt(i);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        if (stageWaveSo == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Vector3 startTop = new Vector3(stageWaveSo.SpawnStartX, stageWaveSo.YMax, 0f);
        Vector3 startBottom = new Vector3(stageWaveSo.SpawnStartX, stageWaveSo.YMin, 0f);
        Gizmos.DrawLine(startTop, startBottom);
        Gizmos.DrawSphere(startTop, gizmoRadius);
        Gizmos.DrawSphere(startBottom, gizmoRadius);

        Gizmos.color = Color.magenta;
        Vector3 endTop = new Vector3(stageWaveSo.SpawnEndX, stageWaveSo.YMax, 0f);
        Vector3 endBottom = new Vector3(stageWaveSo.SpawnEndX, stageWaveSo.YMin, 0f);
        Gizmos.DrawLine(endTop, endBottom);
        Gizmos.DrawSphere(endTop, gizmoRadius);
        Gizmos.DrawSphere(endBottom, gizmoRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startTop, endTop);
        Gizmos.DrawLine(startBottom, endBottom);
    }
}
