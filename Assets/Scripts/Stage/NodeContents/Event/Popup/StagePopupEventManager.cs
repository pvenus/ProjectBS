using UnityEngine;
using Item;
using Stat;
using Session;
using UIFramework.Data;
using System.Collections.Generic;
using System.Linq;
using Skill;
using Progression;
using Progression.RandomGrowth;

namespace Stage
{
    /// <summary>
    /// PopupEvent 노드를 처리하는 매니저.
    /// 이벤트 열기 → 선택지 처리 → 노드 완료까지 담당
    /// </summary>
    public class StagePopupEventManager : MonoBehaviour
    {
        public static StagePopupEventManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private StageManager stageManager;
        [SerializeField] private StagePlayerPopupCoordinator playerPopupCoordinator;

        [Header("Runtime")]
        private EventRewardExecutor rewardExecutor;
        private StageEventResolver eventResolver;
        private ChoiceExecutionRouter executionRouter;
        private readonly PortfolioOutcomeRuntimeService portfolioOutcomeService = new();
        private readonly OrdinaryBattleCompletionService ordinaryBattleService = new();
        private IChoiceRewardPresentation rewardPresentation;
        private PopupEventSO currentEvent;
        private RoundNode currentNode;
        private PopupEventChoice pendingChoice;
        private string pendingExecutionId;
        private int selectionSequence;
        private readonly ChoiceContinuationGate continuationGate =
            new();
        private SafeGrowthPopupRuntimeAdapter safeGrowthAdapter;
        private readonly SafeGrowthEventPopupPresentationBinder safeGrowthPresentationBinder = new();
        private EventPopupView safeGrowthView;
        private SafeGrowthPartyWideOfferPresenter safeGrowthOfferPresenter;
        private PortfolioRandomGrowthPopupRuntimeAdapter portfolioRandomGrowthAdapter;
        private string pendingShopCompletionKey;

        public PopupEventSO CurrentEvent => currentEvent;
        public bool IsOpened => currentEvent != null;

        public event System.Action<PopupEventSO, RoundNode> OnPopupEventOpened;
        public event System.Action<PopupEventSO, PopupEventChoice, RoundNode> OnPopupEventChoiceSelected;
        public event System.Action<PopupEventSO, RoundNode> OnPopupEventClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (stageManager == null)
            {
                stageManager = StageManager.Instance;
            }
            if (playerPopupCoordinator == null)
            {
                playerPopupCoordinator = FindObjectOfType<StagePlayerPopupCoordinator>();
            }
            rewardExecutor = new EventRewardExecutor(
                ItemManager.Instance,
                StatManager.Instance);

            eventResolver = new StageEventResolver();
            executionRouter =
                ChoiceExecutionRouter.CreateDefault();
            rewardPresentation =
                new ImmediateChoiceRewardPresentation();
        }

        private void OnEnable()
        {
            if (stageManager == null)
            {
                stageManager = StageManager.Instance;
            }
            if (playerPopupCoordinator == null)
            {
                playerPopupCoordinator = FindObjectOfType<StagePlayerPopupCoordinator>();
            }
            if (playerPopupCoordinator != null)
            {
                playerPopupCoordinator.ShopClosed -= HandleShopClosed;
                playerPopupCoordinator.ShopClosed += HandleShopClosed;
            }

            stageManager.OnNodeSelected += HandleNodeSelected;
            stageManager.OnStageGenerated += HandleStageGenerated;
        }

        private void OnDisable()
        {
            if (playerPopupCoordinator != null)
            {
                playerPopupCoordinator.ShopClosed -= HandleShopClosed;
            }
            if (stageManager == null)
            {
                return;
            }

            stageManager.OnNodeSelected -= HandleNodeSelected;
            stageManager.OnStageGenerated -= HandleStageGenerated;
        }

        private void HandleStageGenerated(StageGraph graph)
        {
            currentEvent = null;
            currentNode = null;
            selectionSequence = 0;
            ResetPendingChoiceState();
            safeGrowthOfferPresenter = null;
            executionRouter?.ClearHistory();
        }

        private void HandleNodeSelected(RoundNode node)
        {
            TryOpen(node);
        }

        public bool TryOpen(RoundNode node)
        {
            if (node == null)
            {
                return false;
            }

            if (node.nodeType != RoundNodeType.Event &&
                node.nodeType != RoundNodeType.Battle &&
                node.nodeType != RoundNodeType.Boss &&
                node.nodeType != RoundNodeType.RequiredSubEvent &&
                node.nodeType != RoundNodeType.Shop &&
                node.nodeType != RoundNodeType.Rest)
            {
                return false;
            }

            if (eventResolver != null)
            {
                eventResolver.Resolve(node);
            }

            PopupEventSO popupEvent = node.popupEvent;

            if (popupEvent == null)
            {
                Debug.LogWarning($"PopupEvent missing on node. nodeId={node.nodeId}");
                return false;
            }

            Open(popupEvent, node);
            return true;
        }

