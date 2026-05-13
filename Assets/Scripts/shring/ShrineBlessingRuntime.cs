

using System;
using UnityEngine;

namespace Shrine
{
    /// <summary>
    /// 이번 신전에서 생성된 축복 후보 런타임 데이터.
    /// ShrineBlessingSO는 원본 정의이고, ShrineBlessingRuntime은 이번 신전에서의 슬롯/선택 상태를 가진다.
    /// </summary>
    [Serializable]
    public class ShrineBlessingRuntime
    {
        [Header("Identity")]
        public string runtimeId;
        public int slotIndex;

        [Header("Source")]
        public ShrineBlessingSO blessing;

        [Header("Runtime")]
        public bool isSelected;
        public bool isLocked;

        [Header("Debug")]
        public string generatedFromPoolId;

        public string BlessingId => blessing != null ? blessing.blessingId : string.Empty;
        public string DisplayName => blessing != null ? blessing.DisplayName : "Empty Blessing";
        public string Description => blessing != null ? blessing.description : string.Empty;
        public Sprite Icon => blessing != null ? blessing.icon : null;
        public ShrineBlessingCategory Category => blessing != null ? blessing.category : ShrineBlessingCategory.None;
        public ShrineBlessingEffectType EffectType => blessing != null ? blessing.effectType : ShrineBlessingEffectType.None;
        public float EffectValue => blessing != null ? blessing.effectValue : 0f;
        public float SecondaryValue => blessing != null ? blessing.secondaryValue : 0f;
        public ShrineGodType GodType => blessing != null ? blessing.godType : ShrineGodType.None;

        public bool IsValid => blessing != null;
        public bool CanSelect => IsValid && !isSelected && !isLocked;

        public ShrineBlessingRuntime()
        {
        }

        public ShrineBlessingRuntime(ShrineBlessingSO blessing, int slotIndex, string generatedFromPoolId = null)
        {
            this.blessing = blessing;
            this.slotIndex = slotIndex;
            this.generatedFromPoolId = generatedFromPoolId;
            runtimeId = CreateRuntimeId(blessing, slotIndex);
            isSelected = false;
            isLocked = false;
        }

        public bool TrySelect()
        {
            if (!CanSelect)
            {
                return false;
            }

            Select();
            return true;
        }

        public void Select()
        {
            isSelected = true;
        }

        public void Lock()
        {
            if (isSelected)
            {
                return;
            }

            isLocked = true;
        }

        public void Unlock()
        {
            if (isSelected)
            {
                return;
            }

            isLocked = false;
        }

        public string GetEffectDescription()
        {
            return blessing != null ? blessing.GetEffectDescription() : string.Empty;
        }

        private static string CreateRuntimeId(ShrineBlessingSO blessing, int slotIndex)
        {
            string blessingId = blessing != null && !string.IsNullOrWhiteSpace(blessing.blessingId)
                ? blessing.blessingId
                : "empty";

            return $"shrine_blessing_{slotIndex}_{blessingId}_{Guid.NewGuid().ToString("N")[..8]}";
        }
    }
}