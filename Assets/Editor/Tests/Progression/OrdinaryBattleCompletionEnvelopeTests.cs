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
            const string runtimeNodeId = "rt_svg_random_d3_sslot_575_1950_stage.act1.random_event.27.paper_armor_bandits";
            Assert.That(service.TryPrepare(data, session, runtimeNodeId,
                out var identity, out var error),
                Is.True, error);
            var battleSession = new BattleSession { BattleId = battle.battleId };
            var runtimeNode = new RoundNode { nodeId = runtimeNodeId };
            Assert.That(service.TryFinalize(session, battleSession, runtimeNode, out error), Is.True, error);
            Assert.That(session.CurrencyRuntimeData.gold, Is.EqualTo(60));
            Assert.That(service.CommitFinalized(session), Is.True);
            OrdinaryBattleCompletionReceipt receipt =
                session.OrdinaryBattles.ConsumePublication();
            Assert.That(receipt, Is.Not.Null);
            Assert.That(receipt.GoldGranted, Is.EqualTo(50));
            Assert.That(receipt.Identity.NodeId, Is.EqualTo(runtimeNodeId));
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
            Assert.That(service.TryPrepare(Event27Attack(battle), session, "runtime.event27",
                out _, out _), Is.True);
            var wrong = new BattleSession { BattleId = "battle.wrong" };
            Assert.That(service.TryFinalize(session, wrong,
                new RoundNode { nodeId = "runtime.event27" }, out var error), Is.False);
            Assert.That(error, Is.EqualTo("ORDINARY_BATTLE_COMPLETION_IDENTITY_MISMATCH"));
            Assert.That(session.CurrencyRuntimeData.gold, Is.EqualTo(10));
        }
        finally { Object.DestroyImmediate(battle); }
    }

    [Test]
    public void LegacyPopupNodeReceiptFinalizesOnlyAgainstItsBoundRuntimePopup()
    {
        BattleSO battle = ScriptableObject.CreateInstance<BattleSO>();
        PopupEventSO popup = ScriptableObject.CreateInstance<PopupEventSO>();
        try
        {
            battle.battleId = "battle.act1.event02.expose_rain_peddler";
            BattleExecutionData data = Event27Attack(battle);
            popup.eventId = data.sourcePopupId;
            StageSession session = new();
            session.Initialize(new StageRuntimeData());
            var service = new OrdinaryBattleCompletionService();
            Assert.That(service.TryPrepare(data, session, data.nodeId,
                out _, out var prepareError), Is.True, prepareError);

            var runtimeNode = new RoundNode
            {
                nodeId = "rt_svg_random_event27",
                popupEvent = popup
            };
            Assert.That(service.TryFinalize(
                session, new BattleSession { BattleId = battle.battleId },
                runtimeNode, out var error), Is.True, error);
        }
        finally
        {
            Object.DestroyImmediate(popup);
            Object.DestroyImmediate(battle);
        }
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
            Assert.That(service.TryPrepare(Event27Attack(battle), session, "runtime.event27",
                out _, out _), Is.True);
            Assert.That(service.TryFinalize(session,
                new BattleSession { BattleId = battle.battleId },
                new RoundNode { nodeId = "runtime.event27" }, out _), Is.True);
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
