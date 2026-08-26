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

        public string sortingRelation;
    }

    /// <summary>
    /// 스킬 기본 시각 요소 전용 에셋 빌더.
    /// JSON의 base visual 데이터를 BaseVisualSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillBaseVisualAssetBuilder
    {
        private const string SkillAnimationFrameRoot = "Assets/ImagesGenerated/Skill/animation";
        private const string SkillAnimationClipFolder = "Assets/AnimationClips/Skill";
        private const float SkillAnimationFrameRate = 12f;
        private const float SkillAnimationPixelsPerUnit = 100f;
        private static readonly float[] SixFrameEmphasisDurations =
        {
            0.07f,
            0.09f,
            0.24f,
            0.26f,
            0.09f,
            0.07f
        };
        private static readonly float[] FourFrameLoopDurations =
        {
            0.12f,
            0.12f,
            0.12f,
            0.12f
        };

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

            BaseVisualSO visualSo = AssetDatabase.LoadAssetAtPath<BaseVisualSO>(assetPath);
            bool isNewAsset = visualSo == null;

            if (isNewAsset)
            {
                visualSo = ScriptableObject.CreateInstance<BaseVisualSO>();
                AssetDatabase.CreateAsset(visualSo, assetPath);
            }

            Apply(visualSo, json, generateAnimation);

            EditorUtility.SetDirty(visualSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                isNewAsset
                    ? $"[SkillBaseVisualAssetBuilder] Created BaseVisualSO: {assetPath}"
                    : $"[SkillBaseVisualAssetBuilder] Updated BaseVisualSO in place: {assetPath}");

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
            bool generateAnimation)
        {
            if (visualSo == null || json == null)
            {
                return;
            }

            ProjectileVisualType projectileVisualType = ResolveProjectileVisualType(json.projectileVisualType);
            SkillSortingRelation sortingRelation = ResolveSortingRelation(json.sortingRelation);
            AnimationClipEntry[] animationClips;

            if (generateAnimation)
            {
                animationClips = CreateAnimationClipEntries(
                    json.visualId);
            }
            else
            {
                animationClips = Array.Empty<AnimationClipEntry>();
                Debug.Log(
                    $"[SkillBaseVisualAssetBuilder] Skipped skill animation generation: " +
                    $"visualId={json.visualId}. Existing animation clip assets were preserved.");
            }

            visualSo.ApplyEditorData(
                json.visualId,
                projectileVisualType,
                sortingRelation,
                animationClips);
        }

        private static SkillSortingRelation ResolveSortingRelation(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return SkillSortingRelation.SameAsOwner;
            }

            if (Enum.TryParse(value, true, out SkillSortingRelation relation))
            {
                return relation;
            }

            Debug.LogError(
                $"[SkillBaseVisualAssetBuilder] Invalid sortingRelation. value={value}");
            return SkillSortingRelation.SameAsOwner;
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
            string visualId)
        {
            AnimationClip generatedLoopClip = CreateOrUpdateAnimationClipFromFrames(visualId);

            AnimationClip idleClip = FindAnimationClipByVisualId(visualId, "idle");
            AnimationClip castClip = FindAnimationClipByVisualId(visualId, "cast");
            AnimationClip attackClip = FindAnimationClipByVisualId(visualId, "attack");
            AnimationClip loopClip = generatedLoopClip != null
                ? generatedLoopClip
                : LoadGeneratedLoopClip(visualId);
            AnimationClip hitClip = FindAnimationClipByVisualId(visualId, "hit");

            List<AnimationClipEntry> entries = new();
            AddAnimationClipEntry(entries, SkillAnimationClipType.Idle, idleClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Cast, castClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Attack, attackClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.ProjectileLoop, loopClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Hit, hitClip);

            return entries.ToArray();
        }

        private static AnimationClip LoadGeneratedLoopClip(string visualId)
        {
            if (string.IsNullOrWhiteSpace(visualId))
            {
                return null;
            }

            string clipPath = Path.Combine(
                    SkillAnimationClipFolder,
                    $"{visualId}.loop.anim")
                .Replace("\\", "/");

            return AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        }

        private static AnimationClip CreateOrUpdateAnimationClipFromFrames(string visualId)
        {
            if (string.IsNullOrWhiteSpace(visualId))
            {
                return null;
            }

            string skillId = RemoveVisualSuffix(visualId);
            string frameFolder = $"{SkillAnimationFrameRoot}/{skillId}";

            if (!AssetDatabase.IsValidFolder(frameFolder))
            {
                Debug.LogWarning(
                    $"[SkillBaseVisualAssetBuilder] Animation frame folder not found. " +
                    $"visualId={visualId}, path={frameFolder}");
                return null;
            }

            string[] framePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { frameFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(
                    Path.GetDirectoryName(path)?.Replace("\\", "/"),
                    frameFolder,
                    StringComparison.Ordinal))
                .Where(path => TryExtractFrameNumber(Path.GetFileNameWithoutExtension(path), out _))
                .OrderBy(path => GetFrameNumber(path))
                .ThenBy(path => path, StringComparer.Ordinal)
                .ToArray();

            if (framePaths.Length == 0)
            {
                Debug.LogWarning(
                    $"[SkillBaseVisualAssetBuilder] No animation frame textures found. " +
                    $"Expected frame-{{number}}.png or frame_{{number}}.png directly in: {frameFolder}");
                return null;
            }

            if (!ValidateFrameSequence(framePaths, frameFolder))
            {
                return null;
            }

            ConfigureFrameImporters(framePaths);

            Sprite[] sprites = framePaths
                .Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path))
                .Where(sprite => sprite != null)
                .ToArray();

            if (sprites.Length != framePaths.Length || !ValidateFrameDimensions(sprites, frameFolder))
            {
                Debug.LogError(
                    $"[SkillBaseVisualAssetBuilder] Some animation frames could not be imported as matching single Sprites: " +
                    $"{frameFolder} ({sprites.Length}/{framePaths.Length})");
                return null;
            }

            string clipName = $"{visualId}.loop";
            string clipPath = Path.Combine(SkillAnimationClipFolder, clipName + ".anim")
                .Replace("\\", "/");

            IReadOnlyList<float> frameDurations = sprites.Length == FourFrameLoopDurations.Length
                ? FourFrameLoopDurations
                : sprites.Length == SixFrameEmphasisDurations.Length
                    ? SixFrameEmphasisDurations
                    : null;

            AnimationClip clip = frameDurations != null
                ? AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClipWithDurations(
                    clipPath,
                    sprites,
                    frameDurations,
                    SkillAnimationFrameRate,
                    true)
                : AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClip(
                    clipPath,
                    sprites,
                    SkillAnimationFrameRate,
                    true);

            if (clip != null)
            {
                Debug.Log(
                    $"[SkillBaseVisualAssetBuilder] Created or updated animation clip: " +
                    $"{clipPath} / Frames: {sprites.Length} / Source: {frameFolder} / " +
                    $"Timing: {(sprites.Length == FourFrameLoopDurations.Length ? "four-frame loop" : sprites.Length == SixFrameEmphasisDurations.Length ? "six-frame emphasis" : "fixed frame rate")}");
            }

            return clip;
        }

        private static bool ValidateFrameSequence(
            IReadOnlyList<string> framePaths,
            string frameFolder)
        {
            HashSet<int> frameNumbers = new();

            for (int i = 0; i < framePaths.Count; i++)
            {
                int frameNumber = GetFrameNumber(framePaths[i]);

                if (!frameNumbers.Add(frameNumber))
                {
                    Debug.LogError(
                        $"[SkillBaseVisualAssetBuilder] Duplicate animation frame number {frameNumber}: {frameFolder}");
                    return false;
                }
            }

            for (int expected = 0; expected < framePaths.Count; expected++)
            {
                if (frameNumbers.Contains(expected))
                {
                    continue;
                }

                Debug.LogError(
                    $"[SkillBaseVisualAssetBuilder] Missing animation frame-{expected}: {frameFolder}");
                return false;
            }

            return true;
        }

        private static void ConfigureFrameImporters(IReadOnlyList<string> framePaths)
        {
            for (int i = 0; i < framePaths.Count; i++)
            {
                string framePath = framePaths[i];
                TextureImporter importer = AssetImporter.GetAtPath(framePath) as TextureImporter;

                if (importer == null)
                {
                    Debug.LogError($"[SkillBaseVisualAssetBuilder] TextureImporter not found: {framePath}");
                    continue;
                }

                bool changed = false;
                changed |= SetIfDifferent(importer.textureType, TextureImporterType.Sprite,
                    value => importer.textureType = value);
                changed |= SetIfDifferent(importer.spriteImportMode, SpriteImportMode.Single,
                    value => importer.spriteImportMode = value);
                changed |= SetIfDifferent(importer.spritePixelsPerUnit, SkillAnimationPixelsPerUnit,
                    value => importer.spritePixelsPerUnit = value);
                changed |= SetIfDifferent(importer.alphaIsTransparency, true,
                    value => importer.alphaIsTransparency = value);
                changed |= SetIfDifferent(importer.mipmapEnabled, false,
                    value => importer.mipmapEnabled = value);
                changed |= SetIfDifferent(importer.textureCompression, TextureImporterCompression.Uncompressed,
                    value => importer.textureCompression = value);

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static bool ValidateFrameDimensions(
            IReadOnlyList<Sprite> sprites,
            string frameFolder)
        {
            if (sprites.Count == 0)
            {
                return false;
            }

            Rect expectedRect = sprites[0].rect;

            for (int i = 1; i < sprites.Count; i++)
            {
                Rect rect = sprites[i].rect;

                if (Mathf.Approximately(rect.width, expectedRect.width) &&
                    Mathf.Approximately(rect.height, expectedRect.height))
                {
                    continue;
                }

                Debug.LogError(
                    $"[SkillBaseVisualAssetBuilder] Animation frame dimensions do not match: " +
                    $"expected={expectedRect.width}x{expectedRect.height}, " +
                    $"actual={rect.width}x{rect.height}, sprite={sprites[i].name}, folder={frameFolder}");
                return false;
            }

            return true;
        }

        private static bool SetIfDifferent<T>(T current, T expected, Action<T> setter)
        {
            if (EqualityComparer<T>.Default.Equals(current, expected))
            {
                return false;
            }

            setter(expected);
            return true;
        }

        private static string RemoveVisualSuffix(string visualId)
        {
            string trimmed = visualId.Trim();
            const string baseSuffix = ".visual.base";
            if (trimmed.EndsWith(baseSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(0, trimmed.Length - baseSuffix.Length);
            }

            const string suffix = ".visual";
            return trimmed.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? trimmed.Substring(0, trimmed.Length - suffix.Length)
                : trimmed;
        }

        private static int GetFrameNumber(string assetPath)
        {
            return TryExtractFrameNumber(Path.GetFileNameWithoutExtension(assetPath), out int number)
                ? number : int.MaxValue;
        }

        private static bool TryExtractFrameNumber(string fileName, out int number)
        {
            number = -1;
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            const string dashPrefix = "frame-";
            const string underscorePrefix = "frame_";
            string numberText;

            if (fileName.StartsWith(dashPrefix, StringComparison.OrdinalIgnoreCase))
                numberText = fileName.Substring(dashPrefix.Length);
            else if (fileName.StartsWith(underscorePrefix, StringComparison.OrdinalIgnoreCase))
                numberText = fileName.Substring(underscorePrefix.Length);
            else
                return false;

            return numberText.Length > 0 && numberText.All(char.IsDigit) &&
                   int.TryParse(numberText, out number);
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
