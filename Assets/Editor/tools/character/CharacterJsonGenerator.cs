#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Character;
using Stat;
using UnityEditor;
using UnityEngine;
using Skill;
namespace ResourceTools.Character
{
    public static class CharacterJsonGenerator
    {
        private const string ContentJsonFolder = "Assets/Contents/Character/json";
        private const string ContentSoFolder = "Assets/Contents/Character/so";

        [Serializable]
        private class CharacterJson
        {
            public string characterId;
            public string name;
            public string characterType;
            public string job;
            public List<StatEntryJson> baseStats = new();
        }

        [Serializable]
        private class StatEntryJson
        {
            public string statType;
            public float value;
        }


        [MenuItem("Assets/Character/Generate CharacterSO From Json", false, 2000)]
        public static void Generate()
        {
            TextAsset jsonAsset = Selection.activeObject as TextAsset;

            if (jsonAsset == null)
            {
                Debug.LogError("[CharacterJsonGenerator] Select a character json file in the Project window first.");
                return;
            }

            string jsonPath = AssetDatabase.GetAssetPath(jsonAsset);
            GenerateFromJsonPath(jsonPath);
        }

        [MenuItem("Assets/Character/Generate CharacterSO From Json Folder", false, 2001)]
        public static void GenerateFromSelectedFolder()
        {
            string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogError("[CharacterJsonGenerator] Select a folder that contains character json files in the Project window first.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { selectedPath });
            int successCount = 0;
            int failCount = 0;

            foreach (string guid in guids)
            {
                string jsonPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(jsonPath) ||
                    !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CharacterSO characterSo = GenerateFromJsonPath(jsonPath);

                if (characterSo == null)
                {
                    failCount++;
                    continue;
                }

                successCount++;
            }

            Debug.Log($"[CharacterJsonGenerator] Folder generation completed. Folder={selectedPath}, Success={successCount}, Failed={failCount}");
        }

        public static CharacterSO GenerateFromJsonPath(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath) || !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError("[CharacterJsonGenerator] Selected asset is not a json file.");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            CharacterJson data = JsonUtility.FromJson<CharacterJson>(json);

            if (data == null || string.IsNullOrEmpty(data.characterId))
            {
                Debug.LogError($"[CharacterJsonGenerator] Invalid character json: {jsonPath}");
                return null;
            }

            string outputFolder = ResolveOutputFolder(jsonPath);

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[CharacterJsonGenerator] Cannot resolve output folder from json path.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetName = GetSafeAssetName(data.characterId);
            string assetPath = $"{outputFolder}/{assetName}.asset";

            CharacterSO characterSo = AssetDatabase.LoadAssetAtPath<CharacterSO>(assetPath);
            bool isNewAsset = false;

            if (characterSo == null)
            {
                characterSo = ScriptableObject.CreateInstance<CharacterSO>();
                isNewAsset = true;
            }

            CharacterType characterType =
                (CharacterType)Enum.Parse(
                    typeof(CharacterType),
                    data.characterType,
                    true);
            CharacterJob job =
                (CharacterJob)Enum.Parse(
                    typeof(CharacterJob),
                    data.job,
                    true);
            characterSo.ApplyEditorData(
                data.characterId,
                characterType,
                job,
                BuildAnimationClips(data.characterId),
                BuildSkills(data.characterId),
                ConvertBaseStats(data.baseStats));

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(characterSo, assetPath);
                Debug.Log($"[CharacterJsonGenerator] Created CharacterSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(characterSo);
                Debug.Log($"[CharacterJsonGenerator] Updated CharacterSO: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return characterSo;
        }

        private static string ResolveOutputFolder(string jsonPath)
        {
            string jsonFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");
            return string.Equals(jsonFolder, ContentJsonFolder, StringComparison.OrdinalIgnoreCase)
                ? ContentSoFolder
                : jsonFolder;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parentFolder = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string folderName = Path.GetFileName(folderPath);

            EnsureFolder(parentFolder);

            if (!string.IsNullOrEmpty(parentFolder) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }

        [MenuItem("Assets/Character/Generate CharacterSO From Json", true)]
        private static bool ValidateGenerate()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Assets/Character/Generate CharacterSO From Json Folder", true)]
        private static bool ValidateGenerateFromSelectedFolder()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
        }

        private static List<StatEntry> ConvertBaseStats(List<StatEntryJson> stats)
        {
            List<StatEntry> result = new();

            if (stats == null)
            {
                return result;
            }

            foreach (StatEntryJson stat in stats)
            {
                if (stat == null || string.IsNullOrEmpty(stat.statType))
                {
                    continue;
                }

                result.Add(new StatEntry
                {
                    statType = (StatType)Enum.Parse(
                        typeof(StatType),
                        stat.statType,
                        true),
                    value = stat.value
                });
            }

            return result;
        }

        private static List<CharacterAnimationClipEntry> BuildAnimationClips(string characterId)
        {
            List<CharacterAnimationClipEntry> result = new();

            if (string.IsNullOrWhiteSpace(characterId))
            {
                return result;
            }

            Dictionary<string, AnimationClip> clipsByAction = LoadAnimationClipsByAction(characterId);

            if (clipsByAction.Count == 0)
            {
                Debug.Log($"[CharacterJsonGenerator] Generating missing animation clips for: {characterId}");
                global::ResourceTools.CharacterClipBuilder.GenerateAll();
                clipsByAction = LoadAnimationClipsByAction(characterId);
            }

            Array clipTypes = Enum.GetValues(typeof(CharacterAnimationClipType));

            foreach (object value in clipTypes)
            {
                CharacterAnimationClipType clipType = (CharacterAnimationClipType)value;
                string action = GetAnimationAction(clipType);
                string direction = clipType.ToString().EndsWith("Left", StringComparison.Ordinal)
                    ? "left"
                    : "right";
                string clipKey = $"{action}.{direction}";

                if (string.IsNullOrEmpty(action) || !clipsByAction.TryGetValue(clipKey, out AnimationClip clip))
                {
                    continue;
                }

                result.Add(new CharacterAnimationClipEntry
                {
                    clipType = clipType,
                    clip = clip
                });
            }

            return result;
        }

        private static Dictionary<string, AnimationClip> LoadAnimationClipsByAction(string characterId)
        {
            const string clipFolder = "Assets/AnimationClips/Character";
            Dictionary<string, AnimationClip> result = new(StringComparer.OrdinalIgnoreCase);

            if (!AssetDatabase.IsValidFolder(clipFolder))
            {
                Debug.LogWarning($"[CharacterJsonGenerator] Animation clip folder not found: {clipFolder}");
                return result;
            }

            string[] guids = AssetDatabase.FindAssets($"{characterId} t:AnimationClip", new[] { clipFolder });

            foreach (string path in guids
                         .Select(AssetDatabase.GUIDToAssetPath)
                         .Where(path => !string.IsNullOrEmpty(path))
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip == null || !clip.name.StartsWith(characterId + ".", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string action = GetAnimationAction(clip.name);
                string direction = GetAnimationDirection(clip.name);
                string clipKey = $"{action}.{direction}";

                if (!string.IsNullOrEmpty(action) &&
                    !string.IsNullOrEmpty(direction) &&
                    !result.ContainsKey(clipKey))
                {
                    result.Add(clipKey, clip);
                }
            }

            return result;
        }

        private static string GetAnimationAction(CharacterAnimationClipType clipType)
        {
            string name = clipType.ToString();

            if (name.StartsWith("Idle", StringComparison.Ordinal)) return "idle";
            if (name.StartsWith("Move", StringComparison.Ordinal)) return "movement";
            if (name.StartsWith("Attack", StringComparison.Ordinal)) return "attack";
            if (name.StartsWith("Death", StringComparison.Ordinal)) return "death";
            return null;
        }

        private static string GetAnimationDirection(string clipName)
        {
            string normalizedName = clipName.Replace('-', '.').Replace('_', '.');
            string[] parts = normalizedName.Split('.');

            foreach (string part in parts)
            {
                if (part.Equals("left", StringComparison.OrdinalIgnoreCase)) return "left";
                if (part.Equals("right", StringComparison.OrdinalIgnoreCase)) return "right";
            }

            return null;
        }

        private static string GetAnimationAction(string clipName)
        {
            string normalizedName = clipName.Replace('-', '.').Replace('_', '.');
            string[] parts = normalizedName.Split('.');

            foreach (string part in parts)
            {
                if (part.Equals("idle", StringComparison.OrdinalIgnoreCase)) return "idle";
                if (part.Equals("move", StringComparison.OrdinalIgnoreCase) ||
                    part.Equals("movement", StringComparison.OrdinalIgnoreCase)) return "movement";
                if (part.Equals("attack", StringComparison.OrdinalIgnoreCase)) return "attack";
                if (part.Equals("death", StringComparison.OrdinalIgnoreCase)) return "death";
            }

            return null;
        }

        private static List<CharacterSkillEntry> BuildSkills(string characterId)
        {
            List<CharacterSkillEntry> result = new();

            if (string.IsNullOrWhiteSpace(characterId))
            {
                return result;
            }

            string skillPrefix = $"skill.{characterId}.";
            List<EquipmentSkillSO> generatedSkills = GenerateSkillsFromJson(characterId);

            foreach (EquipmentSkillSO skillSo in generatedSkills)
            {
                AddSkillEntry(
                    result,
                    skillSo,
                    skillPrefix);
            }

            return result;
        }

        private static List<EquipmentSkillSO> GenerateSkillsFromJson(string characterId)
        {
            List<EquipmentSkillSO> result = new();

            string[] guids = AssetDatabase.FindAssets($"skill.{characterId} t:TextAsset");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrWhiteSpace(path) ||
                    !path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                EquipmentSkillSO skillSo = Skill.EquipmentSkillJsonGenerator.GenerateFromJsonPath(path);

                if (skillSo != null)
                {
                    result.Add(skillSo);
                }
            }

            return result;
        }

        private static void AddSkillEntry(
            List<CharacterSkillEntry> result,
            EquipmentSkillSO skillSo,
            string skillPrefix)
        {
            if (result == null || skillSo == null || string.IsNullOrWhiteSpace(skillPrefix))
            {
                return;
            }

            string skillId = !string.IsNullOrWhiteSpace(skillSo.EquipmentId)
                ? skillSo.EquipmentId
                : skillSo.name;

            if (string.IsNullOrWhiteSpace(skillId) ||
                !skillId.StartsWith(skillPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string remainder = skillId.Substring(skillPrefix.Length);
            int dotIndex = remainder.IndexOf('.');

            if (dotIndex <= 0)
            {
                Debug.LogWarning($"[CharacterJsonGenerator] Cannot resolve slotKey from skillId={skillId}");
                return;
            }

            string slotKey = remainder.Substring(0, dotIndex);

            for (int i = 0; i < result.Count; i++)
            {
                CharacterSkillEntry existing = result[i];
                if (existing != null && existing.slotKey == slotKey)
                {
                    return;
                }
            }

            result.Add(new CharacterSkillEntry
            {
                slotKey = slotKey,
                skillSo = skillSo
            });
        }


        private static string GetSafeAssetName(string id)
        {
            return string.IsNullOrEmpty(id)
                ? "generated_asset"
                : id.Replace(".", "_").Replace("/", "_").Replace(" ", "_");
        }
    }
}
#endif
