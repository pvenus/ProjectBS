using System.Collections.Generic;
using System.Linq;
using Shop;
using Shrine;
using Progression;

namespace Stage
{
    /// <summary>
    /// 실행 config 자체의 구조와 필수 참조를 검사한다.
    /// 에셋 탐색과 NextEvent 그래프 순환 검사는 Editor validator가 담당한다.
    /// </summary>
    public static class ChoiceExecutionConfigValidator
    {
        public static List<string> Validate(ChoiceExecutionConfig config)
        {
            List<string> errors = new();
            CollectErrors(config, errors);
            return errors;
        }

        public static void CollectErrors(
            ChoiceExecutionConfig config,
            List<string> errors)
        {
            if (errors == null)
            {
                return;
            }

            if (config == null)
            {
                errors.Add("CONFIG_NULL: executionConfig is null.");
                return;
            }

            if (config.executionType == ChoiceExecutionType.None)
            {
                errors.Add("TYPE_NONE: executionType must not be None.");
            }

            if (config.data == null)
            {
                errors.Add("DATA_NULL: execution data is null.");
                return;
            }

            if (!IsTypeMatch(config.executionType, config.data))
            {
                errors.Add(
                    $"TYPE_MISMATCH: {config.executionType} cannot use "
                    + $"{config.data.GetType().Name}.");
                return;
            }

            switch (config.data)
            {
                case NextEventExecutionData nextEventData:
                    if (nextEventData.nextEvent == null)
                    {
                        errors.Add(
                            "NEXT_EVENT_NULL: NextEvent target is required.");
                    }
                    ValidateNextEventContinuation(nextEventData, errors);
                    break;

                case BattleExecutionData battleData:
                    ValidateBattle(battleData, errors);
                    break;

                case ShopExecutionData shopData:
                    ValidateShop(shopData, errors);
                    break;

                case ShrineExecutionData shrineData:
                    ValidateShrine(shrineData, errors);
                    break;

                case RandomGrowthRiskExecutionData riskData:
                    ValidateRandomGrowthRisk(riskData, errors);
                    break;

                case RandomGrowthDeclineExecutionData declineData:
                    ValidateRandomGrowthCommon(declineData, errors);
                    if (declineData.resultKind != "Declined"
                        || declineData.cost != 0
                        || declineData.growthGrant != 0)
                    {
                        errors.Add("RANDOM_GROWTH_DECLINE_CONTRACT: Decline must be terminal 0/0.");
                    }
                    break;

                case RandomGrowthSafeExecutionData safeData:
                    ValidateRandomGrowthSafe(safeData, errors);
                    break;
                case PortfolioOutcomeExecutionData outcome:
                    ValidatePortfolioOutcome(outcome, errors);
                    break;
            }
        }

        private static void ValidateNextEventContinuation(
            NextEventExecutionData data, List<string> errors)
        {
            string[] identity =
            {
                data.parentEventId, data.parentNodeId, data.parentChoiceId,
                data.parentResultId, data.parentReservationId, data.childEventId,
                data.childNodeId, data.childReservationId
            };
            int populated = identity.Count(value => !string.IsNullOrWhiteSpace(value));
            if (populated == 0) return;
            if (populated != identity.Length)
            {
                errors.Add("NEXT_EVENT_CONTINUATION_IDENTITY_PARTIAL");
                return;
            }
            if (!string.Equals(data.parentEventId,
                    "event.act1.random_event.34.half_vein_map",
                    System.StringComparison.Ordinal)
                || !string.Equals(data.childEventId,
                    "event.act1.random_event.34.half_vein_map.followup.unstable_vein",
                    System.StringComparison.Ordinal)
                || !string.Equals(data.nextEvent?.eventId, data.childNodeId,
                    System.StringComparison.Ordinal))
                errors.Add("NEXT_EVENT_CONTINUATION_IDENTITY_INVALID");
        }

