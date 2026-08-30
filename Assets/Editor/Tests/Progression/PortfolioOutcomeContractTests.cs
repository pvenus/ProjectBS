using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Progression.Portfolio;
using Session;
using Stage;

public sealed class PortfolioOutcomeContractTests
{
    [Test]
    public void RuntimeIdentityAcceptsStageWrapperBoundToCanonicalPopupAndRejectsSpoofing()
    {
        var gameObject = new UnityEngine.GameObject("portfolio-runtime-identity-test");
        PopupEventSO popup = UnityEngine.ScriptableObject.CreateInstance<PopupEventSO>();
        PopupEventSO wrongPopup = UnityEngine.ScriptableObject.CreateInstance<PopupEventSO>();
        RoundNodeSO wrapper = UnityEngine.ScriptableObject.CreateInstance<RoundNodeSO>();
        try
        {
            GameSession game = gameObject.AddComponent<GameSession>();
            game.StageSession ??= new StageSession();
            game.StageSession.PortfolioOutcomes.ResetForNewRun("runtime.identity.test");
            PortfolioOutcomeExecutionData data = ValidData();
            popup.eventId = data.nodeId;
            wrongPopup.eventId = data.nodeId;
            wrapper.nodeId = "stage.act1.random_event.26.sleepless_waystation";
            wrapper.popupEvent = popup;
            var node = new RoundNode(wrapper.nodeId, RoundNodeType.Event, 0, 0)
            {
                roundNodeSO = wrapper,
                popupEvent = popup
            };

            object[] valid = { data, game, node, popup, null };
            Assert.That(InvokeInternal("TryValidateIdentity", valid), Is.True,
                valid[4] as string);

            data.nodeId = wrapper.nodeId;
            object[] stageIdInjected = { data, game, node, popup, null };
            Assert.That(InvokeInternal("TryValidateIdentity", stageIdInjected), Is.False);
            Assert.That(stageIdInjected[4], Is.EqualTo(
                "PORTFOLIO_OUTCOME_RUNTIME_IDENTITY_MISMATCH"));
            data.nodeId = popup.eventId;

            node.popupEvent = wrongPopup;
            object[] wrongResolvedPopup = { data, game, node, popup, null };
            Assert.That(InvokeInternal("TryValidateIdentity", wrongResolvedPopup), Is.False);
            node.popupEvent = popup;

            wrapper.popupEvent = wrongPopup;
            object[] wrongWrapperBinding = { data, game, node, popup, null };
            Assert.That(InvokeInternal("TryValidateIdentity", wrongWrapperBinding), Is.False);

            PopupEventSO parentPopup = wrongPopup;
            parentPopup.eventId = "node.act1.random_event.34.half_vein_map.intro";
            popup.eventId = "node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro";
            wrapper.popupEvent = parentPopup;
            node.popupEvent = parentPopup;
            data.eventId = "event.act1.random_event.34.half_vein_map.followup.unstable_vein";
            data.nodeId = popup.eventId;
            data.sourcePopupId = popup.eventId;
            var continuation = new PortfolioNextEventContinuationReceipt
            {
                parentEventId = "event.act1.random_event.34.half_vein_map",
                parentNodeId = parentPopup.eventId,
                parentChoiceId = "choice.parent",
                parentResultId = "result.parent",
                parentReservationId = "reservation.parent",
                childEventId = data.eventId,
                childNodeId = popup.eventId,
                childReservationId = "reservation.child",
                childOpened = true
            };
            Assert.That(game.StageSession.PortfolioOutcomes.TryReserveContinuation(continuation), Is.True);
            object[] childContinuation = { data, game, node, popup, null };
            Assert.That(InvokeInternal("TryValidateIdentity", childContinuation), Is.True,
                childContinuation[4] as string);

            continuation.childNodeId += ".spoof";
            object[] spoofedChild = { data, game, node, popup, null };
            Assert.That(InvokeInternal("TryValidateIdentity", spoofedChild), Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(wrapper);
            UnityEngine.Object.DestroyImmediate(wrongPopup);
            UnityEngine.Object.DestroyImmediate(popup);
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    [TestCase(21, "breath_between_water_drops")]
    [TestCase(22, "sleeping_hawk_watch")]
    [TestCase(23, "temple_hundred_eight_steps")]
    [TestCase(24, "herb_scent_empty_barracks")]
    [TestCase(25, "hot_spring_beneath_ice")]
    [TestCase(26, "sleepless_waystation")]
    [TestCase(27, "paper_armor_bandits")]
    [TestCase(28, "rockfall_scouts")]
    [TestCase(29, "chain_bridge_tollkeepers")]
    [TestCase(30, "night_beacon_intruders")]
    [TestCase(36, "cracked_bronze_mirror")]
    [TestCase(32, "hidden_ledger_salt_cart")]
    [TestCase(33, "false_mountain_rite_offering_box")]
    [TestCase(34, "half_vein_map")]
    [TestCase(34, "half_vein_map.followup.unstable_vein")]
    [TestCase(35, "ownerless_wage_sack")]
    [TestCase(37, "nameless_long_sword_in_rain")]
    [TestCase(38, "three_cups_of_moonlight")]
    [TestCase(40, "jihan_empty_medicine_folio")]
    [TestCase(43, "reverse_growing_moss_marker")]
    [TestCase(46, "funeral_without_black_cloth")]
    public void CatalogUsesExactIndependentIdentity(int number, string slug)
    {
        string eventId = $"event.act1.random_event.{number}.{slug}";
        Assert.That(PortfolioEventIdentityCatalog.TryResolve(eventId, out var identity), Is.True);
        Assert.That(identity.SourcePopupId,
            Is.EqualTo($"node.act1.random_event.{number}.{slug}.intro"));
        Assert.That(identity.NodeId, Is.EqualTo(identity.SourcePopupId));
        Assert.That(identity.CanonicalImagePath,
            Is.EqualTo($"Assets/ImagesGenerated/Stage/popup_main/{identity.SourcePopupId}.main.png"));
    }

    [Test]
    public void FactoryCreatesTypedPortfolioPayload()
    {
        Assert.That(ChoiceExecutionDataFactory.Create(ChoiceExecutionType.PortfolioOutcome),
            Is.TypeOf<PortfolioOutcomeExecutionData>());
    }

    [Test]
    public void OrdinaryBattleLegacyAllEmptyIdentityRemainsValid()
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        try
        {
            battle.battleId = "battle.legacy";
            var data = new BattleExecutionData { battle = battle };
            Assert.That(ChoiceExecutionConfigValidator.Validate(new ChoiceExecutionConfig
                { executionType = ChoiceExecutionType.Battle, data = data }), Is.Empty);
        }
        finally { UnityEngine.Object.DestroyImmediate(battle); }
    }

    [Test]
    public void OrdinaryBattlePartialIdentityFailsClosed()
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        try
        {
            battle.battleId = "battle.act1.event10.jangseung_bandit_ambush";
            var data = new BattleExecutionData
            {
                battle = battle,
                eventId = "event.act1.random_event.30.night_beacon_intruders"
            };
            Assert.That(ChoiceExecutionConfigValidator.Validate(new ChoiceExecutionConfig
                { executionType = ChoiceExecutionType.Battle, data = data }),
                Does.Contain("BATTLE_COMPLETION_IDENTITY_PARTIAL"));
        }
        finally { UnityEngine.Object.DestroyImmediate(battle); }
    }

    [Test]
    public void OrdinaryBattleCompleteIdentityUsesIndependentCatalogFields()
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        try
        {
            battle.battleId = "battle.act1.event10.jangseung_bandit_ambush";
            Assert.That(PortfolioEventIdentityCatalog.TryResolve(
                "event.act1.random_event.30.night_beacon_intruders", out var identity), Is.True);
            var data = new BattleExecutionData
            {
                battle = battle, eventId = identity.EventId, nodeId = identity.NodeId,
                sourcePopupId = identity.SourcePopupId,
                reservationId = "reservation.act1.chapter01.random_event.30.night_beacon_intruders",
                choiceId = "choice.act1.random_event.30.night_beacon_intruders.extinguish_false_beacon",
                expectedVictoryResultId = "result.act1.random_event.30.night_beacon_intruders.false_beacon_extinguished"
            };
            Assert.That(ChoiceExecutionConfigValidator.Validate(new ChoiceExecutionConfig
                { executionType = ChoiceExecutionType.Battle, data = data }), Is.Empty);
        }
        finally { UnityEngine.Object.DestroyImmediate(battle); }
    }

