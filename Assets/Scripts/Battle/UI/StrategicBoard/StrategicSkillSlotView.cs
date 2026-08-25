using System;
using System.Collections;
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
        IPointerEnterHandler,
        IPointerExitHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        [Header("Identity")]
        [SerializeField] private string slotId;

        [Header("Base State Roots")]
        [SerializeField] private RectTransform activeRoot;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject emptyRoot;

        [Header("Overlay State Roots")]
        [SerializeField] private GameObject selectionRoot;
        [SerializeField] private Image selectionImage;
        [SerializeField] private GameObject insufficientRoot;
        [SerializeField] private Image insufficientFillImage;
        [SerializeField] private GameObject overlayLockRoot;

        [Header("Labels")]
        [SerializeField] private TMP_Text costText;

        [Header("Interaction")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform dragVisual;
        [SerializeField, Range(0f, 1f)] private float draggingAlpha = 0.65f;

        [Header("Ready Pulse")]
        [SerializeField, Min(1f)] private float pulseScale = 1.12f;
        [SerializeField, Min(0f)] private float scaleUpDuration = 0.12f;
        [SerializeField, Min(0f)] private float scaleDownDuration = 0.16f;

        private RectTransform rectTransform;
        private Canvas rootCanvas;
        private Vector2 originalAnchoredPosition;
        private UnityEngine.Object executionPayload;
        private bool isSelected;
        private bool isPointerInside;
        private bool suppressHoverUntilExit;
        private bool isDragging;
        private Vector3 activeBaseScale;
        private bool activeBaseScaleCaptured;
        private Coroutine readyPulseCoroutine;
        [SerializeField] private int cost;
        [SerializeField] private StrategicSkillSlotState state = StrategicSkillSlotState.Empty;

        public event Action<StrategicSkillSlotView, PointerEventData> Selected;
        public event Action<StrategicSkillSlotView, PointerEventData> HoverEntered;
        public event Action<StrategicSkillSlotView, PointerEventData> DragStarted;
        public event Action<StrategicSkillSlotView, PointerEventData> Dragged;
        public event Action<StrategicSkillSlotView, PointerEventData> DragEnded;
        public event Action<StrategicSkillSlotView, UnityEngine.Object, PointerEventData> ExecutionRequested;

        public string SlotId => slotId;
        public int Cost => cost;
        public StrategicSkillSlotState State => state;
        public bool IsSelected => isSelected;
        public bool IsPointerInside => isPointerInside;
        public bool IsSelectionVisible => CanInteract &&
            (isSelected || (isPointerInside && !suppressHoverUntilExit));
        public bool CanInteract => state == StrategicSkillSlotState.Ready;
        public UnityEngine.Object ExecutionPayload => executionPayload;
        public float InsufficientFillAmount => insufficientFillImage != null
            ? insufficientFillImage.fillAmount
            : 0f;

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

            CaptureActiveBaseScale();
            RefreshVisuals();
        }

        private void OnDisable()
        {
            ResetInteractionState();
            CancelReadyPulseAndRestoreScale();
        }

        private void OnDestroy()
        {
            CancelReadyPulseAndRestoreScale();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            pulseScale = Mathf.Max(1f, pulseScale);
            scaleUpDuration = Mathf.Max(0f, scaleUpDuration);
            scaleDownDuration = Mathf.Max(0f, scaleDownDuration);

            if (insufficientFillImage != null)
            {
                insufficientFillImage.type = Image.Type.Filled;
            }
        }
