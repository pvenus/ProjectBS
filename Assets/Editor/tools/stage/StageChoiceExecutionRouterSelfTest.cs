using System;
using Battle;
using Shop;
using Shrine;
using Stage;
using Session;
using UnityEditor;
using UnityEngine;

namespace StageEditor
{
    public static class StageChoiceExecutionRouterSelfTest
    {
        [MenuItem(
            "Tools/Stage/Choice Execution Tests/Run Router Tests")]
        public static void RunFromMenu()
        {
            RunAll();
            Debug.Log("Choice execution router tests passed.");
        }

        public static void RunAll()
        {
            PopupEventSO nextEvent =
                ScriptableObject.CreateInstance<PopupEventSO>();
            BattleSO battle =
                ScriptableObject.CreateInstance<BattleSO>();
            ShopProductSO product =
                ScriptableObject.CreateInstance<ShopProductSO>();
            ShopItemPoolSO pool =
                ScriptableObject.CreateInstance<ShopItemPoolSO>();
            ShrineGodSO god =
                ScriptableObject.CreateInstance<ShrineGodSO>();
            ShrineGodSO secondGod =
                ScriptableObject.CreateInstance<ShrineGodSO>();
            ShrineConfigSO shrineConfig =
                ScriptableObject.CreateInstance<ShrineConfigSO>();
            Texture2D iconTexture = new(2, 2);
            Sprite battleIcon = CreateSprite(iconTexture);
            Sprite shopIcon = CreateSprite(iconTexture);
            Sprite eventIcon = CreateSprite(iconTexture);
            Sprite multiIcon = CreateSprite(iconTexture);
            Sprite lifeIcon = CreateSprite(iconTexture);
            Sprite warIcon = CreateSprite(iconTexture);

            try
            {
                pool.products.Add(product);
                ConfigureGod(
                    god,
                    ShrineGodType.Life,
                    lifeIcon);
                ConfigureGod(
                    secondGod,
                    ShrineGodType.War,
                    warIcon);
                ConfigureShrine(
                    shrineConfig,
                    god,
                    secondGod);

                VerifyNextEventAndDuplicateGuard(
                    nextEvent);
                VerifyNextEventTransactionDispatch(nextEvent);
                VerifyDefaultRouter(
                    nextEvent,
                    battle,
                    pool,
                    shrineConfig,
                    god);
                VerifyIconResolver(
                    shrineConfig,
                    god,
                    secondGod,
                    battle,
                    pool,
                    battleIcon,
                    shopIcon,
                    eventIcon,
                    multiIcon,
                    lifeIcon);
                VerifyInvalidConfigHasNoSideEffect();
                VerifyUnsupportedTypeHasNoSideEffect(
                    battle);
                VerifyContinuationGate();
                VerifyStageGraphCompletionGuard();
                VerifyBattleCompletionToken();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(nextEvent);
                UnityEngine.Object.DestroyImmediate(battle);
                UnityEngine.Object.DestroyImmediate(product);
                UnityEngine.Object.DestroyImmediate(pool);
                UnityEngine.Object.DestroyImmediate(god);
                UnityEngine.Object.DestroyImmediate(secondGod);
                UnityEngine.Object.DestroyImmediate(shrineConfig);
                UnityEngine.Object.DestroyImmediate(battleIcon);
                UnityEngine.Object.DestroyImmediate(shopIcon);
                UnityEngine.Object.DestroyImmediate(eventIcon);
                UnityEngine.Object.DestroyImmediate(multiIcon);
                UnityEngine.Object.DestroyImmediate(lifeIcon);
                UnityEngine.Object.DestroyImmediate(warIcon);
                UnityEngine.Object.DestroyImmediate(iconTexture);
            }
        }

