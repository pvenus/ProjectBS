using System;
using System.Collections.Generic;
using System.Text;
using Effect;
using Presentation;
using Skill;
using UnityEditor;
using UnityEngine;

public sealed class SkillPresentationStage6Validation : EditorWindow
{
    private static readonly string[] ApprovedRoots =
    {
        "Assets/Resources/skill/character/generated",
        "Assets/Resources/skill/json",
    };

    private static readonly HashSet<string> AllowedSkillGroupKeys = new(
        StringComparer.Ordinal)
    {
        "Activation",
        "Delivery",
        "Outcome",
        "SpecialEffect",
        "LinkedSkill",
    };

    private EquipmentSkillSO selectedSkill;
    private bool runtimeMode;
    private Vector2 scroll;

    [MenuItem("Tools/ProjectBS/Presentation/Open Skill Data Preview")]
    private static void OpenWindow()
    {
        SkillPresentationStage6Validation window = GetWindow<SkillPresentationStage6Validation>();
        window.titleContent = new GUIContent("Skill Presentation");
        window.selectedSkill = Selection.activeObject as EquipmentSkillSO;
        window.Show();
    }

    [MenuItem("Tools/ProjectBS/Presentation/Run Skill Asset Validation")]
    private static void RunFullValidation()
    {
        SkillPresentationResolver resolver = new();
        SkillPresentationGroupResolver groupResolver = new();
        HashSet<string> paths = FindApprovedSkillPaths();
        ValidationMatrix matrix = new();
        List<string> failures = new();

        foreach (string path in paths)
        {
            EquipmentSkillSO skill = AssetDatabase.LoadAssetAtPath<EquipmentSkillSO>(path);
            if (skill == null)
            {
                failures.Add($"Could not load EquipmentSkillSO: {path}");
                continue;
            }

            matrix.SkillCount++;
            SkillPresentationData preview = resolver.Resolve(skill, PresentationContext.Preview);
            ContentPresentationData previewContent = groupResolver.Resolve(preview);
            ValidateContent(skill, path, preview, previewContent, failures, matrix);
            ContentPresentationData playerContent =
                groupResolver.ResolveForPlayerDisplay(preview);
            ValidatePlayerDisplayCatalog(
                path,
                previewContent,
                playerContent,
                failures,
                matrix);

            EquipmentSkillRuntimeData runtime = new EquipmentSkillResolver().Resolve(
                skill,
                new EquipmentSkillInstanceData
                {
                    equipmentId = skill.EquipmentId,
                    currentLevel = 1,
                    upgradeLevel = 0,
                });
            SkillPresentationData runtimeData = resolver.Resolve(runtime, PresentationContext.Runtime);
            ContentPresentationData runtimeContent = groupResolver.Resolve(runtimeData);
            if (runtimeData.Status != ContentPresentationStatus.Supported
                || runtimeContent == null
                || runtimeContent.Provenance?.Kind != PresentationProvenanceKind.RuntimeResolved)
            {
                failures.Add($"Runtime composition failed: {path}");
            }

            CountSourceCases(skill, preview, matrix);
            CountUnits(previewContent, matrix);
            CountUnits(runtimeContent, matrix);
        }

        ValidateRequiredCases(matrix, failures);
        string report = matrix.BuildReport(paths.Count, failures);
        if (failures.Count == 0)
        {
            Debug.Log($"[SkillPresentationStage6Validation] PASS\n{report}");
        }
        else
        {
            Debug.LogError($"[SkillPresentationStage6Validation] FAIL\n{report}");
        }
    }

