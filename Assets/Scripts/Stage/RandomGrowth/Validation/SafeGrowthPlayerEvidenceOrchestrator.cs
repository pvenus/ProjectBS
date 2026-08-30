using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Progression;
using Progression.RandomGrowth;

namespace Stage
{
    internal enum SafeGrowthEvidenceIdentityError
    {
        None = 0,
        MissingPopup = 10,
        ChoiceCardinalityMismatch = 20,
        MissingObservePayload = 30,
        PopupSourceIdentityMismatch = 40,
        PayloadPortfolioEventMismatch = 50,
        PayloadNodeIdentityMismatch = 60,
        PayloadStageIdentityMismatch = 70,
        CatalogIdentityMismatch = 80,
        CatalogLocaleMismatch = 90,
        CatalogProjectionKindMismatch = 100,
        SemanticDigestMismatch = 110,
        DefinitionFingerprintMismatch = 120,
        CatalogTupleMismatch = 130
    }

    public sealed class SafeGrowthPlayerEvidenceOrchestrator
    {
        public const string Authority = "TypedEvidenceProjection";
        public const string Schema = "chapter1.safe-growth.g2-typed-evidence.v1";

        public bool TryProject(PopupEventSO popup, RandomGrowthPresentationCopyAsset catalog,
            SafeGrowthPlayerEvidenceCase evidenceCase, string token, string planSha,
            out SafeGrowthPresentationSnapshot snapshot, out string payloadSha)
        {
            snapshot = null;
            payloadSha = string.Empty;
            if (evidenceCase == null || evidenceCase.Lane == SafeGrowthEvidenceLane.MacG3
                || !string.Equals(token, SafeGrowthPlayerEvidencePlan.Token, StringComparison.Ordinal)
                || !string.Equals(planSha, SafeGrowthPlayerEvidencePlan.CreateCanonical().Sha256,
                    StringComparison.Ordinal))
                return false;

            if (!TryValidateIdentity(popup, catalog, out RandomGrowthChoiceExecutionData data,
                    out SafeGrowthPresentationCopy copy, out _))
                return false;

            int count = ResolveCandidateCount(evidenceCase.Id);
            SafeGrowthEligibilitySnapshot eligibility = Eligibility(count);
            SafeGrowthInteractionState interaction = ResolveInteraction(evidenceCase.ExpectedState,
                evidenceCase.Id);
            bool applying = evidenceCase.ExpectedState == SafeGrowthPresentationState.BusyApplying;
            bool replay = evidenceCase.ExpectedState == SafeGrowthPresentationState.TerminalReplay;
            ConfirmableChoiceRuntimeState runtime = interaction switch
            {
                SafeGrowthInteractionState.ObserveSelectedPendingRetry => ConfirmableChoiceRuntimeState.PendingRetry,
                SafeGrowthInteractionState.SafeGrowthGranted or SafeGrowthInteractionState.Declined =>
                    ConfirmableChoiceRuntimeState.Terminal,
                _ => ConfirmableChoiceRuntimeState.Offerable
            };
            ConfirmableChoiceDispatchResult observe = ChoiceExecutionRouter.CreateDefault()
                .QueryConfirmable(popup.choices[0].executionConfig, runtime);
            ConfirmableChoiceDispatchResult decline = ChoiceExecutionRouter.CreateDefault()
                .QueryConfirmable(popup.choices[1].executionConfig, runtime);
            payloadSha = HashFields(Schema, Authority,
                evidenceCase.Id, ((int)evidenceCase.ExpectedState).ToString(), count.ToString(),
                popup.eventId, data.eventId, data.stageNodeId, data.sourcePopupId,
                copy.SemanticDigest, copy.DefinitionFingerprint, token, planSha);
            snapshot = new SafeGrowthPresentationBuilder().Build(new SafeGrowthPresentationInput(
                copy, interaction, applying, eligibility, false, false,
                evidenceCase.ExpectedState != SafeGrowthPresentationState.Discovery, replay,
                "evidence." + evidenceCase.Id, "evidence-token." + payloadSha.Substring(0, 16),
                eligibility.Revision, eligibility.Fingerprint, observe, decline));
            return snapshot != null && snapshot.State == evidenceCase.ExpectedState;
        }

