#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Character;
using Stat;
using UnityEditor;
using UnityEngine;
using Skill;
namespace ResourceTools.Character
{
    public static class CharacterJsonGenerator
    {
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

            string outputFolder = Path.GetDirectoryName(jsonPath)?.Replace("\\", "/");

            if (string.IsNullOrEmpty(outputFolder))
            {
                Debug.LogError("[CharacterJsonGenerator] Cannot resolve output folder from json path.");
                return null;
            }

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

            Array clipTypes = Enum.GetValues(typeof(CharacterAnimationClipType));

            foreach (object value in clipTypes)
            {
                CharacterAnimationClipType clipType = (CharacterAnimationClipType)value;
                string clipName = $"{characterId}.{clipType}";
                AnimationClip clip = CreateOrUpdateAnimationClipFromSprites(clipName);

                if (clip == null)
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


        private static AnimationClip CreateOrUpdateAnimationClipFromSprites(string clipName)
        {
            const string spriteFolder = "Assets/Resources/character/animation_png";
            const string clipFolder = "Assets/Resources/character/animation_clip";

            List<Sprite> sprites = FindSpritesForClip(spriteFolder, clipName);

            if (sprites.Count == 0)
            {
                return null;
            }

            EnsureFolder(clipFolder);

            string clipPath = $"{clipFolder}/{clipName}.clip.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            AnimationClip generatedClip = CreateSpriteAnimationClip(sprites);

            if (clip == null)
            {
                AssetDatabase.CreateAsset(generatedClip, clipPath);
                return generatedClip;
            }

            EditorUtility.CopySerialized(generatedClip, clip);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static List<Sprite> FindSpritesForClip(
            string spriteFolder,
            string clipName)
        {
            List<Sprite> result = new();

            string[] guids = AssetDatabase.FindAssets(
                $"{clipName} t:Sprite",
                new[] { spriteFolder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite == null)
                {
                    continue;
                }

                if (!sprite.name.StartsWith(clipName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Add(sprite);
            }

            result.Sort((left, right) =>
                string.Compare(
                    left != null ? left.name : string.Empty,
                    right != null ? right.name : string.Empty,
                    StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private static AnimationClip CreateSpriteAnimationClip(
            List<Sprite> sprites)
        {
            AnimationClip clip = new AnimationClip
            {
                frameRate = 12f
            };

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];

            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(
                clip,
                binding,
                keyframes);

            return clip;
        }

        private static void EnsureFolder(string folderPath)
        {
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

        private static string GetSafeAssetName(string id)
        {
            return string.IsNullOrEmpty(id)
                ? "generated_asset"
                : id.Replace(".", "_").Replace("/", "_").Replace(" ", "_");
        }
    }
}
#endif