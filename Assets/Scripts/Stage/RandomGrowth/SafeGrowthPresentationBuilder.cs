using System;
using Progression.RandomGrowth;

namespace Stage
{
    public sealed class SafeGrowthPresentationInput
    {
        public SafeGrowthPresentationInput(SafeGrowthPresentationCopy copy,
            SafeGrowthInteractionState interactionState, bool applying,
            SafeGrowthEligibilitySnapshot eligibility, bool alreadyGranted, bool capReached,
            bool discovered, bool terminalReplay, string nodeId, string interactionTokenId,
            string runtimeRevision, string runtimeFingerprint,
            ConfirmableChoiceDispatchResult observeDispatch,
            ConfirmableChoiceDispatchResult declineDispatch)
        {
            Copy = copy; InteractionState = interactionState; Applying = applying;
            Eligibility = eligibility; AlreadyGranted = alreadyGranted; CapReached = capReached;
            Discovered = discovered; TerminalReplay = terminalReplay; NodeId = nodeId ?? string.Empty;
            InteractionTokenId = interactionTokenId ?? string.Empty;
            RuntimeRevision = runtimeRevision ?? string.Empty;
            RuntimeFingerprint = runtimeFingerprint ?? string.Empty;
            ObserveDispatch = observeDispatch; DeclineDispatch = declineDispatch;
        }
        public SafeGrowthPresentationCopy Copy { get; }
        public SafeGrowthInteractionState InteractionState { get; }
        public bool Applying { get; }
        public SafeGrowthEligibilitySnapshot Eligibility { get; }
        public bool AlreadyGranted { get; }
        public bool CapReached { get; }
        public bool Discovered { get; }
        public bool TerminalReplay { get; }
        public string NodeId { get; }
        public string InteractionTokenId { get; }
        public string RuntimeRevision { get; }
        public string RuntimeFingerprint { get; }
        public ConfirmableChoiceDispatchResult ObserveDispatch { get; }
        public ConfirmableChoiceDispatchResult DeclineDispatch { get; }
    }

    public sealed class SafeGrowthPresentationBuilder
    {
        private const string EventId = "event.act1.random_growth.02.windworn_sword_marks";
        private const string StageNodeId = "stage.act1.random_growth.02.windworn_sword_marks";
        private const string SourcePopupId = "node.act1.random_growth.02.windworn_sword_marks.intro";
        private const string ObserveId = "choice.act1.random_growth.02.windworn_sword_marks.observe_sword_path";
        private const string DeclineId = "choice.act1.random_growth.02.windworn_sword_marks.leave_training_ground";
        private const string GrantedId = "result.act1.random_growth.02.windworn_sword_marks.safe_growth_granted";
        private const string DeclinedId = "result.act1.random_growth.02.windworn_sword_marks.declined";
        private const string RetryId = "result.act1.random_growth.02.windworn_sword_marks.observe_selected_pending_retry";