    [Test]
    public void UnknownEventIdentityFailsClosed()
    {
        PortfolioOutcomeExecutionData data = ValidData();
        data.eventId = "event.act1.random_event.99.unknown";
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_CATALOG_IDENTITY_MISMATCH"));
    }

    [Test]
    public void NodeAliasFailsClosed()
    {
        PortfolioOutcomeExecutionData data = ValidData();
        data.nodeId = PortfolioEventIdentityCatalog.Entries[3].NodeId;
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_IDENTITY_INVALID"));
    }

    [Test]
    public void ImmediateMultipleOperationsAreRejected()
    {
        PortfolioOutcomeExecutionData data = ValidData();
        data.operations.Add(new PortfolioOutcomeOperationData
            { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue, targetId = "flag.2" });
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_IMMEDIATE_ATOMICITY"));
    }

    [TestCase("event.act1.random_event.32.hidden_ledger_salt_cart")]
    [TestCase("event.act1.random_event.35.ownerless_wage_sack")]
    [TestCase("event.act1.random_event.44.buried_tax_stele")]
    public void LockedGoldFlagPairsAreAccepted(string eventId)
    {
        PortfolioOutcomeExecutionData data = DataFor(eventId);
        data.operations = new List<PortfolioOutcomeOperationData>
        {
            new() { kind = PortfolioOutcomeOperationKind.GoldGrant, amount = 25 },
            new() { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue, targetId = "flag.locked" }
        };
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
    }

