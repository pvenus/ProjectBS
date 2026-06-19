using System;
using System.IO;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    [Serializable]
    public class BaseProfileJson
    {
        public string profileId;
        public string skillType;
        public string effectType;

        public string attackArchetype;
        public bool skipAttackAnimation;

        public float projectileSpawnOffset;

        public int projectileCount;
        public string projectileArrangement;
        public float projectileArrangementValue;
        public float projectileSpreadAngle;
        public float projectileScale;
        public float projectileColliderRadius;
        public float projectileLifetime;

        public float projectileSpawnInterval;
        public float projectileSpawnRadius;

        public string category;
        public string targetType;
        public string tacticalNeed;
        public float basePriority;
    }

    /// <summary>
    /// EquipmentSkillJsonGenerator에서 분리된 BaseProfile 전용 에셋 빌더.
    /// JSON의 baseProfile 데이터를 EquipmentBaseProfileSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class EquipmentBaseProfileAssetBuilder
    {
        public static EquipmentBaseProfileSO CreateOrUpdate(
            BaseProfileJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[EquipmentBaseProfileAssetBuilder] BaseProfile json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[EquipmentBaseProfileAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            EquipmentBaseProfileSO profile =
                AssetDatabase.LoadAssetAtPath<EquipmentBaseProfileSO>(assetPath);

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EquipmentBaseProfileSO>();
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            Apply(profile, json);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[EquipmentBaseProfileAssetBuilder] Updated EquipmentBaseProfileSO: {assetPath}");

            return profile;
        }

        private static string ResolveAssetName(
            BaseProfileJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.profileId))
            {
                return SanitizeFileName(json.profileId);
            }

            return "equipment.base.profile";
        }

        private static void Apply(
            EquipmentBaseProfileSO profile,
            BaseProfileJson json)
        {
            SerializedObject serializedObject = new SerializedObject(profile);

            SetString(serializedObject, "skillType", json.skillType);
            SetString(serializedObject, "effectType", json.effectType);
            SetString(serializedObject, "attackArchetype", json.attackArchetype);
            SetBool(serializedObject, "skipAttackAnimation", json.skipAttackAnimation);


            SetFloat(serializedObject, "projectileSpawnOffset", json.projectileSpawnOffset);
            SetInt(serializedObject, "projectileCount", json.projectileCount);
            SetString(serializedObject, "projectileArrangement", json.projectileArrangement);
            SetFloat(serializedObject, "projectileArrangementValue", json.projectileArrangementValue);
            SetFloat(serializedObject, "projectileSpreadAngle", json.projectileSpreadAngle);
            SetFloat(serializedObject, "projectileScale", json.projectileScale);
            SetFloat(serializedObject, "projectileColliderRadius", json.projectileColliderRadius);
            SetFloat(serializedObject, "projectileLifetime", json.projectileLifetime);
            SetFloat(serializedObject, "projectileSpawnInterval", json.projectileSpawnInterval);
            SetFloat(serializedObject, "projectileSpawnRadius", json.projectileSpawnRadius);

            SetString(serializedObject, "category", json.category);
            SetString(serializedObject, "targetType", json.targetType);
            SetString(serializedObject, "tacticalNeed", json.tacticalNeed);
            SetFloat(serializedObject, "basePriority", json.basePriority);

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
                throw new InvalidOperationException(
                    $"[EquipmentBaseProfileAssetBuilder] Serialized property not found: {propertyName}");
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = value;
                return;
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                SetEnum(property, value, propertyName);
                return;
            }

            throw new InvalidOperationException(
                $"[EquipmentBaseProfileAssetBuilder] Property is not string or enum: {propertyName} type={property.propertyType}");
        }

        private static void SetEnum(
            SerializedProperty property,
            string value,
            string propertyName)
        {
            if (property == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            for (int i = 0; i < property.enumNames.Length; i++)
            {
                if (string.Equals(property.enumNames[i], value, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.enumDisplayNames[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    property.enumValueIndex = i;
                    return;
                }
            }

            throw new InvalidOperationException(
                $"[EquipmentBaseProfileAssetBuilder] Enum value not found. property={propertyName} value={value}");
        }

        private static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[EquipmentBaseProfileAssetBuilder] Serialized property not found: {propertyName}");
            }

            property.boolValue = value;
        }

        private static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[EquipmentBaseProfileAssetBuilder] Serialized property not found: {propertyName}");
            }

            property.intValue = value;
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[EquipmentBaseProfileAssetBuilder] Serialized property not found: {propertyName}");
            }

            property.floatValue = value;
        }



        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return;
            }

            folderPath = folderPath.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');

            if (parts.Length == 0 || parts[0] != "Assets")
            {
                Debug.LogError($"[EquipmentBaseProfileAssetBuilder] Folder path must start with Assets: {folderPath}");
                return;
            }

            string currentPath = "Assets";

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = currentPath + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "equipment.base.profile";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}