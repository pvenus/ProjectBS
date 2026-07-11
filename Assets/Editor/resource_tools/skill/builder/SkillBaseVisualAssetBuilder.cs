using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ResourceTools.Helper;
using Skill;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    [Serializable]
    public class BaseVisualJson
    {
        public string visualId;

        public string projectileVisualType;
    }

    /// <summary>
    /// 스킬 기본 시각 요소 전용 에셋 빌더.
    /// JSON의 base visual 데이터를 BaseVisualSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillBaseVisualAssetBuilder
    {
        private const string SkillAnimationSpriteFolder = "Assets/Resources/skill/animation_png";
        private const float SkillAnimationFrameRate = 12f;

        public static BaseVisualSO CreateOrUpdate(
            BaseVisualJson json,
            string outputFolder,
            bool generateAnimation = true)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillBaseVisualAssetBuilder] BaseVisual json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillBaseVisualAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            if (!AnimationClipAssetHelper.DeleteAssetIfExists(assetPath))
            {
                Debug.LogError($"[SkillBaseVisualAssetBuilder] Failed to delete existing BaseVisualSO: {assetPath}");
                return null;
            }

            BaseVisualSO visualSo = ScriptableObject.CreateInstance<BaseVisualSO>();
            AssetDatabase.CreateAsset(visualSo, assetPath);

            Apply(visualSo, json, outputFolder, generateAnimation);

            EditorUtility.SetDirty(visualSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillBaseVisualAssetBuilder] Deleted and recreated BaseVisualSO: {assetPath}");

            return visualSo;
        }

        private static string ResolveAssetName(BaseVisualJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.visualId))
            {
                return SanitizeFileName(json.visualId);
            }
            return "skill.base.visual";
        }

        private static void Apply(
            BaseVisualSO visualSo,
            BaseVisualJson json,
            string outputFolder,
            bool generateAnimation)
        {
            if (visualSo == null || json == null)
            {
                return;
            }

            ProjectileVisualType projectileVisualType = ResolveProjectileVisualType(json.projectileVisualType);
            AnimationClipEntry[] animationClips;

            if (generateAnimation)
            {
                animationClips = CreateAnimationClipEntries(
                    json.visualId,
                    outputFolder);
            }
            else
            {
                DeleteGeneratedAnimationClip(json.visualId, outputFolder);
                animationClips = Array.Empty<AnimationClipEntry>();
                Debug.Log(
                    $"[SkillBaseVisualAssetBuilder] Skipped skill animation generation: " +
                    $"visualId={json.visualId}");
            }

            visualSo.ApplyEditorData(
                json.visualId,
                projectileVisualType,
                animationClips);
        }

        private static ProjectileVisualType ResolveProjectileVisualType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.LogError("[SkillBaseVisualAssetBuilder] projectileVisualType is required.");
                return ProjectileVisualType.Default;
            }

            if (!Enum.TryParse(
                    value,
                    true,
                    out ProjectileVisualType projectileVisualType))
            {
                Debug.LogError($"[SkillBaseVisualAssetBuilder] Invalid projectileVisualType. value={value}");
                return ProjectileVisualType.Default;
            }

            return projectileVisualType;
        }

        // Animation clips are resolved automatically from visualId.
        // No clip information is stored in JSON.
        private static AnimationClipEntry[] CreateAnimationClipEntries(
            string visualId,
            string outputFolder)
        {
            AnimationClip generatedLoopClip = RecreateAnimationClipFromSpriteSheet(
                visualId,
                outputFolder);

            AnimationClip idleClip = FindAnimationClipByVisualId(visualId, "idle");
            AnimationClip castClip = FindAnimationClipByVisualId(visualId, "cast");
            AnimationClip attackClip = FindAnimationClipByVisualId(visualId, "attack");
            AnimationClip loopClip = generatedLoopClip != null
                ? generatedLoopClip
                : FindAnimationClipByVisualId(visualId, "loop");
            AnimationClip hitClip = FindAnimationClipByVisualId(visualId, "hit");

            List<AnimationClipEntry> entries = new();
            AddAnimationClipEntry(entries, SkillAnimationClipType.Idle, idleClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Cast, castClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Attack, attackClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.ProjectileLoop, loopClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Hit, hitClip);

            return entries.ToArray();
        }

        private static AnimationClip RecreateAnimationClipFromSpriteSheet(
            string visualId,
            string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(visualId) || string.IsNullOrWhiteSpace(outputFolder))
            {
                return null;
            }

            string skillId = RemoveVisualSuffix(visualId);
            string spriteSheetPath = $"{SkillAnimationSpriteFolder}/{skillId}.animation.png";

            if (AssetDatabase.LoadAssetAtPath<Texture2D>(spriteSheetPath) == null)
            {
                Debug.LogWarning(
                    $"[SkillBaseVisualAssetBuilder] Animation sprite sheet not found. " +
                    $"visualId={visualId}, path={spriteSheetPath}");
                return null;
            }

            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spriteSheetPath)
                .OfType<Sprite>()
                .OrderBy(sprite => ExtractTrailingNumber(sprite.name))
                .ThenBy(sprite => sprite.name, StringComparer.Ordinal)
                .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning(
                    $"[SkillBaseVisualAssetBuilder] No sliced sprites found: {spriteSheetPath}");
                return null;
            }

            string clipName = $"{visualId}.loop";
            string clipPath = Path.Combine(outputFolder, clipName + ".anim")
                .Replace("\\", "/");

            AnimationClip clip = AnimationClipAssetHelper.RecreateSpriteAnimationClip(
                clipPath,
                sprites,
                SkillAnimationFrameRate,
                true);

            if (clip != null)
            {
                Debug.Log(
                    $"[SkillBaseVisualAssetBuilder] Recreated animation clip: " +
                    $"{clipPath} / Frames: {sprites.Length} / Source: {spriteSheetPath}");
            }

            return clip;
        }

        private static void DeleteGeneratedAnimationClip(
            string visualId,
            string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(visualId) || string.IsNullOrWhiteSpace(outputFolder))
            {
                return;
            }

            string clipPath = Path.Combine(
                    outputFolder,
                    $"{visualId}.loop.anim")
                .Replace("\\", "/");

            AnimationClipAssetHelper.DeleteAssetIfExists(clipPath);
        }

        private static string RemoveVisualSuffix(string visualId)
        {
            const string suffix = ".visual";
            string trimmed = visualId.Trim();

            return trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? trimmed.Substring(0, trimmed.Length - suffix.Length)
                : trimmed;
        }

        private static int ExtractTrailingNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return int.MaxValue;
            }

            int endIndex = value.Length - 1;

            while (endIndex >= 0 && char.IsDigit(value[endIndex]))
            {
                endIndex--;
            }

            if (endIndex == value.Length - 1)
            {
                return int.MaxValue;
            }

            string numberText = value.Substring(endIndex + 1);
            return int.TryParse(numberText, out int number)
                ? number
                : int.MaxValue;
        }

        private static void AddAnimationClipEntry(
            System.Collections.Generic.List<AnimationClipEntry> entries,
            SkillAnimationClipType clipType,
            AnimationClip clip)
        {
            if (entries == null || clip == null)
            {
                return;
            }

            entries.Add(new AnimationClipEntry(
                clipType,
                clip));
        }

        private static AnimationClip FindAnimationClipByVisualId(
            string visualId,
            string suffix)
        {
            if (string.IsNullOrWhiteSpace(visualId) || string.IsNullOrWhiteSpace(suffix))
            {
                return null;
            }

            return FindAnimationClipByName($"{visualId}.{suffix}");
        }

        private static AnimationClip FindAnimationClipByName(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{clipName} t:AnimationClip");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

                if (clip != null && clip.name == clipName)
                {
                    return clip;
                }
            }

            return null;
        }

        private static void EnsureFolder(string folderPath)
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
                Debug.LogError($"[SkillBaseVisualAssetBuilder] Folder path must start with Assets: {folderPath}");
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

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "skill.base.visual";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}
