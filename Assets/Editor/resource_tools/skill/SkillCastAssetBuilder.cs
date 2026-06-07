using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Skill;

namespace ResourceTools.Skill
{
    [Serializable]
    public class CastJson
    {
        public string castId;
        public string targetingType;
        public string targetMaskLayer;
        public string castType;

        public float castDelay;
        public float castDuration;
        public float cooldown;
        public float range;
        public float castTime;
        public int burstCount = 1;
        public float burstInterval;

        public float radius;
        public float angle;

        public bool requiresTarget;
        public bool canCastWhileMoving;
        public bool rotateToTarget;
        public bool useOwnerDirection;
        public bool useMousePosition;

        public CastMoveJson castMove;
        public Effect.EffectJsonGenerator.HitEffectJson[] selfEffects;
    }

    [Serializable]
    public class CastMoveJson
    {
        public string castMoveId;
        public string moveType;
        public float distance;
        public float duration;
        public bool stopOnWall = true;
        public bool ignoreDuringStun;
    }

    /// <summary>
    /// SkillCastSO 전용 에셋 빌더.
    /// JSON의 cast 데이터를 SkillCastSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillCastAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            CastJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillCastAssetBuilder] Cast json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillCastAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            SkillCastSO castSo =
                AssetDatabase.LoadAssetAtPath<SkillCastSO>(assetPath);

            if (castSo == null)
            {
                castSo = ScriptableObject.CreateInstance<SkillCastSO>();
                AssetDatabase.CreateAsset(castSo, assetPath);
            }

            Apply(castSo, json, outputFolder);

            EditorUtility.SetDirty(castSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillCastAssetBuilder] Updated SkillCastSO: {assetPath}");

            return castSo;
        }

        private static string ResolveAssetName(CastJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.castId))
            {
                return SanitizeFileName(json.castId);
            }

            return "skill.cast";
        }

        private static void Apply(
            SkillCastSO castSo,
            CastJson json,
            string outputFolder)
        {
            SkillCastMoveSO castMoveSo = CreateOrUpdateCastMove(
                json.castMove,
                outputFolder);

            SerializedObject serializedObject = new SerializedObject(castSo);

            SetString(serializedObject, "castId", json.castId);
            SetString(serializedObject, "castType", json.castType);
            SetString(serializedObject, "targetingType", json.targetingType);
            SetLayerMask(serializedObject, "targetMask", ToLayerMask(json.targetMaskLayer));
            SetString(serializedObject, "targetMaskLayer", json.targetMaskLayer);
            SetFloat(serializedObject, "castDelay", json.castDelay);
            SetFloat(serializedObject, "castDuration", json.castDuration);
            SetFloat(serializedObject, "castTime", json.castTime);
            SetFloat(serializedObject, "cooldown", json.cooldown);
            SetFloat(serializedObject, "range", json.range);
            SetFloat(serializedObject, "radius", json.radius);
            SetFloat(serializedObject, "angle", json.angle);
            SetInt(serializedObject, "burstCount", json.burstCount);
            SetFloat(serializedObject, "burstInterval", json.burstInterval);
            SetObjectReference(serializedObject, "castMove", castMoveSo);
            SetHitEffectEntries(
                serializedObject,
                "selfEffects",
                ResourceTools.Effect.EffectJsonGenerator.CreateOrUpdateHitEffectEntries(
                    json.selfEffects,
                    outputFolder));
            SetBool(serializedObject, "requiresTarget", json.requiresTarget);
            SetBool(serializedObject, "canCastWhileMoving", json.canCastWhileMoving);
            SetBool(serializedObject, "canMoveWhileCasting", json.canCastWhileMoving);
            SetBool(serializedObject, "canCancelCasting", json.canCastWhileMoving);
            SetBool(serializedObject, "rotateToTarget", json.rotateToTarget);
            SetBool(serializedObject, "useOwnerDirection", json.useOwnerDirection);
            SetBool(serializedObject, "useMousePosition", json.useMousePosition);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SkillCastMoveSO CreateOrUpdateCastMove(
            CastMoveJson json,
            string outputFolder)
        {
            if (json == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(json.castMoveId))
            {
                Debug.LogWarning("[SkillCastAssetBuilder] castMoveId is empty. SkillCastMoveSO will not be created.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetPath = Path.Combine(
                    outputFolder,
                    SanitizeFileName(json.castMoveId) + ".asset")
                .Replace("\\", "/");

            SkillCastMoveSO castMoveSo =
                AssetDatabase.LoadAssetAtPath<SkillCastMoveSO>(assetPath);

            if (castMoveSo == null)
            {
                castMoveSo = ScriptableObject.CreateInstance<SkillCastMoveSO>();
                AssetDatabase.CreateAsset(castMoveSo, assetPath);
            }

            SerializedObject serializedObject = new SerializedObject(castMoveSo);

            SetString(serializedObject, "castMoveId", json.castMoveId);
            SetString(serializedObject, "moveType", json.moveType);
            SetFloat(serializedObject, "distance", json.distance);
            SetFloat(serializedObject, "duration", json.duration);
            SetBool(serializedObject, "stopOnWall", json.stopOnWall);
            SetBool(serializedObject, "ignoreDuringStun", json.ignoreDuringStun);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(castMoveSo);
            AssetDatabase.SaveAssetIfDirty(castMoveSo);

            return castMoveSo;
        }

        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
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

            Debug.LogWarning($"[SkillCastAssetBuilder] Enum value not found. property={propertyName} value={value}");
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.floatValue = value;
        }

        private static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                return;
            }

            property.intValue = value;
        }

        private static void SetBool(
            SerializedObject serializedObject,
            string propertyName,
            bool value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
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
                Debug.LogWarning($"[SkillCastAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            if (!property.isArray)
            {
                Debug.LogWarning($"[SkillCastAssetBuilder] Serialized property is not array: {propertyName}");
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

        private static LayerMask ToLayerMask(string layerName)
        {
            if (string.IsNullOrWhiteSpace(layerName))
            {
                return default;
            }

            int layer = LayerMask.NameToLayer(layerName);

            if (layer < 0)
            {
                Debug.LogWarning($"[SkillCastAssetBuilder] Layer not found: {layerName}");
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
                Debug.LogError($"[SkillCastAssetBuilder] Folder path must start with Assets: {folderPath}");
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
                return "skill.cast";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}
