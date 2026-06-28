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
    public static class CharacterClipBuilder
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

        public static List<AnimationClip> GenerateFromFolderPath(string selectedPath)
        {
            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogWarning($"[GenerateClips] Invalid folder path: {selectedPath}");
                return new List<AnimationClip>();
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

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GenerateClips] Complete. Created/Updated {generatedClips.Count} clips. Skipped {skippedCount} folders without sprites.");
            return generatedClips
                .Where(info => info != null && info.Clip != null)
                .Select(info => info.Clip)
                .ToList();
        }

        public static List<AnimationClip> GenerateFromCharacterFolderPath(string characterFolderPath)
        {
            if (string.IsNullOrEmpty(characterFolderPath) || !AssetDatabase.IsValidFolder(characterFolderPath))
            {
                Debug.LogWarning($"[GenerateClips] Invalid character folder path: {characterFolderPath}");
                return new List<AnimationClip>();
            }

            string animationFolderPath = $"{characterFolderPath}/animation";

            if (!AssetDatabase.IsValidFolder(animationFolderPath))
            {
                Debug.LogWarning($"[GenerateClips] Animation folder not found: {animationFolderPath}");
                return new List<AnimationClip>();
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