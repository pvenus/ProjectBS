using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    public static class StagePoolGennerator
    {
        private const string DefaultJsonPath = "Assets/Resources/stage_new/stage_pool/job/job_event_pool.json";
        private const string DefaultPoolOutputFolder = "Assets/Resources/stage_new/event_pools";
        private const string DefaultStageNodeOutputFolder = "Assets/Resources/stage_new/nodes";
        private const string DefaultPopupEventOutputFolder = "Assets/Resources/stage_new/popup_events";

        [MenuItem("Assets/Stage/Stage Pool Generator", false, 2001)]
        public static void GenerateSelectedJsonMenu()
        {
            string selectedPath = GetSelectedAssetPath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                selectedPath = DefaultJsonPath;
            }

            GenerateFromJsonPath(
                selectedPath,
                DefaultPoolOutputFolder,
                DefaultStageNodeOutputFolder,
                DefaultPopupEventOutputFolder);
        }

        [MenuItem("Tools/Resource/Stage/Generate Job Event Pool")]
        public static void GenerateDefaultJobPoolMenu()
        {
            GenerateFromJsonPath(
                DefaultJsonPath,
                DefaultPoolOutputFolder,
                DefaultStageNodeOutputFolder,
                DefaultPopupEventOutputFolder);
        }

        public static EventPoolSO GenerateFromJsonPath(
            string jsonPath,
            string poolOutputFolder = DefaultPoolOutputFolder,
            string stageNodeOutputFolder = DefaultStageNodeOutputFolder,
            string popupEventOutputFolder = DefaultPopupEventOutputFolder)
        {
            jsonPath = NormalizeAssetPath(jsonPath);
            string fullPath = ToFullPath(jsonPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[StagePoolGennerator] Json file not found. path={jsonPath}");
                return null;
            }

            string json = File.ReadAllText(fullPath);
            EventPoolJson root = JsonUtility.FromJson<EventPoolJson>(json);

            if (root == null || string.IsNullOrWhiteSpace(root.poolId))
            {
                Debug.LogError($"[StagePoolGennerator] Invalid event pool json. path={jsonPath}");
                return null;
            }

            EnsureFolder(poolOutputFolder);
            EnsureFolder(stageNodeOutputFolder);
            EnsureFolder(popupEventOutputFolder);

            string assetPath = $"{NormalizeAssetPath(poolOutputFolder)}/{ToSafeAssetName(root.poolId)}.asset";
            EventPoolSO pool = AssetDatabase.LoadAssetAtPath<EventPoolSO>(assetPath);

            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<EventPoolSO>();
                AssetDatabase.CreateAsset(pool, assetPath);
            }

            pool.poolId = root.poolId;
            pool.displayName = root.displayName;
            pool.entries.Clear();

            if (root.entries != null)
            {
                foreach (EventPoolEntryJson entryJson in root.entries)
                {
                    EventPoolEntry entry = BuildEntry(
                        entryJson,
                        stageNodeOutputFolder,
                        popupEventOutputFolder);

                    if (entry != null && entry.node != null)
                    {
                        pool.entries.Add(entry);
                    }
                }
            }

            EditorUtility.SetDirty(pool);
            AssetDatabase.SaveAssetIfDirty(pool);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Debug.Log($"[StagePoolGennerator] Generated EventPoolSO. pool={root.poolId}, entries={pool.entries.Count}, path={assetPath}");
            return pool;
        }

        private static EventPoolEntry BuildEntry(
            EventPoolEntryJson entryJson,
            string stageNodeOutputFolder,
            string popupEventOutputFolder)
        {
            if (entryJson == null)
            {
                return null;
            }

            RoundNodeSO node = null;

            if (!string.IsNullOrWhiteSpace(entryJson.nodeJsonPath))
            {
                StageNodeBuilder.BuildResult buildResult = StageNodeBuilder.BuildFromJsonPath(
                    NormalizeAssetPath(entryJson.nodeJsonPath),
                    stageNodeOutputFolder,
                    popupEventOutputFolder);

                node = buildResult.stageNode;
            }

            if (node == null && !string.IsNullOrWhiteSpace(entryJson.nodeId))
            {
                node = FindRoundNodeById(entryJson.nodeId);
            }

            if (node == null)
            {
                Debug.LogWarning($"[StagePoolGennerator] Event pool entry skipped. entryId={entryJson.entryId}, nodeJsonPath={entryJson.nodeJsonPath}, nodeId={entryJson.nodeId}");
                return null;
            }

            return new EventPoolEntry
            {
                entryId = string.IsNullOrWhiteSpace(entryJson.entryId)
                    ? $"entry.{node.nodeId}"
                    : entryJson.entryId,
                node = node,
                weight = entryJson.weight <= 0 ? 1 : entryJson.weight,
                oneShot = entryJson.oneShot,
                cooldownRounds = Mathf.Max(0, entryJson.cooldownRounds),
                minDepth = entryJson.minDepth,
                maxDepth = entryJson.maxDepth,
                tags = entryJson.tags == null
                    ? new List<string>()
                    : entryJson.tags.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList()
            };
        }

        private static RoundNodeSO FindRoundNodeById(string nodeId)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets("t:RoundNodeSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RoundNodeSO node = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(path);
                if (node != null && node.nodeId == nodeId)
                {
                    return node;
                }
            }

            return null;
        }

        private static string GetSelectedAssetPath()
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null)
            {
                return string.Empty;
            }

            return AssetDatabase.GetAssetPath(selected);
        }

        private static void EnsureFolder(string assetFolder)
        {
            assetFolder = NormalizeAssetPath(assetFolder);
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];

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

        private static string ToFullPath(string assetPath)
        {
            assetPath = NormalizeAssetPath(assetPath);
            if (Path.IsPathRooted(assetPath))
            {
                return assetPath;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(projectRoot ?? string.Empty, assetPath);
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? string.Empty
                : path.Replace("\\", "/").Trim();
        }

        private static string ToSafeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "event_pool";
            }

            string result = value;
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalid, '_');
            }

            return result;
        }

        [Serializable]
        private class EventPoolJson
        {
            public string poolId;
            public string displayName;
            public List<EventPoolEntryJson> entries = new();
        }

        [Serializable]
        private class EventPoolEntryJson
        {
            public string entryId;
            public string nodeJsonPath;
            public string nodeId;
            public int weight = 1;
            public bool oneShot;
            public int cooldownRounds;
            public int minDepth;
            public int maxDepth;
            public List<string> tags = new();
        }
    }
}
