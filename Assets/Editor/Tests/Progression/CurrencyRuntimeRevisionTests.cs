using Currency;
using NUnit.Framework;

public sealed class CurrencyRuntimeRevisionTests
{
    [Test]
    public void ExactGrant_IncrementsBalanceAndRevision()
    {
        var wallet = new CurrencyRutimeData { gold = 10 };
        Assert.That(wallet.TryAddGoldExact(25), Is.True);
        Assert.That(wallet.gold, Is.EqualTo(35));
        Assert.That(wallet.Revision, Is.EqualTo(1));
    }

    [Test]
    public void OverflowGrant_MutatesNothing()
    {
        var wallet = new CurrencyRutimeData { gold = int.MaxValue - 1, revision = 4 };
        Assert.That(wallet.TryAddGoldExact(2), Is.False);
        Assert.That(wallet.gold, Is.EqualTo(int.MaxValue - 1));
        Assert.That(wallet.Revision, Is.EqualTo(4));
    }

    [Test]
    public void Spend_Insufficient_MutatesNothing()
    {
        var wallet = new CurrencyRutimeData { gold = 49, revision = 2 };
        Assert.That(wallet.TrySpendGold(50), Is.False);
        Assert.That(wallet.gold, Is.EqualTo(49));
        Assert.That(wallet.Revision, Is.EqualTo(2));
    }

    [Test]
    public void Snapshot_RestoresSingleAppliedMutation()
    {
        var wallet = new CurrencyRutimeData { gold = 100, revision = 3 };
        CurrencyRuntimeSnapshot snapshot = wallet.CaptureSnapshot();
        Assert.That(wallet.TrySpendGold(50), Is.True);
        Assert.That(wallet.TryRestoreSnapshot(snapshot), Is.True);
        Assert.That(wallet.gold, Is.EqualTo(100));
        Assert.That(wallet.Revision, Is.EqualTo(3));
    }

    [Test]
    public void Snapshot_RejectsStaleRevision()
    {
        var wallet = new CurrencyRutimeData { gold = 100 };
        CurrencyRuntimeSnapshot snapshot = wallet.CaptureSnapshot();
        Assert.That(wallet.TryAddGoldExact(5), Is.True);
        Assert.That(wallet.TryAddGoldExact(5), Is.True);
        Assert.That(wallet.TryRestoreSnapshot(snapshot), Is.False);
        Assert.That(wallet.gold, Is.EqualTo(110));
    }

    [Test]
    public void LegacyAddGold_RemainsPositiveOnly()
    {
        var wallet = new CurrencyRutimeData { gold = 10 };
        wallet.AddGold(0);
        wallet.AddGold(-1);
        Assert.That(wallet.gold, Is.EqualTo(10));
        Assert.That(wallet.Revision, Is.Zero);
    }
}
