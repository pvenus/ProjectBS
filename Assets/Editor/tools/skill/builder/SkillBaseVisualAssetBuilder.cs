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

        // Optional, explicit source-of-truth binding. Missing keeps the legacy
        // profile already serialized on BaseVisualSO.
        public string animationVfxProfile;

        public string layeredPresentationProfile;
    }

    /// <summary>
    /// 스킬 기본 시각 요소 전용 에셋 빌더.
    /// JSON의 base visual 데이터를 BaseVisualSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillBaseVisualAssetBuilder
    {
        private const string SkillAnimationFrameRoot = "Assets/ImagesGenerated/Skill/animation";
        private const string SkillAnimationClipFolder = "Assets/AnimationClips/Skill";
        private const string CharacterAnimationFrameRoot = "Assets/ImagesGenerated/Character/animation/character.seojin.1";
        private const string CharacterAnimationClipFolder = "Assets/AnimationClips/Character/character.seojin.1";
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
        private static readonly float[] SwiftStepDurations =
        {
            0.08f,
            0.05f,
            0.07f,
            0.08f,
            0.08f,
            0.06f
        };
        private static readonly float[] CommandChainDurations = { .07f, .07f, .04f, .12f, .06f, .06f };
        private static readonly float[] ThunderCommandDurations = { .04f, .04f, .04f, .04f, .08f, .18f };
        private static readonly float[] ThunderFollowupDurations = { .03f, .03f, .02f, .02f, .04f, .04f };
        private static readonly float[] BlockadeCutDurations = { .05f, .05f, .04f, .14f, .09f, .09f };
        private static readonly float[] JangdanDungDurations = { .05f, .05f, .05f, .10f, .10f, .15f };
        private static readonly float[] JangdanGiDurations = { .025f, .025f, .025f, .05f, .05f, .075f };
        private static readonly float[] JangdanDeokDurations = { .04f, .04f, .045f, .075f, .075f, .10f };
        private static readonly float[] JangdanSequenceDurations =
            { .04f, .04f, .045f, .05f, .05f, .075f, .05f, .05f, .05f, .075f, .10f, .125f };
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

        public static string CreateOrUpdateMouse3BodyActionClip(string skillId)
        {
            string action;
            IReadOnlyList<float> durations;
            if (skillId.EndsWith(".active_5.command_chain", StringComparison.Ordinal))
            {
                action = "command_chain";
                durations = CommandChainDurations;
            }
            else if (skillId.EndsWith(".active_6.thunder_command", StringComparison.Ordinal))
            {
                action = "thunder_command";
                durations = ThunderCommandDurations;
            }
            else if (skillId.EndsWith(".active_7.blockade_cut", StringComparison.Ordinal))
            {
                action = "blockade_cut";
                durations = BlockadeCutDurations;
            }
            else if (skillId.EndsWith(".active_5.jangdan_dung", StringComparison.Ordinal))
            {
                action = "jangdan/active5_dung_kung";
                durations = JangdanDungDurations;
            }
            else if (skillId.EndsWith(".active_6.jangdan_gi", StringComparison.Ordinal))
            {
                action = "jangdan/active6_gi";
                durations = JangdanGiDurations;
            }
            else if (skillId.EndsWith(".active_7.jangdan_deok", StringComparison.Ordinal))
            {
                action = "jangdan/active7_deok";
                durations = JangdanDeokDurations;
            }
            else if (skillId.EndsWith(".active_8.deoreoreoreo", StringComparison.Ordinal))
            {
                action = "jangdan/active8_deoreoreoreo";
                durations = JangdanSequenceDurations;
            }
            else
            {
                return null;
            }

            string frameFolder = $"{CharacterAnimationFrameRoot}/{action}";
            string[] framePaths = AssetDatabase.FindAssets("t:Texture2D", new[] { frameFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(Path.GetDirectoryName(path)?.Replace("\\", "/"), frameFolder, StringComparison.Ordinal))
                .Where(path => TryExtractFrameNumber(Path.GetFileNameWithoutExtension(path), out _))
                .OrderBy(GetFrameNumber)
                .ToArray();
            int expectedCount = action == "thunder_command" || action == "jangdan/active5_dung_kung" ||
                action == "jangdan/active8_deoreoreoreo" ? 12 : 6;
            if (framePaths.Length != expectedCount || !ValidateFrameSequence(framePaths, frameFolder))
            {
                Debug.LogError($"[SkillBaseVisualAssetBuilder] Mouse3 body exact{expectedCount} is incomplete: {frameFolder} ({framePaths.Length}/{expectedCount})");
                return null;
            }

            ConfigureFrameImporters(framePaths);
            Sprite[] sprites = framePaths.Select(AssetDatabase.LoadAssetAtPath<Sprite>).ToArray();
            if (sprites.Any(sprite => sprite == null) || !ValidateFrameDimensions(sprites, frameFolder))
            {
                Debug.LogError($"[SkillBaseVisualAssetBuilder] Mouse3 body sprites failed import validation: {frameFolder}");
                return null;
            }

            string clipToken = action.Replace("jangdan/", "jangdan.");
            string clipPath = $"{CharacterAnimationClipFolder}/character.seojin.1.body_action.{clipToken}.anim";
            bool splitFollowup = action == "thunder_command" || action == "jangdan/active5_dung_kung";
            Sprite[] primarySprites = splitFollowup ? sprites.Take(6).ToArray() : sprites;
            AnimationClip clip = AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClipWithDurations(
                clipPath, primarySprites, durations, SkillAnimationFrameRate, false);
            if (clip == null)
            {
                return null;
            }

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.startTime = 0f;
            settings.stopTime = durations.Sum();
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            if (splitFollowup)
            {
                string followupPath = $"{CharacterAnimationClipFolder}/character.seojin.1.body_action.{clipToken}.followup.anim";
                AnimationClip followup = AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClipWithDurations(
                    followupPath, sprites.Skip(6).Take(6).ToArray(),
                    action == "thunder_command" ? ThunderFollowupDurations : JangdanDungDurations,
                    SkillAnimationFrameRate, false);
                if (followup == null) return null;
            }
            return clipPath;
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

            bool intentionalNone = projectileVisualType == ProjectileVisualType.None;
            if (generateAnimation && !intentionalNone)
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
            if (!string.IsNullOrWhiteSpace(json.animationVfxProfile))
            {
                visualSo.ApplyAnimationVfxProfileEditor(
                    ResolveAnimationVfxProfile(json.animationVfxProfile));
            }
            if (!string.IsNullOrWhiteSpace(json.layeredPresentationProfile))
            {
                visualSo.ApplyLayeredPresentationProfileEditor(
                    ResolveLayeredPresentationProfile(json.layeredPresentationProfile));
            }
            if (intentionalNone)
            {
                // None is an authored policy, not a missing-asset condition. Clear
                // stale clip/profile references so regeneration stays idempotent.
                visualSo.DisableProjectilePresentationEditor();
            }
        }

        private static SkillAnimationVfxProfileSO ResolveAnimationVfxProfile(string profileId)
        {
            string[] guids = AssetDatabase.FindAssets("t:SkillAnimationVfxProfileSO");
            Array.Sort(guids, StringComparer.Ordinal);
            SkillAnimationVfxProfileSO resolved = null;
            for (int i = 0; i < guids.Length; i++)
            {
                SkillAnimationVfxProfileSO candidate = AssetDatabase.LoadAssetAtPath<SkillAnimationVfxProfileSO>(
                    AssetDatabase.GUIDToAssetPath(guids[i]));
                if (candidate == null || !string.Equals(candidate.ProfileId, profileId, StringComparison.Ordinal))
                    continue;
                if (resolved != null && resolved != candidate)
                    throw new InvalidOperationException($"Duplicate animation VFX profile '{profileId}'.");
                resolved = candidate;
            }
            if (resolved == null)
                throw new InvalidOperationException($"Missing animation VFX profile '{profileId}'.");
            return resolved;
        }

        private static LayeredProjectilePresentationProfileSO ResolveLayeredPresentationProfile(
            string profileId)
        {
            string[] guids = AssetDatabase.FindAssets("t:LayeredProjectilePresentationProfileSO");
            Array.Sort(guids, StringComparer.Ordinal);
            LayeredProjectilePresentationProfileSO resolved = null;
            for (int i = 0; i < guids.Length; i++)
            {
                LayeredProjectilePresentationProfileSO candidate =
                    AssetDatabase.LoadAssetAtPath<LayeredProjectilePresentationProfileSO>(
                        AssetDatabase.GUIDToAssetPath(guids[i]));
                if (candidate == null || !string.Equals(candidate.ProfileId, profileId,
                        StringComparison.Ordinal)) continue;
                if (resolved != null && resolved != candidate)
                    throw new InvalidOperationException(
                        $"Duplicate layered presentation profile '{profileId}'.");
                resolved = candidate;
            }
            if (resolved == null)
                throw new InvalidOperationException(
                    $"Missing layered presentation profile '{profileId}'.");
            return resolved;
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

            bool splitJangdanDung = skillId.EndsWith(".active_5.jangdan_dung", StringComparison.Ordinal)
                && sprites.Length == 12;
            if ((skillId.EndsWith(".active_6.thunder_command", StringComparison.Ordinal) && sprites.Length == 12)
                || splitJangdanDung)
            {
                string followupPath = Path.Combine(SkillAnimationClipFolder,
                    $"{visualId}.followup.anim").Replace("\\", "/");
                AnimationClip followup = AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClipWithDurations(
                    followupPath, sprites.Skip(6).Take(6).ToArray(),
                    splitJangdanDung ? JangdanDungDurations : ThunderFollowupDurations,
                    SkillAnimationFrameRate, false);
                if (followup == null) return null;
                sprites = sprites.Take(6).ToArray();
            }

            string clipName = $"{visualId}.loop";
            string clipPath = Path.Combine(SkillAnimationClipFolder, clipName + ".anim")
                .Replace("\\", "/");

            bool isSwiftStep = skillId.EndsWith(".active_4.swift_step", StringComparison.Ordinal);
            bool isJangdanDung = skillId.EndsWith(".active_5.jangdan_dung", StringComparison.Ordinal);
            bool isJangdanGi = skillId.EndsWith(".active_6.jangdan_gi", StringComparison.Ordinal);
            bool isJangdanDeok = skillId.EndsWith(".active_7.jangdan_deok", StringComparison.Ordinal);
            bool isJangdanSequence = skillId.EndsWith(".active_8.deoreoreoreo", StringComparison.Ordinal);
            IReadOnlyList<float> mouse3Durations = skillId.EndsWith(".active_5.command_chain", StringComparison.Ordinal)
                ? CommandChainDurations
                : skillId.EndsWith(".active_6.thunder_command", StringComparison.Ordinal)
                    ? ThunderCommandDurations
                    : skillId.EndsWith(".active_7.blockade_cut", StringComparison.Ordinal)
                        ? BlockadeCutDurations
                        : null;
            IReadOnlyList<float> frameDurations = isJangdanDung
                ? JangdanDungDurations
                : isJangdanGi ? JangdanGiDurations
                : isJangdanDeok ? JangdanDeokDurations
                : isJangdanSequence ? JangdanSequenceDurations
                : isSwiftStep
                ? SwiftStepDurations
                : mouse3Durations != null
                    ? mouse3Durations
                : sprites.Length == FourFrameLoopDurations.Length
                ? FourFrameLoopDurations
                : sprites.Length == SixFrameEmphasisDurations.Length
                    ? SixFrameEmphasisDurations
                    : null;
            bool loopTime = isJangdanDung ||
                (!isSwiftStep && !isJangdanGi && !isJangdanDeok &&
                 !isJangdanSequence && mouse3Durations == null);

            AnimationClip clip = frameDurations != null
                ? AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClipWithDurations(
                    clipPath,
                    sprites,
                    frameDurations,
                    SkillAnimationFrameRate,
                    loopTime)
                : AnimationClipAssetHelper.CreateOrUpdateSpriteAnimationClip(
                    clipPath,
                    sprites,
                    SkillAnimationFrameRate,
                    loopTime);

            if (clip != null)
            {
                if (isSwiftStep || isJangdanDung || isJangdanGi ||
                    isJangdanDeok || isJangdanSequence)
                {
                    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.startTime = 0f;
                    settings.stopTime = isSwiftStep ? 0.42f : frameDurations.Sum();
                    settings.loopTime = isJangdanDung;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    EditorUtility.SetDirty(clip);
                }

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
