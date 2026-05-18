using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Bless
{
    [Serializable]
    public class BlessRuntimeData
    {
        [Header("Inventory")]
        [SerializeField]
        private List<BlessEntry> blessings = new();

        public IReadOnlyList<BlessEntry> Blessings
            => blessings;

        [Serializable]
        public class BlessEntry
        {
            [Header("Source")]
            public BlessSO source;

            [Header("Runtime")]
            public string runtimeId;

            public int level = 1;

            public bool isLocked;

            public bool isEquipped;

            public bool isSelected;


            public int remainingBattleCount = -1;

            public float runtimeValue;

            public bool isTemporary;

            public int slotIndex = -1;

            [Header("Generation")]
            public string generatedFromPoolId;

            public BlessEntry(
                BlessSO source,
                string generatedFromPoolId = null,
                int slotIndex = -1)
            {
                this.source = source;
                this.generatedFromPoolId = generatedFromPoolId;
                this.slotIndex = slotIndex;

                runtimeId = CreateRuntimeId(source, slotIndex);

                if (source != null)
                {
                    runtimeValue = source.effectValue;

                    isTemporary =
                        source.durationType
                        != BlessDurationType.Permanent;

                    remainingBattleCount =
                        source.durationBattleCount;
                }
            }

            public string DisplayName
                => source != null
                    ? source.blessingName
                    : "Unknown Bless";

            public string Description
                => source != null
                    ? source.description
                    : string.Empty;

            public Sprite Icon
                => source != null
                    ? source.icon
                    : null;

            public BlessEffectType EffectType
                => source != null
                    ? source.effectType
                    : BlessEffectType.None;

            public BlessCategory Category
                => source != null
                    ? source.category
                    : BlessCategory.None;

            public bool CanSelect
                => !isLocked;

            public bool TrySelect()
            {
                if (!CanSelect)
                {
                    return false;
                }

                isSelected = true;
                return true;
            }

            public void Deselect()
            {
                isSelected = false;
            }


            public void SetLevel(
                int newLevel)
            {
                level = Mathf.Max(1, newLevel);
            }

            public void SetRuntimeValue(
                float value)
            {
                runtimeValue = value;
            }

            public bool ConsumeBattle()
            {
                if (!isTemporary)
                {
                    return false;
                }

                if (remainingBattleCount < 0)
                {
                    return false;
                }

                remainingBattleCount--;

                return remainingBattleCount <= 0;
            }

            public bool IsExpired()
            {
                return isTemporary
                    && remainingBattleCount <= 0;
            }

            public override string ToString()
            {
                return source != null
                    ? $"{source.name} Lv.{level}"
                    : "Null Bless";
            }

            private static string CreateRuntimeId(
                BlessSO source,
                int slotIndex)
            {
                string blessId =
                    source != null
                        ? source.name
                        : "null";

                return $"{blessId}_{slotIndex}_{Guid.NewGuid():N}";
            }
        }

        public IReadOnlyList<BlessEntry> GetBlessings()
        {
            return blessings;
        }

        public void RemoveBlesses(
            Predicate<BlessEntry> match)
        {
            blessings.RemoveAll(match);
        }

        public BlessEntry AddBless(
            BlessSO source,
            string generatedFromPoolId = null,
            int slotIndex = -1)
        {
            if (source == null)
            {
                return null;
            }

            BlessEntry existing =
                blessings.FirstOrDefault(
                    x => x != null
                         && x.source != null
                         && x.source.groupId == source.groupId
                         && !string.IsNullOrWhiteSpace(source.groupId));

            if (existing != null)
            {
                blessings.Remove(existing);
            }

            BlessEntry entry =
                new BlessEntry(
                    source,
                    generatedFromPoolId,
                    slotIndex);

            blessings.Add(entry);

            return entry;
        }

        public bool RemoveBless(
            string runtimeId)
        {
            BlessEntry entry = GetBless(runtimeId);

            if (entry == null)
            {
                return false;
            }

            blessings.Remove(entry);
            return true;
        }

        public BlessEntry GetBless(
            string runtimeId)
        {
            return blessings.FirstOrDefault(
                x => x != null
                     && x.runtimeId == runtimeId);
        }

        public bool HasBless(
            BlessSO source)
        {
            return blessings.Any(
                x => x != null
                     && x.source == source);
        }

        public void ConsumeBattleBlessings()
        {
            List<BlessEntry> expired = new();

            foreach (BlessEntry entry in blessings)
            {
                if (entry == null)
                {
                    continue;
                }

                entry.ConsumeBattle();

                if (entry.IsExpired())
                {
                    expired.Add(entry);
                }
            }

            foreach (BlessEntry entry in expired)
            {
                blessings.Remove(entry);
            }
        }

        public void ClearSelection()
        {
            foreach (BlessEntry entry in blessings)
            {
                if (entry == null)
                {
                    continue;
                }

                entry.Deselect();
            }
        }
    }
}