        private static void VerifyDefaultRouter(
            PopupEventSO nextEvent,
            BattleSO battle,
            ShopItemPoolSO pool,
            ShrineConfigSO shrineConfig,
            ShrineGodSO god)
        {
            ChoiceExecutionRouter router =
                ChoiceExecutionRouter.CreateDefault();
            int nextCount = 0;
            int battleCount = 0;
            int shopCount = 0;
            int shrineCount = 0;
            int completeCount = 0;

            ChoiceExecutionContext context =
                new(
                    openNextEvent:
                        target =>
                        {
                            Ensure(
                                target == nextEvent,
                                "NextEvent target changed.");
                            nextCount++;
                        },
                    completeEvent:
                        () =>
                        {
                            completeCount++;
                            return true;
                        },
                    beginBattle:
                        target =>
                        {
                            Ensure(
                                target == battle,
                                "Battle target changed.");
                            battleCount++;
                            return true;
                        },
                    openShop:
                        data =>
                        {
                            Ensure(
                                data.pools.Count == 1
                                && data.pools[0] == pool
                                && data.itemCount == 2
                                && data.shopType == ShopType.Rare,
                                "Shop data changed.");
                            shopCount++;
                            return true;
                        },
                    openShrine:
                        data =>
                        {
                            Ensure(
                                data.config == shrineConfig
                                && data.god == god,
                                "Shrine data changed.");
                            shrineCount++;
                            return true;
                        });

            ChoiceExecutionConfig nextConfig =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.NextEvent);
            ((NextEventExecutionData)nextConfig.data).nextEvent =
                nextEvent;

            ChoiceExecutionConfig battleConfig =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Battle);
            ((BattleExecutionData)battleConfig.data).battle =
                battle;

