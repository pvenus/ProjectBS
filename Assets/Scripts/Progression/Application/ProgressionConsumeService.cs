using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public sealed class ProgressionConsumeService
    {
        private readonly RunProgressionLedger ledger;
        private readonly IProgressionSkillLevelGateway gateway;
        private readonly object synchronization = new();
        private readonly HashSet<string> inFlightOpportunityIds = new(StringComparer.Ordinal);

        public ProgressionConsumeService(
            RunProgressionLedger ledger,
            IProgressionSkillLevelGateway gateway)
        {
            this.ledger = ledger ?? throw new ArgumentNullException(nameof(ledger));
            this.gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public ProgressionApplyResult TryApplyCandidate(
            ProgressionApplyCandidateCommand command)
        {
            lock (synchronization)
            {
                if (command == null || !command.IsValid)
                {
                    return Result(ProgressionApplyResultCode.RejectedInvalidCommand, null);
                }

                if (inFlightOpportunityIds.Contains(command.OpportunityId))
                {
                    ledger.TryGetOpportunity(command.OpportunityId, out ProgressionOpportunitySnapshot busy);
                    return Result(ProgressionApplyResultCode.Busy, busy);
                }

                if (!ledger.TryGetOpportunity(
                        command.OpportunityId,
                        out ProgressionOpportunitySnapshot current))
                {
                    return Result(ProgressionApplyResultCode.RejectedNotFound, null);
                }

                ProgressionApplyResult validation = Validate(command, current);
                if (validation != null)
                {
                    return validation;
                }

                inFlightOpportunityIds.Add(command.OpportunityId);
                try
                {
                    return ApplyReserved(command, current);
                }
                finally
                {
                    inFlightOpportunityIds.Remove(command.OpportunityId);
                }
            }
        }

        private ProgressionApplyResult ApplyReserved(
            ProgressionApplyCandidateCommand command,
            ProgressionOpportunitySnapshot current)
        {
            ProgressionConsumeResult reservation = ledger.TryReserveConsume(
                current.OpportunityId,
                command.ExpectedLedgerRevision,
                out ProgressionOpportunitySnapshot consuming);
            if (reservation != ProgressionConsumeResult.Reserved)
            {
                return Result(MapReservationFailure(reservation), consuming);
            }

            ProgressionSkillMutationKey key = new(
                command.OwnerCharacterId,
                command.SkillInstanceId,
                command.CanonicalSkillId);
            SkillLevelMutationResult gatewayResult;
            ProgressionSkillLevelMutation mutation;
            try
            {
                gatewayResult = gateway.TryApplyExactOne(
                    key,
                    command.ExpectedLevel,
                    out mutation);
            }
            catch
            {
                gatewayResult = SkillLevelMutationResult.Faulted;
                mutation = null;
            }

            if (gatewayResult != SkillLevelMutationResult.Applied
                || mutation == null
                || mutation.PreviousLevel != command.ExpectedLevel
                || mutation.AppliedLevel != command.ExpectedLevel + 1
                || !mutation.Key.Equals(key))
            {
                bool gatewayRestored = EnsureGatewayOldLevel(
                    key,
                    command.ExpectedLevel,
                    mutation);

                ProgressionConsumeResult rollback = ledger.TryRollbackConsume(
                    consuming.OpportunityId,
                    consuming.ConsumeReservationId,
                    out ProgressionOpportunitySnapshot restored);
                if (!gatewayRestored || rollback != ProgressionConsumeResult.RolledBack)
                {
                    return Result(ProgressionApplyResultCode.CompensationFaulted, restored);
                }

                return Result(
                    gatewayResult == SkillLevelMutationResult.Faulted
                        ? ProgressionApplyResultCode.GatewayFaulted
                        : ProgressionApplyResultCode.GatewayRejected,
                    restored);
            }

            ProgressionApplyReceipt receipt = new(
                command.OpportunityId,
                command.Fingerprint,
                command.OwnerCharacterId,
                command.SkillInstanceId,
                command.CanonicalSkillId,
                mutation.PreviousLevel,
                mutation.AppliedLevel,
                mutation.MutationId);
            ProgressionConsumeResult committed = ledger.TryCommitConsume(
                consuming.OpportunityId,
                consuming.ConsumeReservationId,
                receipt,
                out ProgressionOpportunitySnapshot applied);
            if (committed == ProgressionConsumeResult.Applied)
            {
                return new ProgressionApplyResult(
                    ProgressionApplyResultCode.Applied,
                    applied,
                    receipt);
            }

            bool levelRestored = EnsureGatewayOldLevel(
                key,
                command.ExpectedLevel,
                mutation);

            ProgressionConsumeResult ledgerRestored = ledger.TryRollbackConsume(
                consuming.OpportunityId,
                consuming.ConsumeReservationId,
                out ProgressionOpportunitySnapshot rolledBack);
            bool restoredOld = levelRestored
                && ledgerRestored == ProgressionConsumeResult.RolledBack;
            return Result(
                restoredOld
                    ? ProgressionApplyResultCode.LedgerCommitFailedRestored
                    : ProgressionApplyResultCode.CompensationFaulted,
                rolledBack);
        }

        private ProgressionApplyResult Validate(
            ProgressionApplyCandidateCommand command,
            ProgressionOpportunitySnapshot current)
        {
            if (current.State == ProgressionOpportunityState.Applied)
            {
                if (!ReceiptMatches(current.AppliedReceipt, command))
                {
                    return Result(ProgressionApplyResultCode.RejectedCandidate, current);
                }

                return new ProgressionApplyResult(
                    ProgressionApplyResultCode.AlreadyApplied,
                    current,
                    current.AppliedReceipt);
            }

            if (current.State == ProgressionOpportunityState.Consuming)
            {
                return Result(ProgressionApplyResultCode.Busy, current);
            }

            if (current.State != ProgressionOpportunityState.Pending || current.Offer == null)
            {
                return Result(ProgressionApplyResultCode.RejectedState, current);
            }

            if (current.Revision != command.ExpectedLedgerRevision)
            {
                return Result(ProgressionApplyResultCode.RejectedRevision, current);
            }

            if (!string.Equals(current.Offer.Fingerprint, command.Fingerprint, StringComparison.Ordinal))
            {
                return Result(ProgressionApplyResultCode.RejectedFingerprint, current);
            }

            ProgressionSkillCandidateSnapshot candidate = current.Offer.Candidates.SingleOrDefault(value =>
                string.Equals(value.OwnerCharacterId, command.OwnerCharacterId, StringComparison.Ordinal)
                && string.Equals(value.SkillInstanceId, command.SkillInstanceId, StringComparison.Ordinal)
                && string.Equals(value.CanonicalSkillId, command.CanonicalSkillId, StringComparison.Ordinal));
            if (candidate == null)
            {
                return Result(ProgressionApplyResultCode.RejectedCandidate, current);
            }

            if (candidate.CurrentLevel != command.ExpectedLevel)
            {
                return Result(ProgressionApplyResultCode.RejectedExpectedLevel, current);
            }

            ProgressionSkillMutationKey key = new(
                command.OwnerCharacterId,
                command.SkillInstanceId,
                command.CanonicalSkillId);
            int level;
            try
            {
                if (!gateway.TryGetCurrentLevel(key, out level)
                    || level != command.ExpectedLevel)
                {
                    return Result(ProgressionApplyResultCode.RejectedExpectedLevel, current);
                }
            }
            catch
            {
                return Result(ProgressionApplyResultCode.GatewayFaulted, current);
            }

            return null;
        }

        private static ProgressionApplyResultCode MapReservationFailure(
            ProgressionConsumeResult result)
        {
            return result switch
            {
                ProgressionConsumeResult.RejectedRevision => ProgressionApplyResultCode.RejectedRevision,
                ProgressionConsumeResult.RejectedCap => ProgressionApplyResultCode.RejectedCap,
                ProgressionConsumeResult.RejectedNotFound => ProgressionApplyResultCode.RejectedNotFound,
                ProgressionConsumeResult.RejectedState => ProgressionApplyResultCode.RejectedState,
                _ => ProgressionApplyResultCode.GatewayFaulted
            };
        }

        private static ProgressionApplyResult Result(
            ProgressionApplyResultCode code,
            ProgressionOpportunitySnapshot opportunity) =>
            new(code, opportunity, opportunity?.AppliedReceipt);

        private bool EnsureGatewayOldLevel(
            ProgressionSkillMutationKey key,
            int expectedLevel,
            ProgressionSkillLevelMutation mutation)
        {
            try
            {
                if (gateway.TryGetCurrentLevel(key, out int currentLevel)
                    && currentLevel == expectedLevel)
                {
                    return true;
                }

                if (mutation != null)
                {
                    if (gateway.TryRollback(mutation))
                    {
                        return true;
                    }
                }

                return gateway.TryRestoreExactLevel(
                    key,
                    expectedLevel + 1,
                    expectedLevel);
            }
            catch
            {
                return false;
            }
        }

        private static bool ReceiptMatches(
            ProgressionApplyReceipt receipt,
            ProgressionApplyCandidateCommand command) =>
            receipt != null
            && string.Equals(receipt.OpportunityId, command.OpportunityId, StringComparison.Ordinal)
            && string.Equals(receipt.Fingerprint, command.Fingerprint, StringComparison.Ordinal)
            && string.Equals(receipt.OwnerCharacterId, command.OwnerCharacterId, StringComparison.Ordinal)
            && string.Equals(receipt.SkillInstanceId, command.SkillInstanceId, StringComparison.Ordinal)
            && string.Equals(receipt.CanonicalSkillId, command.CanonicalSkillId, StringComparison.Ordinal)
            && receipt.PreviousLevel == command.ExpectedLevel;
    }
}
