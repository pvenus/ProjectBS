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

        public bool applyDirectionRotation;
        public float rotationOffset;

        public LinearMoveJson linear;
        public HoverMoveJson hover;
        public WarpMoveJson warp;
        public HomingMoveJson homing;
        public OrbitMoveJson orbit;
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

    [Serializable]
    public class HomingMoveJson
    {
        public float speed;
        public float turnSpeed;
    }

    [Serializable]
    public class OrbitMoveJson
    {
        public float orbitRadius;
        public float orbitAngularSpeed;
        public bool clockwise;

        public int spawnOrder;
        public int maxProjectileCount;
        public bool resetPhaseWhenLayoutChanges;

        public float radialPulseAmplitude;
        public float radialPulseFrequency;
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

            if (string.Equals(moveType, "Homing", StringComparison.OrdinalIgnoreCase))
            {
                HomingMoveConfig config = configProperty.managedReferenceValue as HomingMoveConfig;
                if (config == null)
                {
                    config = new HomingMoveConfig();
                }

                if (json?.homing != null)
                {
                    config.speed = json.homing.speed;
                    config.turnSpeed = json.homing.turnSpeed;
                }

                configProperty.managedReferenceValue = config;
                return config;
            }

            if (string.Equals(moveType, "Orbit", StringComparison.OrdinalIgnoreCase))
            {
                OrbitMoveConfig config = configProperty.managedReferenceValue as OrbitMoveConfig;
                if (config == null)
                {
                    config = new OrbitMoveConfig();
                }

                if (json?.orbit != null)
                {
                    config.orbitRadius = json.orbit.orbitRadius;
                    config.orbitAngularSpeed = json.orbit.orbitAngularSpeed;
                    config.clockwise = json.orbit.clockwise;
                    config.spawnOrder = json.orbit.spawnOrder;
                    config.maxProjectileCount = json.orbit.maxProjectileCount;
                    config.resetPhaseWhenLayoutChanges = json.orbit.resetPhaseWhenLayoutChanges;
                    config.radialPulseAmplitude = json.orbit.radialPulseAmplitude;
                    config.radialPulseFrequency = json.orbit.radialPulseFrequency;
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