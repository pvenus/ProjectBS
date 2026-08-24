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
            bool loopTime = true,
            bool flipX = false)
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

            AnimationClip clip = CreateSpriteAnimationClip(validSprites, frameRate, loopTime, flipX);
            clip.name = Path.GetFileNameWithoutExtension(normalizedPath);
            AssetDatabase.CreateAsset(clip, normalizedPath);
            EditorUtility.SetDirty(clip);

            return clip;
        }

        public static AnimationClip CreateOrUpdateSpriteAnimationClip(
            string assetPath,
            IReadOnlyList<Sprite> sprites,
            float frameRate = DefaultFrameRate,
            bool loopTime = true,
            bool flipX = false)
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

            if (validSprites.Length == 0 || frameRate <= 0f)
            {
                Debug.LogWarning($"[AnimationClipAssetHelper] Invalid sprite animation input: {normalizedPath}");
                return null;
            }

            EnsureParentFolder(normalizedPath);

            AnimationClip generatedClip = CreateSpriteAnimationClip(
                validSprites,
                frameRate,
                loopTime,
                flipX);
            generatedClip.name = Path.GetFileNameWithoutExtension(normalizedPath);

            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(normalizedPath);

            if (existingClip == null)
            {
                AssetDatabase.CreateAsset(generatedClip, normalizedPath);
                EditorUtility.SetDirty(generatedClip);
                return generatedClip;
            }

            EditorUtility.CopySerialized(generatedClip, existingClip);
            existingClip.name = Path.GetFileNameWithoutExtension(normalizedPath);
            EditorUtility.SetDirty(existingClip);
            UnityEngine.Object.DestroyImmediate(generatedClip);
            return existingClip;
        }

        public static AnimationClip CreateOrUpdateSpriteAnimationClipWithDurations(
            string assetPath,
            IReadOnlyList<Sprite> sprites,
            IReadOnlyList<float> frameDurations,
            float frameRate = DefaultFrameRate,
            bool loopTime = true,
            bool flipX = false)
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

            if (validSprites.Length == 0 || frameRate <= 0f ||
                frameDurations == null || frameDurations.Count != validSprites.Length ||
                frameDurations.Any(duration => duration <= 0f))
            {
                Debug.LogWarning($"[AnimationClipAssetHelper] Invalid timed sprite animation input: {normalizedPath}");
                return null;
            }

            EnsureParentFolder(normalizedPath);

            AnimationClip generatedClip = CreateSpriteAnimationClip(
                validSprites,
                frameRate,
                loopTime,
                flipX,
                frameDurations);
            generatedClip.name = Path.GetFileNameWithoutExtension(normalizedPath);

            AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(normalizedPath);

            if (existingClip == null)
            {
                AssetDatabase.CreateAsset(generatedClip, normalizedPath);
                EditorUtility.SetDirty(generatedClip);
                return generatedClip;
            }

            EditorUtility.CopySerialized(generatedClip, existingClip);
            existingClip.name = Path.GetFileNameWithoutExtension(normalizedPath);
            EditorUtility.SetDirty(existingClip);
            UnityEngine.Object.DestroyImmediate(generatedClip);
            return existingClip;
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
            bool loopTime,
            bool flipX,
            IReadOnlyList<float> frameDurations = null)
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

            float keyframeTime = 0f;

            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = keyframeTime,
                    value = sprites[i]
                };

                keyframeTime += frameDurations != null
                    ? frameDurations[i]
                    : 1f / frameRate;
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            EditorCurveBinding flipBinding = EditorCurveBinding.FloatCurve(
                string.Empty,
                typeof(SpriteRenderer),
                "m_FlipX");
            float clipDuration = frameDurations != null
                ? keyframeTime
                : Mathf.Max((sprites.Count - 1) / frameRate, 1f / frameRate);
            AnimationCurve flipCurve = AnimationCurve.Constant(
                0f,
                Mathf.Max(clipDuration, 1f / frameRate),
                flipX ? 1f : 0f);
            AnimationUtility.SetEditorCurve(clip, flipBinding, flipCurve);

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
