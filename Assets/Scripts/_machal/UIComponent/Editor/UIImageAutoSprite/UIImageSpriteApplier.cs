#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;

namespace UIFramework.Editor
{
    public class UIImageSpriteApplyReport
    {
        public int targetRootCount = 0;
        public int entryCount = 0;
        public int appliedCount = 0;
        public int skippedCount = 0;
        public int missingObjectCount = 0;
        public int duplicateObjectCount = 0;
        public int missingImageCount = 0;
        public int missingSpriteCount = 0;
        public int unchangedCount = 0;
        public int changedPrefabCount = 0;
        public List<string> failedPrefabs = new List<string>();
        public List<string> logs = new List<string>();

        public void AddLog(string log) => logs.Add(log);

        public void Merge(UIImageSpriteApplyReport other)
        {
            targetRootCount += other.targetRootCount;
            entryCount += other.entryCount;
            appliedCount += other.appliedCount;
            skippedCount += other.skippedCount;
            missingObjectCount += other.missingObjectCount;
            duplicateObjectCount += other.duplicateObjectCount;
            missingImageCount += other.missingImageCount;
            missingSpriteCount += other.missingSpriteCount;
            unchangedCount += other.unchangedCount;
            changedPrefabCount += other.changedPrefabCount;
            failedPrefabs.AddRange(other.failedPrefabs);
            logs.AddRange(other.logs);
        }
    }

    public static class UIImageSpriteApplier
    {
        public static UIImageSpriteApplyReport Apply(GameObject root, UISpriteMappingProfileSO profile, bool dryRun)
        {
            var report = new UIImageSpriteApplyReport();
            report.targetRootCount = 1;

            if (profile == null || profile.entries == null || profile.entries.Count == 0)
            {
                report.AddLog($"[Info] No entries found in SO profile.");
                return report;
            }

            report.entryCount = profile.entries.Count;

            UIAutoImage[] allAutoImages = root.GetComponentsInChildren<UIAutoImage>(true);
            if (allAutoImages.Length == 0)
            {
                report.AddLog($"[Warning] No UIAutoImage components found in root: {root.name}");
                return report;
            }

            var imagesByKey = new Dictionary<string, List<UIAutoImage>>();
            var imagePathDict = new Dictionary<UIAutoImage, string>();

            foreach (var autoImg in allAutoImages)
            {
                string key = autoImg.GetEffectiveKey();
                if (!imagesByKey.ContainsKey(key)) imagesByKey[key] = new List<UIAutoImage>();
                imagesByKey[key].Add(autoImg);
                imagePathDict[autoImg] = AnimationUtility.CalculateTransformPath(autoImg.transform, root.transform);
            }

            if (!dryRun)
            {
                Undo.RegisterFullObjectHierarchyUndo(root, "Apply Auto Sprite (SO)");
            }

            bool anyChanged = false;

            foreach (var entry in profile.entries)
            {
                if (!entry.enabled)
                {
                    report.skippedCount++;
                    report.AddLog($"[Skip] Disabled entry: {entry.objectName ?? entry.path}");
                    continue;
                }

                List<UIAutoImage> targetsToApply = new List<UIAutoImage>();
                
                // 1. Try finding by Path
                if (profile.usePathFirst && !string.IsNullOrEmpty(entry.path))
                {
                    var exactTarget = allAutoImages.FirstOrDefault(img => imagePathDict[img] == entry.path);
                    
                    if (exactTarget == null && !profile.allowObjectNameFallback)
                    {
                        report.missingObjectCount++;
                        report.AddLog($"[Error] UIAutoImage object not found at path: {entry.path}");
                        continue;
                    }
                    if (exactTarget != null)
                    {
                        targetsToApply.Add(exactTarget);
                    }
                }

                // 2. Fallback or find by Key
                if (targetsToApply.Count == 0 && !string.IsNullOrEmpty(entry.objectName))
                {
                    if (imagesByKey.TryGetValue(entry.objectName, out var list))
                    {
                        targetsToApply.AddRange(list);
                    }
                    else
                    {
                        report.missingObjectCount++;
                        report.AddLog($"[Error] UIAutoImage object not found with key/name: {entry.objectName}");
                        continue;
                    }
                }

                if (targetsToApply.Count == 0)
                {
                    report.missingObjectCount++;
                    report.AddLog($"[Error] Entry missing valid path and key, or target not found.");
                    continue;
                }

                if (entry.sprite == null)
                {
                    report.missingSpriteCount++;
                    report.AddLog($"[Error] Sprite reference is null in entry for target: {entry.objectName}");
                    continue;
                }

                foreach (var targetImage in targetsToApply)
                {
                    Image rawImg = targetImage.ImageComponent;
                    if (rawImg == null)
                    {
                        report.missingImageCount++;
                        continue;
                    }

                    if (profile.skipIfTargetSpriteAlreadyAssigned && rawImg.sprite != null && rawImg.sprite != entry.sprite)
                    {
                        report.skippedCount++;
                        report.AddLog($"[Skip] Target {targetImage.name} already has a different sprite assigned ({rawImg.sprite.name}).");
                        continue;
                    }

                    if (rawImg.sprite == entry.sprite)
                    {
                        report.unchangedCount++;
                        report.AddLog($"[Unchanged] {targetImage.name} already has sprite: {entry.sprite.name}");
                    }
                    else
                    {
                        report.AddLog($"[Apply] {targetImage.name}: {(rawImg.sprite ? rawImg.sprite.name : "None")} -> {entry.sprite.name}");
                        if (!dryRun)
                        {
                            rawImg.sprite = entry.sprite;
                            EditorUtility.SetDirty(rawImg);
                        }
                        report.appliedCount++;
                        anyChanged = true;
                    }
                }
            }

            if (anyChanged)
            {
                report.changedPrefabCount = 1;
            }

            return report;
        }