            ChoiceExecutionConfig shopConfig =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Shop);
            ShopExecutionData shopData =
                (ShopExecutionData)shopConfig.data;
            shopData.pools.Add(pool);
            shopData.itemCount = 2;
            shopData.shopType = ShopType.Rare;

            ChoiceExecutionConfig shrineChoiceConfig =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Shrine);
            ShrineExecutionData shrineData =
                (ShrineExecutionData)shrineChoiceConfig.data;
            shrineData.config = shrineConfig;
            shrineData.god = god;

            ChoiceExecutionConfig completeConfig =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.CompleteEvent);

            EnsureSuccess(
                router,
                "router.default.next",
                nextConfig,
                context);
            EnsureSuccess(
                router,
                "router.default.battle",
                battleConfig,
                context);
            EnsureSuccess(
                router,
                "router.default.shop",
                shopConfig,
                context);
            EnsureSuccess(
                router,
                "router.default.shrine",
                shrineChoiceConfig,
                context);
            EnsureSuccess(
                router,
                "router.default.complete",
                completeConfig,
                context);

            Ensure(
                nextCount == 1
                && battleCount == 1
                && shopCount == 1
                && shrineCount == 1
                && completeCount == 1,
                "Default router did not execute every type once.");
        }

        private static void VerifyNextEventAndDuplicateGuard(
            PopupEventSO nextEvent)
        {
            ChoiceExecutionRouter router =
                ChoiceExecutionRouter.CreateNextEventOnly();
            PopupEventSO openedEvent = null;
            int openCount = 0;
            ChoiceExecutionContext context = new(
                popupEvent =>
                {
                    openedEvent = popupEvent;
                    openCount++;
                });
            ChoiceExecutionConfig config =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.NextEvent);
            ((NextEventExecutionData)config.data).nextEvent =
                nextEvent;

            ChoiceExecutionResult first =
                router.TryExecute(
                    "router.next.1",
                    config,
                    context,
                    out string firstError);

            Ensure(
                first == ChoiceExecutionResult.Success,
                $"NextEvent execution failed: {firstError}");
            Ensure(
                openCount == 1 && openedEvent == nextEvent,
                "NextEvent executor did not open the target once.");

            ChoiceExecutionResult duplicate =
                router.TryExecute(
                    "router.next.1",
                    config,
                    context,
                    out _);

            Ensure(
                duplicate
                    == ChoiceExecutionResult.AlreadyExecuted,
                "Duplicate execution ID was not rejected.");
            Ensure(
                openCount == 1,
                "Duplicate execution caused a side effect.");

            ChoiceExecutionResult second =
                router.TryExecute(
                    "router.next.2",
                    config,
                    context,
                    out string secondError);

            Ensure(
                second == ChoiceExecutionResult.Success,
                $"A new execution ID failed: {secondError}");
            Ensure(
                openCount == 2,
                "A new execution ID did not execute.");
        }

        private static void VerifyNextEventTransactionDispatch(PopupEventSO nextEvent)
        {
            ChoiceExecutionRouter router = ChoiceExecutionRouter.CreateNextEventOnly();
            int legacyOpenCount = 0;
            int transactionOpenCount = 0;
            PortfolioOutcomeOwnership ownership = new();
            ownership.ResetForNewRun("router.next.transaction");
            ChoiceExecutionContext context = new(
                openNextEvent: _ => legacyOpenCount++,
                openNextEventTransaction: (NextEventExecutionData data, out string error) =>
                {
                    error = string.Empty;
                    transactionOpenCount++;
                    return ownership.TryReserveContinuation(new PortfolioNextEventContinuationReceipt
                    {
                        parentEventId = data.parentEventId,
                        parentNodeId = data.parentNodeId,
                        parentChoiceId = data.parentChoiceId,
                        parentResultId = data.parentResultId,
                        parentReservationId = data.parentReservationId,
                        childEventId = data.childEventId,
                        childNodeId = data.childNodeId,
                        childReservationId = data.childReservationId
                    });
                });

            ChoiceExecutionConfig legacy = ChoiceExecutionDataFactory.CreateConfig(
                ChoiceExecutionType.NextEvent);
            ((NextEventExecutionData)legacy.data).nextEvent = nextEvent;
            EnsureSuccess(router, "router.next.legacy", legacy, context);
            Ensure(legacyOpenCount == 1 && transactionOpenCount == 0
                && ownership.PendingContinuation == null,
                "Legacy NextEvent did not use the non-transactional fallback exactly once.");

            ChoiceExecutionConfig typed = ChoiceExecutionDataFactory.CreateConfig(
                ChoiceExecutionType.NextEvent);
            NextEventExecutionData typedData = (NextEventExecutionData)typed.data;
            typedData.nextEvent = nextEvent;
            typedData.parentEventId = "event.act1.random_event.34.half_vein_map";
            typedData.parentNodeId = "node.act1.random_event.34.half_vein_map.intro";
            typedData.parentChoiceId = "choice.act1.random_event.34.half_vein_map.follow_unstable_vein";
            typedData.parentResultId = "result.act1.random_event.34.half_vein_map.unstable_vein_entered";
            typedData.parentReservationId = "reservation.act1.chapter01.random_event.34.half_vein_map";
            typedData.childEventId = "event.act1.random_event.34.half_vein_map.followup.unstable_vein";
            typedData.childNodeId = "node.act1.random_event.34.half_vein_map.followup.unstable_vein.intro";
            typedData.childReservationId = "reservation.act1.chapter01.random_event.34.half_vein_map.followup.unstable_vein";
            nextEvent.eventId = typedData.childNodeId;
            EnsureSuccess(router, "router.next.typed", typed, context);
            Ensure(transactionOpenCount == 1 && legacyOpenCount == 1
                && ownership.PendingContinuation != null,
                "Complete typed NextEvent did not reserve transactionally.");
            Ensure(ownership.TryCommitContinuation(typedData.childEventId, typedData.childNodeId),
                "Typed continuation did not commit and clear its reservation.");
            Ensure(ownership.PendingContinuation == null,
                "Committed typed continuation remained pending.");
            Ensure(router.TryExecute("router.next.typed", typed, context, out _)
                    == ChoiceExecutionResult.AlreadyExecuted
                && transactionOpenCount == 1,
                "Typed NextEvent replay opened twice.");

            ChoiceExecutionConfig partial = ChoiceExecutionDataFactory.CreateConfig(
                ChoiceExecutionType.NextEvent);
            NextEventExecutionData partialData = (NextEventExecutionData)partial.data;
            partialData.nextEvent = nextEvent;
            partialData.parentEventId = "event.partial";
            bool partialResult = new NextEventChoiceExecutionExecutor().TryExecute(
                partialData, context, out string partialError);
            Ensure(!partialResult
                && partialError.StartsWith("NEXT_EVENT_TRANSACTION_IDENTITY_INVALID",
                    StringComparison.Ordinal)
                && legacyOpenCount == 1 && transactionOpenCount == 1,
                "Partial NextEvent identity did not fail closed without mutation.");

            PortfolioNextEventContinuationReceipt stale = new()
            {
                parentEventId = "event.stale",
                parentNodeId = "node.stale",
                parentChoiceId = "choice.stale",
                parentResultId = "result.stale",
                parentReservationId = "reservation.stale",
                childEventId = "event.stale.child",
                childNodeId = "node.stale.child",
                childReservationId = "reservation.stale.child"
            };
            Ensure(ownership.TryReserveContinuation(stale),
                "Stale reservation fixture was not established.");
            ChoiceExecutionResult conflict = router.TryExecute(
                "router.next.conflict", typed, context, out _);
            Ensure(conflict == ChoiceExecutionResult.ExecutionFailed
                && ownership.PendingContinuation == stale,
                "A real typed reservation conflict was not preserved fail-closed.");
            Ensure(ownership.TryReleaseContinuation(stale)
                && ownership.PendingContinuation == null,
                "Stale typed continuation did not release cleanly.");
        }

        private static void VerifyInvalidConfigHasNoSideEffect()
        {
            ChoiceExecutionRouter router =
                ChoiceExecutionRouter.CreateNextEventOnly();
            int openCount = 0;
            ChoiceExecutionContext context =
                new(_ => openCount++);
            ChoiceExecutionConfig mismatch =
                new()
                {
                    executionType =
                        ChoiceExecutionType.NextEvent,
                    data = new BattleExecutionData()
                };

            ChoiceExecutionResult result =
                router.TryExecute(
                    "router.invalid.1",
                    mismatch,
                    context,
                    out string error);

            Ensure(
                result == ChoiceExecutionResult.InvalidConfig,
                $"Mismatched config returned {result}: {error}");
            Ensure(
                openCount == 0,
                "Invalid config caused a side effect.");

            ChoiceExecutionConfig missingTarget =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.NextEvent);
            result = router.TryExecute(
                "router.invalid.2",
                missingTarget,
                context,
                out error);

            Ensure(
                result == ChoiceExecutionResult.InvalidConfig,
                $"Missing NextEvent returned {result}: {error}");
            Ensure(
                openCount == 0,
                "Missing NextEvent caused a side effect.");
        }

        private static void VerifyUnsupportedTypeHasNoSideEffect(
            BattleSO battle)
        {
            ChoiceExecutionRouter router =
                ChoiceExecutionRouter.CreateNextEventOnly();
            int openCount = 0;
            ChoiceExecutionContext context =
                new(_ => openCount++);
            ChoiceExecutionConfig config =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Battle);
            ((BattleExecutionData)config.data).battle =
                battle;

            ChoiceExecutionResult result =
                router.TryExecute(
                    "router.battle.1",
                    config,
                    context,
                    out string error);

            Ensure(
                result == ChoiceExecutionResult.UnsupportedType,
                $"Unsupported Battle returned {result}: {error}");
            Ensure(
                openCount == 0,
                "Unsupported execution caused a side effect.");
        }

        private static void VerifyContinuationGate()
        {
            ChoiceContinuationGate gate = new();

            Ensure(
                gate.Begin("gate.confirm-first"),
                "Gate did not start.");
            Ensure(
                !gate.RequestConfirmation(),
                "Confirmation alone continued execution.");
            Ensure(
                !gate.CompleteRewardPresentation("wrong-id"),
                "A stale Reward callback continued execution.");
            Ensure(
                gate.CompleteRewardPresentation(
                    "gate.confirm-first"),
                "Confirm-first flow did not continue.");
            Ensure(
                !gate.CompleteRewardPresentation(
                    "gate.confirm-first"),
                "Gate continued more than once.");

            Ensure(
                gate.Begin("gate.reward-first"),
                "Gate did not restart.");
            Ensure(
                !gate.CompleteRewardPresentation(
                    "gate.reward-first"),
                "Reward completion alone continued execution.");
            Ensure(
                gate.RequestConfirmation(),
                "Reward-first flow did not continue.");
            Ensure(
                !gate.RequestConfirmation(),
                "Repeated confirmation continued execution.");

            gate.Reset();
            Ensure(
                !gate.CompleteRewardPresentation(
                    "gate.reward-first"),
                "Reset gate accepted a stale callback.");
        }

        private static void VerifyStageGraphCompletionGuard()
        {
            StageGraph graph =
                new("lifecycle.test", "Lifecycle Test");
            RoundNode current =
                new(
                    "node.current",
                    RoundNodeType.Event,
                    0,
                    0)
                {
                    state = RoundNodeState.Available
                };
            RoundNode next =
                new(
                    "node.next",
                    RoundNodeType.Event,
                    1,
                    0)
                {
                    state = RoundNodeState.Locked
                };

            current.AddNextNode(next.nodeId);
            next.AddPrevNode(current.nodeId);
            graph.AddNode(current);
            graph.AddNode(next);

            Ensure(
                graph.SelectNode(current.nodeId),
                "Lifecycle graph could not select the current node.");
            Ensure(
                !graph.TryCompleteCurrentNode("node.stale"),
                "A stale node ID completed the current node.");
            Ensure(
                !current.IsCompleted && next.IsLocked,
                "A rejected completion changed graph state.");
            Ensure(
                graph.TryCompleteCurrentNode(current.nodeId),
                "The expected node ID did not complete.");
            Ensure(
                current.IsCompleted && next.IsAvailable,
                "Node completion did not unlock the next node.");
            Ensure(
                graph.TryCompleteCurrentNode(current.nodeId),
                "Repeated completion was not idempotent.");
        }

        private static void VerifyBattleCompletionToken()
        {
            StageGraph graph =
                new("battle.lifecycle.test", "Battle Lifecycle Test");
            RoundNode current =
                new(
                    "node.current",
                    RoundNodeType.Battle,
                    0,
                    0)
                {
                    state = RoundNodeState.Available
                };
            RoundNode next =
                new(
                    "node.next",
                    RoundNodeType.Event,
                    1,
                    0)
                {
                    state = RoundNodeState.Locked
                };

            current.AddNextNode(next.nodeId);
            next.AddPrevNode(current.nodeId);
            graph.AddNode(current);
            graph.AddNode(next);
            Ensure(
                graph.SelectNode(current.nodeId),
                "Battle lifecycle graph could not select its node.");

            StageSession stageSession = new();
            stageSession.Initialize(
                new StageRuntimeData
                {
                    currentGraph = graph,
                    currentNode = current
                });

            BattleSession battleSession =
                new()
                {
                    BattleId = "battle.lifecycle.test",
                    PendingStageNodeId = "node.current",
                    BattleRuntime =
                        new BattleRuntime
                        {
                            isCompleted = false
                        }
                };

            Ensure(
                !battleSession.QueueStageNodeCompletionIfCleared(),
                "An unfinished battle queued node completion.");
            Ensure(
                !stageSession.TryApplyCompletedBattleNode(
                    battleSession,
                    out _,
                    out _,
                    out _),
                "An unfinished battle completed the StageGraph node.");
            Ensure(
                !current.IsCompleted && next.IsLocked,
                "An unfinished battle changed StageGraph state.");

            battleSession.BattleRuntime.isCompleted = true;
            Ensure(
                battleSession.QueueStageNodeCompletionIfCleared(),
                "A cleared battle did not queue node completion.");
            Ensure(
                battleSession.TryGetCompletedStageNodeId(
                    out string completedNodeId)
                && completedNodeId == "node.current",
                "Queued completion lost the stage node ID.");

            battleSession.CompletedStageNodeId = "node.stale";
            Ensure(
                !stageSession.TryApplyCompletedBattleNode(
                    battleSession,
                    out _,
                    out _,
                    out _),
                "A stale battle completion changed the StageGraph.");
            Ensure(
                !current.IsCompleted && next.IsLocked,
                "A stale battle completion changed node state.");

            battleSession.CompletedStageNodeId = completedNodeId;
            Ensure(
                stageSession.TryApplyCompletedBattleNode(
                    battleSession,
                    out RoundNode completedNode,
                    out bool newlyCompleted,
                    out string error),
                $"Battle completion was not applied: {error}");
            Ensure(
                completedNode == current
                && newlyCompleted
                && current.IsCompleted
                && next.IsAvailable,
                "Battle completion did not complete the exact node "
                + "and unlock its successor.");
            Ensure(
                stageSession.RuntimeData.currentNode == current,
                "Stage runtime lost its current node after completion.");
            Ensure(
                !battleSession.TryGetCompletedStageNodeId(out _),
                "Battle completion was consumed more than once.");
            Ensure(
                !stageSession.TryApplyCompletedBattleNode(
                    battleSession,
                    out _,
                    out _,
                    out _),
                "Consumed battle completion was applied twice.");
        }

        private static void VerifyIconResolver(
            ShrineConfigSO shrineConfig,
            ShrineGodSO lifeGod,
            ShrineGodSO warGod,
            BattleSO battle,
            ShopItemPoolSO pool,
            Sprite battleIcon,
            Sprite shopIcon,
            Sprite eventIcon,
            Sprite multiIcon,
            Sprite lifeIcon)
        {
            ChoiceExecutionIconResolver resolver =
                new(
                    type => type switch
                    {
                        ChoiceExecutionType.Battle =>
                            battleIcon,
                        ChoiceExecutionType.Shop =>
                            shopIcon,
                        ChoiceExecutionType.CompleteEvent =>
                            eventIcon,
                        _ => null
                    },
                    () => multiIcon);
            PopupEventSO root =
                ScriptableObject.CreateInstance<PopupEventSO>();
            PopupEventSO next =
                ScriptableObject.CreateInstance<PopupEventSO>();

            try
            {
                root.choices.Add(
                    CreateBattleChoice("battle.1", battle));
                root.choices.Add(
                    CreateBattleChoice("battle.2", battle));
                Ensure(
                    resolver.Resolve(root) == battleIcon,
                    "Same Battle terminals did not use Battle icon.");

                root.choices.Clear();
                root.choices.Add(
                    CreateBattleChoice("battle", battle));
                root.choices.Add(
                    CreateShopChoice("shop", pool));
                Ensure(
                    resolver.Resolve(root) == multiIcon,
                    "Mixed terminals did not use multi icon.");

                root.choices.Clear();
                root.choices.Add(
                    CreateShrineChoice(
                        "shrine.1",
                        shrineConfig,
                        lifeGod));
                root.choices.Add(
                    CreateShrineChoice(
                        "shrine.2",
                        shrineConfig,
                        lifeGod));
                Ensure(
                    resolver.Resolve(root) == lifeIcon,
                    "Same Shrine God did not use its icon.");

                root.choices.Clear();
                root.choices.Add(
                    CreateShrineChoice(
                        "shrine.life",
                        shrineConfig,
                        lifeGod));
                root.choices.Add(
                    CreateShrineChoice(
                        "shrine.war",
                        shrineConfig,
                        warGod));
                Ensure(
                    resolver.Resolve(root) == multiIcon,
                    "Different Shrine Gods did not use multi icon.");

                next.choices.Add(
                    new PopupEventChoice
                    {
                        choiceId = "complete",
                        executionConfig =
                            ChoiceExecutionDataFactory.CreateConfig(
                                ChoiceExecutionType.CompleteEvent)
                    });
                ChoiceExecutionConfig nextConfig =
                    ChoiceExecutionDataFactory.CreateConfig(
                        ChoiceExecutionType.NextEvent);
                ((NextEventExecutionData)nextConfig.data).nextEvent =
                    next;
                root.choices.Clear();
                root.choices.Add(
                    new PopupEventChoice
                    {
                        choiceId = "next",
                        executionConfig = nextConfig
                    });
                Ensure(
                    resolver.Resolve(root) == eventIcon,
                    "NextEvent chain did not resolve terminal icon.");

                ((NextEventExecutionData)nextConfig.data).nextEvent =
                    root;
                Ensure(
                    resolver.Resolve(root, shopIcon) == shopIcon,
                    "NextEvent cycle did not return fallback.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                UnityEngine.Object.DestroyImmediate(next);
            }
        }

        private static PopupEventChoice CreateBattleChoice(
            string choiceId,
            BattleSO battle)
        {
            ChoiceExecutionConfig config =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Battle);
            ((BattleExecutionData)config.data).battle = battle;
            return new PopupEventChoice
            {
                choiceId = choiceId,
                executionConfig = config
            };
        }

        private static PopupEventChoice CreateShopChoice(
            string choiceId,
            ShopItemPoolSO pool)
        {
            ChoiceExecutionConfig config =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Shop);
            ShopExecutionData data =
                (ShopExecutionData)config.data;
            data.pools.Add(pool);
            data.itemCount = 1;
            return new PopupEventChoice
            {
                choiceId = choiceId,
                executionConfig = config
            };
        }

        private static PopupEventChoice CreateShrineChoice(
            string choiceId,
            ShrineConfigSO shrineConfig,
            ShrineGodSO god)
        {
            ChoiceExecutionConfig config =
                ChoiceExecutionDataFactory.CreateConfig(
                    ChoiceExecutionType.Shrine);
            ShrineExecutionData data =
                (ShrineExecutionData)config.data;
            data.config = shrineConfig;
            data.god = god;
            return new PopupEventChoice
            {
                choiceId = choiceId,
                executionConfig = config
            };
        }

        private static Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 1f, 1f),
                Vector2.zero);
        }

        private static void ConfigureGod(
            ShrineGodSO god,
            ShrineGodType targetType,
            Sprite icon)
        {
            SerializedObject godObject = new(god);
            SerializedProperty godType =
                godObject.FindProperty("godType");
            SerializedProperty iconProperty =
                godObject.FindProperty("icon");
            godType.intValue = (int)targetType;
            iconProperty.objectReferenceValue = icon;
            godObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureShrine(
            ShrineConfigSO shrineConfig,
            params ShrineGodSO[] godsToRegister)
        {
            SerializedObject configObject = new(shrineConfig);
            SerializedProperty gods =
                configObject.FindProperty("gods");
            gods.arraySize = godsToRegister.Length;

            for (int i = 0; i < godsToRegister.Length; i++)
            {
                gods.GetArrayElementAtIndex(i).objectReferenceValue =
                    godsToRegister[i];
            }

            configObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureSuccess(
            ChoiceExecutionRouter router,
            string executionId,
            ChoiceExecutionConfig config,
            ChoiceExecutionContext context)
        {
            ChoiceExecutionResult result =
                router.TryExecute(
                    executionId,
                    config,
                    context,
                    out string error);

            Ensure(
                result == ChoiceExecutionResult.Success,
                $"{executionId} failed: {result}, {error}");
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
