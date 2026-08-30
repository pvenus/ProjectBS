using System;

namespace Progression.RandomGrowth
{
    public enum SafeGrowthInteractionState
    {
        Offerable = 0,
        Preconfirm = 10,
        ObserveSelectedPendingRetry = 20,
        SafeGrowthGranted = 30,
        Declined = 40,
        ContentUnavailable = 50
    }

    public enum SafeGrowthInteractionResult
    {
        Changed = 0,
        Existing = 10,
        Busy = 20,
        AlreadyTerminal = 30,
        Rejected = 40,
        Faulted = 50
    }

    public sealed class SafeGrowthInteractionKey
    {
        public SafeGrowthInteractionKey(string runId, string stageGenerationId,
            string reservationId, string encounteredNodeInstanceId)
        {
            RunId = runId;
            StageGenerationId = stageGenerationId;
            ReservationId = reservationId;
            EncounteredNodeInstanceId = encounteredNodeInstanceId;
        }

        public string RunId { get; }
        public string StageGenerationId { get; }
        public string ReservationId { get; }
        public string EncounteredNodeInstanceId { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(RunId)
            && !string.IsNullOrWhiteSpace(StageGenerationId)
            && !string.IsNullOrWhiteSpace(ReservationId)
            && !string.IsNullOrWhiteSpace(EncounteredNodeInstanceId);
        public string StableKey => string.Join("\n", RunId, StageGenerationId,
            ReservationId, EncounteredNodeInstanceId);
    }

    public sealed class SafeGrowthInteractionToken
    {
        internal SafeGrowthInteractionToken(string tokenId, SafeGrowthInteractionKey key,
            string choiceId, string definitionFingerprint)
        {
            TokenId = tokenId;
            Key = key;
            ChoiceId = choiceId;
            DefinitionFingerprint = definitionFingerprint;
        }

        public string TokenId { get; }
        public SafeGrowthInteractionKey Key { get; }
        public string ChoiceId { get; }
        public string DefinitionFingerprint { get; }
    }

    public interface ISafeGrowthInteractionGateway
    {
        SafeGrowthInteractionResult TryBeginApply(SafeGrowthInteractionToken token);
        SafeGrowthInteractionResult TryMarkPendingRetry(SafeGrowthInteractionToken token);
        SafeGrowthInteractionResult TryCommitTerminal(
            SafeGrowthInteractionToken token, SafeGrowthInteractionState terminalState);
    }

    public sealed class SafeGrowthInteractionOwnership : ISafeGrowthInteractionGateway
    {
        private ProgressionRunId runId;
        private Entry entry;

        public SafeGrowthInteractionState State => entry?.State ?? SafeGrowthInteractionState.Offerable;
        public SafeGrowthInteractionToken Token => entry?.Token;
        public bool IsApplying => entry?.Applying == true;

        public void ResetForNewRun(ProgressionRunId newRunId)
        {
            if (!newRunId.IsValid) throw new ArgumentException("Run ID is required.", nameof(newRunId));
            runId = newRunId;
            entry = null;
        }

        public void Clear()
        {
            runId = default;
            entry = null;
        }

