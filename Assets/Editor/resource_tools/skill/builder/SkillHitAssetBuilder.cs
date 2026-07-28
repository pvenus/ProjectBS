using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Skill;
using Effect;

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

        public string damage;
        public string buffEffects;
        public string debuffEffects;
        public string split;

    }

    [Serializable]
    public class HitSplitJson
    {
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

            Apply(hitSo, json, outputFolder);

            EditorUtility.SetDirty(hitSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillHitAssetBuilder] Updated SkillHitSO: {assetPath}");

            return hitSo;
        }


        private static T ParseObject<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonUtility.FromJson<T>(json);
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
            string outputFolder)
        {
            SerializedObject serializedObject = new SerializedObject(hitSo);

            DamageJson damage = ParseObject<DamageJson>(json.damage);
            string[] buffEffects = ParseJsonObjectArray(json.buffEffects);
            string[] debuffEffects = ParseJsonObjectArray(json.debuffEffects);
            HitSplitJson split = ParseObject<HitSplitJson>(json.split);
            SerializedProperty effectsProperty =
                serializedObject.FindProperty("effects");
            SerializedProperty splitProperty =
                serializedObject.FindProperty("split");

            SetDamageProfile(serializedObject, damage);
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
            SetHitEffectEntries(
                effectsProperty,
                "buffEffects",
                CreateHitEffectEntries(buffEffects, outputFolder));
            SetHitEffectEntries(
                effectsProperty,
                "debuffEffects",
                CreateHitEffectEntries(debuffEffects, outputFolder));
            if (split != null)
            {
                SetRelativeBool(
                    splitProperty,
                    "useSplitMultiHitDamage",
                    split.useSplitMultiHitDamage);
                SetRelativeInt(
                    splitProperty,
                    "splitHitCount",
                    split.splitHitCount);
                SetRelativeFloat(
                    splitProperty,
                    "splitHitInterval",
                    split.splitHitInterval);
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetDamageProfile(
            SerializedObject serializedObject,
            DamageJson damage)
        {
            SerializedProperty damageProperty = serializedObject.FindProperty("damage");
            if (damageProperty == null)
            {
                damageProperty = serializedObject.FindProperty("damageProfile");
            }

            if (damageProperty == null)
            {
                Debug.LogWarning("[SkillHitAssetBuilder] Serialized damage profile property not found: damage or damageProfile");
                return;
            }

            if (damage == null)
            {
                SetRelativeString(damageProperty, "damageType", string.Empty);
                SetRelativeFloat(damageProperty, "baseDamage", 0f);
                SetRelativeFloat(damageProperty, "attackPercentDamage", 0f);
                SetRelativeBool(damageProperty, "canCritical", false);
                SetRelativeBool(damageProperty, "ignoreDefense", false);
                return;
            }

            SetRelativeString(damageProperty, "damageType", damage.damageType);
            SetRelativeFloat(damageProperty, "baseDamage", damage.baseDamage);
            SetRelativeFloat(damageProperty, "attackPercentDamage", damage.attackPercentDamage);
            SetRelativeBool(damageProperty, "canCritical", damage.canCritical);
            SetRelativeBool(damageProperty, "ignoreDefense", damage.ignoreDefense);
        }

        private static void SetRelativeString(
            SerializedProperty parent,
            string propertyName,
            string value)
        {
            SerializedProperty property = parent.FindPropertyRelative(propertyName);
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

        private static void SetRelativeFloat(
            SerializedProperty parent,
            string propertyName,
            float value)
        {
            if (parent == null)
            {
                return;
            }

            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetRelativeInt(
            SerializedProperty parent,
            string propertyName,
            int value)
        {
            if (parent == null)
            {
                return;
            }

            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.intValue = value;
            }
        }

        private static void SetRelativeBool(
            SerializedProperty parent,
            string propertyName,
            bool value)
        {
            if (parent == null)
            {
                return;
            }

            SerializedProperty property = parent.FindPropertyRelative(propertyName);
            if (property != null)
            {
                property.boolValue = value;
            }
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
            SerializedProperty parent,
            string propertyName,
            EffectEntrySO[] values)
        {
            if (parent == null)
            {
                Debug.LogWarning(
                    $"[SkillHitAssetBuilder] Serialized effect profile not found: {propertyName}");
                return;
            }

            SerializedProperty property = parent.FindPropertyRelative(propertyName);

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
                element.objectReferenceValue = values[i];
            }
        }

        private static EffectEntrySO[] CreateHitEffectEntries(
            string[] data,
            string outputFolder)
        {
            if (data == null || data.Length == 0)
            {
                return Array.Empty<EffectEntrySO>();
            }

            EffectEntrySO[] entries = new EffectEntrySO[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                string entryJson = data[i];
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
