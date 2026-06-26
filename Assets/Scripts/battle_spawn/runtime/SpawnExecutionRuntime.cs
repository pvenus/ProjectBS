using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SpawnExecutionRuntime
{
    private readonly List<GameObject> spawnedNpcs = new List<GameObject>();
    private bool isSpawnCompleted = false;
    private bool isCancelled = false;

    public IReadOnlyList<GameObject> SpawnedNpcs => spawnedNpcs;

    public int AliveNpcCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < spawnedNpcs.Count; i++)
            {
                if (spawnedNpcs[i] != null) count++;
            }
            return count;
        }
    }

    public bool IsSpawnCompleted => isSpawnCompleted;
    public bool AreAllEnemiesDefeated => isSpawnCompleted && (AliveNpcCount == 0);
    public bool IsCancelled => isCancelled;

    public void AddNpc(GameObject npc)
    {
        if (npc != null && !spawnedNpcs.Contains(npc))
        {
            spawnedNpcs.Add(npc);
        }
    }

    public void SetSpawnCompleted()
    {
        isSpawnCompleted = true;
    }

    public void Cancel()
    {
        isCancelled = true;
    }
}