        public void Open(PopupEventSO popupEvent, RoundNode node)
        {
            ResetPendingChoiceState();
            currentEvent = popupEvent;
            currentNode = node;
            EnsureSafeGrowthAdapterAndRoute(popupEvent, node);

            SafeGrowthPendingConfirmContext restored = safeGrowthAdapter?.Pending;
            if (restored != null
                && string.Equals(restored.PopupId, popupEvent?.eventId, System.StringComparison.Ordinal)
                && string.Equals(restored.NodeInstanceId, node?.nodeId, System.StringComparison.Ordinal))
            {
                pendingChoice = popupEvent.GetChoice(restored.ChoiceId);
                pendingExecutionId = restored.InteractionTokenId;
            }

            // UIPopupViewController 로 EventPopupView 열기
            if (UIPopupViewController.Instance != null)
            {
                EventPopupView view = UIPopupViewController.Instance.Open<EventPopupView>(PopupType.EventPopup);
                if (view != null)
                {
                    safeGrowthView = view;
                    if (!TryRenderSafeGrowthPresentation())
                    {
                        EventPopupViewData viewData = EventPopupViewDataBuilder.Build(popupEvent, node);
                        view.SetData(viewData);
                        ApplyPortfolioChoicePrevalidation(view, popupEvent);
                        view.OnChoiceSelected += SelectChoiceById;
                        view.OnChoiceConfirmed += ConfirmChoiceResult;
                    }
                }
            }

            OnPopupEventOpened?.Invoke(currentEvent, currentNode);
        }

        public void SelectChoiceByIndex(int index)
        {
            if (currentEvent == null)
            {
                return;
            }

            if (currentEvent.choices == null || index < 0 || index >= currentEvent.choices.Count)
            {
                return;
            }

            PopupEventChoice choice = currentEvent.choices[index];
            SelectChoiceById(choice?.choiceId);
        }

        public void SelectChoiceById(string choiceId)
        {
            if (currentEvent == null
                || string.IsNullOrWhiteSpace(choiceId))
            {
                return;
            }

            PopupEventChoice choice =
                currentEvent.GetChoice(choiceId);

            if (choice == null)
            {
                Debug.LogWarning(
                    "[StagePopupEventManager] Choice not found. "
                    + $"eventId={currentEvent.eventId}, "
                    + $"choiceId={choiceId}");
                return;
            }

            SelectChoice(choice);
        }

