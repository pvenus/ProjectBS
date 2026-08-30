using System;
using System.Collections.Generic;
using System.Linq;
using Party;
using Progression;
using Progression.RandomGrowth;
using Session;
using Skill;

namespace Stage
{
    public enum SafeGrowthPopupAdapterStatus
    {
        RequiresConfirmation = 0, Disabled = 10, PendingRetry = 20,
        TerminalReplay = 30, Unsupported = 40, Succeeded = 50, Declined = 60,
        Failed = 70, IdentityMismatch = 80, Cancelled = 90,
        PresentationContentUnavailableAfterDisclosure = 100
    }

    public sealed class SafeGrowthPendingConfirmContext
    {
        public SafeGrowthPendingConfirmContext(string popupId, string nodeInstanceId,
            string nodeId, string choiceId, string definitionFingerprint,
            string interactionTokenId, string eligibilityRevision,
            string eligibilityFingerprint, SafeGrowthTransactionChoice choice)
        {
            PopupId = popupId ?? string.Empty; NodeInstanceId = nodeInstanceId ?? string.Empty;
            NodeId = nodeId ?? string.Empty; ChoiceId = choiceId ?? string.Empty;
            DefinitionFingerprint = definitionFingerprint ?? string.Empty;
            InteractionTokenId = interactionTokenId ?? string.Empty;
            EligibilityRevision = eligibilityRevision ?? string.Empty;
            EligibilityFingerprint = eligibilityFingerprint ?? string.Empty;
            Choice = choice;
        }
        public string PopupId { get; }
        public string NodeInstanceId { get; }
        public string NodeId { get; }
        public string ChoiceId { get; }
        public string DefinitionFingerprint { get; }
        public string InteractionTokenId { get; }
        public string EligibilityRevision { get; }
        public string EligibilityFingerprint { get; }
        public SafeGrowthTransactionChoice Choice { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(PopupId)
            && !string.IsNullOrWhiteSpace(NodeInstanceId) && !string.IsNullOrWhiteSpace(NodeId)
            && !string.IsNullOrWhiteSpace(ChoiceId) && !string.IsNullOrWhiteSpace(DefinitionFingerprint)
            && !string.IsNullOrWhiteSpace(InteractionTokenId);
        public bool SameIdentity(SafeGrowthPendingConfirmContext other) => other != null
            && string.Equals(PopupId, other.PopupId, StringComparison.Ordinal)
            && string.Equals(NodeInstanceId, other.NodeInstanceId, StringComparison.Ordinal)
            && string.Equals(NodeId, other.NodeId, StringComparison.Ordinal)
            && string.Equals(ChoiceId, other.ChoiceId, StringComparison.Ordinal)
            && string.Equals(DefinitionFingerprint, other.DefinitionFingerprint, StringComparison.Ordinal)
            && string.Equals(InteractionTokenId, other.InteractionTokenId, StringComparison.Ordinal);
    }

    public sealed class SafeGrowthPopupAdapterResult
    {
        internal SafeGrowthPopupAdapterResult(SafeGrowthPopupAdapterStatus status,
            SafeGrowthPendingConfirmContext pending, string reason,
            ProgressionOpportunitySnapshot opportunity = null)
        { Status = status; Pending = pending; Reason = reason ?? string.Empty; Opportunity = opportunity; }
        public SafeGrowthPopupAdapterStatus Status { get; }
        public SafeGrowthPendingConfirmContext Pending { get; }
        public string Reason { get; }
        public ProgressionOpportunitySnapshot Opportunity { get; }
    }

    public sealed class SafeGrowthPopupRuntimeAdapter
    {
        private readonly StageSession session;
        private readonly PartyRuntimeData party;
        private readonly EquipmentSkillSO[] catalog;
        private readonly PartyWideSafeGrowthEligibilityQuery eligibility = new();
        private readonly ChoiceExecutionRouter router;

        public SafeGrowthPopupRuntimeAdapter(StageSession session, PartyRuntimeData party,
            IEnumerable<EquipmentSkillSO> catalog, ChoiceExecutionRouter router)
        {
            this.session = session;
            this.party = party;
            this.catalog = (catalog ?? Array.Empty<EquipmentSkillSO>()).Where(x => x != null).ToArray();
            this.router = router ?? ChoiceExecutionRouter.CreateDefault();
        }

