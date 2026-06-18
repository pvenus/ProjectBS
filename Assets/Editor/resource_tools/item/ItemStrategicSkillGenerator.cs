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
        [MenuItem("Assets/Item/Strategic Skill Generator", false, 2000)]
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

        public static void GenerateFromJsonPath(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            StrategicSkillItemJson data = JsonUtility.FromJson<StrategicSkillItemJson>(json);

            if (data == null)
            {
                Debug.LogError($"[ItemStrategicSkillGenerator] Failed to parse json. path={jsonPath}");
                return;
            }

            Debug.Log($"[ItemStrategicSkillGenerator] Parsed: {data.strategicSkillItemId}");

            string folderPath = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                Debug.LogError("[ItemStrategicSkillGenerator] Invalid folder path.");
                return;
            }

            EquipmentSkillSO skillSO = null;
            if (data.skill != null)
            {
                skillSO = Skill.EquipmentSkillJsonGenerator.CreateOrUpdateSkill(
                    data.skill,
                    folderPath);
            }

            StrategicSkillItemSO itemSO = CreateOrUpdateItemSO(
                data,
                skillSO,
                folderPath);

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
        }

        private static StrategicSkillItemSO CreateOrUpdateItemSO(
            StrategicSkillItemJson data,
            EquipmentSkillSO skillSO,
            string folderPath)
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
            SetField(itemSO, "skillId", data.skillId);
            SetField(itemSO, "skillSO", skillSO);
            SetField(itemSO, "skillSo", skillSO);
            SetField(itemSO, "skill", skillSO);
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

            public Skill.EquipmentSkillJsonGenerator.EquipmentSkillJson skill;
        }
    }
}