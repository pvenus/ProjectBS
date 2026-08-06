using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Skill;
using Effect;
namespace ResourceTools.Skill
{
    [Serializable]
    public class CastJson
    {
        public string castId;
        public string targetingType;

        public float castTime;
        public string burst;

        public float cooldown;
        public float range;

        public bool skipAttackAnimation;

        public string castMove;
        public string selfEffects;
    }

    [Serializable]
    public class BurstJson
    {
        public int count = 1;
        public float interval;
    }

    [Serializable]
    public class CastMoveJson
    {
        public string moveType;
        public float distance;
        public float duration;
    }


    /// <summary>
    /// SkillCastSO 전용 에셋 빌더.
    /// JSON의 cast 데이터를 SkillCastSO 에셋으로 생성/갱신한다.
    /// 
    /// Self effect entries are delegated to EffectEntryAssetBuilder.
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

            if (!Validate(json))
            {
                Debug.LogError(
                    $"[SkillCastAssetBuilder] Invalid Cast json. castId={json.castId}");
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

        private static T ParseObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonUtility.FromJson<T>(json);
        }

        private static T[] ParseArray<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(
                $"{{\"items\":{json}}}");

            return wrapper != null
                ? wrapper.items
                : null;
        }

        private static string[] ParseJsonObjectArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            string trimmed = json.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '[' || trimmed[trimmed.Length - 1] != ']')
            {
                return null;
            }

            var results = new System.Collections.Generic.List<string>();
            int depth = 0;
            int startIndex = -1;
            bool inString = false;
            bool escape = false;

            for (int i = 0; i < trimmed.Length; i++)
            {
                char c = trimmed[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (c == '{')
                {
                    if (depth == 0)
                    {
                        startIndex = i;
                    }

                    depth++;
                    continue;
                }

                if (c == '}')
                {
                    depth--;

                    if (depth == 0 && startIndex >= 0)
                    {
                        results.Add(trimmed.Substring(startIndex, i - startIndex + 1));
                        startIndex = -1;
                    }
                }
            }

            return results.ToArray();
        }

        [Serializable]
        private class ArrayWrapper<T>
        {
            public T[] items;
        }

        private static bool Validate(
            CastJson json)
        {
            BurstJson burst = ParseObject<BurstJson>(json.burst);
            CastMoveJson castMove = ParseObject<CastMoveJson>(json.castMove);

            bool isValid = true;
            string castId = string.IsNullOrWhiteSpace(json.castId)
                ? "<empty>"
                : json.castId;

            if (string.IsNullOrWhiteSpace(json.castId))
            {
                LogValidationError(castId, "castId is required.");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(json.targetingType))
            {
                LogValidationError(castId, "targetingType is required.");
                isValid = false;
            }
            else if (!IsDefinedEnumValue<TargetingType>(json.targetingType))
            {
                LogValidationError(castId, $"targetingType enum value not found. value={json.targetingType}");
                isValid = false;
            }

            if (json.castTime < 0f)
            {
                LogValidationError(castId, $"castTime must be >= 0. value={json.castTime}");
                isValid = false;
            }

            if (json.cooldown < 0f)
            {
                LogValidationError(castId, $"cooldown must be >= 0. value={json.cooldown}");
                isValid = false;
            }

            if (json.range < 0f)
            {
                LogValidationError(castId, $"range must be >= 0. value={json.range}");
                isValid = false;
            }


            if (burst != null)
            {
                isValid &= ValidateBurst(castId, burst);
            }

            if (castMove != null)
            {
                isValid &= ValidateCastMove(castId, castMove);
            }

            return isValid;
        }

        private static bool ValidateBurst(
            string castId,
            BurstJson burst)
        {
            bool isValid = true;

            if (burst.count < 1)
            {
                LogValidationError(castId, $"burst.count must be >= 1. value={burst.count}");
                isValid = false;
            }

            if (burst.interval < 0f)
            {
                LogValidationError(castId, $"burst.interval must be >= 0. value={burst.interval}");
                isValid = false;
            }

            return isValid;
        }

        private static bool ValidateCastMove(
            string castId,
            CastMoveJson castMove)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(castMove.moveType))
            {
                LogValidationError(castId, "castMove.moveType is required when castMove block exists.");
                isValid = false;
            }
            else if (!IsDefinedEnumValue<CastMoveType>(castMove.moveType))
            {
                LogValidationError(castId, $"castMove.moveType enum value not found. value={castMove.moveType}");
                isValid = false;
            }

            if (castMove.distance < 0f)
            {
                LogValidationError(castId, $"castMove.distance must be >= 0. value={castMove.distance}");
                isValid = false;
            }

            if (castMove.duration < 0f)
            {
                LogValidationError(castId, $"castMove.duration must be >= 0. value={castMove.duration}");
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
            string castId,
            string message)
        {
            Debug.LogError(
                $"[SkillCastAssetBuilder] Validation failed. castId={castId} {message}");
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
            BurstJson burst = ParseObject<BurstJson>(json.burst);
            CastMoveJson castMove = ParseObject<CastMoveJson>(json.castMove);
            string[] selfEffects = ParseJsonObjectArray(json.selfEffects);

            TargetingType targetingType = ParseEnum<TargetingType>(json.targetingType);
            EffectEntrySO[] selfEffectEntries =
                CreateOrUpdateSelfEffectEntries(
                    json,
                    selfEffects,
                    outputFolder);

            castSo.ApplyEditorData(
                json.castId,
                targetingType,
                json.castTime,
                json.cooldown,
                json.range,
                json.skipAttackAnimation,
                selfEffectEntries);

            if (burst != null)
            {
                castSo.ApplyEditorBurst(
                    burst.count,
                    burst.interval);
            }

            if (castMove != null)
            {
                castSo.ApplyEditorCastMove(
                    ParseEnum<CastMoveType>(castMove.moveType),
                    castMove.distance,
                    castMove.duration);
            }
        }

        private static EffectEntrySO[] CreateOrUpdateSelfEffectEntries(
            CastJson castJson,
            string[] selfEffects,
            string outputFolder)
        {
            if (castJson == null || selfEffects == null || selfEffects.Length == 0)
            {
                return Array.Empty<EffectEntrySO>();
            }

            EffectEntrySO[] entries = new EffectEntrySO[selfEffects.Length];

            for (int i = 0; i < selfEffects.Length; i++)
            {
                string entryJson = selfEffects[i];
                if (string.IsNullOrWhiteSpace(entryJson))
                {
                    continue;
                }

                entries[i] = Effect.EffectEntryAssetBuilder.CreateOrUpdate(
                    entryJson,
                    outputFolder);
            }

            return entries;
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
                $"[SkillCastAssetBuilder] Enum parse failed. enum={typeof(TEnum).Name} value={value}");
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