    [MenuItem("Assets/ProjectBS/Presentation/Log Selected Skill")]
    private static void LogSelectedSkill()
    {
        EquipmentSkillSO skill = Selection.activeObject as EquipmentSkillSO;
        if (skill == null)
        {
            Debug.LogWarning("[SkillPresentationStage6Validation] Select an EquipmentSkillSO asset first.");
            return;
        }

        SkillPresentationResolver resolver = new();
        SkillPresentationGroupResolver groupResolver = new();
        PresentationTextFormatter formatter = new();
        ContentPresentationData preview = groupResolver.Resolve(
            resolver.Resolve(skill, PresentationContext.Preview));
        EquipmentSkillRuntimeData runtime = new EquipmentSkillResolver().Resolve(
            skill,
            new EquipmentSkillInstanceData
            {
                equipmentId = skill.EquipmentId,
                currentLevel = 1,
                upgradeLevel = 0,
            });
        ContentPresentationData runtimeContent = groupResolver.Resolve(
            resolver.Resolve(runtime, PresentationContext.Runtime));

        Debug.Log(
            $"[SkillPresentationStage6Validation] {skill.name}\n\n" +
            $"--- Preview / authored SO ---\n{formatter.FormatPlainText(preview)}\n\n" +
            $"--- Runtime resolved / level 1 ---\n{formatter.FormatPlainText(runtimeContent)}");
    }

