#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Battle;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class BattleJsonGenerator
    {
        private const string MenuRoot = "Assets/Battle";

        [MenuItem(MenuRoot + "/Generate BattleSO From Json", false, 2000)]
        public static void GenerateFromSelectedJson()
        {
            UnityEngine.Object selected = Selection.activeObject;

            if (selected == null)
            {
                Debug.LogWarning("[BattleJsonGenerator] Select a battle json file first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(selected);

            if (string.IsNullOrEmpty(jsonPath) ||
                !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"[BattleJsonGenerator] Selected asset is not json. path={jsonPath}");
                return;
            }

            GenerateFromJsonPath(jsonPath);
        }

        [MenuItem(MenuRoot + "/Generate BattleSO From Json", true)]
        public static bool ValidateGenerateFromSelectedJson()
        {
            UnityEngine.Object selected = Selection.activeObject;

            if (selected == null)
            {
                return false;
            }

            string jsonPath = AssetDatabase.GetAssetPath(selected);

            return !string.IsNullOrEmpty(jsonPath) &&
                jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        public static IReadOnlyList<BattleSO> GenerateFromPath(
            string inputPath,
            bool includeSubFolders = true)
        {
            List<BattleSO> results = new();
            IReadOnlyList<string> jsonFiles = CollectBattleJsonFiles(inputPath, includeSubFolders);

            foreach (string jsonFile in jsonFiles)
            {
                BattleSO generated = GenerateFromJsonPath(jsonFile);
                if (generated != null)
                {
                    results.Add(generated);
                }
            }

            return results;
        }

        public static IReadOnlyList<string> CollectBattleJsonFiles(
            string inputPath,
            bool includeSubFolders = true)
        {
            List<string> result = new();

            if (string.IsNullOrEmpty(inputPath))
            {
                return result;
            }

            inputPath = BattleAssetBuilderUtility.NormalizeAssetPath(inputPath);

            if (File.Exists(ToFullPath(inputPath)))
            {
                if (IsBattleJson(inputPath))
                {
                    result.Add(inputPath);
                }

                return result;
            }

            string fullPath = ToFullPath(inputPath);
            if (!Directory.Exists(fullPath))
            {
                return result;
            }

            SearchOption option = includeSubFolders
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            foreach (string file in Directory.GetFiles(fullPath, "*.json", option))
            {
                string assetPath = ToAssetPath(file);
                if (IsBattleJson(assetPath))
                {
                    result.Add(assetPath);
                }
            }

            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        public static bool IsBattleJson(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                return false;
            }

            string fullPath = ToFullPath(jsonPath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                if (string.IsNullOrWhiteSpace(json) || !json.Contains("\"battleId\""))
                {
                    return false;
                }

                BattleJson data = JsonUtility.FromJson<BattleJson>(json);
                return data != null && !string.IsNullOrEmpty(data.battleId);
            }
            catch
            {
                return false;
            }
        }

        public static BattleSO GenerateFromData(BattleJson data)
        {
            string outputFolder = "Assets/Resources/generated/battle";

            if (!BattleJsonValidation.ValidateParsed(data, "GenerateFromData"))
            {
                return null;
            }

            return BattleSOAssetBuilder.CreateOrUpdate(data, outputFolder);
        }

        public static BattleSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath))
            {
                Debug.LogWarning("[BattleJsonGenerator] Json path is empty.");
                return null;
            }

            string fullPath = ToFullPath(jsonPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[BattleJsonGenerator] Json file not found. path={jsonPath}");
                return null;
            }

            string json = File.ReadAllText(fullPath);
            BattleJson data = JsonUtility.FromJson<BattleJson>(json);

            if (data == null || string.IsNullOrEmpty(data.battleId))
            {
                Debug.LogError($"[BattleJsonGenerator] Invalid battle json. path={jsonPath}");
                return null;
            }

            if (!BattleJsonValidation.ValidateParsed(data, jsonPath))
            {
                return null;
            }

            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[BattleJsonGenerator] Cannot resolve output folder.");
                return null;
            }

            return BattleSOAssetBuilder.CreateOrUpdate(data, outputFolder);
        }

        private static string ToAssetPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return string.Empty;
            }

            string normalizedFullPath = BattleAssetBuilderUtility.NormalizeAssetPath(fullPath);
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            string normalizedProjectRoot =
                BattleAssetBuilderUtility.NormalizeAssetPath(projectRoot ?? string.Empty);

            if (!string.IsNullOrEmpty(normalizedProjectRoot) &&
                normalizedFullPath.StartsWith(normalizedProjectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return normalizedFullPath.Substring(normalizedProjectRoot.Length).TrimStart('/');
            }

            return normalizedFullPath;
        }

        private static string ToFullPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(assetPath))
            {
                return assetPath;
            }

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(projectRoot ?? string.Empty, assetPath);
        }

        [Serializable]
        public class BattleJson
        {
            public string battleId;
            public string battleName;
            public string victoryRule;
            public float survivalTimeSeconds;
            public string backgroundPrefab;
            public string spawnSequenceId;
            public string spawnSequencePath;
            public List<SpawnUnitBindingJson> spawnUnitBindings = new();
            public int rewardExperience;
            public float normalRelicDropChance;
            public float bossRelicDropChance;
            public List<TimedPropPlacementJson> timedPropPlacements = new();
            public List<PropDefinitionJson> propDefinitions = new();
        }

        [Serializable]
        public class SpawnUnitBindingJson
        {
            public string unitKey;
            public string role;
            public string characterId;
        }

        [Serializable]
        public class PropDefinitionJson
        {
            public string propId;
            public string role;
            public string prefab;
            public List<string> skills = new();
            public List<PropStateVisualJson> stateVisuals = new();
            public SpawnOnHitJson spawnOnHit;
            public SpawnSequenceSpawnerJson spawnSequenceSpawner;
        }

        [Serializable]
        public class PropStateVisualJson
        {
            public string state;
            public string animationClip;
            public string effectPrefab;
        }

        [Serializable]
        public class SpawnOnHitJson
        {
            public int spawnHitThreshold = 10;
            public string spawnPropOnHit;
            public bool destroyAfterSpawnOnHit = true;
        }

        [Serializable]
        public class SpawnSequenceSpawnerJson
        {
            public string spawnSequenceId;
            public string spawnSequencePath;
            public bool playOnInitialize;
        }

        [Serializable]
        public class TimedPropPlacementJson
        {
            public float spawnTimeSeconds;
            public string propId;
            public Vector3Json position;
            public float rotationZ;
            public string runtimeId;
        }

        [Serializable]
        public class Vector3Json
        {
            public float x;
            public float y;
            public float z;

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }
    }
}
#endif
