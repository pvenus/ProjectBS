using Effect;
using UnityEditor;
using UnityEngine;
using System;

namespace ResourceTools.Effect
{
    public static class EffectEntryAssetBuilder
    {
        [Serializable]
        public class EffectEntryJson
        {
            public string entryId;
            public bool hasValueOverride;
            public float valueOverride;
            public string effect;
            public string effectSO;
            public string lifetimeType;
            public string categoryType;
            public float duration = -1f;
            public int maxApplyCount = 1;
        }

        public static EffectEntrySO CreateOrUpdate(
            string entryJson,
            string outputFolder)
        {
            if (string.IsNullOrEmpty(entryJson))
                return null;

            EffectEntryJson data = JsonUtility.FromJson<EffectEntryJson>(entryJson);
            if (data == null)
                return null;

            data.effect = ExtractJsonValue(entryJson, "effect");

            if (string.IsNullOrWhiteSpace(data.effect))
            {
                Debug.LogError("[EffectEntryAssetBuilder] effect json is required.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(data.entryId))
            {
                data.entryId = ResolveEntryId(data.effect);
            }

            EffectSO effectSO = EffectAssetBuilder.CreateOrUpdate(
                data.effect,
                outputFolder);
            if (effectSO == null)
                return null;

            string assetPath = System.IO.Path.Combine(
                outputFolder,
                data.entryId + ".asset")
                .Replace("\\", "/");

            if (string.IsNullOrWhiteSpace(data.lifetimeType) ||
                !Enum.TryParse(data.lifetimeType, out EffectLifetimeType lifetimeType))
            {
                Debug.LogError($"[EffectEntryAssetBuilder] Invalid lifetimeType. entryId={data.entryId}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(data.categoryType) ||
                !Enum.TryParse(data.categoryType, out EffectCategoryType categoryType))
            {
                Debug.LogError($"[EffectEntryAssetBuilder] Invalid categoryType. entryId={data.entryId}");
                return null;
            }

            return CreateOrUpdate(
                assetPath,
                effectSO,
                lifetimeType,
                categoryType,
                data.duration,
                data.maxApplyCount,
                data.hasValueOverride,
                data.valueOverride
            );
        }

        public static EffectEntrySO CreateOrUpdate(
            string assetPath,
            EffectSO effectSO,
            EffectLifetimeType lifetimeType,
            EffectCategoryType categoryType,
            float duration,
            int maxApplyCount,
            bool hasValueOverride,
            float valueOverride)
        {
            if (!Validate(
                    assetPath,
                    effectSO,
                    duration,
                    maxApplyCount))
            {
                return null;
            }

            EffectEntrySO asset = AssetDatabase.LoadAssetAtPath<EffectEntrySO>(assetPath);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EffectEntrySO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            asset.ApplyEditorData(
                effectSO,
                lifetimeType,
                categoryType,
                duration,
                maxApplyCount,
                hasValueOverride,
                valueOverride);

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);

            return asset;
        }

        private static string ResolveEntryId(
            string effectJson)
        {
            EffectEntryEffectHeaderJson header = JsonUtility.FromJson<EffectEntryEffectHeaderJson>(effectJson);

            if (header != null && !string.IsNullOrWhiteSpace(header.effectId))
            {
                return header.effectId + ".entry";
            }

            return "effect.entry";
        }

        private static string ExtractJsonValue(
            string json,
            string propertyName)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
            {
                return null;
            }

            string key = $"\"{propertyName}\"";
            int keyIndex = json.IndexOf(key, StringComparison.Ordinal);
            if (keyIndex < 0)
            {
                return null;
            }

            int colonIndex = json.IndexOf(':', keyIndex + key.Length);
            if (colonIndex < 0)
            {
                return null;
            }

            int valueStart = colonIndex + 1;
            while (valueStart < json.Length && char.IsWhiteSpace(json[valueStart]))
            {
                valueStart++;
            }

            if (valueStart >= json.Length)
            {
                return null;
            }

            char first = json[valueStart];
            if (first == '{')
            {
                return ExtractBalanced(json, valueStart, '{', '}');
            }

            if (first == '[')
            {
                return ExtractBalanced(json, valueStart, '[', ']');
            }

            if (first == '"')
            {
                return ExtractString(json, valueStart);
            }

            int valueEnd = valueStart;
            while (valueEnd < json.Length && json[valueEnd] != ',' && json[valueEnd] != '}')
            {
                valueEnd++;
            }

            return json.Substring(valueStart, valueEnd - valueStart).Trim();
        }

        private static string ExtractBalanced(
            string json,
            int startIndex,
            char open,
            char close)
        {
            int depth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = startIndex; i < json.Length; i++)
            {
                char c = json[i];

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

                if (c == open)
                {
                    depth++;
                }
                else if (c == close)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return json.Substring(startIndex, i - startIndex + 1);
                    }
                }
            }

            return null;
        }

        private static string ExtractString(
            string json,
            int startIndex)
        {
            bool escape = false;

            for (int i = startIndex + 1; i < json.Length; i++)
            {
                char c = json[i];

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
                    return json.Substring(startIndex + 1, i - startIndex - 1);
                }
            }

            return null;
        }

        [Serializable]
        private class EffectEntryEffectHeaderJson
        {
            public string effectId;
        }

        private static bool Validate(
            string assetPath,
            EffectSO effectSO,
            float duration,
            int maxApplyCount)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                Debug.LogError("[EffectEntryAssetBuilder] assetPath is required.");
                return false;
            }

            if (effectSO == null)
            {
                Debug.LogError("[EffectEntryAssetBuilder] effectSO is required.");
                return false;
            }

            if (duration < 0f)
            {
                Debug.LogError($"[EffectEntryAssetBuilder] duration must be >= 0. assetPath={assetPath}");
                return false;
            }

            if (maxApplyCount <= 0)
            {
                Debug.LogError($"[EffectEntryAssetBuilder] maxApplyCount must be > 0. assetPath={assetPath}");
                return false;
            }

            return true;
        }
    }
}