using System;
using System.Collections.Generic;
using System.IO;
using Skill;
using UnityEditor;
using UnityEngine;
using Effect;

namespace ResourceTools.Skill
{
    public static class SkillUpgradeAsssetBuilder
    {
        private const string DefaultAssetFolder = "Assets/Resources/skill/upgrade";

        [MenuItem("Tools/Resource Tools/Skill/Generate Upgrade Table From Selected Json")]
        public static void GenerateFromSelectedJson()
        {
            TextAsset selectedJson = Selection.activeObject as TextAsset;
            if (selectedJson == null)
            {
                Debug.LogWarning("[SkillUpgradeAsssetBuilder] Select a skill upgrade json TextAsset first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(selectedJson);
            GenerateFromJsonPath(jsonPath);
        }

        public static EquipmentUpgradeTableSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                Debug.LogWarning("[SkillUpgradeAsssetBuilder] Json path is empty.");
                return null;
            }

            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning($"[SkillUpgradeAsssetBuilder] Json file not found. path={jsonPath}");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            SkillUpgradeTableJson tableJson = JsonUtility.FromJson<SkillUpgradeTableJson>(json);

            if (tableJson == null)
            {
                Debug.LogWarning($"[SkillUpgradeAsssetBuilder] Invalid json. path={jsonPath}");
                return null;
            }

            return CreateOrUpdate(
                tableJson,
                jsonPath);
        }

        public static EquipmentUpgradeTableSO CreateOrUpdate(
            SkillUpgradeTableJson tableJson,
            string sourceJsonPath = null)
        {
            if (tableJson == null)
            {
                return null;
            }

            string tableId = ResolveTableId(tableJson);
            string assetPath = ResolveAssetPath(
                tableJson,
                tableId,
                sourceJsonPath);

            assetPath = NormalizeAssetPath(assetPath);

            if (!assetPath.StartsWith("Assets/", StringComparison.Ordinal))
            {
                Debug.LogError($"[SkillUpgradeAsssetBuilder] Invalid asset path. path={assetPath}");
                return null;
            }

            string assetFolder = Path.GetDirectoryName(assetPath);
            assetFolder = NormalizeAssetPath(assetFolder);
            EnsureFolder(assetFolder);

            Debug.Log($"[SkillUpgradeAsssetBuilder] CreateOrUpdate path resolved. sourceJsonPath={sourceJsonPath} assetPath={assetPath}");

            EquipmentUpgradeTableSO tableSo =
                AssetDatabase.LoadAssetAtPath<EquipmentUpgradeTableSO>(assetPath);

            if (tableSo == null && File.Exists(assetPath))
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                tableSo = AssetDatabase.LoadAssetAtPath<EquipmentUpgradeTableSO>(assetPath);
            }

            if (tableSo == null)
            {
                tableSo = ScriptableObject.CreateInstance<EquipmentUpgradeTableSO>();
                AssetDatabase.CreateAsset(tableSo, assetPath);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                tableSo = AssetDatabase.LoadAssetAtPath<EquipmentUpgradeTableSO>(assetPath) ?? tableSo;
            }

            if (tableSo == null)
            {
                Debug.LogError($"[SkillUpgradeAsssetBuilder] Failed to create upgrade table asset. path={assetPath}");
                return null;
            }

            ApplyTableJson(tableSo, tableJson, tableId);
            EditorUtility.SetDirty(tableSo);

            AssetDatabase.SaveAssetIfDirty(tableSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            EquipmentUpgradeTableSO savedTableSo =
                AssetDatabase.LoadAssetAtPath<EquipmentUpgradeTableSO>(assetPath);

            if (savedTableSo == null)
            {
                Debug.LogError($"[SkillUpgradeAsssetBuilder] Upgrade table asset was saved but cannot be loaded. path={assetPath}");
            }


            Debug.Log($"[SkillUpgradeAsssetBuilder] Generated upgrade table. id={tableId} path={assetPath}");
            return tableSo;
        }

        private static void ApplyTableJson(
            EquipmentUpgradeTableSO tableSo,
            SkillUpgradeTableJson tableJson,
            string tableId)
        {

            List<EquipmentUpgradeEntry> entries = new();

            if (tableJson.entries != null)
            {
                for (int i = 0; i < tableJson.entries.Count; i++)
                {
                    SkillUpgradeEntryJson entryJson = tableJson.entries[i];
                    if (entryJson == null)
                    {
                        continue;
                    }

                    EquipmentUpgradeEntry entry = new();
                    entry.ApplyEditorData(
                        Mathf.Max(1, entryJson.level),
                        CreateStatModifiers(entryJson.statModifiers),
                        CreateEffectModifiers(entryJson.effectModifiers));

                    entries.Add(entry);
                }
            }

            entries.Sort((a, b) => a.Level.CompareTo(b.Level));
            tableSo.ApplyEditorData(
                tableId,
                entries);
        }

