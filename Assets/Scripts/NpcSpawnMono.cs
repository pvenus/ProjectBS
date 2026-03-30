using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamic NPC spawner with wave pacing.
/// - Keeps alive count under control.
/// - Alternates between Calm / BuildUp / Peak / Recovery phases.
/// - Creates visible pressure rise and fall instead of constant flat spawning.
/// - Supports spawn modes:
///   1) WorldRect: random point inside a world-space rectangle
///   2) AroundPlayerRing: random point in an annulus around the player
/// - Avoids obstacles via Physics2D overlap checks.
/// </summary>
[System.Serializable]
public class SpawnPattern
{
    public enum ShapeType
    {
        Random,
        Circle,
        Line
    }

    public string name;
    public int burst;
    public float interval;
    public int maxAlive;
    public ShapeType shapeType = ShapeType.Random;
    public SpawnCirclePattern circlePattern;
    public SpawnLinePattern linePattern;
}

[System.Serializable]
public class WaveConfig
{
    public string name;
    public float duration;
    public List<SpawnPattern> patterns;
}

public class NpcSpawnMono : MonoBehaviour
{
    public enum SpawnMode
    {
        WorldRect,
        AroundPlayerRing
    }

    [Header("Prefab")]
    [Tooltip("Legacy single NPC prefab. Used only if resource-based spawning is disabled or resource prefabs are missing.")]
    [SerializeField] private GameObject npcPrefab;
    [Tooltip("If enabled, the spawner will load NpcSiege / NpcFlying / NpcNormal from Resources and pick one automatically.")]
    [SerializeField] private bool useResourceNpcPrefabs = true;
    [SerializeField] private string npcNormalResourcePath = "NpcNormal";
    [SerializeField] private string npcSiegeResourcePath = "NpcSiege";
    [SerializeField] private string npcFlyingResourcePath = "NpcFlying";
    [Range(0f, 1f)]
    [SerializeField] private float normalSpawnWeight = 0.6f;
    [Range(0f, 1f)]
    [SerializeField] private float siegeSpawnWeight = 0.25f;
    [Range(0f, 1f)]
    [SerializeField] private float flyingSpawnWeight = 0.15f;

    [Header("Target")]
    [Tooltip("Optional. Used by AroundPlayerRing mode. If null, will auto-find tag 'Player'.")]
    [SerializeField] private Transform player;

