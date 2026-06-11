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

        public string archetype;
        public string projectileVisualType;
        public string projectilePrefab;
        public string projectileSprite;
        public string idleClip;
        public string castClip;
        public string attackClip;
        public string projectileLoopClip;
        public string hitClip;

        public string prefabName;
        public string spriteName;
        public string animationClipName;

        public float scale = 1f;
        public float offsetX;
        public float offsetY;
        public float rotationOffset;

        public bool followOwner;
        public bool followTarget;
        public bool useDirectionRotation;
        public bool destroyOnSkillEnd = true;
    }

    /// <summary>
    /// 스킬 기본 시각 요소 전용 에셋 빌더.
    /// JSON의 base visual 데이터를 BaseVisualSO 에셋으로 생성/갱신한다.
    /// </summary>
    public static class SkillBaseVisualAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
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

            Type baseVisualType = FindType("BaseVisualSO");

            if (baseVisualType == null)
            {
                Debug.LogError("[SkillBaseVisualAssetBuilder] BaseVisualSO type not found.");
                return null;
            }

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            ScriptableObject visualSo =
                AssetDatabase.LoadAssetAtPath(assetPath, baseVisualType) as ScriptableObject;

            if (visualSo == null)
            {
                visualSo = ScriptableObject.CreateInstance(baseVisualType);
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

            string prefabName = ResolvePrefabName(json);

            if (!string.IsNullOrWhiteSpace(prefabName))
            {
                return SanitizeFileName(prefabName + ".base_visual");
            }

            return "skill.base.visual";
        }

        private static void Apply(
            ScriptableObject visualSo,
            BaseVisualJson json)
        {
            string prefabName = ResolvePrefabName(json);
            string animationClipName = ResolveAnimationClipName(json);

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.visualId,
                "visualId",
                "baseVisualId",
                "id");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.archetype,
                "archetype",
                "attackArchetype");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.projectileVisualType,
                "projectileVisualType");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindPrefabByName(prefabName),
                "prefab",
                "visualPrefab",
                "effectPrefab",
                "projectilePrefab");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindAnimationClipByName(animationClipName),
                "animationClip",
                "clip",
                "spawnAnimationClip",
                "idleClip");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindAnimationClipByName(json.idleClip),
                "idleClip");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindAnimationClipByName(json.castClip),
                "castClip");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindAnimationClipByName(json.attackClip),
                "attackClip");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindAnimationClipByName(json.projectileLoopClip),
                "projectileLoopClip",
                "loopClip");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                FindAnimationClipByName(json.hitClip),
                "hitClip");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.scale,
                "scale",
                "visualScale");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                new Vector2(json.offsetX, json.offsetY),
                "offset",
                "visualOffset",
                "spawnOffset");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.offsetX,
                "offsetX",
                "visualOffsetX");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.offsetY,
                "offsetY",
                "visualOffsetY");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.rotationOffset,
                "rotationOffset",
                "visualRotationOffset");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.followOwner,
                "followOwner");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.followTarget,
                "followTarget");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.useDirectionRotation,
                "useDirectionRotation",
                "applyDirectionRotation");

            EditorFieldSetter.SetFirstExistingField(
                visualSo,
                json.destroyOnSkillEnd,
                "destroyOnSkillEnd",
                "destroyOnEnd");
        }

        private static string ResolvePrefabName(BaseVisualJson json)
        {
            if (json == null)
            {
                return null;
            }

            return !string.IsNullOrWhiteSpace(json.projectilePrefab)
                ? json.projectilePrefab
                : json.prefabName;
        }

        private static string ResolveAnimationClipName(BaseVisualJson json)
        {
            if (json == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(json.animationClipName))
            {
                return json.animationClipName;
            }

            if (!string.IsNullOrWhiteSpace(json.projectileLoopClip))
            {
                return json.projectileLoopClip;
            }

            if (!string.IsNullOrWhiteSpace(json.idleClip))
            {
                return json.idleClip;
            }

            return null;
        }

        private static GameObject FindPrefabByName(string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null && prefab.name == prefabName)
                {
                    return prefab;
                }
            }

            return null;
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

        private static Type FindType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);

                if (type != null)
                {
                    return type;
                }

                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Name == typeName)
                    {
                        return types[i];
                    }
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