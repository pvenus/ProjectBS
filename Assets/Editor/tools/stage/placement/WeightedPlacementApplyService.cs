#if UNITY_EDITOR
using System;
using System.IO;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage.Placement
{
    internal static class WeightedPlacementApplyService
    {
        internal const string Root = "Assets/Contents/Stage/placement";
        internal const string RulePath =
            "Assets/Contents/Stage/placement/rules/WeightedPoolPlacementRule.asset";
        internal const string JsonPath = Root + "/json/chapter1-events01-46.weighted-placement.json";

        internal static StagePlacementRuleSO CompileTransient(string json)
        {
            WeightedPlacementJsonDocument document = WeightedPlacementJsonCodec.Parse(json);
            var errors = WeightedPlacementJsonValidator.Validate(document);
            if (errors.Count != 0) throw new InvalidDataException(string.Join("\n", errors));
            var catalog = ScriptableObject.CreateInstance<StagePlacementRuleSO>();
            WeightedPlacementCompiler.ApplyDocument(document, catalog);
            return catalog;
        }

        internal static StagePlacementRuleSO Apply(string json, bool save)
        {
            WeightedPlacementJsonDocument document = WeightedPlacementJsonCodec.Parse(json);
            var errors = WeightedPlacementJsonValidator.Validate(document);
            if (errors.Count != 0) throw new InvalidDataException(string.Join("\n", errors));
            StagePlacementRuleSO catalog = AssetDatabase.LoadAssetAtPath<StagePlacementRuleSO>(RulePath);
            if (catalog == null) throw new FileNotFoundException("Canonical WeightedPoolPlacementRule is missing.", RulePath);
            WeightedPlacementCompiler.ApplyDocument(document, catalog);
            if (save)
            {
                AssetDatabase.SaveAssets();
            }
            return catalog;
        }

    }
}
#endif
