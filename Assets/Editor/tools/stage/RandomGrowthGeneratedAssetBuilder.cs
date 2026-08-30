#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Stage;
using UnityEditor;
using UnityEngine;

namespace ResourceTools.Stage
{
    public enum RandomGrowthGenerationDisposition
    {
        Blocked = 0,
        ReadyToGenerate = 1,
        NoOp = 2
    }

    public enum RandomGrowthGeneratedContentKind
    {
        SmithyRisk = 0,
        SafeObservation = 1
    }

    public sealed class RandomGrowthGeneratedAssetPreflight
    {
        internal RandomGrowthGeneratedAssetPreflight(
            RandomGrowthGenerationDisposition disposition,
            RandomGrowthProjectionBlueprint blueprint,
            IEnumerable<string> errors)
        {
            Disposition = disposition;
            Blueprint = blueprint;
            Errors = errors.ToArray();
        }

        public RandomGrowthGenerationDisposition Disposition { get; }
        public RandomGrowthProjectionBlueprint Blueprint { get; }
        public IReadOnlyList<string> Errors { get; }
        public bool CanWrite => Disposition == RandomGrowthGenerationDisposition.ReadyToGenerate;
    }

    public sealed class RandomGrowthPresentationCatalogPreflight
    {
        internal RandomGrowthPresentationCatalogPreflight(
            RandomGrowthGenerationDisposition disposition,
            RandomGrowthSafePresentationCatalogBlueprint blueprint,
            IEnumerable<string> errors)
        { Disposition = disposition; Blueprint = blueprint; Errors = errors.ToArray(); }
        public RandomGrowthGenerationDisposition Disposition { get; }
        public RandomGrowthSafePresentationCatalogBlueprint Blueprint { get; }
        public IReadOnlyList<string> Errors { get; }
        public bool CanWrite => Disposition == RandomGrowthGenerationDisposition.ReadyToGenerate;
    }

    public sealed class SafeV2ProductionSnapshot
    {
        internal SafeV2ProductionSnapshot(string popupSha, string popupGuid,
            string catalogSha, string catalogGuid, string nodeSha, string poolSha,
            OrdinaryPoolLinkSnapshot placement)
        { PopupSha = popupSha; PopupGuid = popupGuid; CatalogSha = catalogSha;
          CatalogGuid = catalogGuid; RoundNodeSha = nodeSha; EventPoolSha = poolSha;
          Placement = placement; }
        public string PopupSha { get; }
        public string PopupGuid { get; }
        public string CatalogSha { get; }
        public string CatalogGuid { get; }
        public string RoundNodeSha { get; }
        public string EventPoolSha { get; }
        public OrdinaryPoolLinkSnapshot Placement { get; }
    }

    public sealed class RandomGrowthProjectionBlueprint
    {
        public RandomGrowthGeneratedContentKind Kind { get; internal set; }
        public RandomGrowthContentBuildPlan Paths { get; internal set; }
        public string StageId { get; internal set; }
        public string PopupId { get; internal set; }
        public string PoolId { get; internal set; }
        public string ImagePath { get; internal set; }
        public string ImageGuid { get; internal set; }
        public string ExpectedImageGuid { get; internal set; }
        public long ImageLocalId { get; internal set; }
        public string DefinitionFingerprint { get; internal set; }
        public string DisplayName { get; internal set; }
        public RandomGrowthContentChoice Risk { get; internal set; }
        public RandomGrowthContentChoice Decline { get; internal set; }
        public RandomGrowthPoolEntryContent PoolEntry { get; internal set; }
        public IReadOnlyList<PopupEventChoice> ProjectedChoices { get; internal set; }
        public string EntryId { get; internal set; }
        public int EntryWeight { get; internal set; }
        public bool EntryOneShot { get; internal set; }
        public int EntryCooldownRounds { get; internal set; }
        public int EntryMinDepth { get; internal set; }
        public int EntryMaxDepth { get; internal set; }
        public IReadOnlyList<string> EntryTags { get; internal set; }
    }

    /// <summary>
    /// 신규 랜덤 성장 콘텐츠만 다루는 전용 생성기다.
    /// Legacy CSV/string/popup builders를 호출하지 않으며, 모든 출력 preflight가
    /// 성공한 뒤에만 Popup -> RoundNode -> EventPool 순으로 신규 에셋을 만든다.
    /// </summary>
    public static class RandomGrowthGeneratedAssetBuilder
    {
        public const string ExpectedImageGuid = "ec28c27a89a3242c3830369462e22290";
        public const string SafeExpectedImageGuid = "e977923f8a294398ac4091979381ed8f";
        public const string SafeImagePath = "Assets/ImagesGenerated/Stage/popup_main/node.act1.random_growth.02.windworn_sword_marks.intro.main.png";
        public const long ExpectedSpriteLocalId = 21300000;
        public const string ManifestOnlyPoolGuid = "8771f6d8564d343e08adbaed6dc28256";
        public const string ManifestOnlyPoolId = "event_pool.act1.random_growth.cheongun_sangui";
        public const string SafeManifestOnlyPoolId = "event_pool.act1.random_growth.safe";
        public const string OrdinaryPlacementRulePath =
            "Assets/Contents/Stage/placement/rules/WeightedPoolPlacementRule.asset";
        public const string ManifestOnlyOrdinaryLinkError = "MANIFEST_ONLY_POOL_ORDINARY_LINK";