    private void OnGUI()
    {
        selectedSkill = (EquipmentSkillSO)EditorGUILayout.ObjectField(
            "Skill",
            selectedSkill,
            typeof(EquipmentSkillSO),
            false);
        runtimeMode = EditorGUILayout.Toggle("Runtime resolved", runtimeMode);

        if (selectedSkill == null)
        {
            EditorGUILayout.HelpBox(
                "Select an EquipmentSkillSO from an approved Skill asset path.",
                MessageType.Info);
            return;
        }

        ContentPresentationData content = ResolveSelectedContent();
        PresentationTextFormatter formatter = new();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.LabelField(
            string.IsNullOrWhiteSpace(content.Identity.DisplayName)
                ? content.Identity.ContentId
                : content.Identity.DisplayName,
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Content ID", content.Identity.ContentId);
        EditorGUILayout.LabelField("Status", content.Status.ToString());
        EditorGUILayout.LabelField("Provenance", content.Provenance?.Kind.ToString() ?? "Unknown");

        if (!string.IsNullOrWhiteSpace(content.Description))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(content.Description, EditorStyles.wordWrappedLabel);
        }

        foreach (string classification in content.ClassificationKeys)
        {
            EditorGUILayout.LabelField("Tag", formatter.FormatClassification(classification));
        }

        foreach (PresentationGroupData group in content.Groups)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(formatter.FormatLabel(group.Key), EditorStyles.boldLabel);
            if (!string.IsNullOrWhiteSpace(group.SourceContentId))
            {
                EditorGUILayout.LabelField("Source ID", group.SourceContentId);
            }
            if (!string.IsNullOrWhiteSpace(group.Description))
            {
                EditorGUILayout.LabelField(group.Description, EditorStyles.wordWrappedLabel);
            }

            foreach (PresentationEntryData entry in group.Entries)
            {
                EditorGUILayout.LabelField(
                    formatter.FormatLabel(entry.Key),
                    formatter.FormatValues(entry.Values));
                if (!string.IsNullOrWhiteSpace(entry.DetailContentId))
                {
                    EditorGUILayout.LabelField("Detail ID", entry.DetailContentId);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private ContentPresentationData ResolveSelectedContent()
    {
        SkillPresentationResolver resolver = new();
        SkillPresentationGroupResolver groupResolver = new();
        if (!runtimeMode)
        {
            return groupResolver.Resolve(resolver.Resolve(selectedSkill, PresentationContext.Preview));
        }

        EquipmentSkillRuntimeData runtime = new EquipmentSkillResolver().Resolve(
            selectedSkill,
            new EquipmentSkillInstanceData
            {
                equipmentId = selectedSkill.EquipmentId,
                currentLevel = 1,
                upgradeLevel = 0,
            });
        return groupResolver.Resolve(resolver.Resolve(runtime, PresentationContext.Runtime));
    }

    private static HashSet<string> FindApprovedSkillPaths()
    {
        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
        string[] guids = AssetDatabase.FindAssets("t:EquipmentSkillSO", ApprovedRoots);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrWhiteSpace(path))
            {
                paths.Add(path);
            }
        }
        return paths;
    }

    private static void ValidateContent(
        EquipmentSkillSO skill,
        string path,
        SkillPresentationData data,
        ContentPresentationData content,
        ICollection<string> failures,
        ValidationMatrix matrix)
    {
        if (data == null || content == null)
        {
            failures.Add($"Null presentation result: {path}");
            return;
        }
        if (data.Status != ContentPresentationStatus.Supported)
        {
            failures.Add($"Skill was not supported: {path}");
        }
        if (content.Identity == null || content.Identity.ContentId != skill.EquipmentId)
        {
            failures.Add($"Identity mismatch: {path}");
        }
        if (content.Provenance?.Kind != PresentationProvenanceKind.AuthoredAsset)
        {
            failures.Add($"Preview provenance mismatch: {path}");
        }

        ValidateSourceFaithfulGrouping(content, path, failures);

        foreach (SkillEffectPresentationItem item in EnumerateEffects(data))
        {
            switch (item.Effect.Status)
            {
                case ContentPresentationStatus.Supported:
                    matrix.SupportedEffectCount++;
                    break;
                case ContentPresentationStatus.DescriptionOnly:
                    matrix.DescriptionOnlyEffectCount++;
                    break;
                case ContentPresentationStatus.Unsupported:
                    matrix.UnsupportedEffectCount++;
                    failures.Add($"Unsupported effect in approved Skill: {path}");
                    break;
            }
        }
    }

    private static void ValidateSourceFaithfulGrouping(
        ContentPresentationData content,
        string path,
        ICollection<string> failures)
    {
        HashSet<string> seenGroups = new(StringComparer.Ordinal);
        foreach (PresentationGroupData group in content.Groups)
        {
            if (group == null)
            {
                continue;
            }

            if (!AllowedSkillGroupKeys.Contains(group.Key))
            {
                failures.Add($"Unclassified Skill group key '{group.Key}': {path}");
            }
            if (!seenGroups.Add(group.Key))
            {
                failures.Add($"Duplicate Skill group key '{group.Key}': {path}");
            }

            if (group.Key.StartsWith("Skill.Hit.", StringComparison.Ordinal)
                || group.Key.StartsWith("Skill.Effect.", StringComparison.Ordinal)
                || group.Key.EndsWith(".Behavior", StringComparison.Ordinal)
                || group.Key.EndsWith(".CountAndScale", StringComparison.Ordinal)
                || group.Key.EndsWith(".SizeAndLifetime", StringComparison.Ordinal))
            {
                failures.Add($"Invented presentation group key '{group.Key}': {path}");
            }

            foreach (PresentationEntryData entry in group.Entries)
            {
                if (entry != null && entry.Values.Count > 1)
                {
                    failures.Add(
                        $"Multiple source values were combined in entry '{entry.Key}': {path}");
                }

                string expectedGroup = GetExpectedNormalizedGroup(entry?.Key);
                if (!string.IsNullOrEmpty(expectedGroup)
                    && !string.Equals(group.Key, expectedGroup, StringComparison.Ordinal))
                {
                    failures.Add(
                        $"Entry '{entry.Key}' belongs to '{expectedGroup}', not '{group.Key}': {path}");
                }
            }
        }
    }

    private static string GetExpectedNormalizedGroup(string entryKey)
    {
        if (string.IsNullOrWhiteSpace(entryKey))
        {
            return null;
        }

        if (entryKey.StartsWith("Activation.", StringComparison.Ordinal))
        {
            return "Activation";
        }
        if (entryKey.StartsWith("Displacement.", StringComparison.Ordinal)
            || entryKey.StartsWith("Control.", StringComparison.Ordinal))
        {
            return "SpecialEffect";
        }
        if (entryKey.StartsWith("SkillInvoke.", StringComparison.Ordinal))
        {
            return "LinkedSkill";
        }
        if (entryKey.StartsWith("damage.", StringComparison.Ordinal)
            || entryKey.StartsWith("StatModifier.", StringComparison.Ordinal)
            || entryKey.StartsWith("Heal.", StringComparison.Ordinal)
            || entryKey.StartsWith("CooldownChange.", StringComparison.Ordinal)
            || entryKey.StartsWith("PeriodicDamage.", StringComparison.Ordinal))
        {
            return "Outcome";
        }

        return null;
    }

    private static void ValidatePlayerDisplayCatalog(
        string path,
        ContentPresentationData inspection,
        ContentPresentationData player,
        ICollection<string> failures,
        ValidationMatrix matrix)
    {
        if (player == null)
        {
            failures.Add($"Null player-display presentation: {path}");
            return;
        }

        matrix.InspectionEntryCount += CountEntries(inspection);
        matrix.PlayerEntryCount += CountEntries(player);

        foreach (string tag in player.ClassificationKeys)
        {
            if (!PresentationDisplayCatalog.IsPlayerVisibleTag(tag)
                || string.IsNullOrWhiteSpace(
                    PresentationDisplayCatalog.GetTagLabelKey(tag)))
            {
                failures.Add($"Unmapped player tag '{tag}': {path}");
            }
        }

        foreach (PresentationGroupData group in player.Groups)
        {
            if (group == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    PresentationDisplayCatalog.GetGroupLabelKey(group.Key)))
            {
                failures.Add($"Unmapped player group '{group.Key}': {path}");
            }

            foreach (PresentationEntryData entry in group.Entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (!PresentationDisplayCatalog.IsPlayerVisibleEntry(entry.Key)
                    || string.IsNullOrWhiteSpace(
                        PresentationDisplayCatalog.GetEntryLabelKey(entry.Key)))
                {
                    failures.Add($"Unmapped player entry '{entry.Key}': {path}");
                    continue;
                }

                foreach (PresentationValueData value in entry.Values)
                {
                    if (value == null
                        || value.Kind != PresentationValueKind.Token
                        || PresentationDisplayCatalog.IsEntryTokenLocalizedText(entry.Key))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(
                            PresentationDisplayCatalog.GetEntryTokenKey(
                                entry.Key,
                                value.Token)))
                    {
                        failures.Add(
                            $"Unmapped player token '{value.Token}' for '{entry.Key}': {path}");
                    }
                }
            }
        }
    }

    private static int CountEntries(ContentPresentationData content)
    {
        if (content == null)
        {
            return 0;
        }

        int count = 0;
        foreach (PresentationGroupData group in content.Groups)
        {
            if (group != null)
            {
                count += group.Entries.Count;
            }
        }
        return count;
    }

    private static void CountSourceCases(
        EquipmentSkillSO skill,
        SkillPresentationData data,
        ValidationMatrix matrix)
    {
        int hitCount = skill.HitSos?.Length ?? 0;
        int effectCount = 0;
        foreach (SkillEffectPresentationItem _ in EnumerateEffects(data))
        {
            effectCount++;
        }

        matrix.NullEffectSlotCount += CountNullEntries(skill.CastSo?.SelfEffects);

        if (hitCount == 0)
        {
            matrix.NoHitSkillCount++;
        }
        if (effectCount == 0)
        {
            matrix.NoEffectSkillCount++;
        }
        else if (effectCount == 1)
        {
            matrix.OneEffectSkillCount++;
        }
        else
        {
            matrix.MultipleEffectSkillCount++;
        }

        if (skill.HitSos == null)
        {
            return;
        }
        foreach (SkillHitSO hit in skill.HitSos)
        {
            if (hit == null)
            {
                continue;
            }

            matrix.NullEffectSlotCount += CountNullEntries(hit.BuffEffects);
            matrix.NullEffectSlotCount += CountNullEntries(hit.DebuffEffects);
            if (hit.SpawnSkill != null)
            {
                matrix.DeferredNestedSkillCount++;
            }
        }
    }

    private static int CountNullEntries(EffectEntrySO[] entries)
    {
        if (entries == null || entries.Length == 0)
        {
            return 0;
        }

        int count = 0;
        foreach (EffectEntrySO entry in entries)
        {
            if (entry == null)
            {
                count++;
            }
        }
        return count;
    }

    private static IEnumerable<SkillEffectPresentationItem> EnumerateEffects(
        SkillPresentationData data)
    {
        foreach (SkillEffectPresentationItem effect in data.SelfEffects)
        {
            yield return effect;
        }
        foreach (SkillHitPresentationData hit in data.Hits)
        {
            foreach (SkillEffectPresentationItem effect in hit.Effects)
            {
                yield return effect;
            }
        }
    }

    private static void CountUnits(
        ContentPresentationData content,
        ValidationMatrix matrix)
    {
        foreach (PresentationGroupData group in content.Groups)
        {
            foreach (PresentationEntryData entry in group.Entries)
            {
                foreach (PresentationValueData value in entry.Values)
                {
                    if (value.Unit == PresentationValueUnit.Ratio)
                    {
                        matrix.RatioValueCount++;
                    }
                    else if (value.Unit == PresentationValueUnit.Percent)
                    {
                        matrix.PercentValueCount++;
                    }
                }
            }
        }
    }

    private static void ValidateRequiredCases(
        ValidationMatrix matrix,
        ICollection<string> failures)
    {
        if (matrix.SkillCount == 0) failures.Add("No approved EquipmentSkillSO assets found.");
        if (matrix.NoHitSkillCount == 0) failures.Add("No no-hit Skill fixture found.");
        if (matrix.NoEffectSkillCount == 0) failures.Add("No no-effect Skill fixture found.");
        if (matrix.OneEffectSkillCount == 0) failures.Add("No one-effect Skill fixture found.");
        if (matrix.MultipleEffectSkillCount == 0) failures.Add("No multiple-effect Skill fixture found.");
        if (matrix.PercentValueCount == 0) failures.Add("No Percent presentation value found.");
        if (matrix.RatioValueCount == 0) failures.Add("No Ratio presentation value found.");
    }

    private sealed class ValidationMatrix
    {
        public int SkillCount;
        public int NoHitSkillCount;
        public int NoEffectSkillCount;
        public int OneEffectSkillCount;
        public int MultipleEffectSkillCount;
        public int SupportedEffectCount;
        public int DescriptionOnlyEffectCount;
        public int UnsupportedEffectCount;
        public int NullEffectSlotCount;
        public int DeferredNestedSkillCount;
        public int RatioValueCount;
        public int PercentValueCount;
        public int InspectionEntryCount;
        public int PlayerEntryCount;

        public string BuildReport(int uniquePathCount, IReadOnlyCollection<string> failures)
        {
            StringBuilder builder = new();
            builder.AppendLine($"Approved unique Skill paths: {uniquePathCount}");
            builder.AppendLine($"Resolved Skills: {SkillCount}");
            builder.AppendLine($"No hit / no Effect / one Effect / multiple Effects: {NoHitSkillCount} / {NoEffectSkillCount} / {OneEffectSkillCount} / {MultipleEffectSkillCount}");
            builder.AppendLine($"Supported / description-only / unsupported Effects: {SupportedEffectCount} / {DescriptionOnlyEffectCount} / {UnsupportedEffectCount}");
            builder.AppendLine($"Ignored null EffectEntry slots: {NullEffectSlotCount}");
            builder.AppendLine($"Ratio / Percent values: {RatioValueCount} / {PercentValueCount}");
            builder.AppendLine($"Inspection / player-visible entries: {InspectionEntryCount} / {PlayerEntryCount}");
            builder.AppendLine($"Deferred nested Skill references: {DeferredNestedSkillCount}");
            builder.AppendLine("JSON/SO mismatches: retained from the Stage 1 inventory; no repair or migration performed.");
            builder.AppendLine($"Failures: {failures.Count}");
            foreach (string failure in failures)
            {
                builder.AppendLine($"- {failure}");
            }
            return builder.ToString().TrimEnd();
        }
    }
}