        public SafeGrowthPendingConfirmContext Pending => session?.SafeGrowthPendingConfirm;

        public bool TryGetSafeGrowthOpportunity(out ProgressionOpportunitySnapshot opportunity)
        {
            opportunity = session?.SafeGrowthRuntime?.ProgressionLedger?.GetSnapshots()
                .SingleOrDefault(x => x.SourceType == ProgressionSourceType.RandomEventSafe
                    && string.Equals(x.SourceId, ProgressionSourceRegistry.RandomGrowthSafeSource,
                        StringComparison.Ordinal));
            return opportunity != null;
        }

        public SafeGrowthPopupAdapterResult ValidateV2PresentationCatalog(
            PopupEventSO popup, RandomGrowthPresentationCopyAsset presentationCatalog)
        {
            if (popup?.choices == null || popup.choices.Count != 2)
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch, "SAFE_POPUP_CHOICE_COUNT");
            RandomGrowthChoiceExecutionData observe = popup.choices[0]?.executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            RandomGrowthChoiceExecutionData decline = popup.choices[1]?.executionConfig?.data
                as RandomGrowthChoiceExecutionData;
            if (observe == null || decline == null
                || observe.schemaVersion != 2 || decline.schemaVersion != 2
                || observe.contentContractVersion != "chapter1-random-growth-safe-content.v2"
                || decline.contentContractVersion != "chapter1-random-growth-safe-content.v2"
                || observe.presentationCatalogId != "presentation.catalog.act1.random_growth.02.windworn_sword_marks.ko-KR"
                || decline.presentationCatalogId != observe.presentationCatalogId
                || observe.definitionFingerprint != SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint
                || decline.definitionFingerprint != SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint)
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch, "SAFE_V2_POPUP_BINDING_MISMATCH");
            if (presentationCatalog == null || presentationCatalog.Fields.Count != 31)
                return Result(SafeGrowthPopupAdapterStatus.PresentationContentUnavailableAfterDisclosure,
                    RandomGrowthPresentationCopyMismatch.Missing.ToString());
            var expected = new RandomGrowthPresentationCopyExpectation(2,
                observe.contentContractVersion, observe.presentationLocale,
                observe.presentationCatalogId, observe.presentationProjectionKind,
                "chapter1.random-growth-safe.semantic-copy.v2",
                "chapter1.random-growth-safe.definition.v2", observe.eventId,
                observe.sourcePopupId, observe.presentationTextDigestKo,
                observe.definitionFingerprint, presentationCatalog.Fields.Select(x => x.Name));
            return SafeGrowthPresentationCopyResolver.TryResolveV2(presentationCatalog, expected,
                    out _, out RandomGrowthPresentationCopyMismatch mismatch)
                ? Result(SafeGrowthPopupAdapterStatus.Unsupported, "SAFE_V2_PRESENTATION_READY")
                : Result(SafeGrowthPopupAdapterStatus.PresentationContentUnavailableAfterDisclosure,
                    mismatch.ToString());
        }

        public SafeGrowthPresentationSnapshot GetPresentationSnapshot(
            PopupEventSO popup, SafeGrowthPresentationCopy copy,
            SafeGrowthPopupAdapterStatus runtimeStatus = SafeGrowthPopupAdapterStatus.Unsupported)
        {
            SafeGrowthEligibilitySnapshot current = eligibility.Query(party, catalog);
            SafeGrowthPendingConfirmContext pending = Pending;
            ProgressionChapterSummary summary = session?.SafeGrowthRuntime?.ProgressionLedger?.GetChapterSummary();
            bool alreadyGranted = summary?.RandomEarned >= 1;
            bool capReached = !alreadyGranted && summary?.TotalApplied >= 3;
            string expectedFingerprint = pending?.EligibilityFingerprint ?? current.Fingerprint;
            string expectedRevision = pending?.EligibilityRevision ?? current.Revision;
            ConfirmableChoiceRuntimeState queryState = session?.SafeGrowthInteraction?.State switch
            {
                SafeGrowthInteractionState.ObserveSelectedPendingRetry => ConfirmableChoiceRuntimeState.PendingRetry,
                SafeGrowthInteractionState.SafeGrowthGranted or SafeGrowthInteractionState.Declined => ConfirmableChoiceRuntimeState.Terminal,
                _ => ConfirmableChoiceRuntimeState.Offerable
            };
            ConfirmableChoiceDispatchResult observe = router.QueryConfirmable(
                popup?.GetChoice(SafeGrowthTransactionIds.ObserveChoiceId)?.executionConfig, queryState);
            ConfirmableChoiceDispatchResult decline = router.QueryConfirmable(
                popup?.GetChoice(SafeGrowthTransactionIds.DeclineChoiceId)?.executionConfig, queryState);
            SafeGrowthPresentationInput input = new(copy,
                session?.SafeGrowthInteraction?.State ?? SafeGrowthInteractionState.Offerable,
                session?.SafeGrowthInteraction?.IsApplying == true, current,
                alreadyGranted, capReached, session?.SafeGrowthRouteEncounter != null,
                runtimeStatus == SafeGrowthPopupAdapterStatus.TerminalReplay,
                session?.SafeGrowthRouteEncounter?.NodeInstanceId,
                pending?.InteractionTokenId ?? session?.SafeGrowthInteraction?.Token?.TokenId,
                expectedRevision, expectedFingerprint, observe, decline);
            return new SafeGrowthPresentationBuilder().Build(input);
        }

