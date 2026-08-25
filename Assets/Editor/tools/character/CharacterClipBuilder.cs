#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Character;
using ResourceTools.Helper;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class CharacterClipBuilder
    {
        private const float FrameRate = 12f;
        private const string SourceFolderPath = "Assets/ImagesGenerated/Character/animation";
        private const string OutputFolderPath = "Assets/AnimationClips/Character";

        private sealed class GeneratedClipInfo
        {
            public string ClipName;
            public AnimationClip Clip;
        }

        [MenuItem("Assets/Character/Generate Animation Clips From Child Folders", true)]
        private static bool ValidateExecute()
        {
            return AssetDatabase.IsValidFolder(SourceFolderPath);
        }

        [MenuItem("Assets/Character/Generate Animation Clips From Child Folders", false, 2001)]
        public static void Execute()
        {
            GenerateAll();
        }

        public static List<AnimationClip> GenerateAll()
        {
            return GenerateFromFolderPath(SourceFolderPath);
        }

        public static List<AnimationClip> GenerateFromFolderPath(string selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogWarning($"[GenerateClips] Invalid folder path: {selectedPath}");
                return new List<AnimationClip>();
            }

            string outputFolderPath = EnsureOutputFolder();
            string[] targetFolders = GetFolderAndChildren(selectedPath);

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

                string baseClipName = CreateClipName(SourceFolderPath, folderPath);
                AnimationClip rightClip = CreateDirectionalClip(
                    outputFolderPath,
                    baseClipName,
                    "Right",
                    sprites,
                    false);
                AnimationClip leftClip = CreateDirectionalClip(
                    outputFolderPath,
                    baseClipName,
                    "Left",
                    sprites,
                    true);

                if (rightClip == null || leftClip == null)
                {
                    Debug.LogError($"[GenerateClips] Failed to recreate directional clips: {baseClipName}");
                    continue;
                }

                generatedClips.Add(new GeneratedClipInfo
                {
                    ClipName = rightClip.name,
                    Clip = rightClip
                });
                generatedClips.Add(new GeneratedClipInfo
                {
                    ClipName = leftClip.name,
                    Clip = leftClip
                });
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GenerateClips] Complete. Recreated {generatedClips.Count} clips. Skipped {skippedCount} folders without sprites.");
            return generatedClips
                .Where(info => info != null && info.Clip != null)
                .Select(info => info.Clip)
                .ToList();
        }

        private static AnimationClip CreateDirectionalClip(
            string outputFolderPath,
            string baseClipName,
            string direction,
            Sprite[] sprites,
            bool flipX)
        {
            string clipName = $"{baseClipName}.{direction}";
            string clipPath = $"{outputFolderPath}/{clipName}.anim";
            AnimationClip clip = AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClip(
                clipPath,
                sprites,
                FrameRate,
                true,
                flipX);

            if (clip != null)
            {
                Debug.Log($"[GenerateClips] Recreated clip: {clipPath} / Frames: {sprites.Length} / FlipX: {flipX}");
            }

            return clip;
        }

        public static List<AnimationClip> GenerateFromCharacterFolderPath(string characterFolderPath)
        {
            if (string.IsNullOrWhiteSpace(characterFolderPath))
            {
                return new List<AnimationClip>();
            }

            string normalizedPath = characterFolderPath.Replace("\\", "/").TrimEnd('/');
            if (!normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath = $"{SourceFolderPath}/{normalizedPath}";
            }

            return GenerateFromFolderPath(normalizedPath);
        }

        private static Sprite[] LoadSpritesInFolderOnly(string folderPath)
        {
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

            return spriteGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => Path.GetDirectoryName(path)?.Replace("\\", "/") == folderPath)
                .Where(IsAnimationFrameFile)
                .Distinct()
                .OrderBy(GetNumericFileName)
                .ThenBy(path => path)
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .Where(sprite => sprite != null)
                .ToArray();
        }

        private static bool IsAnimationFrameFile(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return HasNumericFrameSuffix(fileName, "frame-") ||
                   HasNumericFrameSuffix(fileName, "frame_");
        }

        private static bool HasNumericFrameSuffix(string fileName, string prefix)
        {
            if (!fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string frameNumber = fileName.Substring(prefix.Length);
            return frameNumber.Length > 0 && frameNumber.All(char.IsDigit);
        }

        private static int GetNumericFileName(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            int digitStart = fileName.Length;

            while (digitStart > 0 && char.IsDigit(fileName[digitStart - 1]))
            {
                digitStart--;
            }

            return digitStart < fileName.Length &&
                   int.TryParse(fileName.Substring(digitStart), out int number)
                ? number
                : int.MaxValue;
        }

        private static string CreateClipName(string rootPath, string folderPath)
        {
            string relativePath = folderPath.Replace(rootPath, string.Empty).Trim('/');

            if (string.IsNullOrEmpty(relativePath))
            {
                relativePath = new DirectoryInfo(folderPath).Name;
            }

            string[] pathParts = relativePath.Split('/');
            string characterId = pathParts[0];
            string animationName = pathParts[pathParts.Length - 1];
            string clipName = animationName.StartsWith(characterId + ".", StringComparison.OrdinalIgnoreCase)
                ? animationName
                : $"{characterId}.{animationName}";

            clipName = clipName.Replace("/", "_").Replace(" ", "_");
            return string.IsNullOrEmpty(clipName) ? "AnimationClip" : clipName;
        }

        private static string EnsureOutputFolder()
        {
            string[] folderParts = OutputFolderPath.Split('/');
            string currentPath = folderParts[0];

            for (int i = 1; i < folderParts.Length; i++)
            {
                string nextPath = $"{currentPath}/{folderParts[i]}";

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folderParts[i]);
                }

                currentPath = nextPath;
            }

            return OutputFolderPath;
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