        internal static bool TryValidateIdentity(PopupEventSO popup,
            RandomGrowthPresentationCopyAsset catalog, out RandomGrowthChoiceExecutionData data,
            out SafeGrowthPresentationCopy copy, out SafeGrowthEvidenceIdentityError error)
        {
            data = null;
            copy = null;
            error = SafeGrowthEvidenceIdentityError.None;
            if (popup == null) return Fail(SafeGrowthEvidenceIdentityError.MissingPopup, out error);
            if (popup.choices?.Count != 2)
                return Fail(SafeGrowthEvidenceIdentityError.ChoiceCardinalityMismatch, out error);
            data = popup.choices[0]?.executionConfig?.data as RandomGrowthChoiceExecutionData;
            if (data == null)
                return Fail(SafeGrowthEvidenceIdentityError.MissingObservePayload, out error);
            if (!string.Equals(popup.eventId, data.sourcePopupId, StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.PopupSourceIdentityMismatch, out error);
            if (!string.Equals(data.eventId, SafeGrowthTransactionIds.EventId, StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.PayloadPortfolioEventMismatch, out error);
            if (!string.Equals(data.sourcePopupId, ConfirmableChoiceContract.SourcePopupId,
                    StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.PayloadNodeIdentityMismatch, out error);
            if (!string.Equals(data.stageNodeId, ConfirmableChoiceContract.StageNodeId,
                    StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.PayloadStageIdentityMismatch, out error);
            if (!string.Equals(data.presentationCatalogId, ConfirmableChoiceContract.V2CatalogId,
                    StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.CatalogIdentityMismatch, out error);
            if (!string.Equals(data.presentationLocale, ConfirmableChoiceContract.V2Locale,
                    StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.CatalogLocaleMismatch, out error);
            if (!string.Equals(data.presentationProjectionKind, ConfirmableChoiceContract.V2ProjectionKind,
                    StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.CatalogProjectionKindMismatch, out error);
            if (!string.Equals(data.presentationTextDigestKo,
                    SafeGrowthPresentationCopyResolver.V2SemanticDigest, StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.SemanticDigestMismatch, out error);
            if (!string.Equals(data.definitionFingerprint,
                    SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint, StringComparison.Ordinal))
                return Fail(SafeGrowthEvidenceIdentityError.DefinitionFingerprintMismatch, out error);

            var expected = new RandomGrowthPresentationCopyExpectation(2,
                data.contentContractVersion, data.presentationLocale, data.presentationCatalogId,
                data.presentationProjectionKind, "chapter1.random-growth-safe.semantic-copy.v2",
                "chapter1.random-growth-safe.definition.v2", data.eventId, data.sourcePopupId,
                data.presentationTextDigestKo, data.definitionFingerprint,
                Array.ConvertAll(catalog?.Fields?.ToArray() ?? Array.Empty<RandomGrowthPresentationCopyFieldData>(),
                    field => field.Name));
            if (SafeGrowthPresentationCopyResolver.TryResolveV2(catalog, expected,
                    out copy, out RandomGrowthPresentationCopyMismatch mismatch))
                return true;
            return Fail(Map(mismatch), out error);
        }

        private static SafeGrowthEvidenceIdentityError Map(RandomGrowthPresentationCopyMismatch mismatch) =>
            mismatch switch
            {
                RandomGrowthPresentationCopyMismatch.WrongLocale =>
                    SafeGrowthEvidenceIdentityError.CatalogLocaleMismatch,
                RandomGrowthPresentationCopyMismatch.WrongIdentity =>
                    SafeGrowthEvidenceIdentityError.CatalogIdentityMismatch,
                RandomGrowthPresentationCopyMismatch.WrongProjectionKind =>
                    SafeGrowthEvidenceIdentityError.CatalogProjectionKindMismatch,
                RandomGrowthPresentationCopyMismatch.DigestMismatch =>
                    SafeGrowthEvidenceIdentityError.SemanticDigestMismatch,
                RandomGrowthPresentationCopyMismatch.FingerprintMismatch =>
                    SafeGrowthEvidenceIdentityError.DefinitionFingerprintMismatch,
                _ => SafeGrowthEvidenceIdentityError.CatalogTupleMismatch
            };

        private static bool Fail(SafeGrowthEvidenceIdentityError value,
            out SafeGrowthEvidenceIdentityError error)
        { error = value; return false; }

        private static int ResolveCandidateCount(string id)
        {
            if (id?.Contains("c0", StringComparison.Ordinal) == true) return 0;
            if (id?.Contains("c1", StringComparison.Ordinal) == true) return 1;
            return 2;
        }

        private static SafeGrowthInteractionState ResolveInteraction(
            SafeGrowthPresentationState state, string id) => state switch
        {
            SafeGrowthPresentationState.Preconfirm => SafeGrowthInteractionState.Preconfirm,
            SafeGrowthPresentationState.PendingRetry => SafeGrowthInteractionState.ObserveSelectedPendingRetry,
            SafeGrowthPresentationState.TerminalSafeGranted => SafeGrowthInteractionState.SafeGrowthGranted,
            SafeGrowthPresentationState.TerminalDeclined => SafeGrowthInteractionState.Declined,
            SafeGrowthPresentationState.TerminalReplay when id?.Contains("decline", StringComparison.Ordinal) == true
                => SafeGrowthInteractionState.Declined,
            SafeGrowthPresentationState.TerminalReplay => SafeGrowthInteractionState.SafeGrowthGranted,
            _ => SafeGrowthInteractionState.Offerable
        };

        private static SafeGrowthEligibilitySnapshot Eligibility(int count)
        {
            var targets = new List<SafeGrowthEligibleTarget>();
            for (int i = 0; i < count; i++)
                targets.Add(new SafeGrowthEligibleTarget("evidence.owner." + i,
                    "evidence.skill." + i, "evidence.skill." + i, 1, 2));
            return new SafeGrowthEligibilitySnapshot(count == 0
                ? SafeGrowthEligibilityStatus.NoCandidate : SafeGrowthEligibilityStatus.Eligible,
                targets, HashFields("eligibility", count.ToString()));
        }

        public static string HashFields(params string[] fields)
        {
            using MemoryStream stream = new();
            foreach (string value in fields ?? Array.Empty<string>())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
                stream.WriteByte((byte)(bytes.Length >> 24));
                stream.WriteByte((byte)(bytes.Length >> 16));
                stream.WriteByte((byte)(bytes.Length >> 8));
                stream.WriteByte((byte)bytes.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            using SHA256 sha = SHA256.Create();
            StringBuilder result = new();
            foreach (byte value in sha.ComputeHash(stream.ToArray())) result.Append(value.ToString("x2"));
            return result.ToString();
        }
    }
}
