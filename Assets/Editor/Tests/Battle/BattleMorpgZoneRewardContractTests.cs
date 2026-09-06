using System;
using System.IO;
using Battle.Morpg;
using NUnit.Framework;
using UnityEngine;

public sealed class BattleMorpgZoneRewardContractTests
{
    private const string DefinitionPath =
        "Assets/Resources/battle/act1/chapter01/battle.act1.chapter01.01.rescue_villagers.morpg-zone-reward.v1.json";

    [Test]
    public void ExactDefinitionConservesThreeZonesAndRewards()
    {
        string json = File.ReadAllText(DefinitionPath);
        BattleMorpgDecodeResult decoded = BattleMorpgDefinitionCodec.Decode(json);
        Assert.That(decoded.Success, Is.True, decoded.Error);
        var definition = decoded.Definition;
        Assert.That(BattleMorpgDefinitionValidator.TryValidate(definition, out string error), Is.True, error);
        Assert.That(definition.zones[0].reservations.Length, Is.EqualTo(19));
        Assert.That(definition.zones[1].reservations.Length, Is.EqualTo(5));
        Assert.That(definition.zones[2].reservations.Length, Is.EqualTo(4));
    }

    [Test]
    public void MissingVersionIsLegacyAndFutureVersionFailsClosed()
    {
        Assert.That(BattleMorpgDefinitionCodec.Decode("{}").IsLegacy, Is.True);
        BattleMorpgDecodeResult future = BattleMorpgDefinitionCodec.Decode("{\"schemaVersion\":\"battle-morpg-zone-reward.v99\"}");
        Assert.That(future.Success, Is.False);
        Assert.That(future.Error, Does.Contain("unsupported schemaVersion"));
    }

    [Test]
    public void DecodeAndValidationRerunAreMutationFree()
    {
        string before = File.ReadAllText(DefinitionPath);
        Assert.That(BattleMorpgDefinitionCodec.Decode(before).Success, Is.True);
        Assert.That(BattleMorpgDefinitionCodec.Decode(before).Success, Is.True);
        Assert.That(File.ReadAllText(DefinitionPath), Is.EqualTo(before));
    }

    [Test]
    public void UnsafeCheckpointQueuesSaveWithoutExpandingStageSession()
    {
        Assert.That(BattleMorpgCheckpointPolicy.MustQueueDurableSave(MorpgCheckpointState.TransitionPrepared), Is.True);
        Assert.That(BattleMorpgCheckpointPolicy.MustQueueDurableSave(MorpgCheckpointState.RewardTransactionPending), Is.True);
        Assert.That(BattleMorpgCheckpointPolicy.MustStartNewAttemptAfterReload(MorpgCheckpointState.TransitionCommitted), Is.True);
        Assert.That(BattleMorpgCheckpointPolicy.MustStartNewAttemptAfterReload(MorpgCheckpointState.RewardCredited), Is.True);
        Assert.That(BattleMorpgCheckpointPolicy.MustQueueDurableSave(MorpgCheckpointState.OutcomeTerminal), Is.False);
    }

    [Test]
    public void AllTwentyEightBundlesCreditExactTotalsRegardlessOfCallbackOrder()
    {
        var definition = BattleMorpgDefinitionCodec.Decode(File.ReadAllText(DefinitionPath)).Definition;
        var gold = new Account(); var xp = new Account(); var ledger = new BattleRewardLedger(gold, xp);
        long sequence = 0;
        for (int z = definition.zones.Length - 1; z >= 0; z--)
            for (int i = definition.zones[z].reservations.Length - 1; i >= 0; i--)
            {
                var row = definition.zones[z].reservations[i];
                decimal gd = row.unitKey == "black" ? 1m : 3m;
                int xd = row.unitKey == "black" ? 1 : 3;
                string id = $"drop.{definition.battleId}.generation1.{row.reservationId}";
                Assert.That(ledger.TryCredit(id, ++sequence, row.reservationId, gd, xd, out string error), Is.True, error);
                Assert.That(ledger.TryCredit(id, sequence, row.reservationId, gd, xd, out error), Is.True, error);
            }
        Assert.That(ledger.Bundles.Count, Is.EqualTo(28));
        Assert.That(gold.Value, Is.EqualTo(40m));
        Assert.That(xp.Value, Is.EqualTo(10m));
    }

