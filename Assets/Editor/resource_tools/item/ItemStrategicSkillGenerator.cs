using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Item;
using Skill;

namespace ResourceTools
{
    public static class ItemStrategicSkillGenerator
    {
        [MenuItem("Assets/Item/Strategic Item Generator", false, 2000)]
        public static void Generate()
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null)
            {
                Debug.LogError("[ItemStrategicSkillGenerator] Select item json file.");
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrWhiteSpace(assetPath) || !assetPath.EndsWith(".json"))
            {
                Debug.LogError("[ItemStrategicSkillGenerator] Selected asset is not json.");
                return;
            }

            GenerateFromJsonPath(assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Item/Generate All Strategic Item SO From Folder", false, 2001)]
        public static void GenerateAllFromSelectedFolder()
        {
            string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrWhiteSpace(folderPath) ||
                !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError(
                    "[ItemStrategicSkillGenerator] Select a folder in the Project window first.");
                return;
            }

            string[] jsonPaths = Directory.GetFiles(
                folderPath,
                "*.json",
                SearchOption.TopDirectoryOnly);
            Array.Sort(jsonPaths, StringComparer.Ordinal);

            int generatedCount = 0;
            int failedCount = 0;
            int skippedCount = 0;

            for (int i = 0; i < jsonPaths.Length; i++)
            {
                string jsonPath = jsonPaths[i].Replace("\\", "/");
                try
                {
                    string json = File.ReadAllText(jsonPath);
                    StrategicSkillItemJson data =
                        JsonUtility.FromJson<StrategicSkillItemJson>(json);

                    if (data == null ||
                        string.IsNullOrWhiteSpace(data.strategicSkillItemId) ||
                        !data.strategicSkillItemId.StartsWith(
                            "item.strategic.",
                            StringComparison.Ordinal))
                    {
                        skippedCount++;
                        continue;
                    }

                    if (GenerateFromJsonPath(jsonPath))
                    {
                        generatedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }
                catch (Exception exception)
                {
                    failedCount++;
                    Debug.LogError(
                        $"[ItemStrategicSkillGenerator] Failed to generate item. " +
                        $"path={jsonPath}\n{exception}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[ItemStrategicSkillGenerator] Folder generation completed. " +
                $"folder={folderPath}, generated={generatedCount}, " +
                $"failed={failedCount}, skipped={skippedCount}");
        }

        [MenuItem("Assets/Item/Generate All Strategic Item SO From Folder", true)]
        private static bool ValidateGenerateAllFromSelectedFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrWhiteSpace(path) &&
                   AssetDatabase.IsValidFolder(path);
        }

        public static bool GenerateFromJsonPath(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            StrategicSkillItemJson data = JsonUtility.FromJson<StrategicSkillItemJson>(json);

            if (data == null)
            {
                Debug.LogError($"[ItemStrategicSkillGenerator] Failed to parse json. path={jsonPath}");
                return false;
            }

            Debug.Log($"[ItemStrategicSkillGenerator] Parsed: {data.strategicSkillItemId}");

            string folderPath = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                Debug.LogError("[ItemStrategicSkillGenerator] Invalid folder path.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(data.skillId))
            {
                Debug.LogError(
                    $"[ItemStrategicSkillGenerator] skillId is required. item={data.strategicSkillItemId}");
                return false;
            }

            if (!TryFindUniqueSkillById(data.skillId, out EquipmentSkillSO skillSo))
            {
                return false;
            }

            StrategicSkillItemSO itemSO = CreateOrUpdateItemSO(
                data,
                folderPath,
                skillSo);

            ItemStringBuilder.BuildResult stringBuildResult =
                ItemStringBuilder.BuildFromJsonPath(jsonPath);

            foreach (string error in stringBuildResult.errors)
            {
                Debug.LogError($"[ItemStrategicSkillGenerator] String build error: {error}");
            }

            foreach (string warning in stringBuildResult.warnings)
            {
                Debug.LogWarning($"[ItemStrategicSkillGenerator] String build warning: {warning}");
            }

            EditorUtility.SetDirty(itemSO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return true;
        }

        private static StrategicSkillItemSO CreateOrUpdateItemSO(
            StrategicSkillItemJson data,
            string folderPath,
            EquipmentSkillSO skillSo)
        {
            string assetPath = $"{folderPath}/{data.strategicSkillItemId}.asset";

            StrategicSkillItemSO itemSO =
                AssetDatabase.LoadAssetAtPath<StrategicSkillItemSO>(assetPath);

            if (itemSO == null)
            {
                itemSO = ScriptableObject.CreateInstance<StrategicSkillItemSO>();
                AssetDatabase.CreateAsset(itemSO, assetPath);
            }

            SetField(itemSO, "strategicSkillItemId", data.strategicSkillItemId);
            SetField(itemSO, "itemId", data.strategicSkillItemId);
            SetField(itemSO, "gaugeCost", data.gaugeCost);
            SetField(itemSO, "reusable", data.reusable);
            SetField(itemSO, "defaultPrice", data.defaultPrice);
            SetField(itemSO, "skillSo", skillSo);
            SetField(itemSO, "icon", FindSprite(data.icon));
            SetField(itemSO, "iconName", data.icon);
            SetField(itemSO, "grade", data.grade);
            SetField(
                itemSO,
                "tags",
                data.tags == null
                    ? Array.Empty<string>()
                    : data.tags.ToArray());

            return itemSO;
        }

        private static bool TryFindUniqueSkillById(
            string skillId,
            out EquipmentSkillSO resolved)
        {
            EquipmentSkillSO[] skills = Resources.LoadAll<EquipmentSkillSO>(string.Empty);
            resolved = null;

            for (int i = 0; i < skills.Length; i++)
            {
                EquipmentSkillSO skill = skills[i];
                if (skill != null && skill.EquipmentId == skillId)
                {
                    if (resolved != null)
                    {
                        Debug.LogError(
                            $"[ItemStrategicSkillGenerator] Duplicate EquipmentSkillSO ID in Resources. " +
                            $"skillId={skillId}");
                        return false;
                    }

                    resolved = skill;
                }
            }

            if (resolved != null)
            {
                return true;
            }

            Debug.LogError(
                $"[ItemStrategicSkillGenerator] EquipmentSkillSO not found in Resources. " +
                $"Generate the skill separately before the item. skillId={skillId}");
            return false;
        }

        private static Sprite FindSprite(string spriteName)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (directSprite != null && directSprite.name == spriteName)
                {
                    return directSprite;
                }

                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object asset in assets)
                {
                    if (asset is Sprite sprite && sprite.name == spriteName)
                    {
                        return sprite;
                    }
                }
            }

            string[] textureGuids = AssetDatabase.FindAssets(spriteName);
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object asset in assets)
                {
                    if (asset is Sprite sprite && sprite.name == spriteName)
                    {
                        return sprite;
                    }
                }
            }

            Debug.LogWarning($"[ItemStrategicSkillGenerator] Sprite not found. spriteName={spriteName}");
            return null;
        }

        private static void SetField(
            object target,
            string fieldName,
            object value)
        {
            if (target == null)
            {
                return;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        [Serializable]
        public class StrategicSkillItemJson
        {
            public string strategicSkillItemId;
            public string grade;

            public string nameKo;
            public string descriptionKo;

            public string icon;

            public int gaugeCost;
            public bool reusable;

            public string skillId;

            public int defaultPrice;

            public List<string> tags = new();
        }
    }
}
