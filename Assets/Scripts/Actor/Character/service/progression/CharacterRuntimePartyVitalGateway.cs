using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Party;
using Progression;

namespace Character.ProgressionBridge
{
    public enum CharacterRuntimeVitalFailure
    {
        None = 0,
        EmptyRoster,
        NullMember,
        EmptyOwnerId,
        DuplicateOwner,
        MissingCurrentHp,
        DuplicateCurrentHp,
        MissingMaxHp,
        DuplicateMaxHp,
        InvalidHpState,
        MirrorMismatch,
        StaleCurrentHp,
        InvalidPlan,
        ApplyConflict,
        RestoreConflict
    }

    internal enum CharacterVitalGatewayMutationPoint
    {
        BeforeApply,
        BeforeRestore
    }

    public sealed class CharacterRuntimePartyVitalGateway : IPartyVitalMutationGateway
    {
        private readonly PartyRuntimeData party;
        private readonly Action<CharacterVitalGatewayMutationPoint, int> mutationObserver;
        private readonly Dictionary<string, PartyVitalMutationReceipt> receipts = new(StringComparer.Ordinal);
        private readonly HashSet<string> restoredTransactions = new(StringComparer.Ordinal);

        public CharacterRuntimePartyVitalGateway(PartyRuntimeData party)
            : this(party, null)
        {
        }

        internal CharacterRuntimePartyVitalGateway(
            PartyRuntimeData party,
            Action<CharacterVitalGatewayMutationPoint, int> mutationObserver)
        {
            this.party = party ?? throw new ArgumentNullException(nameof(party));
            this.mutationObserver = mutationObserver;
        }

        public CharacterRuntimeVitalFailure LastFailure { get; private set; }

        public bool TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster)
        {
            roster = Array.Empty<PartyVitalSnapshot>();
            if (!TryResolveRoster(out Dictionary<string, CharacterRuntimeData> members))
            {
                return false;
            }

            List<PartyVitalSnapshot> snapshots = new(members.Count);
            foreach (KeyValuePair<string, CharacterRuntimeData> pair in members.OrderBy(
                         item => item.Key, StringComparer.Ordinal))
            {
                CharacterVitalCasResult read = pair.Value.TryReadExactVitalStats(
                    out float currentHp,
                    out float maxHp);
                if (read != CharacterVitalCasResult.Applied)
                {
                    LastFailure = Map(read);
                    return false;
                }

                snapshots.Add(new PartyVitalSnapshot(pair.Key, currentHp, maxHp));
            }

            LastFailure = CharacterRuntimeVitalFailure.None;
            roster = snapshots.AsReadOnly();
            return true;
        }

