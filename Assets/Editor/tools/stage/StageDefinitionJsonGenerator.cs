#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    public static class StageDefinitionJsonGenerator
    {
        private const string DefaultOutputFolder = "Assets/Contents/Stage/so";

        [Serializable]
        private sealed class StageDefinitionJson
        {
            public string stageId;
            public string stageName;
            public string chapterKey;
            public string svgSourcePath;
            public string defaultPlacementRule;
            public bool useFixedSeed;
            public int seed;
            public List<SectionPlacementRuleJson> sectionPlacementRules = new();
        }

        [Serializable]
        private sealed class SectionPlacementRuleJson
        {
            public string sectionId;
            public string placementRule;
        }

        public static bool IsStageDefinitionJson(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath) || !File.Exists(jsonPath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(jsonPath);
                StageDefinitionJson data = JsonUtility.FromJson<StageDefinitionJson>(json);
                return data != null
                    && !string.IsNullOrWhiteSpace(data.stageId)
                    && !string.IsNullOrWhiteSpace(data.svgSourcePath);
            }
            catch
            {
                return false;
            }
        }

        public static StageDefinitionSO GenerateFromJsonPath(
            string jsonPath,
            string outputFolder = DefaultOutputFolder)
        {
            if (!IsStageDefinitionJson(jsonPath))
            {
                Debug.LogError($"[StageDefinitionJsonGenerator] Invalid definition json: {jsonPath}");
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            StageDefinitionJson data = JsonUtility.FromJson<StageDefinitionJson>(json);
            string svgPath = NormalizeAssetPath(data.svgSourcePath);

            if (!File.Exists(svgPath))
            {
                Debug.LogError($"[StageDefinitionJsonGenerator] SVG source not found: {svgPath}");
                return null;
            }

            EnsureFolder(outputFolder);

            StageSvgSlotMapBuilder.BuildResult slotBuild =
                new StageSvgSlotMapBuilder().BuildFromSvg(File.ReadAllText(svgPath));
            if (slotBuild == null || slotBuild.report == null || !slotBuild.report.isSuccess)
            {
                string errors = slotBuild?.report?.errorMessages == null
                    ? "unknown error"
                    : string.Join("; ", slotBuild.report.errorMessages);
                Debug.LogError($"[StageDefinitionJsonGenerator] SVG parsing failed: {errors}");
                return null;
            }

            string chapterKey = string.IsNullOrWhiteSpace(data.chapterKey)
                ? ResolveChapterKey(data.stageId)
                : data.chapterKey.Trim();
            List<RoundNodeSO> nodes = FindAssets<RoundNodeSO>(
                NormalizeAssetPath(outputFolder));
            List<StageStorySlotBinding> bindings = new StageStoryBindingResolver()
                .ResolveBindings(slotBuild.slots, chapterKey, nodes, slotBuild.report);
            List<StageRandomSection> sections = new StageRandomSectionResolver()
                .ResolveSections(slotBuild.slots, null, slotBuild.report);

            ApplyPlacementRules(data, sections);

            string normalizedOutput = NormalizeAssetPath(outputFolder);
            string assetPath = $"{normalizedOutput}/{data.stageId}.asset";
            StageDefinitionSO definition =
                AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(assetPath);
            bool isNew = definition == null;
            if (isNew)
            {
                definition = ScriptableObject.CreateInstance<StageDefinitionSO>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            definition.stageId = data.stageId;
            definition.stageName = data.stageName;
            definition.useFixedSeed = data.useFixedSeed;
            definition.seed = data.seed;
            new StageSlotMapDefinitionApplier().ApplyToDefinition(
                definition,
                slotBuild.slots,
                bindings,
                sections);

            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssetIfDirty(definition);
            AssetDatabase.SaveAssets();

            foreach (string warning in slotBuild.report.warningMessages)
            {
                Debug.LogWarning($"[StageDefinitionJsonGenerator] {warning}");
            }

            Debug.Log(
                $"[StageDefinitionJsonGenerator] {(isNew ? "Created" : "Updated")} "
                + $"StageDefinitionSO: {assetPath}, slots={slotBuild.slots.Count}, "
                + $"bindings={bindings.Count}, sections={sections.Count}");
            return definition;
        }

        private static void ApplyPlacementRules(
            StageDefinitionJson data,
            List<StageRandomSection> sections)
        {
            var overrides = (data.sectionPlacementRules ?? new List<SectionPlacementRuleJson>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.sectionId))
                .ToDictionary(x => x.sectionId, x => x.placementRule, StringComparer.OrdinalIgnoreCase);

            foreach (StageRandomSection section in sections)
            {
                string ruleKey = overrides.TryGetValue(section.sectionId, out string value)
                    ? value
                    : data.defaultPlacementRule;
                section.placementRule = FindPlacementRule(ruleKey);

                if (section.placementRule == null)
                {
                    Debug.LogWarning(
                        $"[StageDefinitionJsonGenerator] PlacementRuleSO not found. "
                        + $"section={section.sectionId}, key={ruleKey}");
                }
            }
        }

        private static StagePlacementRuleSO FindPlacementRule(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            string normalizedKey = NormalizeKey(key);
            return FindAssets<StagePlacementRuleSO>().FirstOrDefault(rule =>
                NormalizeKey(rule.name) == normalizedKey);
        }

        private static List<T> FindAssets<T>(
            params string[] searchFolders)
            where T : UnityEngine.Object
        {
            string[] guids = searchFolders != null && searchFolders.Length > 0
                ? AssetDatabase.FindAssets(
                    $"t:{typeof(T).Name}",
                    searchFolders)
                : AssetDatabase.FindAssets($"t:{typeof(T).Name}");

            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null)
                .ToList();
        }

        private static string ResolveChapterKey(string stageId)
        {
            const string prefix = "stage.";
            return !string.IsNullOrWhiteSpace(stageId)
                && stageId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    ? stageId.Substring(prefix.Length)
                    : stageId;
        }

        private static string NormalizeKey(string value)
        {
            return new string((value ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private static string NormalizeAssetPath(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/').TrimEnd('/');
        }

        private static void EnsureFolder(string assetFolder)
        {
            assetFolder = NormalizeAssetPath(assetFolder);
            if (AssetDatabase.IsValidFolder(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
#endif