        public SafeGrowthPresentationSnapshot Build(SafeGrowthPresentationInput input)
        {
            if (input?.Copy == null
                || !KnownCopy(input.Copy)
                || !Confirmable(input.ObserveDispatch) || !Confirmable(input.DeclineDispatch))
                return Snapshot(input, SafeGrowthPresentationState.Invalid,
                    SafeGrowthPresentationDisabledReason.IdentityMismatch, false, false,
                    SafeGrowthPresentationActionIntent.None, "", "", "", "", "", "", "", "", "");

            SafeGrowthPresentationState state = ResolveState(input);
            SafeGrowthPresentationDisabledReason reason = ResolveReason(state);
            bool observe = state == SafeGrowthPresentationState.Discovery
                || state == SafeGrowthPresentationState.Offerable;
            bool decline = observe || state == SafeGrowthPresentationState.Preconfirm;
            string title = input.Copy.Get("discoveryTitle");
            string body = input.Copy.Get("discoveryBody");
            string method = input.Copy.Get("methodLabel") + "\n" + input.Copy.Get("methodSummary");
            string reward = input.Copy.Get("rewardSummary");
            string cap = input.Copy.Get("capNotice");
            string assist = input.Copy.Get("observeAssist");
            string status = string.Empty;
            string cta = input.Copy.Get("observeLabel");
            string cancel = input.Copy.Get("declineLabel");
            SafeGrowthPresentationActionIntent[] actions;

            switch (state)
            {
                case SafeGrowthPresentationState.Discovery:
                case SafeGrowthPresentationState.Offerable:
                    actions = new[] { SafeGrowthPresentationActionIntent.RequestObservePreconfirm,
                        SafeGrowthPresentationActionIntent.ConfirmDecline }; break;
                case SafeGrowthPresentationState.Preconfirm:
                    title = input.Copy.Get("confirmTitle"); body = input.Copy.Get("confirmBody");
                    status = CandidateStatus(input);
                    cta = input.Copy.Get("confirmCta"); cancel = input.Copy.Get("confirmCancel");
                    actions = new[] { SafeGrowthPresentationActionIntent.ConfirmObserve,
                        SafeGrowthPresentationActionIntent.CancelPreconfirm }; break;
                case SafeGrowthPresentationState.DisabledNoCandidate:
                case SafeGrowthPresentationState.DisabledPartyChanged:
                case SafeGrowthPresentationState.DisabledInvalidData:
                    body = input.Copy.Get("candidateZeroDisabled"); assist = input.Copy.Get("candidateZeroReason");
                    cta = input.Copy.Get("candidateZeroRecheckCta"); cancel = input.Copy.Get("declineLabel");
                    decline = true; actions = new[] { SafeGrowthPresentationActionIntent.RecheckEligibility,
                        SafeGrowthPresentationActionIntent.ConfirmDecline }; break;
                case SafeGrowthPresentationState.DisabledAlreadyGranted:
                case SafeGrowthPresentationState.DisabledCapReached:
                    body = input.Copy.Get("alreadyGrantedDisabled"); assist = cap; cta = string.Empty;
                    cancel = input.Copy.Get("declineLabel"); decline = true;
                    actions = new[] { SafeGrowthPresentationActionIntent.ConfirmDecline }; break;
                case SafeGrowthPresentationState.PendingRetry:
                    body = input.Copy.Get("failureBody"); assist = input.Copy.Get("failureAssist");
                    status = CandidateStatus(input);
                    cta = input.Copy.Get("failureRetryCta"); cancel = string.Empty;
                    actions = new[] { SafeGrowthPresentationActionIntent.RetrySameChoice }; break;
                case SafeGrowthPresentationState.TerminalSafeGranted:
                    body = input.Copy.Get("successBody"); status = input.Copy.Get("successStatus");
                    cta = input.Copy.Get("successCta"); cancel = string.Empty;
                    actions = new[] { SafeGrowthPresentationActionIntent.OpenGrowthOffer }; break;
                case SafeGrowthPresentationState.TerminalDeclined:
                    body = input.Copy.Get("declineBody"); status = input.Copy.Get("declineStatus");
                    cta = input.Copy.Get("declineCta"); cancel = string.Empty;
                    actions = new[] { SafeGrowthPresentationActionIntent.ContinueStage }; break;
                case SafeGrowthPresentationState.TerminalReplay:
                    bool granted = input.InteractionState == SafeGrowthInteractionState.SafeGrowthGranted;
                    body = input.Copy.Get(granted ? "successBody" : "declineBody");
                    status = input.Copy.Get(granted ? "successStatus" : "declineStatus");
                    cta = input.Copy.Get(granted ? "successCta" : "declineCta"); cancel = string.Empty;
                    actions = new[] { granted ? SafeGrowthPresentationActionIntent.OpenGrowthOffer
                        : SafeGrowthPresentationActionIntent.ContinueStage }; break;
                case SafeGrowthPresentationState.BusyApplying:
                    title = input.Copy.Get("confirmTitle"); body = input.Copy.Get("busyStatus");
                    status = input.Copy.Get("busyStatus"); observe = false; decline = false;
                    cta = string.Empty; cancel = string.Empty;
                    actions = new[] { SafeGrowthPresentationActionIntent.None }; break;
                default:
                    observe = false; decline = false; body = input.Copy.Get("failureBody");
                    assist = input.Copy.Get("failureAssist"); cta = string.Empty; cancel = string.Empty;
                    actions = new[] { SafeGrowthPresentationActionIntent.None }; break;
            }
            return Snapshot(input, state, reason, observe, decline, actions,
                title, body, method, reward, cap, assist, status, cta, cancel);
        }

        private static bool Confirmable(ConfirmableChoiceDispatchResult result) => result != null
            && result.Kind != ConfirmableChoiceDispatchKind.Disabled
            && result.Kind != ConfirmableChoiceDispatchKind.Unsupported;

        private static bool KnownCopy(SafeGrowthPresentationCopy copy) => copy != null
            && ((copy.SchemaVersion == 1
                    && copy.SemanticDigest == SafeGrowthPresentationCopyResolver.SemanticDigest
                    && copy.DefinitionFingerprint == SafeGrowthPresentationCopyResolver.DefinitionFingerprint)
                || (copy.SchemaVersion == 2
                    && copy.SemanticDigest == SafeGrowthPresentationCopyResolver.V2SemanticDigest
                    && copy.DefinitionFingerprint == SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint));

        private static string CandidateStatus(SafeGrowthPresentationInput input)
        {
            if (input?.Copy?.SchemaVersion != 2) return string.Empty;
            return input.Eligibility?.EligibleCount >= 2
                ? input.Copy.Get("candidateTwoStatus")
                : input.Eligibility?.EligibleCount == 1
                    ? input.Copy.Get("candidateOneStatus") : string.Empty;
        }