    [Test]
    public void LockedGoldFlagPair_ReversedOrderFailsClosed()
    {
        PortfolioOutcomeExecutionData data = DataFor(
            "event.act1.random_event.32.hidden_ledger_salt_cart");
        data.operations = new List<PortfolioOutcomeOperationData>
        {
            new() { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue, targetId = "flag.locked" },
            new() { kind = PortfolioOutcomeOperationKind.GoldGrant, amount = 25 }
        };
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_IMMEDIATE_ATOMICITY"));
    }

    [Test]
    public void Event35ZeroEffectiveHeal_IsAcceptedOnlyForLockedChoice()
    {
        PortfolioOutcomeExecutionData data = DataFor(
            "event.act1.random_event.35.ownerless_wage_sack");
        data.choiceId = "choice.act1.random_event.35.ownerless_wage_sack.return_all_wages";
        data.operations = new List<PortfolioOutcomeOperationData>
        {
            new() { kind = PortfolioOutcomeOperationKind.VitalDelta,
                maxHpPercent = 25, allowEffectiveZero = true },
            new() { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue, targetId = "flag.locked" }
        };
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
        data.choiceId = "choice.wrong";
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_ZERO_EFFECT_POLICY_INVALID"));
    }

    [Test]
    public void Event28VitalRoutePair_IsAcceptedOnlyInLockedOrder()
    {
        PortfolioOutcomeExecutionData data = DataFor(
            "event.act1.random_event.28.rockfall_scouts");
        data.operations = new List<PortfolioOutcomeOperationData>
        {
            new() { kind = PortfolioOutcomeOperationKind.VitalDelta,
                maxHpPercent = -10, nonlethal = true },
            new() { kind = PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute,
                snapshotId = data.reservationId,
                selectionMode = ImmediateSuccessorRouteSelectionMode.LongestRemainingToSectionExit }
        };
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
        data.operations.Reverse();
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_IMMEDIATE_ATOMICITY"));
    }

