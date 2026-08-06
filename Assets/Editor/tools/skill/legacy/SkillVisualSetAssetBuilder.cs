using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    [Serializable]
    public class VisualSetJson
    {
        public string visualSetId;

        public BaseVisualJson baseVisual;
        public ImpactVisualJson impactVisual;
        public TrailVisualJson trailVisual;
    }

    [Serializable]
    public class ImpactVisualJson
    {
        public string visualId;
        public string prefabName;
        public string spriteName;
        public string animationClipName;
        public float scale = 1f;
        public float offsetX;
        public float offsetY;
        public float rotationOffset;
        public bool useDirectionRotation;
    }

    [Serializable]
    public class TrailVisualJson
    {
        public string visualId;
        public string prefabName;
        public string materialName;
        public string spriteName;
        public float width = 1f;
        public float duration = 0.3f;
        public bool followProjectile = true;
    }

    /// <summary>
    /// 스킬 시각 세트 전용 에셋 빌더.
    /// Base/Impact/Trail visual 에셋을 생성한 뒤 SkillVisualSetSO에 연결한다.
    /// </summary>
    public static class SkillVisualSetAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            VisualSetJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillVisualSetAssetBuilder] VisualSet json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillVisualSetAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            Type visualSetType = FindType("SkillVisualSetSO");

            if (visualSetType == null)
            {
                Debug.LogError("[SkillVisualSetAssetBuilder] SkillVisualSetSO type not found.");
                return null;
            }

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            ScriptableObject visualSetSo =
                AssetDatabase.LoadAssetAtPath(assetPath, visualSetType) as ScriptableObject;

            if (visualSetSo == null)
            {
                visualSetSo = ScriptableObject.CreateInstance(visualSetType);
                AssetDatabase.CreateAsset(visualSetSo, assetPath);
            }

            ScriptableObject baseVisualSo =
                SkillBaseVisualAssetBuilder.CreateOrUpdate(json.baseVisual, outputFolder);

            ScriptableObject impactVisualSo =
                SkillImpactVisualAssetBuilder.CreateOrUpdate(json.impactVisual, outputFolder);

            ScriptableObject trailVisualSo =
                SkillTrailVisualAssetBuilder.CreateOrUpdate(json.trailVisual, outputFolder);

            Apply(
                visualSetSo,
                json,
                baseVisualSo,
                impactVisualSo,
                trailVisualSo);

            EditorUtility.SetDirty(visualSetSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillVisualSetAssetBuilder] Updated SkillVisualSetSO: {assetPath}");

            return visualSetSo;
        }

        private static void Apply(
            ScriptableObject visualSetSo,
            VisualSetJson json,
            ScriptableObject baseVisualSo,
            ScriptableObject impactVisualSo,
            ScriptableObject trailVisualSo)
        {
            EditorFieldSetter.SetFirstExistingField(
                visualSetSo,
                json.visualSetId,
                "visualSetId",
                "skillVisualSetId",
                "id");

            EditorFieldSetter.SetFirstExistingField(
                visualSetSo,
                baseVisualSo,
                "baseVisualSo",
                "baseVisualSO",
                "baseVisual",
                "mainVisual",
                "projectileVisual");

            EditorFieldSetter.SetFirstExistingField(
                visualSetSo,
                impactVisualSo,
                "impactVisualSo",
                "impactVisualSO",
                "impactVisual",
                "hitVisual");

            EditorFieldSetter.SetFirstExistingField(
                visualSetSo,
                trailVisualSo,
                "trailVisualSo",
                "trailVisualSO",
                "trailVisual");
        }


        private static string ResolveAssetName(VisualSetJson json)
        {
            if (!string.IsNullOrWhiteSpace(json.visualSetId))
            {
                return SanitizeFileName(json.visualSetId);
            }

            if (json.baseVisual != null && !string.IsNullOrWhiteSpace(json.baseVisual.visualId))
            {
                return SanitizeFileName(json.baseVisual.visualId + ".visual_set");
            }

            return "skill.visual.set";
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
                Debug.LogError($"[SkillVisualSetAssetBuilder] Folder path must start with Assets: {folderPath}");
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
                return "skill.visual.set";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}