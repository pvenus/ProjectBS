#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Stage;
using UnityEngine;

namespace ResourceTools.Stage
{
    public enum RandomGrowthSafePreflightDisposition
    {
        Blocked = 0,
        ReadyToGenerate = 1,
        NoOp = 2
    }

    public sealed class RandomGrowthSafeBuildPlan
    {
        public string RoundNodeAssetPath { get; internal set; }
        public string PopupEventAssetPath { get; internal set; }
        public string EventPoolAssetPath { get; internal set; }
        public string DefinitionFingerprint { get; internal set; }
    }

    public sealed class RandomGrowthSafeProjectionSnapshot
    {
        internal RandomGrowthSafeProjectionSnapshot(RandomGrowthSafeBuildPlan paths,
            ChoiceExecutionConfig safe, ChoiceExecutionConfig decline)
        {
            Paths = paths; Safe = safe; Decline = decline;
            SafeRewards = Array.AsReadOnly(Array.Empty<string>());
            DeclineRewards = Array.AsReadOnly(Array.Empty<string>());
        }
        public RandomGrowthSafeBuildPlan Paths { get; }
        public ChoiceExecutionConfig Safe { get; }
        public ChoiceExecutionConfig Decline { get; }
        public IReadOnlyList<string> SafeRewards { get; }
        public IReadOnlyList<string> DeclineRewards { get; }
    }

