using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Character.ProgressionBridge;
using Party;
using Stage;
using Progression.RandomGrowth;

namespace Progression
{
    public enum PortfolioRandomGrowthState
    {
        Offerable, PendingConfirm, Applying, PendingRetry, Succeeded, Declined
    }

    public sealed class PortfolioRandomGrowthPending
    {
        public PortfolioRandomGrowthPending(string tokenId, RandomGrowthEventIdentity identity,
            string nodeInstanceId, string stageGenerationId, string fingerprint)
        { TokenId=tokenId;Identity=identity;NodeInstanceId=nodeInstanceId;StageGenerationId=stageGenerationId;Fingerprint=fingerprint; }
        public string TokenId { get; } public RandomGrowthEventIdentity Identity { get; }
        public string NodeInstanceId { get; } public string Fingerprint { get; }
        public string StageGenerationId { get; }
        public string TerminalKey => string.Join("\n",Identity.EventId,NodeInstanceId);
    }

    public sealed class PortfolioRandomGrowthInteractionOwnership
    {
        private string runId=string.Empty;
        private readonly Dictionary<string,PortfolioRandomGrowthState> terminal=new(StringComparer.Ordinal);
        public PortfolioRandomGrowthPending Pending { get; private set; }
        public PortfolioRandomGrowthState State { get; private set; }=PortfolioRandomGrowthState.Offerable;
        public void ResetForNewRun(string value){runId=value??string.Empty;terminal.Clear();Pending=null;State=PortfolioRandomGrowthState.Offerable;}
        public bool TryBegin(RandomGrowthEventIdentity identity,string node,string stageGenerationId,string fingerprint,out PortfolioRandomGrowthPending pending)
        {
            pending=null;if(string.IsNullOrWhiteSpace(runId)||identity==null||string.IsNullOrWhiteSpace(node)
                ||string.IsNullOrWhiteSpace(stageGenerationId)||string.IsNullOrWhiteSpace(fingerprint))return false;
            string key=identity.EventId+"\n"+node;if(terminal.ContainsKey(key))return false;
            if(Pending!=null){pending=Pending;return Pending.TerminalKey==key&&Pending.Identity.ChoiceId==identity.ChoiceId;}
            string id="portfolio-random-growth-"+ComputeHex(new[]{runId,stageGenerationId,identity.RouteId,node,fingerprint}).Substring(0,32);
            Pending=pending=new PortfolioRandomGrowthPending(id,identity,node,stageGenerationId,fingerprint);
            State=PortfolioRandomGrowthState.PendingConfirm;return true;
        }
        public bool TryApplying(PortfolioRandomGrowthPending value){if(!Owns(value)||State!=PortfolioRandomGrowthState.PendingConfirm&&State!=PortfolioRandomGrowthState.PendingRetry)return false;State=PortfolioRandomGrowthState.Applying;return true;}
        public bool TryRetry(PortfolioRandomGrowthPending value){if(!Owns(value)||State!=PortfolioRandomGrowthState.Applying)return false;State=PortfolioRandomGrowthState.PendingRetry;return true;}
        public bool TryCommit(PortfolioRandomGrowthPending value,PortfolioRandomGrowthState result)
        {if(!Owns(value)||State!=PortfolioRandomGrowthState.Applying||(result!=PortfolioRandomGrowthState.Succeeded&&result!=PortfolioRandomGrowthState.Declined))return false;terminal[value.TerminalKey]=result;State=result;Pending=null;return true;}
        public bool Cancel(PortfolioRandomGrowthPending value){if(!Owns(value)||State!=PortfolioRandomGrowthState.PendingConfirm)return false;Pending=null;State=PortfolioRandomGrowthState.Offerable;return true;}
        public bool IsTerminal(string eventId,string node)=>terminal.ContainsKey(eventId+"\n"+node);
        public bool TryCommitExternal(string eventId,string node)
        {string key=(eventId??string.Empty)+"\n"+(node??string.Empty);if(string.IsNullOrWhiteSpace(eventId)||string.IsNullOrWhiteSpace(node)||terminal.ContainsKey(key)||Pending!=null)return false;terminal[key]=PortfolioRandomGrowthState.Succeeded;return true;}
        private bool Owns(PortfolioRandomGrowthPending value)=>Pending!=null&&value!=null&&Pending.TokenId==value.TokenId;
        private static string ComputeHex(IEnumerable<string> fields)
        {
            StringBuilder encoded=new();
            foreach(string field in fields){string value=field??string.Empty;encoded.Append(Encoding.UTF8.GetByteCount(value).ToString(CultureInfo.InvariantCulture));encoded.Append(':');encoded.Append(value);encoded.Append('|');}
            using SHA256 sha=SHA256.Create();byte[] hash=sha.ComputeHash(Encoding.UTF8.GetBytes(encoded.ToString()));
            StringBuilder result=new(hash.Length*2);foreach(byte value in hash)result.Append(value.ToString("x2",CultureInfo.InvariantCulture));return result.ToString();
        }
    }