    [Header("Spawn Control")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.AroundPlayerRing;
    [SerializeField] private int maxAlive = 20;

    [Header("Wave System")]
    [SerializeField] private List<WaveConfig> waves;
    [SerializeField] private float waveInterval = 5f;
    [SerializeField] private bool loopWaves = true;

    private int currentWaveIndex;
    private float waveTimer;
    private SpawnPattern currentPattern;

    [Header("Legacy Flat Spawn (used when wave pacing is disabled)")]
    [SerializeField] private int spawnBurst = 3;
    [SerializeField] private float spawnInterval = 1.0f;

    [Header("Spawn Area: WorldRect")]
    [Tooltip("World-space center of the spawn rectangle.")]
    [SerializeField] private Vector2 rectCenter = Vector2.zero;
    [Tooltip("World-space size of the spawn rectangle.")]
    [SerializeField] private Vector2 rectSize = new Vector2(30f, 18f);

    [Header("Spawn Area: AroundPlayerRing")]
    [SerializeField] private float ringInnerRadius = 6f;
    [SerializeField] private float ringOuterRadius = 10f;

    [Header("Spawn Validation")]
    [Tooltip("Minimum distance from player when spawning.")]
    [SerializeField] private float minDistanceFromPlayer = 4f;
    [Tooltip("How many random attempts to find a valid spawn location per spawn.")]
    [SerializeField] private int maxTriesPerNpc = 20;
    [Tooltip("Overlap check radius (approx NPC size).")]
    [SerializeField] private float spawnCheckRadius = 0.5f;
    [Tooltip("Colliders in these layers are considered blocked for spawning.")]
    [SerializeField] private LayerMask blockedMask = ~0;

    private readonly List<GameObject> _alive = new List<GameObject>();
    private float _timer;
    private GameObject _npcNormalPrefab;
    private GameObject _npcSiegePrefab;
    private GameObject _npcFlyingPrefab;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        LoadNpcPrefabsFromResources();

        // Auto-generate dummy wave for testing if none configured
        if (waves == null || waves.Count == 0)
        {
            waves = new List<WaveConfig>
            {
                new WaveConfig
                {
                    name = "Dummy Wave",
                    duration = 10f,
                    patterns = new List<SpawnPattern>
                    {
                        new SpawnPattern
                        {
                            name = "Dummy Circle Pattern",
                            burst = 6,
                            interval = 10f,
                            maxAlive = 30,
                            shapeType = SpawnPattern.ShapeType.Circle,
                            circlePattern = new SpawnCirclePattern()
                        },
                        new SpawnPattern
                        {
                            name = "Top Horizontal Line",
                            burst = 5,
                            interval = 10f,
                            maxAlive = 30,
                            shapeType = SpawnPattern.ShapeType.Line,
                            linePattern = new SpawnLinePattern()
                                .SetCenterOffset(new Vector2(0f, 30f))
                                .SetSpawnCount(10)
                                .SetSpacing(3f)
                                .SetDirectionMode(SpawnLinePattern.LineDirectionMode.AutoPerpendicularToOffset)
                        },
                        new SpawnPattern
                        {
                            name = "Bottom Horizontal Line",
                            burst = 5,
                            interval = 10f,
                            maxAlive = 30,
                            shapeType = SpawnPattern.ShapeType.Line,
                            linePattern = new SpawnLinePattern()
                                .SetCenterOffset(new Vector2(0f, -30f))
                                .SetSpawnCount(10)
                                .SetSpacing(3f)
                                .SetDirectionMode(SpawnLinePattern.LineDirectionMode.AutoPerpendicularToOffset)
                        },
                        new SpawnPattern
                        {
                            name = "Left Vertical Line",
                            burst = 5,
                            interval = 10f,
                            maxAlive = 30,
                            shapeType = SpawnPattern.ShapeType.Line,
                            linePattern = new SpawnLinePattern()
                                .SetCenterOffset(new Vector2(-30f, 0f))
                                .SetSpawnCount(10)
                                .SetSpacing(3f)
                                .SetDirectionMode(SpawnLinePattern.LineDirectionMode.AutoPerpendicularToOffset)
                        },
                        new SpawnPattern
                        {
                            name = "Right Vertical Line",
                            burst = 5,
                            interval = 10f,
                            maxAlive = 30,
                            shapeType = SpawnPattern.ShapeType.Line,
                            linePattern = new SpawnLinePattern()
                                .SetCenterOffset(new Vector2(30f, 0f))
                                .SetSpawnCount(10)
                                .SetSpacing(3f)
                                .SetDirectionMode(SpawnLinePattern.LineDirectionMode.AutoPerpendicularToOffset)
                        }
                    }
                }
            };
        }

        SelectNextWave();
    }

