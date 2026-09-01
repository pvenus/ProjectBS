using System;
using Progression;
using Session;

namespace Stage
{
    public enum PortfolioRandomGrowthDispatchStatus
    { Unsupported, RequiresConfirmation, PendingRetry, TerminalReplay, Succeeded, Declined, Failed }

    public sealed class PortfolioRandomGrowthDispatchResult
    {
        public PortfolioRandomGrowthDispatchResult(PortfolioRandomGrowthDispatchStatus status,string error="")
        {Status=status;Error=error??string.Empty;}
        public PortfolioRandomGrowthDispatchStatus Status{get;} public string Error{get;}
    }

    public sealed class PortfolioRandomGrowthPopupRuntimeAdapter
    {
        private readonly StageSession session;
        public PortfolioRandomGrowthPopupRuntimeAdapter(StageSession value){session=value;}
        public PortfolioRandomGrowthDispatchResult Select(PopupEventSO popup,RoundNode node,PopupEventChoice choice)
        {
            if(choice?.executionConfig?.data is not RandomGrowthChoiceExecutionData data
                || !IsB1(data.eventId))return new(PortfolioRandomGrowthDispatchStatus.Unsupported);
            RandomGrowthPayloadKind kind=choice.executionConfig.executionType switch
            {ChoiceExecutionType.RandomGrowthSafe=>RandomGrowthPayloadKind.Safe,
             ChoiceExecutionType.RandomGrowthRisk=>RandomGrowthPayloadKind.Risk,
             ChoiceExecutionType.RandomGrowthDecline=>RandomGrowthPayloadKind.Decline,_=>(RandomGrowthPayloadKind)0};
            if(kind==0||popup==null||node==null||!RandomGrowthEventIdentityCatalog.TryResolve(
                data.eventId,data.choiceId,kind,out RandomGrowthEventIdentity identity)
                ||popup.eventId!=identity.NodeId||data.sourcePopupId!=identity.NodeId
                ||data.reservationId!=identity.ReservationId||data.segmentId!=identity.SegmentId)
                return new(PortfolioRandomGrowthDispatchStatus.Failed,"PORTFOLIO_RANDOM_GROWTH_IDENTITY_MISMATCH");
            if(session.PortfolioRandomGrowth.IsTerminal(identity.EventId,node.nodeId))
                return new(PortfolioRandomGrowthDispatchStatus.TerminalReplay);
            PortfolioRandomGrowthPending existing=session.PortfolioRandomGrowth.Pending;
            string stageGenerationId=session.RandomGrowthSession?.StageGenerationId??string.Empty;
            bool samePendingRequest=existing!=null
                &&string.Equals(existing.Identity?.EventId,identity.EventId,StringComparison.Ordinal)
                &&string.Equals(existing.NodeInstanceId,node.nodeId,StringComparison.Ordinal)
                &&string.Equals(existing.Identity?.ChoiceId,identity.ChoiceId,StringComparison.Ordinal);
            if(existing!=null&&!samePendingRequest)
            {
                // An abandoned confirmation (including another choice on the same node) has
                // applied no mutation and must not reserve the whole run. An identical request
                // remains idempotent and is reused by TryBegin below.
                // PendingRetry/Applying and cross-generation ownership remain fail-closed.
                bool differentRuntimeNode=!string.Equals(
                    existing.NodeInstanceId,node.nodeId,StringComparison.Ordinal);
                bool sameGeneration=string.Equals(
                    existing.StageGenerationId,stageGenerationId,StringComparison.Ordinal);
                bool released=sameGeneration&&(session.PortfolioRandomGrowth.Cancel(existing)
                    ||(differentRuntimeNode
                        &&session.PortfolioRandomGrowth.AbandonRolledBackRetry(existing)));
                if(!released)
                    return new(PortfolioRandomGrowthDispatchStatus.Failed,
                        "PORTFOLIO_RANDOM_GROWTH_PENDING_CONFLICT:"
                        +session.PortfolioRandomGrowth.State+":"
                        +existing.NodeInstanceId+":"+existing.Identity?.ChoiceId);
            }
            if(!session.PortfolioRandomGrowth.TryBegin(identity,node.nodeId,
                    stageGenerationId,data.definitionFingerprint,out _))
                return new(PortfolioRandomGrowthDispatchStatus.Failed,
                    "PORTFOLIO_RANDOM_GROWTH_CONTEXT_OR_PENDING_INVALID:"
                    +(session.RandomGrowthSession?.RunId.IsValid==true?"run-valid":"run-missing")+":"
                    +(string.IsNullOrWhiteSpace(stageGenerationId)?"generation-missing":"generation-valid")+":"
                    +(string.IsNullOrWhiteSpace(data.definitionFingerprint)?"fingerprint-missing":"fingerprint-valid")+":"
                    +session.PortfolioRandomGrowth.State);
            return new(session.PortfolioRandomGrowth.State==PortfolioRandomGrowthState.PendingRetry
                ?PortfolioRandomGrowthDispatchStatus.PendingRetry:PortfolioRandomGrowthDispatchStatus.RequiresConfirmation);
        }
        public PortfolioRandomGrowthDispatchResult Confirm(RoundNode node,
            Action<RoundNode> completed, Action<StageProgressState> progress)
        {
            PortfolioRandomGrowthPending pending=session?.PortfolioRandomGrowth?.Pending;
            if(pending==null||session.PortfolioRandomGrowthRuntime==null
                ||!session.PortfolioRandomGrowth.TryApplying(pending))
                return new(PortfolioRandomGrowthDispatchStatus.Failed,"PORTFOLIO_RANDOM_GROWTH_PENDING_INVALID");
            string revision=StageAtomicNodeCompletionService.ComputeRevision(session.RuntimeData?.currentGraph);
            StageAtomicNodeCompletionService completion=session.PortfolioRandomGrowthRuntime.NodeCompletion;
            AtomicNodeCompletionToken nodeToken=null;
            if(node==null||completion.TryPrepareCompletion(session,
                    session.PortfolioRandomGrowthRuntime.RunId,pending.StageGenerationId,node.nodeId,
                    node.roundNodeSO?.nodeId,revision,out nodeToken)
                !=AtomicNodeCompletionResult.Prepared
                ||completion.TryCommit(nodeToken)!=AtomicNodeCompletionResult.Committed)
            {session.PortfolioRandomGrowth.TryRetry(pending);return new(PortfolioRandomGrowthDispatchStatus.Failed,"PORTFOLIO_RANDOM_GROWTH_NODE_PREPARE_FAILED");}
            if(!session.PortfolioRandomGrowthRuntime.TryExecute(pending,out _,out string error))
            {completion.TryRollback(nodeToken);session.PortfolioRandomGrowth.TryRetry(pending);return new(PortfolioRandomGrowthDispatchStatus.Failed,error);}
            PortfolioRandomGrowthState terminal=pending.Identity.PayloadKind==RandomGrowthPayloadKind.Decline
                ?PortfolioRandomGrowthState.Declined:PortfolioRandomGrowthState.Succeeded;
            if(!session.PortfolioRandomGrowth.TryCommit(pending,terminal))
                return new(PortfolioRandomGrowthDispatchStatus.Failed,"PORTFOLIO_RANDOM_GROWTH_TERMINAL_CONFLICT");
            completion.PublishDeferred(nodeToken,completed,progress);
            return new(terminal==PortfolioRandomGrowthState.Declined
                ?PortfolioRandomGrowthDispatchStatus.Declined:PortfolioRandomGrowthDispatchStatus.Succeeded);
        }
        private static bool IsB1(string eventId)=>eventId=="event.act1.random_event.21.breath_between_water_drops"
            ||eventId=="event.act1.random_event.22.sleeping_hawk_watch"
            ||eventId=="event.act1.random_event.23.temple_hundred_eight_steps";
    }
}
