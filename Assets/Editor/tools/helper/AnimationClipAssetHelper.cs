#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Helper
{
    /// <summary>
    /// Sprite AnimationClip 에셋을 항상 삭제 후 재생성하는 Editor 전용 helper.
    /// </summary>
    public static class AnimationClipAssetHelper
    {
        public const float DefaultFrameRate = 12f;

        public static AnimationClip RecreateSpriteAnimationClip(
            string assetPath,
            IReadOnlyList<Sprite> sprites,
            float frameRate = DefaultFrameRate,
            bool loopTime = true)
        {
            string normalizedPath = NormalizeAssetPath(assetPath);

            if (!IsValidAnimationClipPath(normalizedPath))
            {
                Debug.LogError($"[AnimationClipAssetHelper] Invalid AnimationClip asset path: {assetPath}");
                return null;
            }

            Sprite[] validSprites = sprites?
                .Where(sprite => sprite != null)
                .ToArray() ?? Array.Empty<Sprite>();

            if (validSprites.Length == 0)
            {
                Debug.LogWarning($"[AnimationClipAssetHelper] No sprites supplied: {normalizedPath}");
                return null;
            }

            if (frameRate <= 0f)
            {
                Debug.LogError($"[AnimationClipAssetHelper] Frame rate must be greater than zero: {frameRate}");
                return null;
            }

            EnsureParentFolder(normalizedPath);

            if (!DeleteAssetIfExists(normalizedPath))
            {
                return null;
            }

            AnimationClip clip = CreateSpriteAnimationClip(validSprites, frameRate, loopTime);
            clip.name = Path.GetFileNameWithoutExtension(normalizedPath);
            AssetDatabase.CreateAsset(clip, normalizedPath);
            EditorUtility.SetDirty(clip);

            return clip;
        }

        public static bool DeleteAssetIfExists(string assetPath)
        {
            string normalizedPath = NormalizeAssetPath(assetPath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return false;
            }

            UnityEngine.Object existingAsset = AssetDatabase.LoadMainAssetAtPath(normalizedPath);

            if (existingAsset == null)
            {
                return true;
            }

            if (AssetDatabase.DeleteAsset(normalizedPath))
            {
                return true;
            }

            Debug.LogError($"[AnimationClipAssetHelper] Failed to delete existing asset: {normalizedPath}");
            return false;
        }

        private static AnimationClip CreateSpriteAnimationClip(
            IReadOnlyList<Sprite> sprites,
            float frameRate,
            bool loopTime)
        {
            AnimationClip clip = new AnimationClip
            {
                frameRate = frameRate
            };

            EditorCurveBinding spriteBinding = new EditorCurveBinding
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
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loopTime;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            return clip;
        }

        private static bool IsValidAnimationClipPath(string assetPath)
        {
            return !string.IsNullOrWhiteSpace(assetPath)
                && assetPath.StartsWith("Assets/", StringComparison.Ordinal)
                && string.Equals(Path.GetExtension(assetPath), ".anim", StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureParentFolder(string assetPath)
        {
            string parentFolder = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");

            if (string.IsNullOrWhiteSpace(parentFolder) || AssetDatabase.IsValidFolder(parentFolder))
            {
                return;
            }

            string[] parts = parentFolder.Split('/');
            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = $"{currentPath}/{parts[i]}";

                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace("\\", "/").Trim();
        }
    }
}
#endif
