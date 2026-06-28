using System;
using System.IO;
using Effect;
using Skill;
using UnityEditor;
using UnityEngine;
using Stat;

namespace ResourceTools.Effect
{
    /// <summary>
    /// EffectSO JSON generator.
    ///
    /// Other editor generators can call:
    /// - EffectJsonGenerator.CreateOrUpdateEffect(...)
    /// - EffectJsonGenerator.CreateOrUpdateHitEffectEntry(...)
    ///
    /// New effect types can be added by extending ResolveEffectSoTypeName(...)
    /// or by making effectType equal to the concrete EffectSO class name.
    /// </summary>
    public static class EffectJsonGenerator
    {

        [Serializable]
        public class HitEffectJson
        {
            public string effect;
            public string effectSO;
            public string lifetimeType;
            public string categoryType;
            public float duration = -1f;
            public int maxApplyCount = 1;
        }

        public static EffectSO CreateOrUpdateEffectFromJson(
            string json,
            string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("[EffectJsonGenerator] Empty effect json.");
                return null;
            }

            return EffectAssetBuilder.CreateOrUpdate(json, outputFolder);
        }

        public static EffectEntrySO CreateOrUpdateHitEffectEntry(
            HitEffectJson data,
            string outputFolder)
        {
            if (data == null)
            {
                return null;
            }

            EffectSO effectSo = !string.IsNullOrWhiteSpace(data.effect)
                ? EffectAssetBuilder.CreateOrUpdate(data.effect, outputFolder)
                : FindEffectSoByIdOrName(data.effectSO);

            string assetName = effectSo != null
                ? effectSo.name
                : "unnamed";

            if (string.IsNullOrWhiteSpace(data.lifetimeType)
                || !Enum.TryParse(
                    data.lifetimeType,
                    true,
                    out EffectLifetimeType lifetimeType))
            {
                Debug.LogError(
                    $"[EffectJsonGenerator] Invalid lifetimeType. effectId={assetName}, lifetimeType={data.lifetimeType}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(data.categoryType)
                || !Enum.TryParse(
                    data.categoryType,
                    true,
                    out EffectCategoryType categoryType))
            {
                Debug.LogError(
                    $"[EffectJsonGenerator] Invalid categoryType. effectId={assetName}, categoryType={data.categoryType}");
                return null;
            }

            float duration = data.duration;
            int maxApplyCount = data.maxApplyCount;
            string entryAssetPath = $"{outputFolder}/{assetName}.entry.asset";
            return EffectEntryAssetBuilder.CreateOrUpdate(
                entryAssetPath,
                effectSo,
                lifetimeType,
                categoryType,
                duration,
                maxApplyCount,
                false,
                0f);
        }

        public static EffectEntrySO[] CreateOrUpdateHitEffectEntries(
            HitEffectJson[] data,
            string outputFolder)
        {
            if (data == null || data.Length == 0)
            {
                return Array.Empty<EffectEntrySO>();
            }

            EffectEntrySO[] result =
                new EffectEntrySO[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = CreateOrUpdateHitEffectEntry(
                    data[i],
                    outputFolder);
            }

            return result;
        }

        public static EffectSO FindEffectSoByIdOrName(string effectIdOrName)
        {
            if (string.IsNullOrEmpty(effectIdOrName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{effectIdOrName} t:EffectSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EffectSO effect = AssetDatabase.LoadAssetAtPath<EffectSO>(path);

                if (effect != null)
                {
                    SerializedObject serializedEffect = new SerializedObject(effect);
                    SerializedProperty effectIdProp = serializedEffect.FindProperty("effectId");
                    if (effectIdProp != null && effectIdProp.stringValue.Equals(effectIdOrName, StringComparison.OrdinalIgnoreCase))
                    {
                        return effect;
                    }
                }
            }

            // Fallback to matching by asset name
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EffectSO effect = AssetDatabase.LoadAssetAtPath<EffectSO>(path);

                if (effect != null &&
                    effect.name.Equals(effectIdOrName, StringComparison.OrdinalIgnoreCase))
                {
                    return effect;
                }
            }

            Debug.LogWarning($"[EffectJsonGenerator] EffectSO not found: {effectIdOrName}");
            return null;
        }

    }
}