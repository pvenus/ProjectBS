using System;
using System.IO;
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
        public static BaseVisualSO CreateOrUpdate(
            BaseVisualJson json,
            string outputFolder)
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

            BaseVisualSO visualSo =
                AssetDatabase.LoadAssetAtPath<BaseVisualSO>(assetPath);

            if (visualSo == null)
            {
                visualSo = ScriptableObject.CreateInstance<BaseVisualSO>();
                AssetDatabase.CreateAsset(visualSo, assetPath);
            }

            Apply(visualSo, json);

            EditorUtility.SetDirty(visualSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillBaseVisualAssetBuilder] Updated BaseVisualSO: {assetPath}");

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
            BaseVisualJson json)
        {
            if (visualSo == null || json == null)
            {
                return;
            }

            ProjectileVisualType projectileVisualType = ResolveProjectileVisualType(json.projectileVisualType);
            AnimationClipEntry[] animationClips = CreateAnimationClipEntries(json.visualId);

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
        private static AnimationClipEntry[] CreateAnimationClipEntries(string visualId)
        {
            AnimationClip idleClip = FindAnimationClipByVisualId(visualId, "idle");
            AnimationClip castClip = FindAnimationClipByVisualId(visualId, "cast");
            AnimationClip attackClip = FindAnimationClipByVisualId(visualId, "attack");
            AnimationClip loopClip = FindAnimationClipByVisualId(visualId, "loop");
            AnimationClip hitClip = FindAnimationClipByVisualId(visualId, "hit");

            System.Collections.Generic.List<AnimationClipEntry> entries = new();
            AddAnimationClipEntry(entries, SkillAnimationClipType.Idle, idleClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Cast, castClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Attack, attackClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.ProjectileLoop, loopClip);
            AddAnimationClipEntry(entries, SkillAnimationClipType.Hit, hitClip);

            return entries.ToArray();
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