        public bool TryApply(
            string transactionId,
            PartyVitalCostPlan plan,
            out PartyVitalMutationReceipt receipt)
        {
            receipt = null;
            if (string.IsNullOrWhiteSpace(transactionId) || plan == null || !plan.IsEligible)
            {
                LastFailure = CharacterRuntimeVitalFailure.InvalidPlan;
                return false;
            }

            if (receipts.TryGetValue(transactionId, out receipt))
            {
                if (restoredTransactions.Remove(transactionId))
                {
                    receipts.Remove(transactionId);
                    receipt = null;
                }
                else
                {
                    LastFailure = CharacterRuntimeVitalFailure.None;
                    return true;
                }
            }

            if (!TryResolveRoster(out Dictionary<string, CharacterRuntimeData> members)
                || plan.Mutations.Count != members.Count
                || plan.Mutations.Select(item => item.MemberId).Distinct(StringComparer.Ordinal).Count()
                    != plan.Mutations.Count)
            {
                LastFailure = LastFailure == CharacterRuntimeVitalFailure.None
                    ? CharacterRuntimeVitalFailure.InvalidPlan
                    : LastFailure;
                return false;
            }

            for (int i = 0; i < plan.Mutations.Count; i++)
            {
                PartyVitalMutation mutation = plan.Mutations[i];
                if (mutation == null
                    || !members.TryGetValue(mutation.MemberId ?? string.Empty, out CharacterRuntimeData member)
                    || mutation.Cost <= 0
                    || mutation.After < 1f
                    || !CanonicalFloatBits.AreEqual(mutation.Before - mutation.Cost, mutation.After)
                    || member.TryReadExactVitalStats(out float current, out float max) != CharacterVitalCasResult.Applied
                    || !CanonicalFloatBits.AreEqual(current, mutation.Before)
                    || !CanonicalFloatBits.AreEqual(max, mutation.ExpectedMaxHp)
                    || !TryCalculateCost(max, out int expectedCost)
                    || mutation.Cost != expectedCost)
                {
                    LastFailure = CharacterRuntimeVitalFailure.InvalidPlan;
                    return false;
                }
            }

            List<PartyVitalMutation> applied = new(plan.Mutations.Count);
            try
            {
                for (int i = 0; i < plan.Mutations.Count; i++)
                {
                    mutationObserver?.Invoke(CharacterVitalGatewayMutationPoint.BeforeApply, i);
                    PartyVitalMutation mutation = plan.Mutations[i];
                    CharacterVitalCasResult result = members[mutation.MemberId]
                        .TryCompareExchangeCurrentHp(mutation.Before, mutation.After);
                    if (result != CharacterVitalCasResult.Applied)
                    {
                        LastFailure = result == CharacterVitalCasResult.StaleCurrentHp
                            ? CharacterRuntimeVitalFailure.StaleCurrentHp
                            : CharacterRuntimeVitalFailure.ApplyConflict;
                        receipt = Remember(transactionId, applied);
                        return false;
                    }

                    applied.Add(mutation);
                }
            }
            catch
            {
                LastFailure = CharacterRuntimeVitalFailure.ApplyConflict;
                receipt = Remember(transactionId, applied);
                return false;
            }

            LastFailure = CharacterRuntimeVitalFailure.None;
            receipt = Remember(transactionId, applied);
            return true;
        }

        public bool TryRestore(PartyVitalMutationReceipt receipt)
        {
            if (receipt == null || string.IsNullOrWhiteSpace(receipt.TransactionId))
            {
                LastFailure = CharacterRuntimeVitalFailure.RestoreConflict;
                return false;
            }

            if (restoredTransactions.Contains(receipt.TransactionId))
            {
                LastFailure = CharacterRuntimeVitalFailure.None;
                return true;
            }

            if (!receipts.TryGetValue(receipt.TransactionId, out PartyVitalMutationReceipt owned)
                || !ReceiptEquals(owned, receipt)
                || !TryResolveRoster(out Dictionary<string, CharacterRuntimeData> members))
            {
                LastFailure = CharacterRuntimeVitalFailure.RestoreConflict;
                return false;
            }

            for (int i = owned.Applied.Count - 1; i >= 0; i--)
            {
                PartyVitalMutation mutation = owned.Applied[i];
                if (!members.TryGetValue(mutation.MemberId, out CharacterRuntimeData member)
                    || member.TryReadExactVitalStats(out float current, out _) != CharacterVitalCasResult.Applied
                    || !CanonicalFloatBits.AreEqual(current, mutation.After))
                {
                    LastFailure = CharacterRuntimeVitalFailure.RestoreConflict;
                    return false;
                }
            }

            try
            {
                for (int i = owned.Applied.Count - 1; i >= 0; i--)
                {
                    mutationObserver?.Invoke(CharacterVitalGatewayMutationPoint.BeforeRestore, i);
                    PartyVitalMutation mutation = owned.Applied[i];
                    if (members[mutation.MemberId].TryCompareExchangeCurrentHp(
                            mutation.After,
                            mutation.Before) != CharacterVitalCasResult.Applied)
                    {
                        LastFailure = CharacterRuntimeVitalFailure.RestoreConflict;
                        return false;
                    }
                }
            }
            catch
            {
                LastFailure = CharacterRuntimeVitalFailure.RestoreConflict;
                return false;
            }

            restoredTransactions.Add(receipt.TransactionId);
            LastFailure = CharacterRuntimeVitalFailure.None;
            return true;
        }

