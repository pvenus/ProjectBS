using System;
using System.Collections.Generic;
using Progression.RandomGrowth;

namespace Progression
{
    public enum SafeGrowthNodeOnlyFailureResult
    { Prepared, Committed, AlreadyResolved, Busy, InvalidIdentity, Conflict, Aborted, Faulted, CompensationFaulted }

    public sealed class SafeGrowthNodeOnlyFailureToken
    {
        internal SafeGrowthNodeOnlyFailureToken(string id, SafeGrowthNodeOnlyFailureCause cause,
            SafeGrowthInteractionToken interaction, string reservation, SafeGrowthInteractionState restore)
        { TokenId=id; Cause=cause; InteractionToken=interaction; ResultReservationId=reservation; RestoreState=restore; }
        public string TokenId { get; } public SafeGrowthNodeOnlyFailureCause Cause { get; }
        public SafeGrowthInteractionToken InteractionToken { get; } public string ResultReservationId { get; }
        public SafeGrowthInteractionState RestoreState { get; }
    }

    public sealed class SafeGrowthNodeOnlyFailureReceipt
    {
        internal SafeGrowthNodeOnlyFailureReceipt(SafeGrowthNodeOnlyFailureResult result,
            SafeGrowthNodeOnlyFailureToken token, StageEventResultReceipt receipt)
        { Result=result; Token=token; EventReceipt=receipt; }
        public SafeGrowthNodeOnlyFailureResult Result { get; }
        public SafeGrowthNodeOnlyFailureToken Token { get; }
        public StageEventResultReceipt EventReceipt { get; }
    }

    public sealed class SafeGrowthNodeOnlyFailureService
    {
        private readonly object sync=new(); private readonly StageEventResultLedger results;
        private readonly SafeGrowthInteractionOwnership interaction;
        private readonly Dictionary<string,SafeGrowthNodeOnlyFailureToken> prepared=new(StringComparer.Ordinal);
        private readonly Dictionary<string,SafeGrowthNodeOnlyFailureReceipt> finalized=new(StringComparer.Ordinal);
        public SafeGrowthNodeOnlyFailureService(StageEventResultLedger results, SafeGrowthInteractionOwnership interaction)
        { this.results=results??throw new ArgumentNullException(nameof(results));this.interaction=interaction??throw new ArgumentNullException(nameof(interaction)); }

        public SafeGrowthNodeOnlyFailureReceipt Prepare(SafeGrowthNodeOnlyFailureCause cause,
            SafeGrowthInteractionToken token)
        { lock(sync){
            if(!Valid(cause,token))return R(SafeGrowthNodeOnlyFailureResult.InvalidIdentity);
            if(interaction.Token==null||interaction.Token.TokenId!=token.TokenId)return R(SafeGrowthNodeOnlyFailureResult.InvalidIdentity);
            if(interaction.State==SafeGrowthInteractionState.ContentUnavailable)
                return finalized.TryGetValue(cause.StableKey,out var done)?done:R(SafeGrowthNodeOnlyFailureResult.Conflict);
            if(interaction.State==SafeGrowthInteractionState.SafeGrowthGranted||interaction.State==SafeGrowthInteractionState.Declined)
                return R(SafeGrowthNodeOnlyFailureResult.Conflict);
            if(prepared.TryGetValue(cause.StableKey,out var p))return new(SafeGrowthNodeOnlyFailureResult.Busy,p,null);
            var restore=interaction.State; if(interaction.TryBeginApply(token)!=SafeGrowthInteractionResult.Changed)return R(SafeGrowthNodeOnlyFailureResult.Busy);
            var ledgerCause=new StageEventCause(cause.RunId,cause.StageGenerationId,cause.EncounteredNodeInstanceId,
                cause.EventId,SafeGrowthNodeOnlyFailureIds.SemanticClass,cause.FailureReceiptId);
            string tx="safe-node-failure-"+token.TokenId;
            var rr=results.TryReserve(ledgerCause,StageEventChoiceKind.TechnicalFailure,tx,out string reservation,out var existing);
            if(rr==StageEventResultLedgerResult.AlreadyResolved){interaction.TryMarkPendingRetry(token);return new(SafeGrowthNodeOnlyFailureResult.AlreadyResolved,null,existing);}
            if(rr!=StageEventResultLedgerResult.Reserved){interaction.TryMarkPendingRetry(token);return R(SafeGrowthNodeOnlyFailureResult.Faulted);}
            string prepareId="safe-node-failure-prepare-"+CanonicalOfferHash.ComputeHex(
                new[]{SafeGrowthNodeOnlyFailureIds.CauseDomain,cause.StableKey}).Substring(0,32);
            var t=new SafeGrowthNodeOnlyFailureToken(prepareId,cause,token,reservation,restore);
            prepared[cause.StableKey]=t;return new(SafeGrowthNodeOnlyFailureResult.Prepared,t,null);
        }}