        public SafeGrowthInteractionResult TryEnterPreconfirm(
            SafeGrowthInteractionKey key, string choiceId, string definitionFingerprint,
            bool hasCandidate, out SafeGrowthInteractionToken token)
        {
            token = null;
            if (!runId.IsValid || key == null || !key.IsValid
                || !string.Equals(key.RunId, runId.Value, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(choiceId)
                || string.IsNullOrWhiteSpace(definitionFingerprint)
                || !hasCandidate)
                return SafeGrowthInteractionResult.Rejected;

            if (entry != null)
            {
                token = entry.Token;
                if (!Same(key, choiceId, definitionFingerprint)) return SafeGrowthInteractionResult.Rejected;
                if (IsTerminal(entry.State)) return SafeGrowthInteractionResult.AlreadyTerminal;
                if (entry.Applying) return SafeGrowthInteractionResult.Busy;
                if (entry.State == SafeGrowthInteractionState.Preconfirm
                    || entry.State == SafeGrowthInteractionState.ObserveSelectedPendingRetry)
                    return SafeGrowthInteractionResult.Existing;
            }

            token = new SafeGrowthInteractionToken(
                "safe-interaction-" + CanonicalOfferHash.ComputeHex(new[] { key.StableKey, choiceId, definitionFingerprint }).Substring(0, 32),
                key, choiceId, definitionFingerprint);
            entry = new Entry(token, SafeGrowthInteractionState.Preconfirm);
            return SafeGrowthInteractionResult.Changed;
        }

        public SafeGrowthInteractionResult TryCancelPreconfirm(SafeGrowthInteractionToken token)
        {
            if (!Owns(token) || entry.Applying) return SafeGrowthInteractionResult.Rejected;
            if (entry.State != SafeGrowthInteractionState.Preconfirm) return SafeGrowthInteractionResult.Rejected;
            entry.State = SafeGrowthInteractionState.Offerable;
            entry = null;
            return SafeGrowthInteractionResult.Changed;
        }

        public SafeGrowthInteractionResult TryBeginApply(SafeGrowthInteractionToken token)
        {
            if (!Owns(token)) return SafeGrowthInteractionResult.Rejected;
            if (IsTerminal(entry.State)) return SafeGrowthInteractionResult.AlreadyTerminal;
            if (entry.Applying) return SafeGrowthInteractionResult.Busy;
            if (entry.State != SafeGrowthInteractionState.Preconfirm
                && entry.State != SafeGrowthInteractionState.ObserveSelectedPendingRetry)
                return SafeGrowthInteractionResult.Rejected;
            entry.Applying = true;
            return SafeGrowthInteractionResult.Changed;
        }

        public SafeGrowthInteractionResult TryMarkPendingRetry(SafeGrowthInteractionToken token)
        {
            if (!Owns(token) || IsTerminal(entry.State)) return SafeGrowthInteractionResult.Rejected;
            entry.Applying = false;
            entry.State = SafeGrowthInteractionState.ObserveSelectedPendingRetry;
            return SafeGrowthInteractionResult.Changed;
        }

        public SafeGrowthInteractionResult TryAbortApply(
            SafeGrowthInteractionToken token, SafeGrowthInteractionState restoreState)
        {
            if (!Owns(token) || IsTerminal(entry.State) || !entry.Applying
                || (restoreState != SafeGrowthInteractionState.Preconfirm
                    && restoreState != SafeGrowthInteractionState.ObserveSelectedPendingRetry))
                return SafeGrowthInteractionResult.Rejected;
            entry.Applying = false;
            entry.State = restoreState;
            return SafeGrowthInteractionResult.Changed;
        }

        public SafeGrowthInteractionResult TryCommitTerminal(
            SafeGrowthInteractionToken token, SafeGrowthInteractionState terminalState)
        {
            if (!Owns(token)) return SafeGrowthInteractionResult.Rejected;
            if (IsTerminal(entry.State))
                return entry.State == terminalState ? SafeGrowthInteractionResult.Existing : SafeGrowthInteractionResult.AlreadyTerminal;
            if (!entry.Applying || !IsTerminal(terminalState)) return SafeGrowthInteractionResult.Rejected;
            entry.Applying = false;
            entry.State = terminalState;
            return SafeGrowthInteractionResult.Changed;
        }

        private bool Owns(SafeGrowthInteractionToken token) => entry != null && token != null
            && string.Equals(entry.Token.TokenId, token.TokenId, StringComparison.Ordinal);
        private bool Same(SafeGrowthInteractionKey key, string choiceId, string fingerprint) =>
            string.Equals(entry.Token.Key.StableKey, key.StableKey, StringComparison.Ordinal)
            && string.Equals(entry.Token.ChoiceId, choiceId, StringComparison.Ordinal)
            && string.Equals(entry.Token.DefinitionFingerprint, fingerprint, StringComparison.Ordinal);
        private static bool IsTerminal(SafeGrowthInteractionState state) =>
            state == SafeGrowthInteractionState.SafeGrowthGranted
            || state == SafeGrowthInteractionState.Declined
            || state == SafeGrowthInteractionState.ContentUnavailable;

        private sealed class Entry
        {
            public Entry(SafeGrowthInteractionToken token, SafeGrowthInteractionState state)
            { Token = token; State = state; }
            public SafeGrowthInteractionToken Token;
            public SafeGrowthInteractionState State;
            public bool Applying;
        }
    }
}
