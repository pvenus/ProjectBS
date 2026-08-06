#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class SetResourcesSpritesToSingle
    {
        private const float PixelsPerUnit = 100f;

        [MenuItem("Assets/Set Sprites To Single", true)]
        private static bool ValidateExecute()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(selectedObject);
            return AssetDatabase.IsValidFolder(path);
        }

        [MenuItem("Assets/Set Sprites To Single", false, 2000)]
        public static void Execute()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogWarning("[SetResourcesSpritesToSingle] Please select a folder.");
                return;
            }

            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogWarning("[SetResourcesSpritesToSingle] Selected asset is not a folder.");
                return;
            }
            string[] searchFolders = GetFolderAndChildren(selectedPath);
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", searchFolders);

            if (textureGuids == null || textureGuids.Length == 0)
            {
                Debug.LogWarning($"[SetResourcesSpritesToSingle] No Texture2D assets found under {selectedPath}.");
                return;
            }

            int updatedCount = 0;

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (string guid in textureGuids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                    if (importer == null)
                    {
                        continue;
                    }

                    bool changed = false;

                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }

                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }

                    if (!Mathf.Approximately(importer.spritePixelsPerUnit, PixelsPerUnit))
                    {
                        importer.spritePixelsPerUnit = PixelsPerUnit;
                        changed = true;
                    }

                    if (importer.filterMode != FilterMode.Point)
                    {
                        importer.filterMode = FilterMode.Point;
                        changed = true;
                    }

                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        changed = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        changed = true;
                    }

                    if (!changed)
                    {
                        continue;
                    }

                    importer.SaveAndReimport();
                    updatedCount++;

                    Debug.Log($"[SetResourcesSpritesToSingle] Updated: {assetPath}");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            Debug.Log($"[SetResourcesSpritesToSingle] Complete. Updated {updatedCount}/{textureGuids.Length} textures under {selectedPath} including child folders.");
        }

        private static string[] GetFolderAndChildren(string rootPath)
        {
            List<string> folders = new List<string> { rootPath };
            CollectChildFolders(rootPath, folders);
            return folders.ToArray();
        }

        private static void CollectChildFolders(string parentPath, List<string> folders)
        {
            string[] childGuids = AssetDatabase.FindAssets("t:Folder", new[] { parentPath });

            foreach (string childGuid in childGuids)
            {
                string childPath = AssetDatabase.GUIDToAssetPath(childGuid);

                if (string.IsNullOrEmpty(childPath))
                {
                    continue;
                }

                if (childPath == parentPath)
                {
                    continue;
                }

                if (!AssetDatabase.IsValidFolder(childPath))
                {
                    continue;
                }

                if (folders.Contains(childPath))
                {
                    continue;
                }

                folders.Add(childPath);
            }
        }

    }
}
#endif