    public sealed class RandomGrowthSafePreflight
    {
        internal RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition disposition,
            RandomGrowthSafeProjectionSnapshot snapshot, IEnumerable<string> errors)
        { Disposition = disposition; Snapshot = snapshot; Errors = errors.ToArray(); }
        public RandomGrowthSafePreflightDisposition Disposition { get; }
        public RandomGrowthSafeProjectionSnapshot Snapshot { get; }
        public IReadOnlyList<string> Errors { get; }
        public bool CanWrite => Disposition == RandomGrowthSafePreflightDisposition.ReadyToGenerate;
    }

    [Serializable] public sealed class RandomGrowthSafeContentDocument
    {
        public int schemaVersion;
        public string documentType, contentContractVersion, locale, manifestVersion, fallbackVersion;
        public string poolId, eventId, stageNodeId, nodeId, sourcePopupId, segmentId, reservationId, poolMode;
        public string presentationCatalogId, presentationProjectionKind, semanticDomain, definitionDomain;
        public string semanticCopyDigest, definitionFingerprint;
        public List<RandomGrowthSafePlacementContent> pairedPlacements = new();
        public List<RandomGrowthSafeCopyFieldContent> semanticCopyKo = new();
        public RandomGrowthSafeCapContent capPolicy;
        public List<RandomGrowthSafeChoiceContent> choices = new();
    }

    [Serializable] public sealed class RandomGrowthSafePlacementContent
    { public string sectionId, slotId; }

    [Serializable] public sealed class RandomGrowthSafeCopyFieldContent
    { public string name, value; }

    [Serializable] public sealed class RandomGrowthSafeCapContent
    { public int fixedApplied, optionalGranted, optionalApplied, totalApplied; }

    [Serializable] public sealed class RandomGrowthSafeChoiceContent
    {
        public string choiceId, sourcePopupId, executionType, selectionResultId, resultId, retryResultId;
        public int executionTypeValue, cost, growthGrant, targetCount, disabledExecutorCallCount;
        public bool isTerminal, battleRequired, candidateZeroBlocksExecutor, candidateZeroAllowsFallback;
        public string poolMode;
        public RandomGrowthSafeCapContent capPolicy;
        public List<string> rewards = new();
    }

    [Serializable] public sealed class RandomGrowthSafePoolDocument
    {
        public int schemaVersion;
        public string documentType, contentContractVersion, poolId, probabilityOwner;
        public List<RandomGrowthSafePoolEntryContent> entries = new();
    }

    [Serializable] public sealed class RandomGrowthSafePoolEntryContent
    {
        public string entryId, nodeJsonPath, nodeId;
        public int weight;
        public bool oneShot;
        public List<string> tags = new();
    }

    public sealed class RandomGrowthSafeContentValidation
    {
        internal RandomGrowthSafeContentValidation(RandomGrowthSafeContentDocument content,
            RandomGrowthSafePoolDocument pool, IEnumerable<string> errors)
        { Content = content; Pool = pool; Errors = errors.ToArray(); }
        public RandomGrowthSafeContentDocument Content { get; }
        public RandomGrowthSafePoolDocument Pool { get; }
        public IReadOnlyList<string> Errors { get; }
        public bool IsValid => Errors.Count == 0;
    }

    public sealed class RandomGrowthSafePresentationCatalogBlueprint
    {
        internal RandomGrowthSafePresentationCatalogBlueprint(string path,
            RandomGrowthPresentationCopyExpectation expectation,
            IReadOnlyList<KeyValuePair<string, string>> fields)
        { AssetPath = path; Expectation = expectation; Fields = fields; }
        public string AssetPath { get; }
        public RandomGrowthPresentationCopyExpectation Expectation { get; }
        public IReadOnlyList<KeyValuePair<string, string>> Fields { get; }
    }

    /// <summary>Safe Growth .02의 write-free typed projection/preflight 계약.</summary>
    public static class RandomGrowthSafeProjectionContract
    {
        public const int SchemaVersion = 1;
        public const string ContentContractVersion = "chapter1-random-growth-safe-content.v1";
        public const string Locale = "ko-KR";
        public const string EventId = "event.act1.random_growth.02.windworn_sword_marks";
        public const string StageId = "stage.act1.random_growth.02.windworn_sword_marks";
        public const string NodeId = "node.act1.random_growth.02.windworn_sword_marks.intro";
        public const string SegmentId = "progress.segment.act1.chapter01.optional_random_growth";
        public const string ReservationId = "reservation.act1.chapter01.random_growth.after_episode02";
        public const string LeftSectionId = "sec_ep_2_to_ep_3_1";
        public const string RightSectionId = "sec_ep_2_to_ep_3_2";
        public const string LeftSlotId = "slot_430_2085";
        public const string RightSlotId = "slot_1370_2085";
        public const string SafeChoiceId = "choice.act1.random_growth.02.windworn_sword_marks.observe_sword_path";
        public const string DeclineChoiceId = "choice.act1.random_growth.02.windworn_sword_marks.leave_training_ground";
        public const string PoolId = "event_pool.act1.random_growth.safe";
        public const string CopyDigestDomain = "chapter1.random-growth-safe.semantic-copy.v1";
        public const string DefinitionDomain = "chapter1.random-growth-safe.definition.v1";
        public const string ManifestVersion = "chapter1.portfolio48.manifest.v1";
        public const string FallbackVersion = "chapter1.portfolio48.fallback.v1";
        public const string EventJsonPath = "Assets/Contents/Stage/json/event/act01/event.act1.random_growth.02.windworn_sword_marks.json";
        public const string PoolJsonPath = "Assets/Contents/Stage/json/event/event_pool.act1.random_growth.safe.json";
        public const string ObserveResultId = "result.act1.random_growth.02.windworn_sword_marks.observe_selected";
        public const string SafeGrantedResultId = "result.act1.random_growth.02.windworn_sword_marks.safe_growth_granted";
        public const string DeclinedResultId = "result.act1.random_growth.02.windworn_sword_marks.declined";
        public const string RetryResultId = "result.act1.random_growth.02.windworn_sword_marks.observe_selected_pending_retry";
        public const int V2SchemaVersion = 2;
        public const string V2ContentContractVersion = "chapter1-random-growth-safe-content.v2";
        public const string V2CopyDigestDomain = "chapter1.random-growth-safe.semantic-copy.v2";
        public const string V2DefinitionDomain = "chapter1.random-growth-safe.definition.v2";
        public const string V2CatalogId = "presentation.catalog.act1.random_growth.02.windworn_sword_marks.ko-KR";
        public const string V2ProjectionKind = "safe-growth-presentation-copy.v2";
        public const string V2CatalogAssetPath = "Assets/Resources/Stage/RandomGrowth/Presentation/event.act1.random_growth.02.windworn_sword_marks.ko-KR.asset";
        public const string V2SemanticCopyDigest = "5b6ab7c72e213c70b0f38e5601263c461bc025fb40689357ae264b07920c9b80";
        public const string V2DefinitionFingerprint = "72acd7c52fdc3aebe4e9c5cedbdb01377c80e1f65512478d25657973b65da6bd";

        private static readonly (string Name, string Value)[] SemanticCopyFields =
        {
            ("discoveryTitle", "바람에 남은 검식"),
            ("discoveryBody", "바람이 억새밭을 눕힐 때마다 흙 위의 오래된 목검 자국이 이어진다.\n한 걸음 물러서자 끊긴 흔적이 하나의 검로로 보인다.\n잠시 머문다면 그 움직임을 일행의 방식으로 익힐 수 있을 듯하다."),
            ("capNotice", "이번 장의 추가 성장은 한 번만 확정할 수 있습니다."),
            ("methodLabel", "성장 방식 · 관찰"),
            ("methodSummary", "전투와 비용 없이 흔적을 관찰합니다."),
            ("rewardSummary", "성장 정비 · 무작위 후보 최대 2개"),
            ("observeLabel", "흔적을 따라 검식을 살핀다"),
            ("observeAssist", "비용 없이 관찰을 마치면 성장 정비가 열립니다."),
            ("declineLabel", "억새밭을 지나간다"),
            ("declineAssist", "성장 없음 · 이번 장의 추가 성장은 아직 가능합니다."),
            ("confirmTitle", "이 검식을 익히시겠습니까?"),
            ("confirmBody", "비용 없이 관찰을 마치면 성장 정비가 열립니다.\n무작위 후보는 최대 2개이며, 그중 하나를 선택합니다."),
            ("confirmCta", "검식을 익힌다"),
            ("confirmCancel", "다시 살펴본다"),
            ("candidateZeroDisabled", "지금은 익힐 수 있는 검식이 없습니다."),
            ("candidateZeroReason", "강화 가능한 스킬이 생긴 뒤 다시 확인할 수 있습니다."),
            ("candidateZeroRecheckCta", "다시 확인"),
            ("alreadyGrantedDisabled", "이번 장의 추가 성장은 이미 확정되었습니다."),
            ("successBody", "바람이 다시 억새를 눕히자 끊어진 검흔이 하나의 흐름으로 이어졌다.\n일행은 남겨진 동작을 그대로 흉내 내지 않고, 각자의 전투 방식에 맞춰 새 호흡으로 다듬었다."),
            ("successStatus", "성장 기회가 준비되었습니다."),
            ("successCta", "성장 정비로 이동"),
            ("declineBody", "일행은 흙 위의 검흔을 흐트러뜨리지 않고 억새밭을 빠져나왔다.\n뒤에서 바람이 한 번 더 길을 그렸지만, 이번에는 걸음을 멈추지 않았다."),
            ("declineStatus", "성장 없음 · 이번 장의 추가 성장은 아직 가능합니다."),
            ("declineCta", "길을 계속 간다"),
            ("failureBody", "관찰의 결과를 확정하지 못했습니다.\n성장 기회는 생기지 않았으며, 추가 성장도 소비되지 않았습니다."),
            ("failureAssist", "같은 선택으로 다시 시도할 수 있습니다."),
            ("failureRetryCta", "다시 시도"),
            ("reminderTemplate", "{ownerName}이(가) 바람에 남은 검식을 다듬었습니다 · {skillName} Lv {levelBefore}→{levelAfter}")
        };

        private static readonly (string Name, string Value)[] V2SemanticCopyFields =
            SemanticCopyFields.Concat(new[]
            {
                ("candidateTwoStatus", "무작위 성장 후보 2개를 확인합니다."),
                ("candidateOneStatus", "현재 확인 가능한 무작위 성장 후보는 1개입니다."),
                ("busyStatus", "관찰의 결과를 확정하고 있습니다.")
            }).ToArray();

        public static string SemanticCopyDigest => ComputeSemanticCopyDigest();

        public static RandomGrowthSafePresentationCatalogBlueprint CreateV2CatalogBlueprint()
        {
            string digest = ComputeV2SemanticCopyDigest();
            string fingerprint = ComputeV2Fingerprint(digest);
            if (digest != V2SemanticCopyDigest || fingerprint != V2DefinitionFingerprint)
                throw new InvalidOperationException("SAFE_V2_GOLDEN_MISMATCH");
            string[] names = V2SemanticCopyFields.Select(x => x.Name).ToArray();
            var expected = new RandomGrowthPresentationCopyExpectation(V2SchemaVersion,
                V2ContentContractVersion, Locale, V2CatalogId, V2ProjectionKind,
                V2CopyDigestDomain, V2DefinitionDomain, EventId, NodeId,
                digest, fingerprint, names);
            return new RandomGrowthSafePresentationCatalogBlueprint(V2CatalogAssetPath,
                expected, Array.AsReadOnly(V2SemanticCopyFields.Select(x =>
                    new KeyValuePair<string, string>(x.Name, x.Value)).ToArray()));
        }

        public static string ComputeV2SemanticCopyDigest()
        {
            List<string> tuple = new();
            foreach ((string name, string value) in V2SemanticCopyFields)
            { tuple.Add(name); tuple.Add(value); }
            return RandomGrowthPresentationCopyResolver.ComputeDigest(V2CopyDigestDomain, tuple);
        }

        public static string ComputeV2Fingerprint(string semanticCopyDigest)
        {
            if (semanticCopyDigest != V2SemanticCopyDigest)
                throw new ArgumentException("Approved v2 digest is required.", nameof(semanticCopyDigest));
            string[] fields =
            {
                V2DefinitionDomain, V2SchemaVersion.ToString(), V2ContentContractVersion, Locale,
                V2CatalogId, V2ProjectionKind, V2CopyDigestDomain, semanticCopyDigest,
                EventId, StageId, NodeId, SegmentId, ReservationId,
                LeftSectionId, RightSectionId, LeftSlotId, RightSlotId, PoolId,
                SafeChoiceId, "RandomGrowthSafe", "ObserveSelected", "SafeGrowthGranted",
                "ObserveSelectedPendingRetry", "PartyWide", "targetCount=2", "cost=None/0",
                "battle=false", "caps=2/1/1/3", "rewards=[]",
                DeclineChoiceId, "RandomGrowthDecline", "Declined", "cost=0", "grant=0",
                "optionalClaimPreserved=true", "rewards=[]", ManifestVersion, FallbackVersion
            };
            using SHA256 sha = SHA256.Create();
            List<byte> bytes = new();
            foreach (string field in fields) AppendLengthPrefixed(bytes, field);
            return string.Concat(sha.ComputeHash(bytes.ToArray()).Select(x => x.ToString("x2")));
        }

        public static ChoiceExecutionConfig CreateV2ExecutionConfig(bool decline)
        {
            RandomGrowthSafePresentationCatalogBlueprint catalog = CreateV2CatalogBlueprint();
            ChoiceExecutionConfig config = ChoiceExecutionDataFactory.CreateConfig(decline
                ? ChoiceExecutionType.RandomGrowthDecline : ChoiceExecutionType.RandomGrowthSafe);
            RandomGrowthChoiceExecutionData data = (RandomGrowthChoiceExecutionData)config.data;
            PopulateCommon(data, decline ? DeclineChoiceId : SafeChoiceId,
                catalog.Expectation.DefinitionFingerprint, catalog.Expectation.SemanticCopyDigest);
            data.schemaVersion = V2SchemaVersion;
            data.contentContractVersion = V2ContentContractVersion;
            data.presentationCatalogId = V2CatalogId;
            data.presentationProjectionKind = V2ProjectionKind;
            data.presentationLocale = Locale;
            if (decline)
            {
                RandomGrowthDeclineExecutionData x = (RandomGrowthDeclineExecutionData)data;
                x.resultKind = "Declined"; x.cost = 0; x.growthGrant = 0;
            }
            else
            {
                RandomGrowthSafeExecutionData x = (RandomGrowthSafeExecutionData)data;
                x.resultKind = "ObserveSelected"; x.successResultKind = "SafeGrowthGranted";
                x.failureState = "ObserveSelectedPendingRetry";
                x.candidateUnavailableState = "CandidateUnavailable";
                x.capPolicy = new RandomGrowthCapProjectionData
                { fixedApplied = 2, optionalGranted = 1, optionalApplied = 1, totalApplied = 3 };
                x.cost = 0; x.growthGrant = 1; x.candidateZeroBlocksExecutor = true;
                x.candidateZeroAllowsFallback = true; x.mutationCountBeforeConfirm = 0;
            }
            return config;
        }

        public static RandomGrowthSafeProjectionSnapshot CreateSnapshot(string semanticCopyDigest)
        {
            if (!string.Equals(semanticCopyDigest, SemanticCopyDigest, StringComparison.Ordinal))
                throw new ArgumentException("Approved semantic copy digest is required.", nameof(semanticCopyDigest));
            string fingerprint = ComputeFingerprint(semanticCopyDigest);
            ChoiceExecutionConfig safeConfig = ChoiceExecutionDataFactory.CreateConfig(
                ChoiceExecutionType.RandomGrowthSafe);
            PopulateCommon((RandomGrowthChoiceExecutionData)safeConfig.data, SafeChoiceId,
                fingerprint, semanticCopyDigest);
            RandomGrowthSafeExecutionData safe = (RandomGrowthSafeExecutionData)safeConfig.data;
            safe.resultKind = "ObserveSelected";
            safe.successResultKind = "SafeGrowthGranted";
            safe.failureState = "ObserveSelectedPendingRetry";
            safe.candidateUnavailableState = "CandidateUnavailable";
            safe.cost = 0;
            safe.growthGrant = 1;
            safe.candidateZeroBlocksExecutor = true;
            safe.candidateZeroAllowsFallback = true;
            safe.mutationCountBeforeConfirm = 0;
            safe.capPolicy = new RandomGrowthCapProjectionData
            { fixedApplied = 2, optionalGranted = 1, optionalApplied = 1, totalApplied = 3 };

            ChoiceExecutionConfig declineConfig = ChoiceExecutionDataFactory.CreateConfig(
                ChoiceExecutionType.RandomGrowthDecline);
            PopulateCommon((RandomGrowthChoiceExecutionData)declineConfig.data, DeclineChoiceId,
                fingerprint, semanticCopyDigest);
            RandomGrowthDeclineExecutionData decline = (RandomGrowthDeclineExecutionData)declineConfig.data;
            decline.resultKind = "Declined";
            decline.cost = 0;
            decline.growthGrant = 0;

            return new RandomGrowthSafeProjectionSnapshot(new RandomGrowthSafeBuildPlan
            {
                RoundNodeAssetPath = $"Assets/Contents/Stage/so/{StageId}.asset",
                PopupEventAssetPath = $"Assets/Contents/Stage/so/{NodeId}.asset",
                EventPoolAssetPath = $"Assets/Contents/Stage/so/{PoolId}.asset",
                DefinitionFingerprint = fingerprint
            }, safeConfig, declineConfig);
        }

        public static RandomGrowthSafePreflight Preflight(string semanticCopyDigest, bool popupExists,
            bool nodeExists, bool poolExists, bool semanticMatch,
            IEnumerable<string> stableIdCollisions = null)
        {
            RandomGrowthSafeProjectionSnapshot snapshot;
            try { snapshot = CreateSnapshot(semanticCopyDigest); }
            catch (ArgumentException)
            {
                return new RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition.Blocked,
                    null, new[] { "SAFE_SEMANTIC_COPY_DIGEST_INVALID" });
            }
            List<string> errors = (stableIdCollisions ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => "SAFE_STABLE_ID_COLLISION:" + x).ToList();
            errors.AddRange(ChoiceExecutionConfigValidator.Validate(snapshot.Safe));
            errors.AddRange(ChoiceExecutionConfigValidator.Validate(snapshot.Decline));
            if (errors.Count > 0)
                return new RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition.Blocked, snapshot, errors);
            int count = (popupExists ? 1 : 0) + (nodeExists ? 1 : 0) + (poolExists ? 1 : 0);
            if (count == 0)
                return new RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition.ReadyToGenerate,
                    snapshot, Array.Empty<string>());
            if (count != 3)
                return new RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition.Blocked,
                    snapshot, new[] { "SAFE_PARTIAL_OUTPUT_SET" });
            return semanticMatch
                ? new RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition.NoOp,
                    snapshot, Array.Empty<string>())
                : new RandomGrowthSafePreflight(RandomGrowthSafePreflightDisposition.Blocked,
                    snapshot, new[] { "SAFE_SNAPSHOT_MISMATCH" });
        }

        public static string ComputeFingerprint(string semanticCopyDigest)
        {
            if (!string.Equals(semanticCopyDigest, SemanticCopyDigest, StringComparison.Ordinal))
                throw new ArgumentException("Approved semantic copy digest is required.", nameof(semanticCopyDigest));
            string[] fields =
            {
                DefinitionDomain, SchemaVersion.ToString(), ContentContractVersion, Locale,
                CopyDigestDomain, semanticCopyDigest, EventId, StageId, NodeId, SegmentId, ReservationId,
                LeftSectionId, RightSectionId, LeftSlotId, RightSlotId, PoolId,
                SafeChoiceId, "RandomGrowthSafe", "ObserveSelected", "SafeGrowthGranted",
                "ObserveSelectedPendingRetry", "PartyWide", "targetCount=2", "cost=None/0",
                "battle=false", "caps=2/1/1/3", "rewards=[]",
                DeclineChoiceId, "RandomGrowthDecline", "Declined", "cost=0", "grant=0",
                "optionalClaimPreserved=true", "rewards=[]", ManifestVersion, FallbackVersion
            };
            using SHA256 sha = SHA256.Create();
            List<byte> bytes = new();
            foreach (string field in fields)
                AppendLengthPrefixed(bytes, field);
            return string.Concat(sha.ComputeHash(bytes.ToArray()).Select(x => x.ToString("x2")));
        }

        public static string ComputeSemanticCopyDigest()
        {
            using SHA256 sha = SHA256.Create();
            List<byte> bytes = new();
            AppendLengthPrefixed(bytes, CopyDigestDomain);
            foreach ((string name, string value) in SemanticCopyFields)
            {
                AppendLengthPrefixed(bytes, name);
                AppendLengthPrefixed(bytes, value.Normalize(NormalizationForm.FormC)
                    .Replace("\r\n", "\n").Replace("\r", "\n"));
            }
            return string.Concat(sha.ComputeHash(bytes.ToArray()).Select(x => x.ToString("x2")));
        }

        public static IReadOnlyList<KeyValuePair<string, string>> GetSemanticCopyFields() =>
            Array.AsReadOnly(SemanticCopyFields.Select(x =>
                new KeyValuePair<string, string>(x.Name, x.Value)).ToArray());

        public static RandomGrowthSafeContentValidation ValidateProductionFiles(
            string eventPath = EventJsonPath, string poolPath = PoolJsonPath)
        {
            if (!File.Exists(eventPath) || !File.Exists(poolPath))
                return new RandomGrowthSafeContentValidation(null, null,
                    new[] { "SAFE_CONTENT_FILE_MISSING" });
            return ValidateProductionJson(File.ReadAllText(eventPath), File.ReadAllText(poolPath));
        }

        public static RandomGrowthSafeContentValidation ValidateProductionJson(string eventJson, string poolJson)
        {
            List<string> errors = new();
            RandomGrowthSafeContentDocument content = Parse<RandomGrowthSafeContentDocument>(
                eventJson, "SAFE_EVENT_JSON_INVALID", errors);
            RandomGrowthSafePoolDocument pool = Parse<RandomGrowthSafePoolDocument>(
                poolJson, "SAFE_POOL_JSON_INVALID", errors);
            if (content != null) ValidateProductionContent(content, errors);
            if (pool != null) ValidateProductionPool(pool, errors);
            return new RandomGrowthSafeContentValidation(content, pool, errors);
        }

        public static RandomGrowthSafeContentValidation ValidateProductionV2Files()
        {
            if (!File.Exists(EventJsonPath) || !File.Exists(PoolJsonPath))
                return new RandomGrowthSafeContentValidation(null, null,
                    new[] { "SAFE_V2_SOURCE_FILE_MISSING" });
            return ValidateProductionV2Json(File.ReadAllText(EventJsonPath, Encoding.UTF8),
                File.ReadAllText(PoolJsonPath, Encoding.UTF8));
        }

        public static RandomGrowthSafeContentValidation ValidateProductionV2Json(
            string eventJson, string poolJson)
        {
            List<string> errors = new();
            RandomGrowthSafeContentDocument content = Parse<RandomGrowthSafeContentDocument>(
                eventJson, "SAFE_V2_EVENT_JSON_INVALID", errors);
            RandomGrowthSafePoolDocument pool = Parse<RandomGrowthSafePoolDocument>(
                poolJson, "SAFE_V2_POOL_JSON_INVALID", errors);
            if (content != null) ValidateProductionV2Content(content, errors);
            if (pool != null) ValidateProductionV2Pool(pool, errors);
            return new RandomGrowthSafeContentValidation(content, pool, errors);
        }

        private static void ValidateProductionV2Content(
            RandomGrowthSafeContentDocument c, List<string> errors)
        {
            Require(c.schemaVersion == V2SchemaVersion && c.documentType == "randomGrowthSafeEvent"
                && c.contentContractVersion == V2ContentContractVersion && c.locale == Locale,
                "SAFE_V2_CONTENT_HEADER", errors);
            Require(c.manifestVersion == ManifestVersion && c.fallbackVersion == FallbackVersion,
                "SAFE_V2_CONTENT_VERSIONS", errors);
            Require(c.poolId == PoolId && c.eventId == EventId && c.stageNodeId == StageId
                && c.nodeId == NodeId && c.sourcePopupId == NodeId && c.segmentId == SegmentId
                && c.reservationId == ReservationId && c.poolMode == "PartyWide",
                "SAFE_V2_EXACT_IDS", errors);
            Require(c.presentationCatalogId == V2CatalogId
                && c.presentationProjectionKind == V2ProjectionKind
                && c.semanticDomain == V2CopyDigestDomain
                && c.definitionDomain == V2DefinitionDomain,
                "SAFE_V2_CATALOG_IDENTITY", errors);
            Require(c.semanticCopyDigest == V2SemanticCopyDigest
                && c.definitionFingerprint == V2DefinitionFingerprint,
                "SAFE_V2_GOLDEN", errors);
            Require(c.pairedPlacements != null && c.pairedPlacements.Count == 2
                && c.pairedPlacements[0].sectionId == LeftSectionId
                && c.pairedPlacements[0].slotId == LeftSlotId
                && c.pairedPlacements[1].sectionId == RightSectionId
                && c.pairedPlacements[1].slotId == RightSlotId,
                "SAFE_V2_PAIRED_PLACEMENT", errors);
            Require(c.semanticCopyKo != null && c.semanticCopyKo.Count == V2SemanticCopyFields.Length
                && c.semanticCopyKo.Select(x => (x.name, x.value)).SequenceEqual(V2SemanticCopyFields),
                "SAFE_V2_CANONICAL_COPY31", errors);
            Require(ComputeV2SemanticCopyDigest() == c.semanticCopyDigest
                && ComputeV2Fingerprint(c.semanticCopyDigest) == c.definitionFingerprint,
                "SAFE_V2_RECOMPUTED_GOLDEN", errors);
            Require(CapValid(c.capPolicy), "SAFE_V2_CAP_POLICY", errors);
            Require(c.choices != null && c.choices.Count == 2, "SAFE_V2_CHOICE_COUNT", errors);
            if (c.choices == null || c.choices.Count != 2) return;
            ValidateObserve(c.choices[0], errors); ValidateDecline(c.choices[1], errors);
        }

        private static void ValidateProductionV2Pool(
            RandomGrowthSafePoolDocument p, List<string> errors)
        {
            Require(p.schemaVersion == V2SchemaVersion && p.documentType == "randomGrowthSafeEventPool"
                && p.contentContractVersion == V2ContentContractVersion && p.poolId == PoolId
                && p.probabilityOwner == "chapterPortfolioManifest",
                "SAFE_V2_POOL_HEADER", errors);
            Require(p.entries != null && p.entries.Count == 1, "SAFE_V2_POOL_ENTRY_COUNT", errors);
            if (p.entries == null || p.entries.Count != 1) return;
            RandomGrowthSafePoolEntryContent x = p.entries[0];
            Require(x.entryId == StageId && x.nodeJsonPath == EventJsonPath && x.nodeId == StageId
                && x.weight == 1 && x.oneShot, "SAFE_V2_POOL_ENTRY", errors);
        }

        private static void ValidateProductionContent(RandomGrowthSafeContentDocument c, List<string> errors)
        {
            Require(c.schemaVersion == SchemaVersion && c.documentType == "randomGrowthSafeEvent"
                && c.contentContractVersion == ContentContractVersion && c.locale == Locale,
                "SAFE_CONTENT_HEADER", errors);
            Require(c.manifestVersion == ManifestVersion && c.fallbackVersion == FallbackVersion,
                "SAFE_CONTENT_VERSIONS", errors);
            Require(c.poolId == PoolId && c.eventId == EventId && c.stageNodeId == StageId
                && c.nodeId == NodeId && c.sourcePopupId == NodeId && c.segmentId == SegmentId
                && c.reservationId == ReservationId && c.poolMode == "PartyWide",
                "SAFE_CONTENT_EXACT_IDS", errors);
            Require(c.semanticCopyDigest == SemanticCopyDigest
                && c.definitionFingerprint == ComputeFingerprint(SemanticCopyDigest),
                "SAFE_CONTENT_GOLDEN", errors);
            Require(c.pairedPlacements != null && c.pairedPlacements.Count == 2
                && c.pairedPlacements[0].sectionId == LeftSectionId
                && c.pairedPlacements[0].slotId == LeftSlotId
                && c.pairedPlacements[1].sectionId == RightSectionId
                && c.pairedPlacements[1].slotId == RightSlotId,
                "SAFE_PAIRED_PLACEMENT", errors);
            Require(c.semanticCopyKo != null && c.semanticCopyKo.Count == SemanticCopyFields.Length
                && c.semanticCopyKo.Select(x => (x.name, x.value)).SequenceEqual(SemanticCopyFields),
                "SAFE_CANONICAL_COPY", errors);
            Require(CapValid(c.capPolicy), "SAFE_CAP_POLICY", errors);
            Require(c.choices != null && c.choices.Count == 2, "SAFE_CHOICE_COUNT", errors);
            if (c.choices == null || c.choices.Count != 2) return;
            ValidateObserve(c.choices[0], errors);
            ValidateDecline(c.choices[1], errors);
        }

        private static void ValidateObserve(RandomGrowthSafeChoiceContent c, List<string> errors)
        {
            Require(c.choiceId == SafeChoiceId && c.sourcePopupId == NodeId
                && c.executionType == "RandomGrowthSafe" && c.executionTypeValue == 1020
                && c.selectionResultId == ObserveResultId && c.resultId == SafeGrantedResultId
                && c.retryResultId == RetryResultId,
                "SAFE_OBSERVE_TYPED_ID", errors);
            Require(!c.isTerminal && !c.battleRequired && c.cost == 0 && c.growthGrant == 1
                && c.poolMode == "PartyWide" && c.targetCount == 2
                && c.disabledExecutorCallCount == 0 && c.candidateZeroBlocksExecutor
                && c.candidateZeroAllowsFallback && CapValid(c.capPolicy),
                "SAFE_OBSERVE_PAYLOAD", errors);
            Require(c.rewards != null && c.rewards.Count == 0, "SAFE_OBSERVE_REWARDS", errors);
        }

        private static void ValidateDecline(RandomGrowthSafeChoiceContent c, List<string> errors)
        {
            Require(c.choiceId == DeclineChoiceId && c.sourcePopupId == NodeId
                && c.executionType == "RandomGrowthDecline" && c.executionTypeValue == 1010
                && string.IsNullOrEmpty(c.selectionResultId) && c.resultId == DeclinedResultId
                && string.IsNullOrEmpty(c.retryResultId),
                "SAFE_DECLINE_TYPED_ID", errors);
            Require(c.isTerminal && !c.battleRequired && c.cost == 0 && c.growthGrant == 0
                && c.targetCount == 0 && c.disabledExecutorCallCount == 0,
                "SAFE_DECLINE_ZERO_ZERO", errors);
            Require(c.rewards != null && c.rewards.Count == 0, "SAFE_DECLINE_REWARDS", errors);
        }

        private static void ValidateProductionPool(RandomGrowthSafePoolDocument p, List<string> errors)
        {
            Require(p.schemaVersion == SchemaVersion && p.documentType == "randomGrowthSafeEventPool"
                && p.contentContractVersion == ContentContractVersion && p.poolId == PoolId
                && p.probabilityOwner == "chapterPortfolioManifest",
                "SAFE_POOL_HEADER", errors);
            Require(p.entries != null && p.entries.Count == 1, "SAFE_POOL_ENTRY_COUNT", errors);
            if (p.entries == null || p.entries.Count != 1) return;
            RandomGrowthSafePoolEntryContent x = p.entries[0];
            Require(x.entryId == StageId && x.nodeJsonPath == EventJsonPath && x.nodeId == StageId
                && x.weight == 1 && x.oneShot,
                "SAFE_POOL_ENTRY", errors);
        }

        private static T Parse<T>(string json, string code, List<string> errors) where T : class
        {
            try
            {
                T value = string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<T>(json);
                if (value == null) errors.Add(code);
                return value;
            }
            catch { errors.Add(code); return null; }
        }

        private static bool CapValid(RandomGrowthSafeCapContent c) => c != null
            && c.fixedApplied == 2 && c.optionalGranted == 1
            && c.optionalApplied == 1 && c.totalApplied == 3;

        private static void Require(bool condition, string code, List<string> errors)
        { if (!condition) errors.Add(code); }

        private static void AppendLengthPrefixed(List<byte> bytes, string field)
        {
            byte[] value = Encoding.UTF8.GetBytes(field);
            bytes.Add((byte)(value.Length >> 24));
            bytes.Add((byte)(value.Length >> 16));
            bytes.Add((byte)(value.Length >> 8));
            bytes.Add((byte)value.Length);
            bytes.AddRange(value);
        }

        private static void PopulateCommon(RandomGrowthChoiceExecutionData data, string choiceId,
            string fingerprint, string semanticCopyDigest)
        {
            data.schemaVersion = SchemaVersion;
            data.contentContractVersion = ContentContractVersion;
            data.definitionFingerprint = fingerprint;
            data.presentationTextDigestKo = semanticCopyDigest;
            data.eventId = EventId;
            data.stageNodeId = StageId;
            data.sourcePopupId = NodeId;
            data.choiceId = choiceId;
            data.segmentId = SegmentId;
            data.reservationId = ReservationId;
            data.poolMode = "PartyWide";
            data.targetCount = 2;
        }

    }
}
#endif