        private static void ValidateBattle(
            BattleExecutionData data, List<string> errors)
        {
            if (data.battle == null)
            {
                errors.Add("BATTLE_NULL: BattleSO reference is required.");
                return;
            }

            string[] identity =
            {
                data.eventId,
                data.nodeId,
                data.sourcePopupId,
                data.reservationId,
                data.choiceId,
                data.expectedVictoryResultId
            };
            int populated = identity.Count(value =>
                !string.IsNullOrWhiteSpace(value));
            if (populated == 0)
            {
                return;
            }
            if (populated != identity.Length)
            {
                errors.Add("BATTLE_COMPLETION_IDENTITY_PARTIAL");
                return;
            }
            if (!string.Equals(data.nodeId, data.sourcePopupId,
                    System.StringComparison.Ordinal))
            {
                errors.Add("BATTLE_COMPLETION_NODE_SOURCE_MISMATCH");
                return;
            }
            if (!PortfolioEventIdentityCatalog.TryResolve(
                    data.eventId, out PortfolioEventIdentity eventIdentity)
                || !string.Equals(eventIdentity.NodeId, data.nodeId,
                    System.StringComparison.Ordinal)
                || !string.Equals(eventIdentity.SourcePopupId,
                    data.sourcePopupId, System.StringComparison.Ordinal))
            {
                errors.Add("BATTLE_COMPLETION_CATALOG_IDENTITY_MISMATCH");
            }
        }

        public static bool IsTypeMatch(
            ChoiceExecutionType executionType,
            ChoiceExecutionData data)
        {
            return executionType switch
            {
                ChoiceExecutionType.NextEvent =>
                    data is NextEventExecutionData,
                ChoiceExecutionType.Battle =>
                    data is BattleExecutionData,
                ChoiceExecutionType.Shop =>
                    data is ShopExecutionData,
                ChoiceExecutionType.Shrine =>
                    data is ShrineExecutionData,
                ChoiceExecutionType.CompleteEvent =>
                    data is CompleteEventExecutionData,
                ChoiceExecutionType.RandomGrowthRisk =>
                    data is RandomGrowthRiskExecutionData,
                ChoiceExecutionType.RandomGrowthDecline =>
                    data is RandomGrowthDeclineExecutionData,
                ChoiceExecutionType.RandomGrowthSafe =>
                    data is RandomGrowthSafeExecutionData,
                ChoiceExecutionType.PortfolioOutcome =>
                    data is PortfolioOutcomeExecutionData,
                _ => data == null
            };
        }

