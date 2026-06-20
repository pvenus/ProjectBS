using System;
using System.IO;
using UnityEditor;
using UnityEngine;

using Skill;
using Skills.Move.Config;

namespace ResourceTools.Skill
{
    [Serializable]
    public class MoveJson
    {
        public string moveId;
        public string moveType;
        public string movementType;

        public LinearMoveJson linear;
        public HoverMoveJson hover;
        public WarpMoveJson warp;

        public float speed;
        public float acceleration;
        public float turnSpeed;
        public float duration;

        public float arrivalThreshold;
        public bool applyDirectionRotation;
        public float rotationOffset;

        public float followOffsetX;
        public float followOffsetY;
        public float followLerpSpeed;
        public bool snapOnInitialize;

        public bool useHoverMotion;
        public float hoverAmplitude;
        public float hoverFrequency;
        public float hoverAxisX;
        public float hoverAxisY;

        public bool endWhenOwnerMissing;

        public float orbitRadius;
        public float orbitAngularSpeed;
        public bool clockwise;

        public int spawnOrder;
        public int maxProjectileCount;
        public bool resetPhaseWhenLayoutChanges;
        public bool useRadialPulse;
        public float radialPulseAmplitude;
        public float radialPulseFrequency;

        public bool useTarget;
        public bool useOwnerDirection;
        public bool rotateToMovement;
    }

    [Serializable]
    public class LinearMoveJson
    {
        public float speed;
    }

    [Serializable]
    public class HoverMoveJson
    {
        public float followOffsetX;
        public float followOffsetY;
    }

    [Serializable]
    public class WarpMoveJson
    {
    }

