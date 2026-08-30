using System;
using NUnit.Framework;

namespace Progression.Tests
{
    public sealed class RunProgressionLedgerTests
    {
        private ProgressionRunId runId;
        private ProgressionSourceRegistry registry;

        [SetUp]
        public void SetUp()
        {
            runId = new ProgressionRunId("run.test.chapter1");
            registry = new ProgressionSourceRegistry();
        }

        [Test]
        public void UnknownSourceIsRejectedWithoutRecord()
        {
            RunProgressionLedger ledger = CreateLedger();
            ProgressionEarnRequest request = new(
                "progress.segment.unknown",
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.BattleVictory,
                "battle.unknown",
                "result.unknown");

            ProgressionEarnResult result = ledger.TryEarn(request, out _);

            Assert.That(result, Is.EqualTo(ProgressionEarnResult.RejectedSourceNotAllowed));
            Assert.That(ledger.Count, Is.Zero);
        }

        [Test]
        public void SameCauseOneHundredTimesCreatesOneRecord()
        {
            RunProgressionLedger ledger = CreateLedger();
            ProgressionEarnRequest request = RescueRequest();
            int earned = 0;
            int alreadyEarned = 0;

            for (int i = 0; i < 100; i++)
            {
                ProgressionEarnResult result = ledger.TryEarn(request, out _);
                earned += result == ProgressionEarnResult.Earned ? 1 : 0;
                alreadyEarned += result == ProgressionEarnResult.AlreadyEarned ? 1 : 0;
            }

            Assert.That(earned, Is.EqualTo(1));
            Assert.That(alreadyEarned, Is.EqualTo(99));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void AlternateCauseForSameSegmentReturnsExistingRecord()
        {
            RunProgressionLedger ledger = CreateLedger();
            ProgressionEarnRequest first = PartyUnitedRequest(
                ProgressionSourceRegistry.PartyUnitedJihanSource,
                "result.party.jihan");
            ProgressionEarnRequest second = PartyUnitedRequest(
                ProgressionSourceRegistry.PartyUnitedYujinSource,
                "result.party.yujin");

            Assert.That(ledger.TryEarn(first, out ProgressionOpportunitySnapshot original),
                Is.EqualTo(ProgressionEarnResult.Earned));
            Assert.That(ledger.TryEarn(second, out ProgressionOpportunitySnapshot duplicate),
                Is.EqualTo(ProgressionEarnResult.AlreadyEarned));
            Assert.That(duplicate.OpportunityId, Is.EqualTo(original.OpportunityId));
            Assert.That(ledger.Count, Is.EqualTo(1));
        }

        [Test]
        public void ConsumeReservationIsExclusiveAndRollbackRestoresPending()
        {
            RunProgressionLedger ledger = CreateLedger();
            ledger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot earned);

            ProgressionConsumeResult reserved = ledger.TryReserveConsume(
                earned.OpportunityId,
                earned.Revision,
                out ProgressionOpportunitySnapshot consuming);
            ProgressionConsumeResult duplicate = ledger.TryReserveConsume(
                earned.OpportunityId,
                earned.Revision,
                out _);
            ProgressionConsumeResult rolledBack = ledger.TryRollbackConsume(
                consuming.OpportunityId,
                consuming.ConsumeReservationId,
                out ProgressionOpportunitySnapshot pending);

            Assert.That(reserved, Is.EqualTo(ProgressionConsumeResult.Reserved));
            Assert.That(duplicate, Is.EqualTo(ProgressionConsumeResult.RejectedRevision));
            Assert.That(rolledBack, Is.EqualTo(ProgressionConsumeResult.RolledBack));
            Assert.That(pending.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(pending.ConsumeReservationId, Is.Empty);
        }

        [Test]
        public void AppliedCapsAreFixedTwoRandomOneTotalThree()
        {
            RunProgressionLedger ledger = CreateLedger();
            Apply(ledger, RescueRequest());
            Apply(ledger, PartyUnitedRequest(
                ProgressionSourceRegistry.PartyUnitedJihanSource,
                "result.party"));
            Apply(ledger, RandomRequest());

            ProgressionChapterSummary summary = ledger.GetChapterSummary();
            Assert.That(summary.FixedApplied, Is.EqualTo(2));
            Assert.That(summary.RandomApplied, Is.EqualTo(1));
            Assert.That(summary.TotalApplied, Is.EqualTo(3));

            ProgressionEarnResult duplicateRandom = ledger.TryEarn(
                new ProgressionEarnRequest(
                    ProgressionSourceRegistry.RandomGrowthSegment,
                    ProgressionSourceCategory.Random,
                    ProgressionSourceType.RandomEventRisk,
                    ProgressionSourceRegistry.RandomGrowthRiskSource,
                    "result.random.other"),
                out _);
            Assert.That(duplicateRandom, Is.EqualTo(ProgressionEarnResult.AlreadyEarned));
        }

        [Test]
        public void CandidateFailureCanRemainBlockedWithoutConsumption()
        {
            RunProgressionLedger ledger = CreateLedger();
            ledger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot earned);

            Assert.That(
                ledger.TryMarkPendingBlocked(
                    earned.OpportunityId,
                    earned.Revision,
                    "NO_VALID_CANDIDATE",
                    out ProgressionOpportunitySnapshot blocked),
                Is.EqualTo(ProgressionStateResult.Changed));
            Assert.That(blocked.State, Is.EqualTo(ProgressionOpportunityState.PendingBlocked));
            Assert.That(
                ledger.TryReserveConsume(blocked.OpportunityId, blocked.Revision, out _),
                Is.EqualTo(ProgressionConsumeResult.RejectedState));
            Assert.That(ledger.GetChapterSummary().TotalApplied, Is.Zero);

            Assert.That(
                ledger.TryRestorePending(
                    blocked.OpportunityId,
                    blocked.Revision,
                    out ProgressionOpportunitySnapshot restored),
                Is.EqualTo(ProgressionStateResult.Changed));
            Assert.That(restored.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(restored.BlockedReason, Is.Empty);
        }

        [TestCase((int)ProgressionLedgerMutationPoint.EarnRecordAdded)]
        [TestCase((int)ProgressionLedgerMutationPoint.ConsumeReserved)]
        [TestCase((int)ProgressionLedgerMutationPoint.ConsumeApplied)]
        [TestCase((int)ProgressionLedgerMutationPoint.ConsumeRolledBack)]
        public void FaultInjectionRestoresOldState(int faultPointValue)
        {
            ProgressionLedgerMutationPoint faultPoint =
                (ProgressionLedgerMutationPoint)faultPointValue;
            RunProgressionLedger ledger = new(
                runId,
                ProgressionCapPolicy.Chapter1P0,
                registry,
                point =>
                {
                    if (point == faultPoint)
                    {
                        throw new InvalidOperationException("Injected fault.");
                    }
                });

            if (faultPoint == ProgressionLedgerMutationPoint.EarnRecordAdded)
            {
                Assert.That(ledger.TryEarn(RescueRequest(), out _),
                    Is.EqualTo(ProgressionEarnResult.Faulted));
                Assert.That(ledger.Count, Is.Zero);
                return;
            }

            ledger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot earned);

            if (faultPoint == ProgressionLedgerMutationPoint.ConsumeReserved)
            {
                Assert.That(
                    ledger.TryReserveConsume(earned.OpportunityId, earned.Revision, out _),
                    Is.EqualTo(ProgressionConsumeResult.Faulted));
                AssertPending(ledger, earned.OpportunityId);
                return;
            }

            Assert.That(
                ledger.TryReserveConsume(
                    earned.OpportunityId,
                    earned.Revision,
                    out ProgressionOpportunitySnapshot consuming),
                Is.EqualTo(ProgressionConsumeResult.Reserved));

            if (faultPoint == ProgressionLedgerMutationPoint.ConsumeApplied)
            {
                Assert.That(
                    ledger.TryCommitConsume(
                        consuming.OpportunityId,
                        consuming.ConsumeReservationId,
                        out _),
                    Is.EqualTo(ProgressionConsumeResult.Faulted));
                ledger.TryGetOpportunity(earned.OpportunityId, out ProgressionOpportunitySnapshot restored);
                Assert.That(restored.State, Is.EqualTo(ProgressionOpportunityState.Consuming));
                Assert.That(restored.ConsumeReservationId, Is.EqualTo(consuming.ConsumeReservationId));
                return;
            }

            Assert.That(
                ledger.TryRollbackConsume(
                    consuming.OpportunityId,
                    consuming.ConsumeReservationId,
                    out _),
                Is.EqualTo(ProgressionConsumeResult.Faulted));
            ledger.TryGetOpportunity(earned.OpportunityId, out ProgressionOpportunitySnapshot rollbackRestored);
            Assert.That(rollbackRestored.State, Is.EqualTo(ProgressionOpportunityState.Consuming));
            Assert.That(rollbackRestored.ConsumeReservationId, Is.EqualTo(consuming.ConsumeReservationId));
        }