        public static RandomGrowthGeneratedAssetPreflight Preflight()
        {
            RandomGrowthContentContractValidator.Result source =
                RandomGrowthContentContractValidator.ValidateFiles();
            if (!source.IsValid)
            {
                return Blocked(null, source.Errors.Select(x => "CONTENT:" + x));
            }

            RandomGrowthProjectionBlueprint blueprint = CreateBlueprint(source);
            return PreflightBlueprint(blueprint);
        }

        public static RandomGrowthGeneratedAssetPreflight PreflightSafe()
        {
            RandomGrowthSafeContentValidation source =
                RandomGrowthSafeProjectionContract.ValidateProductionV2Files();
            if (!source.IsValid)
                return Blocked(null, source.Errors.Select(x => "SAFE_CONTENT:" + x));
            return PreflightBlueprint(CreateSafeBlueprint(source));
        }

        /// <summary>Gate1 synthetic-only catalog output-set evaluation. Performs no AssetDatabase writes.</summary>
        public static RandomGrowthPresentationCatalogPreflight PreflightSafeV2Catalog(
            bool corePopupExists, bool coreNodeExists, bool corePoolExists,
            bool coreSemanticMatch, bool catalogExists, bool catalogSemanticMatch,
            IEnumerable<string> collisions = null)
        {
            RandomGrowthSafePresentationCatalogBlueprint blueprint =
                RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
            List<string> errors = (collisions ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => "PRESENTATION_CATALOG_COLLISION:" + x).ToList();
            int coreCount = (corePopupExists ? 1 : 0) + (coreNodeExists ? 1 : 0)
                + (corePoolExists ? 1 : 0);
            if (coreCount != 0 && coreCount != 3) errors.Add("SAFE_CORE_PARTIAL_OUTPUT_SET");
            if (coreCount == 3 && !coreSemanticMatch) errors.Add("SAFE_CORE_SNAPSHOT_MISMATCH");
            if (coreCount == 0 && catalogExists) errors.Add("PRESENTATION_CATALOG_ORPHAN");
            if (catalogExists && !catalogSemanticMatch) errors.Add("PRESENTATION_CATALOG_SNAPSHOT_MISMATCH");
            if (errors.Count > 0)
                return new RandomGrowthPresentationCatalogPreflight(
                    RandomGrowthGenerationDisposition.Blocked, blueprint, errors);
            if (!catalogExists)
                return new RandomGrowthPresentationCatalogPreflight(
                    RandomGrowthGenerationDisposition.ReadyToGenerate, blueprint, Array.Empty<string>());
            return new RandomGrowthPresentationCatalogPreflight(
                RandomGrowthGenerationDisposition.NoOp, blueprint, Array.Empty<string>());
        }

