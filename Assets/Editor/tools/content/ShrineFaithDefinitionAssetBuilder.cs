using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Bless;
using Character;
using Shrine;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Content
{
    public static class ShrineFaithDefinitionAssetBuilder
    {
        private const string SourceRoot = "Assets/Contents/Faith";
        private const string GeneratedBlessRoot = "Assets/Contents/Faith/Generated";
        private const string FaithStringPath = "Assets/Resources/string/faith_string.csv";
        private const int ExpectedSourceCount = 6;

        [Serializable]
        private sealed class FaithDefinitionJson
        {
            public string faithId;
            public string nameKo;
            public string descriptionKo;
            public string godType;
            public string shrineGodAssetPath;
            public string sourceReference;
            public BasicBlessJson basicBlessing;
            public JobChangeJson exclusiveJobChange;
            public ExclusiveBlessJson exclusiveBless1;
            public ExclusiveBlessJson exclusiveBless2;
        }

        [Serializable]
        private sealed class BasicBlessJson
        {
            public string featureId;
            public string sourceStatus;
            public string nameKo;
            public BasicBlessLevelJson[] levels;
        }

        [Serializable]
        private sealed class BasicBlessLevelJson
        {
            public int level;
            public string descriptionKo;
        }

        [Serializable]
        private sealed class JobChangeJson
        {
            public string featureId;
            public string sourceStatus;
            public int unlockLevel;
            public string faithLockRequirement;
            public JobTransitionJson[] transitions;
        }

        [Serializable]
        private sealed class JobTransitionJson
        {
            public string fromJob;
            public string toJob;
            public string fromNameKo;
            public string toNameKo;
        }

        [Serializable]
        private sealed class ExclusiveBlessJson
        {
            public string featureId;
            public string sourceStatus;
            public int unlockLevel;
            public string faithLockRequirement;
            public string nameKo;
            public string descriptionKo;
        }

        [MenuItem("Tools/ProjectBS/Contents/Build Faith Definitions", false, 2210)]
        private static void BuildMenu()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Build Faith definitions",
                "Create ShrineFaithDefinitionSO assets beside the Faith JSON files and "
                + "description-only BlessSO assets under Faith/Generated.\n\n"
                + "This does not modify legacy Assets/Resources/shring data and does not "
                + "claim unimplemented Bless effects are runtime-ready.\n\nContinue?",
                "Build",
                "Cancel");

            if (confirmed)
            {
                BuildAll();
            }
        }

        [MenuItem("Tools/ProjectBS/Contents/Validate Faith Definition Sources", false, 2211)]
        private static void ValidateMenu()
        {
            ValidateSources();
        }

        public static void BuildAll()
        {
            List<FaithDefinitionJson> sources = LoadAndValidateSources();
            EnsureFolder(GeneratedBlessRoot);

            foreach (FaithDefinitionJson source in sources)
            {
                BuildDefinition(source);
            }

            WriteLocalization(sources);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ShrineFaithDefinitionAssetBuilder] Build passed. Faith definitions={sources.Count}.");
        }

        public static void ValidateSources()
        {
            List<FaithDefinitionJson> sources = LoadAndValidateSources();
            Debug.Log($"[ShrineFaithDefinitionAssetBuilder] Source validation passed. Faith definitions={sources.Count}.");
        }

        private static List<FaithDefinitionJson> LoadAndValidateSources()
        {
            string[] paths = Directory.GetFiles(SourceRoot, "faith.*.json", SearchOption.TopDirectoryOnly);
            Array.Sort(paths, StringComparer.Ordinal);

            if (paths.Length != ExpectedSourceCount)
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Expected {ExpectedSourceCount} Faith JSON files, found {paths.Length}.");
            }

            HashSet<string> faithIds = new(StringComparer.Ordinal);
            HashSet<string> featureIds = new(StringComparer.Ordinal);
            List<FaithDefinitionJson> result = new();

            foreach (string path in paths)
            {
                FaithDefinitionJson source = JsonUtility.FromJson<FaithDefinitionJson>(File.ReadAllText(path));
                ValidateSource(source, path, faithIds, featureIds);
                result.Add(source);
            }

            return result;
        }

        private static void ValidateSource(
            FaithDefinitionJson source,
            string path,
            ISet<string> faithIds,
            ISet<string> featureIds)
        {
            if (source == null
                || string.IsNullOrWhiteSpace(source.faithId)
                || string.IsNullOrWhiteSpace(source.nameKo)
                || string.IsNullOrWhiteSpace(source.sourceReference)
                || !faithIds.Add(source.faithId)
                || !Enum.TryParse(source.godType, true, out ShrineGodType _))
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Invalid Faith identity. path={path}");
            }

            ValidateFeatureId(source.basicBlessing?.featureId, path, featureIds);
            ValidateFeatureId(source.exclusiveJobChange?.featureId, path, featureIds);
            ValidateFeatureId(source.exclusiveBless1?.featureId, path, featureIds);
            ValidateFeatureId(source.exclusiveBless2?.featureId, path, featureIds);

            ValidateBasicBlessing(source.basicBlessing, path);
            ValidateJobChange(source.exclusiveJobChange, path);
            ValidateExclusiveBless(source.exclusiveBless1, path);
            ValidateExclusiveBless(source.exclusiveBless2, path);
        }

        private static void ValidateFeatureId(string featureId, string path, ISet<string> featureIds)
        {
            if (string.IsNullOrWhiteSpace(featureId) || !featureIds.Add(featureId))
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Missing or duplicate featureId. featureId={featureId}, path={path}");
            }
        }

        private static void ValidateBasicBlessing(BasicBlessJson source, string path)
        {
            FaithDefinitionSourceStatus status = ParseStatus(source.sourceStatus, path);
            BasicBlessLevelJson[] levels = source.levels ?? Array.Empty<BasicBlessLevelJson>();

            if (status == FaithDefinitionSourceStatus.ConfirmedDescriptionOnly && levels.Length != 10)
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Confirmed Basic Bless requires 10 explicit levels. path={path}");
            }

            HashSet<int> seenLevels = new();
            foreach (BasicBlessLevelJson level in levels)
            {
                if (level == null
                    || level.level < 1
                    || level.level > 10
                    || string.IsNullOrWhiteSpace(level.descriptionKo)
                    || !seenLevels.Add(level.level))
                {
                    throw new InvalidOperationException(
                        $"[ShrineFaithDefinitionAssetBuilder] Invalid Basic Bless level. path={path}");
                }
            }
        }

        private static void ValidateJobChange(JobChangeJson source, string path)
        {
            FaithDefinitionSourceStatus status = ParseStatus(source.sourceStatus, path);
            ParseLockRequirement(source.faithLockRequirement, path);
            JobTransitionJson[] transitions = source.transitions ?? Array.Empty<JobTransitionJson>();

            if (status != FaithDefinitionSourceStatus.PendingSource && transitions.Length == 0)
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Confirmed Job Change has no transitions. path={path}");
            }

            HashSet<CharacterJob> sources = new();
            foreach (JobTransitionJson transition in transitions)
            {
                if (transition == null
                    || !Enum.TryParse(transition.fromJob, true, out CharacterJob fromJob)
                    || !Enum.TryParse(transition.toJob, true, out CharacterJob toJob)
                    || !sources.Add(fromJob)
                    || !new FaithJobTransitionDefinition(
                        fromJob,
                        toJob,
                        JobLocalizationKey(fromJob),
                        JobLocalizationKey(toJob)).IsValid)
                {
                    throw new InvalidOperationException(
                        $"[ShrineFaithDefinitionAssetBuilder] Invalid Job transition. path={path}");
                }
            }
        }

        private static void ValidateExclusiveBless(ExclusiveBlessJson source, string path)
        {
            FaithDefinitionSourceStatus status = ParseStatus(source.sourceStatus, path);
            ParseLockRequirement(source.faithLockRequirement, path);

            if (source.unlockLevel < 0
                || source.unlockLevel > 10
                || status != FaithDefinitionSourceStatus.PendingSource
                && string.IsNullOrWhiteSpace(source.nameKo)
                || status != FaithDefinitionSourceStatus.PendingSource
                && string.IsNullOrWhiteSpace(source.descriptionKo))
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Invalid Exclusive Bless. featureId={source.featureId}, path={path}");
            }
        }

        private static void BuildDefinition(FaithDefinitionJson source)
        {
            ShrineGodType godType = ParseEnum<ShrineGodType>(source.godType, source.faithId);
            ShrineGodSO shrineGod = string.IsNullOrWhiteSpace(source.shrineGodAssetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<ShrineGodSO>(source.shrineGodAssetPath);

            List<FaithBasicBlessLevelDefinition> basicLevels = new();
            foreach (BasicBlessLevelJson level in source.basicBlessing.levels ?? Array.Empty<BasicBlessLevelJson>())
            {
                string blessId = $"bless.{source.basicBlessing.featureId}.level_{level.level:00}";
                BlessSO bless = CreateOrUpdateDescriptionBless(
                    blessId,
                    source.faithId,
                    godType);
                basicLevels.Add(new FaithBasicBlessLevelDefinition(level.level, bless));
            }

            FaithBasicBlessDefinition basicBlessing = new(
                source.basicBlessing.featureId,
                ParseStatus(source.basicBlessing.sourceStatus, source.faithId),
                basicLevels);

            List<FaithJobTransitionDefinition> transitions = new();
            foreach (JobTransitionJson transition in source.exclusiveJobChange.transitions ?? Array.Empty<JobTransitionJson>())
            {
                transitions.Add(new FaithJobTransitionDefinition(
                    ParseEnum<CharacterJob>(transition.fromJob, source.faithId),
                    ParseEnum<CharacterJob>(transition.toJob, source.faithId),
                    JobLocalizationKey(ParseEnum<CharacterJob>(transition.fromJob, source.faithId)),
                    JobLocalizationKey(ParseEnum<CharacterJob>(transition.toJob, source.faithId))));
            }

            FaithJobChangeDefinition jobChange = new(
                source.exclusiveJobChange.featureId,
                ParseStatus(source.exclusiveJobChange.sourceStatus, source.faithId),
                source.exclusiveJobChange.unlockLevel,
                ParseLockRequirement(source.exclusiveJobChange.faithLockRequirement, source.faithId),
                transitions);

            FaithExclusiveBlessDefinition exclusive1 = BuildExclusiveBless(
                source.faithId,
                godType,
                source.exclusiveBless1);
            FaithExclusiveBlessDefinition exclusive2 = BuildExclusiveBless(
                source.faithId,
                godType,
                source.exclusiveBless2);

            string assetPath = $"{SourceRoot}/{source.faithId}.asset";
            ShrineFaithDefinitionSO definition =
                AssetDatabase.LoadAssetAtPath<ShrineFaithDefinitionSO>(assetPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<ShrineFaithDefinitionSO>();
                AssetDatabase.CreateAsset(definition, assetPath);
            }

            definition.ApplyEditorData(
                source.faithId,
                godType,
                shrineGod,
                source.sourceReference,
                basicBlessing,
                jobChange,
                exclusive1,
                exclusive2);
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssetIfDirty(definition);
        }

        private static FaithExclusiveBlessDefinition BuildExclusiveBless(
            string faithId,
            ShrineGodType godType,
            ExclusiveBlessJson source)
        {
            FaithDefinitionSourceStatus status = ParseStatus(source.sourceStatus, faithId);
            BlessSO bless = status == FaithDefinitionSourceStatus.PendingSource
                ? null
                : CreateOrUpdateDescriptionBless($"bless.{source.featureId}", faithId, godType);

            return new FaithExclusiveBlessDefinition(
                source.featureId,
                status,
                source.unlockLevel,
                ParseLockRequirement(source.faithLockRequirement, faithId),
                bless);
        }

        private static BlessSO CreateOrUpdateDescriptionBless(
            string blessingId,
            string groupId,
            ShrineGodType godType)
        {
            string assetPath = $"{GeneratedBlessRoot}/{blessingId}.asset";
            BlessSO bless = AssetDatabase.LoadAssetAtPath<BlessSO>(assetPath);
            if (bless == null)
            {
                bless = ScriptableObject.CreateInstance<BlessSO>();
                AssetDatabase.CreateAsset(bless, assetPath);
            }

            SerializedObject serialized = new(bless);
            serialized.FindProperty("blessingId").stringValue = blessingId;
            serialized.FindProperty("groupId").stringValue = groupId;
            serialized.FindProperty("category").intValue = (int)BlessCategory.Special;
            serialized.FindProperty("godType").intValue = (int)godType;
            serialized.FindProperty("durationType").intValue = (int)BlessDurationType.Permanent;
            serialized.FindProperty("durationBattleCount").intValue = -1;
            serialized.FindProperty("effectEntries").arraySize = 0;
            serialized.FindProperty("tags").arraySize = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(bless);
            AssetDatabase.SaveAssetIfDirty(bless);
            return bless;
        }

        private static FaithDefinitionSourceStatus ParseStatus(string value, string source)
        {
            return ParseEnum<FaithDefinitionSourceStatus>(value, source);
        }

        private static FaithLockRequirement ParseLockRequirement(string value, string source)
        {
            return ParseEnum<FaithLockRequirement>(value, source);
        }

        private static TEnum ParseEnum<TEnum>(string value, string source)
            where TEnum : struct
        {
            if (Enum.TryParse(value, true, out TEnum parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException(
                $"[ShrineFaithDefinitionAssetBuilder] Invalid {typeof(TEnum).Name}. value={value}, source={source}");
        }

        private static string JobLocalizationKey(CharacterJob job)
        {
            return $"character.job.{job}";
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private static void WriteLocalization(IReadOnlyList<FaithDefinitionJson> sources)
        {
            Dictionary<string, string> rows = new(StringComparer.Ordinal);

            foreach (FaithDefinitionJson source in sources)
            {
                AddLocalization(rows, source.faithId, "name", source.nameKo);
                AddLocalization(rows, source.faithId, "desc", source.descriptionKo);

                foreach (JobTransitionJson transition in
                         source.exclusiveJobChange.transitions ?? Array.Empty<JobTransitionJson>())
                {
                    CharacterJob fromJob = ParseEnum<CharacterJob>(transition.fromJob, source.faithId);
                    CharacterJob toJob = ParseEnum<CharacterJob>(transition.toJob, source.faithId);
                    AddLocalization(rows, JobLocalizationKey(fromJob), "name", transition.fromNameKo);
                    AddLocalization(rows, JobLocalizationKey(toJob), "name", transition.toNameKo);
                }

                foreach (BasicBlessLevelJson level in
                         source.basicBlessing.levels ?? Array.Empty<BasicBlessLevelJson>())
                {
                    string blessId = $"bless.{source.basicBlessing.featureId}.level_{level.level:00}";
                    AddLocalization(rows, blessId, "name", source.basicBlessing.nameKo);
                    AddLocalization(rows, blessId, "desc", level.descriptionKo);
                }

                AddExclusiveLocalization(rows, source.exclusiveBless1);
                AddExclusiveLocalization(rows, source.exclusiveBless2);
            }

            StringBuilder csv = new("main_key,sub_key,ko,en\n");
            foreach (KeyValuePair<string, string> row in rows)
            {
                int separator = row.Key.LastIndexOf('|');
                csv.Append(EscapeCsv(row.Key.Substring(0, separator)))
                    .Append(',')
                    .Append(EscapeCsv(row.Key.Substring(separator + 1)))
                    .Append(',')
                    .Append(EscapeCsv(row.Value))
                    .Append(',')
                    .Append('\n');
            }

            File.WriteAllText(FaithStringPath, csv.ToString(), new UTF8Encoding(true));
            AssetDatabase.ImportAsset(FaithStringPath, ImportAssetOptions.ForceUpdate);
        }

        private static void AddExclusiveLocalization(
            IDictionary<string, string> rows,
            ExclusiveBlessJson source)
        {
            if (source == null
                || ParseStatus(source.sourceStatus, source.featureId)
                == FaithDefinitionSourceStatus.PendingSource)
            {
                return;
            }

            string blessId = $"bless.{source.featureId}";
            AddLocalization(rows, blessId, "name", source.nameKo);
            AddLocalization(rows, blessId, "desc", source.descriptionKo);
        }

        private static void AddLocalization(
            IDictionary<string, string> rows,
            string mainKey,
            string subKey,
            string korean)
        {
            if (string.IsNullOrWhiteSpace(mainKey)
                || string.IsNullOrWhiteSpace(subKey)
                || string.IsNullOrWhiteSpace(korean))
            {
                return;
            }

            string key = $"{mainKey}|{subKey}";
            if (rows.TryGetValue(key, out string existing)
                && !string.Equals(existing, korean, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"[ShrineFaithDefinitionAssetBuilder] Conflicting localization. key={mainKey}.{subKey}");
            }

            rows[key] = korean;
        }

        private static string EscapeCsv(string value)
        {
            string normalized = value ?? string.Empty;
            return normalized.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0
                ? normalized
                : $"\"{normalized.Replace("\"", "\"\"")}\"";
        }
    }
}
