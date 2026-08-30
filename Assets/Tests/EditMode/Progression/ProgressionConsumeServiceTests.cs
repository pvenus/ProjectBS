using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Progression.Tests
{
    public sealed class ProgressionConsumeServiceTests
    {
        private RunProgressionLedger ledger;
        private FakeSkillLevelGateway gateway;
        private ProgressionOpportunitySnapshot opportunity;
        private ProgressionSkillCandidateSnapshot candidate;
        private ProgressionConsumeService service;

        [SetUp]
        public void SetUp()
        {
            ledger = CreateLedger();
            ledger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot earned);
            FixedOfferService offers = new();
            offers.GetOrCreate(
                ledger,
                earned.OpportunityId,
                earned.Revision,
                Catalog(),
                out opportunity);
            candidate = opportunity.Offer.Candidates[0];
            gateway = new FakeSkillLevelGateway();
            foreach (ProgressionSkillCandidateSnapshot value in opportunity.Offer.Candidates)
            {
                gateway.Add(value, value.CurrentLevel);
            }

            service = new ProgressionConsumeService(ledger, gateway);
        }

        [Test]
        public void SuccessAppliesSelectedCandidateExactlyOnceAndStoresReceipt()
        {
            ProgressionApplyCandidateCommand command = Command();

            ProgressionApplyResult result = service.TryApplyCandidate(command);

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.Applied));
            Assert.That(result.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Applied));
            Assert.That(result.Receipt.PreviousLevel, Is.EqualTo(candidate.CurrentLevel));
            Assert.That(result.Receipt.AppliedLevel, Is.EqualTo(candidate.CurrentLevel + 1));
            Assert.That(result.Opportunity.AppliedReceipt, Is.SameAs(result.Receipt));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel + 1));
            Assert.That(gateway.ApplyCalls, Is.EqualTo(1));
            Assert.That(gateway.RollbackCalls, Is.Zero);
            Assert.That(opportunity.Offer.Fingerprint, Is.EqualTo(result.Opportunity.Offer.Fingerprint));
        }

        [Test]
        public void DuplicateDeliveryReturnsAlreadyAppliedWithoutSecondMutation()
        {
            ProgressionApplyCandidateCommand command = Command();

            Assert.That(service.TryApplyCandidate(command).Code,
                Is.EqualTo(ProgressionApplyResultCode.Applied));
            ProgressionApplyResult duplicate = service.TryApplyCandidate(command);

            Assert.That(duplicate.Code, Is.EqualTo(ProgressionApplyResultCode.AlreadyApplied));
            Assert.That(gateway.ApplyCalls, Is.EqualTo(1));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel + 1));
        }

        [Test]
        public void AppliedRecordStillRejectsMismatchedDuplicateIdentity()
        {
            ProgressionApplyCandidateCommand command = Command();
            Assert.That(service.TryApplyCandidate(command).Code,
                Is.EqualTo(ProgressionApplyResultCode.Applied));
            ProgressionApplyCandidateCommand forged = new(
                command.OpportunityId,
                command.Fingerprint,
                command.OwnerCharacterId,
                command.SkillInstanceId,
                "skill.forged",
                command.ExpectedLevel,
                command.ExpectedLedgerRevision);

            ProgressionApplyResult result = service.TryApplyCandidate(forged);

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.RejectedCandidate));
            Assert.That(gateway.ApplyCalls, Is.EqualTo(1));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel + 1));
        }

        [Test]
        public void ReentrantDoubleConfirmIsBusyAndMutatesOnce()
        {
            ProgressionApplyResult nested = null;
            ProgressionApplyCandidateCommand command = Command();
            gateway.OnApplying = () => nested = service.TryApplyCandidate(command);

            ProgressionApplyResult outer = service.TryApplyCandidate(command);

            Assert.That(nested.Code, Is.EqualTo(ProgressionApplyResultCode.Busy));
            Assert.That(outer.Code, Is.EqualTo(ProgressionApplyResultCode.Applied));
            Assert.That(gateway.ApplyCalls, Is.EqualTo(1));
        }

        [TestCase("fingerprint")]
        [TestCase("owner")]
        [TestCase("instance")]
        [TestCase("skill")]
        [TestCase("level")]
        [TestCase("revision")]
        public void StaleOrMismatchedCommandDoesNotCallGateway(string mismatch)
        {
            ProgressionApplyCandidateCommand command = new(
                opportunity.OpportunityId,
                mismatch == "fingerprint" ? "wrong" : opportunity.Offer.Fingerprint,
                mismatch == "owner" ? "wrong" : candidate.OwnerCharacterId,
                mismatch == "instance" ? "wrong" : candidate.SkillInstanceId,
                mismatch == "skill" ? "wrong" : candidate.CanonicalSkillId,
                mismatch == "level" ? candidate.CurrentLevel + 1 : candidate.CurrentLevel,
                mismatch == "revision" ? opportunity.Revision + 1 : opportunity.Revision);

            ProgressionApplyResult result = service.TryApplyCandidate(command);

            Assert.That(result.Code, Is.Not.EqualTo(ProgressionApplyResultCode.Applied));
            Assert.That(gateway.ApplyCalls, Is.Zero);
            AssertPendingUnchanged();
        }

        [Test]
        public void PendingBlockedNeverCallsGateway()
        {
            RunProgressionLedger blockedLedger = CreateLedger();
            blockedLedger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot earned);
            new FixedOfferService().GetOrCreate(
                blockedLedger,
                earned.OpportunityId,
                earned.Revision,
                Array.Empty<ProgressionSkillCandidateSnapshot>(),
                out ProgressionOpportunitySnapshot blocked);
            FakeSkillLevelGateway blockedGateway = new();
            ProgressionConsumeService blockedService = new(blockedLedger, blockedGateway);

            ProgressionApplyResult result = blockedService.TryApplyCandidate(new ProgressionApplyCandidateCommand(
                blocked.OpportunityId,
                "fixed-empty-fingerprint",
                "owner",
                "instance",
                "skill",
                1,
                blocked.Revision));

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.RejectedState));
            Assert.That(blockedGateway.ApplyCalls, Is.Zero);
        }

        [TestCase(SkillLevelMutationResult.RejectedExpectedLevel)]
        [TestCase(SkillLevelMutationResult.Faulted)]
        public void GatewayRejectOrFaultRollsLedgerBackAndCanRetry(
            SkillLevelMutationResult failure)
        {
            gateway.NextApplyResult = failure;

            ProgressionApplyResult failed = service.TryApplyCandidate(Command());

            Assert.That(failed.Code, Is.EqualTo(
                failure == SkillLevelMutationResult.Faulted
                    ? ProgressionApplyResultCode.GatewayFaulted
                    : ProgressionApplyResultCode.GatewayRejected));
            Assert.That(failed.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(failed.Opportunity.Offer.Fingerprint, Is.EqualTo(opportunity.Offer.Fingerprint));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel));

            gateway.NextApplyResult = SkillLevelMutationResult.Applied;
            ProgressionApplyResult retry = service.TryApplyCandidate(Command(failed.Opportunity.Revision));
            Assert.That(retry.Code, Is.EqualTo(ProgressionApplyResultCode.Applied));
        }

        [Test]
        public void GatewayExceptionRollsLedgerBackWithoutLevelMutation()
        {
            gateway.ThrowOnApply = true;

            ProgressionApplyResult result = service.TryApplyCandidate(Command());

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.GatewayFaulted));
            Assert.That(result.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel));
        }

        [Test]
        public void GatewayExceptionAfterMutationRestoresLevelAndLedger()
        {
            gateway.ThrowAfterMutation = true;

            ProgressionApplyResult result = service.TryApplyCandidate(Command());

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.GatewayFaulted));
            Assert.That(result.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel));
            Assert.That(gateway.ExactRestoreCalls, Is.EqualTo(1));
        }

        [Test]
        public void MalformedAppliedMutationIsCompensatedAndDoesNotConsume()
        {
            gateway.ReturnMalformedMutation = true;

            ProgressionApplyResult result = service.TryApplyCandidate(Command());

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.GatewayRejected));
            Assert.That(result.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel));
            Assert.That(gateway.RollbackCalls, Is.EqualTo(1));
            Assert.That(gateway.ExactRestoreCalls, Is.EqualTo(1));
        }

        [Test]
        public void LedgerCommitFaultRestoresGatewayAndLedgerOldState()
        {
            ProgressionRunId runId = new("run.consume.commit-fault");
            RunProgressionLedger faultLedger = new(
                runId,
                ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry(),
                point =>
                {
                    if (point == ProgressionLedgerMutationPoint.ConsumeApplied)
                    {
                        throw new InvalidOperationException("Injected commit fault.");
                    }
                });
            faultLedger.TryEarn(RescueRequest(), out ProgressionOpportunitySnapshot earned);
            new FixedOfferService().GetOrCreate(
                faultLedger, earned.OpportunityId, earned.Revision, Catalog(), out ProgressionOpportunitySnapshot fixedOffer);
            ProgressionSkillCandidateSnapshot selected = fixedOffer.Offer.Candidates[0];
            FakeSkillLevelGateway faultGateway = new();
            faultGateway.Add(selected, selected.CurrentLevel);
            ProgressionConsumeService faultService = new(faultLedger, faultGateway);

            ProgressionApplyResult result = faultService.TryApplyCandidate(
                Command(fixedOffer, selected));

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.LedgerCommitFailedRestored));
            Assert.That(result.Opportunity.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(result.Opportunity.AppliedReceipt, Is.Null);
            Assert.That(faultGateway.GetLevel(selected), Is.EqualTo(selected.CurrentLevel));
            Assert.That(faultGateway.RollbackCalls, Is.EqualTo(1));
        }

        [Test]
        public void GatewayPrevalidationExceptionReturnsFaultWithoutMutation()
        {
            gateway.ThrowOnGet = true;

            ProgressionApplyResult result = service.TryApplyCandidate(Command());

            Assert.That(result.Code, Is.EqualTo(ProgressionApplyResultCode.GatewayFaulted));
            Assert.That(gateway.ApplyCalls, Is.Zero);
            AssertPendingUnchanged();
        }

        [Test]
        public void ReceiptAndCommandExposeNoMutableSetters()
        {
            AssertNoWritableProperties(typeof(ProgressionApplyCandidateCommand));
            AssertNoWritableProperties(typeof(ProgressionApplyReceipt));
            AssertNoWritableProperties(typeof(ProgressionApplyResult));
            AssertNoWritableProperties(typeof(ProgressionSkillLevelMutation));
        }

        private void AssertPendingUnchanged()
        {
            ledger.TryGetOpportunity(opportunity.OpportunityId, out ProgressionOpportunitySnapshot current);
            Assert.That(current.State, Is.EqualTo(ProgressionOpportunityState.Pending));
            Assert.That(current.Revision, Is.EqualTo(opportunity.Revision));
            Assert.That(current.Offer.Fingerprint, Is.EqualTo(opportunity.Offer.Fingerprint));
            Assert.That(gateway.GetLevel(candidate), Is.EqualTo(candidate.CurrentLevel));
        }

        private ProgressionApplyCandidateCommand Command(int? revision = null) =>
            Command(opportunity, candidate, revision);

        private static ProgressionApplyCandidateCommand Command(
            ProgressionOpportunitySnapshot source,
            ProgressionSkillCandidateSnapshot selected,
            int? revision = null) =>
            new(
                source.OpportunityId,
                source.Offer.Fingerprint,
                selected.OwnerCharacterId,
                selected.SkillInstanceId,
                selected.CanonicalSkillId,
                selected.CurrentLevel,
                revision ?? source.Revision);

        private static RunProgressionLedger CreateLedger() =>
            new(
                new ProgressionRunId("run.consume.test"),
                ProgressionCapPolicy.Chapter1P0,
                new ProgressionSourceRegistry());

        private static ProgressionEarnRequest RescueRequest() =>
            new(
                ProgressionSourceRegistry.FixedRescueSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.BattleVictory,
                ProgressionSourceRegistry.RescueBattleSource,
                "result.consume.test");

        private static ProgressionSkillCandidateSnapshot[] Catalog() =>
            new[]
            {
                new ProgressionSkillCandidateSnapshot("owner.a", "instance.a1", "skill.a1", 1, 10),
                new ProgressionSkillCandidateSnapshot("owner.b", "instance.b1", "skill.b1", 2, 10),
                new ProgressionSkillCandidateSnapshot("owner.a", "instance.a2", "skill.a2", 3, 10)
            };

        private static void AssertNoWritableProperties(Type type)
        {
            foreach (System.Reflection.PropertyInfo property in type.GetProperties())
            {
                Assert.That(property.CanWrite, Is.False, type.Name + "." + property.Name);
            }
        }

        private sealed class FakeSkillLevelGateway : IProgressionSkillLevelGateway
        {
            private readonly Dictionary<ProgressionSkillMutationKey, int> levels = new();

            public int ApplyCalls { get; private set; }
            public int RollbackCalls { get; private set; }
            public int ExactRestoreCalls { get; private set; }
            public SkillLevelMutationResult NextApplyResult { get; set; } =
                SkillLevelMutationResult.Applied;
            public bool ThrowOnApply { get; set; }
            public bool ThrowOnGet { get; set; }
            public bool ThrowAfterMutation { get; set; }
            public bool ReturnMalformedMutation { get; set; }
            public Action OnApplying { get; set; }

            public void Add(ProgressionSkillCandidateSnapshot value, int level)
            {
                levels[Key(value)] = level;
            }

            public int GetLevel(ProgressionSkillCandidateSnapshot value) => levels[Key(value)];

            public bool TryGetCurrentLevel(ProgressionSkillMutationKey key, out int currentLevel)
            {
                if (ThrowOnGet)
                {
                    throw new InvalidOperationException("Injected lookup exception.");
                }

                return levels.TryGetValue(key, out currentLevel);
            }

            public SkillLevelMutationResult TryApplyExactOne(
                ProgressionSkillMutationKey key,
                int expectedLevel,
                out ProgressionSkillLevelMutation mutation)
            {
                ApplyCalls++;
                OnApplying?.Invoke();
                if (ThrowOnApply)
                {
                    throw new InvalidOperationException("Injected apply exception.");
                }

                if (NextApplyResult != SkillLevelMutationResult.Applied)
                {
                    mutation = null;
                    return NextApplyResult;
                }

                if (!levels.TryGetValue(key, out int current) || current != expectedLevel)
                {
                    mutation = null;
                    return SkillLevelMutationResult.RejectedExpectedLevel;
                }

                levels[key] = current + 1;
                mutation = new ProgressionSkillLevelMutation(
                    key,
                    current,
                    ReturnMalformedMutation ? current + 2 : current + 1,
                    "mutation." + ApplyCalls);
                if (ThrowAfterMutation)
                {
                    mutation = null;
                    throw new InvalidOperationException("Injected post-mutation exception.");
                }

                return SkillLevelMutationResult.Applied;
            }

            public bool TryRollback(ProgressionSkillLevelMutation mutation)
            {
                RollbackCalls++;
                if (!levels.TryGetValue(mutation.Key, out int current)
                    || current != mutation.AppliedLevel)
                {
                    return false;
                }

                levels[mutation.Key] = mutation.PreviousLevel;
                return true;
            }

            public bool TryRestoreExactLevel(
                ProgressionSkillMutationKey key,
                int expectedAppliedLevel,
                int restoreLevel)
            {
                ExactRestoreCalls++;
                if (!levels.TryGetValue(key, out int current)
                    || current != expectedAppliedLevel)
                {
                    return false;
                }

                levels[key] = restoreLevel;
                return true;
            }

            private static ProgressionSkillMutationKey Key(
                ProgressionSkillCandidateSnapshot value) =>
                new(value.OwnerCharacterId, value.SkillInstanceId, value.CanonicalSkillId);
        }
    }
}