        public void SelectChoice(PopupEventChoice choice)
        {
            if (currentEvent == null || choice == null)
            {
                return;
            }

            if (pendingChoice != null)
            {
                Debug.LogWarning(
                    "[StagePopupEventManager] A choice is already pending. "
                    + $"pendingChoiceId={pendingChoice.choiceId}");
                return;
            }

            if (currentEvent.GetChoice(choice.choiceId) != choice)
            {
                Debug.LogWarning(
                    "[StagePopupEventManager] Choice does not belong "
                    + "to the current event.");
                return;
            }

            if (choice.executionConfig?.data is PortfolioOutcomeExecutionData portfolio
                && !portfolioOutcomeService.CanExecuteChoice(portfolio,
                    out string disabledCopy, out string prevalidationError))
            {
                Debug.LogWarning("[StagePopupEventManager] Choice disabled before dispatch. "
                    + $"choiceId={choice.choiceId}, error={prevalidationError}, "
                    + $"copy={disabledCopy}");
                return;
            }

            PortfolioRandomGrowthDispatchResult portfolioGrowth =
                portfolioRandomGrowthAdapter?.Select(currentEvent, currentNode, choice);
            if (portfolioGrowth != null
                && portfolioGrowth.Status != PortfolioRandomGrowthDispatchStatus.Unsupported)
            {
                if (portfolioGrowth.Status == PortfolioRandomGrowthDispatchStatus.RequiresConfirmation
                    || portfolioGrowth.Status == PortfolioRandomGrowthDispatchStatus.PendingRetry)
                {
                    pendingChoice = choice;
                    pendingExecutionId = GameSession.Instance?.StageSession?.PortfolioRandomGrowth?.Pending?.TokenId;
                    OnPopupEventChoiceSelected?.Invoke(currentEvent, choice, currentNode);
                }
                else if (portfolioGrowth.Status == PortfolioRandomGrowthDispatchStatus.Failed)
                {
                    Debug.LogError(
                        "[StagePopupEventManager] Portfolio random growth selection failed. "
                        + $"eventId={currentEvent?.eventId}, choiceId={choice.choiceId}, "
                        + $"nodeId={currentNode?.nodeId}, error={portfolioGrowth.Error}");
                }
                return;
            }

            SafeGrowthPopupAdapterResult safe = SelectSafeChoice(choice);
            if (safe != null && safe.Status != SafeGrowthPopupAdapterStatus.Unsupported)
            {
                if (safe.Status == SafeGrowthPopupAdapterStatus.RequiresConfirmation
                    || safe.Status == SafeGrowthPopupAdapterStatus.PendingRetry)
                {
                    pendingChoice = choice;
                    pendingExecutionId = safe.Pending?.InteractionTokenId;
                    OnPopupEventChoiceSelected?.Invoke(currentEvent, choice, currentNode);
                }
                else if (safe.Status == SafeGrowthPopupAdapterStatus.TerminalReplay)
                {
                    TryRenderSafeGrowthPresentation();
                }
                return;
            }

            pendingChoice = choice;
            pendingExecutionId =
                CreateExecutionId(currentEvent, choice);
            continuationGate.Begin(pendingExecutionId);

            // Choice를 선택한 순간 보상을 먼저 지급한다.
            List<PopupEventRewardData> immediateRewards =
                GetImmediateRewards(choice);

            if (immediateRewards.Count > 0)
            {
                ExecuteImmediateRewards(
                    choice,
                    immediateRewards);
            }

            OnPopupEventChoiceSelected?.Invoke(currentEvent, choice, currentNode);

            string selectionId = pendingExecutionId;
            IReadOnlyList<PopupEventRewardData> rewards =
                immediateRewards;
            IChoiceRewardPresentation presenter =
                rewardPresentation
                ?? new ImmediateChoiceRewardPresentation();

            presenter.Present(
                rewards,
                () => HandleRewardPresentationCompleted(selectionId));
        }

        public void ConfirmChoiceResult()
        {
            if (pendingChoice == null)
            {
                return;
            }

            if (safeGrowthAdapter?.Pending != null)
            {
                ConfirmSafePending();
                return;
            }

            if (GameSession.Instance?.StageSession?.PortfolioRandomGrowth?.Pending != null)
            {
                PortfolioRandomGrowthDispatchResult result =
                    portfolioRandomGrowthAdapter?.Confirm(currentNode,
                    node => stageManager?.PublishAtomicCompletion(node, stageManager.ProgressState),
                    _ => { });
                if (result?.Status == PortfolioRandomGrowthDispatchStatus.Succeeded
                    || result?.Status == PortfolioRandomGrowthDispatchStatus.Declined)
                {
                    CloseAfterSafeTerminal();
                }
                else if (result != null)
                {
                    Debug.LogError(
                        "[StagePopupEventManager] Portfolio random growth confirmation failed. "
                        + $"eventId={currentEvent?.eventId}, "
                        + $"nodeId={currentNode?.nodeId}, error={result.Error}");
                }
                return;
            }

            if (continuationGate.RequestConfirmation())
            {
                ContinuePendingChoice();
            }
        }

        public void Complete()
        {
            PopupEventSO closedEvent = currentEvent;
            RoundNode node = currentNode;

            currentEvent = null;
            currentNode = null;
            ResetPendingChoiceState();

            UIPopupViewController.Instance?.Close(PopupType.EventPopup);

            if (stageManager != null && stageManager.CurrentNode == node)
            {
                stageManager.CompleteCurrentNode();
            }

            OnPopupEventClosed?.Invoke(closedEvent, node);
        }

        public void CloseWithoutComplete()
        {
            PopupEventSO closedEvent = currentEvent;
            RoundNode node = currentNode;

            currentEvent = null;
            currentNode = null;
            ResetPendingChoiceState();

            UIPopupViewController.Instance?.Close(PopupType.EventPopup);

            OnPopupEventClosed?.Invoke(closedEvent, node);
        }

        public void SetRewardPresentation(
            IChoiceRewardPresentation presentation)
        {
            rewardPresentation =
                presentation
                ?? new ImmediateChoiceRewardPresentation();
        }

        public SafeGrowthPendingConfirmContext GetSafePendingSnapshot() =>
            safeGrowthAdapter?.Pending ?? GameSession.Instance?.StageSession?.SafeGrowthPendingConfirm;

