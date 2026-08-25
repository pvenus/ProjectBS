#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UIFramework.Editor
{
    public class UIImageAutoSpriteWindow : EditorWindow
    {
        public enum TargetMode
        {
            SelectedHierarchyObject,
            SelectedPrefabAssets,
            SelectedFoldersRecursive
        }

        private TargetMode targetMode = TargetMode.SelectedHierarchyObject;
        private UISpriteMappingProfileSO mappingProfile;
        
        // Generation Options
        private bool includeUnmatchedObjects = false;
        private bool preserveExistingEntries = true;

        private Vector2 scrollPosition;

        [MenuItem("Tools/UI/Auto Sprite Applier (SO)")]
        public static void ShowWindow()
        {
            GetWindow<UIImageAutoSpriteWindow>("Auto Sprite Applier");
        }

        [MenuItem("GameObject/UI/Auto Sprite Applier (SO)", false, 0)]
        public static void ShowWindowFromHierarchy(MenuCommand menuCommand)
        {
            var window = GetWindow<UIImageAutoSpriteWindow>("Auto Sprite Applier");
            window.targetMode = TargetMode.SelectedHierarchyObject;
            window.Show();
        }

        [MenuItem("Assets/UI/Auto Sprite Applier (SO)", false, 0)]
        public static void ShowWindowFromProject()
        {
            var window = GetWindow<UIImageAutoSpriteWindow>("Auto Sprite Applier");
            
            var prefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
            var folders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
            
            if (prefabs.Length > 0)
            {
                window.targetMode = TargetMode.SelectedPrefabAssets;
            }
            else if (folders.Length > 0)
            {
                window.targetMode = TargetMode.SelectedFoldersRecursive;
            }
            
            window.Show();
        }

        [MenuItem("Assets/Create/UI/Create Sprite Mapping SO from Folder", false, 0)]
        public static void CreateMappingSOFromFolder()
        {
            var folders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
            if (folders.Length == 0)
            {
                Debug.LogWarning("Please select a folder first.");
                return;
            }

            foreach (var folder in folders)
            {
                string folderPath = AssetDatabase.GetAssetPath(folder);
                if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                UISpriteMappingProfileSO newProfile = ScriptableObject.CreateInstance<UISpriteMappingProfileSO>();
                newProfile.baseImageFolder = folder;
                newProfile.profileId = folder.name + "_Mapping";

                string savePath = $"{folderPath}/{folder.name}_SpriteMapping.asset";
                savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);

                AssetDatabase.CreateAsset(newProfile, savePath);
                
                // Automatically generate entries from the folder
                UIImageSpriteApplier.GenerateProfileFromFolder(newProfile, false);

                EditorGUIUtility.PingObject(newProfile);
                Debug.Log($"Created new Sprite Mapping SO at: {savePath}");
            }
        }

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Target Settings", EditorStyles.boldLabel);
            targetMode = (TargetMode)EditorGUILayout.EnumPopup("Target Mode", targetMode);

            GUILayout.Space(10);
            GUILayout.Label("Sprite Settings", EditorStyles.boldLabel);
            mappingProfile = (UISpriteMappingProfileSO)EditorGUILayout.ObjectField("Mapping Profile", mappingProfile, typeof(UISpriteMappingProfileSO), false);

            if (mappingProfile == null)
            {
                EditorGUILayout.HelpBox("Mapping Profile SO is required.", MessageType.Warning);
            }

            GUILayout.Space(10);
            string targetInfo = GetTargetInfo();
            EditorGUILayout.HelpBox(targetInfo, MessageType.Info);

            GUILayout.Space(10);

            // ----------------------------------------------------
            // Apply Section
            // ----------------------------------------------------
            GUILayout.BeginHorizontal();
            GUI.enabled = mappingProfile != null;
            if (GUILayout.Button("Dry Run", GUILayout.Height(30)))
            {
                ExecuteApply(true);
            }
            
            if (GUILayout.Button("Apply", GUILayout.Height(30)))
            {
                ExecuteApply(false);
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            // ----------------------------------------------------
            // Generator Section
            // ----------------------------------------------------
            GUILayout.Label("Draft Generator / Refresher", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Scans the target object and generates/updates the Mapping Profile SO based on Image names.", MessageType.Info);
            
            includeUnmatchedObjects = EditorGUILayout.Toggle("Include Unmatched Objects", includeUnmatchedObjects);
            preserveExistingEntries = EditorGUILayout.Toggle("Preserve Existing Entries", preserveExistingEntries);

            GUI.enabled = mappingProfile != null && mappingProfile.baseImageFolder != null;
            if (GUILayout.Button("Generate / Refresh from Prefab/Scene", GUILayout.Height(30)))
            {
                ExecuteGenerateDraft();
            }

            if (GUILayout.Button("Generate Directly from Image Folder", GUILayout.Height(30)))
            {
                if (mappingProfile != null)
                {
                    UIImageSpriteApplier.GenerateProfileFromFolder(mappingProfile, preserveExistingEntries);
                }
            }
            GUI.enabled = true;

            if (mappingProfile != null && mappingProfile.baseImageFolder == null)
            {
                EditorGUILayout.HelpBox("Assign 'Base Image Folder' inside the Mapping Profile to enable Generator.", MessageType.Warning);
            }

            GUILayout.Space(20);

            // ----------------------------------------------------
            // Validation Section
            // ----------------------------------------------------
            GUILayout.Label("Validation", EditorStyles.boldLabel);
            GUI.enabled = mappingProfile != null;
            if (GUILayout.Button("Validate Profile", GUILayout.Height(30)))
            {
                UIImageSpriteApplier.ValidateProfile(mappingProfile);
            }
            GUI.enabled = true;

            GUILayout.EndScrollView();
        }

        private string GetTargetInfo()
        {
            switch (targetMode)
            {
                case TargetMode.SelectedHierarchyObject:
                    GameObject go = Selection.activeGameObject;
                    if (go != null && !PrefabUtility.IsPartOfPrefabAsset(go))
                        return $"Target: {go.name} (Scene Object)";
                    return "Target: None or Invalid. Please select a Scene Object in Hierarchy.";
                
                case TargetMode.SelectedPrefabAssets:
                    var prefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
                    return $"Target: {prefabs.Length} Prefab(s) selected.";
                    
                case TargetMode.SelectedFoldersRecursive:
                    var folders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
                    int count = 0;
                    foreach(var f in folders)
                    {
                        string path = AssetDatabase.GetAssetPath(f);
                        if (AssetDatabase.IsValidFolder(path))
                            count++;
                    }
                    return $"Target: {count} Folder(s) selected.";
            }
            return "";
        }

        private void ExecuteApply(bool dryRun)
        {
            if (mappingProfile == null)
            {
                Debug.LogError("Mapping Profile SO is required.");
                return;
            }

            UIImageSpriteApplyReport totalReport = new UIImageSpriteApplyReport();

            System.Action<GameObject, string, string> processRoot = (root, assetPath, displayName) =>
            {
                var report = UIImageSpriteApplier.Apply(root, mappingProfile, dryRun);
                totalReport.Merge(report);
                Debug.Log($"--- Report for {displayName} ---\n" + string.Join("\n", report.logs));
            };

            ProcessTargets(processRoot, dryRun, out List<string> failedPrefabs);
            totalReport.failedPrefabs.AddRange(failedPrefabs);

            Debug.Log($"=== Final Summary (DryRun: {dryRun}) ===\n" +
                      $"Target Roots: {totalReport.targetRootCount}\n" +
                      $"Prefabs Changed: {totalReport.changedPrefabCount}\n" +
                      $"Total Entries: {totalReport.entryCount}\n" +
                      $"Applied: {totalReport.appliedCount}\n" +
                      $"Skipped: {totalReport.skippedCount}\n" +
                      $"Unchanged: {totalReport.unchangedCount}\n" +
                      $"Missing Objects: {totalReport.missingObjectCount}\n" +
                      $"Duplicate Objects: {totalReport.duplicateObjectCount}\n" +
                      $"Missing Sprites: {totalReport.missingSpriteCount}\n" +
                      $"Failed Prefabs: {totalReport.failedPrefabs.Count}");
                      
            if (totalReport.failedPrefabs.Count > 0)
            {
                Debug.LogError("Failed Prefabs:\n" + string.Join("\n", totalReport.failedPrefabs));
            }

            if (!dryRun)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void ExecuteGenerateDraft()
        {
            GameObject root = null;
            if (targetMode == TargetMode.SelectedHierarchyObject)
            {
                root = Selection.activeGameObject;
                if (root == null || PrefabUtility.IsPartOfPrefabAsset(root))
                {
                    Debug.LogError("Select a Scene Object or Prefab Instance in Hierarchy.");
                    return;
                }
            }
            else if (targetMode == TargetMode.SelectedPrefabAssets)
            {
                var prefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
                if (prefabs.Length > 0)
                {
                    root = prefabs[0];
                    if (prefabs.Length > 1)
                        Debug.LogWarning("Generator only uses the first selected prefab.");
                }
            }
            
            if (root == null)
            {
                Debug.LogError("Valid root object is required for generating a draft.");
                return;
            }

            bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(root);
            GameObject processTarget = root;

            if (isPrefab)
            {
                string path = AssetDatabase.GetAssetPath(root);
                processTarget = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    UIImageSpriteApplier.GenerateOrRefreshProfile(processTarget, mappingProfile, includeUnmatchedObjects, preserveExistingEntries);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(processTarget);
                }
            }
            else
            {
                UIImageSpriteApplier.GenerateOrRefreshProfile(processTarget, mappingProfile, includeUnmatchedObjects, preserveExistingEntries);
            }
        }

        private void ProcessTargets(System.Action<GameObject, string, string> action, bool dryRun, out List<string> failedPrefabs)
        {
            failedPrefabs = new List<string>();

            if (targetMode == TargetMode.SelectedHierarchyObject)
            {
                GameObject root = Selection.activeGameObject;
                if (root == null || PrefabUtility.IsPartOfPrefabAsset(root))
                {
                    Debug.LogWarning("Selected object is not a valid hierarchy object.");
                    return;
                }
                action(root, "", root.name);
            }
            else if (targetMode == TargetMode.SelectedPrefabAssets)
            {
                var prefabs = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
                foreach (var prefab in prefabs)
                {
                    string path = AssetDatabase.GetAssetPath(prefab);
                    ProcessSinglePrefab(path, action, dryRun, failedPrefabs);
                }
            }
            else if (targetMode == TargetMode.SelectedFoldersRecursive)
            {
                var folders = Selection.GetFiltered<DefaultAsset>(SelectionMode.Assets);
                var prefabPaths = new List<string>();

                foreach (var folder in folders)
                {
                    string folderPath = AssetDatabase.GetAssetPath(folder);
                    if (!AssetDatabase.IsValidFolder(folderPath)) continue;

                    string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (!prefabPaths.Contains(path))
                        {
                            prefabPaths.Add(path);
                        }
                    }
                }

                foreach (string path in prefabPaths)
                {
                    ProcessSinglePrefab(path, action, dryRun, failedPrefabs);
                }
            }
        }

        private void ProcessSinglePrefab(string assetPath, System.Action<GameObject, string, string> action, bool dryRun, List<string> failedPrefabs)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(assetPath);
                if (root != null)
                {
                    action(root, assetPath, root.name);

                    if (!dryRun)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, assetPath);
                    }
                }
                else
                {
                    failedPrefabs.Add(assetPath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error processing prefab {assetPath}: {e.Message}");
                failedPrefabs.Add(assetPath);
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
    }
}
#endif
