#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    public static class EquipmentSkillJsonGenerator
    {
        // New JSON model (2024-06):
        [Serializable]
        public class EquipmentSkillJson
        {
            public string equipmentId;
            public string skillName;

            public string baseProfile;
            public string cast;
            public string move;
            public string hits;
            public string spawnSkill;
            public string spawn;
            public string upgrade;
            public string baseVisual;
        }

        [Serializable]
        private class EquipmentSkillRootJson
        {
            public string equipmentId;
            public string skillName;
        }


        // Example JSON format:
        // {
        //   "equipmentId": "skill.military_officer.1.passive_1",
        //   "characterName": "military_officer",
        //   "skillName": "basic_attack",
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
            EquipmentSkillJson data = ParseEquipmentSkillJson(json);

            BaseVisualJson baseVisual = ParseObject<BaseVisualJson>(data?.baseVisual);
            if (baseVisual != null)
            {
                Debug.Log($"[EquipmentSkillJsonGenerator] BaseVisual detected: {baseVisual.visualId}");
            }

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

            return CreateOrUpdateSkill(
                data,
                outputFolder,
                jsonPath);
        }

        public static EquipmentSkillSO CreateOrUpdateSkill(
            EquipmentSkillJson data,
            string outputFolder,
            string sourceJsonPath = null)
        {
            if (data == null || string.IsNullOrEmpty(data.equipmentId))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] Invalid EquipmentSkill json data.");
                return null;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[EquipmentSkillJsonGenerator] outputFolder is empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            GenerateSkillString(data);

            BaseProfileJson baseProfile = ParseBaseProfile(data.baseProfile);
            CastJson cast = ParseCast(data.cast);
            MoveJson move = ParseObject<MoveJson>(data.move);
            HitJson[] hits = ParseHitArray(data.hits);
            SpawnSkillJson spawnSkill = ParseObject<SpawnSkillJson>(data.spawnSkill);
            SpawnSkillJson spawn = ParseObject<SpawnSkillJson>(data.spawn);
            SkillUpgradeAsssetBuilder.SkillUpgradeTableJson upgrade =
                ParseObject<SkillUpgradeAsssetBuilder.SkillUpgradeTableJson>(data.upgrade);
            BaseVisualJson baseVisual = ParseObject<BaseVisualJson>(data.baseVisual);

            string assetPath = $"{outputFolder}/{data.equipmentId}.asset";
            EquipmentSkillSO skillSo = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(assetPath);
            bool isNewAsset = false;

            EquipmentBaseProfileSO baseProfileSo =
                HasBaseProfile(baseProfile)
                    ? EquipmentBaseProfileAssetBuilder.CreateOrUpdate(
                        baseProfile,
                        outputFolder)
                    : null;

            SkillCastSO castSo =
                HasCast(cast)
                    ? SkillCastAssetBuilder.CreateOrUpdate(
                        cast,
                        outputFolder) as SkillCastSO
                    : null;

            SkillMoveSO moveSo =
                HasMove(move)
                    ? SkillMoveAssetBuilder.CreateOrUpdate(
                        move,
                        outputFolder) as SkillMoveSO
                    : null;

            SkillHitSO[] hitSos =
                HasHits(hits)
                    ? CreateOrUpdateHits(
                        hits,
                        outputFolder)
                    : Array.Empty<SkillHitSO>();

            // Support both spawnSkill and spawn for spawn skill JSON
            SpawnSkillJson resolvedSpawnSkill = spawnSkill ?? spawn;

            SpawnSkillSO spawnSkillSo =
                SkillSpawnAssetBuilder.HasSpawnSkill(resolvedSpawnSkill)
                    ? SkillSpawnAssetBuilder.CreateOrUpdate(
                        resolvedSpawnSkill,
                        outputFolder)
                    : null;

            EquipmentUpgradeTableSO upgradeTableSo =
                upgrade != null
                    ? SkillUpgradeAsssetBuilder.CreateOrUpdate(
                        upgrade,
                        sourceJsonPath)
                    : null;

            BaseVisualSO baseVisualSo =
                HasBaseVisual(baseVisual)
                    ? SkillBaseVisualAssetBuilder.CreateOrUpdate(
                        baseVisual,
                        outputFolder,
                        ShouldGenerateSkillAnimation(data.equipmentId, cast))
                    : null;

            if (skillSo == null)
            {
                skillSo = ScriptableObject.CreateInstance<EquipmentSkillSO>();
                isNewAsset = true;
            }

            ApplySkillFields(
                skillSo,
                data,
                baseProfileSo,
                castSo,
                moveSo,
                hitSos,
                spawnSkillSo,
                upgradeTableSo,
                baseVisualSo);

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

            AssetDatabase.SaveAssetIfDirty(skillSo);
            AssetDatabase.SaveAssets();

            return skillSo;
        }


        private static bool HasBaseProfile(
            BaseProfileJson baseProfile)
        {
            return baseProfile != null &&
                   !string.IsNullOrWhiteSpace(baseProfile.baseProfileId);
        }

        private static bool ShouldGenerateSkillAnimation(
            string equipmentId,
            CastJson cast)
        {
            const float meleeBasicAttackMaxRange = 1f;

            bool isBasicAttack = !string.IsNullOrWhiteSpace(equipmentId)
                && equipmentId.IndexOf(
                    ".basic_attack.",
                    StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMeleeRange = cast != null
                && cast.range <= meleeBasicAttackMaxRange;

            return !isBasicAttack || !isMeleeRange;
        }

        private static bool HasCast(
            CastJson cast)
        {
            return cast != null &&
                   !string.IsNullOrWhiteSpace(cast.castId);
        }

        private static bool HasMove(
            MoveJson move)
        {
            return move != null &&
                   !string.IsNullOrWhiteSpace(move.moveId);
        }

        private static bool HasBaseVisual(
            BaseVisualJson baseVisual)
        {
            return baseVisual != null &&
                   !string.IsNullOrWhiteSpace(baseVisual.visualId);
        }


        private static bool HasHits(
            HitJson[] hits)
        {
            if (hits == null || hits.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null &&
                    !string.IsNullOrWhiteSpace(hits[i].hitId))
                {
                    return true;
                }
            }

            return false;
        }

        private static void GenerateSkillString(
            EquipmentSkillJson data)
        {
            if (data == null ||
                string.IsNullOrWhiteSpace(data.equipmentId) ||
                string.IsNullOrWhiteSpace(data.skillName))
            {
                return;
            }

            SkillStringBuilder.ExtractSkillName(
                data.equipmentId,
                data.skillName);
        }

        private static void ApplySkillFields(
            EquipmentSkillSO skillSo,
            EquipmentSkillJson data,
            EquipmentBaseProfileSO baseProfileSo,
            SkillCastSO castSo,
            SkillMoveSO moveSo,
            SkillHitSO[] hitSos,
            SpawnSkillSO spawnSkillSo,
            EquipmentUpgradeTableSO upgradeTableSo,
            BaseVisualSO baseVisualSo)
        {
            SerializedObject serializedObject = new SerializedObject(skillSo);

            SetString(serializedObject, "equipmentId", data.equipmentId);
            SetObjectReference(
                serializedObject,
                "icon",
                FindSpriteById($"{data.equipmentId}.icon"));
            SetObjectReference(serializedObject, "baseProfileSo", baseProfileSo);
            SetObjectReference(serializedObject, "castSo", castSo);
            SetObjectReference(serializedObject, "moveSo", moveSo);
            SetObjectArray(serializedObject, "hitSos", hitSos);
            SetObjectReference(serializedObject, "spawnSkillSo", spawnSkillSo);
            SetObjectReference(serializedObject, "upgradeTableSo", upgradeTableSo);
            SetObjectReference(serializedObject, "baseVisualSo", baseVisualSo);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }


        private static void SetString(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            property.stringValue = value;
        }


        private static void SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            property.objectReferenceValue = value;
        }

        private static void SetObjectArray<T>(
            SerializedObject serializedObject,
            string propertyName,
            T[] values)
            where T : UnityEngine.Object
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property not found: {propertyName}");
                return;
            }

            if (!property.isArray)
            {
                Debug.LogWarning($"[EquipmentSkillJsonGenerator] Serialized property is not array: {propertyName}");
                return;
            }

            int length = values != null ? values.Length : 0;
            property.arraySize = length;

            for (int i = 0; i < length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }


        private static SkillHitSO[] CreateOrUpdateHits(
            HitJson[] hits,
            string outputFolder)
        {
            if (hits == null || hits.Length == 0)
            {
                return Array.Empty<SkillHitSO>();
            }

            List<SkillHitSO> result = new();

            for (int i = 0; i < hits.Length; i++)
            {
                HitJson hitJson = hits[i];
                if (hitJson == null ||
                    string.IsNullOrWhiteSpace(hitJson.hitId))
                {
                    continue;
                }

                SkillHitSO hitSo = SkillHitAssetBuilder.CreateOrUpdate(
                    hitJson,
                    outputFolder) as SkillHitSO;

                if (hitSo == null)
                {
                    Debug.LogError(
                        $"[EquipmentSkillJsonGenerator] Failed to create hit asset. hitId={hitJson.hitId}");
                    continue;
                }

                result.Add(hitSo);
            }

            return result.Count > 0
                ? result.ToArray()
                : Array.Empty<SkillHitSO>();
        }

        [MenuItem("Assets/Skill/Generate EquipmentSkillSO From Json", true)]
        private static bool ValidateGenerate()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
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

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            folder = folder.Replace("\\", "/");

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
        private static EquipmentSkillJson ParseEquipmentSkillJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            EquipmentSkillRootJson root = JsonUtility.FromJson<EquipmentSkillRootJson>(json);
            if (root == null)
            {
                return null;
            }

            EquipmentSkillJson data = new EquipmentSkillJson
            {
                equipmentId = root.equipmentId,
                skillName = root.skillName,
                baseProfile = ExtractJsonValue(json, "baseProfile"),
                cast = ExtractJsonValue(json, "cast"),
                move = ExtractJsonValue(json, "move"),
                hits = ExtractJsonValue(json, "hits"),
                spawnSkill = ExtractJsonValue(json, "spawnSkill"),
                spawn = ExtractJsonValue(json, "spawn"),
                upgrade = ExtractJsonValue(json, "upgradeTable"),
                baseVisual = ExtractJsonValue(json, "baseVisual")
            };

            return data;
        }

        private static BaseProfileJson ParseBaseProfile(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            BaseProfileJson baseProfile = JsonUtility.FromJson<BaseProfileJson>(json);
            if (baseProfile != null)
            {
                baseProfile.projectile = ExtractJsonValue(json, "projectile");
                baseProfile.projectileSpawn = ExtractJsonValue(json, "projectileSpawn");
                baseProfile.brainMeta = ExtractJsonValue(json, "brainMeta");
            }

            return baseProfile;
        }

        private static CastJson ParseCast(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            CastJson cast = JsonUtility.FromJson<CastJson>(json);
            if (cast != null)
            {
                cast.burst = ExtractJsonValue(json, "burst");
                cast.castMove = ExtractJsonValue(json, "castMove");
                cast.selfEffects = ExtractJsonValue(json, "selfEffects");
            }

            return cast;
        }

        private static HitJson[] ParseHitArray(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            string[] itemJsons = ExtractJsonArrayItems(json);
            if (itemJsons == null || itemJsons.Length == 0)
            {
                return Array.Empty<HitJson>();
            }

            HitJson[] result = new HitJson[itemJsons.Length];
            for (int i = 0; i < itemJsons.Length; i++)
            {
                string itemJson = itemJsons[i];
                HitJson hitJson = JsonUtility.FromJson<HitJson>(itemJson);

                if (hitJson != null)
                {
                    hitJson.damage = ExtractJsonValue(itemJson, "damage");
                    hitJson.buffEffects = ExtractJsonValue(itemJson, "buffEffects");
                    hitJson.debuffEffects = ExtractJsonValue(itemJson, "debuffEffects");
                    hitJson.split = ExtractJsonValue(itemJson, "split");
                }

                result[i] = hitJson;
            }

            return result;
        }

        private static string[] ExtractJsonArrayItems(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            int startIndex = json.IndexOf('[');
            int endIndex = json.LastIndexOf(']');
            if (startIndex < 0 || endIndex <= startIndex)
            {
                return null;
            }

            List<string> items = new();
            int index = startIndex + 1;

            while (index < endIndex)
            {
                while (index < endIndex &&
                       (char.IsWhiteSpace(json[index]) || json[index] == ','))
                {
                    index++;
                }

                if (index >= endIndex)
                {
                    break;
                }

                if (json[index] != '{')
                {
                    break;
                }

                string item = ExtractBalancedJson(json, index, '{', '}');
                if (string.IsNullOrWhiteSpace(item))
                {
                    break;
                }

                items.Add(item);
                index += item.Length;
            }

            return items.ToArray();
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

            ArrayWrapper<T> wrapper = JsonUtility.FromJson<ArrayWrapper<T>>($"{{\"items\":{json}}}");
            return wrapper != null
                ? wrapper.items
                : null;
        }

        [Serializable]
        private class ArrayWrapper<T>
        {
            public T[] items;
        }

        private static string ExtractJsonValue(string json, string propertyName)
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

            char startChar = json[valueStart];
            if (startChar == '{')
            {
                return ExtractBalancedJson(json, valueStart, '{', '}');
            }

            if (startChar == '[')
            {
                return ExtractBalancedJson(json, valueStart, '[', ']');
            }

            return null;
        }

        private static string ExtractBalancedJson(
            string json,
            int startIndex,
            char openChar,
            char closeChar)
        {
            int depth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = startIndex; i < json.Length; i++)
            {
                char current = json[i];

                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (current == '\\')
                {
                    escape = true;
                    continue;
                }

                if (current == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                {
                    continue;
                }

                if (current == openChar)
                {
                    depth++;
                }
                else if (current == closeChar)
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
    }
}
#endif