    [Test]
    public void RewardLedgerCreditsGoldAndXpAtomicallyAndDuplicateIsNoOp()
    {
        var gold = new Account(); var xp = new Account();
        var ledger = new BattleRewardLedger(gold, xp);
        Assert.That(ledger.TryCredit("drop.battle.1.rv.z1.00", 1, "rv.z1.00", 1m, 1, out string error), Is.True, error);
        Assert.That(ledger.TryCredit("drop.battle.1.rv.z1.00", 1, "rv.z1.00", 1m, 1, out error), Is.True, error);
        Assert.That(gold.Value, Is.EqualTo(1m)); Assert.That(xp.Value, Is.EqualTo(.25m));
        Assert.That(gold.Revision, Is.EqualTo(1)); Assert.That(xp.Revision, Is.EqualTo(1));
    }

    [Test]
    public void SecondAccountFailureRestoresFirstAndFailsBundle()
    {
        var gold = new Account(); var xp = new Account { RejectApply = true };
        var ledger = new BattleRewardLedger(gold, xp);
        Assert.That(ledger.TryCredit("drop.battle.1.rv.z1.00", 1, "rv.z1.00", 1m, 1, out _), Is.False);
        Assert.That(gold.Value, Is.Zero); Assert.That(xp.Value, Is.Zero);
        Assert.That(ledger.Bundles["drop.battle.1.rv.z1.00"].State, Is.EqualTo(RewardBundleState.Failed));
    }

    [Test]
    public void OutcomeArbiterAllowsExactlyOneTerminalState()
    {
        var attempt = new BattleAttemptKey("run", "episode", BattleMorpgDefinitionValidator.BattleId, "1");
        var arbiter = new BattleOutcomeArbiter(attempt);
        Assert.That(arbiter.TryRequest(), Is.True);
        Assert.That(arbiter.TryCommit(MorpgOutcome.Victory), Is.True);
        Assert.That(arbiter.TryCommit(MorpgOutcome.Defeat), Is.False);
        Assert.That(arbiter.Outcome, Is.EqualTo(MorpgOutcome.Victory));
        Assert.That(arbiter.OutcomeKey, Is.EqualTo(BattleMorpgCanonicalKeys.Attempt(attempt)));
    }

    [Test]
    public void ClearWaveAndTransitionGraphsRejectIllegalEdges()
    {
        var clear = new ClearRecord();
        Assert.That(clear.TryCommit(), Is.False);
        Assert.That(clear.TryDetect(), Is.True);
        Assert.That(clear.TryDetect(), Is.False);
        Assert.That(clear.TryCommit(), Is.True);

        var wave = new WaveStartRecord();
        Assert.That(wave.TryComplete(), Is.False);
        Assert.That(wave.TryStart(), Is.True);
        Assert.That(wave.TryStart(), Is.False);
        Assert.That(wave.TryComplete(), Is.True);

        var attempt = new BattleAttemptKey("run", "episode", BattleMorpgDefinitionValidator.BattleId, "2");
        var transition = new TransitionRecord(attempt, "left", "center", 1);
        Assert.That(transition.TryCommit(), Is.False);
        Assert.That(transition.TryPrepare(), Is.True);
        Assert.That(transition.TryCommit(), Is.True);
        Assert.That(transition.TryRestore(), Is.False);
        Assert.That(BattleMorpgCanonicalKeys.WaveStart(attempt, "wave"), Does.StartWith("waveStart=attempt="));
    }

    [Test]
    public void UnknownPropertiesFailClosed()
    {
        string json = File.ReadAllText(DefinitionPath).Replace("\"backtracking\":false", "\"backtracking\":false,\"unexpected\":1");
        BattleMorpgDecodeResult decoded = BattleMorpgDefinitionCodec.Decode(json);
        Assert.That(decoded.Success, Is.False);
        Assert.That(decoded.Error, Does.Contain("unknown property"));
    }

    private sealed class Account : IRevisionedRewardAccount
    {
        public decimal Value { get; private set; }
        public long Revision { get; private set; }
        public bool RejectApply;
        public bool TryApply(decimal delta, long expectedRevision)
        {
            if (RejectApply || expectedRevision != Revision) return false;
            Value += delta; Revision++; return true;
        }
        public bool TryRestore(decimal value, long expectedRevision)
        {
            if (expectedRevision != Revision) return false;
            Value = value; Revision++; return true;
        }
    }
}
