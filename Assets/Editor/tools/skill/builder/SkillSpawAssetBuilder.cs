using System;
using System.IO;
using Character;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    [Serializable]
    public class SpawnSkillJson
    {
        public string spawnSkillId;
        public int spawnCount = 1;
        public float spawnInterval;
        public float spawnLifeTime;
        public string characterSO;
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

            return !string.IsNullOrWhiteSpace(spawnSkill.characterSO);
        }

        public static SpawnSkillSO CreateOrUpdate(
            SpawnSkillJson spawnJson,
            string outputFolder)
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

            Apply(
                spawnSkillSo,
                spawnJson);

            EditorUtility.SetDirty(spawnSkillSo);
            AssetDatabase.SaveAssetIfDirty(spawnSkillSo);

            return spawnSkillSo;
        }

        private static void Apply(
            SpawnSkillSO spawnSkillSo,
            SpawnSkillJson spawnJson)
        {
            spawnSkillSo.ApplyEditorData(
                Mathf.Max(1, spawnJson.spawnCount),
                Mathf.Max(0f, spawnJson.spawnInterval),
                Mathf.Max(0f, spawnJson.spawnLifeTime));

            spawnSkillSo.ApplyEditorCharacterSpawn(
                FindCharacterSO(spawnJson.characterSO));
        }

        private static CharacterSO FindCharacterSO(
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
                CharacterSO characterSo = AssetDatabase.LoadAssetAtPath<CharacterSO>(path);

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
            CharacterSO characterSo,
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