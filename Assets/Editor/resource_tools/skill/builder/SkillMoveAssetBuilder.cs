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
        public bool applyDirectionRotation = true;
        public float rotationOffset;

        public MoveConfigJson config;

        // Legacy type-specific blocks. Kept as fallbacks for older JSON data.
        public LinearMoveJson linear;
        public HoverMoveJson hover;
        public WarpMoveJson warp;
        public HomingMoveJson homing;
        public OrbitMoveJson orbit;
    }

    [Serializable]
    public class MoveConfigJson
    {
        public float speed;
        public Vector2Json followOffset;
        public float turnSpeed;

        public float orbitRadius;
        public float orbitAngularSpeed;
        public bool clockwise;
        public int spawnOrder;
        public int maxProjectileCount;
        public bool resetPhaseWhenLayoutChanges;
        public float radialPulseAmplitude;
        public float radialPulseFrequency;
    }

    [Serializable]
    public class Vector2Json
    {
        public float x;
        public float y;
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
            ProjectileMoveType moveType = ParseMoveType(
                ResolveMoveType(json));

            SkillMoveConfig config = CreateConfig(
                moveType,
                json);

            moveSo.ApplyEditorData(
                json.moveId,
                moveType,
                json.applyDirectionRotation,
                json.rotationOffset);

            moveSo.ApplyEditorConfig(config);
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

        private static ProjectileMoveType ParseMoveType(
            string value)
        {
            if (Enum.TryParse(
                    value,
                    true,
                    out ProjectileMoveType result))
            {
                return result;
            }

            Debug.LogWarning($"[SkillMoveAssetBuilder] Invalid moveType. value={value}. fallback=Linear");
            return ProjectileMoveType.Linear;
        }

        private static SkillMoveConfig CreateConfig(
            ProjectileMoveType moveType,
            MoveJson json)
        {
            switch (moveType)
            {
                case ProjectileMoveType.Linear:
                    return CreateLinearConfig(json);

                case ProjectileMoveType.Hover:
                    return CreateHoverConfig(json);

                case ProjectileMoveType.Warp:
                    return new WarpMoveConfig();

                case ProjectileMoveType.Homing:
                    return CreateHomingConfig(json);

                case ProjectileMoveType.Orbit:
                    return CreateOrbitConfig(json);

                default:
                    return null;
            }
        }

        private static SkillMoveConfig CreateLinearConfig(
            MoveJson json)
        {
            return new LinearMoveConfig
            {
                speed = json != null && json.config != null
                    ? json.config.speed
                    : json != null && json.linear != null
                        ? json.linear.speed
                        : 0f
            };
        }

        private static SkillMoveConfig CreateHoverConfig(
            MoveJson json)
        {
            var config = new HoverMoveConfig();

            if (json != null && json.config != null && json.config.followOffset != null)
            {
                config.followOffset = new Vector2(
                    json.config.followOffset.x,
                    json.config.followOffset.y);
            }
            else if (json != null && json.hover != null)
            {
                config.followOffset = new Vector2(
                    json.hover.followOffsetX,
                    json.hover.followOffsetY);
            }

            return config;
        }

        private static SkillMoveConfig CreateHomingConfig(
            MoveJson json)
        {
            var config = new HomingMoveConfig();

            if (json != null && json.config != null)
            {
                config.speed = json.config.speed;
                config.turnSpeed = json.config.turnSpeed;
            }
            else if (json != null && json.homing != null)
            {
                config.speed = json.homing.speed;
                config.turnSpeed = json.homing.turnSpeed;
            }

            return config;
        }

        private static SkillMoveConfig CreateOrbitConfig(
            MoveJson json)
        {
            var config = new OrbitMoveConfig();

            if (json != null && json.config != null)
            {
                config.orbitRadius = json.config.orbitRadius;
                config.orbitAngularSpeed = json.config.orbitAngularSpeed;
                config.clockwise = json.config.clockwise;
                config.spawnOrder = json.config.spawnOrder;
                config.maxProjectileCount = json.config.maxProjectileCount;
                config.resetPhaseWhenLayoutChanges = json.config.resetPhaseWhenLayoutChanges;
                config.radialPulseAmplitude = json.config.radialPulseAmplitude;
                config.radialPulseFrequency = json.config.radialPulseFrequency;
            }
            else if (json != null && json.orbit != null)
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

            return config;
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
