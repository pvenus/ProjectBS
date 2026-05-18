using UnityEngine;
using Item;
using Stat;
using Shrine;

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
        private PopupEventSO currentEvent;
        private RoundNode currentNode;
        private PopupEventChoice pendingChoice;

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
            pendingChoice = null;
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

            if (node.executeMode != RoundExecuteMode.Popup)
            {
                return false;
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
            currentEvent = popupEvent;
            currentNode = node;

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

            SelectChoice(currentEvent.choices[index]);
        }

        public void SelectChoice(PopupEventChoice choice)
        {
            if (currentEvent == null || choice == null)
            {
                return;
            }

            pendingChoice = choice;

            if (rewardExecutor != null)
            {
                rewardExecutor.Execute(choice.rewards);
            }

            OnPopupEventChoiceSelected?.Invoke(currentEvent, choice, currentNode);
        }

        public void ConfirmChoiceResult()
        {
            if (pendingChoice == null)
            {
                return;
            }

            PopupEventChoice confirmedChoice = pendingChoice;
            pendingChoice = null;

            if (confirmedChoice.nextEvent != null)
            {
                Open(confirmedChoice.nextEvent, currentNode);
                return;
            }

            bool shouldCompleteEvent =
                confirmedChoice.completesEvent
                || confirmedChoice.nextEvent == null;

            if (shouldCompleteEvent)
            {
                Complete();
            }
        }

        public void Complete()
        {
            PopupEventSO closedEvent = currentEvent;
            RoundNode node = currentNode;

            currentEvent = null;
            currentNode = null;
            pendingChoice = null;

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
            pendingChoice = null;

            OnPopupEventClosed?.Invoke(closedEvent, node);
        }
    }
}