using System;
using System.Collections.Generic;
using NUnit.Framework;
using Progression;
using Stage;
using UIFramework.Data;

public sealed class SafeGrowthPartyWideOfferPresenterTests
{
    [Test]
    public void LegacyFixedOfferOverloadKeepsDefaultThree()
    {
        RunProgressionLedger ledger = Earn(out ProgressionOpportunitySnapshot earned);
        Assert.That(new FixedOfferService().GetOrCreate(ledger, earned.OpportunityId, earned.Revision,
            Candidates(3), out ProgressionOpportunitySnapshot attached), Is.EqualTo(ProgressionOfferAttachResult.Attached));
        Assert.That(attached.Offer.TargetCount, Is.EqualTo(3));
        Assert.That(attached.Offer.Candidates, Has.Count.EqualTo(3));
    }

    [Test]
    public void SafeFixedOfferUsesTargetTwo()
    {
        RunProgressionLedger ledger = Earn(out ProgressionOpportunitySnapshot earned);
        Assert.That(new FixedOfferService().GetOrCreate(ledger, earned.OpportunityId, earned.Revision,
            Candidates(3), 2, out ProgressionOpportunitySnapshot attached), Is.EqualTo(ProgressionOfferAttachResult.Attached));
        Assert.That(attached.Offer.TargetCount, Is.EqualTo(2));
        Assert.That(attached.Offer.Candidates, Has.Count.EqualTo(2));
    }

    [TestCase(1)]
    [TestCase(2)]
    public void SafeOfferContainsOnlyAvailableCandidatesWithoutPlaceholders(int count)
    {
        RunProgressionLedger ledger = Earn(out ProgressionOpportunitySnapshot earned);
        new FixedOfferService().GetOrCreate(ledger, earned.OpportunityId, earned.Revision,
            Candidates(count), 2, out ProgressionOpportunitySnapshot attached);
        Assert.That(attached.Offer.TargetCount, Is.EqualTo(2));
        Assert.That(attached.Offer.Candidates, Has.Count.EqualTo(count));
    }

    [Test]
    public void ReentryReturnsSameSafeOfferAndFingerprint()
    {
        RunProgressionLedger ledger = Earn(out ProgressionOpportunitySnapshot earned);
        FixedOfferService service = new();
        service.GetOrCreate(ledger, earned.OpportunityId, earned.Revision, Candidates(4), 2,
            out ProgressionOpportunitySnapshot first);
        Assert.That(service.GetOrCreate(ledger, earned.OpportunityId, first.Revision, Candidates(1), 2,
            out ProgressionOpportunitySnapshot replay), Is.EqualTo(ProgressionOfferAttachResult.AlreadyAttached));
        Assert.That(replay.Offer.Fingerprint, Is.EqualTo(first.Offer.Fingerprint));
        Assert.That(replay.Offer.Candidates, Is.EqualTo(first.Offer.Candidates));
    }

    [Test]
    public void TargetTwoFingerprintIsSeparatedFromDefaultThree()
    {
        ProgressionRunId run = new("run.safe.offer.fingerprint");
        var generator = new PartyWideFixedOfferGenerator();
        ProgressionOfferSnapshot two = generator.Generate(new ProgressionOfferSeedDescriptor(run,
            ProgressionSourceRegistry.OptionalRandomGrowthSegment, "opportunity.safe", targetCount: 2), Candidates(3));
        ProgressionOfferSnapshot three = generator.Generate(new ProgressionOfferSeedDescriptor(run,
            ProgressionSourceRegistry.OptionalRandomGrowthSegment, "opportunity.safe", targetCount: 3), Candidates(3));
        Assert.That(two.Fingerprint, Is.Not.EqualTo(three.Fingerprint));
    }

    [Test]
    public void StaleRevisionDoesNotAttachOffer()
    {
        RunProgressionLedger ledger = Earn(out ProgressionOpportunitySnapshot earned);
        Assert.That(new FixedOfferService().GetOrCreate(ledger, earned.OpportunityId, earned.Revision + 1,
            Candidates(2), 2, out ProgressionOpportunitySnapshot unchanged),
            Is.EqualTo(ProgressionOfferAttachResult.RejectedRevision));
        Assert.That(unchanged.Offer, Is.Null);
        Assert.That(unchanged.State, Is.EqualTo(ProgressionOpportunityState.Pending));
    }

    [Test]
    public void PresenterWithoutOpportunityDoesNotOpenView()
    {
        FakeHost host = new();
        Assert.That(new SafeGrowthPartyWideOfferPresenter(null, null, null, host).Open(),
            Is.EqualTo(SafeGrowthPartyWideOfferOpenResult.MissingOpportunity));
        Assert.That(host.OpenCount, Is.Zero);
    }

    [Test]
    public void AlreadyOpenPresenterDoesNotAttachOrBindAgain()
    {
        FakeHost host = new() { IsOpen = true };
        Assert.That(new SafeGrowthPartyWideOfferPresenter(null, null, null, host).Open(),
            Is.EqualTo(SafeGrowthPartyWideOfferOpenResult.AlreadyOpen));
        Assert.That(host.OpenCount, Is.Zero);
    }

    private static RunProgressionLedger Earn(out ProgressionOpportunitySnapshot earned)
    {
        RunProgressionLedger ledger = new(new ProgressionRunId("run.safe.offer"),
            ProgressionCapPolicy.Chapter1P0, new ProgressionSourceRegistry());
        Assert.That(ledger.TryEarn(new ProgressionEarnRequest(
            ProgressionSourceRegistry.OptionalRandomGrowthSegment, ProgressionSourceCategory.Random,
            ProgressionSourceType.RandomEventSafe, ProgressionSourceRegistry.RandomGrowthSafeSource,
            "result.safe.offer"), out earned), Is.EqualTo(ProgressionEarnResult.Earned));
        return ledger;
    }

    private static IEnumerable<ProgressionSkillCandidateSnapshot> Candidates(int count)
    {
        for (int i = 0; i < count; i++)
            yield return new ProgressionSkillCandidateSnapshot("owner." + i, "instance." + i,
                "skill." + i, 1, 10);
    }

    private sealed class FakeHost : ISafeGrowthPartyWideOfferViewHost
    {
        public bool IsOpen { get; set; }
        public int OpenCount { get; private set; }
        public bool Open(SkillUpgradeViewData data, Action<int> selected) { OpenCount++; return true; }
        public void Close() { }
    }
}
