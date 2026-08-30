using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Item;
using Progression.Portfolio;
using Session;

namespace Stage
{
    public sealed class PortfolioOutcomeRuntimeService
    {
        public bool CanExecuteChoice(
            PortfolioOutcomeExecutionData data,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (data?.operations == null || data.operations.Count == 0)
            {
                error = "PORTFOLIO_OUTCOME_PAYLOAD_INVALID";
                return false;
            }
            PortfolioOutcomeOperationData routeBound = data.operations.FirstOrDefault(item =>
                item?.kind == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute
                || item?.kind == PortfolioOutcomeOperationKind.RelicRouteTrade
                || (item?.kind == PortfolioOutcomeOperationKind.RevealImmediateSuccessorPurpose
                    && !string.IsNullOrWhiteSpace(item.snapshotId)));
            if (routeBound != null && !TryEnsureRouteSnapshot(data, routeBound, out error))
                return false;
            if (string.Equals(data?.eventId,
                    "event.act1.random_event.40.jihan_empty_medicine_folio",
                    StringComparison.Ordinal)
                && !HasCharacter("character.jihan"))
            {
                disabledCopy = "지한이 함께하지 않아 이 선택을 실행할 수 없습니다.";
                error = "EVENT40_REQUIRED_CHARACTER_MISSING";
                return false;
            }
            if (string.Equals(data?.eventId,
                    "event.act1.random_event.41.yujin_broken_arrow_fletching",
                    StringComparison.Ordinal)
                && !HasCharacter("character.yujin"))
            {
                disabledCopy = "유진이 함께하지 않아 이 선택을 실행할 수 없습니다.";
                error = "EVENT41_REQUIRED_CHARACTER_MISSING";
                return false;
            }
            if (IsEvent37BattleChoice(data))
                return CanExecuteEvent37(data, out disabledCopy, out error);
            if (IsEvent31BattleChoice(data))
                return CanExecuteEvent31(data, out disabledCopy, out error);
            if (IsEvent36AtomicPair(data))
                return CanExecuteEvent36(data, out disabledCopy, out error);
            if (IsEvent39(data) && !IsEvent39Choice(data))
            {
                error = "EVENT39_ENTITLEMENT_PAYLOAD_INVALID";
                return false;
            }
            if (IsEvent36(data) && data?.operations?.Count != 1)
            {
                error = "EVENT36_ATOMIC_PAYLOAD_INVALID";
                return false;
            }
            if (!IsEvent25(data))
            {
                PortfolioOutcomeOperationData operation = data?.operations?.Count == 1
                    ? data.operations[0]
                    : null;
                if (operation?.kind == PortfolioOutcomeOperationKind.InventoryGrant
                    && operation.relic != null)
                {
                    if (ItemManager.Instance == null)
                    {
                        error = "PORTFOLIO_OUTCOME_INVENTORY_PREVALIDATION_UNAVAILABLE";
                        return false;
                    }
                    return EvaluateDirectRelicPrevalidation(operation.relic,
                        ItemManager.Instance.HasRelic(operation.relic),
                        out disabledCopy, out error);
                }
                if (operation?.kind == PortfolioOutcomeOperationKind.VitalDelta
                    && operation.maxHpPercent > 0 && !operation.allowEffectiveZero)
                {
                    IReadOnlyList<CharacterRuntimeData> members =
                        GameSession.Instance?.BattleSession?.PartyRuntimeData?.Members;
                    if (!TryReadVitalPrevalidation(members, out var vitals, out error))
                        return false;
                    return EvaluatePositiveVitalPrevalidation(vitals,
                        out disabledCopy, out error);
                }
                if (operation?.kind == PortfolioOutcomeOperationKind.GoldSpend)
                {
                    Currency.CurrencyRutimeData wallet =
                        GameSession.Instance?.StageSession?.CurrencyRuntimeData;
                    if (wallet == null || operation.amount <= 0)
                    {
                        error = "PORTFOLIO_OUTCOME_WALLET_INVALID";
                        return false;
                    }
                    if (wallet.gold >= operation.amount) return true;
                    disabledCopy = "Gold가 부족해 이 선택을 실행할 수 없습니다.";
                    error = "PORTFOLIO_OUTCOME_GOLD_INSUFFICIENT";
                    return false;
                }
                if (data.operations.Any(item => item?.kind == PortfolioOutcomeOperationKind.GoldGrant))
                {
                    Currency.CurrencyRutimeData wallet =
                        GameSession.Instance?.StageSession?.CurrencyRuntimeData;
                    long total = data.operations.Where(item =>
                            item?.kind == PortfolioOutcomeOperationKind.GoldGrant)
                        .Sum(item => (long)item.amount);
                    if (wallet == null || total <= 0 || wallet.gold + total > int.MaxValue)
                    {
                        error = "PORTFOLIO_OUTCOME_GOLD_OVERFLOW";
                        return false;
                    }
                }
                return true;
            }
            PortfolioOutcomeOperationData grant = data.operations?
                .SingleOrDefault(item => item.kind == PortfolioOutcomeOperationKind.InventoryGrant);
            if (grant == null || ItemManager.Instance == null)
            {
                error = "EVENT25_RELIC_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            if (!TryValidateEvent25Pool(grant, out error)) return false;
            if (Event25EligibleRelics(grant).Length > 0) return true;
            disabledCopy = Event25RelicClaimService.EligibleZeroCopy;
            error = "EVENT25_RELIC_ELIGIBLE_ZERO";
            return false;
        }

        private static bool HasCharacter(string requiredId)
        {
            IReadOnlyList<CharacterRuntimeData> members =
                GameSession.Instance?.BattleSession?.PartyRuntimeData?.Members;
            return members != null && members.Any(member =>
            {
                string id = member?.characterSO?.CharacterId;
                return string.Equals(id, requiredId, StringComparison.Ordinal)
                    || id?.StartsWith(requiredId + ".", StringComparison.Ordinal) == true;
            });
        }

        private static bool CanExecuteEvent37(
            PortfolioOutcomeExecutionData data,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (!IsEvent37BattlePair(data))
            {
                error = "EVENT37_BATTLE_PAIR_INVALID";
                return false;
            }
            PortfolioOutcomeOperationData grant = data.operations[1];
            if (grant.relic == null || ItemManager.Instance == null)
            {
                error = "PORTFOLIO_OUTCOME_INVENTORY_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            return EvaluateDirectRelicPrevalidation(grant.relic,
                ItemManager.Instance.HasRelic(grant.relic),
                out disabledCopy, out error);
        }

        private static bool CanExecuteEvent31(
            PortfolioOutcomeExecutionData data,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (!IsEvent31BattlePair(data))
            {
                error = "EVENT31_BATTLE_PAIR_INVALID";
                return false;
            }
            PortfolioOutcomeOperationData grant = data.operations[1];
            if (grant.relic == null || ItemManager.Instance == null)
            {
                error = "PORTFOLIO_OUTCOME_INVENTORY_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            if (ItemManager.Instance.HasRelic(grant.relic))
            {
                disabledCopy = Chapter1Event31SelectionContract.OwnedDisabledCopy;
                error = "PORTFOLIO_OUTCOME_INVENTORY_ALREADY_OWNED";
                return false;
            }
            return true;
        }

        private static bool CanExecuteEvent36(
            PortfolioOutcomeExecutionData data,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (data.operations?.Count != 2
                || data.operations[0]?.kind != PortfolioOutcomeOperationKind.VitalDelta
                || data.operations[1]?.kind != PortfolioOutcomeOperationKind.InventoryGrant)
            {
                error = "EVENT36_ATOMIC_PAYLOAD_INVALID";
                return false;
            }
            PortfolioOutcomeOperationData grant = data.operations[1];
            if (grant.relic == null || ItemManager.Instance == null)
            {
                error = "PORTFOLIO_OUTCOME_INVENTORY_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            if (!EvaluateDirectRelicPrevalidation(grant.relic,
                    ItemManager.Instance.HasRelic(grant.relic),
                    out disabledCopy, out error))
                return false;

            IReadOnlyList<CharacterRuntimeData> members =
                GameSession.Instance?.BattleSession?.PartyRuntimeData?.Members;
            if (!TryReadVitalPrevalidation(members, out var vitals, out error)) return false;
            return EvaluateEvent36VitalCost(vitals, out disabledCopy, out error);
        }

        internal static bool EvaluateDirectRelicPrevalidation(
            RelicSO relic,
            bool alreadyOwned,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (relic == null)
            {
                error = "PORTFOLIO_OUTCOME_INVENTORY_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            if (!alreadyOwned) return true;
            disabledCopy = DirectRelicDisabledCopy(relic);
            error = "PORTFOLIO_OUTCOME_INVENTORY_ALREADY_OWNED";
            return false;
        }

        internal static bool EvaluatePositiveVitalPrevalidation(
            IReadOnlyList<(bool isDead, float current, float max)> vitals,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (vitals == null || vitals.Count == 0)
            {
                error = "PORTFOLIO_OUTCOME_VITAL_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            if (!vitals.Any(item => !item.isDead))
            {
                error = "PORTFOLIO_OUTCOME_NO_LIVING_MEMBER";
                return false;
            }
            if (vitals.Any(item => !item.isDead && item.current < item.max)) return true;
            disabledCopy = "이미 모든 생존자가 온전해 회복할 필요가 없습니다.";
            error = "PORTFOLIO_OUTCOME_VITAL_NO_EFFECT";
            return false;
        }

        internal static bool EvaluateEvent36VitalCost(
            IReadOnlyList<(bool isDead, float current, float max)> vitals,
            out string disabledCopy,
            out string error)
        {
            disabledCopy = string.Empty;
            error = string.Empty;
            if (vitals == null || vitals.Count == 0 || !vitals.Any(item => !item.isDead))
            {
                error = "PORTFOLIO_OUTCOME_VITAL_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            bool payable = vitals.Where(item => !item.isDead).All(item =>
                item.current - UnityEngine.Mathf.CeilToInt(item.max * 0.10f) >= 1f);
            if (payable) return true;
            disabledCopy = "모든 생존자가 비용을 전액 지불하고 HP 1 이상 남아야 합니다.";
            error = "EVENT36_VITAL_COST_UNPAYABLE";
            return false;
        }

        private static bool TryReadVitalPrevalidation(
            IReadOnlyList<CharacterRuntimeData> members,
            out IReadOnlyList<(bool isDead, float current, float max)> vitals,
            out string error)
        {
            error = string.Empty;
            var result = new List<(bool isDead, float current, float max)>();
            vitals = result;
            if (members == null || members.Count == 0 || members.Any(item => item == null))
            {
                error = "PORTFOLIO_OUTCOME_VITAL_PREVALIDATION_UNAVAILABLE";
                return false;
            }
            foreach (CharacterRuntimeData member in members)
            {
                if (member.isDead)
                {
                    result.Add((true, 0f, 0f));
                    continue;
                }
                if (member.TryReadExactVitalStats(out float current, out float max)
                    != CharacterVitalCasResult.Applied)
                {
                    error = "PORTFOLIO_OUTCOME_VITAL_PREVALIDATION_UNAVAILABLE";
                    return false;
                }
                result.Add((false, current, max));
            }
            return true;
        }

        private static string DirectRelicDisabledCopy(RelicSO relic) => relic.relicId switch
        {
            "item.relic.frozen_nail" =>
                "이미 ‘얼어붙은 못’을 지니고 있어 달빛을 다시 굳힐 수 없습니다.",
            "item.relic.toxic_pouch" =>
                "이미 ‘독주머니’를 지니고 있어 같은 독기를 거둘 수 없습니다.",
            "item.relic.spiked_carapace" =>
                "이미 ‘가시 갑각’을 지니고 있어 같은 거울 조각을 품을 수 없습니다.",
            "item.relic.ember_necklace" =>
                "이미 ‘잿불 목걸이’를 지니고 있어 수호령의 보상을 받을 수 없습니다.",
            _ => "이미 보유한 유물입니다."
        };

        public bool TryExecute(
            PortfolioOutcomeExecutionData data,
            GameSession gameSession,
            RoundNode node,
            PopupEventSO popup,
            Func<bool> completeEvent,
            Func<Battle.BattleSO, bool> beginBattle,
            out string error)
        {
            error = string.Empty;
            if (!TryValidateIdentity(data, gameSession, node, popup, out error)) return false;

            PortfolioOutcomeOwnership ownership = gameSession.StageSession.PortfolioOutcomes;
            string terminalKey = CreateTerminalKey(data);
            if (ownership.IsResolved(terminalKey)) return true;

            PortfolioOutcomeOperationData battle = data.operations
                .SingleOrDefault(item => item.kind == PortfolioOutcomeOperationKind.BeginBattle);
            if (battle != null)
            {
                if (IsEvent37(data) && !IsEvent37BattlePair(data))
                {
                    error = "EVENT37_BATTLE_PAIR_INVALID";
                    return false;
                }
                return TryBeginBattle(data, battle, ownership, gameSession, node,
                    beginBattle, out error);
            }

            if (!TryApplyImmediate(data, ownership, gameSession, node, terminalKey,
                    out Action rollback, out error)) return false;
            if (completeEvent == null || !completeEvent())
            {
                rollback?.Invoke();
                error = "PORTFOLIO_OUTCOME_NODE_COMPLETE_FAILED";
                return false;
            }
            return true;
        }

        public bool TryFinalizeCompletedBattle(
            GameSession gameSession,
            string nodeId,
            out string error)
        {
            error = string.Empty;
            PortfolioOutcomeOwnership ownership = gameSession?.StageSession?.PortfolioOutcomes;
            PortfolioOutcomePendingBattle pending = ownership?.PendingBattle;
            if (pending == null) return true;
            if (!string.Equals(pending.nodeInstanceId, nodeId, StringComparison.Ordinal))
            {
                error = "PORTFOLIO_OUTCOME_BATTLE_NODE_MISMATCH";
                return false;
            }

            PortfolioOutcomeOperationData continuation = pending.outcome.operations[1];
            if (continuation.kind == PortfolioOutcomeOperationKind.GoldGrant)
                return TryFinalizeBattleGold(gameSession, pending, continuation, out error);
            if (continuation.kind == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute)
                return TryFinalizeBattleRoute(gameSession, pending, continuation, out error);
            PortfolioOutcomeOperationData grant = continuation;
            RelicSO relic;
            if (IsEvent25(pending.outcome))
            {
                if (pending.rewardClaimState == 0)
                    pending.rewardClaimState = Event25RelicClaimState.BattleVictoryCommitted;
                relic = ResolveOrFixEvent25Relic(pending, grant, nodeId);
                if (relic == null)
                {
                    pending.rewardClaimState = Event25RelicClaimState.RelicClaimPendingRetry;
                    error = "EVENT25_RELIC_CLAIM_PENDING_RETRY";
                    return false;
                }
                pending.rewardClaimState = Event25RelicClaimState.Granting;
            }
            else relic = ResolveRelic(grant);
            if (relic == null || ItemManager.Instance == null || ItemManager.Instance.HasRelic(relic)
                || !ItemManager.Instance.AddRelic(relic))
            {
                if (IsEvent25(pending.outcome))
                    pending.rewardClaimState = Event25RelicClaimState.RelicClaimPendingRetry;
                error = "PORTFOLIO_OUTCOME_BATTLE_GRANT_FAILED";
                return false;
            }

            string terminalKey = CreateTerminalKey(pending.outcome);
            if (!ownership.TryCommitTerminal(terminalKey))
            {
                ItemManager.Instance.RemoveRelic(relic);
                if (IsEvent25(pending.outcome))
                    pending.rewardClaimState = Event25RelicClaimState.RelicClaimPendingRetry;
                error = "PORTFOLIO_OUTCOME_TERMINAL_CONFLICT";
                return false;
            }
            if (IsEvent25(pending.outcome))
                pending.rewardClaimState = Event25RelicClaimState.GrantedTerminal;
            return true;
        }

        public bool CommitFinalizedBattle(GameSession gameSession, string nodeId) =>
            gameSession?.StageSession?.PortfolioOutcomes?.ConsumeBattle(nodeId) != null;

        public bool RollbackFinalizedBattle(GameSession gameSession, string nodeId)
        {
            PortfolioOutcomePendingBattle pending =
                gameSession?.StageSession?.PortfolioOutcomes?.PendingBattle;
            if (pending == null || !string.Equals(pending.nodeInstanceId, nodeId,
                    StringComparison.Ordinal)) return false;
            PortfolioOutcomeOperationData grant = pending.outcome.operations[1];
            if (grant.kind == PortfolioOutcomeOperationKind.GoldGrant)
            {
                Currency.CurrencyRutimeData wallet = gameSession.StageSession.CurrencyRuntimeData;
                bool restored = wallet != null && wallet.TryRestoreSnapshot(
                    new Currency.CurrencyRuntimeSnapshot(pending.goldBefore,
                        pending.walletRevisionBefore));
                bool terminalGold = gameSession.StageSession.PortfolioOutcomes.TryRollbackTerminal(
                    CreateTerminalKey(pending.outcome));
                pending.continuationApplied = false;
                return restored && terminalGold;
            }
            if (grant.kind == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute)
            {
                StageGraph graph = gameSession.StageSession.RuntimeData?.currentGraph;
                bool restored = graph != null && graph.TryRollbackImmediateSuccessorRoute(
                    new StageRouteCommitSnapshot(pending.routeRevisionBefore,
                        pending.routeSourceBefore, pending.routeTargetBefore));
                bool terminalRoute = gameSession.StageSession.PortfolioOutcomes.TryRollbackTerminal(
                    CreateTerminalKey(pending.outcome));
                pending.continuationApplied = false;
                return restored && terminalRoute;
            }
            RelicSO relic = IsEvent25(pending.outcome)
                ? ResolveFixedEvent25Relic(pending, grant)
                : ResolveRelic(grant);
            bool removed = relic != null && ItemManager.Instance != null
                && ItemManager.Instance.RemoveRelic(relic);
            bool terminal = gameSession.StageSession.PortfolioOutcomes.TryRollbackTerminal(
                CreateTerminalKey(pending.outcome));
            if (IsEvent25(pending.outcome))
                pending.rewardClaimState = Event25RelicClaimState.RelicClaimPendingRetry;
            return removed && terminal;
        }

        private static bool TryApplyImmediate(
            PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOwnership ownership,
            GameSession gameSession,
            RoundNode node,
            string terminalKey,
            out Action rollback,
            out string error)
        {
            rollback = null;
            error = string.Empty;
            if (IsEvent36AtomicPair(data))
                return TryApplyEvent36(data, ownership, gameSession, terminalKey,
                    out rollback, out error);
            if (data.operations.Count > 1
                || data.operations[0].kind == PortfolioOutcomeOperationKind.GoldSpend
                || data.operations[0].kind == PortfolioOutcomeOperationKind.GoldGrant
                || data.operations[0].kind == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute)
                return TryApplyAtomicOperations(data, ownership, gameSession, terminalKey,
                    out rollback, out error);
            PortfolioOutcomeOperationData operation = data.operations.Single();
            switch (operation.kind)
            {
                case PortfolioOutcomeOperationKind.VitalDelta:
                    if (!TryApplyVital(operation, gameSession, out rollback, out error)) return false;
                    break;
                case PortfolioOutcomeOperationKind.InventoryGrant:
                    RelicSO relic = ResolveRelic(operation);
                    if (relic == null || ItemManager.Instance == null || ItemManager.Instance.HasRelic(relic)
                        || !ItemManager.Instance.AddRelic(relic))
                    {
                        error = "PORTFOLIO_OUTCOME_INVENTORY_FAILED";
                        return false;
                    }
                    PortfolioSceneEntitlementReceipt grantEntitlement = null;
                    if (!string.IsNullOrWhiteSpace(operation.sourceEntitlementId))
                    {
                        grantEntitlement = NewEntitlement(data, operation, relic);
                        if (!ownership.TryReserveEntitlement(grantEntitlement)
                            || !ownership.TryCommitEntitlement(grantEntitlement,
                                PortfolioSceneEntitlementState.GrantedToInventory))
                        {
                            ItemManager.Instance.RemoveRelic(relic);
                            ownership.TryReleaseEntitlement(grantEntitlement);
                            error = "PORTFOLIO_OUTCOME_ENTITLEMENT_GRANT_FAILED";
                            return false;
                        }
                    }
                    rollback = () =>
                    {
                        ItemManager.Instance?.RemoveRelic(relic);
                        ownership.TryRollbackEntitlement(grantEntitlement);
                    };
                    break;
                case PortfolioOutcomeOperationKind.SetRunFlagTrue:
                    if (!ownership.TrySetRunFlag(operation.targetId))
                    {
                        error = "PORTFOLIO_OUTCOME_FLAG_FAILED";
                        return false;
                    }
                    rollback = () => ownership.TryRollbackRunFlag(operation.targetId);
                    break;
                case PortfolioOutcomeOperationKind.RevealImmediateSuccessorPurpose:
                    string[] successors;
                    if (!string.IsNullOrWhiteSpace(operation.snapshotId)
                        && ownership.TryGetRouteSnapshot(operation.snapshotId,
                            out StageRouteCandidateSnapshot disclosureSnapshot))
                        successors = disclosureSnapshot.candidates.Select(item => item.purposeId).ToArray();
                    else successors = gameSession.StageSession.RuntimeData.currentGraph
                        .GetNextNodes(node).Select(item => item.LocalizationMainKey).ToArray();
                    if (!ownership.TryStoreDisclosure(data.reservationId, successors))
                    {
                        error = "PORTFOLIO_OUTCOME_DISCLOSURE_FAILED";
                        return false;
                    }
                    rollback = () => ownership.TryRollbackDisclosure(data.reservationId);
                    break;
                case PortfolioOutcomeOperationKind.RelicRouteTrade:
                    if (!TryApplyRelicRouteTrade(data, operation, ownership, gameSession,
                            out rollback, out error)) return false;
                    break;
                default:
                    error = "PORTFOLIO_OUTCOME_IMMEDIATE_KIND_INVALID";
                    return false;
            }

            if (ownership.TryCommitTerminal(terminalKey))
            {
                Action effectRollback = rollback;
                rollback = () =>
                {
                    ownership.TryRollbackTerminal(terminalKey);
                    effectRollback?.Invoke();
                };
                return true;
            }
            rollback?.Invoke();
            error = "PORTFOLIO_OUTCOME_TERMINAL_CONFLICT";
            return false;
        }

        private static bool TryApplyAtomicOperations(
            PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOwnership ownership,
            GameSession gameSession,
            string terminalKey,
            out Action rollback,
            out string error)
        {
            rollback = null;
            error = string.Empty;
            if (data?.operations == null || data.operations.Count == 0
                || gameSession?.StageSession == null)
            {
                error = "PORTFOLIO_ATOMIC_PAYLOAD_INVALID";
                return false;
            }

            var rollbacks = new List<Action>();
            foreach (PortfolioOutcomeOperationData operation in data.operations)
            {
                Action operationRollback;
                switch (operation?.kind)
                {
                    case PortfolioOutcomeOperationKind.VitalDelta:
                        if (!TryApplyVital(operation, gameSession, out operationRollback, out error,
                                requireFullNonlethalCost: operation.maxHpPercent < 0))
                        {
                            RollbackAll(rollbacks);
                            return false;
                        }
                        break;
                    case PortfolioOutcomeOperationKind.SetRunFlagTrue:
                        if (!ownership.TrySetRunFlag(operation.targetId))
                        {
                            error = "PORTFOLIO_OUTCOME_FLAG_FAILED";
                            RollbackAll(rollbacks);
                            return false;
                        }
                        operationRollback = () => ownership.TryRollbackRunFlag(operation.targetId);
                        break;
                    case PortfolioOutcomeOperationKind.GoldSpend:
                    case PortfolioOutcomeOperationKind.GoldGrant:
                        Currency.CurrencyRutimeData wallet = gameSession.StageSession.CurrencyRuntimeData;
                        if (wallet == null || operation.amount <= 0)
                        {
                            error = "PORTFOLIO_OUTCOME_WALLET_INVALID";
                            RollbackAll(rollbacks);
                            return false;
                        }
                        Currency.CurrencyRuntimeSnapshot snapshot = wallet.CaptureSnapshot();
                        bool applied = operation.kind == PortfolioOutcomeOperationKind.GoldSpend
                            ? wallet.TrySpendGold(operation.amount)
                            : wallet.TryAddGoldExact(operation.amount);
                        if (!applied)
                        {
                            error = operation.kind == PortfolioOutcomeOperationKind.GoldSpend
                                ? "PORTFOLIO_OUTCOME_GOLD_INSUFFICIENT"
                                : "PORTFOLIO_OUTCOME_GOLD_OVERFLOW";
                            RollbackAll(rollbacks);
                            return false;
                        }
                        operationRollback = () => wallet.TryRestoreSnapshot(snapshot);
                        break;
                    case PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute:
                        StageGraph graph = gameSession.StageSession.RuntimeData?.currentGraph;
                        if (graph == null || !ownership.TryGetRouteSnapshot(operation.snapshotId,
                                out StageRouteCandidateSnapshot candidates)
                            || !graph.TryCommitImmediateSuccessorRoute(candidates,
                                operation.selectionMode, out StageRouteCommitSnapshot routeSnapshot,
                                out error))
                        {
                            if (string.IsNullOrWhiteSpace(error))
                                error = "PORTFOLIO_OUTCOME_ROUTE_COMMIT_FAILED";
                            RollbackAll(rollbacks);
                            return false;
                        }
                        operationRollback = () => graph.TryRollbackImmediateSuccessorRoute(routeSnapshot);
                        break;
                    default:
                        error = "PORTFOLIO_ATOMIC_OPERATION_INVALID";
                        RollbackAll(rollbacks);
                        return false;
                }
                rollbacks.Add(operationRollback);
            }

            if (!ownership.TryCommitTerminal(terminalKey))
            {
                RollbackAll(rollbacks);
                error = "PORTFOLIO_OUTCOME_TERMINAL_CONFLICT";
                return false;
            }
            rollback = () =>
            {
                ownership.TryRollbackTerminal(terminalKey);
                RollbackAll(rollbacks);
            };
            return true;
        }

        private static void RollbackAll(IReadOnlyList<Action> rollbacks)
        {
            for (int i = rollbacks.Count - 1; i >= 0; i--) rollbacks[i]?.Invoke();
        }

        private static bool TryApplyEvent36(
            PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOwnership ownership,
            GameSession gameSession,
            string terminalKey,
            out Action rollback,
            out string error)
        {
            rollback = null;
            error = string.Empty;
            if (data.operations?.Count != 2)
            {
                error = "EVENT36_ATOMIC_PAYLOAD_INVALID";
                return false;
            }
            if (!TryApplyVital(data.operations[0], gameSession, out Action vitalRollback,
                    out error, requireFullNonlethalCost: true))
                return false;

            RelicSO relic = ResolveRelic(data.operations[1]);
            if (relic == null || ItemManager.Instance == null || ItemManager.Instance.HasRelic(relic)
                || !ItemManager.Instance.AddRelic(relic))
            {
                vitalRollback?.Invoke();
                error = "PORTFOLIO_OUTCOME_INVENTORY_FAILED";
                return false;
            }
            Action effectRollback = () =>
            {
                ItemManager.Instance?.RemoveRelic(relic);
                vitalRollback?.Invoke();
            };
            if (!ownership.TryCommitTerminal(terminalKey))
            {
                effectRollback();
                error = "PORTFOLIO_OUTCOME_TERMINAL_CONFLICT";
                return false;
            }
            rollback = () =>
            {
                ownership.TryRollbackTerminal(terminalKey);
                effectRollback();
            };
            return true;
        }

        private static bool TryApplyVital(
            PortfolioOutcomeOperationData operation,
            GameSession gameSession,
            out Action rollback,
            out string error,
            bool requireFullNonlethalCost = false)
        {
            rollback = null;
            error = string.Empty;
            IReadOnlyList<CharacterRuntimeData> members = gameSession.BattleSession?.PartyRuntimeData?.Members;
            if (members == null || members.Count == 0 || members.Any(item => item == null))
            {
                error = "PORTFOLIO_OUTCOME_ROSTER_INVALID";
                return false;
            }

            List<(CharacterRuntimeData member, float before, float after)> plan = new();
            foreach (CharacterRuntimeData member in members)
            {
                if (member.isDead) continue;
                if (member.TryReadExactVitalStats(out float current, out float max)
                    != CharacterVitalCasResult.Applied)
                {
                    error = "PORTFOLIO_OUTCOME_VITAL_READ_FAILED";
                    return false;
                }
                int amount = UnityEngine.Mathf.CeilToInt(max * Math.Abs(operation.maxHpPercent) / 100f);
                if (requireFullNonlethalCost && current - amount < 1f)
                {
                    error = "EVENT36_VITAL_COST_UNPAYABLE";
                    return false;
                }
                float after = operation.maxHpPercent > 0
                    ? Math.Max(current, Math.Min(max, current + amount))
                    : Math.Max(operation.nonlethal ? 1f : 0f, current - amount);
                plan.Add((member, current, after));
            }
            if (plan.Count == 0) { error = "PORTFOLIO_OUTCOME_NO_LIVING_MEMBER"; return false; }

            List<(CharacterRuntimeData member, float before, float after)> applied = new();
            foreach (var mutation in plan)
            {
                if (mutation.member.TryCompareExchangeCurrentHp(mutation.before, mutation.after)
                    != CharacterVitalCasResult.Applied)
                {
                    RestoreVitals(applied);
                    error = "PORTFOLIO_OUTCOME_VITAL_APPLY_FAILED";
                    return false;
                }
                applied.Add(mutation);
            }
            rollback = () => RestoreVitals(applied);
            return true;
        }

        private static void RestoreVitals(
            IReadOnlyList<(CharacterRuntimeData member, float before, float after)> applied)
        {
            for (int i = applied.Count - 1; i >= 0; i--)
                applied[i].member.TryCompareExchangeCurrentHp(applied[i].after, applied[i].before);
        }

        private static bool TryBeginBattle(
            PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOperationData battle,
            PortfolioOutcomeOwnership ownership,
            GameSession gameSession,
            RoundNode node,
            Func<Battle.BattleSO, bool> beginBattle,
            out string error)
        {
            error = string.Empty;
            if (data.operations?.Count != 2)
            {
                error = "PORTFOLIO_OUTCOME_BATTLE_CONTINUATION_INVALID";
                return false;
            }
            PortfolioOutcomeOperationData grant = data.operations[1];
            RelicSO relic = IsEvent25(data) ? null : ResolveRelic(grant);
            bool inventoryContinuation = grant.kind == PortfolioOutcomeOperationKind.InventoryGrant;
            if (IsEvent25(data) && !TryValidateEvent25Pool(grant, out error)) return false;
            if (inventoryContinuation && (ItemManager.Instance == null
                || (IsEvent25(data) && Event25EligibleRelics(grant).Length == 0)
                || (!IsEvent25(data) && (relic == null || ItemManager.Instance.HasRelic(relic)))))
            {
                error = IsEvent25(data)
                    ? "EVENT25_RELIC_ELIGIBLE_ZERO"
                    : "PORTFOLIO_OUTCOME_BATTLE_GRANT_INVALID";
                return false;
            }
            PortfolioOutcomePendingBattle pending = new()
            {
                runId = gameSession.ProgressionSession != null
                    ? gameSession.ProgressionSession.RunId.Value : string.Empty,
                stageGenerationId = gameSession.StageSession.RandomGrowthSession?.StageGenerationId
                    ?? string.Empty,
                nodeInstanceId = node.nodeId,
                outcome = data,
                fixedInventoryId = relic != null ? relic.name : string.Empty,
                rewardClaimState = 0,
                continuationKind = grant.kind
            };
            if (!ownership.TryPrepareBattle(pending))
            {
                error = "PORTFOLIO_OUTCOME_BATTLE_BUSY";
                return false;
            }
            if (beginBattle != null && beginBattle(battle.battle)) return true;
            ownership.AbortBattle(pending);
            error = "PORTFOLIO_OUTCOME_BATTLE_BEGIN_FAILED";
            return false;
        }

        private static bool TryFinalizeBattleGold(
            GameSession gameSession,
            PortfolioOutcomePendingBattle pending,
            PortfolioOutcomeOperationData grant,
            out string error)
        {
            error = string.Empty;
            Currency.CurrencyRutimeData wallet = gameSession.StageSession.CurrencyRuntimeData;
            string terminalKey = CreateTerminalKey(pending.outcome);
            if (gameSession.StageSession.PortfolioOutcomes.IsResolved(terminalKey)) return true;
            if (wallet == null || grant.amount <= 0)
            {
                error = "PORTFOLIO_OUTCOME_BATTLE_GOLD_INVALID";
                return false;
            }
            if (!pending.continuationApplied)
            {
                Currency.CurrencyRuntimeSnapshot snapshot = wallet.CaptureSnapshot();
                if (!wallet.TryAddGoldExact(grant.amount))
                {
                    error = "PORTFOLIO_OUTCOME_BATTLE_GOLD_PENDING_RETRY";
                    return false;
                }
                pending.goldBefore = snapshot.Gold;
                pending.walletRevisionBefore = snapshot.Revision;
                pending.continuationApplied = true;
            }
            if (gameSession.StageSession.PortfolioOutcomes.TryCommitTerminal(terminalKey)) return true;
            wallet.TryRestoreSnapshot(new Currency.CurrencyRuntimeSnapshot(
                pending.goldBefore, pending.walletRevisionBefore));
            pending.continuationApplied = false;
            error = "PORTFOLIO_OUTCOME_TERMINAL_CONFLICT";
            return false;
        }

        private static bool TryFinalizeBattleRoute(
            GameSession gameSession,
            PortfolioOutcomePendingBattle pending,
            PortfolioOutcomeOperationData route,
            out string error)
        {
            error = string.Empty;
            StageGraph graph = gameSession.StageSession.RuntimeData?.currentGraph;
            string terminalKey = CreateTerminalKey(pending.outcome);
            if (gameSession.StageSession.PortfolioOutcomes.IsResolved(terminalKey)) return true;
            RoundNode source = graph?.CurrentNode;
            if (graph == null || source == null)
            {
                error = "PORTFOLIO_OUTCOME_BATTLE_ROUTE_UNAVAILABLE";
                return false;
            }
            if (!pending.continuationApplied)
            {
                if (!gameSession.StageSession.PortfolioOutcomes.TryGetRouteSnapshot(route.snapshotId,
                        out StageRouteCandidateSnapshot candidates)
                    || !graph.TryCommitImmediateSuccessorRoute(candidates, route.selectionMode,
                        out StageRouteCommitSnapshot snapshot, out error)) return false;
                pending.routeRevisionBefore = snapshot.Revision;
                pending.routeSourceBefore = snapshot.SourceNodeId;
                pending.routeTargetBefore = snapshot.TargetNodeId;
                pending.continuationApplied = true;
            }
            if (gameSession.StageSession.PortfolioOutcomes.TryCommitTerminal(terminalKey)) return true;
            graph.TryRollbackImmediateSuccessorRoute(new StageRouteCommitSnapshot(
                pending.routeRevisionBefore, pending.routeSourceBefore, pending.routeTargetBefore));
            pending.continuationApplied = false;
            error = "PORTFOLIO_OUTCOME_TERMINAL_CONFLICT";
            return false;
        }

        private static bool TryEnsureRouteSnapshot(
            PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOperationData operation,
            out string error)
        {
            error = string.Empty;
            GameSession session = GameSession.Instance;
            PortfolioOutcomeOwnership ownership = session?.StageSession?.PortfolioOutcomes;
            StageGraph graph = session?.StageSession?.RuntimeData?.currentGraph;
            RoundNode source = graph?.CurrentNode;
            string snapshotId = operation.snapshotId;
            if (ownership == null || graph == null || source == null
                || string.IsNullOrWhiteSpace(snapshotId))
            {
                error = "PORTFOLIO_OUTCOME_ROUTE_SNAPSHOT_UNAVAILABLE";
                return false;
            }
            if (ownership.TryGetRouteSnapshot(snapshotId, out StageRouteCandidateSnapshot existing))
            {
                if (existing.graphRevision == graph.routeRevision
                    && string.Equals(existing.sourceNodeId, source.nodeId, StringComparison.Ordinal))
                    return true;
                error = "PORTFOLIO_OUTCOME_ROUTE_SNAPSHOT_STALE";
                return false;
            }
            if (!graph.TryCreateImmediateSuccessorRouteSnapshot(source, snapshotId,
                    out StageRouteCandidateSnapshot created, out error)) return false;
            if (ownership.TryStoreRouteSnapshot(created)) return true;
            error = "PORTFOLIO_OUTCOME_ROUTE_SNAPSHOT_CONFLICT";
            return false;
        }

        private static RelicSO ResolveRelic(PortfolioOutcomeOperationData operation)
        {
            if (operation.relic != null) return operation.relic;
            return operation.relicPool?.relics?
                .Where(item => item?.relic != null)
                .Select(item => item.relic)
                .OrderBy(item => item.name, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        private static PortfolioSceneEntitlementReceipt NewEntitlement(
            PortfolioOutcomeExecutionData data, PortfolioOutcomeOperationData operation,
            RelicSO relic) => new()
        {
            eventId = data.eventId,
            reservationId = data.reservationId,
            entitlementId = operation.sourceEntitlementId,
            relicId = relic?.relicId,
            state = PortfolioSceneEntitlementState.Pending
        };

        private static bool TryApplyRelicRouteTrade(PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOperationData operation, PortfolioOutcomeOwnership ownership,
            GameSession gameSession, out Action rollback, out string error)
        {
            rollback = null;
            error = string.Empty;
            RelicSO relic = ResolveRelic(operation);
            PortfolioSceneEntitlementReceipt receipt = NewEntitlement(data, operation, relic);
            if (relic == null || !ownership.TryReserveEntitlement(receipt))
            {
                error = "PORTFOLIO_OUTCOME_ENTITLEMENT_RESERVE_FAILED";
                return false;
            }
            StageGraph graph = gameSession.StageSession.RuntimeData?.currentGraph;
            if (graph == null || !ownership.TryGetRouteSnapshot(operation.snapshotId,
                    out StageRouteCandidateSnapshot candidates)
                || !graph.TryCommitImmediateSuccessorRoute(candidates,
                    operation.selectionMode, out StageRouteCommitSnapshot routeSnapshot,
                    out error))
            {
                ownership.TryReleaseEntitlement(receipt);
                if (string.IsNullOrWhiteSpace(error))
                    error = "PORTFOLIO_OUTCOME_ROUTE_COMMIT_FAILED";
                return false;
            }
            if (!ownership.TryCommitEntitlement(receipt,
                    PortfolioSceneEntitlementState.ConsumedForRoute))
            {
                graph.TryRollbackImmediateSuccessorRoute(routeSnapshot);
                ownership.TryReleaseEntitlement(receipt);
                error = "PORTFOLIO_OUTCOME_ENTITLEMENT_COMMIT_FAILED";
                return false;
            }
            rollback = () =>
            {
                graph.TryRollbackImmediateSuccessorRoute(routeSnapshot);
                ownership.TryRollbackEntitlement(receipt);
            };
            return true;
        }

        private static bool IsEvent25(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId, Chapter1Event25SelectionContract.Event25Id,
                StringComparison.Ordinal);

        private static bool IsEvent36(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId,
                "event.act1.random_event.36.cracked_bronze_mirror",
                StringComparison.Ordinal);

        private static bool IsEvent37(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId, Chapter1Event37SelectionContract.Event37Id,
                StringComparison.Ordinal);

        private static bool IsEvent39(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId, Chapter1Event39SelectionContract.Event39Id,
                StringComparison.Ordinal);

        private static bool IsEvent39Choice(PortfolioOutcomeExecutionData data)
        {
            if (!IsEvent39(data) || data.operations?.Count != 1) return false;
            PortfolioOutcomeOperationData operation = data.operations[0];
            return operation != null
                && operation.sourceEntitlementId == Chapter1Event39SelectionContract.EntitlementId
                && operation.relic?.relicId == Chapter1Event39SelectionContract.RelicId
                && operation.relicPool == null
                && (operation.kind == PortfolioOutcomeOperationKind.InventoryGrant
                    || operation.kind == PortfolioOutcomeOperationKind.RelicRouteTrade);
        }

        private static bool IsEvent31(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId, Chapter1Event31SelectionContract.Event31Id,
                StringComparison.Ordinal);

        private static bool IsEvent31BattleChoice(PortfolioOutcomeExecutionData data) =>
            IsEvent31(data) && data?.operations?.Any(operation =>
                operation?.kind == PortfolioOutcomeOperationKind.BeginBattle) == true;

        private static bool IsEvent31BattlePair(PortfolioOutcomeExecutionData data)
        {
            if (!IsEvent31(data) || data.operations?.Count != 2) return false;
            PortfolioOutcomeOperationData battle = data.operations[0];
            PortfolioOutcomeOperationData grant = data.operations[1];
            return battle != null
                && battle.kind == PortfolioOutcomeOperationKind.BeginBattle
                && string.Equals(battle.battle?.battleId,
                    Chapter1Event31SelectionContract.BattleId, StringComparison.Ordinal)
                && grant != null
                && grant.kind == PortfolioOutcomeOperationKind.InventoryGrant
                && grant.count == 1
                && grant.unique
                && grant.relicPool == null
                && string.Equals(grant.relic?.relicId,
                    Chapter1Event31SelectionContract.RelicId, StringComparison.Ordinal);
        }

        private static bool IsEvent37BattleChoice(PortfolioOutcomeExecutionData data) =>
            IsEvent37(data) && data?.operations?.Any(operation =>
                operation?.kind == PortfolioOutcomeOperationKind.BeginBattle) == true;

        private static bool IsEvent37BattlePair(PortfolioOutcomeExecutionData data)
        {
            if (!IsEvent37(data) || data.operations?.Count != 2) return false;
            PortfolioOutcomeOperationData battle = data.operations[0];
            PortfolioOutcomeOperationData grant = data.operations[1];
            return battle != null
                && battle.kind == PortfolioOutcomeOperationKind.BeginBattle
                && string.Equals(battle.battle?.battleId,
                    Chapter1Event37SelectionContract.BattleId, StringComparison.Ordinal)
                && grant != null
                && grant.kind == PortfolioOutcomeOperationKind.InventoryGrant
                && grant.count == 1
                && grant.unique
                && grant.relicPool == null
                && string.Equals(grant.relic?.relicId,
                    Chapter1Event37SelectionContract.RelicId, StringComparison.Ordinal);
        }

        private static bool IsEvent36AtomicPair(PortfolioOutcomeExecutionData data)
        {
            if (!IsEvent36(data) || data.operations?.Count != 2) return false;
            PortfolioOutcomeOperationData vital = data.operations[0];
            PortfolioOutcomeOperationData grant = data.operations[1];
            return vital != null
                && vital.kind == PortfolioOutcomeOperationKind.VitalDelta
                && vital.maxHpPercent == -10
                && vital.nonlethal
                && grant != null
                && grant.kind == PortfolioOutcomeOperationKind.InventoryGrant
                && grant.count == 1
                && grant.unique
                && grant.relicPool == null
                && string.Equals(grant.relic?.relicId, "item.relic.spiked_carapace",
                    StringComparison.Ordinal);
        }

        private static RelicSO[] Event25EligibleRelics(PortfolioOutcomeOperationData grant) =>
            grant?.relicPool?.relics?
                .Where(entry => entry?.relic != null)
                .Select(entry => entry.relic)
                .Distinct()
                .Where(relic => ItemManager.Instance != null && !ItemManager.Instance.HasRelic(relic))
                .OrderBy(relic => relic.relicId, StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<RelicSO>();

        private static bool TryValidateEvent25Pool(
            PortfolioOutcomeOperationData grant,
            out string error)
        {
            error = string.Empty;
            string[] expected =
            {
                "item.relic.blunt_gear",
                "item.relic.electric_crystal",
                "item.relic.ember_necklace",
                "item.relic.frozen_nail",
                "item.relic.spiked_carapace",
                "item.relic.toxic_pouch"
            };
            Item.RelicPoolSO.RelicPoolEntry[] entries = grant?.relicPool?.relics?.ToArray();
            string[] actual = entries?
                .Where(entry => entry?.relic != null)
                .Select(entry => entry.relic.relicId)
                .ToArray() ?? Array.Empty<string>();
            if (entries == null || entries.Length != expected.Length
                || actual.Length != expected.Length
                || !actual.SequenceEqual(expected, StringComparer.Ordinal)
                || actual.Distinct(StringComparer.Ordinal).Count() != expected.Length
                || entries.Any(entry => entry.weight != 100
                    || entry.relic.rarity != RelicRarity.Common
                    || entry.relic.hidden || entry.relic.developerOnly))
            {
                error = "EVENT25_RELIC_POOL_CONTRACT_INVALID";
                return false;
            }
            return true;
        }

        private static RelicSO ResolveOrFixEvent25Relic(
            PortfolioOutcomePendingBattle pending,
            PortfolioOutcomeOperationData grant,
            string battleVictoryReceiptId)
        {
            if (!string.IsNullOrWhiteSpace(pending.fixedInventoryId))
                return ResolveFixedEvent25Relic(pending, grant);
            RelicSO[] eligible = Event25EligibleRelics(grant);
            Event25RelicClaim claim = new Event25RelicClaimService().CreateVictoryClaim(
                pending.runId, pending.stageGenerationId, pending.outcome.reservationId,
                battleVictoryReceiptId, eligible.Select(relic => relic.relicId), Array.Empty<string>());
            if (claim == null) return null;
            pending.fixedInventoryId = claim.selectedRelicId;
            pending.eligibleFingerprint = claim.eligibleFingerprint;
            pending.rewardClaimCauseId = claim.causeId;
            pending.rewardClaimState = Event25RelicClaimState.RelicClaimPending;
            return eligible.SingleOrDefault(relic => string.Equals(
                relic.relicId, pending.fixedInventoryId, StringComparison.Ordinal));
        }

        private static RelicSO ResolveFixedEvent25Relic(
            PortfolioOutcomePendingBattle pending,
            PortfolioOutcomeOperationData grant) =>
            grant?.relicPool?.relics?
                .Where(entry => entry?.relic != null)
                .Select(entry => entry.relic)
                .SingleOrDefault(relic => string.Equals(
                    relic.relicId, pending.fixedInventoryId, StringComparison.Ordinal));

        private static bool TryValidateIdentity(
            PortfolioOutcomeExecutionData data,
            GameSession gameSession,
            RoundNode node,
            PopupEventSO popup,
            out string error)
        {
            error = string.Empty;
            if (data == null || gameSession?.StageSession?.PortfolioOutcomes == null
                || node == null || popup == null) { error = "PORTFOLIO_OUTCOME_RUNTIME_MISSING"; return false; }
            bool popupIdentityMatches = string.Equals(popup.eventId, data.nodeId,
                    StringComparison.Ordinal)
                && string.Equals(popup.eventId, data.sourcePopupId, StringComparison.Ordinal);
            bool wrapperBindingMatches;
            if (node.roundNodeSO != null)
            {
                PopupEventSO boundPopup = node.roundNodeSO.popupEvent;
                bool directBinding = ReferenceEquals(boundPopup, popup)
                    && (node.popupEvent == null || ReferenceEquals(node.popupEvent, popup));
                PortfolioNextEventContinuationReceipt continuation =
                    gameSession.StageSession.PortfolioOutcomes.PendingContinuation;
                bool continuationBinding = continuation?.childOpened == true
                    && string.Equals(continuation.childEventId, data.eventId,
                        StringComparison.Ordinal)
                    && string.Equals(continuation.childNodeId, popup.eventId,
                        StringComparison.Ordinal)
                    && string.Equals(continuation.parentNodeId, boundPopup?.eventId,
                        StringComparison.Ordinal)
                    && (node.popupEvent == null || ReferenceEquals(node.popupEvent, boundPopup));
                wrapperBindingMatches = directBinding || continuationBinding;
            }
            else
            {
                wrapperBindingMatches = string.Equals(node.nodeId, data.nodeId,
                        StringComparison.Ordinal)
                    && (node.popupEvent == null || ReferenceEquals(node.popupEvent, popup));
            }
            if (!popupIdentityMatches || !wrapperBindingMatches)
            {
                error = "PORTFOLIO_OUTCOME_RUNTIME_IDENTITY_MISMATCH";
                return false;
            }
            return true;
        }

        private static string CreateTerminalKey(PortfolioOutcomeExecutionData data) =>
            string.Join("|", data.eventId, data.nodeId, data.sourcePopupId,
                data.choiceId, data.resultId, data.reservationId);
    }
}
