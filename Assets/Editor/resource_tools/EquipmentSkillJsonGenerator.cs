#if UNITY_EDITOR
using System;
using System.IO;
using Skills;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class EquipmentSkillJsonGenerator
    {
        // New JSON model (2024-06):
        [Serializable]
        private class EquipmentSkillJson
        {
            public string equipmentId;
            public string slotName;
            public string skillName;
            public string iconName;

            public BaseProfileJson baseProfile;
            public CastJson cast;
            public MoveJson move;
            public HitJson hit;
        }

        [Serializable]
        private class BaseProfileJson
        {
            public string profileId;

            public string attackArchetype;

            public string projectilePrefabName;
            public float projectileSpawnOffset;

            public int projectileCount;
            public float projectileScale;
            public float projectileLifetime;

            public float projectileSpawnInterval;
            public float projectileSpawnRadius;

            public string category;
            public string targetType;
            public string tacticalNeed;
            public float basePriority;
        }

        [Serializable]
        private class CastJson
        {
            public string castId;

            public float cooldown;
            public float castTime;
            public float range;

            public int burstCount;
            public float burstInterval;

            public string castType;
            public string targetingType;

            public bool canMoveWhileCasting;
            public bool canCancelCasting;
            public bool autoCast;
        }

        [Serializable]
        private class MoveJson
        {
            public string moveId;
            public string moveType;

            public float speed;
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
        }

        [Serializable]
        private class DamageJson
        {
            public string skillId;
            public string damageType;
            public float baseDamage;
            public float attackPercentDamage;
            public bool canCritical;
            public bool ignoreDefense;
        }

        [Serializable]
        private class HitJson
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

            public DamageJson damage;

            public bool useSplitMultiHitDamage;
            public int splitHitCount;
            public float splitHitInterval;

            public bool useKnockback;
            public float knockbackForce;
        }

        // Example JSON format:
        // {
        //   "equipmentId": "basic_attack",
        //   "characterName": "military_officer",
        //   "slotName": "basic",
        //   "skillName": "basic_attack",
        //   "iconName": "basic_attack"
        // }
        // Usage:
        // Select a json file in the Project window.
        // Right Click -> Skill -> Generate EquipmentSkillSO From Json
        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", false, 2000)]
        public static void Generate()
        {
            TextAsset jsonAsset = Selection.activeObject as TextAsset;

            if (jsonAsset == null)
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Select a json file in the Project window first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(jsonAsset);
            GenerateFromJsonPath(jsonPath);
        }

        public static EquipmentSkillSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Selected asset is not a json file.");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            EquipmentSkillJson data = JsonUtility.FromJson<EquipmentSkillJson>(json);

            if (data == null || string.IsNullOrEmpty(data.equipmentId))
            {
                Debug.LogError($"[EquipmentSkillJsonGenerator] Invalid EquipmentSkill json: {jsonPath}");
                return null;
            }

            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Cannot resolve output folder from json path.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.equipmentId}.asset";
            EquipmentSkillSO skillSo = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(assetPath);
            bool isNewAsset = false;

            EquipmentBaseProfileSO baseProfileSo =
                CreateOrUpdateBaseProfile(data.baseProfile, outputFolder);

            SkillCastSO castSo =
                CreateOrUpdateCast(data.cast, outputFolder);

            SkillMoveSO moveSo =
                CreateOrUpdateMove(data.move, outputFolder);

            SkillHitSO hitSo =
                CreateOrUpdateHit(data.hit, outputFolder);

            if (skillSo == null)
            {
                skillSo = ScriptableObject.CreateInstance<EquipmentSkillSO>();
                isNewAsset = true;
            }

            SetField(skillSo, "equipmentId", data.equipmentId);
            SetField(skillSo, "icon", FindSpriteById(data.iconName));
            SetField(skillSo, "baseProfileSo", baseProfileSo);
            SetField(skillSo, "castSo", castSo);
            SetField(skillSo, "moveSo", moveSo);
            SetField(skillSo, "hitSo", hitSo);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(skillSo, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created EquipmentSkillSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(skillSo);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated EquipmentSkillSO: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return skillSo;
        }

        private static SkillHitSO CreateOrUpdateHit(
            HitJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrEmpty(data.hitId))
            {
                Debug.LogWarning("[EquipmentSkillJsonGenerator] HitJson is empty. hitSo will not be assigned.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.hitId}.asset";
            SkillHitSO hit = AssetDatabase.LoadAssetAtPath<SkillHitSO>(assetPath);

            bool isNewAsset = false;

            if (hit == null)
            {
                hit = ScriptableObject.CreateInstance<SkillHitSO>();
                isNewAsset = true;
            }

            SkillDamageSO damageSo = CreateOrUpdateDamage(data.damage, outputFolder);

            SetField(hit, "maxHitCount", data.maxHitCount);
            SetField(hit, "ignoreSameRoot", data.ignoreSameRoot);
            SetField(hit, "useRepeatInterval", data.useRepeatInterval);
            SetField(hit, "repeatInterval", data.repeatInterval);

            SetField(hit, "useHitWindow", data.useHitWindow);
            SetField(hit, "hitStartTime", data.hitStartTime);
            SetField(hit, "hitDuration", data.hitDuration);
            SetField(hit, "deactivateAfterFirstHit", data.deactivateAfterFirstHit);

            SetField(hit, "targetLayerMask", ToLayerMask(data.targetLayerMask));
            SetField(hit, "damageSo", damageSo);

            SetField(hit, "useSplitMultiHitDamage", data.useSplitMultiHitDamage);
            SetField(hit, "splitHitCount", data.splitHitCount);
            SetField(hit, "splitHitInterval", data.splitHitInterval);

            SetField(hit, "useKnockback", data.useKnockback);
            SetField(hit, "knockbackForce", data.knockbackForce);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(hit, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created SkillHitSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(hit);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated SkillHitSO: {assetPath}");
            }

            return hit;
        }

        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", true)]
        private static bool ValidateGenerate()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        private static EquipmentBaseProfileSO CreateOrUpdateBaseProfile(
            BaseProfileJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrEmpty(data.profileId))
            {
                Debug.LogWarning("[EquipmentSkillJsonGenerator] BaseProfileJson is empty. baseProfileSo will not be assigned.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.profileId}.asset";
            EquipmentBaseProfileSO profile =
                AssetDatabase.LoadAssetAtPath<EquipmentBaseProfileSO>(assetPath);

            bool isNewAsset = false;

            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EquipmentBaseProfileSO>();
                isNewAsset = true;
            }

            SetField(profile, "profileId", data.profileId);
            SetEnumField(profile, "attackArchetype", data.attackArchetype);

            SetField(profile, "projectilePrefab", FindProjectilePrefabByName(data.projectilePrefabName));
            SetField(profile, "projectileSpawnOffset", data.projectileSpawnOffset);

            SetField(profile, "projectileCount", data.projectileCount);
            SetField(profile, "projectileScale", data.projectileScale);
            SetField(profile, "projectileLifetime", data.projectileLifetime);

            SetField(profile, "projectileSpawnInterval", data.projectileSpawnInterval);
            SetField(profile, "projectileSpawnRadius", data.projectileSpawnRadius);

            SetEnumField(profile, "category", data.category);
            SetEnumField(profile, "targetType", data.targetType);
            SetEnumField(profile, "tacticalNeed", data.tacticalNeed);
            SetField(profile, "basePriority", data.basePriority);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(profile, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created EquipmentBaseProfileSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(profile);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated EquipmentBaseProfileSO: {assetPath}");
            }

            return profile;
        }

        private static SkillCastSO CreateOrUpdateCast(
            CastJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrEmpty(data.castId))
            {
                Debug.LogWarning("[EquipmentSkillJsonGenerator] CastJson is empty. castSo will not be assigned.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.castId}.asset";
            SkillCastSO cast = AssetDatabase.LoadAssetAtPath<SkillCastSO>(assetPath);

            bool isNewAsset = false;

            if (cast == null)
            {
                cast = ScriptableObject.CreateInstance<SkillCastSO>();
                isNewAsset = true;
            }

            SetField(cast, "cooldown", data.cooldown);
            SetField(cast, "castTime", data.castTime);
            SetField(cast, "range", data.range);

            SetField(cast, "burstCount", data.burstCount);
            SetField(cast, "burstInterval", data.burstInterval);

            SetEnumField(cast, "castType", data.castType);
            SetEnumField(cast, "targetingType", data.targetingType);

            SetField(cast, "canMoveWhileCasting", data.canMoveWhileCasting);
            SetField(cast, "canCancelCasting", data.canCancelCasting);
            SetField(cast, "autoCast", data.autoCast);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(cast, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created SkillCastSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(cast);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated SkillCastSO: {assetPath}");
            }

            return cast;
        }

        private static SkillMoveSO CreateOrUpdateMove(
            MoveJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrEmpty(data.moveId))
            {
                Debug.LogWarning("[EquipmentSkillJsonGenerator] MoveJson is empty. moveSo will not be assigned.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.moveId}.asset";
            SkillMoveSO move = AssetDatabase.LoadAssetAtPath<SkillMoveSO>(assetPath);

            bool isNewAsset = false;

            if (move == null)
            {
                move = ScriptableObject.CreateInstance<SkillMoveSO>();
                isNewAsset = true;
            }

            SetField(move, "moveId", data.moveId);
            SetEnumField(move, "moveType", data.moveType);

            SetField(move, "speed", data.speed);
            SetField(move, "arrivalThreshold", data.arrivalThreshold);

            SetField(move, "applyDirectionRotation", data.applyDirectionRotation);
            SetField(move, "rotationOffset", data.rotationOffset);

            SetField(move, "followOffset", new Vector2(data.followOffsetX, data.followOffsetY));
            SetField(move, "followLerpSpeed", data.followLerpSpeed);
            SetField(move, "snapOnInitialize", data.snapOnInitialize);

            SetField(move, "useHoverMotion", data.useHoverMotion);
            SetField(move, "hoverAmplitude", data.hoverAmplitude);
            SetField(move, "hoverFrequency", data.hoverFrequency);
            SetField(move, "hoverAxis", new Vector2(data.hoverAxisX, data.hoverAxisY));

            SetField(move, "endWhenOwnerMissing", data.endWhenOwnerMissing);

            SetField(move, "orbitRadius", data.orbitRadius);
            SetField(move, "orbitAngularSpeed", data.orbitAngularSpeed);
            SetField(move, "clockwise", data.clockwise);

            SetField(move, "spawnOrder", data.spawnOrder);
            SetField(move, "maxProjectileCount", data.maxProjectileCount);
            SetField(move, "resetPhaseWhenLayoutChanges", data.resetPhaseWhenLayoutChanges);

            SetField(move, "useRadialPulse", data.useRadialPulse);
            SetField(move, "radialPulseAmplitude", data.radialPulseAmplitude);
            SetField(move, "radialPulseFrequency", data.radialPulseFrequency);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(move, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created SkillMoveSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(move);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated SkillMoveSO: {assetPath}");
            }

            return move;
        }

        private static SkillDamageSO CreateOrUpdateDamage(
            DamageJson data,
            string outputFolder)
        {
            if (data == null || string.IsNullOrEmpty(data.skillId))
            {
                Debug.LogWarning("[EquipmentSkillJsonGenerator] DamageJson is empty. damageSo will not be assigned.");
                return null;
            }

            string assetPath = $"{outputFolder}/{data.skillId}.asset";
            SkillDamageSO damage = AssetDatabase.LoadAssetAtPath<SkillDamageSO>(assetPath);

            bool isNewAsset = false;

            if (damage == null)
            {
                damage = ScriptableObject.CreateInstance<SkillDamageSO>();
                isNewAsset = true;
            }

            SetField(damage, "skillId", data.skillId);
            SetEnumField(damage, "damageType", data.damageType);
            SetField(damage, "baseDamage", data.baseDamage);
            SetField(damage, "attackPercentDamage", data.attackPercentDamage);
            SetField(damage, "canCritical", data.canCritical);
            SetField(damage, "ignoreDefense", data.ignoreDefense);

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(damage, assetPath);
                Debug.Log($"[EquipmentSkillJsonGenerator] Created SkillDamageSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(damage);
                Debug.Log($"[EquipmentSkillJsonGenerator] Updated SkillDamageSO: {assetPath}");
            }

            return damage;
        }

        private static Sprite FindSpriteById(string iconId)
        {
            if (string.IsNullOrEmpty(iconId))
            {
                return null;
            }

            Debug.Log($"[EquipmentSkillJsonGenerator] Find icon sprite: {iconId}");
            string[] guids = AssetDatabase.FindAssets($"{iconId} t:Sprite");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null && sprite.name.Equals(iconId, StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            Debug.LogWarning($"[EquipmentSkillJsonGenerator] Icon sprite not found: {iconId}");
            return null;
        }

        private static ProjectileEntity FindProjectilePrefabByName(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
            {
                return null;
            }

            Debug.Log($"[EquipmentSkillJsonGenerator] Find projectile prefab: {prefabName}");

            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null || !prefab.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ProjectileEntity projectile = prefab.GetComponent<ProjectileEntity>();

                if (projectile != null)
                {
                    return projectile;
                }
            }

            Debug.LogWarning($"[EquipmentSkillJsonGenerator] Projectile prefab not found: {prefabName}");
            return null;
        }

        private static LayerMask ToLayerMask(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return default;
            }

            int mask = LayerMask.GetMask(layerName);

            if (mask == 0)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Layer not found or empty mask: {layerName}");
            }

            return mask;
        }

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                return;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Field not found: {fieldName}");
                return;
            }

            field.SetValue(target, value);
        }

        private static void SetEnumField(object target, string fieldName, string enumName)
        {
            if (target == null || string.IsNullOrEmpty(enumName))
            {
                return;
            }

            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);

            if (field == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Field not found: {fieldName}");
                return;
            }

            if (!field.FieldType.IsEnum)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Field is not enum: {fieldName}");
                return;
            }

            try
            {
                object parsedValue = Enum.Parse(field.FieldType, enumName, true);
                field.SetValue(target, parsedValue);
            }
            catch
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Failed to parse enum {fieldName}: {enumName}");
            }
        }
    }
}
#endif