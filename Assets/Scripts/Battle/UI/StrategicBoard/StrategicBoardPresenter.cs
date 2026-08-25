using System.Collections.Generic;
using Battle;
using Character;
using Item;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// ItemManager 및 StrategicSkillCostManager의 런타임 데이터를 StrategicBoardView에 바인딩하고,
    /// 슬롯의 실행 요청 이벤트를 ItemManager의 스킬 발동 API로 중계하는 Presenter 컴포넌트.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StrategicBoardPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private StrategicBoardView boardView;

        [Header("Camera")]
        [SerializeField] private Camera worldCamera;

        [Header("Targeting Guide")]
        [SerializeField] private StrategicSkillTargetingGuideView targetingGuidePrefab;
        [SerializeField] private StrategicSkillTargetingGuideView targetingGuideView;
        [SerializeField] private Transform targetingGuideParent;

        [Header("Target Tint Preview")]
        [SerializeField] private bool enableTargetTint = true;
        [SerializeField] private Color targetTint = new(1f, 0.25f, 0.25f, 1f);
        [SerializeField] private bool useCalibratedDisplayRadiusForTargetDetection;

        [Header("Debug")]
        [SerializeField] private bool logDebug;

        private ItemManager subscribedItemManager;
        private StrategicSkillCostManager subscribedCostManager;
        private readonly List<StrategicSkillSlotView> boundSlots = new();
        private StrategicSkillSlotView activeDragSlot;
        private bool ownsTargetingGuideView;
        private bool dragWarningIssued;
        private bool targetTintWarningIssued;
        private readonly List<Collider2D> targetOverlapResults = new(128);
        private readonly HashSet<CharacterManager> detectedTargets = new();
        private readonly List<CharacterManager> targetsToRestore = new();
        private readonly Dictionary<CharacterManager, List<ShaderTintState>> highlightedTargets = new();

        public StrategicBoardView BoardView => boardView;
        public int CurrentHighlightedTargetCount => highlightedTargets.Count;
        public int CurrentTargetLayerMask { get; private set; }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeEvents();
            Rebind();
        }

        private void OnDisable()
        {
            ClearInteractionSelection(activeDragSlot);
            UnsubscribeEvents();
            HideTargetingGuide();
        }

        private void OnDestroy()
        {
            ClearTargetTintPreview();

            if (ownsTargetingGuideView && targetingGuideView != null)
            {
                Destroy(targetingGuideView.gameObject);
            }
        }

        private void ResolveReferences()
        {
            if (boardView == null)
            {
                boardView = GetComponent<StrategicBoardView>();
            }

            if (boardView != null)
            {
                boardView.EnsureSlotsReady();
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        [ContextMenu("Rebind")]
        public void Rebind()
        {
            if (boardView == null)
            {
                return;
            }

            IReadOnlyList<StrategicSkillSlotView> slots = boardView.Slots;
            if (slots == null || slots.Count == 0)
            {
                return;
            }

            ItemManager itemManager = ItemManager.Instance;
            StrategicSkillItemRuntimeData runtimeData = itemManager != null ? itemManager.StrategicSkillItemRuntimeData : null;
            IReadOnlyList<StrategicSkillItemRuntimeData.StrategicSkillItemEntry> entries = runtimeData?.StrategicSkillItems;

            int entryIndex = 0;
            int entryCount = entries?.Count ?? 0;

            for (int i = 0; i < slots.Count; i++)
            {
                StrategicSkillSlotView slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                // Find next valid owned item
                StrategicSkillItemSO itemSO = null;
                while (entryIndex < entryCount)
                {
                    StrategicSkillItemRuntimeData.StrategicSkillItemEntry entry = entries[entryIndex++];
                    if (entry != null && entry.Owned && entry.StrategicSkillItem != null)
                    {
                        itemSO = entry.StrategicSkillItem;
                        break;
                    }
                }

                if (itemSO != null)
                {
                    slot.SetContent(itemSO.icon, itemSO.gaugeCost, itemSO);
                    if (logDebug)
                    {
                        Debug.Log($"[StrategicBoardPresenter] Bound slot {slot.SlotId} with item {itemSO.DisplayName} (cost: {itemSO.gaugeCost}).", this);
                    }
                }
                else
                {
                    slot.ClearContent();
                }
            }

            StrategicSkillCostManager costManager = StrategicSkillCostManager.Instance;
            int currentGauge = costManager != null ? costManager.CurrentGauge : 0;
            boardView.RefreshSlotResourceStates(currentGauge);
        }

        private void SubscribeEvents()
        {
            UnsubscribeEvents();

            // 1. Subscribe to Slot Execution Events
            if (boardView != null && boardView.Slots != null)
            {
                foreach (StrategicSkillSlotView slot in boardView.Slots)
                {
                    if (slot != null)
                    {
                        slot.Selected += HandleSlotSelected;
                        slot.HoverEntered += HandleSlotHoverEntered;
                        slot.ExecutionRequested += HandleSlotExecutionRequested;
                        slot.DragStarted += HandleSlotDragStarted;
                        slot.Dragged += HandleSlotDragged;
                        slot.DragEnded += HandleSlotDragEnded;
                        boundSlots.Add(slot);
                    }
                }
            }

            // 2. Subscribe to ItemManager Events
            subscribedItemManager = ItemManager.Instance;
            if (subscribedItemManager != null)
            {
                subscribedItemManager.OnStrategicSkillItemAdded += HandleItemChanged;
                subscribedItemManager.OnStrategicSkillItemRemoved += HandleItemChanged;
            }

            // 3. Subscribe to CostManager Events
            subscribedCostManager = StrategicSkillCostManager.Instance;
            if (subscribedCostManager != null)
            {
                subscribedCostManager.OnGaugeChanged += HandleGaugeChanged;
            }
        }

        private void UnsubscribeEvents()
        {
            foreach (StrategicSkillSlotView slot in boundSlots)
            {
                if (slot != null)
                {
                    slot.Selected -= HandleSlotSelected;
                    slot.HoverEntered -= HandleSlotHoverEntered;
                    slot.ExecutionRequested -= HandleSlotExecutionRequested;
                    slot.DragStarted -= HandleSlotDragStarted;
                    slot.Dragged -= HandleSlotDragged;
                    slot.DragEnded -= HandleSlotDragEnded;
                }
            }
            boundSlots.Clear();

            if (subscribedItemManager != null)
            {
                subscribedItemManager.OnStrategicSkillItemAdded -= HandleItemChanged;
                subscribedItemManager.OnStrategicSkillItemRemoved -= HandleItemChanged;
                subscribedItemManager = null;
            }

            if (subscribedCostManager != null)
            {
                subscribedCostManager.OnGaugeChanged -= HandleGaugeChanged;
                subscribedCostManager = null;
            }

            activeDragSlot = null;
        }

        private void HandleSlotSelected(
            StrategicSkillSlotView selectedSlot,
            PointerEventData eventData)
        {
            SelectOnly(selectedSlot);
        }

        private void SelectOnly(StrategicSkillSlotView selectedSlot)
        {
            foreach (StrategicSkillSlotView slot in boundSlots)
            {
                if (slot != null)
                {
                    slot.SetSelected(slot == selectedSlot);
                }
            }
        }

        private void HandleSlotHoverEntered(
            StrategicSkillSlotView hoveredSlot,
            PointerEventData eventData)
        {
            if (activeDragSlot != null && activeDragSlot != hoveredSlot)
            {
                hoveredSlot.SuppressHoverSelectionUntilExit();
                return;
            }

            SelectOnly(null);
        }

        private void HandleSlotDragStarted(
            StrategicSkillSlotView slotView,
            PointerEventData eventData)
        {
            activeDragSlot = slotView;
            SelectOnly(slotView);
            dragWarningIssued = false;
            targetTintWarningIssued = false;
            UpdateTargetingGuide(slotView, eventData, true);
        }

        private void HandleSlotDragged(
            StrategicSkillSlotView slotView,
            PointerEventData eventData)
        {
            if (slotView != activeDragSlot)
            {
                return;
            }

            UpdateTargetingGuide(slotView, eventData, false);
        }

        private void HandleSlotDragEnded(
            StrategicSkillSlotView slotView,
            PointerEventData eventData)
        {
            HideTargetingGuide();
            ClearInteractionSelection(slotView);
            activeDragSlot = null;
            dragWarningIssued = false;
            targetTintWarningIssued = false;
        }

        private void HandleSlotExecutionRequested(StrategicSkillSlotView slotView, Object payload, PointerEventData eventData)
        {
            bool succeeded = false;

            try
            {
                HideTargetingGuide();

                if (payload is not StrategicSkillItemSO strategicSkillItem)
                {
                    if (logDebug)
                    {
                        Debug.LogWarning(
                            "[StrategicBoardPresenter] Execution failed: slot payload is not a StrategicSkillItemSO.",
                            this);
                    }

                    return;
                }

                if (eventData == null)
                {
                    if (logDebug)
                    {
                        Debug.LogWarning(
                            $"[StrategicBoardPresenter] Execution failed: PointerEventData is null. item={strategicSkillItem.strategicSkillItemId}",
                            this);
                    }

                    return;
                }

                if (worldCamera == null)
                {
                    worldCamera = Camera.main;
                }

                if (worldCamera == null)
                {
                    Debug.LogWarning(
                        $"[StrategicBoardPresenter] Execution failed: world camera is unavailable. item={strategicSkillItem.strategicSkillItemId}",
                        this);
                    return;
                }

                ItemManager itemManager = ItemManager.Instance;
                if (itemManager == null)
                {
                    Debug.LogError(
                        $"[StrategicBoardPresenter] Execution failed: ItemManager.Instance is null. item={strategicSkillItem.strategicSkillItemId}",
                        this);
                    return;
                }

                succeeded = itemManager.TryUseStrategicSkillItemFromScreenPosition(
                    strategicSkillItem,
                    eventData.position,
                    worldCamera,
                    logDebug,
                    this);

                if (logDebug)
                {
                    Debug.Log(
                        $"[StrategicBoardPresenter] Execution {(succeeded ? "succeeded" : "failed")}. " +
                        $"item={strategicSkillItem.strategicSkillItemId} screen={eventData.position}",
                        this);
                }
            }
            finally
            {
                HideTargetingGuide();
                ClearInteractionSelection(slotView);
                activeDragSlot = null;
                dragWarningIssued = false;
                targetTintWarningIssued = false;
            }
        }

        private void ClearInteractionSelection(StrategicSkillSlotView interactionSlot)
        {
            foreach (StrategicSkillSlotView slot in boundSlots)
            {
                if (slot != null)
                {
                    slot.SetSelected(false);
                }
            }

            if (interactionSlot != null)
            {
                interactionSlot.ClearSelectionAfterInteraction();
            }
        }

        private void UpdateTargetingGuide(
            StrategicSkillSlotView slotView,
            PointerEventData eventData,
            bool logCalibration)
        {
            if (slotView == null || eventData == null ||
                slotView.ExecutionPayload is not StrategicSkillItemSO strategicSkillItem)
            {
                HideTargetingGuide();
                WarnForCurrentDrag(
                    "Cannot show the targeting guide because the slot payload is not a StrategicSkillItemSO.");
                return;
            }

            if (strategicSkillItem.skillSo == null || strategicSkillItem.skillSo.BaseProfileSo == null)
            {
                HideTargetingGuide();
                WarnForCurrentDrag(
                    $"Cannot show the targeting guide for slot '{slotView.SlotId}' because skillSo or BaseProfileSo is missing.");
                return;
            }

            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (worldCamera == null)
            {
                HideTargetingGuide();
                WarnForCurrentDrag(
                    "Cannot show the targeting guide because no world camera is available.");
                return;
            }

            if (!EnsureTargetingGuideView())
            {
                HideTargetingGuide();
                WarnForCurrentDrag(
                    "Cannot show the targeting guide because no guide prefab or scene guide view is assigned.");
                return;
            }

            Vector3 screenPoint = new Vector3(
                eventData.position.x,
                eventData.position.y,
                Mathf.Abs(worldCamera.transform.position.z));
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPoint);
            float radius = strategicSkillItem.skillSo.BaseProfileSo.ProjectileColliderRadius;

            if (!targetingGuideView.Show(worldPosition, radius))
            {
                HideTargetingGuide();
                WarnForCurrentDrag(
                    "Cannot show the targeting guide because its SpriteRenderer sprite is missing or has invalid bounds.");
                return;
            }

            int targetLayerMask = BuildTargetLayerMask(strategicSkillItem);
            float overlapRadius = useCalibratedDisplayRadiusForTargetDetection
                ? targetingGuideView.DisplayRadius
                : radius;
            UpdateTargetTintPreview(worldPosition, overlapRadius, targetLayerMask);

            if (logDebug && logCalibration)
            {
                Debug.Log(
                    $"[StrategicBoardPresenter] Targeting guide item={strategicSkillItem.strategicSkillItemId}, " +
                    $"gameplayRadius={targetingGuideView.GameplayRadius:0.###}, " +
                    $"calibratedRadius={targetingGuideView.DisplayRadius:0.###}, " +
                    $"worldDiameter={targetingGuideView.WorldDiameter:0.###}, " +
                    $"overlapRadius={overlapRadius:0.###}, mask={targetLayerMask}, " +
                    $"targetCount={CurrentHighlightedTargetCount}.",
                    this);
            }
        }

        private bool EnsureTargetingGuideView()
        {
            if (targetingGuideView != null)
            {
                return true;
            }

            if (targetingGuidePrefab == null)
            {
                return false;
            }

            targetingGuideView = Instantiate(targetingGuidePrefab, targetingGuideParent);
            targetingGuideView.name = targetingGuidePrefab.name;
            ownsTargetingGuideView = true;
            targetingGuideView.Hide();
            return true;
        }

        private void HideTargetingGuide()
        {
            if (targetingGuideView != null)
            {
                targetingGuideView.Hide();
            }

            ClearTargetTintPreview();
        }

        public void ConfigureTargetTintPreview(
            bool enabled,
            Color tint,
            bool useCalibratedDisplayRadius)
        {
            enableTargetTint = enabled;
            targetTint = tint;
            useCalibratedDisplayRadiusForTargetDetection = useCalibratedDisplayRadius;

            if (!enableTargetTint)
            {
                ClearTargetTintPreview();
                return;
            }

            RefreshHighlightedTargetTint();
        }

        private int BuildTargetLayerMask(StrategicSkillItemSO strategicSkillItem)
        {
            int combinedMask = 0;
            var hitSos = strategicSkillItem?.skillSo?.HitSos;

            if (hitSos != null)
            {
                foreach (var hitSo in hitSos)
                {
                    if (hitSo != null)
                    {
                        combinedMask |= hitSo.TargetLayerMask.value;
                    }
                }
            }

            CurrentTargetLayerMask = combinedMask;
            return combinedMask;
        }

        private void UpdateTargetTintPreview(
            Vector3 worldPosition,
            float overlapRadius,
            int targetLayerMask)
        {
            if (!enableTargetTint)
            {
                ClearTargetTintPreview();
                return;
            }

            if (targetLayerMask == 0 || overlapRadius <= 0f)
            {
                ClearTargetTintPreview();
                WarnForTargetTint(
                    targetLayerMask == 0
                        ? "Cannot preview target tint because the skill has no valid HitSO target layer mask."
                        : "Cannot preview target tint because the selected target radius is not positive.");
                return;
            }

            detectedTargets.Clear();
            targetOverlapResults.Clear();
            ContactFilter2D contactFilter = new()
            {
                useLayerMask = true,
                layerMask = targetLayerMask,
                useTriggers = Physics2D.queriesHitTriggers
            };
            int hitCount = Physics2D.OverlapCircle(
                worldPosition,
                overlapRadius,
                contactFilter,
                targetOverlapResults);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D hitCollider = targetOverlapResults[i];

                if (hitCollider == null)
                {
                    continue;
                }

                CharacterManager characterManager = hitCollider.GetComponentInParent<CharacterManager>();
                if (characterManager == null ||
                    !characterManager.isActiveAndEnabled ||
                    !characterManager.gameObject.activeInHierarchy ||
                    (characterManager.RuntimeData != null && characterManager.RuntimeData.isDead))
                {
                    continue;
                }

                detectedTargets.Add(characterManager);
            }

            targetsToRestore.Clear();
            foreach (CharacterManager highlightedTarget in highlightedTargets.Keys)
            {
                if (highlightedTarget == null || !detectedTargets.Contains(highlightedTarget))
                {
                    targetsToRestore.Add(highlightedTarget);
                }
            }

            foreach (CharacterManager target in targetsToRestore)
            {
                RestoreTargetTint(target);
            }

            foreach (CharacterManager target in detectedTargets)
            {
                if (!highlightedTargets.ContainsKey(target))
                {
                    ApplyTargetTint(target);
                }
            }
        }

        private void ApplyTargetTint(CharacterManager target)
        {
            ShaderMono[] shaderMonos = target.GetComponentsInChildren<ShaderMono>(true);
            List<ShaderTintState> tintStates = new(shaderMonos.Length);

            foreach (ShaderMono shaderMono in shaderMonos)
            {
                if (shaderMono == null || !shaderMono.HasValidRenderer())
                {
                    continue;
                }

                tintStates.Add(new ShaderTintState(shaderMono, shaderMono.GetTint(Color.white)));
                shaderMono.SetTint(targetTint);
                shaderMono.ApplyIfDirty();
            }

            if (tintStates.Count > 0)
            {
                highlightedTargets.Add(target, tintStates);
            }
        }

        private void RestoreTargetTint(CharacterManager target)
        {
            if (!highlightedTargets.TryGetValue(target, out List<ShaderTintState> tintStates))
            {
                return;
            }

            foreach (ShaderTintState tintState in tintStates)
            {
                if (tintState.ShaderMono == null || !tintState.ShaderMono.HasValidRenderer())
                {
                    continue;
                }

                tintState.ShaderMono.SetTint(tintState.OriginalTint);
                tintState.ShaderMono.ApplyIfDirty();
            }

            highlightedTargets.Remove(target);
        }

        private void ClearTargetTintPreview()
        {
            foreach (List<ShaderTintState> tintStates in highlightedTargets.Values)
            {
                foreach (ShaderTintState tintState in tintStates)
                {
                    if (tintState.ShaderMono == null || !tintState.ShaderMono.HasValidRenderer())
                    {
                        continue;
                    }

                    tintState.ShaderMono.SetTint(tintState.OriginalTint);
                    tintState.ShaderMono.ApplyIfDirty();
                }
            }

            highlightedTargets.Clear();
            detectedTargets.Clear();
            targetsToRestore.Clear();
            CurrentTargetLayerMask = 0;
        }

        private void RefreshHighlightedTargetTint()
        {
            foreach (List<ShaderTintState> tintStates in highlightedTargets.Values)
            {
                foreach (ShaderTintState tintState in tintStates)
                {
                    if (tintState.ShaderMono == null || !tintState.ShaderMono.HasValidRenderer())
                    {
                        continue;
                    }

                    tintState.ShaderMono.SetTint(targetTint);
                    tintState.ShaderMono.ApplyIfDirty();
                }
            }
        }

        private void WarnForTargetTint(string message)
        {
            if (targetTintWarningIssued)
            {
                return;
            }

            targetTintWarningIssued = true;
            Debug.LogWarning($"[StrategicBoardPresenter] {message}", this);
        }

        private readonly struct ShaderTintState
        {
            public ShaderTintState(ShaderMono shaderMono, Color originalTint)
            {
                ShaderMono = shaderMono;
                OriginalTint = originalTint;
            }

            public ShaderMono ShaderMono { get; }
            public Color OriginalTint { get; }
        }

        private void WarnForCurrentDrag(string message)
        {
            if (dragWarningIssued)
            {
                return;
            }

            dragWarningIssued = true;
            Debug.LogWarning($"[StrategicBoardPresenter] {message}", this);
        }

        private void HandleGaugeChanged(int currentGauge, int maxGauge)
        {
            if (boardView != null)
            {
                boardView.RefreshSlotResourceStates(currentGauge);
            }
        }

        private void HandleItemChanged(StrategicSkillItemSO _)
        {
            Rebind();
        }
    }
}
