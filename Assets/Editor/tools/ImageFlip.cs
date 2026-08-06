

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ResourceTools
{
    public static class ImageFlip
    {
        private const string OutputFolderName = "_Flipped";
        private const string FlippedSuffix = "_flip";

        [MenuItem("Assets/Resource Tools/Flip Images In Selected Folder", true)]
        private static bool ValidateFlipImagesInSelectedFolder()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                return false;
            }

            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);
            return AssetDatabase.IsValidFolder(selectedPath);
        }

        [MenuItem("Assets/Resource Tools/Flip Images In Selected Folder", false, 2100)]
        private static void FlipImagesInSelectedFolder()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject == null)
            {
                Debug.LogWarning("[ImageFlip] Please select a folder.");
                return;
            }

            string selectedPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(selectedPath) || !AssetDatabase.IsValidFolder(selectedPath))
            {
                Debug.LogWarning("[ImageFlip] Selected asset is not a folder.");
                return;
            }

            string outputFolderPath = EnsureOutputFolder(selectedPath);
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { selectedPath });

            int createdCount = 0;
            int skippedCount = 0;

            foreach (string guid in textureGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrEmpty(assetPath))
                {
                    skippedCount++;
                    continue;
                }

                if (assetPath.StartsWith(outputFolderPath + "/"))
                {
                    skippedCount++;
                    continue;
                }

                if (!IsSupportedImagePath(assetPath))
                {
                    skippedCount++;
                    continue;
                }

                Texture2D sourceTexture = LoadReadableTexture(assetPath);

                if (sourceTexture == null)
                {
                    skippedCount++;
                    continue;
                }

                Texture2D flippedTexture = FlipTextureHorizontal(sourceTexture);
                string outputPath = CreateOutputPath(selectedPath, outputFolderPath, assetPath);

                byte[] bytes = EncodeByExtension(flippedTexture, outputPath);

                if (bytes == null || bytes.Length == 0)
                {
                    Object.DestroyImmediate(flippedTexture);
                    skippedCount++;
                    continue;
                }

                string fullOutputPath = ToFullPath(outputPath);
                string fullOutputDirectory = Path.GetDirectoryName(fullOutputPath);

                if (!Directory.Exists(fullOutputDirectory))
                {
                    Directory.CreateDirectory(fullOutputDirectory);
                }

                File.WriteAllBytes(fullOutputPath, bytes);
                Object.DestroyImmediate(flippedTexture);

                CopyTextureImporterSettings(assetPath, outputPath);
                createdCount++;
            }

            AssetDatabase.Refresh();

            Debug.Log($"[ImageFlip] Complete. Created {createdCount} flipped images. Skipped {skippedCount} files.");
        }

        private static Texture2D LoadReadableTexture(string assetPath)
        {
            string fullPath = ToFullPath(assetPath);

            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[ImageFlip] File not found: {assetPath}");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            if (!texture.LoadImage(bytes))
            {
                Object.DestroyImmediate(texture);
                Debug.LogWarning($"[ImageFlip] Failed to load image: {assetPath}");
                return null;
            }

            texture.name = Path.GetFileNameWithoutExtension(assetPath);
            return texture;
        }

        private static Texture2D FlipTextureHorizontal(Texture2D source)
        {
            int width = source.width;
            int height = source.height;
            Texture2D flipped = new Texture2D(width, height, TextureFormat.RGBA32, false);

            Color32[] sourcePixels = source.GetPixels32();
            Color32[] flippedPixels = new Color32[sourcePixels.Length];

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;

                for (int x = 0; x < width; x++)
                {
                    int sourceIndex = rowStart + x;
                    int targetIndex = rowStart + (width - 1 - x);
                    flippedPixels[targetIndex] = sourcePixels[sourceIndex];
                }
            }

            flipped.SetPixels32(flippedPixels);
            flipped.Apply();
            return flipped;
        }

        private static string CreateOutputPath(string selectedPath, string outputFolderPath, string sourceAssetPath)
        {
            string relativePath = sourceAssetPath.Substring(selectedPath.Length).TrimStart('/');
            string relativeDirectory = Path.GetDirectoryName(relativePath)?.Replace("\\", "/");
            string fileName = Path.GetFileNameWithoutExtension(sourceAssetPath);
            string extension = Path.GetExtension(sourceAssetPath).ToLowerInvariant();

            string outputFileName = $"{fileName}{FlippedSuffix}{extension}";

            if (string.IsNullOrEmpty(relativeDirectory))
            {
                return $"{outputFolderPath}/{outputFileName}";
            }

            return $"{outputFolderPath}/{relativeDirectory}/{outputFileName}";
        }

        private static byte[] EncodeByExtension(Texture2D texture, string outputPath)
        {
            string extension = Path.GetExtension(outputPath).ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return texture.EncodeToJPG(95);

                case ".png":
                    return texture.EncodeToPNG();

                default:
                    return texture.EncodeToPNG();
            }
        }

        private static void CopyTextureImporterSettings(string sourcePath, string targetPath)
        {
            AssetDatabase.ImportAsset(targetPath);

            TextureImporter sourceImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            TextureImporter targetImporter = AssetImporter.GetAtPath(targetPath) as TextureImporter;

            if (sourceImporter == null || targetImporter == null)
            {
                return;
            }

            EditorUtility.CopySerialized(sourceImporter, targetImporter);
            targetImporter.SaveAndReimport();
        }

        private static bool IsSupportedImagePath(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLowerInvariant();
            return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
        }

        private static string EnsureOutputFolder(string selectedPath)
        {
            string outputFolderPath = $"{selectedPath}/{OutputFolderName}";

            if (!AssetDatabase.IsValidFolder(outputFolderPath))
            {
                AssetDatabase.CreateFolder(selectedPath, OutputFolderName);
            }

            return outputFolderPath;
        }

        private static string ToFullPath(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return Path.Combine(projectRoot, assetPath);
        }
    }
}
#endif