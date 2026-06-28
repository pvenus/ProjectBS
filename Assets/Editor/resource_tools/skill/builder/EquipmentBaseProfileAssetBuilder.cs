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
        public string baseProfileId;
        public string skillType;
        public string skillComponentType;
        public int projectileCount;
        public float projectileScale;
        public float projectileColliderRadius;
        public float projectileLifetime;

        public string projectile;
        public string projectileSpawn;
        public string brainMeta;
    }

    [Serializable]
    public class ProjectileArrangementJson
    {
        public string arrangement;
        public float arrangementValue;
        public float spreadAngle;
        public float radius;
    }

    [Serializable]
    public class ProjectileSpawnJson
    {
        public float spawnOffset;
        public float interval;
    }

    [Serializable]
    public class BattleSkillBrainMetaJson
    {
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

            if (!Validate(json))
            {
                Debug.LogError(
                    $"[EquipmentBaseProfileAssetBuilder] Invalid BaseProfile json. baseProfileId={json.baseProfileId}");
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
            if (!string.IsNullOrWhiteSpace(json.baseProfileId))
            {
                return SanitizeFileName(json.baseProfileId);
            }

            return "equipment.base.profile";
        }

        private static bool Validate(
            BaseProfileJson json)
        {
            bool isValid = true;
            string baseProfileId = string.IsNullOrWhiteSpace(json.baseProfileId)
                ? "<empty>"
                : json.baseProfileId;

            if (string.IsNullOrWhiteSpace(json.baseProfileId))
            {
                LogValidationError(baseProfileId, "baseProfileId is required.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(json.skillType))
            {
                LogValidationError(baseProfileId, "skillType is required.");
                isValid = false;
            }
            else if (!IsDefinedEnumValue<SkillType>(json.skillType))
            {
                LogValidationError(baseProfileId, $"skillType enum value not found. value={json.skillType}");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(json.skillComponentType))
            {
                LogValidationError(baseProfileId, "skillComponentType is required.");
                isValid = false;
            }
            else if (!IsDefinedEnumValue<SkillComponentType>(json.skillComponentType))
            {
                LogValidationError(baseProfileId, $"skillComponentType enum value not found. value={json.skillComponentType}");
                isValid = false;
            }

            if (json.projectileCount < 1)
            {
                LogValidationError(baseProfileId, $"projectileCount must be >= 1. value={json.projectileCount}");
                isValid = false;
            }

            if (json.projectileScale <= 0f)
            {
                LogValidationError(baseProfileId, $"projectileScale must be > 0. value={json.projectileScale}");
                isValid = false;
            }

            if (json.projectileColliderRadius <= 0f)
            {
                LogValidationError(baseProfileId, $"projectileColliderRadius must be > 0. value={json.projectileColliderRadius}");
                isValid = false;
            }

            if (json.projectileLifetime <= 0f)
            {
                LogValidationError(baseProfileId, $"projectileLifetime must be > 0. value={json.projectileLifetime}");
                isValid = false;
            }

            ProjectileArrangementJson projectile = ParseOptionalJson<ProjectileArrangementJson>(json.projectile);
            ProjectileSpawnJson projectileSpawn = ParseOptionalJson<ProjectileSpawnJson>(json.projectileSpawn);
            BattleSkillBrainMetaJson brainMeta = ParseOptionalJson<BattleSkillBrainMetaJson>(json.brainMeta);

            if (projectile != null)
            {
                isValid &= ValidateProjectileArrangement(baseProfileId, projectile);
            }

            if (projectileSpawn != null)
            {
                isValid &= ValidateProjectileSpawn(baseProfileId, projectileSpawn);
            }

            if (brainMeta != null)
            {
                isValid &= ValidateBrainMeta(baseProfileId, brainMeta);
            }

            return isValid;
        }

        private static bool ValidateProjectileArrangement(
            string profileId,
            ProjectileArrangementJson projectile)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(projectile.arrangement))
            {
                LogValidationError(profileId, "projectile.arrangement is required when projectile block exists.");
                isValid = false;
            }
            else if (!IsDefinedEnumValue<ProjectileArrangementType>(projectile.arrangement))
            {
                LogValidationError(profileId, $"projectile.arrangement enum value not found. value={projectile.arrangement}");
                isValid = false;
            }

            if (projectile.arrangementValue < 0f)
            {
                LogValidationError(profileId, $"projectile.arrangementValue must be >= 0. value={projectile.arrangementValue}");
                isValid = false;
            }

            if (projectile.spreadAngle < 0f)
            {
                LogValidationError(profileId, $"projectile.spreadAngle must be >= 0. value={projectile.spreadAngle}");
                isValid = false;
            }

            if (projectile.radius < 0f)
            {
                LogValidationError(profileId, $"projectile.radius must be >= 0. value={projectile.radius}");
                isValid = false;
            }

            return isValid;
        }

        private static bool ValidateProjectileSpawn(
            string profileId,
            ProjectileSpawnJson projectileSpawn)
        {
            bool isValid = true;

            if (projectileSpawn.interval < 0f)
            {
                LogValidationError(profileId, $"projectileSpawn.interval must be >= 0. value={projectileSpawn.interval}");
                isValid = false;
            }

            return isValid;
        }

        private static bool ValidateBrainMeta(
            string profileId,
            BattleSkillBrainMetaJson brainMeta)
        {
            bool isValid = true;

            if (!string.IsNullOrWhiteSpace(brainMeta.category) &&
                !IsDefinedEnumValue<BattleSkillCategory>(brainMeta.category))
            {
                LogValidationError(profileId, $"brainMeta.category enum value not found. value={brainMeta.category}");
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(brainMeta.targetType) &&
                !IsDefinedEnumValue<BattleSkillTargetType>(brainMeta.targetType))
            {
                LogValidationError(profileId, $"brainMeta.targetType enum value not found. value={brainMeta.targetType}");
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(brainMeta.tacticalNeed) &&
                !IsDefinedEnumValue<BattleSkillTacticalNeed>(brainMeta.tacticalNeed))
            {
                LogValidationError(profileId, $"brainMeta.tacticalNeed enum value not found. value={brainMeta.tacticalNeed}");
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(brainMeta.category) &&
                string.IsNullOrWhiteSpace(brainMeta.targetType))
            {
                LogValidationError(profileId, "brainMeta.targetType is required when brainMeta.category is set.");
                isValid = false;
            }

            if (brainMeta.basePriority < 0f)
            {
                LogValidationError(profileId, $"brainMeta.basePriority must be >= 0. value={brainMeta.basePriority}");
                isValid = false;
            }

            return isValid;
        }

        private static bool IsDefinedEnumValue<TEnum>(
            string value)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] names = Enum.GetNames(typeof(TEnum));

            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(names[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void LogValidationError(
            string baseProfileId,
            string message)
        {
            Debug.LogError(
                $"[EquipmentBaseProfileAssetBuilder] Validation failed. baseProfileId={baseProfileId} {message}");
        }

        private static void Apply(
            EquipmentBaseProfileSO profile,
            BaseProfileJson json)
        {
            profile.ApplyEditorData(
                json.baseProfileId,
                ParseEnum<SkillType>(json.skillType),
                ParseEnum<SkillComponentType>(json.skillComponentType),
                json.projectileCount,
                json.projectileScale,
                json.projectileColliderRadius,
                json.projectileLifetime);

            ProjectileArrangementJson projectile = ParseOptionalJson<ProjectileArrangementJson>(json.projectile);
            ProjectileSpawnJson projectileSpawn = ParseOptionalJson<ProjectileSpawnJson>(json.projectileSpawn);
            BattleSkillBrainMetaJson brainMeta = ParseOptionalJson<BattleSkillBrainMetaJson>(json.brainMeta);

            if (projectile != null)
            {
                profile.ApplyEditorProjectileArrangement(
                    ParseEnum<ProjectileArrangementType>(projectile.arrangement),
                    projectile.arrangementValue,
                    projectile.spreadAngle,
                    projectile.radius);
            }

            if (projectileSpawn != null)
            {
                profile.ApplyEditorProjectileSpawn(
                    projectileSpawn.spawnOffset,
                    projectileSpawn.interval);
            }

            if (brainMeta != null)
            {
                profile.ApplyEditorBrainMeta(
                    ParseEnum<BattleSkillCategory>(brainMeta.category),
                    ParseEnum<BattleSkillTargetType>(brainMeta.targetType),
                    ParseOptionalEnum<BattleSkillTacticalNeed>(brainMeta.tacticalNeed),
                    brainMeta.basePriority);
            }
        }



        private static T ParseOptionalJson<T>(
            string json)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<T>(json);
        }

        private static TEnum ParseEnum<TEnum>(
            string value)
            where TEnum : struct, Enum
        {
            if (Enum.TryParse(value, true, out TEnum result))
            {
                return result;
            }

            throw new InvalidOperationException(
                $"[EquipmentBaseProfileAssetBuilder] Enum parse failed. enum={typeof(TEnum).Name} value={value}");
        }

        private static TEnum ParseOptionalEnum<TEnum>(
            string value)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default;
            }

            return ParseEnum<TEnum>(value);
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