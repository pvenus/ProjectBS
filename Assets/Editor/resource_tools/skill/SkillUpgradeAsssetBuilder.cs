using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Skill;
using UnityEditor;
using UnityEngine;

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

            TryAssignToEquipmentSkill(tableJson, tableSo);

            Debug.Log($"[SkillUpgradeAsssetBuilder] Generated upgrade table. id={tableId} path={assetPath}");
            return tableSo;
        }

        private static void ApplyTableJson(
            EquipmentUpgradeTableSO tableSo,
            SkillUpgradeTableJson tableJson,
            string tableId)
        {
            SetPrivateField(tableSo, "upgradeTableId", tableId);
            SetPrivateField(tableSo, "displayName", tableJson.displayName);

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
                    SetPrivateField(entry, "level", Mathf.Max(1, entryJson.level));
                    SetPrivateField(
                        entry,
                        "statModifiers",
                        CreateStatModifiers(entryJson.statModifiers));

                    entries.Add(entry);
                }
            }

            entries.Sort((a, b) => a.Level.CompareTo(b.Level));
            SetPrivateField(tableSo, "entries", entries);
        }

        private static List<SkillStatModifierRuntimeData> CreateStatModifiers(
            List<SkillStatModifierJson> modifierJsons)
        {
            List<SkillStatModifierRuntimeData> result = new();

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

                result.Add(
                    new SkillStatModifierRuntimeData
                    {
                        modifierType = modifierJson.statType,
                        operationType = ResolveOperationType(modifierJson),
                        value = modifierJson.value
                    });
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

            return modifierJson.hasOperationType
                ? modifierJson.operationType
                : modifierJson.modifierType;
        }

        private static string ResolveTableId(SkillUpgradeTableJson tableJson)
        {
            if (!string.IsNullOrWhiteSpace(tableJson.upgradeTableId))
            {
                return tableJson.upgradeTableId;
            }

            if (!string.IsNullOrWhiteSpace(tableJson.skillId))
            {
                return $"upgrade.{tableJson.skillId}";
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

        private static void TryAssignToEquipmentSkill(
            SkillUpgradeTableJson tableJson,
            EquipmentUpgradeTableSO tableSo)
        {
            if (tableJson == null || tableSo == null)
            {
                return;
            }

            EquipmentSkillSO equipmentSkillSo = null;

            if (!string.IsNullOrWhiteSpace(tableJson.equipmentSkillAssetPath))
            {
                equipmentSkillSo =
                    AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(tableJson.equipmentSkillAssetPath);
            }

            if (equipmentSkillSo == null && !string.IsNullOrWhiteSpace(tableJson.skillId))
            {
                equipmentSkillSo = FindEquipmentSkillBySkillId(tableJson.skillId);
            }

            if (equipmentSkillSo == null)
            {
                return;
            }

            SetPrivateField(equipmentSkillSo, "upgradeTableSo", tableSo);
            EditorUtility.SetDirty(equipmentSkillSo);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SkillUpgradeAsssetBuilder] Assigned upgrade table to skill. skill={equipmentSkillSo.name}");
        }

        private static EquipmentSkillSO FindEquipmentSkillBySkillId(string skillId)
        {
            string[] guids = AssetDatabase.FindAssets("t:EquipmentSkillSO");

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EquipmentSkillSO skillSo =
                    AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(assetPath);

                if (skillSo == null)
                {
                    continue;
                }

                string resolvedSkillId = TryGetSkillId(skillSo);
                if (string.Equals(resolvedSkillId, skillId, StringComparison.Ordinal))
                {
                    return skillSo;
                }
            }

            return null;
        }

        private static string TryGetSkillId(EquipmentSkillSO skillSo)
        {
            if (skillSo == null)
            {
                return null;
            }

            Type type = skillSo.GetType();
            PropertyInfo property = type.GetProperty(
                "SkillId",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property != null && property.PropertyType == typeof(string))
            {
                return property.GetValue(skillSo) as string;
            }

            FieldInfo field = type.GetField(
                "skillId",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return field?.GetValue(skillSo) as string;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return;
            }

            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null)
            {
                Debug.LogWarning($"[SkillUpgradeAsssetBuilder] Field not found. type={target.GetType().Name} field={fieldName}");
                return;
            }

            field.SetValue(target, value);
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
            public string skillId;
            public string upgradeTableId;
            public string displayName;
            public string assetPath;
            public string equipmentSkillAssetPath;
            public List<SkillUpgradeEntryJson> entries = new();
        }

        [Serializable]
        public class SkillUpgradeEntryJson
        {
            public int level = 1;
            public List<SkillStatModifierJson> statModifiers = new();
        }

        [Serializable]
        public class SkillStatModifierJson
        {
            public SkillStatModifierType statType;
            public SkillStatModifierOperationType modifierType = SkillStatModifierOperationType.Flat;
            public SkillStatModifierOperationType operationType = SkillStatModifierOperationType.Flat;
            public bool hasOperationType;
            public float value;
        }
    }
}