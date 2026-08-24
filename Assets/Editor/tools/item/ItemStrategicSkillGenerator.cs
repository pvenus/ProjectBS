using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Item;
using Skill;
using ResourceTools.Helper;

namespace ResourceTools
{
    public static class ItemStrategicSkillGenerator
    {
        private const string ContentItemSoFolder =
            "Assets/Contents/Item/so";

        private const string ContentSkillSoFolder =
            "Assets/Contents/Skill/so";

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
                skillSo);
            if (itemSO == null)
            {
                return false;
            }

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
            EquipmentSkillSO skillSo)
        {
            string iconId = $"{data.skillId}.icon";
            Sprite icon = SpriteHelper.FindSpriteByName(iconId);
            if (icon == null)
            {
                Debug.LogError(
                    $"[ItemStrategicSkillGenerator] Icon Sprite not found. " +
                    $"itemId={data.strategicSkillItemId}, iconId={iconId}");
                return null;
            }

            EnsureFolder(ContentItemSoFolder);
            string assetPath =
                $"{ContentItemSoFolder}/{data.strategicSkillItemId}.asset";

            StrategicSkillItemSO itemSO =
                AssetDatabase.LoadAssetAtPath<StrategicSkillItemSO>(assetPath);

            if (itemSO == null)
            {
                itemSO = ScriptableObject.CreateInstance<StrategicSkillItemSO>();
                AssetDatabase.CreateAsset(itemSO, assetPath);
            }

            itemSO.strategicSkillItemId = data.strategicSkillItemId;
            itemSO.icon = icon;
            itemSO.gaugeCost = data.gaugeCost;
            itemSO.reusable = data.reusable;
            itemSO.skillSo = skillSo;
            itemSO.defaultPrice = data.defaultPrice;
            itemSO.tags = data.tags == null
                ? Array.Empty<string>()
                : data.tags.ToArray();

            return itemSO;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static bool TryFindUniqueSkillById(
            string skillId,
            out EquipmentSkillSO resolved)
        {
            resolved = null;

            string[] contentGuids = AssetDatabase.FindAssets(
                "t:EquipmentSkillSO",
                new[] { ContentSkillSoFolder });

            for (int i = 0; i < contentGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(contentGuids[i]);
                EquipmentSkillSO skill =
                    AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(path);

                if (skill == null || skill.EquipmentId != skillId)
                {
                    continue;
                }

                if (resolved != null)
                {
                    Debug.LogError(
                        $"[ItemStrategicSkillGenerator] Duplicate EquipmentSkillSO ID in content folder. " +
                        $"skillId={skillId}, folder={ContentSkillSoFolder}");
                    return false;
                }

                resolved = skill;
            }

            if (resolved != null)
            {
                return true;
            }

            string skillJsonPath =
                $"Assets/Contents/Skill/json/{skillId}.json";
            if (File.Exists(skillJsonPath))
            {
                Debug.Log(
                    $"[ItemStrategicSkillGenerator] EquipmentSkillSO is missing; " +
                    $"generating from json. skillId={skillId}, path={skillJsonPath}");

                EquipmentSkillSO generated =
                    ResourceTools.Skill.EquipmentSkillJsonGenerator
                        .GenerateFromJsonPath(skillJsonPath);

                if (generated != null && generated.EquipmentId == skillId)
                {
                    resolved = generated;
                    return true;
                }

                Debug.LogWarning(
                    $"[ItemStrategicSkillGenerator] Failed to auto-generate EquipmentSkillSO. " +
                    $"skillId={skillId}, path={skillJsonPath}");
            }

            // Compatibility fallback for skills that have not migrated to
            // Assets/Contents/Skill/so yet.
            EquipmentSkillSO[] skills =
                Resources.LoadAll<EquipmentSkillSO>(string.Empty);

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
                $"[ItemStrategicSkillGenerator] EquipmentSkillSO not found. " +
                $"Content json was also unavailable or generation failed. " +
                $"skillId={skillId}, expectedJson={skillJsonPath}");
            return false;
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