        private static void ValidatePortfolioOutcome(
            PortfolioOutcomeExecutionData data, List<string> errors)
        {
            if (data.schemaVersion != 1
                || string.IsNullOrWhiteSpace(data.eventId)
                || string.IsNullOrWhiteSpace(data.nodeId)
                || string.IsNullOrWhiteSpace(data.sourcePopupId)
                || string.IsNullOrWhiteSpace(data.choiceId)
                || string.IsNullOrWhiteSpace(data.resultId)
                || string.IsNullOrWhiteSpace(data.reservationId)
                || data.nodeId != data.sourcePopupId)
            {
                errors.Add("PORTFOLIO_OUTCOME_IDENTITY_INVALID");
                return;
            }
            if (!PortfolioEventIdentityCatalog.TryResolve(data.eventId, out PortfolioEventIdentity identity)
                || !string.Equals(identity.NodeId, data.nodeId, System.StringComparison.Ordinal)
                || !string.Equals(identity.SourcePopupId, data.sourcePopupId,
                    System.StringComparison.Ordinal))
            {
                errors.Add("PORTFOLIO_OUTCOME_CATALOG_IDENTITY_MISMATCH");
                return;
            }
            if (data.operations == null || data.operations.Count == 0 || data.operations.Count > 4)
            {
                errors.Add("PORTFOLIO_OUTCOME_OPERATION_COUNT");
                return;
            }
            int battles = 0;
            foreach (PortfolioOutcomeOperationData operation in data.operations)
            {
                if (operation == null) { errors.Add("PORTFOLIO_OUTCOME_OPERATION_NULL"); continue; }
                switch (operation.kind)
                {
                    case PortfolioOutcomeOperationKind.VitalDelta:
                        if (operation.maxHpPercent is < -100 or > 100 || operation.maxHpPercent == 0)
                            errors.Add("PORTFOLIO_OUTCOME_VITAL_INVALID");
                        if (operation.allowEffectiveZero
                            && (!string.Equals(data.eventId,
                                    "event.act1.random_event.35.ownerless_wage_sack",
                                    System.StringComparison.Ordinal)
                                || !string.Equals(data.choiceId,
                                    "choice.act1.random_event.35.ownerless_wage_sack.return_all_wages",
                                    System.StringComparison.Ordinal)
                                || operation.maxHpPercent != 25))
                            errors.Add("PORTFOLIO_OUTCOME_ZERO_EFFECT_POLICY_INVALID");
                        break;
                    case PortfolioOutcomeOperationKind.InventoryGrant:
                        if (operation.count != 1 || (!operation.relic && !operation.relicPool))
                            errors.Add("PORTFOLIO_OUTCOME_INVENTORY_INVALID");
                        if (!ValidateEvent39Grant(data, operation))
                            errors.Add("EVENT39_ENTITLEMENT_GRANT_INVALID");
                        break;
                    case PortfolioOutcomeOperationKind.SetRunFlagTrue:
                        if (string.IsNullOrWhiteSpace(operation.targetId))
                            errors.Add("PORTFOLIO_OUTCOME_FLAG_INVALID");
                        break;
                    case PortfolioOutcomeOperationKind.RevealImmediateSuccessorPurpose:
                        break;
                    case PortfolioOutcomeOperationKind.BeginBattle:
                        battles++;
                        if (!operation.battle) errors.Add("PORTFOLIO_OUTCOME_BATTLE_INVALID");
                        break;
                    case PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute:
                        if (operation.selectionMode == ImmediateSuccessorRouteSelectionMode.None
                            || string.IsNullOrWhiteSpace(operation.snapshotId))
                            errors.Add("PORTFOLIO_OUTCOME_ROUTE_INVALID");
                        if (operation.selectionMode == ImmediateSuccessorRouteSelectionMode
                                .BattlePurposeThenShortestRemainingToSectionExit
                            && !string.Equals(data.eventId,
                                Progression.Portfolio.Chapter1Event45SelectionContract.Event45Id,
                                System.StringComparison.Ordinal))
                            errors.Add("EVENT45_ROUTE_MODE_SCOPE_INVALID");
                        break;
                    case PortfolioOutcomeOperationKind.RelicRouteTrade:
                        if (!ValidateEvent39Trade(data, operation))
                            errors.Add("EVENT39_RELIC_ROUTE_TRADE_INVALID");
                        break;
                    case PortfolioOutcomeOperationKind.GoldSpend:
                    case PortfolioOutcomeOperationKind.GoldGrant:
                        if (operation.amount <= 0)
                            errors.Add("PORTFOLIO_OUTCOME_GOLD_INVALID");
                        break;
                    default:
                        errors.Add("PORTFOLIO_OUTCOME_KIND_INVALID");
                        break;
                }
            }
            if (battles > 1) errors.Add("PORTFOLIO_OUTCOME_BATTLE_DUPLICATE");
            if (battles == 1 && !IsSupportedBattleContinuation(data))
                errors.Add("PORTFOLIO_OUTCOME_BATTLE_GRANT_ORDER");
            if (battles == 1 && IsEvent37(data) && !IsEvent37BattlePair(data))
                errors.Add("EVENT37_BATTLE_PAIR_INVALID");
            if (battles == 1 && IsEvent31(data) && !IsEvent31BattlePair(data))
                errors.Add("EVENT31_BATTLE_PAIR_INVALID");
            if (battles == 0 && data.operations.Count != 1
                && !IsEvent36AtomicPair(data) && !IsSupportedImmediateAtomicPair(data))
                errors.Add("PORTFOLIO_OUTCOME_IMMEDIATE_ATOMICITY");
        }