    public sealed class PortfolioRandomGrowthRuntime
    {
        private readonly RunProgressionLedger ledger; private readonly StageEventResultLedger results=new();
        private readonly CharacterRuntimePartyVitalGateway vitals; private readonly string runId;
        private readonly RandomGrowthEventTransactionService randomTransaction;
        private readonly SafeGrowthInteractionOwnership safeInteraction;
        private readonly SafeGrowthTransactionService safeTransaction;
        public StageAtomicNodeCompletionService NodeCompletion { get; } = new();
        public PortfolioRandomGrowthRuntime(ProgressionSession session,PartyRuntimeData party)
        {
            ledger=session?.Ledger;runId=session?.RunId.Value??string.Empty;
            if(party!=null)vitals=new CharacterRuntimePartyVitalGateway(party);
            if(ledger!=null&&vitals!=null){randomTransaction=new RandomGrowthEventTransactionService(ledger,results,vitals);safeInteraction=new SafeGrowthInteractionOwnership();if(session.RunId.IsValid)safeInteraction.ResetForNewRun(session.RunId);safeTransaction=new SafeGrowthTransactionService(ledger,results,safeInteraction);}
        }
        public bool IsReady=>randomTransaction!=null&&safeTransaction!=null&&!string.IsNullOrWhiteSpace(runId);
        public string RunId=>runId;
        public bool TryExecute(PortfolioRandomGrowthPending pending,out RandomGrowthEventTransactionReceipt receipt,out string error)
        {
            receipt=null;error=string.Empty;if(!IsReady||pending?.Identity==null){error="PORTFOLIO_RANDOM_GROWTH_RUNTIME_INVALID";return false;}
            RandomGrowthEventIdentity identity=pending.Identity;
            var cause=new StageEventCause(runId,pending.StageGenerationId,pending.NodeInstanceId,identity.EventId,identity.ChoiceId,identity.ResultId);
            if(identity.PayloadKind==RandomGrowthPayloadKind.Safe)return TrySafe(cause,identity,pending.Fingerprint,out receipt,out error);
            if(!vitals.TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster)){error="PORTFOLIO_RANDOM_GROWTH_ROSTER_INVALID";return false;}
            var request=identity.PayloadKind==RandomGrowthPayloadKind.Risk?new ProgressionEarnRequest(identity.SegmentId,
                ProgressionSourceCategory.Random,ProgressionSourceType.RandomEventRisk,identity.SourceId,identity.ResultId):null;
            receipt=randomTransaction.Execute(new RandomGrowthEventCommand(
                cause,identity.PayloadKind==RandomGrowthPayloadKind.Risk?StageEventChoiceKind.Risk:StageEventChoiceKind.Decline,roster,request));
            bool ok=receipt.Result==RandomGrowthEventTransactionResult.Succeeded||receipt.Result==RandomGrowthEventTransactionResult.Declined||receipt.Result==RandomGrowthEventTransactionResult.AlreadyResolved;
            if(!ok)error=receipt.Result.ToString();return ok;
        }
        private bool TrySafe(StageEventCause cause,RandomGrowthEventIdentity identity,string fingerprint,out RandomGrowthEventTransactionReceipt receipt,out string error)
        {
            receipt=null;error=string.Empty;
            var key=new SafeGrowthInteractionKey(runId,cause.StageGenerationId,identity.ReservationId,cause.SlotId);
            SafeGrowthInteractionResult entered=safeInteraction.TryEnterPreconfirm(
                key,identity.ChoiceId,fingerprint,true,out SafeGrowthInteractionToken token);
            if(entered!=SafeGrowthInteractionResult.Changed&&entered!=SafeGrowthInteractionResult.Existing)
            {error="PORTFOLIO_RANDOM_GROWTH_SAFE_INTERACTION_FAILED";return false;}
            var request=new ProgressionEarnRequest(identity.SegmentId,ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventSafe,identity.SourceId,identity.ResultId);
            SafeGrowthTransactionReceipt safe=safeTransaction.Execute(new SafeGrowthTransactionCommand(
                token,SafeGrowthTransactionChoice.Observe,request,2));
            bool ok=safe.Result==SafeGrowthTransactionResult.Succeeded||safe.Result==SafeGrowthTransactionResult.AlreadyResolved;
            if(!ok)error=safe.Result.ToString();return ok;
        }
    }
}