    private void Update()
    {
        CleanupDead();

        waveTimer -= Time.deltaTime;
        if (waveTimer <= 0f)
        {
            SelectNextWave();
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = currentPattern != null ? currentPattern.interval : spawnInterval;

        int toSpawn;

        if (UsesFixedPatternShape())
        {
            toSpawn = GetSpawnCountForCurrentPattern(int.MaxValue);
        }
        else
        {
            int max = currentPattern != null ? currentPattern.maxAlive : maxAlive;
            int need = Mathf.Max(0, max - _alive.Count);
            if (need <= 0) return;

            toSpawn = GetSpawnCountForCurrentPattern(need);
        }

        if (!TrySpawnPattern(toSpawn))
        {
            for (int i = 0; i < toSpawn; i++)
            {
                TrySpawnOne();
            }
        }
    }

    private bool UsesFixedPatternShape()
    {
        if (currentPattern == null)
            return false;

        return currentPattern.shapeType == SpawnPattern.ShapeType.Circle
            || currentPattern.shapeType == SpawnPattern.ShapeType.Line;
    }

    private int GetSpawnCountForCurrentPattern(int need)
    {
        if (currentPattern == null)
            return Mathf.Min(need, spawnBurst);

        switch (currentPattern.shapeType)
        {
            case SpawnPattern.ShapeType.Circle:
                if (currentPattern.circlePattern != null)
                    return Mathf.Min(need, currentPattern.circlePattern.GetTotalSpawnCount());
                break;

            case SpawnPattern.ShapeType.Line:
                if (currentPattern.linePattern != null)
                    return Mathf.Min(need, currentPattern.linePattern.GetTotalSpawnCount());
                break;
        }

        return Mathf.Min(need, currentPattern.burst);
    }

    private bool TrySpawnPattern(int count)
    {
        if (currentPattern == null)
            return false;

        switch (currentPattern.shapeType)
        {
            case SpawnPattern.ShapeType.Circle:
                return TrySpawnCirclePattern(count);

            case SpawnPattern.ShapeType.Line:
                return TrySpawnLinePattern(count);

            case SpawnPattern.ShapeType.Random:
            default:
                return false;
        }
    }

    private bool TrySpawnCirclePattern(int count)
    {
        if (currentPattern == null || currentPattern.circlePattern == null)
            return false;

        Vector2 origin = GetPatternOrigin();
        List<Vector2> positions = currentPattern.circlePattern.BuildPositions(origin);
        return TrySpawnFromPatternPositions(positions, count);
    }

    private bool TrySpawnLinePattern(int count)
    {
        if (currentPattern == null || currentPattern.linePattern == null)
            return false;

        Vector2 origin = GetPatternOrigin();
        List<Vector2> positions = currentPattern.linePattern.BuildPositions(origin);
        return TrySpawnFromPatternPositions(positions, count);
    }

    private bool TrySpawnFromPatternPositions(List<Vector2> positions, int count)
    {
        if (positions == null || positions.Count == 0)
            return false;

        int spawned = 0;
        int limit = count == int.MaxValue ? positions.Count : Mathf.Min(count, positions.Count);
        Debug.Log($"[NpcSpawnMono] Pattern spawn attempt type={(currentPattern != null ? currentPattern.shapeType.ToString() : "None")} requested={count} positions={positions.Count} using={limit}");

        for (int i = 0; i < limit; i++)
        {
            Vector2 pos = positions[i];

            if (player != null && minDistanceFromPlayer > 0f)
            {
                if (Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                    continue;
            }

            if (!IsSpawnPositionFree(pos))
                continue;

            var go = SpawnNpcAt(pos);
            if (go == null)
                continue;

            _alive.Add(go);
            spawned++;
        }

        return spawned > 0;
    }

    private Vector2 GetPatternOrigin()
    {
        switch (spawnMode)
        {
            case SpawnMode.WorldRect:
                return rectCenter;

            case SpawnMode.AroundPlayerRing:
            default:
                return player != null ? (Vector2)player.position : rectCenter;
        }
    }

    private void LoadNpcPrefabsFromResources()
    {
        if (!useResourceNpcPrefabs)
            return;

        if (!string.IsNullOrWhiteSpace(npcNormalResourcePath))
            _npcNormalPrefab = Resources.Load<GameObject>(npcNormalResourcePath);

        if (!string.IsNullOrWhiteSpace(npcSiegeResourcePath))
            _npcSiegePrefab = Resources.Load<GameObject>(npcSiegeResourcePath);

        if (!string.IsNullOrWhiteSpace(npcFlyingResourcePath))
            _npcFlyingPrefab = Resources.Load<GameObject>(npcFlyingResourcePath);
    }

    private GameObject SelectNpcPrefabForSpawn()
    {
        if (!useResourceNpcPrefabs)
            return npcPrefab;

        float normalWeight = Mathf.Max(0f, normalSpawnWeight);
        float siegeWeight = Mathf.Max(0f, siegeSpawnWeight);
        float flyingWeight = Mathf.Max(0f, flyingSpawnWeight);

        bool hasNormal = _npcNormalPrefab != null;
        bool hasSiege = _npcSiegePrefab != null;
        bool hasFlying = _npcFlyingPrefab != null;

        float totalWeight = 0f;
        if (hasNormal) totalWeight += normalWeight;
        if (hasSiege) totalWeight += siegeWeight;
        if (hasFlying) totalWeight += flyingWeight;

        if (totalWeight <= 0f)
        {
            if (hasNormal) return _npcNormalPrefab;
            if (hasSiege) return _npcSiegePrefab;
            if (hasFlying) return _npcFlyingPrefab;
            return npcPrefab;
        }

        float roll = Random.value * totalWeight;

        if (hasNormal)
        {
            if (roll <= normalWeight)
                return _npcNormalPrefab;
            roll -= normalWeight;
        }

        if (hasSiege)
        {
            if (roll <= siegeWeight)
                return _npcSiegePrefab;
            roll -= siegeWeight;
        }

        if (hasFlying)
            return _npcFlyingPrefab;

        return npcPrefab;
    }

    private void SelectNextWave()
    {
        if (waves == null || waves.Count == 0)
            return;

        if (currentWaveIndex >= waves.Count)
        {
            if (loopWaves)
                currentWaveIndex = 0;
            else
                return;
        }

        var wave = waves[currentWaveIndex];
        currentWaveIndex++;

        waveTimer = wave.duration + waveInterval;

        if (wave.patterns != null && wave.patterns.Count > 0)
        {
            currentPattern = wave.patterns[Random.Range(0, wave.patterns.Count)];
        }
    }

    private void CleanupDead()
    {
        for (int i = _alive.Count - 1; i >= 0; i--)
        {
            if (_alive[i] == null)
                _alive.RemoveAt(i);
        }
    }

    private void TrySpawnOne()
    {
        for (int t = 0; t < maxTriesPerNpc; t++)
        {
            Vector2 pos = SampleSpawnPosition();

            if (player != null && minDistanceFromPlayer > 0f)
            {
                if (Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                    continue;
            }

            if (!IsSpawnPositionFree(pos))
                continue;

            var go = SpawnNpcAt(pos);
            if (go != null)
            {
                _alive.Add(go);
                return;
            }
        }
    }

    private Vector2 SampleSpawnPosition()
    {
        switch (spawnMode)
        {
            case SpawnMode.WorldRect:
            {
                float x = Random.Range(rectCenter.x - rectSize.x * 0.5f, rectCenter.x + rectSize.x * 0.5f);
                float y = Random.Range(rectCenter.y - rectSize.y * 0.5f, rectCenter.y + rectSize.y * 0.5f);
                return new Vector2(x, y);
            }

            case SpawnMode.AroundPlayerRing:
            default:
            {
                Vector2 center = player != null ? (Vector2)player.position : rectCenter;
                float angle = Random.Range(0f, Mathf.PI * 2f);

                float r0 = Mathf.Max(0.01f, ringInnerRadius);
                float r1 = Mathf.Max(r0 + 0.01f, ringOuterRadius);
                float rr = Random.Range(r0 * r0, r1 * r1);
                float r = Mathf.Sqrt(rr);

                return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            }
        }
    }

    private bool IsSpawnPositionFree(Vector2 pos)
    {
        var hits = Physics2D.OverlapCircleAll(pos, spawnCheckRadius, blockedMask);
        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            if (player != null && (c.transform == player || c.transform.IsChildOf(player)))
                continue;

            return false;
        }
        return true;
    }

    private GameObject SpawnNpcAt(Vector2 pos)
    {
        GameObject selectedPrefab = SelectNpcPrefabForSpawn();
        GameObject go;

        if (selectedPrefab != null)
        {
            go = Instantiate(selectedPrefab, pos, Quaternion.identity);
        }
        else if (npcPrefab != null)
        {
            go = Instantiate(npcPrefab, pos, Quaternion.identity);
        }
        else
        {
            go = new GameObject("NPC");
            go.transform.position = pos;
            go.AddComponent<NpcMono>();
        }

        return go;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.25f);

        if (spawnMode == SpawnMode.WorldRect)
        {
            Gizmos.DrawWireCube(rectCenter, rectSize);
        }
        else
        {
            Vector3 center = player != null ? player.position : (Vector3)rectCenter;
            Gizmos.DrawWireSphere(center, ringInnerRadius);
            Gizmos.DrawWireSphere(center, ringOuterRadius);
        }
        
        if (currentPattern != null)
        {
            Vector2 origin = Application.isPlaying
                ? GetPatternOrigin()
                : (player != null ? (Vector2)player.position : rectCenter);

            if (currentPattern.shapeType == SpawnPattern.ShapeType.Circle && currentPattern.circlePattern != null)
            {
                currentPattern.circlePattern.DrawGizmos(origin, Color.cyan, Color.blue);
            }
            else if (currentPattern.shapeType == SpawnPattern.ShapeType.Line && currentPattern.linePattern != null)
            {
                currentPattern.linePattern.DrawGizmos(origin, Color.yellow, Color.red);
            }
        }
    }
#endif
}
