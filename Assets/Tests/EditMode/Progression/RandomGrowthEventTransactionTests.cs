using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Progression.Tests
{
    public sealed class RandomGrowthEventTransactionTests
    {
        [TestCase(100, 10)] [TestCase(101, 11)] [TestCase(999, 100)]
        public void CostIsCeilingTenPercent(int maxHp, int expected)
        {
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(new[] { new PartyVitalSnapshot("a", maxHp, maxHp) });
            Assert.That(plan.Mutations.Single().Cost, Is.EqualTo(expected));
        }

        [Test]
        public void FractionalVitalsPreserveBitsAndOnlyCostIsCeiled()
        {
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(new[] { S("a", 499.75f, 500f) });
            Assert.That(plan.IsEligible, Is.True);
            Assert.That(plan.Mutations.Single().Cost, Is.EqualTo(50));
            Assert.That(CanonicalFloatBits.AreEqual(plan.Mutations.Single().After, 449.75f), Is.True);
            Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("b", 101.2f, 101.2f) })
                .Mutations.Single().Cost, Is.EqualTo(11));
            Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("c", 2.25f, 10f) }).IsEligible, Is.True);
            Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("d", 1.5f, 10f) }).IsEligible, Is.False);
        }

        [Test]
        public void InvalidAndSubnormalHpStatesFailClosedWithoutChangingBits()
        {
            float positiveSubnormal = BitConverter.Int32BitsToSingle(1);
            float negativeZero = BitConverter.Int32BitsToSingle(unchecked((int)0x80000000));
            Assert.That(CanonicalFloatBits.AreEqual(0f, negativeZero), Is.False);
            float[] invalidCurrent = { float.NaN, float.PositiveInfinity, float.NegativeInfinity, 0f, negativeZero, -1f };
            foreach (float current in invalidCurrent)
                Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("a", current, 100f) }).Reason,
                    Is.EqualTo("InvalidHpState"));
            Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("a", positiveSubnormal, 100f) }).IsEligible, Is.False);
            Assert.That(CanonicalFloatBits.GetBits(positiveSubnormal), Is.EqualTo(1));
            Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("a", 10f, positiveSubnormal) }).Reason,
                Is.EqualTo("InvalidHpState"));
            Assert.That(PartyVitalCostPolicy.Evaluate(new[] { S("a", 10f, float.MaxValue) }).Reason,
                Is.EqualTo("InvalidHpState"));
            FakeVitals invalidGateway = new(S("a", float.NaN, 100f));
            Assert.That(Service(invalidGateway).Execute(Risk(invalidGateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.InvalidHpState));
            Assert.That(invalidGateway.ApplyCalls, Is.Zero);
        }

        [Test]
        public void CurrentAboveMaxIsAllowedWithoutClamp()
        {
            PartyVitalCostPlan plan = PartyVitalCostPolicy.Evaluate(new[] { S("a", 120.5f, 100f) });
            Assert.That(plan.IsEligible, Is.True);
            Assert.That(plan.Mutations.Single().After, Is.EqualTo(110.5f));
        }

        [Test]
        public void HpOneBoundaryBlocksWholePartyBeforeGatewayMutation()
        {
            FakeVitals gateway = new(S("a", 11, 100), S("b", 1, 10));
            RandomGrowthEventTransactionReceipt receipt = Service(gateway).Execute(Risk(gateway.Roster));
            Assert.That(receipt.Result, Is.EqualTo(RandomGrowthEventTransactionResult.Ineligible));
            Assert.That(gateway.ApplyCalls, Is.Zero);
        }

        [Test]
        public void RiskCommitsAllCostsResultAndOnePendingOpportunity()
        {
            FakeVitals gateway = new(S("a", 100, 100), S("b", 51, 101));
            RunProgressionLedger ledger = Ledger(); StageEventResultLedger results = new();
            RandomGrowthEventTransactionReceipt receipt = Service(gateway, ledger, results).Execute(Risk(gateway.Roster));
            Assert.That(receipt.Result, Is.EqualTo(RandomGrowthEventTransactionResult.Succeeded));
            Assert.That(gateway.Current("a"), Is.EqualTo(90));
            Assert.That(gateway.Current("b"), Is.EqualTo(40));
            Assert.That(ledger.Count, Is.EqualTo(1));
            Assert.That(receipt.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(results.CommittedCount, Is.EqualTo(1));
        }

        [Test]
        public void SameCauseOneHundredTimesChargesAndGrantsOnce()
        {
            FakeVitals gateway = new(S("a", 200, 200)); RunProgressionLedger ledger = Ledger();
            RandomGrowthEventTransactionService service = Service(gateway, ledger, new StageEventResultLedger());
            RandomGrowthEventCommand command = Risk(gateway.Roster);
            int success = 0, duplicate = 0;
            for (int i = 0; i < 100; i++)
            {
                RandomGrowthEventTransactionResult result = service.Execute(command).Result;
                success += result == RandomGrowthEventTransactionResult.Succeeded ? 1 : 0;
                duplicate += result == RandomGrowthEventTransactionResult.AlreadyResolved ? 1 : 0;
            }
            Assert.That(success, Is.EqualTo(1)); Assert.That(duplicate, Is.EqualTo(99));
            Assert.That(gateway.ApplyCalls, Is.EqualTo(1)); Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void DeclineOneHundredTimesHasNoVitalOrGrowthMutation()
        {
            FakeVitals gateway = new(S("a", 100, 100)); RunProgressionLedger ledger = Ledger();
            StageEventResultLedger results = new(); RandomGrowthEventTransactionService service = Service(gateway, ledger, results);
            RandomGrowthEventCommand command = new(Cause("leave"), StageEventChoiceKind.Decline, Array.Empty<PartyVitalSnapshot>());
            Assert.That(service.Execute(command).Result, Is.EqualTo(RandomGrowthEventTransactionResult.Declined));
            for (int i = 1; i < 100; i++) Assert.That(service.Execute(command).Result, Is.EqualTo(RandomGrowthEventTransactionResult.AlreadyResolved));
            Assert.That(gateway.ApplyCalls, Is.Zero); Assert.That(ledger.Count, Is.Zero); Assert.That(results.CommittedCount, Is.EqualTo(1));
        }

        [Test]
        public void RiskAndDeclineAreMutuallyExclusiveTerminalChoices()
        {
            FakeVitals gateway = new(S("a", 100, 100));
            RandomGrowthEventTransactionService service = Service(gateway);
            Assert.That(service.Execute(Risk(gateway.Roster)).Result, Is.EqualTo(RandomGrowthEventTransactionResult.Succeeded));
            RandomGrowthEventCommand decline = new(Cause("leave"), StageEventChoiceKind.Decline, Array.Empty<PartyVitalSnapshot>());
            Assert.That(service.Execute(decline).Result, Is.EqualTo(RandomGrowthEventTransactionResult.AlreadyResolved));
            Assert.That(gateway.ApplyCalls, Is.EqualTo(1));
        }

        [Test]
        public void EmptyDuplicateOrChangedRosterMutatesNothing()
        {
            FakeVitals empty = new(); Assert.That(Service(empty).Execute(Risk(empty.Roster)).Result, Is.EqualTo(RandomGrowthEventTransactionResult.InvalidRoster));
            FakeVitals duplicate = new(S("a", 100, 100), S("a", 100, 100));
            Assert.That(Service(duplicate).Execute(Risk(duplicate.Roster)).Result, Is.EqualTo(RandomGrowthEventTransactionResult.InvalidRoster));
            FakeVitals changed = new(S("a", 99, 100));
            Assert.That(Service(changed).Execute(Risk(new[] { S("a", 100, 100) })).Result, Is.EqualTo(RandomGrowthEventTransactionResult.StaleRoster));
            Assert.That(empty.ApplyCalls + duplicate.ApplyCalls + changed.ApplyCalls, Is.Zero);
        }

        [Test]
        public void PartialVitalFailureRestoresEveryAppliedMember()
        {
            FakeVitals gateway = new(S("a", 99.75f, 100f), S("b", 100.25f, 100f)) { FailAfterAppliedCount = 1 };
            RunProgressionLedger ledger = Ledger(); StageEventResultLedger results = new();
            Assert.That(Service(gateway, ledger, results).Execute(Risk(gateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.VitalMutationFailed));
            Assert.That(CanonicalFloatBits.AreEqual(gateway.Current("a"), 99.75f), Is.True);
            Assert.That(CanonicalFloatBits.AreEqual(gateway.Current("b"), 100.25f), Is.True);
            Assert.That(ledger.Count, Is.Zero); Assert.That(results.CommittedCount, Is.Zero);
        }

        [Test]
        public void GatewayExceptionLeavesAllLedgersAndVitalsOld()
        {
            FakeVitals gateway = new(S("a", 100, 100)) { ThrowOnApply = true };
            RunProgressionLedger ledger = Ledger(); StageEventResultLedger results = new();
            Assert.That(Service(gateway, ledger, results).Execute(Risk(gateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.LedgerFaulted));
            Assert.That(gateway.Current("a"), Is.EqualTo(100));
            Assert.That(ledger.Count, Is.Zero); Assert.That(results.CommittedCount, Is.Zero);
        }

        [Test]
        public void EntitlementCommitFaultRestoresVitalsAndLeavesNoResult()
        {
            FakeVitals gateway = new(S("a", 100, 100));
            RunProgressionLedger ledger = Ledger(ProgressionLedgerMutationPoint.EarnCommitted);
            StageEventResultLedger results = new();
            Assert.That(Service(gateway, ledger, results).Execute(Risk(gateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.LedgerFaulted));
            Assert.That(gateway.Current("a"), Is.EqualTo(100)); Assert.That(ledger.Count, Is.Zero); Assert.That(results.CommittedCount, Is.Zero);
        }

        [Test]
        public void ResultCommitFaultRollsBackEntitlementAndVitals()
        {
            FakeVitals gateway = new(S("a", 100, 100)); RunProgressionLedger ledger = Ledger();
            StageEventResultLedger results = new(point => { if (point == StageEventResultMutationPoint.Committed) throw new Exception("fault"); });
            Assert.That(Service(gateway, ledger, results).Execute(Risk(gateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.ResultFaulted));
            Assert.That(gateway.Current("a"), Is.EqualTo(100)); Assert.That(ledger.Count, Is.Zero); Assert.That(results.CommittedCount, Is.Zero);
        }

        [Test]
        public void RestoreConflictIsExplicitCompensationFault()
        {
            FakeVitals gateway = new(S("a", 100, 100), S("b", 100, 100)) { FailAfterAppliedCount = 1, RestoreFails = true };
            Assert.That(Service(gateway).Execute(Risk(gateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.CompensationFaulted));
        }

        [Test]
        public void RandomAndTotalEarnCapsRejectWithoutLastingCost()
        {
            FakeVitals gateway = new(S("a", 100, 100)); RunProgressionLedger ledger = Ledger();
            ledger.TryEarn(RandomRequest("existing"), out _);
            Assert.That(Service(gateway, ledger, new StageEventResultLedger()).Execute(Risk(gateway.Roster)).Result,
                Is.EqualTo(RandomGrowthEventTransactionResult.LedgerFaulted));
            Assert.That(gateway.Current("a"), Is.EqualTo(100)); Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void ReceiptCollectionsAreImmutableCopies()
        {
            FakeVitals gateway = new(S("a", 100, 100));
            RandomGrowthEventTransactionReceipt receipt = Service(gateway).Execute(Risk(gateway.Roster));
            Assert.That(receipt.EventReceipt.Costs, Is.Not.InstanceOf<List<PartyVitalMutation>>());
            Assert.That(receipt.EventReceipt.Costs.Single().Before, Is.EqualTo(100));
        }

        private static RandomGrowthEventTransactionService Service(FakeVitals gateway,
            RunProgressionLedger ledger = null, StageEventResultLedger results = null) =>
            new(ledger ?? Ledger(), results ?? new StageEventResultLedger(), gateway);
        private static RunProgressionLedger Ledger(ProgressionLedgerMutationPoint? fault = null) =>
            new(new ProgressionRunId("run.f1"), ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry(), point => { if (point == fault) throw new Exception("fault"); });
        private static RandomGrowthEventCommand Risk(IEnumerable<PartyVitalSnapshot> roster) =>
            new(Cause("risk"), StageEventChoiceKind.Risk, roster, RandomRequest("risk"));
        private static ProgressionEarnRequest RandomRequest(string result) => new(
            ProgressionSourceRegistry.RandomGrowthSegment, ProgressionSourceCategory.Random,
            ProgressionSourceType.RandomEventRisk, ProgressionSourceRegistry.RandomGrowthRiskSource, result);
        private static StageEventCause Cause(string suffix) => new("run.f1", "stage.gen.1", "slot.1",
            RandomGrowthEventIds.Event, suffix == "risk" ? RandomGrowthEventIds.RiskChoice : RandomGrowthEventIds.DeclineChoice,
            "result." + suffix);
        private static PartyVitalSnapshot S(string id, float current, float max) => new(id, current, max);

        private sealed class FakeVitals : IPartyVitalMutationGateway
        {
            private readonly Dictionary<string, PartyVitalSnapshot> values = new(StringComparer.Ordinal);
            public FakeVitals(params PartyVitalSnapshot[] roster) { Roster = roster; foreach (PartyVitalSnapshot s in roster) if (!values.ContainsKey(s.MemberId)) values.Add(s.MemberId, s); }
            public IReadOnlyList<PartyVitalSnapshot> Roster { get; }
            public int ApplyCalls; public int FailAfterAppliedCount = -1; public bool RestoreFails; public bool ThrowOnApply;
            public float Current(string id) => values[id].CurrentHp;
            public bool TryCapture(out IReadOnlyList<PartyVitalSnapshot> roster) { roster = values.Values.Select(x => new PartyVitalSnapshot(x.MemberId, x.CurrentHp, x.MaxHp)).ToArray(); return true; }
            public bool TryApply(string transactionId, PartyVitalCostPlan plan, out PartyVitalMutationReceipt receipt)
            {
                ApplyCalls++; List<PartyVitalMutation> applied = new();
                if (ThrowOnApply) throw new InvalidOperationException("Injected vital gateway fault.");
                foreach (PartyVitalMutation mutation in plan.Mutations)
                {
                    if (FailAfterAppliedCount >= 0 && applied.Count >= FailAfterAppliedCount) { receipt = new PartyVitalMutationReceipt(transactionId, applied); return false; }
                    PartyVitalSnapshot current = values[mutation.MemberId];
                    if (!CanonicalFloatBits.AreEqual(current.CurrentHp, mutation.Before)) { receipt = new PartyVitalMutationReceipt(transactionId, applied); return false; }
                    values[mutation.MemberId] = new PartyVitalSnapshot(current.MemberId, mutation.After, current.MaxHp); applied.Add(mutation);
                }
                receipt = new PartyVitalMutationReceipt(transactionId, applied); return true;
            }
            public bool TryRestore(PartyVitalMutationReceipt receipt)
            {
                if (RestoreFails) return false;
                foreach (PartyVitalMutation mutation in receipt.Applied.Reverse())
                {
                    PartyVitalSnapshot current = values[mutation.MemberId];
                    if (!CanonicalFloatBits.AreEqual(current.CurrentHp, mutation.After)) return false;
                    values[mutation.MemberId] = new PartyVitalSnapshot(current.MemberId, mutation.Before, current.MaxHp);
                }
                return true;
            }
        }
    }
}