        [Test]
        public void SessionResetClearsRecordsWhilePassiveAccessKeepsThem()
        {
            ProgressionSession session = new();
            session.ResetForNewRun(runId);
            session.Ledger.TryEarn(RescueRequest(), out _);

            RunProgressionLedger sameLedger = session.Ledger;
            Assert.That(session.Ledger, Is.SameAs(sameLedger));
            Assert.That(session.Ledger.Count, Is.EqualTo(1));

            ProgressionRunId nextRun = new("run.test.chapter1.next");
            session.ResetForNewRun(nextRun);

            Assert.That(session.RunId, Is.EqualTo(nextRun));
            Assert.That(session.Ledger, Is.Not.SameAs(sameLedger));
            Assert.That(session.Ledger.Count, Is.Zero);
        }

        [Test]
        public void SnapshotsDoNotExposeMutableRecords()
        {
            RunProgressionLedger ledger = CreateLedger();
            ledger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot first);
            ProgressionOpportunitySnapshot queried = ledger.GetSnapshots()[0];

            Assert.That(queried, Is.Not.SameAs(first));
            Assert.That(queried.OpportunityId, Is.EqualTo(first.OpportunityId));
            foreach (System.Reflection.PropertyInfo property in
                     typeof(ProgressionOpportunitySnapshot).GetProperties())
            {
                Assert.That(property.CanWrite, Is.False, property.Name);
            }
        }

