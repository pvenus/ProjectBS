using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Stage
{
    public enum SafeGrowthPresentationState
    {
        Invalid = 0, Discovery = 10, Offerable = 20, Preconfirm = 30,
        DisabledNoCandidate = 40, DisabledAlreadyGranted = 50,
        DisabledCapReached = 60, DisabledInvalidData = 70,
        DisabledPartyChanged = 80, PendingRetry = 90,
        TerminalSafeGranted = 100, TerminalDeclined = 110,
        TerminalReplay = 120, BusyApplying = 130
    }

    public enum SafeGrowthPresentationDisabledReason
    {
        None = 0, NoCandidate = 10, AlreadyGranted = 20, CapReached = 30,
        InvalidData = 40, PartyChanged = 50, IdentityMismatch = 60, Busy = 70
    }

    public enum SafeGrowthPresentationActionIntent
    {
        None = 0, RequestObservePreconfirm = 10, CancelPreconfirm = 20,
        ConfirmObserve = 30, ConfirmDecline = 40, RecheckEligibility = 50,
        OpenGrowthOffer = 60, ContinueStage = 70, RetrySameChoice = 80
    }

    public sealed class SafeGrowthCopyField
    {
        public SafeGrowthCopyField(string name, string value)
        { Name = name ?? string.Empty; Value = value ?? string.Empty; }
        public string Name { get; }
        public string Value { get; }
    }

    public sealed class SafeGrowthPresentationCopy
    {
        internal SafeGrowthPresentationCopy(IReadOnlyDictionary<string, string> values,
            string semanticDigest, string definitionFingerprint, int schemaVersion = 1)
        {
            Values = values; SemanticDigest = semanticDigest;
            DefinitionFingerprint = definitionFingerprint; SchemaVersion = schemaVersion;
        }
        internal IReadOnlyDictionary<string, string> Values { get; }
        public string SemanticDigest { get; }
        public string DefinitionFingerprint { get; }
        public int SchemaVersion { get; }
        public string Get(string name) => Values.TryGetValue(name, out string value) ? value : string.Empty;
    }

    public static class SafeGrowthPresentationCopyResolver
    {
        public const string SemanticDigest = "a5d02f07c900e11c29887811197dd8183c269162abaec5219a0551bdec19ac35";
        public const string DefinitionFingerprint = "0de9e9ac1418ccdce75d0fc2826c919d26790eebbc5b841d69bc2e35814252bb";
        public const string V2SemanticDigest = "5b6ab7c72e213c70b0f38e5601263c461bc025fb40689357ae264b07920c9b80";
        public const string V2DefinitionFingerprint = "72acd7c52fdc3aebe4e9c5cedbdb01377c80e1f65512478d25657973b65da6bd";
        private const string Domain = "chapter1.random-growth-safe.semantic-copy.v1";
        private static readonly string[] OrderedNames =
        {
            "discoveryTitle", "discoveryBody", "capNotice", "methodLabel", "methodSummary",
            "rewardSummary", "observeLabel", "observeAssist", "declineLabel", "declineAssist",
            "confirmTitle", "confirmBody", "confirmCta", "confirmCancel", "candidateZeroDisabled",
            "candidateZeroReason", "candidateZeroRecheckCta", "alreadyGrantedDisabled", "successBody",
            "successStatus", "successCta", "declineBody", "declineStatus", "declineCta",
            "failureBody", "failureAssist", "failureRetryCta", "reminderTemplate"
        };

        public static bool TryResolve(IEnumerable<SafeGrowthCopyField> fields,
            string definitionFingerprint, out SafeGrowthPresentationCopy copy)
        {
            copy = null;
            SafeGrowthCopyField[] input = fields?.ToArray();
            if (input == null || input.Length != OrderedNames.Length
                || !string.Equals(definitionFingerprint, DefinitionFingerprint, StringComparison.Ordinal))
                return false;
            Dictionary<string, string> values = new(StringComparer.Ordinal);
            List<string> normalized = new();
            for (int i = 0; i < OrderedNames.Length; i++)
            {
                SafeGrowthCopyField field = input[i];
                if (field == null || !string.Equals(field.Name, OrderedNames[i], StringComparison.Ordinal))
                    return false;
                string value = Normalize(field.Value);
                if (!string.Equals(value, field.Value, StringComparison.Ordinal) || !values.TryAdd(field.Name, value))
                    return false;
                normalized.Add(field.Name); normalized.Add(value);
            }
            if (!string.Equals(ComputeDigest(normalized), SemanticDigest, StringComparison.Ordinal))
                return false;
            copy = new SafeGrowthPresentationCopy(
                new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(values),
                SemanticDigest, DefinitionFingerprint);
            return true;
        }

        public static bool TryResolveV2(RandomGrowthPresentationCopyAsset asset,
            RandomGrowthPresentationCopyExpectation expected, out SafeGrowthPresentationCopy copy,
            out RandomGrowthPresentationCopyMismatch mismatch)
        {
            copy = null;
            mismatch = RandomGrowthPresentationCopyMismatch.WrongSchema;
            if (expected == null || expected.SchemaVersion != 2
                || expected.OrderedFieldNames.Count != 31) return false;
            if (expected.SemanticCopyDigest != V2SemanticDigest)
            { mismatch = RandomGrowthPresentationCopyMismatch.DigestMismatch; return false; }
            if (expected.DefinitionFingerprint != V2DefinitionFingerprint)
            { mismatch = RandomGrowthPresentationCopyMismatch.FingerprintMismatch; return false; }
            if (!RandomGrowthPresentationCopyResolver.TryResolve(asset, expected,
                    out RandomGrowthResolvedPresentationCopy resolved, out mismatch))
                return false;
            copy = new SafeGrowthPresentationCopy(resolved.Values,
                expected.SemanticCopyDigest, expected.DefinitionFingerprint, 2);
            return true;
        }

        private static string ComputeDigest(IReadOnlyList<string> nameValues)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, new UTF8Encoding(false), true))
            {
                Write(writer, Domain);
                foreach (string field in nameValues) Write(writer, field);
            }
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(stream.ToArray()).Select(x => x.ToString("x2")));
        }

        private static void Write(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            writer.Write((byte)(bytes.Length >> 24));
            writer.Write((byte)(bytes.Length >> 16));
            writer.Write((byte)(bytes.Length >> 8));
            writer.Write((byte)bytes.Length);
            writer.Write(bytes);
        }

        private static string Normalize(string value) => (value ?? string.Empty)
            .Normalize(NormalizationForm.FormC).Replace("\r\n", "\n").Replace("\r", "\n");
    }

    public sealed class SafeGrowthPresentationSnapshot
    {
        internal SafeGrowthPresentationSnapshot(SafeGrowthPresentationState state,
            SafeGrowthPresentationDisabledReason disabledReason, string eventId, string stageNodeId,
            string sourcePopupId, string nodeInstanceId,
            string observeChoiceId, string declineChoiceId, string resultId, string interactionTokenId,
            string runtimeRevision, string runtimeFingerprint, SafeGrowthPresentationCopy copy,
            bool observeEnabled, bool declineEnabled, int eligibleCount,
            IReadOnlyList<SafeGrowthPresentationActionIntent> actions,
            string title, string body, string method, string reward, string capNotice,
            string assist, string status, string cta, string cancelCta, string recheckCta)
        {
            State = state; DisabledReason = disabledReason; EventId = eventId ?? string.Empty;
            StageNodeId = stageNodeId ?? string.Empty; SourcePopupId = sourcePopupId ?? string.Empty;
            NodeInstanceId = nodeInstanceId ?? string.Empty; ObserveChoiceId = observeChoiceId ?? string.Empty;
            DeclineChoiceId = declineChoiceId ?? string.Empty; ResultId = resultId ?? string.Empty;
            InteractionTokenId = interactionTokenId ?? string.Empty;
            RuntimeRevision = runtimeRevision ?? string.Empty; RuntimeFingerprint = runtimeFingerprint ?? string.Empty;
            SemanticCopyDigest = copy?.SemanticDigest ?? string.Empty;
            DefinitionFingerprint = copy?.DefinitionFingerprint ?? string.Empty;
            ObserveEnabled = observeEnabled; DeclineEnabled = declineEnabled;
            EligibleCount = Math.Max(0, eligibleCount); TargetCount = 2;
            DisplayCandidateCount = Math.Min(TargetCount, EligibleCount);
            Actions = Array.AsReadOnly((actions ?? Array.Empty<SafeGrowthPresentationActionIntent>()).ToArray());
            Title = title ?? string.Empty; Body = body ?? string.Empty; Method = method ?? string.Empty;
            Reward = reward ?? string.Empty; CapNotice = capNotice ?? string.Empty;
            Assist = assist ?? string.Empty; Status = status ?? string.Empty;
            Cta = cta ?? string.Empty; CancelCta = cancelCta ?? string.Empty;
            RecheckCta = recheckCta ?? string.Empty;
        }
        public SafeGrowthPresentationState State { get; }
        public SafeGrowthPresentationDisabledReason DisabledReason { get; }
        public string EventId { get; }
        public string StageNodeId { get; }
        public string SourcePopupId { get; }
        public string NodeInstanceId { get; }
        public string ObserveChoiceId { get; }
        public string DeclineChoiceId { get; }
        public string ResultId { get; }
        public string InteractionTokenId { get; }
        public string RuntimeRevision { get; }
        public string RuntimeFingerprint { get; }
        public string SemanticCopyDigest { get; }
        public string DefinitionFingerprint { get; }
        public bool ObserveEnabled { get; }
        public bool DeclineEnabled { get; }
        public int TargetCount { get; }
        public int EligibleCount { get; }
        public int DisplayCandidateCount { get; }
        public IReadOnlyList<SafeGrowthPresentationActionIntent> Actions { get; }
        public string Title { get; }
        public string Body { get; }
        public string Method { get; }
        public string Reward { get; }
        public string CapNotice { get; }
        public string Assist { get; }
        public string Status { get; }
        public string Cta { get; }
        public string CancelCta { get; }
        public string RecheckCta { get; }
    }
}