        public SafeGrowthNodeOnlyFailureReceipt Finalize(SafeGrowthNodeOnlyFailureToken token)
        { lock(sync){
            if(token==null)return R(SafeGrowthNodeOnlyFailureResult.InvalidIdentity);
            if(finalized.TryGetValue(token.Cause.StableKey,out var done))return done;
            if(!prepared.TryGetValue(token.Cause.StableKey,out var held)||held.TokenId!=token.TokenId)return R(SafeGrowthNodeOnlyFailureResult.InvalidIdentity);
            if(results.TryCommit(token.ResultReservationId,string.Empty,Array.Empty<PartyVitalMutation>(),out var receipt)!=StageEventResultLedgerResult.Committed)
                return R(SafeGrowthNodeOnlyFailureResult.Faulted);
            if(interaction.TryCommitTerminal(token.InteractionToken,SafeGrowthInteractionState.ContentUnavailable)!=SafeGrowthInteractionResult.Changed)
            {results.TryAbortOrRollback(token.ResultReservationId);return R(SafeGrowthNodeOnlyFailureResult.Faulted);}
            var result=new SafeGrowthNodeOnlyFailureReceipt(SafeGrowthNodeOnlyFailureResult.Committed,token,receipt);
            prepared.Remove(token.Cause.StableKey);finalized[token.Cause.StableKey]=result;return result;
        }}

        public SafeGrowthNodeOnlyFailureResult Abort(SafeGrowthNodeOnlyFailureToken token)
        { lock(sync){if(token==null||!prepared.TryGetValue(token.Cause.StableKey,out var held)||held.TokenId!=token.TokenId)return SafeGrowthNodeOnlyFailureResult.InvalidIdentity;
            bool a=results.TryAbortOrRollback(token.ResultReservationId)==StageEventResultLedgerResult.RolledBack;
            bool b=interaction.TryAbortApply(token.InteractionToken,token.RestoreState)==SafeGrowthInteractionResult.Changed;
            if(a&&b){prepared.Remove(token.Cause.StableKey);return SafeGrowthNodeOnlyFailureResult.Aborted;}
            return SafeGrowthNodeOnlyFailureResult.CompensationFaulted;}}
        private static bool Valid(SafeGrowthNodeOnlyFailureCause c,SafeGrowthInteractionToken t)=>c?.IsValid==true&&t?.Key?.IsValid==true
            &&c.EventId==SafeGrowthTransactionIds.EventId&&c.ReservationId==SafeGrowthTransactionIds.ReservationId
            &&c.FailureReceiptId==SafeGrowthNodeOnlyFailureIds.ReceiptId&&c.RunId==t.Key.RunId
            &&c.StageGenerationId==t.Key.StageGenerationId&&c.ReservationId==t.Key.ReservationId
            &&c.EncounteredNodeInstanceId==t.Key.EncounteredNodeInstanceId;
        private static SafeGrowthNodeOnlyFailureReceipt R(SafeGrowthNodeOnlyFailureResult r)=>new(r,null,null);
    }
}
