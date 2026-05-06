using UnityEngine;

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
        [SerializeField] private StageRuntime stageRuntime;

        [Header("Runtime")]
        private PopupEventSO currentEvent;
        private RoundNode currentNode;

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

            if (stageRuntime == null)
            {
                stageRuntime = StageRuntime.Instance;
            }
        }

        private void OnEnable()
        {
            if (stageRuntime == null)
            {
                stageRuntime = StageRuntime.Instance;
            }

            stageRuntime.OnNodeSelected += HandleNodeSelected;
            stageRuntime.OnStageGenerated += HandleStageGenerated;
        }

        private void OnDisable()
        {
            if (stageRuntime == null)
            {
                return;
            }

            stageRuntime.OnNodeSelected -= HandleNodeSelected;
            stageRuntime.OnStageGenerated -= HandleStageGenerated;
        }

        private void HandleStageGenerated(StageGraph graph)
        {
            currentEvent = null;
            currentNode = null;
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

            OnPopupEventChoiceSelected?.Invoke(currentEvent, choice, currentNode);

            if (choice.nextEvent != null)
            {
                Open(choice.nextEvent, currentNode);
                return;
            }

            if (choice.completesEvent)
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

            OnPopupEventClosed?.Invoke(closedEvent, node);

            if (stageRuntime != null && stageRuntime.CurrentNode == node)
            {
                stageRuntime.CompleteCurrentNode();
            }
        }

        public void CloseWithoutComplete()
        {
            PopupEventSO closedEvent = currentEvent;
            RoundNode node = currentNode;

            currentEvent = null;
            currentNode = null;

            OnPopupEventClosed?.Invoke(closedEvent, node);
        }
    }
}