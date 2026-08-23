using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Battle.UI.StrategicBoard
{
    public enum StrategicSkillSlotState
    {
        Empty,
        Locked,
        Ready,
        InsufficientResource,
        Disabled
    }

    /// <summary>
    /// Prototype slot presentation and input boundary. Execution is intentionally delegated
    /// to consumers through events so the existing strategic-skill path can be integrated later.
    /// </summary>
    public sealed class StrategicSkillSlotView : MonoBehaviour,
        IPointerClickHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Identity")]
        [SerializeField] private string slotId;

        [Header("Independent Visual Layers")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Image selectionImage;
        [SerializeField] private Image insufficientResourceImage;
        [SerializeField] private Image emptySlotImage;
        [SerializeField] private Image lockImage;

        [Header("Interaction")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform dragVisual;
        [SerializeField, Range(0f, 1f)] private float draggingAlpha = 0.65f;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Vector2 originalAnchoredPosition;
        private UnityEngine.Object executionPayload;
        private bool isSelected;
        private bool isDragging;
        [SerializeField] private int cost;
        [SerializeField] private StrategicSkillSlotState state = StrategicSkillSlotState.Empty;

        public event Action<StrategicSkillSlotView, PointerEventData> Selected;
        public event Action<StrategicSkillSlotView, PointerEventData> DragStarted;
        public event Action<StrategicSkillSlotView, PointerEventData> Dragged;
        public event Action<StrategicSkillSlotView, PointerEventData> DragEnded;
        public event Action<StrategicSkillSlotView, UnityEngine.Object, PointerEventData> ExecutionRequested;

        public string SlotId => slotId;
        public int Cost => cost;
        public StrategicSkillSlotState State => state;
        public bool IsSelected => isSelected;
        public bool CanInteract => state == StrategicSkillSlotState.Ready;
        public UnityEngine.Object ExecutionPayload => executionPayload;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            rootCanvas = GetComponentInParent<Canvas>();

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (dragVisual == null)
            {
                dragVisual = rectTransform;
            }

            RefreshVisuals();
        }

        public void SetSlotId(string value)
        {
            slotId = value;
        }

        public void SetContent(Sprite icon, int gaugeCost, UnityEngine.Object payload = null)
        {
            cost = Mathf.Max(0, gaugeCost);
            executionPayload = payload;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            if (costText != null)
            {
                costText.text = cost.ToString();
            }

            if (state == StrategicSkillSlotState.Empty)
            {
                SetState(StrategicSkillSlotState.Ready);
            }
        }

        public void ClearContent()
        {
            executionPayload = null;
            cost = 0;
            isSelected = false;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (costText != null)
            {
                costText.text = string.Empty;
            }

            SetState(StrategicSkillSlotState.Empty);
        }

        public void SetState(StrategicSkillSlotState value)
        {
            state = value;

            if (!CanInteract)
            {
                SetSelected(false);
            }

            RefreshVisuals();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected && CanInteract;

            if (selectionImage != null)
            {
                selectionImage.enabled = isSelected;
            }
        }

        public void SetResourceAvailable(bool available)
        {
            if (state == StrategicSkillSlotState.Empty ||
                state == StrategicSkillSlotState.Locked ||
                state == StrategicSkillSlotState.Disabled)
            {
                return;
            }

            SetState(available
                ? StrategicSkillSlotState.Ready
                : StrategicSkillSlotState.InsufficientResource);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!CanInteract || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            SetSelected(true);
            Selected?.Invoke(this, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanInteract || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            isDragging = true;
            SetSelected(true);

            if (dragVisual != null)
            {
                originalAnchoredPosition = dragVisual.anchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = draggingAlpha;
                canvasGroup.blocksRaycasts = false;
            }

            DragStarted?.Invoke(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            if (dragVisual != null)
            {
                float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
                dragVisual.anchoredPosition += eventData.delta / Mathf.Max(0.01f, scaleFactor);
            }

            Dragged?.Invoke(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;

            if (dragVisual != null)
            {
                dragVisual.anchoredPosition = originalAnchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            DragEnded?.Invoke(this, eventData);
            ExecutionRequested?.Invoke(this, executionPayload, eventData);
        }

        private void RefreshVisuals()
        {
            if (selectionImage != null)
            {
                selectionImage.enabled = isSelected && CanInteract;
            }

            if (insufficientResourceImage != null)
            {
                insufficientResourceImage.enabled = state == StrategicSkillSlotState.InsufficientResource;
            }

            if (emptySlotImage != null)
            {
                emptySlotImage.enabled = state == StrategicSkillSlotState.Empty;
            }

            if (lockImage != null)
            {
                lockImage.enabled = state == StrategicSkillSlotState.Locked;
            }

            if (iconImage != null)
            {
                iconImage.color = CanInteract ? Color.white : new Color(0.35f, 0.35f, 0.35f, 1f);
            }

            if (canvasGroup != null && !isDragging)
            {
                canvasGroup.alpha = state == StrategicSkillSlotState.Disabled ? 0.45f : 1f;
                canvasGroup.blocksRaycasts = CanInteract;
                canvasGroup.interactable = CanInteract;
            }
        }
    }
}
