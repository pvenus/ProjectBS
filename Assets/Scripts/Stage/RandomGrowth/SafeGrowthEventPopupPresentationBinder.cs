using System;
using System.Linq;

namespace Stage
{
    public sealed class SafeGrowthEventPopupPresentationBinder
    {
        public bool TryBuild(PopupEventSO popup, RandomGrowthPresentationCopyAsset catalog,
            SafeGrowthPopupRuntimeAdapter adapter, out SafeGrowthPresentationSnapshot snapshot,
            out RandomGrowthPresentationCopyMismatch mismatch)
        {
            snapshot = null;
            mismatch = RandomGrowthPresentationCopyMismatch.Missing;
            if (popup?.choices == null || popup.choices.Count != 2 || adapter == null)
                return false;
            RandomGrowthChoiceExecutionData data = popup.choices[0]?.executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            if (data == null) return false;
            var expected = new RandomGrowthPresentationCopyExpectation(2,
                data.contentContractVersion, data.presentationLocale,
                data.presentationCatalogId, data.presentationProjectionKind,
                "chapter1.random-growth-safe.semantic-copy.v2",
                "chapter1.random-growth-safe.definition.v2", data.eventId,
                data.sourcePopupId, data.presentationTextDigestKo,
                data.definitionFingerprint, catalog?.Fields.Select(x => x.Name));
            if (!SafeGrowthPresentationCopyResolver.TryResolveV2(catalog, expected,
                    out SafeGrowthPresentationCopy copy, out mismatch))
                return false;
            snapshot = adapter.GetPresentationSnapshot(popup, copy);
            return snapshot != null && snapshot.State != SafeGrowthPresentationState.Invalid;
        }

        public static string ResolveLabel(SafeGrowthPresentationSnapshot snapshot,
            SafeGrowthPresentationActionIntent intent) => intent switch
        {
            SafeGrowthPresentationActionIntent.CancelPreconfirm => snapshot.CancelCta,
            SafeGrowthPresentationActionIntent.ConfirmDecline => snapshot.CancelCta,
            SafeGrowthPresentationActionIntent.RecheckEligibility => snapshot.RecheckCta,
            SafeGrowthPresentationActionIntent.RequestObservePreconfirm => snapshot.Cta,
            SafeGrowthPresentationActionIntent.ConfirmObserve => snapshot.Cta,
            SafeGrowthPresentationActionIntent.RetrySameChoice => snapshot.Cta,
            SafeGrowthPresentationActionIntent.OpenGrowthOffer => snapshot.Cta,
            SafeGrowthPresentationActionIntent.ContinueStage => snapshot.Cta,
            _ => string.Empty
        };
    }
}