        public static RandomGrowthPresentationCatalogPreflight PreflightSafeV2Production()
        {
            RandomGrowthSafePresentationCatalogBlueprint blueprint =
                RandomGrowthSafeProjectionContract.CreateV2CatalogBlueprint();
            List<string> errors = new();
            RandomGrowthSafeContentValidation content =
                RandomGrowthSafeProjectionContract.ValidateProductionV2Files();
            if (!content.IsValid) errors.AddRange(content.Errors.Select(x => "SAFE_V2_CONTENT:" + x));

            string popupPath = RandomGrowthSafeProjectionContract.CreateSnapshot(
                RandomGrowthSafeProjectionContract.SemanticCopyDigest).Paths.PopupEventAssetPath;
            string nodePath = RandomGrowthSafeProjectionContract.CreateSnapshot(
                RandomGrowthSafeProjectionContract.SemanticCopyDigest).Paths.RoundNodeAssetPath;
            string poolPath = RandomGrowthSafeProjectionContract.CreateSnapshot(
                RandomGrowthSafeProjectionContract.SemanticCopyDigest).Paths.EventPoolAssetPath;
            PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(popupPath);
            RoundNodeSO node = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(nodePath);
            EventPoolSO pool = AssetDatabase.LoadAssetAtPath<EventPoolSO>(poolPath);
            if (popup == null || node == null || pool == null) errors.Add("SAFE_V2_CORE_PARTIAL_OR_TYPE_MISMATCH");
            else if (node.popupEvent != popup || pool.entries == null || pool.entries.Count != 1
                || pool.entries[0].node != node) errors.Add("SAFE_V2_CORE_REFERENCE_MISMATCH");
            errors.AddRange(ValidateOrdinaryPoolLinks());

            foreach (string guid in AssetDatabase.FindAssets("t:RandomGrowthPresentationCopyAsset"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RandomGrowthPresentationCopyAsset candidate =
                    AssetDatabase.LoadAssetAtPath<RandomGrowthPresentationCopyAsset>(path);
                if (candidate != null && candidate.CatalogId == RandomGrowthSafeProjectionContract.V2CatalogId
                    && path != blueprint.AssetPath)
                    errors.Add("PRESENTATION_CATALOG_COLLISION:" + path);
            }

            bool catalogExists = File.Exists(blueprint.AssetPath);
            bool catalogMatch = false;
            if (catalogExists)
            {
                RandomGrowthPresentationCopyAsset catalog =
                    AssetDatabase.LoadAssetAtPath<RandomGrowthPresentationCopyAsset>(blueprint.AssetPath);
                catalogMatch = RandomGrowthPresentationCopyResolver.TryResolve(catalog,
                    blueprint.Expectation, out _, out RandomGrowthPresentationCopyMismatch mismatch);
                if (!catalogMatch) errors.Add("PRESENTATION_CATALOG_SNAPSHOT_MISMATCH:" + mismatch);
            }
            bool popupV2 = popup != null && PopupMatchesSafeV2(popup);
            bool popupV1 = popup != null && PopupIsApprovedSafeV1(popup);
            if (!popupV1 && !popupV2) errors.Add("SAFE_POPUP_MIGRATION_SOURCE_MISMATCH");
            if (errors.Count > 0)
                return new RandomGrowthPresentationCatalogPreflight(
                    RandomGrowthGenerationDisposition.Blocked, blueprint, errors);
            return new RandomGrowthPresentationCatalogPreflight(
                catalogExists && catalogMatch && popupV2
                    ? RandomGrowthGenerationDisposition.NoOp
                    : RandomGrowthGenerationDisposition.ReadyToGenerate,
                blueprint, Array.Empty<string>());
        }

        public static SafeV2ProductionSnapshot CaptureSafeV2ProductionSnapshot()
        {
            RandomGrowthSafeProjectionSnapshot v1 = RandomGrowthSafeProjectionContract.CreateSnapshot(
                RandomGrowthSafeProjectionContract.SemanticCopyDigest);
            string catalog = RandomGrowthSafeProjectionContract.V2CatalogAssetPath;
            return new SafeV2ProductionSnapshot(
                OptionalFileSha(v1.Paths.PopupEventAssetPath), AssetDatabase.AssetPathToGUID(v1.Paths.PopupEventAssetPath),
                OptionalFileSha(catalog), AssetDatabase.AssetPathToGUID(catalog),
                OptionalFileSha(v1.Paths.RoundNodeAssetPath), OptionalFileSha(v1.Paths.EventPoolAssetPath),
                CaptureOrdinaryPoolLinkSnapshot());
        }

        public static RandomGrowthPresentationCatalogPreflight GenerateAndMigrateSafeV2Production()
        {
            RandomGrowthPresentationCatalogPreflight preflight = PreflightSafeV2Production();
            if (!preflight.CanWrite) return preflight;
            SafeV2ProductionSnapshot before = CaptureSafeV2ProductionSnapshot();
            RandomGrowthSafePresentationCatalogBlueprint blueprint = preflight.Blueprint;
            if (!File.Exists(blueprint.AssetPath))
            {
                string directory = Path.GetDirectoryName(blueprint.AssetPath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                RandomGrowthPresentationCopyAsset asset =
                    ScriptableObject.CreateInstance<RandomGrowthPresentationCopyAsset>();
                PopulateCatalogAsset(asset, blueprint);
                AssetDatabase.CreateAsset(asset, blueprint.AssetPath);
                AssetDatabase.SaveAssets();
            }
            RandomGrowthPresentationCopyAsset saved =
                AssetDatabase.LoadAssetAtPath<RandomGrowthPresentationCopyAsset>(blueprint.AssetPath);
            if (!RandomGrowthPresentationCopyResolver.TryResolve(saved, blueprint.Expectation,
                    out _, out RandomGrowthPresentationCopyMismatch mismatch))
                throw new InvalidOperationException("PRESENTATION_CATALOG_POST_SAVE:" + mismatch);

            RandomGrowthSafeProjectionSnapshot v1 = RandomGrowthSafeProjectionContract.CreateSnapshot(
                RandomGrowthSafeProjectionContract.SemanticCopyDigest);
            PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(v1.Paths.PopupEventAssetPath);
            if (!PopupMatchesSafeV2(popup))
            {
                if (!PopupIsApprovedSafeV1(popup))
                    throw new InvalidOperationException("SAFE_POPUP_MIGRATION_SOURCE_MISMATCH");
                popup.choices[0].executionConfig = RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(false);
                popup.choices[1].executionConfig = RandomGrowthSafeProjectionContract.CreateV2ExecutionConfig(true);
                EditorUtility.SetDirty(popup); AssetDatabase.SaveAssets();
            }
            RandomGrowthPresentationCatalogPreflight result = PreflightSafeV2Production();
            if (result.Disposition != RandomGrowthGenerationDisposition.NoOp)
                throw new InvalidOperationException("SAFE_V2_POST_MIGRATION:" + string.Join(",", result.Errors));
            SafeV2ProductionSnapshot after = CaptureSafeV2ProductionSnapshot();
            if (before.RoundNodeSha != after.RoundNodeSha || before.EventPoolSha != after.EventPoolSha
                || !before.Placement.Equals(after.Placement))
                throw new InvalidOperationException("SAFE_V2_UNRELATED_ASSET_MUTATED");
            return result;
        }

        private static RandomGrowthGeneratedAssetPreflight PreflightBlueprint(
            RandomGrowthProjectionBlueprint blueprint)
        {
            List<string> errors = ValidateImage(blueprint);
            errors.AddRange(ValidateProjectionBlueprint(blueprint));
            errors.AddRange(FindStableIdCollisions(blueprint));
            errors.AddRange(ValidateOrdinaryPoolLinks());
            if (errors.Count > 0)
            {
                return Blocked(blueprint, errors);
            }

            bool[] exists =
            {
                File.Exists(blueprint.Paths.PopupEventAssetPath),
                File.Exists(blueprint.Paths.RoundNodeAssetPath),
                File.Exists(blueprint.Paths.EventPoolAssetPath)
            };

            RandomGrowthGenerationDisposition fileDisposition = EvaluateOutputSet(
                exists[0], exists[1], exists[2], semanticMatch: false);
            if (fileDisposition == RandomGrowthGenerationDisposition.ReadyToGenerate)
            {
                return new RandomGrowthGeneratedAssetPreflight(
                    RandomGrowthGenerationDisposition.ReadyToGenerate,
                    blueprint,
                    Array.Empty<string>());
            }

            if (!exists.All(x => x))
            {
                return Blocked(blueprint, new[] { "PARTIAL_OUTPUT_SET" });
            }

            errors.AddRange(ValidateExistingSnapshot(blueprint));
            return errors.Count == 0
                ? new RandomGrowthGeneratedAssetPreflight(
                    RandomGrowthGenerationDisposition.NoOp,
                    blueprint,
                    Array.Empty<string>())
                : Blocked(blueprint, errors);
        }

        public static RandomGrowthGeneratedAssetPreflight GenerateApprovedNewAssets()
            => GenerateApprovedNewAssets(Preflight);

        public static RandomGrowthGeneratedAssetPreflight GenerateApprovedSafeAssets()
            => GenerateApprovedNewAssets(PreflightSafe);

        private static RandomGrowthGeneratedAssetPreflight GenerateApprovedNewAssets(
            Func<RandomGrowthGeneratedAssetPreflight> preflightFactory)
        {
            OrdinaryPoolLinkSnapshot placementBefore = CaptureOrdinaryPoolLinkSnapshot();
            RandomGrowthGeneratedAssetPreflight preflight = preflightFactory();
            if (!preflight.CanWrite)
            {
                return preflight;
            }

            RandomGrowthProjectionBlueprint b = preflight.Blueprint;
            PopupEventSO popup = BuildPopup(b);
            RoundNodeSO node = BuildRoundNode(b, popup);
            EventPoolSO pool = BuildPool(b, node);

            try
            {
                AssetDatabase.CreateAsset(popup, b.Paths.PopupEventAssetPath);
                AssetDatabase.CreateAsset(node, b.Paths.RoundNodeAssetPath);
                AssetDatabase.CreateAsset(pool, b.Paths.EventPoolAssetPath);
                AssetDatabase.SaveAssets();
            }
            catch
            {
                // 자동 repair/delete는 하지 않는다. 호출자는 실패를 성공으로 처리할 수 없다.
                throw;
            }

            RandomGrowthGeneratedAssetPreflight result = preflightFactory();
            OrdinaryPoolLinkSnapshot placementAfter = CaptureOrdinaryPoolLinkSnapshot();
            if (!placementBefore.Equals(placementAfter))
            {
                throw new InvalidOperationException("ORDINARY_PLACEMENT_RULE_MUTATED");
            }
            if (result.Disposition != RandomGrowthGenerationDisposition.NoOp)
            {
                throw new InvalidOperationException(
                    "Generated asset snapshot verification failed. "
                    + FormatDiagnostic(result));
            }

            return result;
        }

        public static void LogPreflightVerification()
        {
            RandomGrowthGeneratedAssetPreflight result = Preflight();
            Debug.Log("RandomGrowthGeneratedAssetPreflight " + FormatDiagnostic(result));
        }

        private static string FormatDiagnostic(
            RandomGrowthGeneratedAssetPreflight result)
        {
            string errors = result.Errors.Count == 0
                ? "[]"
                : "[" + string.Join(",", result.Errors.OrderBy(x => x, StringComparer.Ordinal)) + "]";
            return $"Disposition={result.Disposition};Errors={errors}";
        }

        public static RandomGrowthProjectionBlueprint CreateBlueprint(
            RandomGrowthContentContractValidator.Result source)
        {
            if (source == null || !source.IsValid)
            {
                throw new ArgumentException("Validated A1 content is required.", nameof(source));
            }

            RandomGrowthContentNode node = source.Content.nodes.Single();
            return new RandomGrowthProjectionBlueprint
            {
                Kind = RandomGrowthGeneratedContentKind.SmithyRisk,
                Paths = source.BuildPlan,
                StageId = source.Content.stageNodeId,
                PopupId = source.Content.nodeId,
                PoolId = source.Pool.poolId,
                ImagePath = source.Content.mainImagePath,
                ImageGuid = AssetDatabase.AssetPathToGUID(source.Content.mainImagePath),
                ExpectedImageGuid = RandomGrowthGeneratedAssetBuilder.ExpectedImageGuid,
                ImageLocalId = ExpectedSpriteLocalId,
                DefinitionFingerprint = source.DefinitionFingerprint,
                DisplayName = source.Pool.displayName,
                Risk = node.choices[0],
                Decline = node.choices[1],
                PoolEntry = source.Pool.entries.Single(),
                ProjectedChoices = new[]
                {
                    BuildChoice(node.choices[0], source.DefinitionFingerprint),
                    BuildChoice(node.choices[1], source.DefinitionFingerprint)
                },
                EntryId = source.Pool.entries.Single().entryId,
                EntryWeight = source.Pool.entries.Single().weight,
                EntryOneShot = source.Pool.entries.Single().oneShot,
                EntryCooldownRounds = source.Pool.entries.Single().cooldownRounds,
                EntryMinDepth = source.Pool.entries.Single().minDepth,
                EntryMaxDepth = source.Pool.entries.Single().maxDepth,
                EntryTags = source.Pool.entries.Single().tags?.ToArray() ?? Array.Empty<string>()
            };
        }

        public static RandomGrowthProjectionBlueprint CreateSafeBlueprint(
            RandomGrowthSafeContentValidation source)
        {
            if (source == null || !source.IsValid)
                throw new ArgumentException("Validated Safe content is required.", nameof(source));
            RandomGrowthSafeProjectionSnapshot snapshot =
                RandomGrowthSafeProjectionContract.CreateSnapshot(
                    RandomGrowthSafeProjectionContract.SemanticCopyDigest);
            RandomGrowthSafePoolEntryContent entry = source.Pool.entries.Single();
            var paths = new RandomGrowthContentBuildPlan
            {
                RoundNodeAssetPath = snapshot.Paths.RoundNodeAssetPath,
                PopupEventAssetPath = snapshot.Paths.PopupEventAssetPath,
                EventPoolAssetPath = snapshot.Paths.EventPoolAssetPath,
                DefinitionFingerprint = snapshot.Paths.DefinitionFingerprint,
                PresentationTextDigestKo = RandomGrowthSafeProjectionContract.SemanticCopyDigest
            };
            return new RandomGrowthProjectionBlueprint
            {
                Kind = RandomGrowthGeneratedContentKind.SafeObservation,
                Paths = paths,
                StageId = source.Content.stageNodeId,
                PopupId = source.Content.nodeId,
                PoolId = source.Pool.poolId,
                ImagePath = SafeImagePath,
                ImageGuid = AssetDatabase.AssetPathToGUID(SafeImagePath),
                ExpectedImageGuid = SafeExpectedImageGuid,
                ImageLocalId = ExpectedSpriteLocalId,
                DefinitionFingerprint = snapshot.Paths.DefinitionFingerprint,
                DisplayName = "Act01 Safe Growth",
                ProjectedChoices = new[]
                {
                    BuildSafeChoice(source.Content.choices[0], snapshot.Safe),
                    BuildSafeChoice(source.Content.choices[1], snapshot.Decline)
                },
                EntryId = entry.entryId,
                EntryWeight = entry.weight,
                EntryOneShot = entry.oneShot,
                EntryCooldownRounds = 0,
                EntryMinDepth = 0,
                EntryMaxDepth = 0,
                EntryTags = entry.tags?.ToArray() ?? Array.Empty<string>()
            };
        }

        public static RandomGrowthGenerationDisposition EvaluateOutputSet(
            bool popupExists,
            bool roundNodeExists,
            bool poolExists,
            bool semanticMatch)
        {
            int count = (popupExists ? 1 : 0) + (roundNodeExists ? 1 : 0) + (poolExists ? 1 : 0);
            if (count == 0)
                return RandomGrowthGenerationDisposition.ReadyToGenerate;
            if (count != 3)
                return RandomGrowthGenerationDisposition.Blocked;
            return semanticMatch
                ? RandomGrowthGenerationDisposition.NoOp
                : RandomGrowthGenerationDisposition.Blocked;
        }

        public static IReadOnlyList<string> EvaluateOrdinaryPoolLinks(
            IEnumerable<string> poolGuids,
            IEnumerable<string> poolIds)
        {
            int guidCount = (poolGuids ?? Array.Empty<string>()).Count(value =>
                string.Equals(value, ManifestOnlyPoolGuid, StringComparison.Ordinal));
            int idCount = (poolIds ?? Array.Empty<string>()).Count(value =>
                string.Equals(value, ManifestOnlyPoolId, StringComparison.Ordinal)
                || string.Equals(value, SafeManifestOnlyPoolId, StringComparison.Ordinal));
            int cardinality = Math.Max(guidCount, idCount);
            return cardinality == 0
                ? Array.Empty<string>()
                : new[] { ManifestOnlyOrdinaryLinkError + ":cardinality=" + cardinality };
        }

        public static OrdinaryPoolLinkSnapshot CaptureOrdinaryPoolLinkSnapshot()
        {
            StagePlacementRuleSO rule = AssetDatabase.LoadAssetAtPath<StagePlacementRuleSO>(
                OrdinaryPlacementRulePath);
            List<StagePlacementPoolEntry> entries = rule?.weightedPool?.pools
                ?? new List<StagePlacementPoolEntry>();
            int linked = entries.Count(entry => entry?.pool != null
                && (string.Equals(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.pool)),
                        ManifestOnlyPoolGuid, StringComparison.Ordinal)
                    || string.Equals(entry.pool.poolId, ManifestOnlyPoolId, StringComparison.Ordinal)
                    || string.Equals(entry.pool.poolId, SafeManifestOnlyPoolId, StringComparison.Ordinal)));

            return new OrdinaryPoolLinkSnapshot(
                FileSha256(OrdinaryPlacementRulePath), entries.Count, linked);
        }