    public static class SkillMoveAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            MoveJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillMoveAssetBuilder] Move json is null.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetName = string.IsNullOrWhiteSpace(json.moveId)
                ? "skill.move"
                : SanitizeFileName(json.moveId);

            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            SkillMoveSO moveSo =
                AssetDatabase.LoadAssetAtPath<SkillMoveSO>(assetPath);

            if (moveSo == null)
            {
                moveSo = ScriptableObject.CreateInstance<SkillMoveSO>();
                AssetDatabase.CreateAsset(moveSo, assetPath);
            }

            Apply(moveSo, json);

            EditorUtility.SetDirty(moveSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillMoveAssetBuilder] Updated SkillMoveSO: {assetPath}");

            return moveSo;
        }

        private static void Apply(
            SkillMoveSO moveSo,
            MoveJson json)
        {
            SerializedObject serializedObject = new SerializedObject(moveSo);

            SkillMoveConfig config = ApplyConfig(serializedObject, json);
            SetMoveType(serializedObject, config != null ? config.MoveType.ToString() : ResolveMoveType(json));

            SetBool(serializedObject, "applyDirectionRotation", json.applyDirectionRotation);
            SetFloat(serializedObject, "rotationOffset", json.rotationOffset);

            SetFloat(serializedObject, "followOffsetX", json.followOffsetX);
            SetFloat(serializedObject, "followOffsetY", json.followOffsetY);
            SetFloat(serializedObject, "followLerpSpeed", json.followLerpSpeed);
            SetBool(serializedObject, "snapOnInitialize", json.snapOnInitialize);

            SetBool(serializedObject, "useHoverMotion", json.useHoverMotion);
            SetFloat(serializedObject, "hoverAmplitude", json.hoverAmplitude);
            SetFloat(serializedObject, "hoverFrequency", json.hoverFrequency);
            SetFloat(serializedObject, "hoverAxisX", json.hoverAxisX);
            SetFloat(serializedObject, "hoverAxisY", json.hoverAxisY);

            SetBool(serializedObject, "endWhenOwnerMissing", json.endWhenOwnerMissing);

            SetFloat(serializedObject, "orbitRadius", json.orbitRadius);
            SetFloat(serializedObject, "orbitAngularSpeed", json.orbitAngularSpeed);
            SetBool(serializedObject, "clockwise", json.clockwise);

            SetInt(serializedObject, "spawnOrder", json.spawnOrder);
            SetInt(serializedObject, "maxProjectileCount", json.maxProjectileCount);
            SetBool(serializedObject, "resetPhaseWhenLayoutChanges", json.resetPhaseWhenLayoutChanges);
            SetBool(serializedObject, "useRadialPulse", json.useRadialPulse);
            SetFloat(serializedObject, "radialPulseAmplitude", json.radialPulseAmplitude);
            SetFloat(serializedObject, "radialPulseFrequency", json.radialPulseFrequency);

            SetBool(serializedObject, "useTarget", json.useTarget);
            SetBool(serializedObject, "useOwnerDirection", json.useOwnerDirection);
            SetBool(serializedObject, "rotateToMovement", json.rotateToMovement);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SkillMoveConfig ApplyConfig(
            SerializedObject serializedObject,
            MoveJson json)
        {
            SerializedProperty configProperty = serializedObject.FindProperty("config");

            if (configProperty == null)
            {
                Debug.LogWarning("[SkillMoveAssetBuilder] Serialized property not found: config");
                return null;
            }

            string moveType = ResolveMoveType(json);

            if (string.Equals(moveType, "Linear", StringComparison.OrdinalIgnoreCase) && json?.linear != null)
            {
                LinearMoveConfig config = configProperty.managedReferenceValue as LinearMoveConfig;
                if (config == null)
                {
                    config = new LinearMoveConfig();
                }

                config.speed = json.linear.speed;
                configProperty.managedReferenceValue = config;
                return config;
            }

            if (string.Equals(moveType, "Hover", StringComparison.OrdinalIgnoreCase) && json?.hover != null)
            {
                HoverMoveConfig config = configProperty.managedReferenceValue as HoverMoveConfig;
                if (config == null)
                {
                    config = new HoverMoveConfig();
                }

                config.followOffset = new Vector2(
                    json.hover.followOffsetX,
                    json.hover.followOffsetY);
                configProperty.managedReferenceValue = config;
                return config;
            }

            if (string.Equals(moveType, "Warp", StringComparison.OrdinalIgnoreCase))
            {
                WarpMoveConfig config = configProperty.managedReferenceValue as WarpMoveConfig;
                if (config == null)
                {
                    config = new WarpMoveConfig();
                }

                configProperty.managedReferenceValue = config;
                return config;
            }

            configProperty.managedReferenceValue = null;
            return null;
        }

        private static string ResolveMoveType(MoveJson json)
        {
            if (json == null)
            {
                return null;
            }

            return !string.IsNullOrWhiteSpace(json.moveType)
                ? json.moveType
                : json.movementType;
        }

        private static void SetMoveType(
            SerializedObject serializedObject,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty("moveType");

            if (property == null)
            {
                Debug.LogWarning("[SkillMoveAssetBuilder] Serialized property not found: moveType");
                return;
            }

            if (property.propertyType == SerializedPropertyType.Enum)
            {
                SetEnum(property, value, "moveType");
                return;
            }

            if (property.propertyType == SerializedPropertyType.String)
            {
                property.stringValue = value;
                return;
            }

            Debug.LogWarning($"[SkillMoveAssetBuilder] moveType property is not enum or string. type={property.propertyType}");
        }

        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillMoveAssetBuilder] Serialized property not found: {propertyName}");
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
                return;
            }

            Debug.LogWarning($"[SkillMoveAssetBuilder] Property is not string or enum: {propertyName} type={property.propertyType}");
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

            Debug.LogWarning($"[SkillMoveAssetBuilder] Enum value not found. property={propertyName} value={value}");
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[SkillMoveAssetBuilder] Serialized property not found: {propertyName}");
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
                Debug.LogWarning($"[SkillMoveAssetBuilder] Serialized property not found: {propertyName}");
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
                Debug.LogWarning($"[SkillMoveAssetBuilder] Serialized property not found: {propertyName}");
                return;
            }

            property.boolValue = value;
        }


        private static void EnsureFolder(string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string current = "Assets";
            string[] parts = folderPath.Split('/');

            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value;
        }
    }
}