        private bool TryResolveRoster(out Dictionary<string, CharacterRuntimeData> members)
        {
            members = new Dictionary<string, CharacterRuntimeData>(StringComparer.Ordinal);
            LastFailure = CharacterRuntimeVitalFailure.None;
            if (party.Members == null || party.Members.Count == 0)
            {
                LastFailure = CharacterRuntimeVitalFailure.EmptyRoster;
                return false;
            }

            for (int i = 0; i < party.Members.Count; i++)
            {
                CharacterRuntimeData member = party.Members[i];
                if (member == null || member.characterSO == null)
                {
                    LastFailure = CharacterRuntimeVitalFailure.NullMember;
                    return false;
                }

                string ownerId = member.characterSO.CharacterId;
                if (string.IsNullOrWhiteSpace(ownerId))
                {
                    LastFailure = CharacterRuntimeVitalFailure.EmptyOwnerId;
                    return false;
                }

                if (!members.TryAdd(ownerId, member))
                {
                    LastFailure = CharacterRuntimeVitalFailure.DuplicateOwner;
                    return false;
                }
            }

            return true;
        }

        private PartyVitalMutationReceipt Remember(string transactionId, IEnumerable<PartyVitalMutation> applied)
        {
            PartyVitalMutationReceipt receipt = new(transactionId, applied);
            receipts[transactionId] = receipt;
            return receipt;
        }

        private static bool ReceiptEquals(PartyVitalMutationReceipt left, PartyVitalMutationReceipt right)
        {
            if (!string.Equals(left.TransactionId, right.TransactionId, StringComparison.Ordinal)
                || left.Applied.Count != right.Applied.Count) return false;
            for (int i = 0; i < left.Applied.Count; i++)
            {
                PartyVitalMutation a = left.Applied[i]; PartyVitalMutation b = right.Applied[i];
                if (!string.Equals(a.MemberId, b.MemberId, StringComparison.Ordinal)
                    || !CanonicalFloatBits.AreEqual(a.Before, b.Before)
                    || !CanonicalFloatBits.AreEqual(a.After, b.After)
                    || !CanonicalFloatBits.AreEqual(a.ExpectedMaxHp, b.ExpectedMaxHp)
                    || a.Cost != b.Cost) return false;
            }
            return true;
        }

        private static CharacterRuntimeVitalFailure Map(CharacterVitalCasResult result) => result switch
        {
            CharacterVitalCasResult.MissingCurrentHp => CharacterRuntimeVitalFailure.MissingCurrentHp,
            CharacterVitalCasResult.DuplicateCurrentHp => CharacterRuntimeVitalFailure.DuplicateCurrentHp,
            CharacterVitalCasResult.MissingMaxHp => CharacterRuntimeVitalFailure.MissingMaxHp,
            CharacterVitalCasResult.DuplicateMaxHp => CharacterRuntimeVitalFailure.DuplicateMaxHp,
            CharacterVitalCasResult.MirrorMismatch => CharacterRuntimeVitalFailure.MirrorMismatch,
            CharacterVitalCasResult.StaleCurrentHp => CharacterRuntimeVitalFailure.StaleCurrentHp,
            _ => CharacterRuntimeVitalFailure.InvalidHpState
        };

        private static bool TryCalculateCost(float maxHp, out int cost)
        {
            cost = 0;
            double rounded = Math.Ceiling((double)maxHp * 0.10d);
            if (rounded < 1d || rounded > int.MaxValue) return false;
            cost = (int)rounded;
            return true;
        }
    }
}
