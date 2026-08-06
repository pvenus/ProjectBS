using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

public sealed class EnemyRegistry
{
    private static EnemyRegistry instance;
    public static EnemyRegistry Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EnemyRegistry();
            }
            return instance;
        }
    }

    private readonly List<GameObject> activeEnemies = new List<GameObject>();
    private bool isListening;

    public event Action<GameObject> OnEnemyRegistered;
    public event Action<GameObject> OnEnemyDied;

    public IReadOnlyList<GameObject> ActiveEnemies
    {
        get
        {
            CleanupNulls();
            return activeEnemies;
        }
    }

    public void StartListening()
    {
        if (isListening) return;

        CharacterManager.OnAnyCharacterDied += HandleCharacterDied;
        isListening = true;
    }

    public void StopListening()
    {
        if (!isListening) return;

        CharacterManager.OnAnyCharacterDied -= HandleCharacterDied;
        isListening = false;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            StartListening(); // 안전하게 리스닝 시작 보장
            OnEnemyRegistered?.Invoke(enemy);
        }
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        if (activeEnemies.Remove(enemy))
        {
            OnEnemyDied?.Invoke(enemy);
        }
    }

    public void Clear()
    {
        activeEnemies.Clear();
    }

    private void HandleCharacterDied(CharacterManager character)
    {
        if (character == null) return;

        GameObject go = character.gameObject;
        if (activeEnemies.Contains(go))
        {
            UnregisterEnemy(go);
        }
    }

    private void CleanupNulls()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }
}
