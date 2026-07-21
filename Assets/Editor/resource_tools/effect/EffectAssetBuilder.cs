using System;
using System.IO;
using Effect;
using Stat;
using UnityEditor;
using UnityEngine;
using Skill;
using ResourceTools.Helper;
namespace ResourceTools.Effect
{
    public static class EffectAssetBuilder
    {
        [Serializable]
        public class EffectJson
        {
            [Header("Common")]
            public string effectId;
            public string skillOutputFolder;
            public string effectType;
            public string effectName;
            public string description;
            public string config;
        }

        [Serializable]
        private class EffectConfigHeaderJson
        {
            public string effectType;
        }

        [Serializable]
        public class StatModifierEffectJson : EffectJson
        {
            public string statType;
            public string modifierType;
            public float value;
        }

        [Serializable]
        public class HealEffectJson : EffectJson
        {
            public bool useMaxHpPercent;
            public float maxHpPercent;
            public float flatHealAmount;
            public bool useAttackScaling;
            public float attackPercentHeal;
            public bool clampToMaxHp;
        }

        [Serializable]
        public class KnockbackEffectJson : EffectJson
        {
            public float force;
            public string directionType;
            public bool normalizeDirection;
            public bool fallbackToProjectileDirection;
        }

        [Serializable]
        public class CooldownReduceEffectJson : EffectJson
        {
            public string reduceType;
            public float reducePercent;
            public float reduceSeconds;
        }

        [Serializable]
        public class ChanceOnHitSkillEffectJson : EffectJson
        {
            public float chance;
            public bool requireCriticalHit;
            public string skillId;
        }

        [Serializable]
        public class ChanceOnHitStatModifierEffectJson : EffectJson
        {
            public float chancePercent;
            public string statType;
            public string valueType;
            public float value;
        }

        [Serializable]
        public class ChanceOnHealStatModifierEffectJson : EffectJson
        {
            public float chance;
            public string triggerTargetType;
            public string statType;
            public string modifierType;
            public float value;
        }

        [Serializable]
        public class ChanceOnHealCooldownReduceEffectJson : EffectJson
        {
            public float chance;
            public string triggerTargetType;
            public string reduceType;
            public float reducePercent;
            public float reduceSeconds;
        }

        [Serializable]
        public class AttackBleedEffectJson : EffectJson
        {
            public float chancePercent;
            public float attackRatioPercent;
        }

        [Serializable]
        public class TauntEffectJson : EffectJson
        {
        }

        public static EffectSO CreateOrUpdate(
            string effectJson,
            string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(effectJson))
            {
                return null;
            }

            EffectJson data = ParseEffectJson(effectJson);

            return CreateOrUpdate(data, outputFolder);
        }

        private static EffectJson ParseEffectJson(
            string json)
        {
            EffectJson header = JsonUtility.FromJson<EffectJson>(json);
            header.effectType = ExtractJsonValue(json, "effectType");

            if (header == null)
            {
                return null;
            }

            string configJson = ExtractJsonValue(json, "config");

            if (!string.IsNullOrWhiteSpace(configJson))
            {
                EffectConfigHeaderJson configHeader =
                    JsonUtility.FromJson<EffectConfigHeaderJson>(configJson);

                EffectType effectType = ParseEnum(
                    !string.IsNullOrWhiteSpace(header.effectType)
                        ? header.effectType
                        : (configHeader != null ? configHeader.effectType : string.Empty),
                    EffectType.None);

                EffectJson parsed = ParseConfigEffectJson(
                    effectType,
                    configJson);

                CopyCommonFields(
                    header,
                    parsed);

                return parsed;
            }

            return ParseRootEffectJson(
                ParseEnum(header.effectType, EffectType.None),
                json);
        }