    [Test]
    public void Event42FlagRoutePair_IsAcceptedOnlyInLockedOrder()
    {
        PortfolioOutcomeExecutionData data = DataFor(
            "event.act1.random_event.42.twice_ringing_mountain_echo");
        data.operations = new List<PortfolioOutcomeOperationData>
        {
            new() { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue,
                targetId = "world_flag.act1.chapter01.echo_shout_alerted_enemies" },
            new() { kind = PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute,
                snapshotId = data.reservationId,
                selectionMode = ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit }
        };
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
        data.operations.Reverse();
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
            Does.Contain("PORTFOLIO_OUTCOME_IMMEDIATE_ATOMICITY"));
    }

    [TestCase("event.act1.random_event.28.rockfall_scouts",
        PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute)]
    [TestCase("event.act1.random_event.33.false_mountain_rite_offering_box",
        PortfolioOutcomeOperationKind.GoldGrant)]
    public void LockedBattleContinuationPairs_AreAccepted(
        string eventId, PortfolioOutcomeOperationKind continuationKind)
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        try
        {
            battle.battleId = "battle.locked";
            PortfolioOutcomeExecutionData data = DataFor(eventId);
            data.operations = new List<PortfolioOutcomeOperationData>
            {
                new() { kind = PortfolioOutcomeOperationKind.BeginBattle, battle = battle },
                continuationKind == PortfolioOutcomeOperationKind.GoldGrant
                    ? new PortfolioOutcomeOperationData { kind = continuationKind, amount = 50 }
                    : new PortfolioOutcomeOperationData { kind = continuationKind,
                        snapshotId = data.reservationId,
                        selectionMode = ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit }
            };
            Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
        }
        finally { UnityEngine.Object.DestroyImmediate(battle); }
    }

    [Test]
    public void Event34Continuation_FullIdentityPassesAndPartialFails()
    {
        var child = UnityEngine.ScriptableObject.CreateInstance<PopupEventSO>();
        try
        {
            child.eventId =
                "node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro";
            var data = new NextEventExecutionData
            {
                nextEvent = child,
                parentEventId = "event.act1.random_event.34.half_vein_map",
                parentNodeId = "node.act1.random_event.34.half_vein_map.intro",
                parentChoiceId = "choice.act1.random_event.34.half_vein_map.follow_unstable_vein",
                parentResultId = "result.act1.random_event.34.half_vein_map.unstable_vein_entered",
                parentReservationId = "reservation.act1.chapter01.random_event.34.half_vein_map",
                childEventId = "event.act1.random_event.34.half_vein_map.followup.unstable_vein",
                childNodeId = "node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro",
                childReservationId =
                    "reservation.act1.chapter01.random_event.34.half_vein_map.followup.unstable_vein"
            };
            var config = new ChoiceExecutionConfig
                { executionType = ChoiceExecutionType.NextEvent, data = data };
            Assert.That(ChoiceExecutionConfigValidator.Validate(config), Is.Empty);
            data.childReservationId = string.Empty;
            Assert.That(ChoiceExecutionConfigValidator.Validate(config),
                Does.Contain("NEXT_EVENT_CONTINUATION_IDENTITY_PARTIAL"));
        }
        finally { UnityEngine.Object.DestroyImmediate(child); }
    }

    [Test]
    public void AcquiredDirectRelicFailsPrevalidationWithoutExecutor()
    {
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            relic.relicId = "item.relic.frozen_nail";
            object[] args = { relic, true, null, null };
            Assert.That(InvokeInternal("EvaluateDirectRelicPrevalidation", args), Is.False);
            string disabledCopy = (string)args[2];
            string error = (string)args[3];
            Assert.That(disabledCopy, Is.EqualTo(
                "이미 ‘얼어붙은 못’을 지니고 있어 달빛을 다시 굳힐 수 없습니다."));
            Assert.That(error, Is.EqualTo("PORTFOLIO_OUTCOME_INVENTORY_ALREADY_OWNED"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void UnownedDirectRelicPassesPrevalidation()
    {
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            object[] args = { relic, false, null, null };
            Assert.That(InvokeInternal("EvaluateDirectRelicPrevalidation", args), Is.True);
            string disabledCopy = (string)args[2];
            string error = (string)args[3];
            Assert.That(disabledCopy, Is.Empty);
            Assert.That(error, Is.Empty);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void FullLivingPartyFailsPositiveVitalPrevalidationWithoutExecutor()
    {
        var vitals = new List<(bool isDead, float current, float max)>
        {
            (false, 100f, 100f),
            (true, 0f, 0f)
        };
        object[] args = { vitals, null, null };
        Assert.That(InvokeInternal("EvaluatePositiveVitalPrevalidation", args), Is.False);
        string disabledCopy = (string)args[1];
        string error = (string)args[2];
        Assert.That(disabledCopy, Is.Not.Empty);
        Assert.That(error, Is.EqualTo("PORTFOLIO_OUTCOME_VITAL_NO_EFFECT"));
    }

    [Test]
    public void MissingLivingHpPassesPositiveVitalPrevalidation()
    {
        var vitals = new List<(bool isDead, float current, float max)>
        {
            (false, 80f, 100f),
            (false, 100f, 100f)
        };
        object[] args = { vitals, null, null };
        Assert.That(InvokeInternal("EvaluatePositiveVitalPrevalidation", args), Is.True);
        string disabledCopy = (string)args[1];
        string error = (string)args[2];
        Assert.That(disabledCopy, Is.Empty);
        Assert.That(error, Is.Empty);
    }

    [Test]
    public void Event36ExactAtomicPairIsAccepted()
    {
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            relic.relicId = "item.relic.spiked_carapace";
            PortfolioOutcomeExecutionData data = Event36Data(relic);
            Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void Event36ReversedAtomicPairFailsClosed()
    {
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            relic.relicId = "item.relic.spiked_carapace";
            PortfolioOutcomeExecutionData data = Event36Data(relic);
            data.operations.Reverse();
            Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
                Does.Contain("PORTFOLIO_OUTCOME_IMMEDIATE_ATOMICITY"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void Event36VitalCostRequiresFullNonlethalPayment()
    {
        var insufficient = new List<(bool isDead, float current, float max)>
        {
            (false, 10f, 100f),
            (false, 11f, 100f)
        };
        object[] rejected = { insufficient, null, null };
        Assert.That(InvokeInternal("EvaluateEvent36VitalCost", rejected), Is.False);
        Assert.That(rejected[1], Is.EqualTo(
            "모든 생존자가 비용을 전액 지불하고 HP 1 이상 남아야 합니다."));
        Assert.That(rejected[2], Is.EqualTo("EVENT36_VITAL_COST_UNPAYABLE"));

        var payable = new List<(bool isDead, float current, float max)>
        {
            (false, 11f, 100f),
            (true, 0f, 0f)
        };
        object[] accepted = { payable, null, null };
        Assert.That(InvokeInternal("EvaluateEvent36VitalCost", accepted), Is.True);
    }

    [Test]
    public void Event36FlagChoiceRemainsSingleOperation()
    {
        PortfolioOutcomeExecutionData data = Event36Data(null);
        data.choiceId = "choice.act1.random_event.36.cracked_bronze_mirror.break_false_reflection";
        data.resultId = "result.act1.random_event.36.cracked_bronze_mirror.false_reflection_broken";
        data.operations = new List<PortfolioOutcomeOperationData>
        {
            new() { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue,
                targetId = "runflag.act1.chapter01.event36.false_reflection_broken" }
        };
        Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
    }

    [Test]
    public void Event37ExactBattleGrantPairIsAccepted()
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            battle.battleId = Chapter1Event37SelectionContract.BattleId;
            relic.relicId = Chapter1Event37SelectionContract.RelicId;
            Assert.That(ChoiceExecutionConfigValidator.Validate(
                Config(Event37Data(battle, relic))), Is.Empty);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(battle);
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void Event37WrongBattleOrRelicFailsClosed()
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            battle.battleId = "battle.wrong";
            relic.relicId = Chapter1Event37SelectionContract.RelicId;
            Assert.That(ChoiceExecutionConfigValidator.Validate(
                Config(Event37Data(battle, relic))),
                Does.Contain("EVENT37_BATTLE_PAIR_INVALID"));

            battle.battleId = Chapter1Event37SelectionContract.BattleId;
            relic.relicId = "item.relic.wrong";
            Assert.That(ChoiceExecutionConfigValidator.Validate(
                Config(Event37Data(battle, relic))),
                Does.Contain("EVENT37_BATTLE_PAIR_INVALID"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(battle);
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void Event31ExactBattleGrantPairIsAcceptedAndWrongPairFailsClosed()
    {
        var battle = UnityEngine.ScriptableObject.CreateInstance<Battle.BattleSO>();
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            battle.battleId = Chapter1Event31SelectionContract.BattleId;
            relic.relicId = Chapter1Event31SelectionContract.RelicId;
            PortfolioOutcomeExecutionData data = Event31Data(battle, relic);
            Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)), Is.Empty);
            battle.battleId = "battle.wrong";
            Assert.That(ChoiceExecutionConfigValidator.Validate(Config(data)),
                Does.Contain("EVENT31_BATTLE_PAIR_INVALID"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(battle);
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void Event37OwnedRelicUsesLockedDisabledCopy()
    {
        var relic = UnityEngine.ScriptableObject.CreateInstance<Item.RelicSO>();
        try
        {
            relic.relicId = Chapter1Event37SelectionContract.RelicId;
            object[] args = { relic, true, null, null };
            Assert.That(InvokeInternal("EvaluateDirectRelicPrevalidation", args), Is.False);
            Assert.That(args[2], Is.EqualTo(
                "이미 ‘잿불 목걸이’를 지니고 있어 수호령의 보상을 받을 수 없습니다."));
            Assert.That(args[3], Is.EqualTo("PORTFOLIO_OUTCOME_INVENTORY_ALREADY_OWNED"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(relic);
        }
    }

    [Test]
    public void Event01AndEvent37ShareBattleReuseExclusionGroup()
    {
        PortfolioEventDescriptor original =
            Chapter1Event37SelectionContract.CreateOriginalEvent01();
        PortfolioEventDescriptor event37 = Chapter1Event37SelectionContract.CreateEvent37();
        Assert.That(original.ExclusionGroup,
            Is.EqualTo(Chapter1Event37SelectionContract.BattleReuseExclusionGroup));
        Assert.That(event37.ExclusionGroup, Is.EqualTo(original.ExclusionGroup));
        Assert.That(original.EventId,
            Is.EqualTo(Chapter1Event37SelectionContract.OriginalEvent01Id));
        Assert.That(event37.EventId, Is.EqualTo(Chapter1Event37SelectionContract.Event37Id));
    }

    private static bool InvokeInternal(string methodName, object[] args)
    {
        MethodInfo method = typeof(PortfolioOutcomeRuntimeService).GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null, $"Missing internal contract {methodName}");
        return (bool)method.Invoke(null, args);
    }

    private static PortfolioOutcomeExecutionData Event36Data(Item.RelicSO relic)
    {
        Assert.That(PortfolioEventIdentityCatalog.TryResolve(
            "event.act1.random_event.36.cracked_bronze_mirror", out var identity), Is.True);
        return new PortfolioOutcomeExecutionData
        {
            schemaVersion = 1,
            eventId = identity.EventId,
            nodeId = identity.NodeId,
            sourcePopupId = identity.SourcePopupId,
            choiceId = "choice.act1.random_event.36.cracked_bronze_mirror.accept_cracked_reflection",
            resultId = "result.act1.random_event.36.cracked_bronze_mirror.spiked_carapace_claimed",
            reservationId = "reservation.act1.chapter01.random_event.36.cracked_bronze_mirror",
            operations = new List<PortfolioOutcomeOperationData>
            {
                new() { kind = PortfolioOutcomeOperationKind.VitalDelta,
                    maxHpPercent = -10, nonlethal = true },
                new() { kind = PortfolioOutcomeOperationKind.InventoryGrant,
                    relic = relic, count = 1, unique = true }
            }
        };
    }

    private static PortfolioOutcomeExecutionData Event37Data(
        Battle.BattleSO battle, Item.RelicSO relic)
    {
        Assert.That(PortfolioEventIdentityCatalog.TryResolve(
            Chapter1Event37SelectionContract.Event37Id, out var identity), Is.True);
        return new PortfolioOutcomeExecutionData
        {
            schemaVersion = 1,
            eventId = identity.EventId,
            nodeId = identity.NodeId,
            sourcePopupId = identity.SourcePopupId,
            choiceId = "choice.act1.random_event.37.nameless_long_sword_in_rain.draw_nameless_sword",
            resultId = "result.act1.random_event.37.nameless_long_sword_in_rain.guardian_defeated_relic_claimed",
            reservationId = "reservation.act1.chapter01.random_event.37.nameless_long_sword_in_rain",
            operations = new List<PortfolioOutcomeOperationData>
            {
                new() { kind = PortfolioOutcomeOperationKind.BeginBattle, battle = battle },
                new() { kind = PortfolioOutcomeOperationKind.InventoryGrant,
                    relic = relic, count = 1, unique = true }
            }
        };
    }

    private static PortfolioOutcomeExecutionData Event31Data(
        Battle.BattleSO battle, Item.RelicSO relic)
    {
        Assert.That(PortfolioEventIdentityCatalog.TryResolve(
            Chapter1Event31SelectionContract.Event31Id, out var identity), Is.True);
        return new PortfolioOutcomeExecutionData
        {
            schemaVersion = 1,
            eventId = identity.EventId,
            nodeId = identity.NodeId,
            sourcePopupId = identity.SourcePopupId,
            choiceId = "choice.act1.random_event.31.wounded_mountain_tiger_domain.defeat_poachers",
            resultId = "result.act1.random_event.31.wounded_mountain_tiger_domain.poachers_defeated_toxic_pouch_claimed",
            reservationId = "reservation.act1.chapter01.random_event.31.wounded_mountain_tiger_domain",
            operations = new List<PortfolioOutcomeOperationData>
            {
                new() { kind = PortfolioOutcomeOperationKind.BeginBattle, battle = battle },
                new() { kind = PortfolioOutcomeOperationKind.InventoryGrant,
                    relic = relic, count = 1, unique = true }
            }
        };
    }

    private static ChoiceExecutionConfig Config(PortfolioOutcomeExecutionData data) => new()
        { executionType = ChoiceExecutionType.PortfolioOutcome, data = data };

    private static PortfolioOutcomeExecutionData DataFor(string eventId)
    {
        Assert.That(PortfolioEventIdentityCatalog.TryResolve(eventId, out var identity), Is.True);
        return new PortfolioOutcomeExecutionData
        {
            schemaVersion = 1,
            eventId = identity.EventId,
            nodeId = identity.NodeId,
            sourcePopupId = identity.SourcePopupId,
            choiceId = $"choice.{identity.Number}.locked",
            resultId = $"result.{identity.Number}.locked",
            reservationId = $"reservation.{identity.Number}.locked"
        };
    }

    internal static PortfolioOutcomeExecutionData ValidData()
    {
        PortfolioEventIdentity identity = PortfolioEventIdentityCatalog.Entries[2];
        return new PortfolioOutcomeExecutionData
        {
            schemaVersion = 1, eventId = identity.EventId, nodeId = identity.NodeId,
            sourcePopupId = identity.SourcePopupId, choiceId = "choice.observe",
            resultId = "result.flag", reservationId = "reservation.23",
            operations = new List<PortfolioOutcomeOperationData>
            {
                new() { kind = PortfolioOutcomeOperationKind.SetRunFlagTrue, targetId = "flag.23" }
            }
        };
    }
}
