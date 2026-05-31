using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [Serializable]
    public class StrategicSkillItemRuntimeData
    {
        [Serializable]
        public class StrategicSkillItemEntry
        {
            [SerializeField] private StrategicSkillItemSO strategicSkillItem;
            [SerializeField] private bool owned;

            public StrategicSkillItemSO StrategicSkillItem => strategicSkillItem;
            public bool Owned => owned;

            public StrategicSkillItemEntry(StrategicSkillItemSO strategicSkillItem)
            {
                this.strategicSkillItem = strategicSkillItem;
                owned = strategicSkillItem != null;
            }

            public bool CanUseInBattle()
            {
                if (!owned)
                {
                    return false;
                }

                if (strategicSkillItem == null)
                {
                    return false;
                }

                return strategicSkillItem.reusable;
            }

            public bool UseInBattle()
            {
                return CanUseInBattle();
            }

            public void ResetBattleUsage()
            {
            }

            public void SetOwned(bool value)
            {
                owned = value;
            }
        }

        [Header("Strategic Skill Item Inventory")]
        [SerializeField]
        private List<StrategicSkillItemEntry> strategicSkillItems = new();

        public IReadOnlyList<StrategicSkillItemEntry> StrategicSkillItems
            => strategicSkillItems;

        public bool AddStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            if (strategicSkillItem == null)
            {
                return false;
            }

            StrategicSkillItemEntry exists =
                FindStrategicSkillItem(strategicSkillItem);

            if (exists != null)
            {
                exists.SetOwned(true);
                return true;
            }

            strategicSkillItems.Add(new StrategicSkillItemEntry(strategicSkillItem));
            return true;
        }

        public bool RemoveStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            StrategicSkillItemEntry entry =
                FindStrategicSkillItem(strategicSkillItem);

            if (entry == null)
            {
                return false;
            }

            return strategicSkillItems.Remove(entry);
        }

        public bool HasStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            return FindStrategicSkillItem(strategicSkillItem) != null;
        }

        public StrategicSkillItemEntry FindStrategicSkillItem(StrategicSkillItemSO strategicSkillItem)
        {
            if (strategicSkillItem == null)
            {
                return null;
            }

            return strategicSkillItems.Find(x => x != null
                && x.StrategicSkillItem == strategicSkillItem);
        }

        public bool UseStrategicSkillItemInBattle(StrategicSkillItemSO strategicSkillItem)
        {
            StrategicSkillItemEntry entry =
                FindStrategicSkillItem(strategicSkillItem);

            if (entry == null)
            {
                return false;
            }

            return entry.UseInBattle();
        }

        public void ResetBattleUsage()
        {
            foreach (StrategicSkillItemEntry entry in strategicSkillItems)
            {
                if (entry == null)
                {
                    continue;
                }

                entry.ResetBattleUsage();
            }
        }

        public void Clear()
        {
            strategicSkillItems.Clear();
        }
    }
}