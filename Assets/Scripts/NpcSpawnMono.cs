
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamic NPC spawner.
/// - Spawns NPCs up to maxAlive.
/// - Supports spawn modes:
///   1) WorldRect: random point inside a world-space rectangle
///   2) AroundPlayerRing: random point in an annulus around the player
/// - Avoids obstacles via Physics2D overlap checks.
/// </summary>
public class NpcSpawnMono : MonoBehaviour
{
    public enum SpawnMode
    {
        WorldRect,
        AroundPlayerRing
    }

    [Header("Prefab")]
    [Tooltip("NPC prefab to spawn. If null, spawner will create an empty NPC and add NpcMono.")]
    [SerializeField] private GameObject npcPrefab;

    [Header("Target")]
    [Tooltip("Optional. Used by AroundPlayerRing mode. If null, will auto-find tag 'Player'.")]
    [SerializeField] private Transform player;

    [Header("Spawn Control")]
    [SerializeField] private SpawnMode spawnMode = SpawnMode.AroundPlayerRing;
    [SerializeField] private int maxAlive = 20;
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

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        _timer = spawnInterval;
    }

    private void Update()
    {
        CleanupDead();

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = spawnInterval;

        int need = Mathf.Max(0, maxAlive - _alive.Count);
        if (need <= 0) return;

        int toSpawn = Mathf.Min(need, spawnBurst);
        for (int i = 0; i < toSpawn; i++)
        {
            TrySpawnOne();
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

            // Keep away from player
            if (player != null && minDistanceFromPlayer > 0f)
            {
                if (Vector2.Distance(pos, player.position) < minDistanceFromPlayer)
                    continue;
            }

            // Avoid obstacles / walls / other colliders
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

                // Uniform in area: sample r^2
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

            // If it's the player, ignore.
            if (player != null && (c.transform == player || c.transform.IsChildOf(player)))
                continue;

            return false;
        }
        return true;
    }

    private GameObject SpawnNpcAt(Vector2 pos)
    {
        GameObject go;

        if (npcPrefab != null)
        {
            go = Instantiate(npcPrefab, pos, Quaternion.identity);
        }
        else
        {
            // Fallback: build a simple NPC at runtime.
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
    }
#endif
}
