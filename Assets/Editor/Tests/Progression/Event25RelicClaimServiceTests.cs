using System.Collections.Generic;
using NUnit.Framework;
using Stage;

public sealed class Event25RelicClaimServiceTests
{
    private static readonly string[] Pool =
    {
        "item.relic.blunt_gear",
        "item.relic.electric_crystal",
        "item.relic.ember_necklace",
        "item.relic.frozen_nail",
        "item.relic.spiked_carapace",
        "item.relic.toxic_pouch"
    };

    [Test]
    public void OwnedEntriesAreExcludedAndEligibleZeroDisablesBattle()
    {
        Assert.That(Event25RelicClaimService.EligibleRelicIds(Pool,
            new[] { Pool[0], Pool[1] }), Is.EqualTo(new[]
        {
            Pool[2], Pool[3], Pool[4], Pool[5]
        }));
        Assert.That(Event25RelicClaimService.CanBeginBattle(Pool, Pool), Is.False);
    }

    [Test]
    public void SameCauseFixesRelicAndEligibleFingerprint()
    {
        Event25RelicClaimService service = new();
        Event25RelicClaim first = service.CreateVictoryClaim(
            "run", "stage", "reservation", "victory", Pool, new[] { Pool[0] });
        Event25RelicClaim replay = service.CreateVictoryClaim(
            "run", "stage", "reservation", "victory", Pool, new[] { Pool[0] });
        Assert.That(replay.selectedRelicId, Is.EqualTo(first.selectedRelicId));
        Assert.That(replay.eligibleFingerprint, Is.EqualTo(first.eligibleFingerprint));
        Assert.That(replay.causeId, Is.EqualTo(first.causeId));
    }

    [Test]
    public void TerminalFailureRetriesWithoutRerollOrSecondAdd()
    {
        Event25RelicClaim claim = new Event25RelicClaimService().CreateVictoryClaim(
            "run", "stage", "reservation", "victory", Pool, new string[0]);
        int adds = 0;
        int terminals = 0;
        Assert.That(new Event25RelicClaimService().TryGrant(claim,
            _ => { adds++; return true; }, _ => { terminals++; return false; }), Is.False);
        string fixedId = claim.selectedRelicId;
        Assert.That(claim.state, Is.EqualTo(Event25RelicClaimState.RelicClaimPendingRetry));
        Assert.That(new Event25RelicClaimService().TryGrant(claim,
            _ => { adds++; return true; }, _ => { terminals++; return true; }), Is.True);
        Assert.That(claim.selectedRelicId, Is.EqualTo(fixedId));
        Assert.That(adds, Is.EqualTo(1));
        Assert.That(terminals, Is.EqualTo(2));
        Assert.That(claim.state, Is.EqualTo(Event25RelicClaimState.GrantedTerminal));
    }
}
