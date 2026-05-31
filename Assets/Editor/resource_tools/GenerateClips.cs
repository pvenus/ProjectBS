#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Character;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class GenerateClips
    {
        private const float FrameRate = 12f;
        private const string OutputFolderName = "_GeneratedClips";

        private sealed class GeneratedClipInfo
        {
            public string ClipName;
            public AnimationClip Clip;
        }

        [MenuItem("Assets/Character/Generate Animation Clips From Child Folders", true)]
        private static bool ValidateExecute()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            return AssetDatabase.IsValidFolder(path);
        }

        [MenuItem("Assets/Character/Generate Animation Clips From Child Folders", false, 2001)]
        public static void Execute()
        {
            UnityEngine.Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogWarning("[GenerateClips] Please select a folder.");
                return;
            }

            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogWarning("[GenerateClips] Selected asset is not a folder.");
                return;
            }

            GenerateFromFolderPath(selectedPath);
        }

        public static AnimationClipSetSO GenerateFromFolderPath(string selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogWarning($"[GenerateClips] Invalid folder path: {selectedPath}");
                return null;
            }

            string outputFolderPath = EnsureOutputFolder(selectedPath);
            string[] targetFolders = GetFolderAndChildren(selectedPath)
                .Where(path => path != outputFolderPath)
                .Where(path => !path.StartsWith(outputFolderPath + "/"))
                .ToArray();

            List<GeneratedClipInfo> generatedClips = new List<GeneratedClipInfo>();
            int skippedCount = 0;

            foreach (string folderPath in targetFolders)
            {
                Sprite[] sprites = LoadSpritesInFolderOnly(folderPath);

                if (sprites.Length == 0)
                {
                    skippedCount++;
                    continue;
                }

                string clipName = CreateClipName(selectedPath, folderPath);
                string clipPath = $"{outputFolderPath}/{clipName}.anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

                if (clip == null)
                {
                    clip = CreateSpriteAnimationClip(sprites);
                    AssetDatabase.CreateAsset(clip, clipPath);
                    Debug.Log($"[GenerateClips] Created clip: {clipPath} / Frames: {sprites.Length}");
                }
                else
                {
                    AnimationClip updatedClip = CreateSpriteAnimationClip(sprites);
                    EditorUtility.CopySerialized(updatedClip, clip);
                    EditorUtility.SetDirty(clip);
                    Debug.Log($"[GenerateClips] Updated clip: {clipPath} / Frames: {sprites.Length}");
                }

                generatedClips.Add(new GeneratedClipInfo
                {
                    ClipName = clipName,
                    Clip = clip
                });
            }

            AnimationClipSetSO clipSet = null;

            if (generatedClips.Count > 0)
            {
                clipSet = CreateOrUpdateAnimationClipSetSO(selectedPath, outputFolderPath, generatedClips);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GenerateClips] Complete. Created/Updated {generatedClips.Count} clips. Skipped {skippedCount} folders without sprites.");
            return clipSet;
        }

        public static AnimationClipSetSO GenerateFromCharacterFolderPath(string characterFolderPath)
        {
            if (string.IsNullOrEmpty(characterFolderPath) || !AssetDatabase.IsValidFolder(characterFolderPath))
            {
                Debug.LogWarning($"[GenerateClips] Invalid character folder path: {characterFolderPath}");
                return null;
            }

            string animationFolderPath = $"{characterFolderPath}/animation";

            if (!AssetDatabase.IsValidFolder(animationFolderPath))
            {
                Debug.LogWarning($"[GenerateClips] Animation folder not found: {animationFolderPath}");
                return null;
            }

            return GenerateFromFolderPath(animationFolderPath);
        }

        private static AnimationClip CreateSpriteAnimationClip(Sprite[] sprites)
        {
            AnimationClip clip = new AnimationClip
            {
                frameRate = FrameRate
            };

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];

            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / FrameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            return clip;
        }

        private static AnimationClipSetSO CreateOrUpdateAnimationClipSetSO(
            string selectedPath,
            string outputFolderPath,
            List<GeneratedClipInfo> generatedClips)
        {
            string selectedFolderName = new DirectoryInfo(selectedPath).Name;
            string assetPath = $"{outputFolderPath}/{selectedFolderName}_AnimationClipSetSO.asset";

            AnimationClipSetSO clipSet = AssetDatabase.LoadAssetAtPath<AnimationClipSetSO>(assetPath);
            bool isNewAsset = false;

            if (clipSet == null)
            {
                clipSet = ScriptableObject.CreateInstance<AnimationClipSetSO>();
                isNewAsset = true;
            }

            clipSet.idleClips = new DirectionalAnimationClips();
            clipSet.moveClips = new DirectionalAnimationClips();
            clipSet.attackClips = new DirectionalAnimationClips();
            clipSet.deathClips = new DirectionalAnimationClips();

            foreach (GeneratedClipInfo info in generatedClips)
            {
                if (!TryParseClipName(info.ClipName, out string stateName, out string directionName))
                {
                    Debug.LogWarning($"[GenerateClips] Cannot map clip name to AnimationClipSetSO: {info.ClipName}");
                    continue;
                }

                DirectionalAnimationClips target = GetTargetClips(clipSet, stateName);

                if (target == null)
                {
                    Debug.LogWarning($"[GenerateClips] Unknown state: {stateName} / Clip: {info.ClipName}");
                    continue;
                }

                AssignDirectionalClip(target, directionName, info.Clip, info.ClipName);
            }

            if (isNewAsset)
            {
                AssetDatabase.CreateAsset(clipSet, assetPath);
                Debug.Log($"[GenerateClips] Created AnimationClipSetSO: {assetPath}");
            }
            else
            {
                EditorUtility.SetDirty(clipSet);
                Debug.Log($"[GenerateClips] Updated AnimationClipSetSO: {assetPath}");
            }

            return clipSet;
        }

        private static DirectionalAnimationClips GetTargetClips(AnimationClipSetSO clipSet, string stateName)
        {
            switch (NormalizeToken(stateName))
            {
                case "idle":
                    return clipSet.idleClips;

                case "move":
                case "walk":
                case "run":
                    return clipSet.moveClips;

                case "attack":
                case "atk":
                    return clipSet.attackClips;

                case "death":
                case "die":
                    return clipSet.deathClips;

                default:
                    return null;
            }
        }

        private static void AssignDirectionalClip(
            DirectionalAnimationClips target,
            string directionName,
            AnimationClip clip,
            string clipName)
        {
            switch (NormalizeToken(directionName))
            {
                case "upright":
                case "northeast":
                case "ne":
                    target.upRight = clip;
                    break;

                case "upleft":
                case "northwest":
                case "nw":
                    target.upLeft = clip;
                    break;

                case "downright":
                case "southeast":
                case "se":
                    target.downRight = clip;
                    break;

                case "downleft":
                case "southwest":
                case "sw":
                    target.downLeft = clip;
                    break;

                default:
                    Debug.LogWarning($"[GenerateClips] Unknown direction: {directionName} / Clip: {clipName}");
                    break;
            }
        }

        private static bool TryParseClipName(string clipName, out string stateName, out string directionName)
        {
            stateName = null;
            directionName = null;

            string[] tokens = clipName.Split(new[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (tokens.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                if (!IsStateToken(NormalizeToken(tokens[i])))
                {
                    continue;
                }

                string combinedDirection = string.Concat(tokens.Skip(i + 1));

                if (IsDirectionToken(NormalizeToken(combinedDirection)))
                {
                    stateName = tokens[i];
                    directionName = combinedDirection;
                    return true;
                }

                for (int j = i + 1; j < tokens.Length; j++)
                {
                    if (!IsDirectionToken(NormalizeToken(tokens[j])))
                    {
                        continue;
                    }

                    stateName = tokens[i];
                    directionName = tokens[j];
                    return true;
                }
            }

            return false;
        }

        private static bool IsStateToken(string token)
        {
            switch (token)
            {
                case "idle":
                case "move":
                case "walk":
                case "run":
                case "attack":
                case "atk":
                case "death":
                case "die":
                    return true;

                default:
                    return false;
            }
        }

        private static bool IsDirectionToken(string token)
        {
            switch (token)
            {
                case "upright":
                case "upleft":
                case "downright":
                case "downleft":
                case "northeast":
                case "northwest":
                case "southeast":
                case "southwest":
                case "ne":
                case "nw":
                case "se":
                case "sw":
                    return true;

                default:
                    return false;
            }
        }

        private static string NormalizeToken(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToLowerInvariant();
        }

        private static Sprite[] LoadSpritesInFolderOnly(string folderPath)
        {
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

            return spriteGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetDirectoryName(path)?.Replace("\\", "/") == folderPath)
                .Distinct()
                .OrderBy(GetNumericFileName)
                .ThenBy(path => path)
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .Where(sprite => sprite != null)
                .ToArray();
        }

        private static int GetNumericFileName(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            return int.TryParse(fileName, out int number) ? number : int.MaxValue;
        }

        private static string CreateClipName(string rootPath, string folderPath)
        {
            string relativePath = folderPath.Replace(rootPath, string.Empty).Trim('/');

            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = new DirectoryInfo(folderPath).Name;
            }

            string clipName = relativePath.Replace("/", "_").Replace(" ", "_");
            return string.IsNullOrEmpty(clipName) ? "AnimationClip" : clipName;
        }

        private static string EnsureOutputFolder(string selectedPath)
        {
            string outputFolderPath = $"{selectedPath}/{OutputFolderName}";

            if (!AssetDatabase.IsValidFolder(outputFolderPath))
            {
                AssetDatabase.CreateFolder(selectedPath, OutputFolderName);
            }

            return outputFolderPath;
        }

        private static string[] GetFolderAndChildren(string rootPath)
        {
            List<string> folders = new List<string> { rootPath };
            CollectChildFolders(rootPath, folders);
            return folders.ToArray();
        }

        private static void CollectChildFolders(string parentPath, List<string> folders)
        {
            string[] childGuids = AssetDatabase.FindAssets("t:Folder", new[] { parentPath });

            foreach (string childGuid in childGuids)
            {
                string childPath = AssetDatabase.GUIDToAssetPath(childGuid);

                if (string.IsNullOrEmpty(childPath))
                {
                    continue;
                }

                if (childPath == parentPath)
                {
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(childPath))
                {
                    continue;
                }

                if (folders.Contains(childPath))
                {
                    continue;
                }

                folders.Add(childPath);
            }
        }
    }
}
#endif