using UnityEngine;
using Item;
using Stat;
using Session;
using UIFramework.Data;
using System.Collections.Generic;

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

        [Header("Runtime")]
        private EventRewardExecutor rewardExecutor;
        private StageEventResolver eventResolver;
        private ChoiceExecutionRouter executionRouter;
        private IChoiceRewardPresentation rewardPresentation;
        private PopupEventSO currentEvent;
        private RoundNode currentNode;
        private PopupEventChoice pendingChoice;
        private string pendingExecutionId;
        private int selectionSequence;
        private readonly ChoiceContinuationGate continuationGate =
            new();

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

            stageManager.OnNodeSelected += HandleNodeSelected;
            stageManager.OnStageGenerated += HandleStageGenerated;
        }

        private void OnDisable()
        {
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
                node.nodeType != RoundNodeType.RequiredSubEvent)
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

            // UIPopupViewController 로 EventPopupView 열기
            if (UIPopupViewController.Instance != null)
            {
                EventPopupView view = UIPopupViewController.Instance.Open<EventPopupView>(PopupType.EventPopup);
                if (view != null)
                {
                    EventPopupViewData viewData = EventPopupViewDataBuilder.Build(popupEvent, node);
                    view.SetData(viewData);
                    view.OnChoiceSelected += SelectChoiceById;
                    view.OnChoiceConfirmed += ConfirmChoiceResult;
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

            OnPopupEventClosed?.Invoke(closedEvent, node);

            if (stageManager != null && stageManager.CurrentNode == node)
            {
                stageManager.CompleteCurrentNode();
            }
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
                            LogShopExecution,
                        openShrine:
                            LogShrineExecution);
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

            Complete();
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

        private bool LogShopExecution(
            ShopExecutionData data)
        {
            if (data == null)
            {
                Debug.LogWarning(
                    "[ChoiceExecution][Shop][Deferred] "
                    + "Shop execution data is null.");
                return false;
            }

            string poolNames = data.pools == null
                ? "(null)"
                : string.Join(
                    ", ",
                    data.pools.ConvertAll(
                        pool => pool == null
                            ? "(null)"
                            : pool.name));

            Debug.Log(
                "[ChoiceExecution][Shop][Deferred] "
                + "Shop entry call point reached. "
                + $"shopType={data.shopType}, "
                + $"itemCount={data.itemCount}, "
                + $"pools=[{poolNames}]. "
                + "Actual StageShopManager connection is deferred.");
            Complete();
            return true;
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
    }
}
