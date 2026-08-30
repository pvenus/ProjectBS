using System;
using Progression;
using Session;

namespace Stage
{
    public enum SafeGrowthNodeOnlyFailureExecutionResult
    { Succeeded, PrepareFailed, NodePrepareFailed, NodeCommitFailed, FinalizeFailed, CompensationFaulted }
    public sealed class SafeGrowthNodeOnlyFailureCoordinator
    {
        public SafeGrowthNodeOnlyFailureExecutionResult Execute(SafeGrowthNodeOnlyFailureService service,
            SafeGrowthNodeOnlyFailureCause cause, Progression.RandomGrowth.SafeGrowthInteractionToken interaction,
            StageAtomicNodeCompletionService completion, StageSession session, string expectedRevision,
            Action deferredPublish, out SafeGrowthNodeOnlyFailureReceipt receipt)
        {
            receipt=service.Prepare(cause,interaction);if(receipt.Result!=SafeGrowthNodeOnlyFailureResult.Prepared)return SafeGrowthNodeOnlyFailureExecutionResult.PrepareFailed;
            var t=receipt.Token;
            if(completion.TryPrepareCompletion(session,cause.RunId,cause.StageGenerationId,cause.EncounteredNodeInstanceId,cause.NodeId,expectedRevision,out var node)!=AtomicNodeCompletionResult.Prepared)
            {service.Abort(t);return SafeGrowthNodeOnlyFailureExecutionResult.NodePrepareFailed;}
            if(completion.TryCommit(node)!=AtomicNodeCompletionResult.Committed)
            {completion.TryAbort(node);service.Abort(t);return SafeGrowthNodeOnlyFailureExecutionResult.NodeCommitFailed;}
            receipt=service.Finalize(t);
            if(receipt.Result!=SafeGrowthNodeOnlyFailureResult.Committed&&receipt.Result!=SafeGrowthNodeOnlyFailureResult.AlreadyResolved)
            {bool graph=completion.TryRollback(node)==AtomicNodeCompletionResult.RolledBack;bool domain=service.Abort(t)==SafeGrowthNodeOnlyFailureResult.Aborted;return graph&&domain?SafeGrowthNodeOnlyFailureExecutionResult.FinalizeFailed:SafeGrowthNodeOnlyFailureExecutionResult.CompensationFaulted;}
            completion.PublishDeferred(node,_=>deferredPublish?.Invoke());return SafeGrowthNodeOnlyFailureExecutionResult.Succeeded;
        }
    }
}
