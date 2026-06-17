using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    public static class StageDefinitionBuilder
    {
        [Serializable]
        private class StageDefinitionJson
        {
            public string stageId;
            public string stageName;
            public bool useFixedSeed;
            public int seed;
            public List<RequiredNodeJson> requiredSubEvents;
            public List<SegmentRuleJson> segmentRules;
        }

        [Serializable]
        private class RequiredNodeJson
        {
            public string nodeId;
            public string jsonPath;
            public int fixedDepth;
            public int fixedColumn;
            public bool hiddenByDefault;
        }

        [Serializable]
        private class SegmentRuleJson
        {
            public int fromDepth;
            public int fromColumn;
            public int toDepth;
            public int toColumn;
            public int minTotalWeight = 90;
            public int maxTotalWeight = 110;
            public int minLayerCount = 2;
            public int maxLayerCount = 4;
            public int minLayerWeight = 15;
            public int maxLayerWeight = 60;
            public int minNodesPerLayer = 1;
            public int maxNodesPerLayer = 3;
            public List<PoolRefJson> pools;
        }

        [Serializable]
        private class PoolRefJson
        {
            public string poolId;
            public bool required;
            public int maxAppearCount = 1;
            public int priority;
        }

        [Serializable]
        private class ActJsonHeader
        {
            public string stageNodeId;
        }

        public static StageDefinitionSO BuildFromJsonPath(
            string jsonPath,
            string stageDefinitionOutputFolder,
            string stageNodeOutputFolder,
            string popupEventOutputFolder)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                throw new ArgumentException("Stage definition json path is empty.", nameof(jsonPath));
            }

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Stage definition json not found: {jsonPath}", jsonPath);
            }

            EnsureFolder(stageDefinitionOutputFolder);
            EnsureFolder(stageNodeOutputFolder);
            EnsureFolder(popupEventOutputFolder);

            string jsonText = File.ReadAllText(jsonPath);
            StageDefinitionJson root = JsonUtility.FromJson<StageDefinitionJson>(jsonText);
            if (root == null)
            {
                throw new InvalidDataException($"Invalid stage definition json: {jsonPath}");
            }

            if (string.IsNullOrWhiteSpace(root.stageId))
            {
                throw new InvalidDataException($"Stage definition json has empty stageId: {jsonPath}");
            }

            string assetPath = $"{stageDefinitionOutputFolder.TrimEnd('/')}/{root.stageId}.asset";
            StageDefinitionSO asset = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StageDefinitionSO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.stageId = root.stageId;
            asset.stageName = root.stageName;
            asset.useFixedSeed = root.useFixedSeed;
            asset.seed = root.seed;

            asset.requiredSubEvents.Clear();
            asset.requiredSubEvents.AddRange(BuildRequiredNodes(
                root.requiredSubEvents,
                stageNodeOutputFolder,
                popupEventOutputFolder));

            asset.segmentRules.Clear();
            asset.segmentRules.AddRange(BuildSegmentRules(root.segmentRules));

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[StageDefinitionBuilder] Generated StageDefinitionSO. Json={jsonPath}, Asset={assetPath}");
            return asset;
        }

        private static List<StageRequiredNode> BuildRequiredNodes(
            List<RequiredNodeJson> nodes,
            string stageNodeOutputFolder,
            string popupEventOutputFolder)
        {
            List<StageRequiredNode> result = new();
            if (nodes == null)
            {
                return result;
            }

            foreach (RequiredNodeJson nodeJson in nodes)
            {
                if (nodeJson == null || string.IsNullOrWhiteSpace(nodeJson.jsonPath))
                {
                    continue;
                }

                RoundNodeSO node = BuildRoundNodeFromActJson(
                    nodeJson.jsonPath,
                    stageNodeOutputFolder,
                    popupEventOutputFolder);

                if (node == null)
                {
                    Debug.LogWarning($"[StageDefinitionBuilder] Failed to build required node. Json={nodeJson.jsonPath}");
                    continue;
                }

                StageRequiredNode requiredNode = new()
                {
                    node = node,
                    depth = nodeJson.fixedDepth,
                    column = nodeJson.fixedColumn,
                    hiddenByDefault = nodeJson.hiddenByDefault
                };

                result.Add(requiredNode);
            }

            return result
                .Where(x => x != null && x.node != null)
                .OrderBy(x => x.depth)
                .ThenBy(x => x.column)
                .ToList();
        }

        private static RoundNodeSO BuildRoundNodeFromActJson(
            string actJsonPath,
            string stageNodeOutputFolder,
            string popupEventOutputFolder)
        {
            if (!File.Exists(actJsonPath))
            {
                Debug.LogWarning($"[StageDefinitionBuilder] Act json not found: {actJsonPath}");
                return null;
            }

            object returned = InvokeStageNodeBuilder(
                actJsonPath,
                stageNodeOutputFolder,
                popupEventOutputFolder);

            if (returned is RoundNodeSO roundNode)
            {
                return roundNode;
            }

            string stageNodeId = ReadStageNodeId(actJsonPath);
            if (string.IsNullOrWhiteSpace(stageNodeId))
            {
                return null;
            }

            string assetPath = $"{stageNodeOutputFolder.TrimEnd('/')}/{stageNodeId}.asset";
            return AssetDatabase.LoadAssetAtPath<RoundNodeSO>(assetPath);
        }

        private static object InvokeStageNodeBuilder(
            string actJsonPath,
            string stageNodeOutputFolder,
            string popupEventOutputFolder)
        {
            MethodInfo method = typeof(StageNodeBuilder).GetMethod(
                "BuildFromJsonPath",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(string), typeof(string) },
                null);

            if (method == null)
            {
                throw new MissingMethodException(
                    nameof(StageNodeBuilder),
                    "BuildFromJsonPath(string, string, string)");
            }

            return method.Invoke(
                null,
                new object[]
                {
                    actJsonPath,
                    stageNodeOutputFolder,
                    popupEventOutputFolder
                });
        }

        private static string ReadStageNodeId(string actJsonPath)
        {
            string jsonText = File.ReadAllText(actJsonPath);
            ActJsonHeader header = JsonUtility.FromJson<ActJsonHeader>(jsonText);
            return header != null
                ? header.stageNodeId
                : string.Empty;
        }

        private static List<StageSegmentRule> BuildSegmentRules(List<SegmentRuleJson> rules)
        {
            List<StageSegmentRule> result = new();
            if (rules == null)
            {
                return result;
            }

            foreach (SegmentRuleJson json in rules)
            {
                if (json == null)
                {
                    continue;
                }

                StageSegmentRule rule = new()
                {
                    fromDepth = json.fromDepth,
                    fromColumn = json.fromColumn,
                    toDepth = json.toDepth,
                    toColumn = json.toColumn,
                    minTotalWeight = json.minTotalWeight,
                    maxTotalWeight = json.maxTotalWeight,
                    minLayerCount = json.minLayerCount,
                    maxLayerCount = json.maxLayerCount,
                    minLayerWeight = json.minLayerWeight,
                    maxLayerWeight = json.maxLayerWeight,
                    minNodesPerLayer = json.minNodesPerLayer,
                    maxNodesPerLayer = json.maxNodesPerLayer,
                    pools = ResolvePools(json.pools)
                };

                result.Add(rule);
            }

            return result;
        }

        private static List<StageSegmentPoolRule> ResolvePools(List<PoolRefJson> pools)
        {
            List<StageSegmentPoolRule> result = new();
            if (pools == null || pools.Count == 0)
            {
                return result;
            }

            EventPoolSO[] allPools = Resources.LoadAll<EventPoolSO>(string.Empty);
            foreach (PoolRefJson poolRef in pools)
            {
                if (poolRef == null || string.IsNullOrWhiteSpace(poolRef.poolId))
                {
                    continue;
                }

                EventPoolSO pool = allPools.FirstOrDefault(x => x != null && x.poolId == poolRef.poolId);
                if (pool == null)
                {
                    Debug.LogWarning($"[StageDefinitionBuilder] EventPoolSO not found. poolId={poolRef.poolId}");
                    continue;
                }

                result.Add(new StageSegmentPoolRule
                {
                    pool = pool,
                    required = poolRef.required,
                    maxAppearCount = poolRef.maxAppearCount,
                    priority = poolRef.priority
                });
            }

            return result;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidDataException($"Unity asset folder path must start with Assets: {folderPath}");
            }

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}