        public SafeGrowthPopupAdapterResult Select(PopupEventSO popup, RoundNode node,
            PopupEventChoice choice)
        {
            ConfirmableChoiceRuntimeState state = session?.SafeGrowthInteraction?.State switch
            {
                SafeGrowthInteractionState.ObserveSelectedPendingRetry => ConfirmableChoiceRuntimeState.PendingRetry,
                SafeGrowthInteractionState.SafeGrowthGranted or SafeGrowthInteractionState.Declined => ConfirmableChoiceRuntimeState.Terminal,
                _ => ConfirmableChoiceRuntimeState.Offerable
            };
            ConfirmableChoiceDispatchResult query = router.QueryConfirmable(choice?.executionConfig, state);
            if (query.Kind == ConfirmableChoiceDispatchKind.Unsupported)
                return Result(SafeGrowthPopupAdapterStatus.Unsupported);
            if (query.Kind == ConfirmableChoiceDispatchKind.Disabled)
                return Result(SafeGrowthPopupAdapterStatus.Disabled, query.DisabledReason.ToString());
            if (!ValidateRoute(popup, node, choice, out SafeGrowthRouteEncounterReceipt encounter,
                    out RandomGrowthChoiceExecutionData data))
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch);
            if (query.Kind == ConfirmableChoiceDispatchKind.TerminalReplay)
                return Result(SafeGrowthPopupAdapterStatus.TerminalReplay, pending: session.SafeGrowthPendingConfirm);

