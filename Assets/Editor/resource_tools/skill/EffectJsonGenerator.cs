

using System;
using System.IO;
using System.Reflection;
using Effect;
using Skill;
using UnityEditor;
using UnityEngine;

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
        public class EffectJson
        {
            [Header("Common")]
            public string effectId;
            public string skillOutputFolder;
            public string effectType;
            public string effectName;
            public string description;
            public string iconName;
            public Skill.EquipmentSkillJsonGenerator.EquipmentSkillJson skill;

            [Header("Lifetime / Stack")]
            public float duration = -1f;
            public int maxStack = 1;
            public bool allowDuplicate;

            [Header("Stat Modifier")]
            public string statType;
            public string modifierType;
            public float value;
            public bool isPercent;

            [Header("Heal")]
            public float flatHealAmount;
            public bool useMaxHpPercent;
            public float maxHpPercent;
            public bool useAttackScaling;
            public float attackPercentHeal;
            public bool clampToMaxHp = true;

            [Header("Chance On Heal Stat Modifier")]
            public string triggerTargetType;

            [Header("Status")]
            public float chance = 1f;
            public bool requireCriticalHit;
            public float interval;
            public int count;

            [Header("Movement / Knockback")]
            public float force;
            public float distance;

            [Header("Skill / Cooldown")]
            public float cooldownReduceValue;
            public bool cooldownReducePercent;
            public string reduceType;
            public float reducePercent;
            public float reduceSeconds;
        }

        [Serializable]
        public class HitEffectJson
        {
            public EffectJson effect;
            public string effectSo;
            public string lifetimeType;
            public string categoryType;
            public float duration = -1f;
            public int maxApplyCount = 1;
        }

        [Serializable]
        private class EffectJsonRoot
        {
            public EffectJson effect;
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

            EffectJson effectJson = null;

            try
            {
                effectJson = JsonUtility.FromJson<EffectJson>(json);

                if (effectJson == null || string.IsNullOrEmpty(effectJson.effectId))
                {
                    EffectJsonRoot root = JsonUtility.FromJson<EffectJsonRoot>(json);
                    effectJson = root?.effect;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EffectJsonGenerator] Failed to parse effect json. {ex.Message}");
                return null;
            }

            return CreateOrUpdateEffect(effectJson, outputFolder);
        }

        public static EffectSO CreateOrUpdateEffect(
            EffectJson data,
            string outputFolder)
        {
            if (data == null)
            {
                Debug.LogWarning("[EffectJsonGenerator] EffectJson is null.");
                return null;
            }

            if (string.IsNullOrEmpty(data.effectId))
            {
                Debug.LogWarning("[EffectJsonGenerator] effectId is required.");
                return null;
            }

            if (string.IsNullOrEmpty(data.effectType))
            {
                Debug.LogWarning($"[EffectJsonGenerator] effectType is required. effectId={data.effectId}");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError($"[EffectJsonGenerator] outputFolder is required. effectId={data.effectId}");
                return null;
            }

            outputFolder = outputFolder.Replace("\\", "/");

            EnsureFolder(outputFolder);

            GenerateEffectString(data);

            Type effectType = ResolveEffectSoType(data.effectType);

            if (effectType == null)
            {
                Debug.LogError($"[EffectJsonGenerator] Unsupported effectType: {data.effectType}");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.effectId}.asset";
            EffectSO effect = AssetDatabase.LoadAssetAtPath<EffectSO>(assetPath);
            bool isNewAsset = false;

            if (effect == null)
            {
                effect = ScriptableObject.CreateInstance(effectType) as EffectSO;
                isNewAsset = true;
            }
            else if (effect.GetType() != effectType)
            {
                Debug.LogWarning(
                    $"[EffectJsonGenerator] Existing effect type mismatch. " +
                    $"path={assetPath}, existing={effect.GetType().Name}, requested={effectType.Name}. Existing asset will be updated as-is.");
            }

            if (effect == null)
            {
                Debug.LogError($"[EffectJsonGenerator] Failed to create EffectSO. type={effectType.Name}");
                return null;
            }

            ApplyCommonFields(effect, data);
            ApplyTypedFields(effect, data, outputFolder);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(effect, assetPath);
                Debug.Log($"[EffectJsonGenerator] Created EffectSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(effect);
                Debug.Log($"[EffectJsonGenerator] Updated EffectSO: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return effect;
        }

        public static SkillProjectileHitEffectEntry CreateOrUpdateHitEffectEntry(
            HitEffectJson data,
            string outputFolder)
        {
            SkillProjectileHitEffectEntry entry =
                new SkillProjectileHitEffectEntry();

            if (data == null)
            {
                return entry;
            }

            entry.effectSo = data.effect != null
                ? CreateOrUpdateEffect(data.effect, outputFolder)
                : FindEffectSoByName(data.effectSo);

            if (!string.IsNullOrEmpty(data.lifetimeType))
            {
                entry.lifetimeType =
                    Enum.Parse<EffectLifetimeType>(
                        data.lifetimeType,
                        true);
            }

            if (!string.IsNullOrEmpty(data.categoryType))
            {
                entry.categoryType =
                    Enum.Parse<EffectCategoryType>(
                        data.categoryType,
                        true);
            }

            entry.duration = data.duration;
            entry.maxApplyCount = data.maxApplyCount;

            return entry;
        }

        public static SkillProjectileHitEffectEntry[] CreateOrUpdateHitEffectEntries(
            HitEffectJson[] data,
            string outputFolder)
        {
            if (data == null || data.Length == 0)
            {
                return Array.Empty<SkillProjectileHitEffectEntry>();
            }

            SkillProjectileHitEffectEntry[] result =
                new SkillProjectileHitEffectEntry[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                result[i] = CreateOrUpdateHitEffectEntry(
                    data[i],
                    outputFolder);
            }

            return result;
        }

        public static EffectSO FindEffectSoByName(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{effectName} t:EffectSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EffectSO effect = AssetDatabase.LoadAssetAtPath<EffectSO>(path);

                if (effect != null &&
                    effect.name.Equals(effectName, StringComparison.OrdinalIgnoreCase))
                {
                    return effect;
                }
            }

            Debug.LogWarning($"[EffectJsonGenerator] EffectSO not found: {effectName}");
            return null;
        }

        private static void GenerateEffectString(
            EffectJson data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.effectId) ||
                string.IsNullOrWhiteSpace(data.effectName))
            {
                return;
            }

            ResourceTools.Skill.SkillStringBuilder.ExtractSkillName(
                data.effectId,
                data.effectName);
        }

        private static void ApplyCommonFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.effectId, "effectId", "id");
            SetFirstExistingField(effect, data.effectName, "effectName", "displayName", "title");
            SetFirstExistingField(effect, data.description, "description", "desc");
            SetFirstExistingField(effect, data.duration, "duration", "defaultDuration");
            SetFirstExistingField(effect, data.maxStack, "maxStack", "stackCount", "maxStackCount");
            SetFirstExistingField(effect, data.allowDuplicate, "allowDuplicate", "allowDuplicates", "canDuplicate");

            if (!string.IsNullOrEmpty(data.iconName))
            {
                SetFirstExistingField(effect, FindSpriteByName(data.iconName), "icon", "sprite");
            }
        }

        private static void ApplyTypedFields(
            EffectSO effect,
            EffectJson data,
            string outputFolder)
        {
            string effectType = data.effectType?.Trim();

            switch (effectType)
            {
                case "StatModifier":
                case "StatModifierEffect":
                case "StatModifierEffectSO":
                    ApplyStatModifierFields(effect, data);
                    break;

                case "Heal":
                case "HealEffect":
                case "HealEffectSO":
                    ApplyHealFields(effect, data);
                    break;

                case "Stun":
                case "StunEffect":
                case "StunEffectSO":
                case "Root":
                case "RootEffect":
                case "RootEffectSO":
                    ApplyStatusFields(effect, data);
                    break;

                case "Knockback":
                case "KnockbackEffect":
                case "KnockbackEffectSO":
                    ApplyKnockbackFields(effect, data);
                    break;

                case "CooldownReduce":
                case "CooldownReduceEffect":
                case "CooldownReduceEffectSO":
                    ApplyCooldownReduceFields(effect, data);
                    break;

                case "ChanceOnHitSkill":
                case "ChanceOnHitSkillEffect":
                case "ChanceOnHitSkillEffectSO":
                    ApplyChanceOnHitSkillFields(effect, data, outputFolder);
                    break;

                case "ChanceOnHealStatModifier":
                case "ChanceOnHealStatModifierEffect":
                case "ChanceOnHealStatModifierEffectSO":
                    ApplyChanceOnHealStatModifierFields(effect, data);
                    break;

                case "ChanceOnHealCooldownReduce":
                case "ChanceOnHealCooldownReduceEffect":
                case "ChanceOnHealCooldownReduceEffectSO":
                    ApplyChanceOnHealCooldownReduceFields(effect, data);
                    break;
            }
        }

        private static void ApplyStatModifierFields(
            EffectSO effect,
            EffectJson data)
        {
            SetEnumFirstExistingField(effect, data.statType, "statType", "targetStat", "stat");
            SetEnumFirstExistingField(effect, data.modifierType, "modifierType", "valueType", "modifyType", "operationType");
            SetFirstExistingField(effect, data.value, "value", "amount", "modifierValue");
            SetFirstExistingField(effect, data.isPercent, "isPercent", "usePercent");
        }

        private static void ApplyHealFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.flatHealAmount, "flatHealAmount");
            SetFirstExistingField(effect, data.useMaxHpPercent, "useMaxHpPercent");
            SetFirstExistingField(effect, data.maxHpPercent, "maxHpPercent");
            SetFirstExistingField(effect, data.useAttackScaling, "useAttackScaling");
            SetFirstExistingField(effect, data.attackPercentHeal, "attackPercentHeal");
            SetFirstExistingField(effect, data.clampToMaxHp, "clampToMaxHp");

            SetFirstExistingField(effect, data.value, "value", "amount", "healAmount");
            SetFirstExistingField(effect, data.isPercent, "isPercent", "usePercent", "isPercentHeal");
        }
        private static void ApplyChanceOnHealStatModifierFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.chance, "chance", "chancePercent");
            SetEnumFirstExistingField(effect, data.triggerTargetType, "triggerTargetType");
            SetEnumFirstExistingField(effect, data.statType, "statType", "targetStat", "stat");
            SetEnumFirstExistingField(effect, data.modifierType, "valueType", "modifierType", "modifyType", "operationType");
            SetFirstExistingField(effect, data.value, "value", "amount", "modifierValue");
        }

        private static void ApplyChanceOnHealCooldownReduceFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.chance, "chance", "chancePercent");
            SetEnumFirstExistingField(effect, data.triggerTargetType, "triggerTargetType");
            SetEnumFirstExistingField(effect, data.reduceType, "reduceType");
            SetFirstExistingField(effect, data.reducePercent, "reducePercent");
            SetFirstExistingField(effect, data.reduceSeconds, "reduceSeconds");
        }

        private static void ApplyStatusFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.duration, "duration", "statusDuration");
            SetFirstExistingField(effect, data.chance, "chance", "applyChance");
            SetFirstExistingField(effect, data.maxStack, "maxStack", "maxStackCount");
        }

        private static void ApplyKnockbackFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.force, "force", "knockbackForce");
            SetFirstExistingField(effect, data.distance, "distance", "knockbackDistance");
        }

        private static void ApplyCooldownReduceFields(
            EffectSO effect,
            EffectJson data)
        {
            SetFirstExistingField(effect, data.cooldownReduceValue, "cooldownReduceValue", "value", "amount");
            SetFirstExistingField(effect, data.cooldownReducePercent, "cooldownReducePercent", "isPercent", "usePercent");
            SetEnumFirstExistingField(effect, data.reduceType, "reduceType");
            SetFirstExistingField(effect, data.reducePercent, "reducePercent");
            SetFirstExistingField(effect, data.reduceSeconds, "reduceSeconds");
        }

        private static void ApplyChanceOnHitSkillFields(
            EffectSO effect,
            EffectJson data,
            string effectOutputFolder)
        {
            SetFirstExistingField(effect, data.chance, "chance", "chancePercent");
            SetFirstExistingField(effect, data.requireCriticalHit, "requireCriticalHit");

            if (data.skill == null)
            {
                return;
            }

            string skillOutputFolder = ResolveGeneratedSkillOutputFolder(
                data,
                effectOutputFolder);

            if (string.IsNullOrWhiteSpace(skillOutputFolder))
            {
                Debug.LogError($"[EffectJsonGenerator] Trigger skill output folder is empty. effectId={data.effectId}");
                return;
            }

            Debug.Log(
                $"[EffectJsonGenerator] TriggerSkillOutputFolder effectId={data.effectId} outputFolder={skillOutputFolder}");

            EquipmentSkillSO skillSo =
                Skill.EquipmentSkillJsonGenerator.CreateOrUpdateSkill(
                    data.skill,
                    skillOutputFolder);

            SetFirstExistingField(effect, skillSo, "skillSo", "skillSO", "skill");
        }

        private static string ResolveGeneratedSkillOutputFolder(
            EffectJson data,
            string effectOutputFolder)
        {
            string normalizedEffectOutputFolder =
                string.IsNullOrWhiteSpace(effectOutputFolder)
                    ? null
                    : effectOutputFolder.Replace("\\", "/");

            if (data != null && !string.IsNullOrWhiteSpace(data.skillOutputFolder))
            {
                return ResolveOutputFolderPath(
                    data.skillOutputFolder,
                    normalizedEffectOutputFolder);
            }

            return normalizedEffectOutputFolder;
        }

        private static string ResolveOutputFolderPath(
            string folderPath,
            string baseFolder)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return baseFolder;
            }

            folderPath = folderPath.Replace("\\", "/");

            if (folderPath == "." || folderPath.Equals("same", StringComparison.OrdinalIgnoreCase))
            {
                return baseFolder?.Replace("\\", "/");
            }

            if (folderPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return folderPath;
            }

            if (string.IsNullOrWhiteSpace(baseFolder))
            {
                return folderPath;
            }

            return $"{baseFolder.Replace("\\", "/")}/{folderPath}";
        }


        private static Type ResolveEffectSoType(string effectType)
        {
            string typeName = ResolveEffectSoTypeName(effectType);
            return FindTypeByName(typeName);
        }

        private static string ResolveEffectSoTypeName(string effectType)
        {
            switch (effectType?.Trim())
            {
                case "StatModifier":
                case "StatModifierEffect":
                    return "StatModifierEffectSO";

                case "Heal":
                case "HealEffect":
                    return "HealEffectSO";

                case "Stun":
                case "StunEffect":
                    return "StunEffectSO";

                case "Root":
                case "RootEffect":
                    return "RootEffectSO";

                case "Knockback":
                case "KnockbackEffect":
                    return "KnockbackEffectSO";

                case "CooldownReduce":
                case "CooldownReduceEffect":
                    return "CooldownReduceEffectSO";

                case "ChanceOnHitSkill":
                case "ChanceOnHitSkillEffect":
                    return "ChanceOnHitSkillEffectSO";

                case "ChanceOnHealStatModifier":
                case "ChanceOnHealStatModifierEffect":
                    return "ChanceOnHealStatModifierEffectSO";

                case "ChanceOnHealCooldownReduce":
                case "ChanceOnHealCooldownReduceEffect":
                    return "ChanceOnHealCooldownReduceEffectSO";

                default:
                    return effectType;
            }
        }

        private static Type FindTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null)
                {
                    continue;
                }

                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];

                    if (type == null)
                    {
                        continue;
                    }

                    if (type.Name == typeName && typeof(EffectSO).IsAssignableFrom(type))
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        private static void SetFirstExistingField(
            object target,
            object value,
            params string[] fieldNames)
        {
            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo field = FindField(target.GetType(), fieldNames[i]);

                if (field == null)
                {
                    continue;
                }

                object convertedValue = ConvertValue(value, field.FieldType);
                field.SetValue(target, convertedValue);
                return;
            }
        }

        private static void SetEnumFirstExistingField(
            object target,
            string enumValue,
            params string[] fieldNames)
        {
            if (string.IsNullOrEmpty(enumValue))
            {
                return;
            }

            for (int i = 0; i < fieldNames.Length; i++)
            {
                FieldInfo field = FindField(target.GetType(), fieldNames[i]);

                if (field == null || !field.FieldType.IsEnum)
                {
                    continue;
                }

                if (!Enum.TryParse(
                        field.FieldType,
                        enumValue,
                        true,
                        out object parsedValue))
                {
                    continue;
                }

                field.SetValue(
                    target,
                    parsedValue);
                return;
            }
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (targetType.IsEnum && value is string enumText)
            {
                return Enum.Parse(targetType, enumText, true);
            }

            return Convert.ChangeType(value, targetType);
        }

        private static Sprite FindSpriteByName(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null &&
                    sprite.name.Equals(spriteName, StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            return null;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            string leaf = Path.GetFileName(folder);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(leaf))
            {
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}