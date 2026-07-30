using Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Stage.UI
{
    /// <summary>
    /// 스테이지 맵에서 개별 노드를 표현하고 입력 상태를 제어하는 컴포넌트.
    /// 시각적 연출(Hover, Click, Disabled/Normal 톤)은 UINodeSlot 컴포넌트에 위임합니다.
    /// </summary>
    public class RoundNodeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        [Header("UI References")]
        [SerializeField] private Transform iconContainer;
        [SerializeField] private GameObject selectedMark;
        [SerializeField] private GameObject clearedMark;
        [SerializeField] private CanvasGroup canvasGroup;

        private RoundNode node;
        private GameObject spawnedVisualObject;
        private UINodeSlot spawnedVisualSlot;
        private NodeIconType currentVisualType = NodeIconType.None;

        public RoundNode Node => node;
        public UINodeSlot VisualSlot => spawnedVisualSlot;

        public void Initialize(RoundNode nodeData)
        {
            node = nodeData;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            UpdateIconAndVisual();
            Refresh();
        }

        public void Refresh()
        {
            if (node == null)
            {
                return;
            }

            bool visible = IsNodeVisible();
            ApplyVisibility(visible);

            if (!visible)
            {
                return;
            }

            if (selectedMark != null)
            {
                selectedMark.SetActive(node.isSelected);
            }

            if (clearedMark != null)
            {
                clearedMark.SetActive(node.IsCompleted);
            }

            UpdateIconAndVisual();

            if (spawnedVisualSlot != null)
            {
                spawnedVisualSlot.SetNodeData(node);
            }
        }

        private void UpdateIconAndVisual()
        {
            if (node == null)
            {
                return;
            }

            if (LibraryManager.Instance == null)
            {
                Debug.LogWarning($"[RoundNodeButton] LibraryManager.Instance is null. NodeId={node.nodeId}");
                return;
            }

            NodeIconType iconType = node.resolvedIconType;
            GameObject visualPrefab = LibraryManager.Instance.GetNodePrefab(iconType);

            UpdateVisualPrefab(visualPrefab, iconType);
        }

        private void UpdateVisualPrefab(GameObject prefab, NodeIconType iconType)
        {
            if (currentVisualType == iconType && spawnedVisualObject != null)
            {
                return;
            }

            if (spawnedVisualObject != null)
            {
                Destroy(spawnedVisualObject);
                spawnedVisualObject = null;
                spawnedVisualSlot = null;
            }

            currentVisualType = iconType;

            if (prefab != null)
            {
                Transform parentTransform = iconContainer != null ? iconContainer : transform;

                spawnedVisualObject = Instantiate(prefab, parentTransform, false);

                if (spawnedVisualObject != null)
                {
                    spawnedVisualObject.transform.localPosition = Vector3.zero;
                    spawnedVisualObject.transform.localScale = Vector3.one;
                    spawnedVisualObject.transform.localRotation = Quaternion.identity;

                    if (spawnedVisualObject.transform is RectTransform rect)
                    {
                        rect.anchoredPosition = Vector2.zero;
                        rect.localScale = Vector3.one;
                    }

                    spawnedVisualSlot = spawnedVisualObject.GetComponent<UINodeSlot>()
                        ?? spawnedVisualObject.GetComponentInChildren<UINodeSlot>();

                    if (spawnedVisualSlot == null)
                    {
                        if (iconType == NodeIconType.Shrine)
                        {
                            spawnedVisualSlot = spawnedVisualObject.AddComponent<UINodeSlot_Shrine>();
                        }
                        else
                        {
                            spawnedVisualSlot = spawnedVisualObject.AddComponent<UINodeSlot>();
                        }
                    }

                    if (spawnedVisualSlot != null && node != null)
                    {
                        spawnedVisualSlot.SetNodeData(node);
                    }
                }
            }
            else
            {
                spawnedVisualSlot = GetComponent<UINodeSlot>() ?? GetComponentInChildren<UINodeSlot>();
                if (spawnedVisualSlot != null && node != null)
                {
                    spawnedVisualSlot.SetNodeData(node);
                }
            }
        }

        private bool IsNodeVisible()
        {
            return node != null;
        }

        private void ApplyVisibility(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
                return;
            }

            if (selectedMark != null)
            {
                selectedMark.SetActive(visible && node != null && node.isSelected);
            }

            if (clearedMark != null)
            {
                clearedMark.SetActive(visible && node != null && node.IsCompleted);
            }
        }

        #region Pointer & Interaction Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (node == null || !node.IsAvailable || node.IsCompleted) return;

            if (spawnedVisualSlot != null)
            {
                spawnedVisualSlot.OnPointerEnter();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (node == null) return;

            if (spawnedVisualSlot != null)
            {
                spawnedVisualSlot.OnPointerExit();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (node == null || !node.IsAvailable || node.IsCompleted) return;

            if (spawnedVisualSlot != null)
            {
                spawnedVisualSlot.OnPointerClick();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick();
        }

        private void OnClick()
        {
            if (node == null) return;
            if (!node.CanExecute()) return;

            if (spawnedVisualSlot != null)
            {
                spawnedVisualSlot.OnPointerClick();
            }

            if (StageManager.Instance != null)
            {
                StageManager.Instance.SelectNode(node.nodeId);
            }
        }
        #endregion
    }
}