            SafeGrowthTransactionChoice transactionChoice = choice.executionConfig.executionType
                == ChoiceExecutionType.RandomGrowthSafe ? SafeGrowthTransactionChoice.Observe
                : SafeGrowthTransactionChoice.Decline;
            SafeGrowthEligibilitySnapshot snapshot = eligibility.Query(party, catalog);
            if (transactionChoice == SafeGrowthTransactionChoice.Observe
                && snapshot.Status != SafeGrowthEligibilityStatus.Eligible)
                return Result(SafeGrowthPopupAdapterStatus.Disabled, snapshot.Status.ToString());
            SafeGrowthInteractionKey key = new(session.SafeGrowthRuntime.RunId.Value,
                session.RandomGrowthSession.StageGenerationId,
                SafeGrowthTransactionIds.ReservationId, node.nodeId);
            SafeGrowthInteractionResult entered = session.SafeGrowthInteraction.TryEnterPreconfirm(
                key, choice.choiceId, data.definitionFingerprint, true, out SafeGrowthInteractionToken token);
            if (entered != SafeGrowthInteractionResult.Changed
                && entered != SafeGrowthInteractionResult.Existing)
                return Result(entered == SafeGrowthInteractionResult.AlreadyTerminal
                    ? SafeGrowthPopupAdapterStatus.TerminalReplay : SafeGrowthPopupAdapterStatus.Disabled,
                    entered.ToString());
            SafeGrowthPendingConfirmContext pending = new(popup.eventId, node.nodeId,
                node.roundNodeSO?.nodeId, choice.choiceId, data.definitionFingerprint,
                token.TokenId, snapshot.Revision, snapshot.Fingerprint, transactionChoice);
            if (!session.TryStoreSafeGrowthPending(pending))
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch);
            return Result(query.Kind == ConfirmableChoiceDispatchKind.PendingRetry
                ? SafeGrowthPopupAdapterStatus.PendingRetry
                : SafeGrowthPopupAdapterStatus.RequiresConfirmation, pending: pending);
        }

        public SafeGrowthPopupAdapterResult Cancel()
        {
            SafeGrowthPendingConfirmContext pending = Pending;
            SafeGrowthInteractionToken token = session?.SafeGrowthInteraction?.Token;
            if (pending == null || token == null
                || !string.Equals(pending.InteractionTokenId, token.TokenId, StringComparison.Ordinal)
                || session.SafeGrowthInteraction.TryCancelPreconfirm(token) != SafeGrowthInteractionResult.Changed)
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch);
            session.ClearSafeGrowthPending();
            return Result(SafeGrowthPopupAdapterStatus.Cancelled);
        }

        public SafeGrowthPopupAdapterResult Recheck()
        {
            SafeGrowthPendingConfirmContext pending = Pending;
            if (pending == null) return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch);
            SafeGrowthEligibilitySnapshot snapshot = eligibility.Query(party, catalog);
            if (pending.Choice == SafeGrowthTransactionChoice.Observe
                && snapshot.Status != SafeGrowthEligibilityStatus.Eligible)
                return Result(SafeGrowthPopupAdapterStatus.Disabled, snapshot.Status.ToString(), pending);
            SafeGrowthPendingConfirmContext refreshed = new(pending.PopupId, pending.NodeInstanceId,
                pending.NodeId, pending.ChoiceId, pending.DefinitionFingerprint,
                pending.InteractionTokenId, snapshot.Revision, snapshot.Fingerprint, pending.Choice);
            if (!session.TryStoreSafeGrowthPending(refreshed))
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch);
            return Result(SafeGrowthPopupAdapterStatus.RequiresConfirmation, pending: refreshed);
        }

        public SafeGrowthPopupAdapterResult Recheck(PopupEventSO popup, RoundNode node,
            PopupEventChoice choice) => Pending == null ? Select(popup, node, choice) : Recheck();

        public SafeGrowthPopupAdapterResult Confirm(PopupEventSO popup, RoundNode node,
            PopupEventChoice choice, SafeGrowthPendingConfirmContext expected,
            Action<RoundNode> completed = null, Action<StageProgressState> progress = null)
        {
            SafeGrowthPendingConfirmContext pending = Pending;
            SafeGrowthInteractionToken token = session?.SafeGrowthInteraction?.Token;
            if (pending == null && expected != null && token != null
                && (session.SafeGrowthInteraction.State == SafeGrowthInteractionState.SafeGrowthGranted
                    || session.SafeGrowthInteraction.State == SafeGrowthInteractionState.Declined)
                && string.Equals(token.TokenId, expected.InteractionTokenId, StringComparison.Ordinal)
                && string.Equals(token.ChoiceId, expected.ChoiceId, StringComparison.Ordinal)
                && string.Equals(token.DefinitionFingerprint, expected.DefinitionFingerprint, StringComparison.Ordinal)
                && ValidateRoute(popup, node, choice, out _, out _))
                return Result(SafeGrowthPopupAdapterStatus.TerminalReplay);
            if (pending == null || expected == null || !pending.SameIdentity(expected)
                || token == null || !string.Equals(token.TokenId, pending.InteractionTokenId, StringComparison.Ordinal)
                || !ValidateRoute(popup, node, choice, out _, out RandomGrowthChoiceExecutionData data)
                || !string.Equals(data.definitionFingerprint, pending.DefinitionFingerprint, StringComparison.Ordinal))
                return Result(SafeGrowthPopupAdapterStatus.IdentityMismatch);
            SafeGrowthEligibilitySnapshot current = eligibility.Query(party, catalog);
            if (pending.Choice == SafeGrowthTransactionChoice.Observe
                && (current.Status != SafeGrowthEligibilityStatus.Eligible
                    || !string.Equals(current.Fingerprint, pending.EligibilityFingerprint, StringComparison.Ordinal)))
                return Result(SafeGrowthPopupAdapterStatus.Disabled, "STALE_ELIGIBILITY", pending);

            ProgressionEarnRequest earn = pending.Choice == SafeGrowthTransactionChoice.Observe
                ? new ProgressionEarnRequest(ProgressionSourceRegistry.OptionalRandomGrowthSegment,
                    ProgressionSourceCategory.Random, ProgressionSourceType.RandomEventSafe,
                    ProgressionSourceRegistry.RandomGrowthSafeSource, SafeGrowthTransactionIds.GrantedResultId)
                : null;
            SafeGrowthTransactionCommand command = new(token, pending.Choice, earn, current.EligibleCount);
            SafeGrowthRuntimeComposition runtime = session.SafeGrowthRuntime;
            SafeGrowthPrepareReceipt prepared = runtime.Transaction.TryPrepare(command);
            if (prepared.Result != SafeGrowthPrepareResult.Prepared)
            {
                session.SafeGrowthInteraction.TryMarkPendingRetry(token);
                return Result(SafeGrowthPopupAdapterStatus.Failed, prepared.Result.ToString(), pending);
            }
            string revision = StageAtomicNodeCompletionService.ComputeRevision(session.RuntimeData?.currentGraph);
            AtomicNodeCompletionResult nodePrepared = runtime.NodeCompletion.TryPrepareCompletion(
                session, token.Key.RunId, token.Key.StageGenerationId, node.nodeId,
                node.roundNodeSO?.nodeId, revision, out AtomicNodeCompletionToken nodeToken);
            if (nodePrepared != AtomicNodeCompletionResult.Prepared)
            {
                runtime.Transaction.Abort(prepared.Token);
                session.SafeGrowthInteraction.TryMarkPendingRetry(token);
                return Result(SafeGrowthPopupAdapterStatus.Failed, nodePrepared.ToString(), pending);
            }
            if (runtime.NodeCompletion.TryCommit(nodeToken) != AtomicNodeCompletionResult.Committed)
            {
                runtime.NodeCompletion.TryAbort(nodeToken);
                runtime.Transaction.Abort(prepared.Token);
                session.SafeGrowthInteraction.TryMarkPendingRetry(token);
                return Result(SafeGrowthPopupAdapterStatus.Failed, "NODE_COMMIT_FAILED", pending);
            }
            SafeGrowthTransactionReceipt receipt = runtime.Transaction.Finalize(prepared.Token);
            SafeGrowthTransactionResult wanted = pending.Choice == SafeGrowthTransactionChoice.Observe
                ? SafeGrowthTransactionResult.Succeeded : SafeGrowthTransactionResult.Declined;
            if (receipt.Result != wanted)
            {
                runtime.NodeCompletion.TryRollback(nodeToken);
                runtime.Transaction.Abort(prepared.Token);
                return Result(SafeGrowthPopupAdapterStatus.Failed, receipt.Result.ToString(), pending);
            }
            runtime.NodeCompletion.PublishDeferred(nodeToken, completed, progress);
            session.ClearSafeGrowthPending();
            return Result(pending.Choice == SafeGrowthTransactionChoice.Observe
                ? SafeGrowthPopupAdapterStatus.Succeeded : SafeGrowthPopupAdapterStatus.Declined,
                opportunity: receipt.Opportunity);
        }

        private bool ValidateRoute(PopupEventSO popup, RoundNode node, PopupEventChoice choice,
            out SafeGrowthRouteEncounterReceipt encounter, out RandomGrowthChoiceExecutionData data)
        {
            encounter = session?.SafeGrowthRouteEncounter;
            data = choice?.executionConfig?.data as RandomGrowthChoiceExecutionData;
            return popup != null && node != null && choice != null && data != null && encounter != null
                && string.Equals(popup.eventId, ConfirmableChoiceContract.SourcePopupId, StringComparison.Ordinal)
                && string.Equals(encounter.DisplayedEventId, SafeGrowthTransactionIds.EventId, StringComparison.Ordinal)
                && string.Equals(node.nodeId, encounter.NodeInstanceId, StringComparison.Ordinal)
                && string.Equals(node.roundNodeSO?.nodeId, ConfirmableChoiceContract.StageNodeId, StringComparison.Ordinal)
                && string.Equals(choice.choiceId, data.choiceId, StringComparison.Ordinal);
        }

        private static SafeGrowthPopupAdapterResult Result(SafeGrowthPopupAdapterStatus status,
            string reason = "", SafeGrowthPendingConfirmContext pending = null,
            ProgressionOpportunitySnapshot opportunity = null) => new(status, pending, reason, opportunity);
    }
}
