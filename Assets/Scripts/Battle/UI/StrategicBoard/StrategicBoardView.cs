using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.UI.StrategicBoard
{
    /// <summary>
    /// Root API for the lower battle board. Layout is authored in the prefab;
    /// this component exposes the gauge and slot presentation surface.
    /// </summary>
    public sealed class StrategicBoardView : MonoBehaviour
    {
        [Header("Independent Visual Layers")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image frameImage;

        [Header("Contents")]
        [SerializeField] private StrategicGaugeView gaugeView;
        [SerializeField] private RectTransform slotRoot;
        [SerializeField] private List<StrategicSkillSlotView> slots = new List<StrategicSkillSlotView>();

        public StrategicGaugeView GaugeView => gaugeView;
        public RectTransform SlotRoot => slotRoot;
        public IReadOnlyList<StrategicSkillSlotView> Slots => slots;

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
            for (int i = 0; i < slots.Count; i++)
            {
                StrategicSkillSlotView slot = slots[i];

                if (slot != null)
                {
                    slot.SetResourceAvailable(currentGauge >= slot.Cost);
                }
            }
        }

        public StrategicSkillSlotView GetSlot(string slotId)
        {
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
    }
}