        public SafeGrowthPopupAdapterResult SelectSafeChoice(PopupEventChoice choice)
        {
            EnsureSafeGrowthAdapterAndRoute(currentEvent, currentNode);
            return safeGrowthAdapter?.Select(currentEvent, currentNode, choice);
        }

        public SafeGrowthPopupAdapterResult CancelSafePending()
        {
            SafeGrowthPopupAdapterResult result = safeGrowthAdapter?.Cancel();
            if (result?.Status == SafeGrowthPopupAdapterStatus.Cancelled)
                ResetPendingChoiceState();
            return result;
        }

        public SafeGrowthPopupAdapterResult RecheckSafeEligibility()
        {
            EnsureSafeGrowthAdapterAndRoute(currentEvent, currentNode);
            return safeGrowthAdapter?.Recheck(currentEvent, currentNode, pendingChoice);
        }

        public SafeGrowthPopupAdapterResult ConfirmSafePending()
            => ConfirmSafePending(GetSafePendingSnapshot());

        public SafeGrowthPopupAdapterResult ConfirmSafePending(
            SafeGrowthPendingConfirmContext expected)
        {
            if (safeGrowthAdapter == null || pendingChoice == null)
                return null;
            SafeGrowthPopupAdapterResult result = safeGrowthAdapter.Confirm(
                currentEvent, currentNode, pendingChoice, expected,
                node => stageManager?.PublishAtomicCompletion(node,
                    stageManager.ProgressState),
                _ => { });
            return result;
        }

        public bool TryRenderSafeGrowthEvidenceProjection(
            SafeGrowthPlayerEvidenceCase evidenceCase, string token, string planSha,
            out SafeGrowthPresentationSnapshot snapshot, out string payloadSha)
        {
            snapshot = null;
            payloadSha = string.Empty;
            if (safeGrowthView == null || safeGrowthAdapter == null)
                return false;
            RandomGrowthPresentationCopyAsset catalog = Resources.Load<RandomGrowthPresentationCopyAsset>(
                SafeGrowthPlayerEvidencePlan.PresentationCatalogResource);
            if (!SafeGrowthPlayerEvidenceOrchestrator.TryValidateIdentity(currentEvent, catalog,
                    out _, out _, out _))
                return false;
            // Require the production binder to accept the actual Popup/SO/catalog/adapter first.
            if (!safeGrowthPresentationBinder.TryBuild(currentEvent, catalog, safeGrowthAdapter,
                    out SafeGrowthPresentationSnapshot production, out _)
                || production == null)
                return false;
            var orchestrator = new SafeGrowthPlayerEvidenceOrchestrator();
            if (!orchestrator.TryProject(currentEvent, catalog, evidenceCase, token, planSha,
                    out snapshot, out payloadSha))
                return false;
            // StagePopupEventManager remains the sole owner of the binder-to-View render seam.
            safeGrowthView.SetSafeGrowthPresentation(snapshot, _ => { });
            return true;
        }

        private void HandleRewardPresentationCompleted(
            string selectionId)
        {
            if (pendingChoice == null
                || !string.Equals(
                    pendingExecutionId,
                    selectionId,
                    System.StringComparison.Ordinal))
            {
                return;
            }

            if (continuationGate.CompleteRewardPresentation(
                    selectionId))
            {
                ContinuePendingChoice();
            }
        }

        private void ContinuePendingChoice()
        {
            if (pendingChoice == null)
            {
                return;
            }

            PopupEventChoice confirmedChoice = pendingChoice;
            string executionId = pendingExecutionId;
            RoundNode node = currentNode;

            ResetPendingChoiceState();
            ExecuteChoice(confirmedChoice, executionId, node);
        }