        private RunProgressionLedger CreateLedger()
        {
            return new RunProgressionLedger(
                runId,
                ProgressionCapPolicy.Chapter1P0,
                registry);
        }

        private static void Apply(
            RunProgressionLedger ledger,
            ProgressionEarnRequest request)
        {
            Assert.That(ledger.TryEarn(request, out ProgressionOpportunitySnapshot earned),
                Is.EqualTo(ProgressionEarnResult.Earned));
            Assert.That(
                ledger.TryReserveConsume(
                    earned.OpportunityId,
                    earned.Revision,
                    out ProgressionOpportunitySnapshot consuming),
                Is.EqualTo(ProgressionConsumeResult.Reserved));
            Assert.That(
                ledger.TryCommitConsume(
                    consuming.OpportunityId,
                    consuming.ConsumeReservationId,
                    out _),
                Is.EqualTo(ProgressionConsumeResult.Applied));
        }

        private static void AssertPending(
            RunProgressionLedger ledger,
            string opportunityId)
        {
            ledger.TryGetOpportunity(opportunityId, out ProgressionOpportunitySnapshot snapshot);
            Assert.That(snapshot.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(snapshot.ConsumeReservationId, Is.Empty);
        }

        private static ProgressionEarnRequest RescueRequest()
        {
            return new ProgressionEarnRequest(
                ProgressionSourceRegistry.FixedRescueSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.BattleVictory,
                ProgressionSourceRegistry.RescueBattleSource,
                "result.rescue");
        }

        private static ProgressionEarnRequest PartyUnitedRequest(
            string sourceId,
            string resultId)
        {
            return new ProgressionEarnRequest(
                ProgressionSourceRegistry.FixedPartyUnitedSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.MajorStoryResolution,
                sourceId,
                resultId);
        }

        private static ProgressionEarnRequest RandomRequest()
        {
            return new ProgressionEarnRequest(
                ProgressionSourceRegistry.RandomGrowthSegment,
                ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventRisk,
                ProgressionSourceRegistry.RandomGrowthRiskSource,
                "result.random.risk");
        }
    }
}
