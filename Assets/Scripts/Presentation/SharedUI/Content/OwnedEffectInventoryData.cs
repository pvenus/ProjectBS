using System;
using System.Collections.Generic;

namespace Presentation
{
    public enum OwnedEffectInventoryCategory
    {
        All = 0,
        Relic = 100,
        GeneralBless = 200,
        FaithBless = 300,
    }

    [Serializable]
    public sealed class OwnedEffectInventoryItemData
    {
        public string ItemId { get; }
        public OwnedEffectInventoryCategory Category { get; }
        public ContentPresentationData Content { get; }

        public OwnedEffectInventoryItemData(
            string itemId,
            OwnedEffectInventoryCategory category,
            ContentPresentationData content)
        {
            ItemId = itemId ?? string.Empty;
            Category = category;
            Content = content;
        }
    }

    [Serializable]
    public sealed class OwnedEffectInventoryData
    {
        private readonly OwnedEffectInventoryItemData[] items;

        public IReadOnlyList<OwnedEffectInventoryItemData> Items => items;

        public OwnedEffectInventoryData(
            IReadOnlyList<OwnedEffectInventoryItemData> items)
        {
            if (items == null || items.Count == 0)
            {
                this.items = Array.Empty<OwnedEffectInventoryItemData>();
                return;
            }

            this.items = new OwnedEffectInventoryItemData[items.Count];
            for (int index = 0; index < items.Count; index++)
            {
                this.items[index] = items[index];
            }
        }
    }
}
