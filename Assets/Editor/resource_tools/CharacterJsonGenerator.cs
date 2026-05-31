#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Character;
using Stat;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class CharacterJsonGenerator
    {
        [Serializable]
        private class CharacterJson
        {
            public string characterId;
            public string characterType;
            public string prefabName;
            public List<StatEntryJson> baseStats = new();
            public AnimationOverrideSetJson animationOverrideSet;
            public SkillOverrideSetJson skillOverrideSet;
        }

        [Serializable]
        private class StatEntryJson
        {
            public string statType;
            public float value;
        }

        [Serializable]
        private class AnimationOverrideSetJson
        {
            public string animationSetId;
        }

        [Serializable]
        private class SkillOverrideSetJson
        {
            public string overrideId;
            public List<SkillSlotJson> slots = new();
        }

        [Serializable]
        private class SkillSlotJson
        {
            public string slot;
            public string skillJson;
        }

        [MenuItem("Assets/Character/Generate CharacterSO From Json", false, 2000)]
        public static void Generate()
        {
            TextAsset jsonAsset = Selection.activeObject as TextAsset;

            if (jsonAsset == null)
            {
                Debug.LogError("[CharacterJsonGenerator] Select a character json file in the Project window first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(jsonAsset);
            GenerateFromJsonPath(jsonPath);
        }

        public static CharacterSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[CharacterJsonGenerator] Selected asset is not a json file.");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            CharacterJson data = JsonUtility.FromJson<CharacterJson>(json);

            if (data == null || string.IsNullOrEmpty(data.characterId))
            {
                Debug.LogError($"[CharacterJsonGenerator] Invalid character json: {jsonPath}");
                return null;
            }

            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[CharacterJsonGenerator] Cannot resolve output folder from json path.");
                return null;
            }

            string assetName = GetSafeAssetName(data.characterId);
            string assetPath = $"{outputFolder}/{assetName}.asset";

            CharacterSO characterSo = AssetDatabase.LoadAssetAtPath<CharacterSO>(assetPath);
            bool isNewAsset = false;

            if (characterSo == null)
            {
                characterSo = ScriptableObject.CreateInstance<CharacterSO>();
                isNewAsset = true;
            }

            SetField(characterSo, "characterId", data.characterId);
            SetEnumField(characterSo, "characterType", data.characterType);
            SetField(characterSo, "prefab", FindPrefabByName(data.prefabName));
            SetField(characterSo, "baseStats", ConvertBaseStats(data.baseStats));
            SetField(characterSo, "animationOverrideSet", GenerateOrLoadAnimationSet(data.animationOverrideSet, outputFolder, jsonPath));
            SetField(characterSo, "skillOverrideSet", CreateOrUpdateSkillOverrideSet(data.skillOverrideSet, outputFolder, jsonPath));

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(characterSo, assetPath);
                Debug.Log($"[CharacterJsonGenerator] Created CharacterSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(characterSo);
                Debug.Log($"[CharacterJsonGenerator] Updated CharacterSO: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return characterSo;
        }

        [MenuItem("Assets/Character/Generate CharacterSO From Json", true)]
        private static bool ValidateGenerate()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        private static List<StatEntry> ConvertBaseStats(List<StatEntryJson> stats)
        {
            List<StatEntry> result = new();

            if (stats == null)
            {
                return result;
            }

            foreach (StatEntryJson stat in stats)
            {
                if (stat == null || string.IsNullOrEmpty(stat.statType))
                {
                    continue;
                }

                try
                {
                    result.Add(new StatEntry
                    {
                        statType = (StatType)Enum.Parse(typeof(StatType), stat.statType, true),
                        value = stat.value
                    });
                }
                catch
                {
                    Debug.LogWarning($"[CharacterJsonGenerator] Failed to parse StatType: {stat.statType}");
                }
            }

            return result;
        }

        private static SkillPoolOverrideSO CreateOrUpdateSkillOverrideSet(
            SkillOverrideSetJson data,
            string outputFolder,
            string characterJsonPath)
        {
            if (data == null || string.IsNullOrEmpty(data.overrideId))
            {
                Debug.LogWarning("[CharacterJsonGenerator] SkillOverrideSetJson is empty. skillOverrideSet will not be assigned.");
                return null;
            }

            string assetName = GetSafeAssetName(data.overrideId) + "_SkillOverride";
            string assetPath = $"{outputFolder}/{assetName}.asset";

            SkillPoolOverrideSO overrideSo = AssetDatabase.LoadAssetAtPath<SkillPoolOverrideSO>(assetPath);
            bool isNewAsset = false;

            if (overrideSo == null)
            {
                overrideSo = ScriptableObject.CreateInstance<SkillPoolOverrideSO>();
                isNewAsset = true;
            }

            overrideSo.overrides.Clear();

            if (data.slots != null)
            {
                string characterFolder = Path.GetDirectoryName(characterJsonPath)?.Replace("\\", "/") ?? outputFolder;

                foreach (SkillSlotJson slot in data.slots)
                {
                    if (slot == null || string.IsNullOrEmpty(slot.slot) || string.IsNullOrEmpty(slot.skillJson))
                    {
                        continue;
                    }

                    string skillJsonPath = NormalizeAssetPath(Path.Combine(characterFolder, slot.skillJson));
                    EquipmentSkillSO skillSo = GenerateOrLoadSkill(skillJsonPath);

                    if (skillSo == null)
                    {
                        Debug.LogWarning($"[CharacterJsonGenerator] Skill generation failed. slot={slot.slot}, path={skillJsonPath}");
                        continue;
                    }

                    overrideSo.overrides.Add(new SkillPoolOverrideEntry
                    {
                        slotKey = slot.slot,
                        skillSo = skillSo
                    });
                }
            }

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(overrideSo, assetPath);
                Debug.Log($"[CharacterJsonGenerator] Created SkillPoolOverrideSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(overrideSo);
                Debug.Log($"[CharacterJsonGenerator] Updated SkillPoolOverrideSO: {assetPath}");
            }

            return overrideSo;
        }

        private static AnimationClipSetSO GenerateOrLoadAnimationSet(
            AnimationOverrideSetJson data,
            string outputFolder,
            string characterJsonPath)
        {
            if (data == null || string.IsNullOrEmpty(data.animationSetId))
            {
                Debug.LogWarning("[CharacterJsonGenerator] AnimationOverrideSetJson is empty. animationOverrideSet will not be assigned.");
                return null;
            }

            string characterFolder = Path.GetDirectoryName(characterJsonPath)?.Replace("\\", "/") ?? outputFolder;
            string animationFolderPath = $"{characterFolder}/animation";

            if (AssetDatabase.IsValidFolder(animationFolderPath))
            {
                AnimationClipSetSO generatedSet = GenerateClips.GenerateFromFolderPath(animationFolderPath);

                if (generatedSet != null)
                {
                    return generatedSet;
                }
            }
            else
            {
                Debug.LogWarning($"[CharacterJsonGenerator] Animation folder not found: {animationFolderPath}");
            }

            return FindAnimationClipSet(data.animationSetId);
        }

        private static EquipmentSkillSO GenerateOrLoadSkill(string skillJsonPath)
        {
            if (string.IsNullOrEmpty(skillJsonPath) || !File.Exists(skillJsonPath))
            {
                Debug.LogWarning($"[CharacterJsonGenerator] Skill json not found: {skillJsonPath}");
                return null;
            }

            EquipmentSkillSO generated = EquipmentSkillJsonGenerator.GenerateFromJsonPath(skillJsonPath);

            if (generated != null)
            {
                return generated;
            }

            string folder = Path.GetDirectoryName(skillJsonPath)?.Replace("\\", "/");
            string fileName = Path.GetFileNameWithoutExtension(skillJsonPath);
            string fallbackAssetPath = $"{folder}/{fileName}.asset";
            return AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(fallbackAssetPath);
        }

        private static GameObject FindPrefabByName(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    return prefab;
                }
            }

            Debug.LogWarning($"[CharacterJsonGenerator] Prefab not found: {prefabName}");
            return null;
        }

        private static AnimationClipSetSO FindAnimationClipSet(string animationSetId)
        {
            if (string.IsNullOrEmpty(animationSetId))
            {
                return null;
            }

            string safeName = GetSafeAssetName(animationSetId);
            string[] guids = AssetDatabase.FindAssets("t:AnimationClipSetSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AnimationClipSetSO set = AssetDatabase.LoadAssetAtPath<AnimationClipSetSO>(path);

                if (set == null)
                {
                    continue;
                }

                if (set.name.Equals(animationSetId, StringComparison.OrdinalIgnoreCase) ||
                    set.name.Equals(safeName, StringComparison.OrdinalIgnoreCase))
                {
                    return set;
                }
            }

            Debug.LogWarning($"[CharacterJsonGenerator] AnimationClipSetSO not found: {animationSetId}");
            return null;
        }

        private static string NormalizeAssetPath(string path)
        {
            return path.Replace("\\", "/");
        }

        private static string GetSafeAssetName(string id)
        {
            return string.IsNullOrEmpty(id)
                ? "generated_asset"
                : id.Replace(".", "_").Replace("/", "_").Replace(" ", "_");
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
                Debug.LogWarning($"[CharacterJsonGenerator] Field not found: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }

        private static void SetEnumField(object target, string fieldName, string enumName)
        {
            if (target == null || string.IsNullOrEmpty(enumName))
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
                Debug.LogWarning($"[CharacterJsonGenerator] Field not found: {fieldName}");
                return;
            }

            if (!field.FieldType.IsEnum)
            {
                Debug.LogWarning($"[CharacterJsonGenerator] Field is not enum: {fieldName}");
                return;
            }

            try
            {
                object parsedValue = Enum.Parse(field.FieldType, enumName, true);
                field.SetValue(target, parsedValue);
            }
            catch
            {
                Debug.LogWarning($"[CharacterJsonGenerator] Failed to parse enum {fieldName}: {enumName}");
            }
        }
    }
}
#endif
