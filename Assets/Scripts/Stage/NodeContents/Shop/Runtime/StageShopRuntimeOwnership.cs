using System;
using System.Collections.Generic;
using Shop;

namespace Stage
{
    [Serializable]
    public sealed class StageShopRuntimeOwnership
    {
        private readonly Dictionary<string, ShopRuntimeData> stocks = new();
        private readonly HashSet<string> completedNodes = new();

        public bool TryGetStock(string key, out ShopRuntimeData stock) =>
            stocks.TryGetValue(key ?? string.Empty, out stock);

        public bool TryStoreStock(string key, ShopRuntimeData stock)
        {
            if (string.IsNullOrWhiteSpace(key) || stock == null) return false;
            if (stocks.TryGetValue(key, out ShopRuntimeData existing)) return ReferenceEquals(existing, stock);
            stocks.Add(key, stock);
            return true;
        }

        public bool TryComplete(string key) =>
            !string.IsNullOrWhiteSpace(key) && completedNodes.Add(key);

        public bool IsComplete(string key) =>
            !string.IsNullOrWhiteSpace(key) && completedNodes.Contains(key);

        public void Reset()
        {
            stocks.Clear();
            completedNodes.Clear();
        }
    }
}
