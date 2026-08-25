using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// Root API for the lower battle board. Layout is authored in the prefab;
    /// this component exposes the gauge and slot presentation surface.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class StrategicBoardView : MonoBehaviour
    {
        private const int ExpectedSlotCount = 4;

        [Header("Independent Visual Layers")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image frameImage;

        [Header("Contents")]
        [SerializeField] private StrategicGaugeView gaugeView;
        [SerializeField] private RectTransform slotRoot;
        [SerializeField] private List<StrategicSkillSlotView> slots = new List<StrategicSkillSlotView>();

        private bool slotReferencesPrepared;

        public StrategicGaugeView GaugeView => gaugeView;
        public RectTransform SlotRoot => slotRoot;
        public IReadOnlyList<StrategicSkillSlotView> Slots
        {
            get
            {
                EnsureSlotsReady();
                return slots;
            }
        }
        public int SlotCount
        {
            get
            {
                EnsureSlotsReady();
                return slots.Count;
            }
        }

        private void Awake()
        {
            ResolveRuntimeReferences();
            EnsureSlotsReady();
        }

        private void OnTransformChildrenChanged()
        {
            slotReferencesPrepared = false;
        }

        public bool EnsureSlotsReady()
        {
            if (slotReferencesPrepared)
            {
                return slots != null && slots.Count == ExpectedSlotCount;
            }

            ResolveRuntimeReferences();
            StrategicSkillSlotView[] discoveredSlots = slotRoot != null
                ? slotRoot.GetComponentsInChildren<StrategicSkillSlotView>(true)
                : GetComponentsInChildren<StrategicSkillSlotView>(true);
            Array.Sort(discoveredSlots, CompareSlotHierarchyOrder);

            slots ??= new List<StrategicSkillSlotView>(discoveredSlots.Length);
            slots.Clear();
            var discoveredSet = new HashSet<StrategicSkillSlotView>();

            foreach (StrategicSkillSlotView slot in discoveredSlots)
            {
                if (slot != null && discoveredSet.Add(slot))
                {
                    slots.Add(slot);

                    if (slots.Count == ExpectedSlotCount)
                    {
                        break;
                    }
                }
            }

            NormalizeSlotIds();
            slotReferencesPrepared = true;
            return slots.Count == ExpectedSlotCount;
        }

        public void SetGauge(int current, int max)
        {
            if (gaugeView != null)
            {
                gaugeView.SetGauge(current, max);
            }
        }

        public void SetChargePerSecond(float amount)
        {
            if (gaugeView != null)
            {
                gaugeView.SetChargePerSecond(amount);
            }
        }

        public void RefreshSlotResourceStates(int currentGauge)
        {
            EnsureSlotsReady();

            for (int i = 0; i < slots.Count; i++)
            {
                StrategicSkillSlotView slot = slots[i];

                if (slot != null)
                {
                    slot.SetResourceGauge(currentGauge);
                }
            }
        }

        public StrategicSkillSlotView GetSlot(string slotId)
        {
            EnsureSlotsReady();

            for (int i = 0; i < slots.Count; i++)
            {
                StrategicSkillSlotView slot = slots[i];

                if (slot != null && slot.SlotId == slotId)
                {
                    return slot;
                }
            }

            return null;
        }

        private void ResolveRuntimeReferences()
        {
            if (gaugeView == null)
            {
                gaugeView = GetComponentInChildren<StrategicGaugeView>(true);
            }

            if (slotRoot != null)
            {
                return;
            }

            StrategicSkillSlotView firstSlot = GetComponentInChildren<StrategicSkillSlotView>(true);
            if (firstSlot != null)
            {
                slotRoot = firstSlot.transform.parent as RectTransform;
            }
        }

        private void NormalizeSlotIds()
        {
            var usedIds = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < slots.Count; i++)
            {
                StrategicSkillSlotView slot = slots[i];
                string authoredId = slot.SlotId;

                if (!string.IsNullOrWhiteSpace(authoredId) && usedIds.Add(authoredId))
                {
                    continue;
                }

                string normalizedId = GetAvailableSlotId(i, usedIds);
                slot.SetSlotId(normalizedId);
                usedIds.Add(normalizedId);
            }
        }

        private string GetAvailableSlotId(int preferredIndex, HashSet<string> usedIds)
        {
            string preferredId = $"strategic-slot-{preferredIndex + 1}";
            if (!usedIds.Contains(preferredId))
            {
                return preferredId;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                string candidate = $"strategic-slot-{i + 1}";
                if (!usedIds.Contains(candidate))
                {
                    return candidate;
                }
            }

            int suffix = slots.Count + 1;
            string fallback;
            do
            {
                fallback = $"strategic-slot-{suffix++}";
            }
            while (usedIds.Contains(fallback));

            return fallback;
        }

        private static int CompareSlotHierarchyOrder(
            StrategicSkillSlotView left,
            StrategicSkillSlotView right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            Transform leftTransform = left.transform;
            Transform rightTransform = right.transform;

            if (leftTransform.parent == rightTransform.parent)
            {
                return leftTransform.GetSiblingIndex().CompareTo(rightTransform.GetSiblingIndex());
            }

            return string.CompareOrdinal(
                GetHierarchyOrderKey(leftTransform),
                GetHierarchyOrderKey(rightTransform));
        }

        private static string GetHierarchyOrderKey(Transform transform)
        {
            string key = string.Empty;

            while (transform != null)
            {
                key = $"/{transform.GetSiblingIndex():D6}{key}";
                transform = transform.parent;
            }

            return key;
        }
    }
}