        private static bool IsSupportedBattleContinuation(PortfolioOutcomeExecutionData data)
        {
            if (data?.operations?.Count != 2
                || data.operations[0]?.kind != PortfolioOutcomeOperationKind.BeginBattle)
                return false;
            PortfolioOutcomeOperationKind continuation = data.operations[1].kind;
            if (continuation == PortfolioOutcomeOperationKind.InventoryGrant) return true;
            return continuation switch
            {
                PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute =>
                    data.eventId == "event.act1.random_event.28.rockfall_scouts",
                PortfolioOutcomeOperationKind.GoldGrant =>
                    data.eventId == "event.act1.random_event.33.false_mountain_rite_offering_box",
                _ => false
            };
        }

        private static bool IsSupportedImmediateAtomicPair(PortfolioOutcomeExecutionData data)
        {
            if (data?.operations?.Count != 2) return false;
            PortfolioOutcomeOperationKind first = data.operations[0].kind;
            PortfolioOutcomeOperationKind second = data.operations[1].kind;
            return data.eventId switch
            {
                "event.act1.random_event.28.rockfall_scouts" =>
                    first == PortfolioOutcomeOperationKind.VitalDelta
                    && second == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute,
                "event.act1.random_event.32.hidden_ledger_salt_cart" =>
                    first == PortfolioOutcomeOperationKind.GoldGrant
                    && second == PortfolioOutcomeOperationKind.SetRunFlagTrue,
                "event.act1.random_event.34.half_vein_map.followup.unstable_vein" =>
                    first == PortfolioOutcomeOperationKind.VitalDelta
                    && second == PortfolioOutcomeOperationKind.GoldGrant,
                "event.act1.random_event.35.ownerless_wage_sack" =>
                    (first == PortfolioOutcomeOperationKind.GoldGrant
                        || first == PortfolioOutcomeOperationKind.VitalDelta)
                    && second == PortfolioOutcomeOperationKind.SetRunFlagTrue,
                "event.act1.random_event.42.twice_ringing_mountain_echo" =>
                    first == PortfolioOutcomeOperationKind.SetRunFlagTrue
                    && second == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute,
                "event.act1.random_event.44.buried_tax_stele" =>
                    first == PortfolioOutcomeOperationKind.GoldGrant
                    && second == PortfolioOutcomeOperationKind.SetRunFlagTrue,
                "event.act1.random_event.45.false_wildfire_boundary_stones" =>
                    first == PortfolioOutcomeOperationKind.SetRunFlagTrue
                    && data.operations[0].targetId ==
                        Progression.Portfolio.Chapter1Event45SelectionContract.PositiveFlagId
                    && second == PortfolioOutcomeOperationKind.CommitImmediateSuccessorRoute
                    && data.operations[1].selectionMode ==
                        ImmediateSuccessorRouteSelectionMode.BattlePurposeThenShortestRemainingToSectionExit,
                _ => false
            };
        }

        private static bool ValidateEvent39Grant(PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOperationData operation)
        {
            if (!string.Equals(data.eventId,
                    Progression.Portfolio.Chapter1Event39SelectionContract.Event39Id,
                    System.StringComparison.Ordinal))
                return string.IsNullOrWhiteSpace(operation.sourceEntitlementId);
            return operation.count == 1 && operation.unique && operation.relicPool == null
                && operation.relic?.relicId ==
                    Progression.Portfolio.Chapter1Event39SelectionContract.RelicId
                && operation.sourceEntitlementId ==
                    Progression.Portfolio.Chapter1Event39SelectionContract.EntitlementId;
        }

