using System.Reflection;
using Battle;
using NUnit.Framework;
using Session;
using Stage;
using UnityEngine;

public sealed class OrdinaryBattleCompletionEnvelopeTests
{
    [Test]
    public void RequiredCharacterGuardFailsClosedWhenRosterIsUnavailable()
    {
        MethodInfo method = typeof(OrdinaryBattleCompletionService).GetMethod(
            "HasRequiredCharacter", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);
        Assert.That(method.Invoke(null, new object[] { "" }), Is.True);
        Assert.That(method.Invoke(null, new object[] { "character.yujin" }), Is.False);
    }

    [Test]
    public void Event27GoldFinalizesExactlyOnceAndPublishesImmutableReceipt()
    {
        BattleSO battle = ScriptableObject.CreateInstance<BattleSO>();
        try
        {
            battle.battleId = "battle.act1.event02.expose_rain_peddler";
            BattleExecutionData data = Event27Attack(battle);
            StageSession session = new();
            session.Initialize(new StageRuntimeData());
            session.CurrencyRuntimeData.gold = 10;
            var service = new OrdinaryBattleCompletionService();
            Assert.That(service.TryPrepare(data, session, out var identity, out var error),
                Is.True, error);
            var battleSession = new BattleSession { BattleId = battle.battleId };
            Assert.That(service.TryFinalize(session, battleSession, data.nodeId, out error),
                Is.True, error);
            Assert.That(session.CurrencyRuntimeData.gold, Is.EqualTo(60));
            Assert.That(service.CommitFinalized(session), Is.True);
            OrdinaryBattleCompletionReceipt receipt =
                session.OrdinaryBattles.ConsumePublication();
            Assert.That(receipt, Is.Not.Null);
            Assert.That(receipt.GoldGranted, Is.EqualTo(50));
            Assert.That(receipt.Identity.Key, Is.EqualTo(identity.Key));
            Assert.That(session.OrdinaryBattles.ConsumePublication(), Is.Null);
        }
        finally { Object.DestroyImmediate(battle); }
    }

    [Test]
    public void WrongBattleFailsWithoutGoldMutation()
    {
        BattleSO battle = ScriptableObject.CreateInstance<BattleSO>();
        try
        {
            battle.battleId = "battle.act1.event02.expose_rain_peddler";
            StageSession session = new();
            session.Initialize(new StageRuntimeData());
            session.CurrencyRuntimeData.gold = 10;
            var service = new OrdinaryBattleCompletionService();
            Assert.That(service.TryPrepare(Event27Attack(battle), session,
                out _, out _), Is.True);
            var wrong = new BattleSession { BattleId = "battle.wrong" };
            Assert.That(service.TryFinalize(session, wrong,
                "node.act1.random_event.27.paper_armor_bandits.intro", out var error), Is.False);
            Assert.That(error, Is.EqualTo("ORDINARY_BATTLE_COMPLETION_IDENTITY_MISMATCH"));
            Assert.That(session.CurrencyRuntimeData.gold, Is.EqualTo(10));
        }
        finally { Object.DestroyImmediate(battle); }
    }

    [Test]
    public void GraphRollbackRestoresGoldAndKeepsPendingForRetry()
    {
        BattleSO battle = ScriptableObject.CreateInstance<BattleSO>();
        try
        {
            battle.battleId = "battle.act1.event02.expose_rain_peddler";
            StageSession session = new();
            session.Initialize(new StageRuntimeData());
            session.CurrencyRuntimeData.gold = 10;
            var service = new OrdinaryBattleCompletionService();
            Assert.That(service.TryPrepare(Event27Attack(battle), session,
                out _, out _), Is.True);
            Assert.That(service.TryFinalize(session,
                new BattleSession { BattleId = battle.battleId },
                "node.act1.random_event.27.paper_armor_bandits.intro", out _), Is.True);
            Assert.That(service.RollbackFinalized(session), Is.True);
            Assert.That(session.CurrencyRuntimeData.gold, Is.EqualTo(10));
            Assert.That(session.OrdinaryBattles.Pending, Is.Not.Null);
            Assert.That(session.OrdinaryBattles.GoldClaimState,
                Is.EqualTo(OrdinaryBattleGoldClaimState.PendingRetry));
        }
        finally { Object.DestroyImmediate(battle); }
    }

    private static BattleExecutionData Event27Attack(BattleSO battle) => new()
    {
        battle = battle,
        eventId = "event.act1.random_event.27.paper_armor_bandits",
        nodeId = "node.act1.random_event.27.paper_armor_bandits.intro",
        sourcePopupId = "node.act1.random_event.27.paper_armor_bandits.intro",
        reservationId = "reservation.act1.chapter01.random_event.27.paper_armor_bandits",
        choiceId = "choice.act1.random_event.27.paper_armor_bandits.attack_before_spoils_sink",
        expectedVictoryResultId =
            "result.act1.random_event.27.paper_armor_bandits.bandits_defeated_spoils_secured"
    };
}