        public static void GenerateOrRefreshProfile(GameObject root, UISpriteMappingProfileSO profile, bool includeUnmatched, bool preserveExisting)
        {
            if (profile == null) return;
            if (profile.baseImageFolder == null)
            {
                Debug.LogWarning("Base Image Folder is not set in Profile!");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(profile.baseImageFolder);
            var spritesDict = BuildSpriteDict(folderPath);

            UIAutoImage[] allAutoImages = root.GetComponentsInChildren<UIAutoImage>(true);

            // Rebuild or merge
            var existingEntries = preserveExisting ? profile.entries.ToList() : new List<UISpriteMappingEntry>();
            if (!preserveExisting)
            {
                profile.entries.Clear();
            }

            foreach (var img in allAutoImages)
            {
                string path = AnimationUtility.CalculateTransformPath(img.transform, root.transform);
                string key = img.GetEffectiveKey();

                var existing = existingEntries.FirstOrDefault(e => e.path == path || (string.IsNullOrEmpty(e.path) && e.objectName == key));
                
                Sprite matchedSprite = null;
                if (spritesDict.TryGetValue(key, out var spriteList))
                {
                    matchedSprite = spriteList.FirstOrDefault();
                }

                if (existing != null)
                {
                    if (preserveExisting && existing.sprite != null && matchedSprite != null && existing.sprite != matchedSprite)
                    {
                        // Preserve existing if it has a valid sprite
                    }
                    else if (matchedSprite != null)
                    {
                        existing.sprite = matchedSprite;
                    }
                }
                else
                {
                    if (matchedSprite != null)
                    {
                        profile.entries.Add(new UISpriteMappingEntry
                        {
                            path = path,
                            objectName = key,
                            sprite = matchedSprite,
                            enabled = true
                        });
                    }
                    else if (includeUnmatched)
                    {
                        profile.entries.Add(new UISpriteMappingEntry
                        {
                            path = path,
                            objectName = key,
                            sprite = null,
                            enabled = true,
                            memo = "Unmatched"
                        });
                    }
                }
            }

            profile.entries = existingEntries;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Profile Generator] Generated from {root.name}. Total entries: {profile.entries.Count}");
        }

