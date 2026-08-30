#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ResourceTools.Stage
{
    public static class RandomGrowthContentContractValidator
    {
        public const string EventJsonPath = "Assets/Contents/Stage/json/event/act01/event.act1.random_growth.01.crying_bell_smithy_trial.json";
        public const string PoolJsonPath = "Assets/Contents/Stage/json/event/event_act1_random_growth_cheongun_sangui_pool.json";
        public const string ImagePath = "Assets/ImagesGenerated/Stage/popup_main/node.act1.random_growth.01.crying_bell_smithy_trial.intro.main.png";
        public const string ImageSha256 = "2733405c92fad4a474acffd6a765c0d039525d8090f412200a186a1e2b9bc3c8";
        public const string PoolId = "event_pool.act1.random_growth.cheongun_sangui";
        public const string EventId = "event.act1.random_growth.01.crying_bell_smithy_trial";
        public const string StageId = "stage.act1.random_growth.01.crying_bell_smithy_trial";
        public const string NodeId = "node.act1.random_growth.01.crying_bell_smithy_trial.intro";
        public const string RiskChoiceId = "choice.act1.random_growth.01.crying_bell_smithy_trial.take_heated_talisman";
        public const string DeclineChoiceId = "choice.act1.random_growth.01.crying_bell_smithy_trial.leave_forge";
        public const string SegmentId = "progress.segment.act1.chapter01.random_before_episode06";
        public const string ReservationId = "reservation.act1.chapter01.random_growth.before_episode06";
        public const string ContentVersion = "crying_bell_smithy_trial.v1";
        public const string GeneratorVersion = "chapter1.random_growth_manifest.v1";
        public const string LeftSection = "sec_ep_5_1_to_ep_6", RightSection = "sec_ep_5_2_to_ep_6";

        private static readonly string[] TextNames = { "title", "discoveryBody", "riskLabel", "riskCostLine",
            "riskEligibilityLine", "riskRewardLine", "declineLabel", "declineHelper", "confirmTitle", "confirmBody",
            "confirmCta", "confirmCancelCta", "disabledInsufficientParty", "disabledInsufficientMemberTemplate",
            "disabledNoCandidate", "disabledCapReached", "disabledTechnical", "revalidateCta", "riskSuccessBody",
            "riskSuccessStatus", "riskSuccessCta", "declineResultBody", "declineResultStatus", "declineResultCta",
            "transactionFailureBody", "transactionFailureHelper", "transactionRetryCta", "growthAppliedFollowupTemplate" };

        private static readonly string[] CanonicalText = {
            "우는 쇠종의 시련",
            "사람 없는 대장간에서 쇠종이 낮게 흐느낀다.\n화덕 속 안전패는 아직 붉고, 집게를 대는 순간 불길이 일행 모두를 덮칠 듯하다.\n그러나 쇳소리 사이에는 전투의 호흡과 닮은 박자가 숨어 있다.",
            "달아오른 안전패를 꺼낸다", "파티 전원: 최대 HP 10% 피해 (올림)",
            "전액을 지불한 뒤 모두 HP 1 이상 남아야 합니다.", "성공: 성장 정비 1회", "대장간을 떠난다",
            "피해 없음 · 성장 없음", "시련을 감수하시겠습니까?",
            "표시된 HP 비용이 파티원 모두에게 적용됩니다.\n이후 무작위 성장 후보 중 하나를 선택합니다.",
            "시련을 감수한다", "다시 생각한다", "선택 불가 — 비용을 감당할 수 없는 파티원이 있습니다.",
            "필요 HP {cost} · 현재 HP {currentDisplay} · 지불 후 생존 불가", "선택 불가 — 강화 가능한 스킬이 없습니다.",
            "선택 불가 — 이번 장의 추가 성장을 모두 마쳤습니다.", "현재 파티 상태를 확인할 수 없습니다. 잠시 후 다시 시도해 주세요.",
            "다시 확인", "집게가 안전패를 끌어내는 순간 불길이 치솟았다.\n화상을 견딘 일행은 쇠종과 망치가 맞물리는 박자에서\n전투의 새로운 호흡을 읽어 냈다.",
            "성장 기회가 준비되었습니다.", "성장 정비로 이동", "일행은 달아오른 안전패에서 손을 뗐다.\n뒤로 물러서자 쇠종은 한 번 길게 울었고,\n대장간은 다시 불길 속에 잠겼다.",
            "피해 없음 · 성장 없음", "길을 계속 간다", "시련의 결과를 확정하지 못했습니다.\n피해와 성장 기회는 적용되지 않았습니다.",
            "같은 선택으로 다시 시도할 수 있습니다.", "다시 시도", "{characterName}는 {skillName}을 불길과 쇳소리 속에서 익힌 호흡으로 가다듬었다." };

        public sealed class Result
        {
            internal Result(RandomGrowthContentDocument c, RandomGrowthPoolDocument p, IEnumerable<string> e, string d, string f, RandomGrowthContentBuildPlan b)
            { Content = c; Pool = p; Errors = e.ToArray(); PresentationTextDigestKo = d; DefinitionFingerprint = f; BuildPlan = b; }
            public RandomGrowthContentDocument Content { get; } public RandomGrowthPoolDocument Pool { get; }
            public IReadOnlyList<string> Errors { get; } public bool IsValid => Errors.Count == 0;
            public string PresentationTextDigestKo { get; } public string DefinitionFingerprint { get; }
            public RandomGrowthContentBuildPlan BuildPlan { get; }
        }

        public static Result ValidateFiles(string eventPath = EventJsonPath, string poolPath = PoolJsonPath)
        {
            if (!File.Exists(eventPath) || !File.Exists(poolPath)) return Empty("CONTENT_FILE_MISSING");
            return ValidateJson(File.ReadAllText(eventPath), File.ReadAllText(poolPath));
        }

        public static Result ValidateJson(string eventJson, string poolJson)
        {
            List<string> errors = new();
            var content = Parse<RandomGrowthContentDocument>(eventJson, "EVENT_JSON_INVALID", errors);
            var pool = Parse<RandomGrowthPoolDocument>(poolJson, "POOL_JSON_INVALID", errors);
            if (content != null) ValidateContent(content, errors);
            if (pool != null) ValidatePool(pool, errors);
            string digest = content?.presentationKo == null ? string.Empty : ComputePresentationTextDigestKo(content.presentationKo);
            if (content != null) Require(content.presentationTextDigestKo == digest, "PRESENTATION_DIGEST", errors);
            string fingerprint = errors.Count == 0 ? ComputeDefinitionFingerprint(content, pool, digest) : string.Empty;
            var plan = errors.Count == 0 ? CreateBuildPlan(fingerprint, digest) : null;
            return new Result(content, pool, errors, digest, fingerprint, plan);
        }

        public static string ComputePresentationTextDigestKo(RandomGrowthPresentationTextKo text)
        {
            string[] values = TextValues(text);
            using var stream = new MemoryStream();
            WriteField(stream, "ProjectBS.RandomGrowthText.ko-KR.v1");
            for (int i = 0; i < TextNames.Length; i++) { WriteField(stream, TextNames[i]); WriteField(stream, Normalize(values[i])); }
            return Sha(stream.ToArray());
        }

        private static void ValidateContent(RandomGrowthContentDocument c, List<string> e)
        {
            Require(c.schemaVersion == 1 && c.documentType == "randomGrowthEvent", "SCHEMA_HEADER", e);
            Require(c.contentContractVersion == ContentVersion && c.generatorVersion == GeneratorVersion, "VERSION", e);
            Require(c.poolId == PoolId && c.eventId == EventId && c.stageNodeId == StageId && c.nodeId == NodeId
                && c.segmentId == SegmentId && c.reservationId == ReservationId, "EXACT_ID_ALLOW_LIST", e);
            Require(c.startNodeId == NodeId && c.poolMode == "PartyWide", "ROOT_LINK", e);
            Require(c.pairedSectionIds != null && c.pairedSectionIds.SequenceEqual(new[] { LeftSection, RightSection }), "PAIRED_SECTIONS", e);
            Require(c.mainImagePath == ImagePath && c.mainImageSha256 == ImageSha256, "IMAGE_AUTHORITY", e);
            Require(CapIsValid(c.capPolicy), "CAP_POLICY", e);
            Require(c.presentationKo != null && TextValues(c.presentationKo).SequenceEqual(CanonicalText), "CANONICAL_KO_COPY", e);
            Require(c.nodes != null && c.nodes.Count == 1, "NODE_COUNT", e); if (c.nodes == null || c.nodes.Count != 1) return;
            var n = c.nodes[0]; Require(n.nodeId == NodeId && n.sourcePopupId == NodeId && n.nodeType == "PopupEvent", "SOURCE_POPUP_ID", e);
            Require(n.choices != null && n.choices.Count == 2, "CHOICE_COUNT", e); if (n.choices == null || n.choices.Count != 2) return;
            ValidateRisk(n.choices[0], e); ValidateDecline(n.choices[1], e);
        }

        private static void ValidateRisk(RandomGrowthContentChoice c, List<string> e)
        {
            Require(c.choiceId == RiskChoiceId && c.sourcePopupId == NodeId, "RISK_ID", e);
            Require(!c.isTerminal && c.disabledExecutorCallCount == 0, "RISK_RESERVATION_STATE", e);
            Require(c.rewards != null && c.rewards.Count == 0, "RISK_REWARDS_EMPTY", e); var x = c.execution;
            Require(Common(x, RiskChoiceId), "RISK_COMMON_PAYLOAD", e); if (x == null) return;
            Require(x.executionType == "RandomGrowthRisk" && x.resultKind == "RiskSelected" && x.successResultKind == "RiskGranted"
                && x.failureState == "RiskSelectedPendingRetry", "RISK_TYPED_STATE", e);
            Require(x.costPolicy != null && x.costPolicy.type == "MaxHpPercentNonlethal" && x.costPolicy.rateBasisPoints == 1000
                && x.costPolicy.rounding == "Ceil" && BitConverter.SingleToInt32Bits(x.costPolicy.minimumRemainingHp) == BitConverter.SingleToInt32Bits(1f), "RISK_COST_POLICY", e);
            Require(CapIsValid(x.capPolicy) && x.growthGrant == 1 && x.rewards != null && x.rewards.Count == 0, "RISK_GRANT", e);
            var r = x.interactionReservation;
            Require(r != null && r.authority == "StageSession" && r.lifetime == "SessionOnly"
                && r.stableKeyFields != null && r.stableKeyFields.SequenceEqual(new[] { "runId", "stageGenerationId", "reservationId", "encounteredNodeInstanceId" })
                && r.orderedStates != null && r.orderedStates.SequenceEqual(new[] { "RiskSelected", "Applying", "RiskSelectedPendingRetry", "RiskGranted" })
                && r.locksDecline && r.blocksDuplicateConfirm && r.mutationCountBeforeAtomicTransaction == 0, "INTERACTION_RESERVATION", e);
        }

        private static void ValidateDecline(RandomGrowthContentChoice c, List<string> e)
        {
            Require(c.choiceId == DeclineChoiceId && c.sourcePopupId == NodeId, "DECLINE_ID", e);
            Require(c.isTerminal && c.disabledExecutorCallCount == 0 && c.rewards != null && c.rewards.Count == 0, "DECLINE_TERMINAL", e);
            var x = c.execution; Require(Common(x, DeclineChoiceId), "DECLINE_COMMON_PAYLOAD", e); if (x == null) return;
            Require(x.executionType == "RandomGrowthDecline" && x.resultKind == "Declined" && x.cost == 0 && x.growthGrant == 0
                && IsSemanticallyEmpty(x.costPolicy) && IsSemanticallyEmpty(x.capPolicy)
                && IsSemanticallyEmpty(x.interactionReservation)
                && x.rewards != null && x.rewards.Count == 0, "DECLINE_ZERO_ZERO", e);
        }

        private static bool IsSemanticallyEmpty(RandomGrowthCostPolicyContent value) => value == null
            || (string.IsNullOrEmpty(value.type) && value.rateBasisPoints == 0 && string.IsNullOrEmpty(value.rounding)
                && BitConverter.SingleToInt32Bits(value.minimumRemainingHp) == BitConverter.SingleToInt32Bits(0f));

        private static bool IsSemanticallyEmpty(RandomGrowthCapPolicyContent value) => value == null
            || (value.fixedApplied == 0 && value.randomApplied == 0 && value.totalApplied == 0);

        private static bool IsSemanticallyEmpty(RandomGrowthInteractionReservationContent value) => value == null
            || (string.IsNullOrEmpty(value.authority) && string.IsNullOrEmpty(value.lifetime)
                && (value.stableKeyFields == null || value.stableKeyFields.Count == 0)
                && (value.orderedStates == null || value.orderedStates.Count == 0)
                && !value.locksDecline && !value.blocksDuplicateConfirm
                && value.mutationCountBeforeAtomicTransaction == 0);

        private static bool Common(RandomGrowthTypedExecution x, string choice) => x != null && x.schemaVersion == 1
            && x.contentContractVersion == ContentVersion && x.eventId == EventId && x.stageNodeId == StageId && x.nodeId == NodeId
            && x.choiceId == choice && x.segmentId == SegmentId && x.reservationId == ReservationId && x.poolMode == "PartyWide";
        private static bool CapIsValid(RandomGrowthCapPolicyContent c) => c != null && c.fixedApplied == 2 && c.randomApplied == 1 && c.totalApplied == 3;

        private static void ValidatePool(RandomGrowthPoolDocument p, List<string> e)
        {
            Require(p.schemaVersion == 1 && p.documentType == "randomGrowthEventPool" && p.contentContractVersion == ContentVersion && p.poolId == PoolId, "POOL_HEADER", e);
            Require(p.probabilityOwner == "chapterManifestAbsoluteRoll", "POOL_PROBABILITY_OWNER", e);
            Require(p.entries != null && p.entries.Count == 1, "POOL_ENTRY_COUNT", e); if (p.entries == null || p.entries.Count != 1) return;
            var x = p.entries[0]; Require(x.entryId == StageId && x.nodeJsonPath == EventJsonPath && x.nodeId == StageId && x.weight == 1, "POOL_ENTRY", e);
            Require(!x.oneShot && x.cooldownRounds == 0, "POOL_LEGACY_GUARDS_UNUSED", e);
        }

        private static string ComputeDefinitionFingerprint(RandomGrowthContentDocument c, RandomGrowthPoolDocument p, string digest)
        {
            var risk = c.nodes[0].choices[0].execution; var decline = c.nodes[0].choices[1].execution;
            string[] fields = { "ProjectBS.RandomGrowthContentDefinition.v1", c.schemaVersion.ToString(CultureInfo.InvariantCulture), c.contentContractVersion,
                c.generatorVersion, c.poolId, c.eventId, c.stageNodeId, c.nodeId, RiskChoiceId, DeclineChoiceId, c.segmentId, c.reservationId,
                LeftSection, RightSection, risk.executionType, risk.resultKind, risk.successResultKind, risk.failureState,
                decline.executionType, decline.resultKind, c.poolMode, risk.costPolicy.type, "1000", risk.costPolicy.rounding, "3f800000",
                "2", "1", "3", "rewards=[]", p.entries[0].weight.ToString(CultureInfo.InvariantCulture), digest };
            using var s = new MemoryStream(); foreach (string f in fields) WriteField(s, f); return Sha(s.ToArray());
        }

        private static string[] TextValues(RandomGrowthPresentationTextKo t) => t == null ? Array.Empty<string>() : new[] { t.title, t.discoveryBody,
            t.riskLabel, t.riskCostLine, t.riskEligibilityLine, t.riskRewardLine, t.declineLabel, t.declineHelper, t.confirmTitle, t.confirmBody,
            t.confirmCta, t.confirmCancelCta, t.disabledInsufficientParty, t.disabledInsufficientMemberTemplate, t.disabledNoCandidate,
            t.disabledCapReached, t.disabledTechnical, t.revalidateCta, t.riskSuccessBody, t.riskSuccessStatus, t.riskSuccessCta,
            t.declineResultBody, t.declineResultStatus, t.declineResultCta, t.transactionFailureBody, t.transactionFailureHelper,
            t.transactionRetryCta, t.growthAppliedFollowupTemplate };
        private static string Normalize(string s) => (s ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n").Normalize(NormalizationForm.FormC);
        private static void WriteField(Stream s, string value) { byte[] b = Encoding.UTF8.GetBytes(value ?? string.Empty); byte[] n = BitConverter.GetBytes(b.Length); if (!BitConverter.IsLittleEndian) Array.Reverse(n); s.Write(n, 0, n.Length); s.Write(b, 0, b.Length); }
        private static string Sha(byte[] b) { using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(b)).Replace("-", "").ToLowerInvariant(); }
        private static T Parse<T>(string json, string code, List<string> e) where T : class { try { var x = string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<T>(json); if (x == null) e.Add(code); return x; } catch { e.Add(code); return null; } }
        private static Result Empty(string code) => new(null, null, new[] { code }, string.Empty, string.Empty, null);
        private static RandomGrowthContentBuildPlan CreateBuildPlan(string f, string d) => new()
        {
            RoundNodeAssetPath = $"Assets/Contents/Stage/so/{StageId}.asset",
            PopupEventAssetPath = $"Assets/Contents/Stage/so/{NodeId}.asset",
            EventPoolAssetPath = $"Assets/Contents/Stage/so/{PoolId}.asset",
            DefinitionFingerprint = f,
            PresentationTextDigestKo = d
        };
        private static void Require(bool ok, string code, List<string> e) { if (!ok) e.Add(code); }
    }
}
#endif
