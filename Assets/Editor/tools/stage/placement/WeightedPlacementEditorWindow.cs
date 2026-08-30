#if UNITY_EDITOR
using System;
using System.IO;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage.Placement
{
    internal static class WeightedPoolPlacementRuleGenerator
    {
        private const string MenuPath =
            "Assets/Stage/Generate WeightedPoolPlacementRule From Json";

        [MenuItem(MenuPath, false, 2002)]
        private static void Generate() => TryGenerateSelected(true);

        internal static bool TryGenerateSelected(bool notify)
        {
            if (!TryGetSelectedDocument(out TextAsset source, out string selectionError))
            {
                if (notify)
                {
                    Debug.LogError(selectionError);
                    EditorUtility.DisplayDialog("Select Weighted Placement JSON",
                        selectionError, "OK");
                }
                return false;
            }
            string sourcePath = AssetDatabase.GetAssetPath(source);
            try
            {
                string json = source.text;
                StagePlacementRuleSO dryRun =
                    WeightedPlacementApplyService.CompileTransient(json);
                UnityEngine.Object.DestroyImmediate(dryRun);

                StagePlacementRuleSO catalog =
                    WeightedPlacementApplyService.Apply(json, true);
                Selection.activeObject = catalog;
                EditorGUIUtility.PingObject(catalog);
                Debug.Log($"[WeightedPlacement] Updated canonical rule from '{sourcePath}' at "
                    + $"'{AssetDatabase.GetAssetPath(catalog)}'.", catalog);
                return true;
            }
            catch (Exception exception)
            {
                string message = $"Weighted placement JSON conversion failed for "
                    + $"'{sourcePath}'. The canonical rule was not changed.\n{exception.Message}";
                Debug.LogError(message);
                if (notify) EditorUtility.DisplayDialog(
                    "Weighted Placement Validation Error", message, "OK");
                return false;
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool CanGenerate() =>
            TryGetSelectedDocument(out _, out _);

        internal static bool TryGetSelectedDocument(
            out TextAsset source, out string error)
        {
            source = null;
            error = "Select one canonical weighted placement document JSON.";
            if (Selection.objects == null || Selection.objects.Length != 1
                || Selection.activeObject is not TextAsset selected) return false;
            source = selected;
            string path = AssetDatabase.GetAssetPath(source);
            if (!string.Equals(Path.GetExtension(path), ".json",
                    StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".schema.json", StringComparison.OrdinalIgnoreCase)
                || path.IndexOf("/schema/", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;
            try
            {
                WeightedPlacementJsonDocument document =
                    WeightedPlacementJsonCodec.Parse(source.text);
                if (document == null
                    || document.schemaVersion != WeightedPoolPlacementConfig.CurrentSchemaVersion
                    || !string.Equals(document.documentType,
                        WeightedPoolPlacementConfig.DocumentType, StringComparison.Ordinal)
                    || string.IsNullOrWhiteSpace(document.catalogId)
                    || string.IsNullOrWhiteSpace(document.chapterId)
                    || document.rows == null || document.rows.Count == 0) return false;
                error = string.Empty;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif
