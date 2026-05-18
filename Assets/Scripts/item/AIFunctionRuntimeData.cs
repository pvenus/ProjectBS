

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    [Serializable]
    public class AIFunctionRuntimeData
    {
        [Serializable]
        public class AIFunctionEntry
        {
            [SerializeField] private AIFunctionSO function;

            [SerializeField] private bool owned;

            [SerializeField] private bool equipped;

            public AIFunctionSO Function => function;

            public bool Owned => owned;

            public bool Equipped => equipped;

            public AIFunctionEntry(
                AIFunctionSO function)
            {
                this.function = function;
                owned = function != null;
                equipped = false;
            }

            public void SetOwned(bool value)
            {
                owned = value;
            }

            public void SetEquipped(bool value)
            {
                equipped = value;
            }
        }

        [Header("AI Function Inventory")]
        [SerializeField]
        private List<AIFunctionEntry> functions = new();

        public IReadOnlyList<AIFunctionEntry> Functions
            => functions;

        public bool AddFunction(AIFunctionSO function)
        {
            if (function == null)
            {
                return false;
            }

            AIFunctionEntry exists =
                FindFunction(function);

            if (exists != null)
            {
                exists.SetOwned(true);
                return true;
            }

            functions.Add(new AIFunctionEntry(function));
            return true;
        }

        public bool RemoveFunction(AIFunctionSO function)
        {
            AIFunctionEntry entry =
                FindFunction(function);

            if (entry == null)
            {
                return false;
            }

            return functions.Remove(entry);
        }

        public bool HasFunction(AIFunctionSO function)
        {
            return FindFunction(function) != null;
        }

        public AIFunctionEntry FindFunction(
            AIFunctionSO function)
        {
            if (function == null)
            {
                return null;
            }

            return functions.Find(x => x != null
                && x.Function == function);
        }

        public bool EquipFunction(AIFunctionSO function)
        {
            AIFunctionEntry entry =
                FindFunction(function);

            if (entry == null)
            {
                return false;
            }

            if (entry.Equipped)
            {
                return true;
            }

            int equippedCount =
                GetEquippedCount(function);

            if (equippedCount >= function.maxEquipCount)
            {
                return false;
            }

            if (!function.allowDuplicateEquip)
            {
                bool alreadyEquipped =
                    functions.Exists(x => x != null
                        && x.Equipped
                        && x.Function == function);

                if (alreadyEquipped)
                {
                    return false;
                }
            }

            entry.SetEquipped(true);
            return true;
        }

        public bool UnequipFunction(AIFunctionSO function)
        {
            AIFunctionEntry entry =
                FindFunction(function);

            if (entry == null)
            {
                return false;
            }

            entry.SetEquipped(false);
            return true;
        }

        public int GetEquippedCount(AIFunctionSO function)
        {
            if (function == null)
            {
                return 0;
            }

            int count = 0;

            foreach (AIFunctionEntry entry in functions)
            {
                if (entry == null)
                {
                    continue;
                }

                if (!entry.Equipped)
                {
                    continue;
                }

                if (entry.Function != function)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        public void Clear()
        {
            functions.Clear();
        }
    }
}