        private static IEnumerable<string> ValidateOrdinaryPoolLinks()
        {
            StagePlacementRuleSO rule = AssetDatabase.LoadAssetAtPath<StagePlacementRuleSO>(
                OrdinaryPlacementRulePath);
            if (rule == null)
            {
                return new[] { "ORDINARY_PLACEMENT_RULE_MISSING" };
            }
            List<StagePlacementPoolEntry> entries = rule.weightedPool?.pools
                ?? new List<StagePlacementPoolEntry>();
            return EvaluateOrdinaryPoolLinks(
                entries.Select(entry => entry?.pool == null ? string.Empty
                    : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(entry.pool))),
                entries.Select(entry => entry?.pool?.poolId ?? string.Empty));
        }

        private static string FileSha256(string path)
        {
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(File.ReadAllBytes(path))
                .Select(value => value.ToString("x2")));
        }

        private static string OptionalFileSha(string path) => File.Exists(path)
            ? FileSha256(path) : string.Empty;

        private static bool PopupIsApprovedSafeV1(PopupEventSO popup)
        {
            if (popup?.choices == null || popup.choices.Count != 2) return false;
            RandomGrowthChoiceExecutionData observe = popup.choices[0]?.executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            RandomGrowthChoiceExecutionData decline = popup.choices[1]?.executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            return observe != null && decline != null
                && observe.schemaVersion == RandomGrowthSafeProjectionContract.SchemaVersion
                && decline.schemaVersion == RandomGrowthSafeProjectionContract.SchemaVersion
                && observe.contentContractVersion == RandomGrowthSafeProjectionContract.ContentContractVersion
                && decline.contentContractVersion == RandomGrowthSafeProjectionContract.ContentContractVersion
                && observe.definitionFingerprint == RandomGrowthSafeProjectionContract.ComputeFingerprint(
                    RandomGrowthSafeProjectionContract.SemanticCopyDigest)
                && decline.definitionFingerprint == observe.definitionFingerprint;
        }

        private static bool PopupMatchesSafeV2(PopupEventSO popup)
        {
            if (popup?.choices == null || popup.choices.Count != 2
                || popup.choices.Any(x => x?.rewards == null || x.rewards.Count != 0)) return false;
            RandomGrowthChoiceExecutionData observe = popup.choices[0].executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            RandomGrowthChoiceExecutionData decline = popup.choices[1].executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            return observe is RandomGrowthSafeExecutionData && decline is RandomGrowthDeclineExecutionData
                && observe.schemaVersion == RandomGrowthSafeProjectionContract.V2SchemaVersion
                && decline.schemaVersion == RandomGrowthSafeProjectionContract.V2SchemaVersion
                && observe.contentContractVersion == RandomGrowthSafeProjectionContract.V2ContentContractVersion
                && decline.contentContractVersion == RandomGrowthSafeProjectionContract.V2ContentContractVersion
                && observe.definitionFingerprint == RandomGrowthSafeProjectionContract.V2DefinitionFingerprint
                && decline.definitionFingerprint == RandomGrowthSafeProjectionContract.V2DefinitionFingerprint
                && observe.presentationTextDigestKo == RandomGrowthSafeProjectionContract.V2SemanticCopyDigest
                && decline.presentationTextDigestKo == RandomGrowthSafeProjectionContract.V2SemanticCopyDigest
                && observe.presentationCatalogId == RandomGrowthSafeProjectionContract.V2CatalogId
                && decline.presentationCatalogId == RandomGrowthSafeProjectionContract.V2CatalogId
                && observe.presentationProjectionKind == RandomGrowthSafeProjectionContract.V2ProjectionKind
                && decline.presentationProjectionKind == RandomGrowthSafeProjectionContract.V2ProjectionKind
                && observe.presentationLocale == RandomGrowthSafeProjectionContract.Locale
                && decline.presentationLocale == RandomGrowthSafeProjectionContract.Locale;
        }

        private static void PopulateCatalogAsset(RandomGrowthPresentationCopyAsset asset,
            RandomGrowthSafePresentationCatalogBlueprint blueprint)
        {
            SerializedObject serialized = new(asset);
            void Set(string name, string value) => serialized.FindProperty(name).stringValue = value;
            serialized.FindProperty("schemaVersion").intValue = blueprint.Expectation.SchemaVersion;
            Set("contentContractVersion", blueprint.Expectation.ContentContractVersion);
            Set("locale", blueprint.Expectation.Locale); Set("catalogId", blueprint.Expectation.CatalogId);
            Set("projectionKind", blueprint.Expectation.ProjectionKind);
            Set("semanticDomain", blueprint.Expectation.SemanticDomain);
            Set("definitionDomain", blueprint.Expectation.DefinitionDomain);
            Set("eventId", blueprint.Expectation.EventId); Set("sourcePopupId", blueprint.Expectation.SourcePopupId);
            Set("semanticCopyDigest", blueprint.Expectation.SemanticCopyDigest);
            Set("definitionFingerprint", blueprint.Expectation.DefinitionFingerprint);
            SerializedProperty fields = serialized.FindProperty("fields");
            fields.arraySize = blueprint.Fields.Count;
            for (int i = 0; i < blueprint.Fields.Count; i++)
            {
                SerializedProperty field = fields.GetArrayElementAtIndex(i);
                field.FindPropertyRelative("name").stringValue = blueprint.Fields[i].Key;
                field.FindPropertyRelative("value").stringValue = blueprint.Fields[i].Value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PopupEventSO BuildPopup(RandomGrowthProjectionBlueprint b)
        {
            var popup = ScriptableObject.CreateInstance<PopupEventSO>();
            popup.eventId = b.PopupId;
            popup.mainImage = AssetDatabase.LoadAssetAtPath<Sprite>(b.ImagePath);
            popup.choices = b.ProjectedChoices.ToList();
            return popup;
        }

        private static PopupEventChoice BuildSafeChoice(
            RandomGrowthSafeChoiceContent source,
            ChoiceExecutionConfig execution)
        {
            return new PopupEventChoice
            {
                choiceId = source.choiceId,
                rewards = new List<PopupEventRewardData>(),
                executionConfig = execution
            };
        }

        private static PopupEventChoice BuildChoice(
            RandomGrowthContentChoice source,
            string definitionFingerprint)
        {
            ChoiceExecutionType type = source.execution.executionType == "RandomGrowthRisk"
                ? ChoiceExecutionType.RandomGrowthRisk
                : ChoiceExecutionType.RandomGrowthDecline;
            ChoiceExecutionConfig config = ChoiceExecutionDataFactory.CreateConfig(type);
            PopulateCommon((RandomGrowthChoiceExecutionData)config.data, source, definitionFingerprint);

            if (config.data is RandomGrowthRiskExecutionData risk)
            {
                RandomGrowthTypedExecution x = source.execution;
                risk.resultKind = x.resultKind;
                risk.successResultKind = x.successResultKind;
                risk.failureState = x.failureState;
                risk.growthGrant = x.growthGrant;
                risk.interactionReservation = new RandomGrowthInteractionReservationData
                {
                    authority = x.interactionReservation.authority,
                    lifetime = x.interactionReservation.lifetime,
                    stableKeyFields = new List<string>(x.interactionReservation.stableKeyFields),
                    orderedStates = new List<string>(x.interactionReservation.orderedStates),
                    locksDecline = x.interactionReservation.locksDecline,
                    blocksDuplicateConfirm = x.interactionReservation.blocksDuplicateConfirm,
                    mutationCountBeforeAtomicTransaction = x.interactionReservation.mutationCountBeforeAtomicTransaction
                };
                risk.costPolicy = new RandomGrowthCostProjectionData
                {
                    type = x.costPolicy.type,
                    rateBasisPoints = x.costPolicy.rateBasisPoints,
                    rounding = x.costPolicy.rounding,
                    minimumRemainingHp = x.costPolicy.minimumRemainingHp
                };
                risk.capPolicy = new RandomGrowthCapProjectionData
                {
                    fixedApplied = x.capPolicy.fixedApplied,
                    randomApplied = x.capPolicy.randomApplied,
                    totalApplied = x.capPolicy.totalApplied
                };
            }
            else if (config.data is RandomGrowthDeclineExecutionData decline)
            {
                decline.resultKind = source.execution.resultKind;
                decline.cost = source.execution.cost;
                decline.growthGrant = source.execution.growthGrant;
            }

            return new PopupEventChoice
            {
                choiceId = source.choiceId,
                rewards = new List<PopupEventRewardData>(),
                executionConfig = config
            };
        }

        private static void PopulateCommon(
            RandomGrowthChoiceExecutionData target,
            RandomGrowthContentChoice source,
            string definitionFingerprint)
        {
            RandomGrowthTypedExecution x = source.execution;
            target.schemaVersion = x.schemaVersion;
            target.contentContractVersion = x.contentContractVersion;
            target.definitionFingerprint = definitionFingerprint;
            target.presentationTextDigestKo =
                RandomGrowthContentContractValidator.ValidateFiles().PresentationTextDigestKo;
            target.eventId = x.eventId;
            target.stageNodeId = x.stageNodeId;
            target.sourcePopupId = source.sourcePopupId;
            target.choiceId = x.choiceId;
            target.segmentId = x.segmentId;
            target.reservationId = x.reservationId;
            target.poolMode = x.poolMode;
        }

        private static RoundNodeSO BuildRoundNode(RandomGrowthProjectionBlueprint b, PopupEventSO popup)
        {
            var node = ScriptableObject.CreateInstance<RoundNodeSO>();
            node.nodeId = b.StageId;
            node.nodeType = RoundNodeType.Event;
            node.popupEvent = popup;
            node.tags = new List<string>(b.EntryTags ?? Array.Empty<string>());
            return node;
        }

        private static EventPoolSO BuildPool(RandomGrowthProjectionBlueprint b, RoundNodeSO node)
        {
            var pool = ScriptableObject.CreateInstance<EventPoolSO>();
            pool.poolId = b.PoolId;
            pool.displayName = b.DisplayName;
            pool.entries = new List<EventPoolEntry>
            {
                new()
                {
                    node = node,
                    entryId = b.EntryId,
                    weight = b.EntryWeight,
                    oneShot = b.EntryOneShot,
                    cooldownRounds = b.EntryCooldownRounds,
                    minDepth = b.EntryMinDepth,
                    maxDepth = b.EntryMaxDepth,
                    tags = new List<string>(b.EntryTags ?? Array.Empty<string>())
                }
            };
            return pool;
        }

        private static List<string> ValidateImage(RandomGrowthProjectionBlueprint b)
        {
            var errors = new List<string>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(b.ImagePath);
            if (sprite == null || b.ImageGuid != b.ExpectedImageGuid)
            {
                errors.Add("IMAGE_REFERENCE_MISMATCH");
                return errors;
            }

            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long localId)
                || guid != b.ExpectedImageGuid || localId != ExpectedSpriteLocalId)
            {
                errors.Add("IMAGE_FILE_ID_MISMATCH");
            }
            return errors;
        }

        private static IEnumerable<string> ValidateProjectionBlueprint(
            RandomGrowthProjectionBlueprint b)
        {
            PopupEventSO popup = BuildPopup(b);
            try
            {
                if (popup.choices == null || popup.choices.Count != 2)
                    return new[] { "PROJECTION_CHOICE_COUNT" };
                if (popup.choices.Any(x => x.rewards == null || x.rewards.Count != 0))
                    return new[] { "PROJECTION_REWARDS_NOT_EMPTY" };
                List<string> errors = popup.choices
                    .SelectMany(x => ChoiceExecutionConfigValidator.Validate(x.executionConfig))
                    .ToList();
                return errors.Select(x => "PROJECTION_EXECUTION:" + x).ToArray();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(popup);
            }
        }

        private static IEnumerable<string> FindStableIdCollisions(RandomGrowthProjectionBlueprint b)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:RoundNodeSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RoundNodeSO asset = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(path);
                if (asset != null && asset.nodeId == b.StageId && path != b.Paths.RoundNodeAssetPath)
                    yield return "DUPLICATE_STAGE_ID:" + path;
            }
            foreach (string guid in AssetDatabase.FindAssets("t:PopupEventSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PopupEventSO asset = AssetDatabase.LoadAssetAtPath<PopupEventSO>(path);
                if (asset != null && asset.eventId == b.PopupId && path != b.Paths.PopupEventAssetPath)
                    yield return "DUPLICATE_POPUP_ID:" + path;
            }
            foreach (string guid in AssetDatabase.FindAssets("t:EventPoolSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EventPoolSO asset = AssetDatabase.LoadAssetAtPath<EventPoolSO>(path);
                if (asset != null && asset.poolId == b.PoolId && path != b.Paths.EventPoolAssetPath)
                    yield return "DUPLICATE_POOL_ID:" + path;
            }
        }

        private static IEnumerable<string> ValidateExistingSnapshot(RandomGrowthProjectionBlueprint b)
        {
            PopupEventSO popup = AssetDatabase.LoadAssetAtPath<PopupEventSO>(b.Paths.PopupEventAssetPath);
            RoundNodeSO node = AssetDatabase.LoadAssetAtPath<RoundNodeSO>(b.Paths.RoundNodeAssetPath);
            EventPoolSO pool = AssetDatabase.LoadAssetAtPath<EventPoolSO>(b.Paths.EventPoolAssetPath);
            if (popup == null || node == null || pool == null) return new[] { "OUTPUT_TYPE_MISMATCH" };
            if (popup.eventId != b.PopupId || node.nodeId != b.StageId || pool.poolId != b.PoolId)
                return new[] { "OUTPUT_ID_MISMATCH" };
            if (node.popupEvent != popup || pool.entries == null || pool.entries.Count != 1 || pool.entries[0].node != node)
                return new[] { "OUTPUT_REFERENCE_MISMATCH" };
            if (popup.choices == null || popup.choices.Count != 2
                || popup.choices.Any(x => x.rewards == null || x.rewards.Count != 0)
                || popup.mainImage == null
                || AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(popup.mainImage)) != b.ExpectedImageGuid
                || pool.entries[0].entryId != b.EntryId
                || pool.entries[0].weight != b.EntryWeight
                || ChoiceExecutionConfigValidator.Validate(popup.choices[0].executionConfig).Count != 0
                || ChoiceExecutionConfigValidator.Validate(popup.choices[1].executionConfig).Count != 0)
                return new[] { "OUTPUT_SNAPSHOT_MISMATCH" };
            RandomGrowthChoiceExecutionData first =
                popup.choices[0].executionConfig?.data as RandomGrowthChoiceExecutionData;
            RandomGrowthChoiceExecutionData second =
                popup.choices[1].executionConfig?.data as RandomGrowthChoiceExecutionData;
            if (first == null || second == null
                || first.definitionFingerprint != b.DefinitionFingerprint
                || second.definitionFingerprint != b.DefinitionFingerprint
                || popup.choices[0].choiceId != b.ProjectedChoices[0].choiceId
                || popup.choices[1].choiceId != b.ProjectedChoices[1].choiceId)
                return new[] { "OUTPUT_SNAPSHOT_MISMATCH" };
            if (b.Kind == RandomGrowthGeneratedContentKind.SmithyRisk
                && first is not RandomGrowthRiskExecutionData)
                return new[] { "OUTPUT_SNAPSHOT_MISMATCH" };
            if (b.Kind == RandomGrowthGeneratedContentKind.SafeObservation)
            {
                RandomGrowthSafeExecutionData safe = first as RandomGrowthSafeExecutionData;
                if (safe == null || safe.targetCount != 2
                    || safe.capPolicy?.optionalGranted != 1
                    || safe.capPolicy.optionalApplied != 1)
                    return new[] { "OUTPUT_SNAPSHOT_MISMATCH" };
            }
            return Array.Empty<string>();
        }

        private static RandomGrowthGeneratedAssetPreflight Blocked(
            RandomGrowthProjectionBlueprint blueprint,
            IEnumerable<string> errors) => new(
                RandomGrowthGenerationDisposition.Blocked,
                blueprint,
                errors);
    }

    public sealed class OrdinaryPoolLinkSnapshot : IEquatable<OrdinaryPoolLinkSnapshot>
    {
        internal OrdinaryPoolLinkSnapshot(string sha256, int poolCardinality, int manifestOnlyLinkCardinality)
        {
            Sha256 = sha256;
            PoolCardinality = poolCardinality;
            ManifestOnlyLinkCardinality = manifestOnlyLinkCardinality;
        }
        public string Sha256 { get; }
        public int PoolCardinality { get; }
        public int ManifestOnlyLinkCardinality { get; }
        public bool Equals(OrdinaryPoolLinkSnapshot other) => other != null
            && Sha256 == other.Sha256
            && PoolCardinality == other.PoolCardinality
            && ManifestOnlyLinkCardinality == other.ManifestOnlyLinkCardinality;
        public override bool Equals(object obj) => Equals(obj as OrdinaryPoolLinkSnapshot);
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = StringComparer.Ordinal.GetHashCode(Sha256 ?? string.Empty);
                hash = (hash * 397) ^ PoolCardinality;
                return (hash * 397) ^ ManifestOnlyLinkCardinality;
            }
        }
    }
}
#endif