        private void ExecuteChoice(
            PopupEventChoice choice,
            string executionId,
            RoundNode node)
        {
            if (choice.executionConfig != null)
            {
                OrdinaryBattleCompletionIdentity preparedBattle = null;
                if (choice.executionConfig.data is BattleExecutionData battleData
                    && !string.IsNullOrWhiteSpace(battleData.eventId))
                {
                    if (!ordinaryBattleService.TryPrepare(
                            battleData, GameSession.Instance?.StageSession,
                            node?.nodeId,
                            out preparedBattle, out string prepareError))
                    {
                        Debug.LogError(
                            "[StagePopupEventManager] Ordinary Battle prepare failed. "
                            + $"choiceId={choice.choiceId}, error={prepareError}");
                        return;
                    }
                }
                executionRouter ??=
                    ChoiceExecutionRouter.CreateDefault();
                ChoiceExecutionContext context =
                    new(
                        openNextEvent:
                            nextEvent => Open(nextEvent, node),
                        completeEvent:
                            CompleteExecution,
                        beginBattle:
                            LogBattleExecution,
                        openShop:
                            OpenStageShop,
                        openShrine:
                            LogShrineExecution,
                        applyPortfolioOutcome:
                            ApplyPortfolioOutcome,
                        openNextEventTransaction:
                            OpenNextEventTransaction);
                ChoiceExecutionResult result =
                    executionRouter.TryExecute(
                        executionId,
                        choice.executionConfig,
                        context,
                        out string error);

                if (result == ChoiceExecutionResult.Success
                    || result
                        == ChoiceExecutionResult.AlreadyExecuted)
                {
                    return;
                }

                if (preparedBattle != null)
                {
                    ordinaryBattleService.Abort(
                        GameSession.Instance?.StageSession, preparedBattle);
                }

                Debug.LogError(
                    "[StagePopupEventManager] Choice execution failed. "
                    + $"choiceId={choice.choiceId}, result={result}, "
                    + $"error={error}");
                return;
            }

            Debug.LogError(
                "[StagePopupEventManager] Choice executionConfig is missing. "
                + $"choiceId={choice.choiceId}");
        }

        private bool ApplyPortfolioOutcome(
            PortfolioOutcomeExecutionData data,
            out string error)
        {
            StageSession session = GameSession.Instance?.StageSession;
            bool sharesGrowthTerminal = data?.eventId
                == "event.act1.random_event.23.temple_hundred_eight_steps";
            if (sharesGrowthTerminal && session?.PortfolioRandomGrowth?.IsTerminal(
                    data.eventId, currentNode?.nodeId) == true)
            {
                error = "PORTFOLIO_RANDOM_GROWTH_TERMINAL_CONFLICT";
                return false;
            }
            bool executed = portfolioOutcomeService.TryExecute(
                data,
                GameSession.Instance,
                currentNode,
                currentEvent,
                CompleteExecution,
                LogBattleExecution,
                out error);
            if (executed && sharesGrowthTerminal
                && session?.PortfolioRandomGrowth?.TryCommitExternal(
                    data.eventId, currentNode?.nodeId) != true)
            {
                error = "PORTFOLIO_RANDOM_GROWTH_TERMINAL_COMMIT_FAILED";
                return false;
            }
            return executed;
        }

        private string CreateExecutionId(
            PopupEventSO popupEvent,
            PopupEventChoice choice)
        {
            selectionSequence++;
            return $"{popupEvent.eventId}/{choice.choiceId}/"
                   + selectionSequence;
        }

        private static List<PopupEventRewardData> GetImmediateRewards(
            PopupEventChoice choice)
        {
            List<PopupEventRewardData> result = new();

            if (choice?.rewards == null)
            {
                return result;
            }

            foreach (PopupEventRewardData reward in choice.rewards)
            {
                if (reward == null)
                {
                    continue;
                }

                result.Add(reward);
            }

            return result;
        }

        private void ExecuteImmediateRewards(
            PopupEventChoice choice,
            List<PopupEventRewardData> rewards)
        {
            if (rewardExecutor == null)
            {
                Debug.LogWarning(
                    "[ChoiceReward] Reward executor is unavailable. "
                    + $"choiceId={choice.choiceId}, "
                    + $"rewardCount={rewards.Count}");
                return;
            }

            rewardExecutor.Execute(rewards);

            foreach (PopupEventRewardData reward in rewards)
            {
                Debug.Log(
                    "[ChoiceReward] Reward dispatch completed. "
                    + $"choiceId={choice.choiceId}, "
                    + $"rewardType={reward.rewardType}, "
                    + $"rewardId={reward.rewardId}, "
                    + $"targetId={reward.targetId}, "
                    + $"value={reward.value}");
            }
        }

        private bool CompleteExecution()
        {
            if (currentEvent == null)
            {
                return false;
            }

            PortfolioOutcomeOwnership ownership =
                GameSession.Instance?.StageSession?.PortfolioOutcomes;
            PortfolioNextEventContinuationReceipt continuation = ownership?.PendingContinuation;
            if (continuation != null
                && string.Equals(continuation.childEventId, currentEvent.eventId,
                    System.StringComparison.Ordinal)
                && !ownership.TryCommitContinuation(currentEvent.eventId,
                    currentNode?.nodeId))
                return false;

            Complete();
            return true;
        }

