using System;

namespace Stage
{
    public sealed class PortfolioAtomicTransactionCoordinator
    {
        public bool TryApply(
            PortfolioOutcomeOwnership ownership,
            PortfolioAtomicTransactionReceipt receipt,
            Func<bool> applyEffect,
            Action rollbackEffect,
            out Action rollback,
            out string error)
        {
            rollback = null;
            error = string.Empty;
            if (ownership == null || receipt?.HasIdentity != true
                || applyEffect == null || rollbackEffect == null)
            {
                error = "PORTFOLIO_ATOMIC_ARGUMENT_INVALID";
                return false;
            }
            if (ownership.IsResolved(receipt.transactionId)) return true;
            if (!ownership.TryReserveTransaction(receipt))
            {
                error = "PORTFOLIO_ATOMIC_RESERVATION_CONFLICT";
                return false;
            }
            if (!applyEffect())
            {
                ownership.TryReleaseTransaction(receipt);
                error = "PORTFOLIO_ATOMIC_EFFECT_FAILED";
                return false;
            }
            receipt.effectApplied = true;
            receipt.terminalCommitted = true;
            rollback = () =>
            {
                receipt.terminalCommitted = false;
                receipt.effectApplied = false;
                rollbackEffect();
                ownership.TryReleaseTransaction(receipt);
            };
            return true;
        }

        public bool TryCommit(
            PortfolioOutcomeOwnership ownership,
            PortfolioAtomicTransactionReceipt receipt,
            out string error)
        {
            error = string.Empty;
            if (ownership?.TryCommitTransaction(receipt) == true) return true;
            error = "PORTFOLIO_ATOMIC_COMMIT_CONFLICT";
            return false;
        }
    }
}