        private static bool ValidateEvent39Trade(PortfolioOutcomeExecutionData data,
            PortfolioOutcomeOperationData operation) =>
            string.Equals(data.eventId,
                Progression.Portfolio.Chapter1Event39SelectionContract.Event39Id,
                System.StringComparison.Ordinal)
            && operation.sourceEntitlementId ==
                Progression.Portfolio.Chapter1Event39SelectionContract.EntitlementId
            && operation.selectionMode ==
                ImmediateSuccessorRouteSelectionMode.ShortestRemainingToSectionExit
            && !string.IsNullOrWhiteSpace(operation.snapshotId)
            && operation.relic?.relicId ==
                Progression.Portfolio.Chapter1Event39SelectionContract.RelicId
            && operation.relicPool == null;

        private static bool IsEvent37(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId,
                "event.act1.random_event.37.nameless_long_sword_in_rain",
                System.StringComparison.Ordinal);

        private static bool IsEvent31(PortfolioOutcomeExecutionData data) =>
            string.Equals(data?.eventId,
                Progression.Portfolio.Chapter1Event31SelectionContract.Event31Id,
                System.StringComparison.Ordinal);

        private static bool IsEvent31BattlePair(PortfolioOutcomeExecutionData data)
        {
            if (!IsEvent31(data) || data.operations?.Count != 2) return false;
            PortfolioOutcomeOperationData battle = data.operations[0];
            PortfolioOutcomeOperationData grant = data.operations[1];
            return battle != null
                && battle.kind == PortfolioOutcomeOperationKind.BeginBattle
                && string.Equals(battle.battle?.battleId,
                    Progression.Portfolio.Chapter1Event31SelectionContract.BattleId,
                    System.StringComparison.Ordinal)
                && grant != null
                && grant.kind == PortfolioOutcomeOperationKind.InventoryGrant
                && grant.count == 1
                && grant.unique
                && grant.relicPool == null
                && string.Equals(grant.relic?.relicId,
                    Progression.Portfolio.Chapter1Event31SelectionContract.RelicId,
                    System.StringComparison.Ordinal);
        }

        private static bool IsEvent37BattlePair(PortfolioOutcomeExecutionData data)
        {
            if (!IsEvent37(data) || data.operations?.Count != 2) return false;
            PortfolioOutcomeOperationData battle = data.operations[0];
            PortfolioOutcomeOperationData grant = data.operations[1];
            return battle != null
                && battle.kind == PortfolioOutcomeOperationKind.BeginBattle
                && string.Equals(battle.battle?.battleId,
                    "battle.act1.event01.named_well_wraith",
                    System.StringComparison.Ordinal)
                && grant != null
                && grant.kind == PortfolioOutcomeOperationKind.InventoryGrant
                && grant.count == 1
                && grant.unique
                && grant.relicPool == null
                && string.Equals(grant.relic?.relicId, "item.relic.ember_necklace",
                    System.StringComparison.Ordinal);
        }

        private static bool IsEvent36AtomicPair(PortfolioOutcomeExecutionData data)
        {
            if (!string.Equals(data.eventId,
                    "event.act1.random_event.36.cracked_bronze_mirror",
                    System.StringComparison.Ordinal)
                || data.operations?.Count != 2)
                return false;
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
                    System.StringComparison.Ordinal);
        }