        private bool OpenNextEventTransaction(NextEventExecutionData data, out string error)
        {
            error = string.Empty;
            if (data?.nextEvent == null || currentNode == null)
            {
                error = "NEXT_EVENT_TRANSACTION_CONTEXT_INVALID";
                return false;
            }
            var receipt = new PortfolioNextEventContinuationReceipt
            {
                parentEventId = data.parentEventId,
                parentNodeId = data.parentNodeId,
                parentChoiceId = data.parentChoiceId,
                parentResultId = data.parentResultId,
                parentReservationId = data.parentReservationId,
                childEventId = data.childEventId,
                childNodeId = data.childNodeId,
                childReservationId = data.childReservationId
            };
            PortfolioOutcomeOwnership ownership =
                GameSession.Instance?.StageSession?.PortfolioOutcomes;
            if (ownership?.TryReserveContinuation(receipt) != true)
            {
                error = "NEXT_EVENT_TRANSACTION_RESERVATION_CONFLICT";
                return false;
            }
            Open(data.nextEvent, currentNode);
            receipt.childOpened = true;
            return true;
        }

        private bool LogBattleExecution(
            Battle.BattleSO battle)
        {
            if (battle == null)
            {
                Debug.LogWarning(
                    "[ChoiceExecution][Battle] BattleSO is null.");
                return false;
            }

            if (GameSession.Instance == null)
            {
                Debug.LogError(
                    "[ChoiceExecution][Battle] GameSession.Instance is null.");
                return false;
            }

            string nodeId = currentNode != null ? currentNode.nodeId : string.Empty;
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                Debug.LogError(
                    "[ChoiceExecution][Battle] currentNode.nodeId is empty.");
                return false;
            }

            // 전투 진입 시 노드를 완료하지 않고 팝업만 종료
            PopupEventSO closedEvent = currentEvent;
            RoundNode node = currentNode;
            currentEvent = null;
            currentNode = null;
            pendingChoice = null;
            pendingExecutionId = null;

            UIPopupViewController.Instance?.Close(PopupType.EventPopup);
            OnPopupEventClosed?.Invoke(closedEvent, node);

            bool success = GameSession.Instance.TryBeginStageBattle(
                battle,
                nodeId);

            if (!success)
            {
                Debug.LogError(
                    $"[ChoiceExecution][Battle] TryBeginStageBattle failed. battle={battle.name}, nodeId={nodeId}");
                return false;
            }

