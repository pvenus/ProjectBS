

using System;
using System.Collections.Generic;

namespace Item.Service
{
    public class AIFunctionItemService
    {
        private readonly List<AIFunctionSO> ownedFunctions = new();

        public event Action<AIFunctionSO> OnAIFunctionAdded;
        public event Action<AIFunctionSO> OnAIFunctionRemoved;
        public event Action OnAIFunctionsChanged;

        public IReadOnlyList<AIFunctionSO> OwnedFunctions => ownedFunctions;

        public bool Add(AIFunctionSO function)
        {
            if (function == null)
                return false;

            if (ownedFunctions.Contains(function))
                return false;

            ownedFunctions.Add(function);
            OnAIFunctionAdded?.Invoke(function);
            OnAIFunctionsChanged?.Invoke();
            return true;
        }

        public bool Remove(AIFunctionSO function)
        {
            if (function == null)
                return false;

            if (!ownedFunctions.Remove(function))
                return false;

            OnAIFunctionRemoved?.Invoke(function);
            OnAIFunctionsChanged?.Invoke();
            return true;
        }

        public bool Has(AIFunctionSO function)
        {
            return function != null
                   && ownedFunctions.Contains(function);
        }

        public bool Has(string functionId)
        {
            if (string.IsNullOrWhiteSpace(functionId))
                return false;

            for (int i = 0; i < ownedFunctions.Count; i++)
            {
                AIFunctionSO function = ownedFunctions[i];
                if (function != null && function.functionId == functionId)
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            if (ownedFunctions.Count <= 0)
                return;

            ownedFunctions.Clear();
            OnAIFunctionsChanged?.Invoke();
        }
    }
}