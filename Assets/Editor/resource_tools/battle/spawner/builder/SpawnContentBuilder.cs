#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[Serializable]
public class JsonSpawnSquadGroup
{
    public int order;
    public string spawnUnitKey;
    public string spawnRole;
    public JsonSpawnPattern pattern;
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
    public JsonSpawnPattern squadPattern;
    public JsonSpawnPattern formationPattern;
    public string formationPatternId;
    public float squadPatternSlotInterval;
    public int squadPatternQuantity;
    public float formationSlotInterval;
    public int formationQuantity;
    public float groupInterval;
    public float slotInterval;
    public int quantity;
    public List<JsonSpawnSquadGroup> groups;

    public string spawnUnitKey;
    public string spawnRole;
    public JsonSpawnPattern pattern;
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
        Dictionary<string, SpawnPatternData> patternPool)
    {
        if (data == null || string.IsNullOrEmpty(data.contentId)) return null;

        bool hasPattern = false;
        if (data.groups != null && data.groups.Count > 0)
        {
            foreach (var g in data.groups)
            {
                if (HasInlinePattern(g.pattern) || !string.IsNullOrEmpty(g.patternId))
                {
                    hasPattern = true;
                    break;
                }
            }
        }
        else if (HasInlinePattern(data.pattern) || !string.IsNullOrEmpty(data.patternId))
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
                SpawnPatternData patternData = ResolvePattern(g.pattern, g.patternId, patternPool);

                SpawnUnitRole role = ParseSpawnRole(g.spawnRole);
                string unitKey = NormalizeUnitKey(g.spawnUnitKey);
                if (!ValidateUnitRequest(data.contentId, g.order, unitKey, role))
                {
                    return null;
                }

                groupList.Add(new SpawnSquadGroup(
                    g.order,
                    unitKey,
                    role,
                    patternData,
                    g.localOffset,
                    g.localRotation,
                    g.slotInterval,
                    g.quantity
                ));
            }
        }
        else
        {
            // 레거시 단일 필드 호환용 처리
            SpawnPatternData patternData = ResolvePattern(data.pattern, data.patternId, patternPool);

            string unitKey = NormalizeUnitKey(data.spawnUnitKey);
            SpawnUnitRole role = ParseSpawnRole(data.spawnRole);
            if (!ValidateUnitRequest(data.contentId, 0, unitKey, role))
            {
                return null;
            }

            groupList.Add(new SpawnSquadGroup(
                0,
                unitKey,
                role,
                patternData,
                Vector2.zero,
                0f,
                0f,
                1
            ));
        }

        int squadQuantity = data.quantity <= 0 ? 1 : data.quantity;
        SpawnPatternData squadPatternData = ResolvePattern(
            data.squadPattern ?? data.formationPattern,
            data.formationPatternId,
            patternPool);

        float squadPatternSlotInterval = data.squadPatternSlotInterval > 0f
            ? data.squadPatternSlotInterval
            : data.formationSlotInterval;
        int squadPatternQuantity = data.squadPatternQuantity > 0
            ? data.squadPatternQuantity
            : data.formationQuantity;
        if (squadPatternQuantity <= 0)
        {
            squadPatternQuantity = 1;
        }

        asset.Initialize(
            data.contentId,
            squadPatternData,
            squadPatternSlotInterval,
            squadPatternQuantity,
            data.groupInterval,
            data.slotInterval,
            squadQuantity,
            groupList);
        EditorUtility.SetDirty(asset);

        AssetDatabase.SaveAssetIfDirty(asset);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        SpawnSquadSO savedAsset = AssetDatabase.LoadAssetAtPath<SpawnSquadSO>(assetPath);
        if (savedAsset == null)
        {
            Debug.LogError($"[SpawnContentBuilder] SpawnSquadSO 저장 후 재로드에 실패했습니다. path={assetPath}");
            return asset;
        }

        if (savedAsset.Groups == null || savedAsset.Groups.Count == 0)
        {
            Debug.LogError($"[SpawnContentBuilder] SpawnSquadSO 저장 후 groups가 비어 있습니다. contentId={data.contentId}, sourceGroupCount={groupList.Count}, path={assetPath}");
        }

        return savedAsset;
    }

    private static string NormalizeUnitKey(string explicitKey)
    {
        return string.IsNullOrWhiteSpace(explicitKey) ? string.Empty : explicitKey.Trim();
    }

    private static bool HasInlinePattern(JsonSpawnPattern pattern)
    {
        if (pattern == null)
        {
            return false;
        }

        return !string.IsNullOrEmpty(pattern.patternKind) ||
            !string.IsNullOrEmpty(pattern.patternType) ||
            pattern.positions != null ||
            pattern.areaSize != Vector2.zero;
    }

    private static SpawnPatternData ResolvePattern(
        JsonSpawnPattern inlinePattern,
        string legacyPatternId,
        Dictionary<string, SpawnPatternData> patternPool)
    {
        if (HasInlinePattern(inlinePattern))
        {
            return SpawnPatternBuilder.Build(inlinePattern);
        }

        if (!string.IsNullOrEmpty(legacyPatternId) && patternPool != null)
        {
            patternPool.TryGetValue(legacyPatternId, out SpawnPatternData patternData);
            if (patternData == null)
            {
                Debug.LogWarning($"[SpawnContentBuilder] PatternId '{legacyPatternId}'를 패턴 데이터 풀에서 찾을 수 없습니다.");
            }
            return patternData;
        }

        return null;
    }

    private static bool ValidateUnitRequest(string contentId, int order, string unitKey, SpawnUnitRole role)
    {
        if (!string.IsNullOrEmpty(unitKey) || role != SpawnUnitRole.Any)
        {
            if (!string.IsNullOrEmpty(unitKey) &&
                unitKey.StartsWith("character.", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[SpawnContentBuilder] '{contentId}' group order {order}의 spawnUnitKey '{unitKey}'는 Character ID처럼 보입니다. spawnUnitKey는 'unit.melee.1' 같은 스폰 슬롯/역할 키여야 하며, 실제 CharacterSO는 SpawnUnitBinding에서 연결해야 합니다.");
                return false;
            }

            return true;
        }

        Debug.LogError($"[SpawnContentBuilder] '{contentId}' group order {order}에 spawnUnitKey 또는 spawnRole이 필요합니다. JSON만으로는 몬스터가 연결되지 않으며 SpawnUnitBinding을 통해 매핑해야 합니다.");
        return false;
    }

    private static SpawnUnitRole ParseSpawnRole(string value)
    {
        if (!string.IsNullOrEmpty(value) && Enum.TryParse(value, true, out SpawnUnitRole role))
        {
            return role;
        }

        return SpawnUnitRole.Any;
    }

    public static SpawnSquadSO BuildFormation(
        JsonSpawnFormation data, 
        string baseDir, 
        Dictionary<string, SpawnSquadSO> squadPool, 
        Dictionary<string, SpawnPatternData> patternPool)
    {
        if (data == null || string.IsNullOrEmpty(data.contentId)) return null;

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

        string assetPath = $"{baseDir}/Squads/{data.contentId}.asset";
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

        // 2. 포메이션 패턴 조회
        SpawnPatternData patternData = null;
        if (!string.IsNullOrEmpty(data.patternId))
        {
            patternPool.TryGetValue(data.patternId, out patternData);
            if (patternData == null)
            {
                Debug.LogWarning($"[SpawnContentBuilder] PatternId '{data.patternId}'에 해당하는 패턴 데이터를 찾을 수 없습니다.");
            }
        }

        float finalInterval = data.slotInterval;
        if (finalInterval <= 0f && data.spawnDelay > 0f)
        {
            finalInterval = data.spawnDelay;
        }

        int qty = data.quantity <= 0 ? 1 : data.quantity;

        List<SpawnSquadGroup> copiedGroups = new List<SpawnSquadGroup>();
        if (squadSO.Groups != null)
        {
            foreach (SpawnSquadGroup group in squadSO.Groups)
            {
                if (group != null)
                {
                    copiedGroups.Add(group.Clone());
                }
            }
        }

        asset.Initialize(
            data.contentId,
            patternData,
            finalInterval,
            qty,
            squadSO.GroupInterval,
            squadSO.SlotInterval,
            squadSO.Quantity,
            copiedGroups);
        EditorUtility.SetDirty(asset);
        return asset;
    }
}
#endif