#endif

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

            if (selectionImage != null)
            {
                selectionImage.sprite = icon;
                selectionImage.enabled = icon != null;
            }

            if (costText != null)
            {
                costText.text = cost.ToString();
            }

            if (state == StrategicSkillSlotState.Empty ||
                state == StrategicSkillSlotState.Ready ||
                state == StrategicSkillSlotState.InsufficientResource)
            {
                SetState(StrategicSkillSlotState.Ready);
            }
            else
            {
                RefreshVisuals();
            }
        }

        public void ClearContent()
        {
            executionPayload = null;
            cost = 0;
            ResetInteractionState();

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }

            if (selectionImage != null)
            {
                selectionImage.sprite = null;
                selectionImage.enabled = false;
            }

            if (costText != null)
            {
                costText.text = string.Empty;
            }

            SetState(StrategicSkillSlotState.Empty);
        }

        public void SetState(StrategicSkillSlotState value)
        {
            SetStateInternal(value);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected && CanInteract;
            RefreshVisuals();
        }

        public void ClearSelectionAfterInteraction()
        {
            isSelected = false;
            suppressHoverUntilExit = isPointerInside;
            RefreshVisuals();
        }

        public void SuppressHoverSelectionUntilExit()
        {
            suppressHoverUntilExit = isPointerInside;
            RefreshVisuals();
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

        public void SetResourceGauge(int currentGauge)
        {
            int safeCurrentGauge = Mathf.Max(0, currentGauge);
            float fillAmount = cost <= 0
                ? 0f
                : 1f - Mathf.Clamp01(safeCurrentGauge / (float)cost);

            if (insufficientFillImage != null)
            {
                insufficientFillImage.type = Image.Type.Filled;
                insufficientFillImage.fillAmount = fillAmount;
            }

            if (state == StrategicSkillSlotState.Empty ||
                state == StrategicSkillSlotState.Locked ||
                state == StrategicSkillSlotState.Disabled)
            {
                RefreshVisuals();
                return;
            }

            StrategicSkillSlotState previousState = state;
            StrategicSkillSlotState nextState = safeCurrentGauge >= cost
                ? StrategicSkillSlotState.Ready
                : StrategicSkillSlotState.InsufficientResource;
            SetStateInternal(nextState);

            if (previousState == StrategicSkillSlotState.InsufficientResource &&
                nextState == StrategicSkillSlotState.Ready)
            {
                PlayReadyPulse();
            }
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerInside = true;
            RefreshVisuals();

            if (CanInteract && !suppressHoverUntilExit)
            {
                HoverEntered?.Invoke(this, eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            suppressHoverUntilExit = false;
            RefreshVisuals();
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

            ClearSelectionAfterInteraction();

            DragEnded?.Invoke(this, eventData);
            ExecutionRequested?.Invoke(this, executionPayload, eventData);
        }

        private void RefreshVisuals()
        {
            bool hasContent = state != StrategicSkillSlotState.Empty;
            SetRootActive(activeRoot != null ? activeRoot.gameObject : null, hasContent);
            SetRootActive(emptyRoot, !hasContent);
            SetRootActive(selectionRoot, IsSelectionVisible);
            SetRootActive(
                insufficientRoot,
                state == StrategicSkillSlotState.InsufficientResource);
            SetRootActive(overlayLockRoot, state == StrategicSkillSlotState.Locked);

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

        private void SetStateInternal(StrategicSkillSlotState value)
        {
            state = value;

            if (!CanInteract)
            {
                isSelected = false;
            }

            if (state != StrategicSkillSlotState.Ready)
            {
                CancelReadyPulseAndRestoreScale();
            }

            RefreshVisuals();
        }

        private void ResetInteractionState()
        {
            bool wasDragging = isDragging;
            isSelected = false;
            isPointerInside = false;
            suppressHoverUntilExit = false;
            isDragging = false;

            if (wasDragging && dragVisual != null)
            {
                dragVisual.anchoredPosition = originalAnchoredPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = CanInteract;
                canvasGroup.interactable = CanInteract;
            }

            RefreshVisuals();
        }

        private void PlayReadyPulse()
        {
            if (!isActiveAndEnabled || activeRoot == null)
            {
                return;
            }

            CaptureActiveBaseScale();
            CancelReadyPulseAndRestoreScale();
            readyPulseCoroutine = StartCoroutine(ReadyPulseRoutine());
        }

        private IEnumerator ReadyPulseRoutine()
        {
            Vector3 pulseTargetScale = new(
                activeBaseScale.x * Mathf.Max(1f, pulseScale),
                activeBaseScale.y * Mathf.Max(1f, pulseScale),
                activeBaseScale.z);

            yield return AnimateActiveScale(
                activeBaseScale,
                pulseTargetScale,
                Mathf.Max(0f, scaleUpDuration));
            yield return AnimateActiveScale(
                pulseTargetScale,
                activeBaseScale,
                Mathf.Max(0f, scaleDownDuration));

            activeRoot.localScale = activeBaseScale;
            readyPulseCoroutine = null;
        }

        private IEnumerator AnimateActiveScale(Vector3 from, Vector3 to, float duration)
        {
            if (duration <= Mathf.Epsilon)
            {
                activeRoot.localScale = to;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                activeRoot.localScale = Vector3.Lerp(
                    from,
                    to,
                    Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            activeRoot.localScale = to;
        }

        private void CaptureActiveBaseScale()
        {
            if (activeRoot == null || activeBaseScaleCaptured)
            {
                return;
            }

            activeBaseScale = activeRoot.localScale;
            activeBaseScaleCaptured = true;
        }

        private void CancelReadyPulseAndRestoreScale()
        {
            if (readyPulseCoroutine != null)
            {
                StopCoroutine(readyPulseCoroutine);
                readyPulseCoroutine = null;
            }

            if (activeRoot != null && activeBaseScaleCaptured)
            {
                activeRoot.localScale = activeBaseScale;
            }
        }

        private static void SetRootActive(GameObject root, bool active)
        {
            if (root != null && root.activeSelf != active)
            {
                root.SetActive(active);
            }
        }
    }
}
