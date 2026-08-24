using System;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Helper
{
    /// <summary>
    /// Editor-only helper for finding Sprite assets by naming convention.
    /// </summary>
    public static class SpriteHelper
    {
        private const string GeneratedSkillIconRoot =
            "Assets/ImagesGenerated/Skill/icon";

        private const string GeneratedItemIconRoot =
            "Assets/ImagesGenerated/Item/icon";

        public static Sprite FindSprite(
            string mainId,
            string subString)
        {
            string spriteName = BuildSpriteName(
                mainId,
                subString);

            return FindSpriteByName(spriteName);
        }

        public static Sprite FindSpriteByName(
            string spriteName)
        {
            if (string.IsNullOrWhiteSpace(spriteName))
            {
                return null;
            }

            Sprite generatedSprite = FindGeneratedSprite(spriteName.Trim());
            if (generatedSprite != null)
            {
                return generatedSprite;
            }

            // Compatibility fallback for content that has not moved to
            // Assets/ImagesGenerated yet. Generated icons must always win
            // when a legacy sprite with the same name also exists.
            string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (sprite == null)
                {
                    continue;
                }

                if (string.Equals(
                        sprite.name,
                        spriteName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            return null;
        }

        private static Sprite FindGeneratedSprite(string spriteName)
        {
            string root = null;

            if (spriteName.StartsWith("skill.", StringComparison.OrdinalIgnoreCase))
            {
                root = GeneratedSkillIconRoot;
            }
            else if (spriteName.StartsWith("item.", StringComparison.OrdinalIgnoreCase))
            {
                root = GeneratedItemIconRoot;
            }

            if (string.IsNullOrEmpty(root))
            {
                return null;
            }

            string assetPath = $"{root}/{spriteName}.png";
            Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (directSprite != null)
            {
                return directSprite;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite &&
                    string.Equals(
                        sprite.name,
                        spriteName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }

            return null;
        }

        public static string BuildSpriteName(
            string mainId,
            string subString)
        {
            if (string.IsNullOrWhiteSpace(mainId))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(subString))
            {
                return mainId.Trim();
            }

            return $"{mainId.Trim()}.{subString.Trim()}";
        }
    }
}
