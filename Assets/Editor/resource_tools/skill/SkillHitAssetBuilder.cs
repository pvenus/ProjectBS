using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Skill;

namespace ResourceTools.Skill
{
    [Serializable]
    public class HitJson
    {
        public string hitId;

        public int maxHitCount;
        public bool ignoreSameRoot;

        public bool useRepeatInterval;
        public float repeatInterval;

        public bool useHitWindow;
        public float hitStartTime;
        public float hitDuration;

        public bool deactivateAfterFirstHit;
        public string targetLayerMask;

        public DamageJson damage;
        public Effect.EffectJsonGenerator.HitEffectJson[] buffEffects;
        public Effect.EffectJsonGenerator.HitEffectJson[] debuffEffects;

        public bool useSplitMultiHitDamage;
        public int splitHitCount;
        public float splitHitInterval;

    }

    /// <summary>
    /// SkillHitSO 전용 에셋 빌더.
    /// JSON의 hit 데이터를 SkillHitSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillHitAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            HitJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillHitAssetBuilder] Hit json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillHitAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            SkillHitSO hitSo =
                AssetDatabase.LoadAssetAtPath<SkillHitSO>(assetPath);

            if (hitSo == null)
            {
                hitSo = ScriptableObject.CreateInstance<SkillHitSO>();
                AssetDatabase.CreateAsset(hitSo, assetPath);
            }

            ScriptableObject damageSo =
                HasDamage(json.damage)
                    ? SkillDamageAssetBuilder.CreateOrUpdate(json.damage, outputFolder)
                    : null;

            Apply(hitSo, json, damageSo, outputFolder);

            EditorUtility.SetDirty(hitSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillHitAssetBuilder] Updated SkillHitSO: {assetPath}");

            return hitSo;
        }

        private static bool HasDamage(
            DamageJson damage)
        {
            return damage != null &&
                   !string.IsNullOrWhiteSpace(damage.skillId);
        }

        private static string ResolveAssetName(HitJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.hitId))
            {
                return SanitizeFileName(json.hitId);
            }

            return "skill.hit";
        }

        private static void Apply(
            SkillHitSO hitSo,
            HitJson json,
            ScriptableObject damageSo,
            string outputFolder)
        {
            SerializedObject serializedObject = new SerializedObject(hitSo);

            SetString(serializedObject, "hitId", json.hitId);
            SetInt(serializedObject, "maxHitCount", json.maxHitCount);
            SetBool(serializedObject, "ignoreSameRoot", json.ignoreSameRoot);
            SetBool(serializedObject, "useRepeatInterval", json.useRepeatInterval);
            SetFloat(serializedObject, "repeatInterval", json.repeatInterval);
            SetBool(serializedObject, "useHitWindow", json.useHitWindow);
            SetFloat(serializedObject, "hitStartTime", json.hitStartTime);
            SetFloat(serializedObject, "hitDuration", json.hitDuration);
            SetBool(serializedObject, "deactivateAfterFirstHit", json.deactivateAfterFirstHit);
            SetLayerMask(serializedObject, "targetLayerMask", ToLayerMask(json.targetLayerMask));
            SetString(serializedObject, "targetLayerName", json.targetLayerMask);
            SetObjectReference(serializedObject, "damageSo", damageSo);
            SetHitEffectEntries(serializedObject, "buffEffects", CreateHitEffectEntries(json.buffEffects, outputFolder));
            SetHitEffectEntries(serializedObject, "debuffEffects", CreateHitEffectEntries(json.debuffEffects, outputFolder));
            SetBool(serializedObject, "useSplitMultiHitDamage", json.useSplitMultiHitDamage);
            SetInt(serializedObject, "splitHitCount", json.splitHitCount);
            SetFloat(serializedObject, "splitHitInterval", json.splitHitInterval);

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
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = value;
                return;
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                SetEnum(property, value, propertyName);
            }
        }

        private static void SetEnum(
            SerializedProperty property,
            string value,
            string propertyName)
        {
            if (property == null || string.IsNullOrWhiteSpace(value))
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

            Debug.LogWarning($"[SkillHitAssetBuilder] Enum value not found. property={propertyName} value={value}");
        }

        private static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
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
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            property.floatValue = value;
        }

        private static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            property.boolValue = value;
        }

        private static void SetLayerMask(
            SerializedObject serializedObject,
            string propertyName,
            LayerMask value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            property.intValue = value.value;
        }

        private static void SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetHitEffectEntries(
            SerializedObject serializedObject,
            string propertyName,
            SkillProjectileHitEffectEntry[] values)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            if (!property.isArray)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Serialized property is not array: {propertyName}");
                return;
            }

            int length = values != null ? values.Length : 0;
            property.arraySize = length;

            for (int i = 0; i < length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                if (element.propertyType == SerializedPropertyType.ManagedReference)
                {
                    element.managedReferenceValue = values[i];
                }
                else if (element.propertyType == SerializedPropertyType.Generic)
                {
                    CopyHitEffectEntry(element, values[i]);
                }
            }
        }

        private static void CopyHitEffectEntry(
            SerializedProperty element,
            SkillProjectileHitEffectEntry value)
        {
            if (value == null)
            {
                return;
            }

            SerializedProperty effectProperty = element.FindPropertyRelative("effectSo");
            if (effectProperty == null)
            {
                effectProperty = element.FindPropertyRelative("effectSO");
            }

            if (effectProperty != null)
            {
                effectProperty.objectReferenceValue = value.effectSo;
            }

            SerializedProperty lifetimeTypeProperty = element.FindPropertyRelative("lifetimeType");
            if (lifetimeTypeProperty != null)
            {
                SetEnum(lifetimeTypeProperty, value.lifetimeType.ToString(), "lifetimeType");
            }

            SerializedProperty categoryTypeProperty = element.FindPropertyRelative("categoryType");
            if (categoryTypeProperty != null)
            {
                SetEnum(categoryTypeProperty, value.categoryType.ToString(), "categoryType");
            }

            SerializedProperty durationProperty = element.FindPropertyRelative("duration");
            if (durationProperty != null)
            {
                durationProperty.floatValue = value.duration;
            }

            SerializedProperty maxApplyCountProperty = element.FindPropertyRelative("maxApplyCount");
            if (maxApplyCountProperty != null)
            {
                maxApplyCountProperty.intValue = value.maxApplyCount;
            }
        }

        private static SkillProjectileHitEffectEntry[] CreateHitEffectEntries(
            Effect.EffectJsonGenerator.HitEffectJson[] data,
            string outputFolder)
        {
            return Effect.EffectJsonGenerator.CreateOrUpdateHitEffectEntries(
                data,
                outputFolder);
        }


        private static LayerMask ToLayerMask(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                return default;
            }

            int layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
            {
                Debug.LogWarning($"[SkillHitAssetBuilder] Layer not found: {layerName}");
                return default;
            }

            return 1 << layer;
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
                Debug.LogError($"[SkillHitAssetBuilder] Folder path must start with Assets: {folderPath}");
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
                return "skill.hit";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}