

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Skill
{
    /// <summary>
    /// Trail visual 에셋 빌더.
    /// 프로젝트에 Trail 전용 ScriptableObject 타입이 없으면 생성하지 않고 null을 반환한다.
    /// </summary>
    public static class SkillTrailVisualAssetBuilder
    {
        public static ScriptableObject CreateOrUpdate(
            TrailVisualJson json,
            string outputFolder)
        {
            if (json == null)
            {
                Debug.LogWarning("[SkillTrailVisualAssetBuilder] TrailVisual json is null.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(outputFolder))
            {
                Debug.LogError("[SkillTrailVisualAssetBuilder] Output folder is null or empty.");
                return null;
            }

            EnsureFolder(outputFolder);

            Type trailVisualType = FindFirstType(
                "SkillTrailVisualSO",
                "TrailVisualSO",
                "ProjectileTrailVisualSO",
                "SkillTrailVisualProfileSO");

            if (trailVisualType == null)
            {
                Debug.LogWarning("[SkillTrailVisualAssetBuilder] Trail visual SO type not found. Trail visual will be skipped. Tried: SkillTrailVisualSO, TrailVisualSO, ProjectileTrailVisualSO, SkillTrailVisualProfileSO");
                return null;
            }

            string assetName = ResolveAssetName(json);
            string assetPath = Path.Combine(outputFolder, assetName + ".asset")
                .Replace("\\", "/");

            ScriptableObject trailSo =
                AssetDatabase.LoadAssetAtPath(assetPath, trailVisualType) as ScriptableObject;

            if (trailSo == null)
            {
                trailSo = ScriptableObject.CreateInstance(trailVisualType);
                AssetDatabase.CreateAsset(trailSo, assetPath);
            }

            Apply(trailSo, json);

            EditorUtility.SetDirty(trailSo);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SkillTrailVisualAssetBuilder] Updated trail visual asset: {assetPath}");

            return trailSo;
        }

        private static string ResolveAssetName(TrailVisualJson json)
        {
            string visualId = ResolveVisualId(json);
            string prefabName = ResolvePrefabName(json);

            if (!string.IsNullOrWhiteSpace(visualId))
            {
                return SanitizeFileName(visualId);
            }

            if (!string.IsNullOrWhiteSpace(prefabName))
            {
                return SanitizeFileName(prefabName + ".trail_visual");
            }

            return "skill.trail.visual";
        }

        private static void Apply(
            ScriptableObject trailSo,
            TrailVisualJson json)
        {
            string visualId = ResolveVisualId(json);
            string prefabName = ResolvePrefabName(json);

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                visualId,
                "visualId",
                "trailVisualId",
                "trailId",
                "id");

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                FindPrefabByName(prefabName),
                "prefab",
                "trailPrefab",
                "visualPrefab",
                "effectPrefab");

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                FindMaterialByName(json.materialName),
                "material",
                "trailMaterial");

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                FindSpriteByName(json.spriteName),
                "sprite",
                "trailSprite",
                "visualSprite");

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                json.width,
                "width",
                "trailWidth");

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                json.duration,
                "duration",
                "trailDuration",
                "lifeTime",
                "lifetime");

            EditorFieldSetter.SetFirstExistingField(
                trailSo,
                json.followProjectile,
                "followProjectile",
                "attachToProjectile",
                "followOwner");
        }
        private static string ResolveVisualId(TrailVisualJson json)
        {
            if (json == null)
            {
                return null;
            }

            string trailId = GetStringMember(json, "trailId");

            if (!string.IsNullOrWhiteSpace(trailId))
            {
                return trailId;
            }

            return json.visualId;
        }

        private static string ResolvePrefabName(TrailVisualJson json)
        {
            if (json == null)
            {
                return null;
            }

            string effectPrefab = GetStringMember(json, "effectPrefab");

            if (!string.IsNullOrWhiteSpace(effectPrefab))
            {
                return effectPrefab;
            }

            return json.prefabName;
        }

        private static string GetStringMember(object target, string memberName)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            Type type = target.GetType();

            var field = type.GetField(
                memberName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (field != null && field.FieldType == typeof(string))
            {
                return field.GetValue(target) as string;
            }

            var property = type.GetProperty(
                memberName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (property != null && property.PropertyType == typeof(string))
            {
                return property.GetValue(target) as string;
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

            Debug.LogWarning($"[SkillTrailVisualAssetBuilder] Prefab not found: {prefabName}");
            return null;
        }

        private static Material FindMaterialByName(string materialName)
        {
            if (string.IsNullOrWhiteSpace(materialName))
            {
                return null;
            }

            string[] guids = AssetDatabase.FindAssets($"{materialName} t:Material");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

                if (material != null && material.name == materialName)
                {
                    return material;
                }
            }

            Debug.LogWarning($"[SkillTrailVisualAssetBuilder] Material not found: {materialName}");
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

            Debug.LogWarning($"[SkillTrailVisualAssetBuilder] Sprite not found: {spriteName}");
            return null;
        }

        private static Type FindFirstType(params string[] typeNames)
        {
            if (typeNames == null || typeNames.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < typeNames.Length; i++)
            {
                Type type = FindType(typeNames[i]);

                if (type != null)
                {
                    return type;
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

                if (type != null && typeof(ScriptableObject).IsAssignableFrom(type))
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
                    if (types[i].Name == typeName &&
                        typeof(ScriptableObject).IsAssignableFrom(types[i]))
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
                Debug.LogError($"[SkillTrailVisualAssetBuilder] Folder path must start with Assets: {folderPath}");
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
                return "skill.trail.visual";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}