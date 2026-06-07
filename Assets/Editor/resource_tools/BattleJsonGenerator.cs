

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Battle;
using Battle.Prop.SO;
using Battle.Prop;
using UnityEditor;
using UnityEngine;
using Wave.SO;

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

            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[BattleJsonGenerator] Cannot resolve output folder.");
                return null;
            }

            CreateOrUpdatePropDefinitions(
                data.propDefinitions,
                outputFolder,
                outputFolder);

            StageWaveSO waveSO = CreateOrFindWaveSO(
                data,
                outputFolder,
                jsonPath);

            string safeBattleAssetName = ToSafeAssetName(data.battleId);
            string assetPath = $"{outputFolder}/{safeBattleAssetName}.asset";
            BattleSO battleSO = AssetDatabase.LoadAssetAtPath<BattleSO>(assetPath);
            bool isNewAsset = false;

            if (battleSO == null)
            {
                battleSO = ScriptableObject.CreateInstance<BattleSO>();
                AssetDatabase.CreateAsset(battleSO, assetPath);
                isNewAsset = true;
            }

            ApplyData(
                battleSO,
                data,
                waveSO);

            EditorUtility.SetDirty(battleSO);
            AssetDatabase.SaveAssetIfDirty(battleSO);
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (isNewAsset)
            {
                Debug.Log($"[BattleJsonGenerator] Created BattleSO: {assetPath}");
            }
            else
            {
                Debug.Log($"[BattleJsonGenerator] Updated BattleSO: {assetPath}");
            }

            return battleSO;
        }

        private static void CreateOrUpdatePropDefinitions(
            List<PropDefinitionJson> propDefinitions,
            string outputFolder,
            string waveOutputFolder)
        {
            if (propDefinitions == null || propDefinitions.Count == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[BattleJsonGenerator] Cannot resolve prop output folder.");
                return;
            }

            Dictionary<string, BattlePropSO> createdProps = new();
            Dictionary<string, string> assetPaths = new();
            HashSet<string> newAssetKeys = new();

            for (int i = 0; i < propDefinitions.Count; i++)
            {
                PropDefinitionJson propData = propDefinitions[i];

                if (propData == null || string.IsNullOrEmpty(propData.propId))
                {
                    continue;
                }

                string normalizedKey = NormalizeKey(propData.propId);
                string safeAssetName = ToSafeAssetName(propData.propId);
                string assetPath = $"{outputFolder}/{safeAssetName}.asset";
                BattlePropSO propSO = AssetDatabase.LoadAssetAtPath<BattlePropSO>(assetPath);

                if (propSO == null)
                {
                    propSO = ScriptableObject.CreateInstance<BattlePropSO>();
                    AssetDatabase.CreateAsset(propSO, assetPath);
                    newAssetKeys.Add(normalizedKey);
                }

                createdProps[normalizedKey] = propSO;
                assetPaths[normalizedKey] = assetPath;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            for (int i = 0; i < propDefinitions.Count; i++)
            {
                PropDefinitionJson propData = propDefinitions[i];

                if (propData == null || string.IsNullOrEmpty(propData.propId))
                {
                    continue;
                }

                string normalizedKey = NormalizeKey(propData.propId);

                if (!createdProps.TryGetValue(normalizedKey, out BattlePropSO propSO))
                {
                    continue;
                }

                ApplyPropDefinition(
                    propSO,
                    propData,
                    waveOutputFolder,
                    createdProps);

                EditorUtility.SetDirty(propSO);
                AssetDatabase.SaveAssetIfDirty(propSO);

                if (assetPaths.TryGetValue(normalizedKey, out string assetPath))
                {
                    AssetDatabase.ImportAsset(
                        assetPath,
                        ImportAssetOptions.ForceUpdate);
                }

                if (newAssetKeys.Contains(normalizedKey))
                {
                    Debug.Log($"[BattleJsonGenerator] Created BattlePropSO: {assetPaths[normalizedKey]}");
                }
                else
                {
                    Debug.Log($"[BattleJsonGenerator] Updated BattlePropSO: {assetPaths[normalizedKey]}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private static void ApplyPropDefinition(
            BattlePropSO propSO,
            PropDefinitionJson propData,
            string waveOutputFolder,
            Dictionary<string, BattlePropSO> propLookup)
        {
            if (propSO == null || propData == null)
            {
                return;
            }

            propSO.propId = propData.propId;

            if (!string.IsNullOrEmpty(propData.role) &&
                Enum.TryParse(propData.role, true, out BattlePropRole parsedRole))
            {
                propSO.role = parsedRole;
            }

            propSO.prefab = FindPrefab(propData.prefab);
            propSO.skills = new List<ScriptableObject>();
            propSO.stateVisuals = ConvertPropStateVisuals(propData.stateVisuals);

            if (propData.spawnOnHit != null)
            {
                propSO.spawnHitThreshold = propData.spawnOnHit.spawnHitThreshold;
                propSO.spawnPropOnHit = ResolveBattlePropSO(
                    propData.spawnOnHit.spawnPropOnHit,
                    propLookup);
                propSO.destroyAfterSpawnOnHit = propData.spawnOnHit.destroyAfterSpawnOnHit;
            }

            if (propData.waveSpawner != null)
            {
                propSO.waveSO = CreateOrFindPropWaveSO(
                    propData.waveSpawner,
                    propData.propId,
                    waveOutputFolder);
                propSO.createWaveSpawnerOnInitialize =
                    propData.waveSpawner.createWaveSpawnerOnInitialize;
                propSO.waveSpawnerObjectName =
                    string.IsNullOrEmpty(propData.waveSpawner.waveSpawnerObjectName)
                        ? "PropNpcSpawner"
                        : propData.waveSpawner.waveSpawnerObjectName;
            }

            EditorUtility.SetDirty(propSO);
        }
        private static StageWaveSO CreateOrFindPropWaveSO(
            WaveSpawnerJson waveSpawner,
            string propId,
            string outputFolder)
        {
            if (waveSpawner == null)
            {
                return null;
            }

            if (waveSpawner.waveSO != null &&
                !string.IsNullOrEmpty(waveSpawner.waveSO.waveId))
            {
                if (string.IsNullOrEmpty(outputFolder))
                {
                    Debug.LogError("[BattleJsonGenerator] Prop wave output folder is empty.");
                    return null;
                }

                EnsureFolder(outputFolder);

                string stageJson = JsonUtility.ToJson(
                    waveSpawner.waveSO,
                    true);

                return StageWaveJsonGenerator.GenerateFromJsonText(
                    stageJson,
                    outputFolder,
                    propId);
            }

            if (!string.IsNullOrEmpty(waveSpawner.waveId))
            {
                return FindStageWaveSO(waveSpawner.waveId);
            }

            return null;
        }
        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) ||
                AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');

            if (parts.Length == 0 || parts[0] != "Assets")
            {
                return;
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

        private static List<BattlePropSO.PropStateVisualEntry> ConvertPropStateVisuals(
            List<PropStateVisualJson> visuals)
        {
            List<BattlePropSO.PropStateVisualEntry> result = new();

            if (visuals == null)
            {
                return result;
            }

            for (int i = 0; i < visuals.Count; i++)
            {
                PropStateVisualJson visual = visuals[i];

                if (visual == null)
                {
                    continue;
                }

                BattlePropState state = BattlePropState.Normal;

                if (!string.IsNullOrEmpty(visual.state))
                {
                    Enum.TryParse(visual.state, true, out state);
                }

                result.Add(new BattlePropSO.PropStateVisualEntry
                {
                    state = state,
                    animationClip = FindAnimationClip(visual.animationClip),
                    effectPrefab = FindPrefab(visual.effectPrefab)
                });
            }

            return result;
        }

        private static StageWaveSO CreateOrFindWaveSO(
            BattleJson data,
            string outputFolder,
            string battleJsonPath)
        {
            if (data.stage != null && !string.IsNullOrEmpty(data.stage.waveId))
            {
                string stageJson = JsonUtility.ToJson(data.stage, true);

                StageWaveSO generatedWave =
                    StageWaveJsonGenerator.GenerateFromJsonText(
                        stageJson,
                        outputFolder,
                        battleJsonPath);

                if (generatedWave != null)
                {
                    return generatedWave;
                }
            }

            if (!string.IsNullOrEmpty(data.waveJsonPath))
            {
                string resolvedPath = NormalizeAssetPath(
                    Path.Combine(outputFolder, data.waveJsonPath));

                StageWaveSO generatedWave =
                    StageWaveJsonGenerator.GenerateFromJsonPath(resolvedPath);

                if (generatedWave != null)
                {
                    return generatedWave;
                }
            }

            if (!string.IsNullOrEmpty(data.waveId))
            {
                string sameFolderWaveJson = NormalizeAssetPath(
                    Path.Combine(outputFolder, $"{data.waveId}.json"));

                if (File.Exists(ToFullPath(sameFolderWaveJson)))
                {
                    StageWaveSO generatedWave =
                        StageWaveJsonGenerator.GenerateFromJsonPath(sameFolderWaveJson);

                    if (generatedWave != null)
                    {
                        return generatedWave;
                    }
                }

                string stageFolderWaveJson = NormalizeAssetPath(
                    Path.Combine("Assets/Resources/battle/stage", $"{data.waveId}.json"));

                if (File.Exists(ToFullPath(stageFolderWaveJson)))
                {
                    StageWaveSO generatedWave =
                        StageWaveJsonGenerator.GenerateFromJsonPath(stageFolderWaveJson);

                    if (generatedWave != null)
                    {
                        return generatedWave;
                    }
                }

                StageWaveSO existingWave = FindStageWaveSO(data.waveId);

                if (existingWave != null)
                {
                    return existingWave;
                }

                Debug.LogWarning($"[BattleJsonGenerator] StageWaveSO not found. waveId={data.waveId} battle={battleJsonPath}");
            }

            return null;
        }

        private static void ApplyData(
            BattleSO battleSO,
            BattleJson data,
            StageWaveSO waveSO)
        {
            SetFieldOrProperty(battleSO, "battleId", data.battleId);
            SetFieldOrProperty(battleSO, "battleName", data.battleName);
            SetEnumFieldOrProperty(battleSO, "victoryRule", data.victoryRule);
            SetFieldOrProperty(battleSO, "survivalTimeSeconds", data.survivalTimeSeconds);
            SetFieldOrProperty(battleSO, "rewardExperience", data.rewardExperience);
            SetFieldOrProperty(battleSO, "normalRelicDropChance", data.normalRelicDropChance);
            SetFieldOrProperty(battleSO, "bossRelicDropChance", data.bossRelicDropChance);
            SetFieldOrProperty(battleSO, "backgroundPrefab", FindPrefab(data.backgroundPrefab));
            SetFieldOrProperty(battleSO, "waveSO", waveSO);

            List<BattleSO.TimedPropPlacement> timedPropPlacements =
                ConvertTimedPropPlacements(data.timedPropPlacements);

            SetFieldOrProperty(
                battleSO,
                "timedPropPlacements",
                timedPropPlacements);
        }

        private static List<BattleSO.TimedPropPlacement> ConvertTimedPropPlacements(
            List<TimedPropPlacementJson> jsonPlacements)
        {
            List<BattleSO.TimedPropPlacement> result = new();

            if (jsonPlacements == null)
            {
                return result;
            }

            foreach (TimedPropPlacementJson jsonPlacement in jsonPlacements)
            {
                if (jsonPlacement == null || string.IsNullOrEmpty(jsonPlacement.propId))
                {
                    continue;
                }

                BattlePropSO prop = FindBattlePropSO(jsonPlacement.propId);

                if (prop == null)
                {
                    Debug.LogWarning($"[BattleJsonGenerator] BattlePropSO not found. propId={jsonPlacement.propId}");
                    continue;
                }

                result.Add(new BattleSO.TimedPropPlacement
                {
                    spawnTimeSeconds = jsonPlacement.spawnTimeSeconds,
                    prop = prop,
                    position = jsonPlacement.position != null
                        ? jsonPlacement.position.ToVector3()
                        : Vector3.zero,
                    rotation = Quaternion.Euler(
                        0f,
                        0f,
                        jsonPlacement.rotationZ),
                    runtimeId = jsonPlacement.runtimeId
                });
            }

            return result;
        }

        private static StageWaveSO FindStageWaveSO(string waveId)
        {
            if (string.IsNullOrEmpty(waveId))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(waveId);
            string[] guids = AssetDatabase.FindAssets("t:StageWaveSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StageWaveSO waveSO = AssetDatabase.LoadAssetAtPath<StageWaveSO>(path);

                if (waveSO == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget)
                {
                    return waveSO;
                }

                string candidateWaveId = GetStringFieldOrProperty(waveSO, "waveId");

                if (NormalizeKey(candidateWaveId) == normalizedTarget)
                {
                    return waveSO;
                }
            }

            return null;
        }

        private static BattlePropSO FindBattlePropSO(string propId)
        {
            if (string.IsNullOrEmpty(propId))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(propId);
            string[] guids = AssetDatabase.FindAssets("t:BattlePropSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                BattlePropSO prop = AssetDatabase.LoadAssetAtPath<BattlePropSO>(path);

                if (prop == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget)
                {
                    return prop;
                }

                if (NormalizeKey(prop.propId) == normalizedTarget)
                {
                    return prop;
                }
            }

            return null;
        }

        private static AnimationClip FindAnimationClip(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(key);
            string[] guids = AssetDatabase.FindAssets("t:AnimationClip");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(clip.name) == normalizedTarget)
                {
                    return clip;
                }
            }

            Debug.LogWarning($"[BattleJsonGenerator] AnimationClip not found. key={key}");
            return null;
        }

        private static GameObject FindPrefab(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(key);
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget ||
                    NormalizeKey(prefab.name) == normalizedTarget)
                {
                    return prefab;
                }
            }

            Debug.LogWarning($"[BattleJsonGenerator] Prefab not found. key={key}");
            return null;
        }

        private static void SetFieldOrProperty(
            object target,
            string name,
            object value)
        {
            if (target == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field = target.GetType().GetField(name, flags);

            if (field != null)
            {
                field.SetValue(target, value);

                if (target is UnityEngine.Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                }

                return;
            }

            PropertyInfo property = target.GetType().GetProperty(name, flags);

            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value);

                if (target is UnityEngine.Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                }
            }
        }

        private static void SetEnumFieldOrProperty(
            object target,
            string name,
            string enumValue)
        {
            if (target == null || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(enumValue))
            {
                return;
            }

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field = target.GetType().GetField(name, flags);

            if (field != null && field.FieldType.IsEnum &&
                Enum.TryParse(field.FieldType, enumValue, true, out object parsedFieldValue))
            {
                field.SetValue(target, parsedFieldValue);

                if (target is UnityEngine.Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                }

                return;
            }

            PropertyInfo property = target.GetType().GetProperty(name, flags);

            if (property != null && property.CanWrite &&
                property.PropertyType.IsEnum &&
                Enum.TryParse(property.PropertyType, enumValue, true, out object parsedPropertyValue))
            {
                property.SetValue(target, parsedPropertyValue);

                if (target is UnityEngine.Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                }
            }
        }

        private static string GetStringFieldOrProperty(
            object target,
            string name)
        {
            if (target == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field = target.GetType().GetField(name, flags);

            if (field != null && field.FieldType == typeof(string))
            {
                return field.GetValue(target) as string;
            }

            PropertyInfo property = target.GetType().GetProperty(name, flags);

            if (property != null && property.PropertyType == typeof(string))
            {
                return property.GetValue(target) as string;
            }

            return null;
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

        private static string ToSafeAssetName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "BattleProp";
            }

            return value
                .Trim()
                .Replace(" ", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(":", "_");
        }

        private static string NormalizeAssetPath(string path)
        {
            return path.Replace("\\", "/");
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Trim().Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
        }

        [Serializable]
        private class BattleJson
        {
            public string battleId;
            public string battleName;
            public string victoryRule;
            public float survivalTimeSeconds;
            public string backgroundPrefab;
            public string waveId;
            public string waveJsonPath;
            public int rewardExperience;
            public float normalRelicDropChance;
            public float bossRelicDropChance;
            public List<TimedPropPlacementJson> timedPropPlacements = new();
            public List<PropDefinitionJson> propDefinitions = new();
            public StageJson stage;
        }

        [Serializable]
        private class PropDefinitionJson
        {
            public string propId;
            public string role;
            public string prefab;
            public List<string> skills = new();
            public List<PropStateVisualJson> stateVisuals = new();
            public SpawnOnHitJson spawnOnHit;
            public WaveSpawnerJson waveSpawner;
        }

        [Serializable]
        private class PropStateVisualJson
        {
            public string state;
            public string animationClip;
            public string effectPrefab;
        }

        [Serializable]
        private class SpawnOnHitJson
        {
            public int spawnHitThreshold = 10;
            public string spawnPropOnHit;
            public bool destroyAfterSpawnOnHit = true;
        }

        [Serializable]
        private class WaveSpawnerJson
        {
            public StageJson waveSO;
            public string waveId;
            public bool createWaveSpawnerOnInitialize;
            public string waveSpawnerObjectName;
        }

        [Serializable]
        private class StageJson
        {
            public string waveId;
            public float duration;
            public SpawnAreaJson spawnArea;
            public List<SpawnPhaseJson> phases = new();
        }

        [Serializable]
        private class SpawnAreaJson
        {
            public float spawnStartX;
            public float spawnEndX;
            public float yMin;
            public float yMax;
            public float groupSpreadRadius;
        }

        [Serializable]
        private class SpawnPhaseJson
        {
            public string phaseId;
            public float startTime;
            public float endTime;
            public float spawnInterval;
            public int spawnCountPerTick;
            public int maxAliveCount;
            public List<WeightedMonsterJson> monsters = new();
        }

        [Serializable]
        private class WeightedMonsterJson
        {
            public string characterId;
            public int weight;
        }

        [Serializable]
        private class TimedPropPlacementJson
        {
            public float spawnTimeSeconds;
            public string propId;
            public Vector3Json position;
            public float rotationZ;
            public string runtimeId;
        }

        [Serializable]
        private class Vector3Json
        {
            public float x;
            public float y;
            public float z;

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }
        // Inserted method: ResolveBattlePropSO
        private static BattlePropSO ResolveBattlePropSO(
            string propId,
            Dictionary<string, BattlePropSO> propLookup)
        {
            if (string.IsNullOrEmpty(propId))
            {
                return null;
            }

            string normalizedKey = NormalizeKey(propId);

            if (propLookup != null &&
                propLookup.TryGetValue(normalizedKey, out BattlePropSO localProp))
            {
                return localProp;
            }

            return FindBattlePropSO(propId);
        }
    }
}
#endif