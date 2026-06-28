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
