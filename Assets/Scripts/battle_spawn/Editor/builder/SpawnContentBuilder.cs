#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Character;

[Serializable]
public class JsonSpawnSquadGroup
{
    public int order;
    public string npcId;
    public string patternId;
    public Vector2 localOffset;
    public float localRotation;
    public float slotInterval;
    public int quantity; // RandomPattern 수량 매칭용
}

[Serializable]
public class JsonSpawnSquad
{
    public string contentId;
    public float groupInterval;
    public List<JsonSpawnSquadGroup> groups;

    // 레거시 호환용 필드
    public string npcId;
    public string patternId;
    public float spawnDelay;
}

[Serializable]
public class JsonSpawnFormation
{
    public string contentId;
    public string patternId;
    public string squadId;
    public float slotInterval;
    public int quantity; // RandomPattern 수량 매칭용

    // 레거시 호환용 필드
    public float spawnDelay;
}

public static class SpawnContentBuilder
{
    public static SpawnSquadSO BuildSquad(
        JsonSpawnSquad data, 
        string baseDir, 
        Dictionary<string, CharacterSO> npcPool, 
        Dictionary<string, SpawnPattern> patternPool)
    {
        if (data == null || string.IsNullOrEmpty(data.contentId)) return null;

        bool hasPattern = false;
        if (data.groups != null && data.groups.Count > 0)
        {
            foreach (var g in data.groups)
            {
                if (!string.IsNullOrEmpty(g.patternId))
                {
                    hasPattern = true;
                    break;
                }
            }
        }
        else if (!string.IsNullOrEmpty(data.patternId))
        {
            hasPattern = true;
        }

        string subFolder = hasPattern ? "Squads" : "Singles";
        string assetPath = $"{baseDir}/{subFolder}/{data.contentId}.asset";
        SpawnSquadSO asset = AssetDatabase.LoadAssetAtPath<SpawnSquadSO>(assetPath);

        if (asset == null)
        {
            string dirPath = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            asset = ScriptableObject.CreateInstance<SpawnSquadSO>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        List<SpawnSquadGroup> groupList = new List<SpawnSquadGroup>();

        if (data.groups != null && data.groups.Count > 0)
        {
            foreach (var g in data.groups)
            {
                CharacterSO character = null;
                if (!string.IsNullOrEmpty(g.npcId))
                {
                    npcPool.TryGetValue(g.npcId, out character);
                }
                if (character == null)
                {
                    Debug.LogWarning($"[SpawnContentBuilder] NpcId '{g.npcId}'를 NPC 풀에서 찾을 수 없습니다.");
                    continue;
                }

                SpawnPattern patternSO = null;
                if (!string.IsNullOrEmpty(g.patternId))
                {
                    patternPool.TryGetValue(g.patternId, out patternSO);
                    if (patternSO == null)
                    {
                        Debug.LogWarning($"[SpawnContentBuilder] PatternId '{g.patternId}'를 패턴 풀에서 찾을 수 없습니다.");
                    }
                }

                int qty = g.quantity <= 0 ? 1 : g.quantity;

                groupList.Add(new SpawnSquadGroup(
                    g.order,
                    character,
                    patternSO,
                    g.localOffset,
                    g.localRotation,
                    g.slotInterval,
                    qty
                ));
            }
        }
        else
        {
            // 레거시 단일 필드 호환용 처리
            CharacterSO character = null;
            if (!string.IsNullOrEmpty(data.npcId))
            {
                npcPool.TryGetValue(data.npcId, out character);
            }

            if (character != null)
            {
                SpawnPattern patternSO = null;
                if (!string.IsNullOrEmpty(data.patternId))
                {
                    patternPool.TryGetValue(data.patternId, out patternSO);
                }

                groupList.Add(new SpawnSquadGroup(
                    0,
                    character,
                    patternSO,
                    Vector2.zero,
                    0f,
                    0f,
                    1
                ));
            }
        }

        asset.Initialize(data.contentId, data.groupInterval, groupList);
        EditorUtility.SetDirty(asset);
        return asset;
    }

    public static SpawnFormationSO BuildFormation(
        JsonSpawnFormation data, 
        string baseDir, 
        Dictionary<string, SpawnSquadSO> squadPool, 
        Dictionary<string, SpawnPattern> patternPool)
    {
        if (data == null || string.IsNullOrEmpty(data.contentId)) return null;

        string assetPath = $"{baseDir}/Formations/{data.contentId}.asset";
        SpawnFormationSO asset = AssetDatabase.LoadAssetAtPath<SpawnFormationSO>(assetPath);

        if (asset == null)
        {
            string dirPath = Path.GetDirectoryName(assetPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            asset = ScriptableObject.CreateInstance<SpawnFormationSO>();
            AssetDatabase.CreateAsset(asset, assetPath);
        }

        // 1. 하위 스쿼드 조회
        SpawnSquadSO squadSO = null;
        if (!string.IsNullOrEmpty(data.squadId))
        {
            squadPool.TryGetValue(data.squadId, out squadSO);
        }
        if (squadSO == null)
        {
            Debug.LogError($"[SpawnContentBuilder] SquadId '{data.squadId}'에 해당하는 SpawnSquadSO를 찾을 수 없습니다.");
            return null;
        }

        // 2. 포메이션 패턴 조회
        SpawnPattern patternSO = null;
        if (!string.IsNullOrEmpty(data.patternId))
        {
            patternPool.TryGetValue(data.patternId, out patternSO);
            if (patternSO == null)
            {
                Debug.LogWarning($"[SpawnContentBuilder] PatternId '{data.patternId}'에 해당하는 SpawnPattern을 찾을 수 없습니다.");
            }
        }

        float finalInterval = data.slotInterval;
        if (finalInterval <= 0f && data.spawnDelay > 0f)
        {
            finalInterval = data.spawnDelay;
        }

        int qty = data.quantity <= 0 ? 1 : data.quantity;

        asset.Initialize(data.contentId, patternSO, squadSO, finalInterval, qty);
        EditorUtility.SetDirty(asset);
        return asset;
    }
}
#endif