        private static EffectJson ParseRootEffectJson(
            EffectType effectType,
            string json)
        {
            switch (effectType)
            {
                case EffectType.StatModifier:
                    return JsonUtility.FromJson<StatModifierEffectJson>(json);
                case EffectType.Heal:
                    return JsonUtility.FromJson<HealEffectJson>(json);
                case EffectType.Knockback:
                    return JsonUtility.FromJson<KnockbackEffectJson>(json);
                case EffectType.CooldownReduce:
                    return JsonUtility.FromJson<CooldownReduceEffectJson>(json);
                case EffectType.ChanceOnHitSkill:
                    return JsonUtility.FromJson<ChanceOnHitSkillEffectJson>(json);
                case EffectType.ChanceOnHitStatModifier:
                    return JsonUtility.FromJson<ChanceOnHitStatModifierEffectJson>(json);
                case EffectType.ChanceOnHealStatModifier:
                    return JsonUtility.FromJson<ChanceOnHealStatModifierEffectJson>(json);
                case EffectType.ChanceOnHealCooldownReduce:
                    return JsonUtility.FromJson<ChanceOnHealCooldownReduceEffectJson>(json);
                case EffectType.AttackBleed:
                    return JsonUtility.FromJson<AttackBleedEffectJson>(json);
                case EffectType.Taunt:
                    return JsonUtility.FromJson<TauntEffectJson>(json);
                default:
                    return JsonUtility.FromJson<EffectJson>(json);
            }
        }

        private static EffectJson ParseConfigEffectJson(
            EffectType effectType,
            string configJson)
        {
            EffectJson parsed = ParseRootEffectJson(
                effectType,
                configJson);

            if (parsed != null)
            {
                parsed.effectType = effectType.ToString();
            }

            return parsed;
        }

        private static void CopyCommonFields(
            EffectJson source,
            EffectJson target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.effectId = source.effectId;
            target.effectType = source.effectType;
            target.skillOutputFolder = source.skillOutputFolder;
            target.effectName = source.effectName;
            target.description = source.description;
            target.config = source.config;
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

        private static EffectSO CreateOrUpdate(
            EffectJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.effectId))
            {
                return null;
            }

            if (!ValidateEffectData(data))
            {
                return null;
            }

            EnsureFolder(outputFolder);

            string assetPath =
                $"{outputFolder}/{SanitizeFileName(data.effectId)}.asset";

            EffectSO effectSo = AssetDatabase.LoadAssetAtPath<EffectSO>(assetPath);

            if (effectSo == null)
            {
                effectSo = ScriptableObject.CreateInstance<EffectSO>();
                AssetDatabase.CreateAsset(effectSo, assetPath);
            }

            Apply(effectSo, data);

            EditorUtility.SetDirty(effectSo);
            AssetDatabase.SaveAssetIfDirty(effectSo);

