using NUnit.Framework;
using Progression;
using Progression.RandomGrowth;
using Session;

public sealed class SafeGrowthSessionBridgeTests
{
    [Test]
    public void ResetRuntimePreservesInteractionReservation()
    {
        StageSession session = new();
        ProgressionRunId run = new("run.safe.bridge");
        session.ResetRandomGrowthForNewRun(run);
        SafeGrowthInteractionKey key = Key(run);
        Assert.That(session.SafeGrowthInteraction.TryEnterPreconfirm(key,
            SafeGrowthTransactionIds.ObserveChoiceId, "fp", true, out var before),
            Is.EqualTo(SafeGrowthInteractionResult.Changed));
        session.ResetRuntime();
        Assert.That(session.SafeGrowthInteraction.TryEnterPreconfirm(key,
            SafeGrowthTransactionIds.ObserveChoiceId, "fp", true, out var after),
            Is.EqualTo(SafeGrowthInteractionResult.Existing));
        Assert.That(after.TokenId, Is.EqualTo(before.TokenId));
    }

    [Test]
    public void ExplicitNewRunClearsInteractionReservation()
    {
        StageSession session = new();
        ProgressionRunId first = new("run.safe.first");
        session.ResetRandomGrowthForNewRun(first);
        session.SafeGrowthInteraction.TryEnterPreconfirm(Key(first),
            SafeGrowthTransactionIds.ObserveChoiceId, "fp", true, out var oldToken);

        ProgressionRunId second = new("run.safe.second");
        session.ResetRandomGrowthForNewRun(second);
        Assert.That(session.SafeGrowthInteraction.State, Is.EqualTo(SafeGrowthInteractionState.Offerable));
        Assert.That(session.SafeGrowthInteraction.Token, Is.Null);
        Assert.That(session.SafeGrowthInteraction.TryEnterPreconfirm(Key(second),
            SafeGrowthTransactionIds.ObserveChoiceId, "fp", true, out var newToken),
            Is.EqualTo(SafeGrowthInteractionResult.Changed));
        Assert.That(newToken.TokenId, Is.Not.EqualTo(oldToken.TokenId));
    }

    [Test]
    public void ClearDestroysSessionOnlyInteraction()
    {
        StageSession session = new();
        ProgressionRunId run = new("run.safe.clear");
        session.ResetRandomGrowthForNewRun(run);
        session.SafeGrowthInteraction.TryEnterPreconfirm(Key(run),
            SafeGrowthTransactionIds.ObserveChoiceId, "fp", true, out _);
        session.Clear();
        Assert.That(session.SafeGrowthInteraction.State, Is.EqualTo(SafeGrowthInteractionState.Offerable));
        Assert.That(session.SafeGrowthInteraction.Token, Is.Null);
    }

    private static SafeGrowthInteractionKey Key(ProgressionRunId run) => new(
        run.Value, "stage-generation.safe.bridge", SafeGrowthTransactionIds.ReservationId,
        "node-instance.safe.bridge");
}
