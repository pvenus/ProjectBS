#if UNITY_EDITOR
using System;
using System.IO;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    public static class EquipmentSkillJsonGenerator
    {
        // New JSON model (2024-06):
        [Serializable]
        public class EquipmentSkillJson
        {
            public string equipmentId;
            public string slotName;
            public string skillName;
            public string iconName;

            public BaseProfileJson baseProfile;
            public CastJson cast;
            public MoveJson move;
            public HitJson[] hits;
            public SpawnSkillJson spawnSkill;
            public SpawnSkillJson spawn;
            public VisualSetJson visualSet;
        }


        // Example JSON format:
        // {
        //   "equipmentId": "basic_attack",
        //   "characterName": "military_officer",
        //   "slotName": "basic",
        //   "skillName": "basic_attack",
        //   "iconName": "basic_attack"
        // }
        // Usage:
        // Select a json file in the Project window.
        // Right Click -> Skill -> Generate EquipmentSkillSO From Json
        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", false, 2000)]
        public static void Generate()
        {
            TextAsset jsonAsset = Selection.activeObject as TextAsset;

            if (jsonAsset == null)
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Select a json file in the Project window first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(jsonAsset);
            GenerateFromJsonPath(jsonPath);
        }

        public static EquipmentSkillSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Selected asset is not a json file.");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            EquipmentSkillJson data = JsonUtility.FromJson<EquipmentSkillJson>(json);

            if (data?.visualSet != null)
            {
                Debug.Log($"[EquipmentSkillJsonGenerator] VisualSet detected: {data.visualSet.visualSetId}");
            }

            if (data == null || string.IsNullOrEmpty(data.equipmentId))
            {
                Debug.LogError($"[EquipmentSkillJsonGenerator] Invalid EquipmentSkill json: {jsonPath}");
                return null;
            }

            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Cannot resolve output folder from json path.");
                return null;
            }

            return CreateOrUpdateSkill(data, outputFolder);
        }

        public static EquipmentSkillSO CreateOrUpdateSkill(
            EquipmentSkillJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrEmpty(data.equipmentId))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Invalid EquipmentSkill json data.");
                return null;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] outputFolder is empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetPath = $"{outputFolder}/{data.equipmentId}.asset";
            EquipmentSkillSO skillSo = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(assetPath);
            bool isNewAsset = false;

            EquipmentBaseProfileSO baseProfileSo =
                HasBaseProfile(data.baseProfile)
                    ? EquipmentBaseProfileAssetBuilder.CreateOrUpdate(
                        data.baseProfile,
                        outputFolder)
                    : null;

            SkillCastSO castSo =
                HasCast(data.cast)
                    ? SkillCastAssetBuilder.CreateOrUpdate(
                        data.cast,
                        outputFolder) as SkillCastSO
                    : null;

            SkillMoveSO moveSo =
                HasMove(data.move)
                    ? SkillMoveAssetBuilder.CreateOrUpdate(
                        data.move,
                        outputFolder) as SkillMoveSO
                    : null;

            SkillHitSO[] hitSos =
                HasHits(data.hits)
                    ? CreateOrUpdateHits(
                        data.hits,
                        outputFolder)
                    : Array.Empty<SkillHitSO>();

            // Support both spawnSkill and spawn for spawn skill JSON
            SpawnSkillJson resolvedSpawnSkill = data.spawnSkill ?? data.spawn;

            SpawnSkillSO spawnSkillSo =
                SkillSpawnAssetBuilder.HasSpawnSkill(resolvedSpawnSkill)
                    ? SkillSpawnAssetBuilder.CreateOrUpdate(
                        resolvedSpawnSkill,
                        outputFolder,
                        CreateOrUpdateSkill)
                    : null;

            ScriptableObject visualSetSo =
                HasVisualSet(data.visualSet)
                    ? SkillVisualSetAssetBuilder.CreateOrUpdate(
                        data.visualSet,
                        outputFolder)
                    : null;

            if (skillSo == null)
            {
                skillSo = ScriptableObject.CreateInstance<EquipmentSkillSO>();
                isNewAsset = true;
            }

            ApplySkillFields(
                skillSo,
                data,
                baseProfileSo,
                castSo,
                moveSo,
                hitSos,
                spawnSkillSo,
                visualSetSo);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(skillSo, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created EquipmentSkillSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(skillSo);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated EquipmentSkillSO: {assetPath}");
            }

            AssetDatabase.SaveAssetIfDirty(skillSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return skillSo;
        }

        private static bool HasBaseProfile(
            BaseProfileJson baseProfile)
        {
            return baseProfile != null &&
                   !string.IsNullOrWhiteSpace(baseProfile.profileId);
        }

        private static bool HasCast(
            CastJson cast)
        {
            return cast != null &&
                   !string.IsNullOrWhiteSpace(cast.castId);
        }

        private static bool HasMove(
            MoveJson move)
        {
            return move != null &&
                   !string.IsNullOrWhiteSpace(move.moveId);
        }

        private static bool HasVisualSet(
            VisualSetJson visualSet)
        {
            return visualSet != null &&
                   !string.IsNullOrWhiteSpace(visualSet.visualSetId);
        }


        private static bool HasHits(
            HitJson[] hits)
        {
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null &&
                    !string.IsNullOrWhiteSpace(hits[i].hitId))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplySkillFields(
            EquipmentSkillSO skillSo,
            EquipmentSkillJson data,
            EquipmentBaseProfileSO baseProfileSo,
            SkillCastSO castSo,
            SkillMoveSO moveSo,
            SkillHitSO[] hitSos,
            SpawnSkillSO spawnSkillSo,
            ScriptableObject visualSetSo)
        {
            SerializedObject serializedObject = new SerializedObject(skillSo);

            SetString(serializedObject, "equipmentId", data.equipmentId);
            SetObjectReference(serializedObject, "icon", FindSpriteById(data.iconName));
            SetObjectReference(serializedObject, "baseProfileSo", baseProfileSo);
            SetObjectReference(serializedObject, "castSo", castSo);
            SetObjectReference(serializedObject, "moveSo", moveSo);
            SetObjectArray(serializedObject, "hitSos", hitSos);
            SetObjectReference(serializedObject, "spawnSkillSo", spawnSkillSo);
            SetObjectReference(serializedObject, "visualSetSo", visualSetSo);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }


        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            property.stringValue = value;
        }


        private static void SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetObjectArray<T>(
            SerializedObject serializedObject,
            string propertyName,
            T[] values)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            if (!property.isArray)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property is not array: {propertyName}");
                return;
            }

            int length = values != null ? values.Length : 0;
            property.arraySize = length;

            for (int i = 0; i < length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }


        private static SkillHitSO[] CreateOrUpdateHits(
            HitJson[] hits,
            string outputFolder)
        {
            if (hits == null || hits.Length == 0)
            {
                return Array.Empty<SkillHitSO>();
            }

            int validCount = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null &&
                    !string.IsNullOrWhiteSpace(hits[i].hitId))
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                return Array.Empty<SkillHitSO>();
            }

            SkillHitSO[] result = new SkillHitSO[validCount];
            int resultIndex = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] == null ||
                    string.IsNullOrWhiteSpace(hits[i].hitId))
                {
                    continue;
                }

                result[resultIndex] = SkillHitAssetBuilder.CreateOrUpdate(
                    hits[i],
                    outputFolder) as SkillHitSO;
                resultIndex++;
            }

            return result;
        }

        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", true)]
        private static bool ValidateGenerate()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        private static Sprite FindSpriteById(string iconId)
        {
            if (string.IsNullOrEmpty(iconId))
            {
                return null;
            }

            Debug.Log($"[EquipmentSkillJsonGenerator] Find icon sprite: {iconId}");
            string[] guids = AssetDatabase.FindAssets($"{iconId} t:Sprite");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null && sprite.name.Equals(iconId, StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            Debug.LogWarning($"[EquipmentSkillJsonGenerator] Icon sprite not found: {iconId}");
            return null;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            folder = folder.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            string leaf = Path.GetFileName(folder);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
#endif