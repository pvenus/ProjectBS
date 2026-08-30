using System;
using System.Reflection;
using NUnit.Framework;
using Progression.Portfolio;

public sealed class Chapter1BattleReuseSelectionContractTests
{
    [TestCase(Chapter1BattleReuseSelectionContract.Original02,
        Chapter1BattleReuseSelectionContract.Event27)]
    [TestCase(Chapter1BattleReuseSelectionContract.Original17,
        Chapter1BattleReuseSelectionContract.Event27)]
    [TestCase(Chapter1BattleReuseSelectionContract.Original10,
        Chapter1BattleReuseSelectionContract.Event30)]
    [TestCase(Chapter1BattleReuseSelectionContract.Original13,
        Chapter1BattleReuseSelectionContract.Event46)]
    [TestCase(Chapter1BattleReuseSelectionContract.Original09,
        Chapter1BattleReuseSelectionContract.Event31)]
    [TestCase(Chapter1BattleReuseSelectionContract.Original04,
        Chapter1BattleReuseSelectionContract.Event31)]
    [TestCase(Chapter1BattleReuseSelectionContract.Original10,
        Chapter1BattleReuseSelectionContract.Event41)]
    [TestCase(Chapter1BattleReuseSelectionContract.Event30,
        Chapter1BattleReuseSelectionContract.Event41)]
    public void CanonicalBattleReusePairsConflict(string original, string portfolio)
    {
        PortfolioEventDescriptor first = Descriptor(original);
        PortfolioEventDescriptor second = Descriptor(portfolio);
        Assert.That(Chapter1BattleReuseSelectionContract.Conflicts(
            new[] { first }, second), Is.True);
        Assert.That(Chapter1BattleReuseSelectionContract.Conflicts(
            new[] { second }, first), Is.True);
    }

    [Test]
    public void UnrelatedEventsRemainCompatible()
    {
        Assert.That(Chapter1BattleReuseSelectionContract.Conflicts(
            new[] { Descriptor(Chapter1BattleReuseSelectionContract.Original10) },
            Descriptor(Chapter1BattleReuseSelectionContract.Event27)), Is.False);
    }

    [Test]
    public void RequiredCharacterAcceptsAnyCanonicalGradeIdentity()
    {
        var descriptor = new PortfolioEventDescriptor(
            "event.act1.random_event.40.jihan_empty_medicine_folio",
            PortfolioPurpose.Recovery, "medicine_folio",
            requiredCharacterId: "character.jihan");
        MethodInfo method = typeof(Chapter1PortfolioManifestBuilder).GetMethod(
            "IsCharacterEligible", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);
        Assert.That(method.Invoke(null,
            new object[] { descriptor, new[] { "character.jihan.2" } }), Is.True);
        Assert.That(method.Invoke(null,
            new object[] { descriptor, Array.Empty<string>() }), Is.False);
    }

    private static PortfolioEventDescriptor Descriptor(string id) =>
        new(id, PortfolioPurpose.World, id);
}
