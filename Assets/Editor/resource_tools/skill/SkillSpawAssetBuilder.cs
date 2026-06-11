

using System;
using System.IO;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    [Serializable]
    public class SpawnSkillJson
    {
        public string spawnSkillId;
        public string timing;
        public string position;
        public int spawnCount = 1;
        public float spawnInterval;
        public float duration;
        public string characterSO;
        public EquipmentSkillJsonGenerator.EquipmentSkillJson skill;
    }

    public static class SkillSpawnAssetBuilder
    {
        public static bool HasSpawnSkill(
            SpawnSkillJson spawnSkill)
        {
            if (spawnSkill == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(spawnSkill.spawnSkillId))
            {
                return true;
            }

            if (spawnSkill.skill != null &&
                !string.IsNullOrWhiteSpace(spawnSkill.skill.equipmentId))
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(spawnSkill.characterSO);
        }

        public static SpawnSkillSO CreateOrUpdate(
            SpawnSkillJson spawnJson,
            string outputFolder,
            Func<EquipmentSkillJsonGenerator.EquipmentSkillJson, string, EquipmentSkillSO> createSkill)
        {
            if (spawnJson == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(spawnJson.spawnSkillId))
            {
                Debug.LogWarning("[SkillSpawnAssetBuilder] spawnSkillId is empty. SpawnSkillSO will not be created.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetPath = $"{outputFolder}/{SanitizeFileName(spawnJson.spawnSkillId)}.asset";
            SpawnSkillSO spawnSkillSo = AssetDatabase.LoadAssetAtPath<SpawnSkillSO>(assetPath);

            if (spawnSkillSo == null)
            {
                spawnSkillSo = ScriptableObject.CreateInstance<SpawnSkillSO>();
                AssetDatabase.CreateAsset(spawnSkillSo, assetPath);
            }

            EquipmentSkillSO childSkillSo = CreateChildSkill(
                spawnJson,
                outputFolder,
                createSkill);

            Apply(
                spawnSkillSo,
                spawnJson,
                childSkillSo);

            EditorUtility.SetDirty(spawnSkillSo);
            AssetDatabase.SaveAssetIfDirty(spawnSkillSo);

            return spawnSkillSo;
        }

        private static EquipmentSkillSO CreateChildSkill(
            SpawnSkillJson spawnJson,
            string outputFolder,
            Func<EquipmentSkillJsonGenerator.EquipmentSkillJson, string, EquipmentSkillSO> createSkill)
        {
            if (spawnJson == null ||
                spawnJson.skill == null ||
                string.IsNullOrWhiteSpace(spawnJson.skill.equipmentId) ||
                createSkill == null)
            {
                return null;
            }

            string childOutputFolder = $"{outputFolder}/spawn_skills";
            EnsureFolder(childOutputFolder);

            return createSkill(
                spawnJson.skill,
                childOutputFolder);
        }

        private static void Apply(
            SpawnSkillSO spawnSkillSo,
            SpawnSkillJson spawnJson,
            EquipmentSkillSO childSkillSo)
        {
            SerializedObject serializedObject = new SerializedObject(spawnSkillSo);

            SetEnum(serializedObject, "timing", spawnJson.timing);
            SetEnum(serializedObject, "position", spawnJson.position);
            SetInt(serializedObject, "spawnCount", Mathf.Max(1, spawnJson.spawnCount));
            SetFloat(serializedObject, "spawnInterval", Mathf.Max(0f, spawnJson.spawnInterval));
            SetFloat(serializedObject, "duration", Mathf.Max(0f, spawnJson.duration));
            SetObjectReference(serializedObject, "characterSO", FindCharacterSO(spawnJson.characterSO));
            SetObjectReference(serializedObject, "skill", childSkillSo);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(
            SerializedObject serializedObject,
            string propertyName,
            string value)
        {
            SerializedProperty property = FindRequiredProperty(
                serializedObject,
                propertyName);

            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (property.propertyType != SerializedPropertyType.Enum)
            {
                throw new InvalidOperationException(
                    $"[SkillSpawnAssetBuilder] Property is not enum: {propertyName} type={property.propertyType}");
            }

            if (propertyName == "position")
            {
                switch (value)
                {
                    case "CasterAround":
                    case "AroundCaster":
                        value = "Caster";
                        break;

                    case "TargetAround":
                    case "AroundTarget":
                        value = "Target";
                        break;

                    case "Projectile":
                        value = "ProjectilePosition";
                        break;
                }
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

            throw new InvalidOperationException(
                $"[SkillSpawnAssetBuilder] Enum value not found. property={propertyName} value={value}");
        }

        private static void SetInt(
            SerializedObject serializedObject,
            string propertyName,
            int value)
        {
            SerializedProperty property = FindRequiredProperty(
                serializedObject,
                propertyName);

            if (property.propertyType != SerializedPropertyType.Integer)
            {
                throw new InvalidOperationException(
                    $"[SkillSpawnAssetBuilder] Property is not int: {propertyName} type={property.propertyType}");
            }

            property.intValue = value;
        }

        private static void SetFloat(
            SerializedObject serializedObject,
            string propertyName,
            float value)
        {
            SerializedProperty property = FindRequiredProperty(
                serializedObject,
                propertyName);

            if (property.propertyType != SerializedPropertyType.Float)
            {
                throw new InvalidOperationException(
                    $"[SkillSpawnAssetBuilder] Property is not float: {propertyName} type={property.propertyType}");
            }

            property.floatValue = value;
        }

        private static void SetObjectReference(
            SerializedObject serializedObject,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedProperty property = FindRequiredProperty(
                serializedObject,
                propertyName);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                throw new InvalidOperationException(
                    $"[SkillSpawnAssetBuilder] Property is not object reference: {propertyName} type={property.propertyType}");
            }

            property.objectReferenceValue = value;
        }

        private static SerializedProperty FindRequiredProperty(
            SerializedObject serializedObject,
            string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);

            if (property == null)
            {
                throw new InvalidOperationException(
                    $"[SkillSpawnAssetBuilder] Serialized property not found: {propertyName}");
            }

            return property;
        }

        private static ScriptableObject FindCharacterSO(
            string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets("t:CharacterSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject characterSo = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (characterSo == null)
                {
                    continue;
                }

                if (string.Equals(characterSo.name, characterId, StringComparison.OrdinalIgnoreCase) ||
                    path.IndexOf(characterId, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    HasCharacterId(characterSo, characterId))
                {
                    return characterSo;
                }
            }

            Debug.LogWarning($"[SkillSpawnAssetBuilder] CharacterSO not found: {characterId}");
            return null;
        }

        private static bool HasCharacterId(
            ScriptableObject characterSo,
            string characterId)
        {
            if (characterSo == null ||
                string.IsNullOrWhiteSpace(characterId))
            {
                return false;
            }

            SerializedObject serializedObject = new SerializedObject(characterSo);
            SerializedProperty property = serializedObject.FindProperty("characterId");

            return property != null &&
                   string.Equals(property.stringValue, characterId, StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureFolder(
            string folderPath)
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
                Debug.LogError($"[SkillSpawnAssetBuilder] Folder path must start with Assets: {folderPath}");
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

        private static string SanitizeFileName(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "skill.spawn";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}