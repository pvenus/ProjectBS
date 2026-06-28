

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    /// <summary>
    /// ImpactVisualSO 전용 에셋 빌더.
    /// SkillVisualSetAssetBuilder의 ImpactVisualJson 구조와 동일한 데이터를 사용한다.
    /// </summary>
    public static class SkillImpactVisualAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            ImpactVisualJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillImpactVisualAssetBuilder] ImpactVisual json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillImpactVisualAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            Type impactVisualType = FindType("ImpactVisualSO");

            if (impactVisualType == null)
            {
                Debug.LogError("[SkillImpactVisualAssetBuilder] ImpactVisualSO type not found.");
                return null;
            }

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            ScriptableObject impactSo =
                AssetDatabase.LoadAssetAtPath(assetPath, impactVisualType) as ScriptableObject;

            if (impactSo == null)
            {
                impactSo = ScriptableObject.CreateInstance(impactVisualType);
                AssetDatabase.CreateAsset(impactSo, assetPath);
            }

            Apply(impactSo, json);

            EditorUtility.SetDirty(impactSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillImpactVisualAssetBuilder] Updated ImpactVisualSO: {assetPath}");

            return impactSo;
        }

        private static string ResolveAssetName(ImpactVisualJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.visualId))
            {
                return SanitizeFileName(json.visualId);
            }

            if (!string.IsNullOrWhiteSpace(json.prefabName))
            {
                return SanitizeFileName(json.prefabName + ".impact_visual");
            }

            if (!string.IsNullOrWhiteSpace(json.spriteName))
            {
                return SanitizeFileName(json.spriteName + ".impact_visual");
            }

            return "skill.impact.visual";
        }

        private static void Apply(
            ScriptableObject impactSo,
            ImpactVisualJson json)
        {
            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                json.visualId,
                "visualId",
                "impactVisualId",
                "id");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                FindPrefabByName(json.prefabName),
                "prefab",
                "impactPrefab",
                "visualPrefab",
                "effectPrefab");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                FindSpriteByName(json.spriteName),
                "sprite",
                "impactSprite",
                "visualSprite");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                FindAnimationClipByName(json.animationClipName),
                "animationClip",
                "clip",
                "impactAnimationClip");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                json.scale,
                "scale",
                "visualScale");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                new Vector2(json.offsetX, json.offsetY),
                "offset",
                "visualOffset",
                "impactOffset");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                json.offsetX,
                "offsetX",
                "visualOffsetX");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                json.offsetY,
                "offsetY",
                "visualOffsetY");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                json.rotationOffset,
                "rotationOffset",
                "visualRotationOffset");

            EditorFieldSetter.SetFirstExistingField(
                impactSo,
                json.useDirectionRotation,
                "useDirectionRotation",
                "applyDirectionRotation");
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

            Debug.LogWarning($"[SkillImpactVisualAssetBuilder] Prefab not found: {prefabName}");
            return null;
        }

        private static Sprite FindSpriteByName(string spriteName)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite != null && sprite.name == spriteName)
                {
                    return sprite;
                }
            }

            Debug.LogWarning($"[SkillImpactVisualAssetBuilder] Sprite not found: {spriteName}");
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

            Debug.LogWarning($"[SkillImpactVisualAssetBuilder] AnimationClip not found: {clipName}");
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
                Debug.LogError($"[SkillImpactVisualAssetBuilder] Folder path must start with Assets: {folderPath}");
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
                return "skill.impact.visual";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}