        private static void ValidateRandomGrowthRisk(
            RandomGrowthRiskExecutionData data,
            List<string> errors)
        {
            ValidateRandomGrowthCommon(data, errors);
            RandomGrowthInteractionReservationData reservation = data.interactionReservation;
            RandomGrowthCostProjectionData cost = data.costPolicy;
            RandomGrowthCapProjectionData cap = data.capPolicy;

            if (data.resultKind != "RiskSelected"
                || data.successResultKind != "RiskGranted"
                || data.failureState != "RiskSelectedPendingRetry")
            {
                errors.Add("RANDOM_GROWTH_RISK_STATE: Reservation and terminal states are invalid.");
            }

            if (reservation == null
                || reservation.authority != "StageSession"
                || reservation.lifetime != "SessionOnly"
                || reservation.stableKeyFields == null
                || !reservation.stableKeyFields.SequenceEqual(new[]
                {
                    "runId", "stageGenerationId", "reservationId", "encounteredNodeInstanceId"
                })
                || reservation.orderedStates == null
                || !reservation.orderedStates.SequenceEqual(new[]
                {
                    "RiskSelected", "Applying", "RiskSelectedPendingRetry", "RiskGranted"
                })
                || !reservation.locksDecline
                || !reservation.blocksDuplicateConfirm
                || reservation.mutationCountBeforeAtomicTransaction != 0)
            {
                errors.Add("RANDOM_GROWTH_RESERVATION_CONTRACT: StageSession reservation is invalid.");
            }

            if (cost == null
                || cost.type != "MaxHpPercentNonlethal"
                || cost.rateBasisPoints != 1000
                || cost.rounding != "Ceil"
                || System.BitConverter.SingleToInt32Bits(cost.minimumRemainingHp)
                    != System.BitConverter.SingleToInt32Bits(1f))
            {
                errors.Add("RANDOM_GROWTH_COST_CONTRACT: Risk cost projection is invalid.");
            }

            if (cap == null || cap.fixedApplied != 2 || cap.randomApplied != 1
                || cap.totalApplied != 3 || data.growthGrant != 1)
            {
                errors.Add("RANDOM_GROWTH_CAP_CONTRACT: Risk cap or grant is invalid.");
            }
        }

        private static void ValidateRandomGrowthSafe(
            RandomGrowthSafeExecutionData data,
            List<string> errors)
        {
            ValidateRandomGrowthCommon(data, errors);
            if (data.resultKind != "ObserveSelected"
                || data.successResultKind != "SafeGrowthGranted"
                || data.failureState != "ObserveSelectedPendingRetry"
                || data.candidateUnavailableState != "CandidateUnavailable"
                || data.cost != 0
                || data.growthGrant != 1
                || data.targetCount != 2
                || !data.candidateZeroBlocksExecutor
                || !data.candidateZeroAllowsFallback
                || data.mutationCountBeforeConfirm != 0)
            {
                errors.Add("RANDOM_GROWTH_SAFE_CONTRACT: Safe growth execution is invalid.");
            }

            RandomGrowthCapProjectionData cap = data.capPolicy;
            if (cap == null || cap.fixedApplied != 2 || cap.optionalGranted != 1
                || cap.optionalApplied != 1 || cap.totalApplied != 3)
            {
                errors.Add("RANDOM_GROWTH_SAFE_CAP_CONTRACT: Safe cap is invalid.");
            }
        }

