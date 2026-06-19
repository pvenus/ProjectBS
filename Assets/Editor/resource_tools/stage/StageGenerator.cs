using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    /// <summary>
    /// Main editor entry point for stage/story json generation.
    ///
    /// Responsibilities:
    /// - Select a single json file or a folder containing json files.
    /// - Stage definition json(requiredSubEvents) creates StageDefinitionSO and calls act node builders.
    /// - Act json(nodes/startNodeId) creates RoundNodeSO/PopupEventSO assets.
    /// - Stage string CSV can be generated from act json files.
    /// </summary>
    public sealed class StageGenerator : EditorWindow
    {
        private const string WindowTitle = "Stage JSON Generator";
        private const string DefaultJsonFolder = "Assets/Resources/stage_new";
        private const string DefaultStageDefinitionOutputFolder = "Assets/Resources/stage_new/definitions";
        private const string DefaultStageNodeOutputFolder = "Assets/Resources/stage_new/nodes";
        private const string DefaultPopupEventOutputFolder = "Assets/Resources/stage_new/popup_events";
        private const string DefaultStageStringCsvPath = "Assets/Resources/string/stage_string.csv";

        [SerializeField] private string jsonPath = DefaultJsonFolder;
        [SerializeField] private string stageDefinitionOutputFolder = DefaultStageDefinitionOutputFolder;
        [SerializeField] private string stageNodeOutputFolder = DefaultStageNodeOutputFolder;
        [SerializeField] private string popupEventOutputFolder = DefaultPopupEventOutputFolder;
        [SerializeField] private string stageStringCsvPath = DefaultStageStringCsvPath;
        [SerializeField] private bool includeSubFolders = true;
        [SerializeField] private bool generateStrings = true;

        private Vector2 scroll;
        private readonly List<string> logs = new();

        [MenuItem("Assets/Stage/Stage JSON Generator", false, 2000)]
        public static void OpenWindow()
        {
            var window = GetWindow<StageGenerator>(WindowTitle);
            window.minSize = new Vector2(620f, 420f);
            window.Show();
        }

        [MenuItem("Tools/Resource Tools/Stage/Generate Selected Stage JSON")]
        public static void GenerateSelectedJsonMenu()
        {
            var selectedPath = GetSelectedAssetPath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                Debug.LogWarning("[StageGenerator] Select a stage json file or folder first.");
                return;
            }

            GenerateStageDefinitionsFromPath(
                selectedPath,
                DefaultStageDefinitionOutputFolder,
                DefaultStageNodeOutputFolder,
                DefaultPopupEventOutputFolder,
                true);

            GenerateFromPath(
                selectedPath,
                DefaultStageNodeOutputFolder,
                DefaultPopupEventOutputFolder,
                DefaultStageStringCsvPath,
                true,
                true);
        }

        public static IReadOnlyList<StageNodeBuilder.BuildResult> GenerateFromPath(
            string inputPath,
            string stageNodeOutputFolder = DefaultStageNodeOutputFolder,
            string popupEventOutputFolder = DefaultPopupEventOutputFolder,
            string stageStringCsvPath = DefaultStageStringCsvPath,
            bool includeSubFolders = true,
            bool generateStrings = true)
        {
            var jsonFiles = CollectJsonFiles(inputPath, includeSubFolders)
                .Where(IsActJson)
                .ToList();
            var results = new List<StageNodeBuilder.BuildResult>();

            if (jsonFiles.Count == 0)
            {
                Debug.LogWarning($"[StageGenerator] No act json files found: {inputPath}");
                return results;
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var file in jsonFiles)
                {
                    try
                    {
                        if (generateStrings)
                        {
                            var stringResult = StageStringBuilder.BuildFromJsonPath(file, stageStringCsvPath);
                            Debug.Log(
                                $"[StageGenerator] Updated strings. " +
                                $"Json={file}, Csv={stringResult.csvPath}, " +
                                $"Added={stringResult.addedCount}, Updated={stringResult.updatedCount}");

                            foreach (var warning in stringResult.warnings)
                            {
                                Debug.LogWarning($"[StageGenerator] {warning}");
                            }
                        }

                        var result = StageNodeBuilder.BuildFromJsonPath(
                            file,
                            stageNodeOutputFolder,
                            popupEventOutputFolder);

                        results.Add(result);

                        Debug.Log(
                            $"[StageGenerator] Generated stage node. " +
                            $"Json={file}, " +
                            $"NodeId={result.stageNodeId}, " +
                            $"Asset={result.stageNodeAssetPath}");

                        if (result.warnings != null)
                        {
                            foreach (var warning in result.warnings)
                            {
                                Debug.LogWarning($"[StageGenerator] {warning}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[StageGenerator] Failed to generate from json: {file}\n{e}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return results;
        }

        public static IReadOnlyList<string> GenerateStageDefinitionsFromPath(
            string inputPath,
            string stageDefinitionOutputFolder = DefaultStageDefinitionOutputFolder,
            string stageNodeOutputFolder = DefaultStageNodeOutputFolder,
            string popupEventOutputFolder = DefaultPopupEventOutputFolder,
            bool includeSubFolders = true)
        {
            var jsonFiles = CollectJsonFiles(inputPath, includeSubFolders)
                .Where(IsStageDefinitionJson)
                .ToList();
            var results = new List<string>();

            foreach (var file in jsonFiles)
            {
                try
                {
                    var definition = StageDefinitionBuilder.BuildFromJsonPath(
                        file,
                        stageDefinitionOutputFolder,
                        stageNodeOutputFolder,
                        popupEventOutputFolder);

                    if (definition != null)
                    {
                        results.Add(file);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[StageGenerator] Failed to generate StageDefinitionSO: {file}\n{e}");
                }
            }

            return results;
        }

        public static IReadOnlyList<string> CollectJsonFiles(string inputPath, bool includeSubFolders)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return result;
            }

            inputPath = NormalizeAssetPath(inputPath);

            if (File.Exists(inputPath))
            {
                if (IsStageStoryJson(inputPath))
                {
                    result.Add(inputPath);
                }

                return result;
            }

            if (!Directory.Exists(inputPath))
            {
                return result;
            }

            var option = includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            result.AddRange(Directory.GetFiles(inputPath, "*.json", option)
                .Select(NormalizeAssetPath)
                .Where(IsStageStoryJson)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase));

            return result;
        }

        private static bool IsStageStoryJson(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (!Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var text = File.ReadAllText(path);
                return text.Contains("\"nodes\"", StringComparison.Ordinal)
                    || text.Contains("\"requiredSubEvents\"", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsActJson(string path)
        {
            if (!IsStageStoryJson(path))
            {
                return false;
            }

            try
            {
                var text = File.ReadAllText(path);
                return text.Contains("\"nodes\"", StringComparison.Ordinal)
                    && text.Contains("\"startNodeId\"", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsStageDefinitionJson(string path)
        {
            if (!IsStageStoryJson(path))
            {
                return false;
            }

            try
            {
                var text = File.ReadAllText(path);
                return text.Contains("\"requiredSubEvents\"", StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Stage JSON Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);

            DrawPathField(
                label: "JSON File / Folder",
                path: ref jsonPath,
                openPanelTitle: "Select Stage JSON File or Folder",
                allowFile: true,
                allowFolder: true);

            DrawPathField(
                label: "Stage Definition Output",
                path: ref stageDefinitionOutputFolder,
                openPanelTitle: "Select Stage Definition Output Folder",
                allowFile: false,
                allowFolder: true);

            DrawPathField(
                label: "Stage Node Output",
                path: ref stageNodeOutputFolder,
                openPanelTitle: "Select Stage Node Output Folder",
                allowFile: false,
                allowFolder: true);

            DrawPathField(
                label: "Popup Event Output",
                path: ref popupEventOutputFolder,
                openPanelTitle: "Select Popup Event Output Folder",
                allowFile: false,
                allowFolder: true);

            DrawPathField(
                label: "Stage String CSV",
                path: ref stageStringCsvPath,
                openPanelTitle: "Select Stage String CSV",
                allowFile: true,
                allowFolder: false);

            includeSubFolders = EditorGUILayout.ToggleLeft("Include subfolders when JSON path is a folder", includeSubFolders);
            generateStrings = EditorGUILayout.ToggleLeft("Generate stage string CSV", generateStrings);

            EditorGUILayout.Space(10f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selection", GUILayout.Height(28f)))
                {
                    var selectedPath = GetSelectedAssetPath();
                    if (!string.IsNullOrWhiteSpace(selectedPath))
                    {
                        jsonPath = selectedPath;
                        AddLog($"Selected: {jsonPath}");
                    }
                    else
                    {
                        AddLog("No asset selected.");
                    }
                }

                if (GUILayout.Button("Generate", GUILayout.Height(28f)))
                {
                    GenerateFromWindow();
                }
            }

            EditorGUILayout.Space(10f);
            DrawPreview();
            DrawLogs();
        }

        private void GenerateFromWindow()
        {
            logs.Clear();

            var files = CollectJsonFiles(jsonPath, includeSubFolders);
            if (files.Count == 0)
            {
                AddLog($"No json files found: {jsonPath}");
                return;
            }

            AddLog($"Found {files.Count} json file(s).");

            var definitionResults = GenerateStageDefinitionsFromPath(
                jsonPath,
                stageDefinitionOutputFolder,
                stageNodeOutputFolder,
                popupEventOutputFolder,
                includeSubFolders);

            AddLog($"Generated {definitionResults.Count} stage definition asset(s).");

            var results = GenerateFromPath(
                jsonPath,
                stageNodeOutputFolder,
                popupEventOutputFolder,
                stageStringCsvPath,
                includeSubFolders,
                generateStrings);

            AddLog($"Generated {results.Count} stage node asset(s).");

            foreach (var result in results)
            {
                AddLog($"- {result.stageNodeId} => {result.stageNodeAssetPath}");

                if (result.warnings == null)
                {
                    continue;
                }

                foreach (var warning in result.warnings)
                {
                    AddLog($"  Warning: {warning}");
                }
            }
        }

        private void DrawPreview()
        {
            var files = CollectJsonFiles(jsonPath, includeSubFolders);
            var definitionCount = files.Count(IsStageDefinitionJson);
            var actCount = files.Count(IsActJson);

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"JSON files to generate: {files.Count}  |  Stage Definitions: {definitionCount}  |  Act Nodes: {actCount}",
                MessageType.Info);

            if (files.Count == 0)
            {
                return;
            }

            var previewCount = Mathf.Min(files.Count, 8);
            for (var i = 0; i < previewCount; i++)
            {
                var type = IsStageDefinitionJson(files[i]) ? "Definition" : "Act";
                EditorGUILayout.LabelField($"[{type}] {files[i]}");
            }

            if (files.Count > previewCount)
            {
                EditorGUILayout.LabelField($"... and {files.Count - previewCount} more");
            }
        }

        private void DrawLogs()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Logs", EditorStyles.boldLabel);

            using (var scrollScope = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.MinHeight(120f)))
            {
                scroll = scrollScope.scrollPosition;

                if (logs.Count == 0)
                {
                    EditorGUILayout.LabelField("No logs yet.");
                    return;
                }

                foreach (var log in logs)
                {
                    EditorGUILayout.LabelField(log, EditorStyles.wordWrappedLabel);
                }
            }
        }

        private static void DrawPathField(
            string label,
            ref string path,
            string openPanelTitle,
            bool allowFile,
            bool allowFolder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                path = EditorGUILayout.TextField(label, path);

                if (GUILayout.Button("Browse", GUILayout.Width(72f)))
                {
                    var absolute = ToAbsolutePath(path);
                    string selected = null;

                    if (allowFile && allowFolder)
                    {
                        selected = EditorUtility.OpenFilePanel(openPanelTitle, Directory.Exists(absolute) ? absolute : Application.dataPath, "json");
                        if (string.IsNullOrWhiteSpace(selected))
                        {
                            selected = EditorUtility.OpenFolderPanel(openPanelTitle, Application.dataPath, string.Empty);
                        }
                    }
                    else if (allowFile)
                    {
                        selected = EditorUtility.OpenFilePanel(openPanelTitle, Directory.Exists(absolute) ? absolute : Application.dataPath, "json");
                    }
                    else if (allowFolder)
                    {
                        selected = EditorUtility.OpenFolderPanel(openPanelTitle, Directory.Exists(absolute) ? absolute : Application.dataPath, string.Empty);
                    }

                    if (!string.IsNullOrWhiteSpace(selected))
                    {
                        path = ToAssetPath(selected);
                    }
                }
            }
        }

        private void AddLog(string message)
        {
            logs.Add(message);
            Repaint();
        }

        private static string GetSelectedAssetPath()
        {
            var selected = Selection.activeObject;
            if (selected == null)
            {
                return null;
            }

            return AssetDatabase.GetAssetPath(selected);
        }

        private static string NormalizeAssetPath(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                ? path
                : path.Replace('\\', '/').Trim();
        }

        private static string ToAbsolutePath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return Application.dataPath;
            }

            assetPath = NormalizeAssetPath(assetPath);
            if (Path.IsPathRooted(assetPath))
            {
                return assetPath;
            }

            if (assetPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            {
                var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                return string.IsNullOrWhiteSpace(projectRoot)
                    ? Application.dataPath
                    : Path.Combine(projectRoot, assetPath);
            }

            return assetPath;
        }

        private static string ToAssetPath(string absolutePath)
        {
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return absolutePath;
            }

            absolutePath = NormalizeAssetPath(absolutePath);
            var projectRoot = NormalizeAssetPath(Directory.GetParent(Application.dataPath)?.FullName);

            if (!string.IsNullOrWhiteSpace(projectRoot) && absolutePath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return absolutePath.Substring(projectRoot.Length).TrimStart('/');
            }

            return absolutePath;
        }
    }
}