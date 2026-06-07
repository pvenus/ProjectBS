#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Character;
using UnityEditor;
using UnityEngine;
using Wave.SO;

namespace ResourceTools
{
    public static class StageWaveJsonGenerator
    {
        [Serializable]
        private class StageWaveJson
        {
            public string waveId;
            public float duration;
            public SpawnAreaJson spawnArea;
            public List<SpawnPhaseJson> phases = new();
        }

        [Serializable]
        private class SpawnAreaJson
        {
            public float spawnX;
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
            public List<SpawnMonsterJson> monsters = new();
        }

        [Serializable]
        private class SpawnMonsterJson
        {
            public string characterId;
            public int weight = 1;
        }

        [MenuItem("Assets/Wave/Generate StageWaveSO From Json", true)]
        private static bool ValidateGenerate()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            return !string.IsNullOrEmpty(path)
                && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Wave/Generate StageWaveSO From Json", false, 2200)]
        private static void Generate()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogError("[StageWaveJsonGenerator] Select a json file first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(selectedObject);
            GenerateFromJsonPath(jsonPath);
        }

        public static StageWaveSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[StageWaveJsonGenerator] Invalid json path: {jsonPath}");
                return null;
            }

            string fullPath = ToFullPath(jsonPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[StageWaveJsonGenerator] Json file not found: {jsonPath}");
                return null;
            }

            string json = File.ReadAllText(fullPath);
            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            return GenerateFromJsonText(
                json,
                outputFolder,
                jsonPath);
        }

        public static StageWaveSO GenerateFromJsonText(
            string json,
            string outputFolder,
            string sourceLabel = null)
        {
            StageWaveJson data = JsonUtility.FromJson<StageWaveJson>(json);

            if (data == null || string.IsNullOrEmpty(data.waveId))
            {
                Debug.LogError($"[StageWaveJsonGenerator] Invalid StageWave json: {sourceLabel}");
                return null;
            }

            return CreateOrUpdateWaveSO(
                data,
                outputFolder,
                sourceLabel);
        }

        private static StageWaveSO CreateOrUpdateWaveSO(
            StageWaveJson data,
            string outputFolder,
            string sourceLabel)
        {
            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[StageWaveJsonGenerator] Cannot resolve output folder.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.waveId}.asset";
            StageWaveSO waveSo = AssetDatabase.LoadAssetAtPath<StageWaveSO>(assetPath);
            bool isNewAsset = false;

            if (waveSo == null)
            {
                waveSo = ScriptableObject.CreateInstance<StageWaveSO>();
                isNewAsset = true;
            }

            ApplyData(waveSo, data);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(waveSo, assetPath);
                Debug.Log($"[StageWaveJsonGenerator] Created StageWaveSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(waveSo);
                Debug.Log($"[StageWaveJsonGenerator] Updated StageWaveSO: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return waveSo;
        }

        private static void ApplyData(StageWaveSO waveSo, StageWaveJson data)
        {
            SetField(waveSo, "waveId", data.waveId);
            SetField(waveSo, "duration", data.duration);

            if (data.spawnArea != null)
            {
                SetField(waveSo, "spawnX", data.spawnArea.spawnX);
                SetField(waveSo, "yMin", data.spawnArea.yMin);
                SetField(waveSo, "yMax", data.spawnArea.yMax);
                SetField(waveSo, "groupSpreadRadius", data.spawnArea.groupSpreadRadius);
            }

            SetField(waveSo, "phases", ConvertPhases(data.phases));
        }

        private static List<SpawnPhase> ConvertPhases(List<SpawnPhaseJson> jsonPhases)
        {
            List<SpawnPhase> result = new();

            if (jsonPhases == null)
            {
                return result;
            }

            foreach (SpawnPhaseJson jsonPhase in jsonPhases)
            {
                if (jsonPhase == null)
                {
                    continue;
                }

                SpawnPhase phase = new SpawnPhase
                {
                    phaseId = jsonPhase.phaseId,
                    startTime = jsonPhase.startTime,
                    endTime = jsonPhase.endTime,
                    spawnInterval = jsonPhase.spawnInterval,
                    spawnCountPerTick = jsonPhase.spawnCountPerTick,
                    maxAliveCount = jsonPhase.maxAliveCount,
                    monsters = ConvertMonsters(jsonPhase.monsters)
                };

                result.Add(phase);
            }

            return result;
        }

        private static List<SpawnMonsterEntry> ConvertMonsters(List<SpawnMonsterJson> jsonMonsters)
        {
            List<SpawnMonsterEntry> result = new();

            if (jsonMonsters == null)
            {
                return result;
            }

            foreach (SpawnMonsterJson jsonMonster in jsonMonsters)
            {
                if (jsonMonster == null || string.IsNullOrEmpty(jsonMonster.characterId))
                {
                    continue;
                }

                CharacterSO characterSo = FindCharacterById(jsonMonster.characterId);

                if (characterSo == null)
                {
                    Debug.LogWarning($"[StageWaveJsonGenerator] CharacterSO not found: {jsonMonster.characterId}");
                    continue;
                }

                result.Add(new SpawnMonsterEntry
                {
                    characterSo = characterSo,
                    weight = jsonMonster.weight
                });
            }

            return result;
        }

        private static CharacterSO FindCharacterById(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return null;
            }

            string normalizedTarget = NormalizeKey(characterId);
            string[] guids = AssetDatabase.FindAssets("t:CharacterSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharacterSO characterSo = AssetDatabase.LoadAssetAtPath<CharacterSO>(path);

                if (characterSo == null)
                {
                    continue;
                }

                string assetName = Path.GetFileNameWithoutExtension(path);

                if (NormalizeKey(assetName) == normalizedTarget)
                {
                    return characterSo;
                }

                string soCharacterId = GetFieldValue<string>(characterSo, "characterId");

                if (NormalizeKey(soCharacterId) == normalizedTarget)
                {
                    return characterSo;
                }
            }

            return null;
        }

        private static string NormalizeKey(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value
                    .Trim()
                    .Replace(".", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLowerInvariant();
        }

        private static T GetFieldValue<T>(object target, string fieldName)
        {
            if (target == null)
            {
                return default;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                return default;
            }

            object value = field.GetValue(target);

            if (value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                Debug.LogWarning($"[StageWaveJsonGenerator] Field not found: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }

        private static string ToFullPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(projectRoot, assetPath);
        }
    }
}
#endif