            return true;
        }

        private bool OpenStageShop(
            ShopExecutionData data)
        {
            if (data == null)
            {
                Debug.LogWarning(
                    "[ChoiceExecution][Shop] "
                    + "Shop execution data is null.");
                return false;
            }
            playerPopupCoordinator ??= FindObjectOfType<StagePlayerPopupCoordinator>();
            if (playerPopupCoordinator == null)
            {
                Debug.LogWarning("[ChoiceExecution][Shop] Popup coordinator is missing.");
                return false;
            }

            StageShopRuntimeOwnership ownership =
                GameSession.Instance?.StageSession?.Shops;
            string nodeId = currentNode?.nodeId ?? string.Empty;
            string stockKey = data.HasCompleteIdentity
                ? $"{data.serviceId}|{data.stockReservationId}|{data.stockReceiptId}|{nodeId}"
                : string.Empty;
            Shop.ShopRuntimeData restored = null;
            if (data.HasCompleteIdentity)
            {
                if (ownership == null || ownership.IsComplete(data.nodeCompletionReceiptId + "|" + nodeId))
                {
                    return false;
                }
                ownership.TryGetStock(stockKey, out restored);
            }

            int deterministicSeed = StableShopSeed(data.serviceId, nodeId);
            bool opened = playerPopupCoordinator.OpenShop(
                data, restored, data.HasCompleteIdentity ? deterministicSeed : null,
                data.HasCompleteIdentity ? data.serviceId : null);
            if (!opened) return false;

            if (data.HasCompleteIdentity && restored == null)
            {
                Shop.ShopRuntimeData generated = Shop.StageShopManager.Instance?.CurrentShop;
                if (!ownership.TryStoreStock(stockKey, generated))
                {
                    playerPopupCoordinator.CloseCurrentPanel();
                    return false;
                }
            }
            pendingShopCompletionKey = data.HasCompleteIdentity
                ? data.nodeCompletionReceiptId + "|" + nodeId
                : "legacy|" + nodeId;
            return true;
        }

        private void HandleShopClosed()
        {
            if (string.IsNullOrWhiteSpace(pendingShopCompletionKey)) return;
            string completionKey = pendingShopCompletionKey;
            pendingShopCompletionKey = null;
            StageShopRuntimeOwnership ownership = GameSession.Instance?.StageSession?.Shops;
            if (completionKey.StartsWith("legacy|") || ownership?.TryComplete(completionKey) == true)
            {
                Complete();
            }
        }

        private static int StableShopSeed(string serviceId, string nodeId)
        {
            unchecked
            {
                uint hash = 2166136261;
                string value = (serviceId ?? string.Empty) + "|" + (nodeId ?? string.Empty);
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619;
                }
                return (int)(hash & 0x7fffffff);
            }
        }

        private bool LogShrineExecution(
            ShrineExecutionData data)
        {
            if (data?.config == null || data.god == null)
            {
                Debug.LogWarning(
                    "[ChoiceExecution][Shrine][Deferred] "
                    + "Shrine config and god are required.");
                return false;
            }

            Debug.Log(
                "[ChoiceExecution][Shrine][Deferred] "
                + "Shrine entry call point reached. "
                + $"config={data.config.name}, "
                + $"god={data.god.name}, "
                + $"godType={data.god.GodType}. "
                + "Actual ShrineManager connection is deferred.");
            Complete();
            return true;
        }

        private void ResetPendingChoiceState()
        {
            pendingChoice = null;
            pendingExecutionId = null;
            continuationGate.Reset();
        }

        private void EnsureSafeGrowthAdapterAndRoute(PopupEventSO popup, RoundNode node)
        {
            GameSession game = GameSession.Instance;
            if (game?.StageSession == null || game.BattleSession?.PartyRuntimeData == null)
                return;
            EquipmentSkillSO[] catalog = ResolveSafeGrowthSkillCatalog(game);
            safeGrowthAdapter = new SafeGrowthPopupRuntimeAdapter(game.StageSession,
                game.BattleSession.PartyRuntimeData, catalog,
                executionRouter ??= ChoiceExecutionRouter.CreateDefault());
            game.StageSession.ConfigurePortfolioRandomGrowthRuntime(
                game.ProgressionSession, game.BattleSession.PartyRuntimeData);
            portfolioRandomGrowthAdapter = new PortfolioRandomGrowthPopupRuntimeAdapter(game.StageSession);
            if (game.StageSession.SafeGrowthRouteEncounter != null || stageManager == null
                || !stageManager.TryResolveSvgPlacement(node, out string sectionId, out string slotId))
                return;
            new SafeGrowthRouteEntryBridge().TryEnter(game.StageSession, sectionId, slotId,
                node.nodeId, node.roundNodeSO?.nodeId,
                game.StageSession.SafeGrowthPlacement?.Assignment?.DisplayedEventId,
                game.BattleSession.PartyRuntimeData, catalog);
        }

        private void ApplyPortfolioChoicePrevalidation(EventPopupView view, PopupEventSO popup)
        {
            if (view == null || popup?.choices == null) return;
            foreach (PopupEventChoice choice in popup.choices)
                if (choice?.executionConfig?.data is PortfolioOutcomeExecutionData data
                    && !portfolioOutcomeService.CanExecuteChoice(data,
                        out string disabledCopy, out _))
                    view.SetChoiceDisabled(choice.choiceId, disabledCopy);
        }

        private static EquipmentSkillSO[] ResolveSafeGrowthSkillCatalog(GameSession game)
        {
            if (game?.BattleSession?.PartyRuntimeData?.Members == null)
                return System.Array.Empty<EquipmentSkillSO>();
            return game.BattleSession.PartyRuntimeData.Members
                .Where(x => x?.characterSO?.Skills != null)
                .SelectMany(x => x.characterSO.Skills)
                .Where(x => x?.skillSo != null)
                .Select(x => x.skillSo)
                .GroupBy(x => x.EquipmentId, System.StringComparer.Ordinal)
                .Select(x => x.First()).ToArray();
        }

        private void CloseAfterSafeTerminal()
        {
            PopupEventSO closedEvent = currentEvent;
            RoundNode node = currentNode;
            currentEvent = null;
            currentNode = null;
            ResetPendingChoiceState();
            UIPopupViewController.Instance?.Close(PopupType.EventPopup);
            OnPopupEventClosed?.Invoke(closedEvent, node);
        }

        private bool TryRenderSafeGrowthPresentation()
        {
            if (safeGrowthView == null || safeGrowthAdapter == null)
                return false;
            if (!string.Equals(currentEvent?.eventId, ConfirmableChoiceContract.SourcePopupId,
                    System.StringComparison.Ordinal))
                return false;
            RandomGrowthPresentationCopyAsset catalog = Resources.Load<RandomGrowthPresentationCopyAsset>(
                "Stage/RandomGrowth/Presentation/event.act1.random_growth.02.windworn_sword_marks.ko-KR");
            if (!SafeGrowthPlayerEvidenceOrchestrator.TryValidateIdentity(currentEvent, catalog,
                    out _, out _, out _))
            {
                ExecuteSafePresentationUnavailable();
                return true;
            }
            if (safeGrowthPresentationBinder.TryBuild(currentEvent, catalog, safeGrowthAdapter,
                    out SafeGrowthPresentationSnapshot snapshot, out _))
            {
                safeGrowthView.SetSafeGrowthPresentation(snapshot, HandleSafeGrowthIntent);
                return true;
            }
            ExecuteSafePresentationUnavailable();
            return true;
        }

        private void HandleSafeGrowthIntent(SafeGrowthPresentationActionIntent intent)
        {
            switch (intent)
            {
                case SafeGrowthPresentationActionIntent.RequestObservePreconfirm:
                    SelectChoiceById(SafeGrowthTransactionIds.ObserveChoiceId); break;
                case SafeGrowthPresentationActionIntent.ConfirmDecline:
                    if (safeGrowthAdapter?.Pending == null)
                        SelectChoiceById(SafeGrowthTransactionIds.DeclineChoiceId);
                    else ConfirmSafePending();
                    break;
                case SafeGrowthPresentationActionIntent.CancelPreconfirm:
                    CancelSafePending(); break;
                case SafeGrowthPresentationActionIntent.RecheckEligibility:
                    RecheckSafeEligibility(); break;
                case SafeGrowthPresentationActionIntent.ConfirmObserve:
                case SafeGrowthPresentationActionIntent.RetrySameChoice:
                    ConfirmSafePending(); break;
                case SafeGrowthPresentationActionIntent.OpenGrowthOffer:
                    if (TryOpenSafeGrowthOffer()) CloseAfterSafeTerminal();
                    return;
                case SafeGrowthPresentationActionIntent.ContinueStage:
                    CloseAfterSafeTerminal(); return;
                default: return;
            }
            if (currentEvent != null) TryRenderSafeGrowthPresentation();
        }

        private bool TryOpenSafeGrowthOffer()
        {
            GameSession game = GameSession.Instance;
            if (game?.StageSession?.SafeGrowthRuntime?.ProgressionLedger == null
                || game.BattleSession?.PartyRuntimeData == null)
                return false;
            safeGrowthOfferPresenter ??= new SafeGrowthPartyWideOfferPresenter(
                game.StageSession.SafeGrowthRuntime.ProgressionLedger,
                game.BattleSession.PartyRuntimeData,
                ResolveSafeGrowthSkillCatalog(game),
                new SafeGrowthSkillUpgradeViewHost());
            SafeGrowthPartyWideOfferOpenResult result = safeGrowthOfferPresenter.Open();
            return result == SafeGrowthPartyWideOfferOpenResult.Opened
                || result == SafeGrowthPartyWideOfferOpenResult.AlreadyOpen
                || result == SafeGrowthPartyWideOfferOpenResult.AlreadyApplied;
        }

        private void ExecuteSafePresentationUnavailable()
        {
            GameSession game = GameSession.Instance;
            StageSession session = game?.StageSession;
            if (session?.SafeGrowthRuntime?.IsReady != true || currentNode == null) return;
            SafeGrowthInteractionToken token = session.SafeGrowthInteraction.Token;
            if (token == null)
            {
                SafeGrowthInteractionKey key = new(session.SafeGrowthRuntime.RunId.Value,
                    session.RandomGrowthSession.StageGenerationId,
                    SafeGrowthTransactionIds.ReservationId, currentNode.nodeId);
                session.SafeGrowthInteraction.TryEnterPreconfirm(key,
                    SafeGrowthTransactionIds.ObserveChoiceId,
                    SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint, true, out token);
            }
            if (token == null) return;
            var cause = new SafeGrowthNodeOnlyFailureCause(token.Key.RunId,
                token.Key.StageGenerationId, SafeGrowthTransactionIds.EventId,
                currentNode.roundNodeSO?.nodeId, SafeGrowthTransactionIds.ReservationId,
                currentNode.nodeId, SafeGrowthNodeOnlyFailureIds.ReceiptId);
            string revision = StageAtomicNodeCompletionService.ComputeRevision(session.RuntimeData?.currentGraph);
            SafeGrowthNodeOnlyFailureExecutionResult result = session.SafeGrowthRuntime.NodeOnlyFailureCoordinator.Execute(
                session.SafeGrowthRuntime.NodeOnlyFailure, cause, token,
                session.SafeGrowthRuntime.NodeCompletion, session, revision,
                () => stageManager?.PublishAtomicCompletion(currentNode, stageManager.ProgressState), out _);
            if (result == SafeGrowthNodeOnlyFailureExecutionResult.Succeeded)
                CloseAfterSafeTerminal();
        }
    }
}