        private static List<SkillStatModifierData> CreateStatModifiers(
            List<SkillStatModifierJson> modifierJsons)
        {
            List<SkillStatModifierData> result = new();

            if (modifierJsons == null)
            {
                return result;
            }

            for (int i = 0; i < modifierJsons.Count; i++)
            {
                SkillStatModifierJson modifierJson = modifierJsons[i];
                if (modifierJson == null)
                {
                    continue;
                }

                if (!Enum.TryParse(
                        modifierJson.modifierType,
                        true,
                        out SkillStatModifierType modifierType))
                {
                    Debug.LogWarning($"[SkillUpgradeAsssetBuilder] Invalid modifierType. value={modifierJson.modifierType}");
                    continue;
                }

                SkillStatModifierData modifier = new SkillStatModifierData();
                modifier.ApplyEditorData(
                    modifierType,
                    ResolveOperationType(modifierJson),
                    modifierJson.value);

                result.Add(modifier);
            }

            return result;
        }

        private static List<EffectUpgradeModifierData> CreateEffectModifiers(
            List<EffectUpgradeModifierJson> modifierJsons)
        {
            List<EffectUpgradeModifierData> result = new();

            if (modifierJsons == null)
            {
                return result;
            }

            for (int i = 0; i < modifierJsons.Count; i++)
            {
                EffectUpgradeModifierJson modifierJson = modifierJsons[i];
                if (modifierJson == null)
                {
                    continue;
                }

                EffectUpgradeModifierData modifier = new EffectUpgradeModifierData();
                modifier.ApplyEditorData(
                    modifierJson.targetEffectId,
                    ResolveEffectFieldType(modifierJson),
                    ResolveOperationType(modifierJson),
                    modifierJson.value);

                result.Add(modifier);
            }

            return result;
        }

        private static SkillStatModifierOperationType ResolveOperationType(
            SkillStatModifierJson modifierJson)
        {
            if (modifierJson == null)
            {
                return SkillStatModifierOperationType.Flat;
            }

            string operationName = modifierJson.hasOperationType
                ? modifierJson.operationType
                : "Flat";

            return Enum.TryParse(
                operationName,
                true,
                out SkillStatModifierOperationType operationType)
                ? operationType
                : SkillStatModifierOperationType.Flat;
        }

        private static SkillStatModifierOperationType ResolveOperationType(
            EffectUpgradeModifierJson modifierJson)
        {
            Enum.TryParse(
                modifierJson.operationType,
                true,
                out SkillStatModifierOperationType operationType);

            return operationType;
        }

        private static EffectModifierFieldType ResolveEffectFieldType(
            EffectUpgradeModifierJson modifierJson)
        {
            Enum.TryParse(
                modifierJson.fieldType,
                true,
                out EffectModifierFieldType fieldType);

            return fieldType;
        }

        private static string ResolveTableId(SkillUpgradeTableJson tableJson)
        {
            if (!string.IsNullOrWhiteSpace(tableJson.upgradeTableId))
            {
                return tableJson.upgradeTableId;
            }

            return "upgrade.generated";
        }

        private static string ResolveAssetPath(
            SkillUpgradeTableJson tableJson,
            string tableId,
            string sourceJsonPath)
        {
            if (!string.IsNullOrWhiteSpace(tableJson.assetPath))
            {
                return tableJson.assetPath;
            }

            if (!string.IsNullOrWhiteSpace(sourceJsonPath))
            {
                string sourceFolder = Path.GetDirectoryName(sourceJsonPath);
                if (!string.IsNullOrWhiteSpace(sourceFolder))
                {
                    return NormalizeAssetPath($"{sourceFolder}/{CreateSafeAssetFileName(tableId)}.asset");
                }
            }

            return NormalizeAssetPath($"{DefaultAssetFolder}/{CreateSafeAssetFileName(tableId)}.asset");
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? path
                : path.Replace("\\", "/");
        }

        private static string CreateSafeAssetFileName(string tableId)
        {
            return string.IsNullOrWhiteSpace(tableId)
                ? "upgrade.generated"
                : tableId
                    .Replace("/", "_")
                    .Replace("\\", "_")
                    .Replace(" ", "_");
        }



        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
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

        [Serializable]
        public class SkillUpgradeTableJson
        {
            public string upgradeTableId;
            public string assetPath;
            public List<SkillUpgradeEntryJson> entries = new();
        }

        [Serializable]
        public class SkillUpgradeEntryJson
        {
            public int level = 1;
            public List<SkillStatModifierJson> statModifiers = new();
            public List<EffectUpgradeModifierJson> effectModifiers = new();
        }

        [Serializable]
        public class SkillStatModifierJson
        {
            public string modifierType;
            public string operationType = "Flat";
            public bool hasOperationType;
            public float value;
        }

        [Serializable]
        public class EffectUpgradeModifierJson
        {
            public string targetEffectId;
            public string fieldType;
            public string operationType;
            public float value;
        }
    }
}