        private static void ValidateRandomGrowthCommon(
            RandomGrowthChoiceExecutionData data,
            List<string> errors)
        {
            bool portfolioB1 = data.eventId == "event.act1.random_event.21.breath_between_water_drops"
                || data.eventId == "event.act1.random_event.22.sleeping_hawk_watch"
                || data.eventId == "event.act1.random_event.23.temple_hundred_eight_steps";
            bool legacy = data.schemaVersion == 1;
            bool catalogV2 = data.schemaVersion == 2;
            bool catalogIdentityPresent = !string.IsNullOrWhiteSpace(data.presentationCatalogId)
                || !string.IsNullOrWhiteSpace(data.presentationProjectionKind)
                || !string.IsNullOrWhiteSpace(data.presentationLocale);
            if ((!legacy && !catalogV2)
                || string.IsNullOrWhiteSpace(data.contentContractVersion)
                || !IsSha256(data.definitionFingerprint)
                || !IsSha256(data.presentationTextDigestKo)
                || string.IsNullOrWhiteSpace(data.eventId)
                || string.IsNullOrWhiteSpace(data.stageNodeId)
                || string.IsNullOrWhiteSpace(data.sourcePopupId)
                || string.IsNullOrWhiteSpace(data.choiceId)
                || string.IsNullOrWhiteSpace(data.segmentId)
                || string.IsNullOrWhiteSpace(data.reservationId)
                || data.poolMode != "PartyWide"
                || (legacy && catalogIdentityPresent)
                || (catalogV2 && (string.IsNullOrWhiteSpace(data.presentationCatalogId)
                    || string.IsNullOrWhiteSpace(data.presentationProjectionKind)
                    || string.IsNullOrWhiteSpace(data.presentationLocale))))
            {
                errors.Add("RANDOM_GROWTH_COMMON_CONTRACT: Typed identity or fingerprint is invalid.");
            }
            if (portfolioB1)
            {
                RandomGrowthPayloadKind kind = data is RandomGrowthSafeExecutionData
                    ? RandomGrowthPayloadKind.Safe : data is RandomGrowthRiskExecutionData
                        ? RandomGrowthPayloadKind.Risk : RandomGrowthPayloadKind.Decline;
                if (!RandomGrowthEventIdentityCatalog.TryResolve(data.eventId, data.choiceId, kind,
                        out RandomGrowthEventIdentity identity)
                    || data.sourcePopupId != identity.NodeId
                    || data.reservationId != identity.ReservationId
                    || data.segmentId != identity.SegmentId)
                    errors.Add("PORTFOLIO_RANDOM_GROWTH_IDENTITY_MISMATCH");
            }
        }

        private static bool IsSha256(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 64)
            {
                return false;
            }

            foreach (char c in value)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
                {
                    return false;
                }
            }

            return true;
        }

        private static void ValidateShop(
            ShopExecutionData data,
            List<string> errors)
        {
            if (data.pools == null || data.pools.Count == 0)
            {
                errors.Add(
                    "SHOP_POOLS_EMPTY: At least one ShopItemPoolSO is required.");
            }
            else
            {
                for (int i = 0; i < data.pools.Count; i++)
                {
                    ShopItemPoolSO pool = data.pools[i];

                    if (pool == null)
                    {
                        errors.Add(
                            $"SHOP_POOL_NULL: pools[{i}] is null.");
                        continue;
                    }

                    if (!HasValidProduct(pool))
                    {
                        errors.Add(
                            $"SHOP_POOL_EMPTY: pools[{i}] has no valid product.");
                    }
                }
            }

            if (data.itemCount <= 0)
            {
                errors.Add(
                    "SHOP_ITEM_COUNT: itemCount must be greater than zero.");
            }
        }

        private static bool HasValidProduct(ShopItemPoolSO pool)
        {
            if (pool.products == null)
            {
                return false;
            }

            foreach (ShopProductSO product in pool.products)
            {
                if (product != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateShrine(
            ShrineExecutionData data,
            List<string> errors)
        {
            if (data.config == null)
            {
                errors.Add(
                    "SHRINE_CONFIG_NULL: ShrineConfigSO reference is required.");
            }

            if (data.god == null)
            {
                errors.Add(
                    "SHRINE_GOD_NULL: ShrineGodSO reference is required.");
                return;
            }

            if (data.god.GodType == ShrineGodType.None)
            {
                errors.Add(
                    "SHRINE_GOD_NONE: ShrineGodType must not be None.");
            }

            if (data.config != null && !ContainsGod(data.config, data.god))
            {
                errors.Add(
                    "SHRINE_GOD_NOT_REGISTERED: "
                    + "ShrineConfigSO must contain the selected ShrineGodSO.");
            }
        }

        private static bool ContainsGod(
            ShrineConfigSO config,
            ShrineGodSO targetGod)
        {
            if (config.Gods == null)
            {
                return false;
            }

            foreach (ShrineGodSO god in config.Gods)
            {
                if (god == targetGod)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