            return effectSo;
        }

        private static void Apply(
            EffectSO effectSo,
            EffectJson data)
        {
            if (effectSo == null || data == null)
            {
                return;
            }

            EffectConfig config = CreateConfig(data);

            ApplyEffectData(
                effectSo,
                data,
                SpriteHelper.FindSprite(
                    data.effectId,
                    "icon"),
                config);
        }

        private static void ApplyEffectData(
            EffectSO effectSo,
            EffectJson data,
            Sprite icon,
            EffectConfig config)
        {
            SerializedObject serializedObject = new SerializedObject(effectSo);

            SerializedProperty effectIdProperty =
                serializedObject.FindProperty("effectId");

            if (effectIdProperty != null)
            {
                effectIdProperty.stringValue = data.effectId;
            }

            SerializedProperty effectNameProperty =
                serializedObject.FindProperty("effectName");

            if (effectNameProperty != null)
            {
                effectNameProperty.stringValue = data.effectName;
            }

            SerializedProperty descriptionProperty =
                serializedObject.FindProperty("description");

            if (descriptionProperty != null)
            {
                descriptionProperty.stringValue = data.description;
            }

            SerializedProperty iconProperty =
                serializedObject.FindProperty("icon");

            if (iconProperty != null)
            {
                iconProperty.objectReferenceValue = icon;
            }

            SerializedProperty configProperty =
                serializedObject.FindProperty("config");

            if (configProperty != null)
            {
                configProperty.managedReferenceValue = config;
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static EffectType ResolveEffectType(
            EffectJson data)
        {
            return data != null
                ? ParseEnum(data.effectType, EffectType.None)
                : EffectType.None;
        }

        private static bool ValidateEffectData(
            EffectJson data)
        {
            switch (ResolveEffectType(data))
            {
                case EffectType.StatModifier:
                {
                    StatModifierEffectJson json =
                        data as StatModifierEffectJson;

                    return json != null
                        && !string.IsNullOrWhiteSpace(json.statType)
                        && !string.IsNullOrWhiteSpace(json.modifierType);
                }

                case EffectType.Heal:
                    return data is HealEffectJson;

                case EffectType.Knockback:
                {
                    KnockbackEffectJson json =
                        data as KnockbackEffectJson;

                    return json != null
                        && !string.IsNullOrWhiteSpace(json.directionType);
                }

                case EffectType.CooldownReduce:
                {
                    CooldownReduceEffectJson json =
                        data as CooldownReduceEffectJson;

                    return json != null
                        && !string.IsNullOrWhiteSpace(json.reduceType);
                }

                case EffectType.ChanceOnHitSkill:
                    return data is ChanceOnHitSkillEffectJson;

                case EffectType.ChanceOnHitStatModifier:
                {
                    ChanceOnHitStatModifierEffectJson json =
                        data as ChanceOnHitStatModifierEffectJson;

                    return json != null
                        && !string.IsNullOrWhiteSpace(json.statType)
                        && !string.IsNullOrWhiteSpace(json.valueType);
                }

                case EffectType.ChanceOnHealStatModifier:
                {
                    ChanceOnHealStatModifierEffectJson json =
                        data as ChanceOnHealStatModifierEffectJson;

                    return json != null
                        && !string.IsNullOrWhiteSpace(json.triggerTargetType)
                        && !string.IsNullOrWhiteSpace(json.statType)
                        && !string.IsNullOrWhiteSpace(json.modifierType);
                }

                case EffectType.ChanceOnHealCooldownReduce:
                {
                    ChanceOnHealCooldownReduceEffectJson json =
                        data as ChanceOnHealCooldownReduceEffectJson;

                    return json != null
                        && !string.IsNullOrWhiteSpace(json.triggerTargetType)
                        && !string.IsNullOrWhiteSpace(json.reduceType);
                }

                case EffectType.AttackBleed:
                    return data is AttackBleedEffectJson;

                case EffectType.Taunt:
                    return data is TauntEffectJson;

                default:
                    Debug.LogError(
                        $"[EffectAssetBuilder] Validation failed. Unsupported effectType={data.effectType}, effectId={data.effectId}");
                    return false;
            }
        }

        private static EffectConfig CreateConfig(
            EffectJson data)
        {
            switch (ResolveEffectType(data))
            {
                case EffectType.StatModifier:
                    return CreateStatModifierConfig(data as StatModifierEffectJson);

                case EffectType.Heal:
                    return CreateHealConfig(data as HealEffectJson);

                case EffectType.Knockback:
                    return CreateKnockbackConfig(data as KnockbackEffectJson);

                case EffectType.CooldownReduce:
                    return CreateCooldownReduceConfig(data as CooldownReduceEffectJson);

                case EffectType.ChanceOnHitSkill:
                    return CreateChanceOnHitSkillConfig(data as ChanceOnHitSkillEffectJson);

                case EffectType.ChanceOnHitStatModifier:
                    return CreateChanceOnHitStatModifierConfig(data as ChanceOnHitStatModifierEffectJson);

                case EffectType.ChanceOnHealStatModifier:
                    return CreateChanceOnHealStatModifierConfig(data as ChanceOnHealStatModifierEffectJson);

                case EffectType.ChanceOnHealCooldownReduce:
                    return CreateChanceOnHealCooldownReduceConfig(data as ChanceOnHealCooldownReduceEffectJson);

                case EffectType.AttackBleed:
                    return CreateAttackBleedConfig(data as AttackBleedEffectJson);

                case EffectType.Taunt:
                    return CreateTauntConfig(data as TauntEffectJson);

                default:
                    return null;
            }
        }

        private static StatModifierEffectConfig CreateStatModifierConfig(
            StatModifierEffectJson data)
        {
            if (data == null)
                return null;
            var config = new StatModifierEffectConfig();

            config.ApplyEditorData(
                ParseEnum(data.statType, StatType.None),
                ParseEnum(data.modifierType, StatModifierType.Flat),
                data.value);

            return config;
        }

        private static HealEffectConfig CreateHealConfig(
            HealEffectJson data)
        {
            if (data == null)
                return null;
            var config = new HealEffectConfig();

            config.ApplyEditorData(
                data.useMaxHpPercent,
                data.maxHpPercent,
                data.flatHealAmount,
                data.useAttackScaling,
                data.attackPercentHeal,
                data.clampToMaxHp);

            return config;
        }

        private static ChanceOnHitStatModifierEffectConfig CreateChanceOnHitStatModifierConfig(
            ChanceOnHitStatModifierEffectJson data)
        {
            if (data == null)
                return null;
            var config = new ChanceOnHitStatModifierEffectConfig();

            config.ApplyEditorData(
                data.chancePercent,
                ParseEnum(data.statType, StatType.Attack),
                ParseEnum(data.valueType, StatModifierType.Percent),
                data.value);

            return config;
        }

        private static ChanceOnHealStatModifierEffectConfig CreateChanceOnHealStatModifierConfig(
            ChanceOnHealStatModifierEffectJson data)
        {
            if (data == null)
                return null;
            var config = new ChanceOnHealStatModifierEffectConfig();

            config.ApplyEditorData(
                data.chance,
                ParseEnum(
                    data.triggerTargetType,
                    HealTriggerTargetType.AnyAlly),
                ParseEnum(data.statType, StatType.Attack),
                ParseEnum(data.modifierType, StatModifierType.Flat),
                data.value);

            return config;
        }

        private static KnockbackEffectConfig CreateKnockbackConfig(
            KnockbackEffectJson data)
        {
            if (data == null)
                return null;
            var config = new KnockbackEffectConfig();

            config.ApplyEditorData(
                data.force,
                ParseEnum(
                    data.directionType,
                    KnockbackDirectionType.PushAwayFromSource),
                Vector2.up,
                data.normalizeDirection,
                data.fallbackToProjectileDirection);

            return config;
        }

        private static CooldownReduceEffectConfig CreateCooldownReduceConfig(
            CooldownReduceEffectJson data)
        {
            if (data == null)
                return null;
            var config = new CooldownReduceEffectConfig();

            config.ApplyEditorData(
                ParseEnum(
                    data.reduceType,
                    CooldownReduceType.Percent),
                data.reducePercent,
                data.reduceSeconds);

            return config;
        }

        private static ChanceOnHealCooldownReduceEffectConfig CreateChanceOnHealCooldownReduceConfig(
            ChanceOnHealCooldownReduceEffectJson data)
        {
            if (data == null)
                return null;
            var config = new ChanceOnHealCooldownReduceEffectConfig();

            config.ApplyEditorData(
                data.chance,
                ParseEnum(
                    data.triggerTargetType,
                    HealTriggerTargetType.AnyAlly),
                ParseEnum(
                    data.reduceType,
                    CooldownReduceType.FlatSeconds),
                data.reducePercent,
                data.reduceSeconds);

            return config;
        }

        private static AttackBleedEffectConfig CreateAttackBleedConfig(
            AttackBleedEffectJson data)
        {
            if (data == null)
                return null;
            var config = new AttackBleedEffectConfig();

            config.ApplyEditorData(
                data.chancePercent,
                data.attackRatioPercent);

            return config;
        }

        private static TauntEffectConfig CreateTauntConfig(
            TauntEffectJson data)
        {
            if (data == null)
                return null;

            var config = new TauntEffectConfig();
            config.ApplyEditorData();

            return config;
        }

        private static ChanceOnHitSkillEffectConfig CreateChanceOnHitSkillConfig(
            ChanceOnHitSkillEffectJson data)
        {
            if (data == null)
                return null;
            var config = new ChanceOnHitSkillEffectConfig();
            config.ApplyEditorData(
                data.chance,
                data.requireCriticalHit,
                ResolveEquipmentSkillSO(data.skillId),
                -1f);
            return config;
        }

        private static EquipmentSkillSO ResolveEquipmentSkillSO(
            string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return null;
            }

            EquipmentSkillSO[] skills = Resources.LoadAll<EquipmentSkillSO>(string.Empty);

            for (int i = 0; i < skills.Length; i++)
            {
                EquipmentSkillSO skill = skills[i];

                if (skill == null)
                {
                    continue;
                }

                if (skill.EquipmentId == skillId)
                {
                    return skill;
                }
            }

            return null;
        }

        private static TEnum ParseEnum<TEnum>(
            string value,
            TEnum fallback)
            where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return Enum.TryParse(value, true, out TEnum result)
                ? result
                : fallback;
        }

        private static void EnsureFolder(
            string folderPath)
        {
            folderPath = folderPath.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string SanitizeFileName(
            string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value;
        }

    }
}