        private static SafeGrowthPresentationState ResolveState(SafeGrowthPresentationInput input)
        {
            if (input.TerminalReplay) return SafeGrowthPresentationState.TerminalReplay;
            if (input.Applying) return SafeGrowthPresentationState.BusyApplying;
            if (input.InteractionState == SafeGrowthInteractionState.SafeGrowthGranted)
                return SafeGrowthPresentationState.TerminalSafeGranted;
            if (input.InteractionState == SafeGrowthInteractionState.Declined)
                return SafeGrowthPresentationState.TerminalDeclined;
            if (input.InteractionState == SafeGrowthInteractionState.ObserveSelectedPendingRetry)
                return SafeGrowthPresentationState.PendingRetry;
            if (input.InteractionState == SafeGrowthInteractionState.Preconfirm)
                return SafeGrowthPresentationState.Preconfirm;
            if (input.AlreadyGranted) return SafeGrowthPresentationState.DisabledAlreadyGranted;
            if (input.CapReached) return SafeGrowthPresentationState.DisabledCapReached;
            if (input.Eligibility == null) return SafeGrowthPresentationState.DisabledInvalidData;
            if (input.Eligibility.Status == SafeGrowthEligibilityStatus.NoCandidate)
                return SafeGrowthPresentationState.DisabledNoCandidate;
            if (input.Eligibility.Status == SafeGrowthEligibilityStatus.Stale
                || (!string.IsNullOrWhiteSpace(input.RuntimeFingerprint)
                    && !string.Equals(input.RuntimeFingerprint, input.Eligibility.Fingerprint, StringComparison.Ordinal)))
                return SafeGrowthPresentationState.DisabledPartyChanged;
            if (input.Eligibility.Status != SafeGrowthEligibilityStatus.Eligible)
                return SafeGrowthPresentationState.DisabledInvalidData;
            return input.Discovered ? SafeGrowthPresentationState.Offerable : SafeGrowthPresentationState.Discovery;
        }

        private static SafeGrowthPresentationDisabledReason ResolveReason(SafeGrowthPresentationState state) => state switch
        {
            SafeGrowthPresentationState.DisabledNoCandidate => SafeGrowthPresentationDisabledReason.NoCandidate,
            SafeGrowthPresentationState.DisabledAlreadyGranted => SafeGrowthPresentationDisabledReason.AlreadyGranted,
            SafeGrowthPresentationState.DisabledCapReached => SafeGrowthPresentationDisabledReason.CapReached,
            SafeGrowthPresentationState.DisabledInvalidData => SafeGrowthPresentationDisabledReason.InvalidData,
            SafeGrowthPresentationState.DisabledPartyChanged => SafeGrowthPresentationDisabledReason.PartyChanged,
            SafeGrowthPresentationState.BusyApplying => SafeGrowthPresentationDisabledReason.Busy,
            _ => SafeGrowthPresentationDisabledReason.None
        };

        private static SafeGrowthPresentationSnapshot Snapshot(SafeGrowthPresentationInput input,
            SafeGrowthPresentationState state, SafeGrowthPresentationDisabledReason reason,
            bool observe, bool decline, SafeGrowthPresentationActionIntent action,
            string title, string body, string method, string reward, string cap,
            string assist, string status, string cta, string cancel) => Snapshot(input, state, reason,
                observe, decline, new[] { action }, title, body, method, reward, cap, assist, status, cta, cancel);

        private static SafeGrowthPresentationSnapshot Snapshot(SafeGrowthPresentationInput input,
            SafeGrowthPresentationState state, SafeGrowthPresentationDisabledReason reason,
            bool observe, bool decline, SafeGrowthPresentationActionIntent[] actions,
            string title, string body, string method, string reward, string cap,
            string assist, string status, string cta, string cancel)
        {
            int eligible = input?.Eligibility?.EligibleCount ?? 0;
            string result = state switch
            {
                SafeGrowthPresentationState.PendingRetry => RetryId,
                SafeGrowthPresentationState.TerminalSafeGranted => GrantedId,
                SafeGrowthPresentationState.TerminalDeclined => DeclinedId,
                SafeGrowthPresentationState.TerminalReplay => input?.InteractionState == SafeGrowthInteractionState.SafeGrowthGranted ? GrantedId : DeclinedId,
                _ => string.Empty
            };
            return new SafeGrowthPresentationSnapshot(state, reason, EventId, StageNodeId,
                SourcePopupId, input?.NodeId,
                ObserveId, DeclineId, result, input?.InteractionTokenId,
                input?.RuntimeRevision, input?.RuntimeFingerprint, input?.Copy,
                observe, decline, eligible, actions, title, body, method, reward, cap,
                assist, status, cta, cancel, input?.Copy?.Get("candidateZeroRecheckCta"));
        }
    }
}