        public static void GenerateProfileFromFolder(UISpriteMappingProfileSO profile, bool preserveExisting)
        {
            if (profile == null) return;
            if (profile.baseImageFolder == null)
            {
                Debug.LogError("Base Image Folder is not set in Profile!");
                return;
            }

            string folderPath = AssetDatabase.GetAssetPath(profile.baseImageFolder);
            var spritesDict = BuildSpriteDict(folderPath);

            var existingEntries = preserveExisting ? profile.entries.ToList() : new List<UISpriteMappingEntry>();
            if (!preserveExisting)
            {
                profile.entries.Clear();
            }

            int addedCount = 0;

            foreach (var kvp in spritesDict)
            {
                string spriteName = kvp.Key;
                Sprite sprite = kvp.Value.FirstOrDefault();

                var existing = existingEntries.Find(e => e.sprite == sprite || e.objectName == spriteName);
                if (existing != null)
                {
                    if (existing.sprite == null)
                    {
                        existing.sprite = sprite;
                    }
                    continue;
                }

                existingEntries.Add(new UISpriteMappingEntry
                {
                    objectName = spriteName,
                    sprite = sprite,
                    enabled = true
                });
                addedCount++;
            }

            profile.entries = existingEntries;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Profile Generator] Generated from folder '{folderPath}'. Added {addedCount} new entries. Total: {profile.entries.Count}");
        }

        public static void ValidateProfile(UISpriteMappingProfileSO profile)
        {
            if (profile == null)
            {
                Debug.LogError("Profile is null.");
                return;
            }

            if (profile.entries == null || profile.entries.Count == 0)
            {
                Debug.LogWarning("Profile has no entries.");
                return;
            }

            string basePath = profile.baseImageFolder != null ? AssetDatabase.GetAssetPath(profile.baseImageFolder) : "";
            
            int missingSprite = 0;
            int outOfFolder = 0;
            int duplicatePaths = 0;
            int duplicateNames = 0;

            var paths = new HashSet<string>();
            var names = new HashSet<string>();

            foreach (var entry in profile.entries)
            {
                if (!entry.enabled) continue;

                if (entry.sprite == null)
                {
                    missingSprite++;
                    Debug.LogWarning($"[Validate] Missing Sprite reference in entry: {entry.objectName ?? entry.path}");
                }
                else if (!string.IsNullOrEmpty(basePath))
                {
                    string assetPath = AssetDatabase.GetAssetPath(entry.sprite);
                    if (!assetPath.StartsWith(basePath))
                    {
                        outOfFolder++;
                        Debug.LogWarning($"[Validate] Sprite '{entry.sprite.name}' is outside of Base Folder. Path: {assetPath}");
                    }
                }

                if (string.IsNullOrEmpty(entry.path) && string.IsNullOrEmpty(entry.objectName))
                {
                    Debug.LogWarning($"[Validate] Entry has both empty path and objectName. Index: {profile.entries.IndexOf(entry)}");
                }

                if (!string.IsNullOrEmpty(entry.path))
                {
                    if (!paths.Add(entry.path)) duplicatePaths++;
                }

                if (!string.IsNullOrEmpty(entry.objectName))
                {
                    if (!names.Add(entry.objectName)) duplicateNames++;
                }
            }

            Debug.Log($"=== Validation Complete ===\n" +
                      $"Missing Sprites: {missingSprite}\n" +
                      $"Out of Folder Sprites: {outOfFolder}\n" +
                      $"Duplicate Paths: {duplicatePaths}\n" +
                      $"Duplicate Names: {duplicateNames}");
        }

        private static Dictionary<string, List<Sprite>> BuildSpriteDict(string folderPath)
        {
            var dict = new Dictionary<string, List<Sprite>>();
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
                return dict;

            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            
            var allGuids = new HashSet<string>(textureGuids);
            allGuids.UnionWith(spriteGuids);

            foreach (string guid in allGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetExtension(assetPath).ToLower() == ".spriteatlas") continue;

                var allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (var asset in allAssets)
                {
                    if (asset is Sprite sprite)
                    {
                        if (!dict.ContainsKey(sprite.name)) dict[sprite.name] = new List<Sprite>();
                        if (!dict[sprite.name].Contains(sprite)) dict[sprite.name].Add(sprite);
                    }
                }
            }
            return dict;
        }
    }
}
#endif
