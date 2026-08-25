using System;
using System.Collections.Generic;

namespace Presentation
{
    public enum ContentInventoryDisplayMode
    {
        OwnedOnly = 0,
        Catalog = 100,
    }

    public enum ContentAcquisitionState
    {
        Owned = 0,
        Unowned = 100,
        Locked = 200,
    }

    public enum ContentActivationState
    {
        Inactive = 0,
        Active = 100,
    }

    [Serializable]
    public sealed class ContentInventoryItemData
    {
        public string ItemId { get; }
        public ContentAcquisitionState AcquisitionState { get; }
        public ContentActivationState ActivationState { get; }
        public ContentPresentationData Content { get; }

        public ContentInventoryItemData(
            string itemId,
            ContentAcquisitionState acquisitionState,
            ContentActivationState activationState,
            ContentPresentationData content)
        {
            ItemId = itemId ?? string.Empty;
            AcquisitionState = acquisitionState;
            ActivationState = activationState;
            Content = content;
        }
    }

    [Serializable]
    public sealed class ContentInventoryCategoryData
    {
        private readonly ContentInventoryItemData[] items;

        public string CategoryId { get; }
        public string TitleLocalizationKey { get; }
        public ContentInventoryDisplayMode DisplayMode { get; }
        public IReadOnlyList<ContentInventoryItemData> Items => items;

        public ContentInventoryCategoryData(
            string categoryId,
            string titleLocalizationKey,
            ContentInventoryDisplayMode displayMode,
            IReadOnlyList<ContentInventoryItemData> items)
        {
            CategoryId = categoryId ?? string.Empty;
            TitleLocalizationKey = titleLocalizationKey ?? string.Empty;
            DisplayMode = displayMode;
            this.items = CopyItems(items);
        }

        private static ContentInventoryItemData[] CopyItems(
            IReadOnlyList<ContentInventoryItemData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<ContentInventoryItemData>();
            }

            ContentInventoryItemData[] result =
                new ContentInventoryItemData[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }

    [Serializable]
    public sealed class ContentInventoryPageData
    {
        private readonly ContentInventoryCategoryData[] categories;

        public IReadOnlyList<ContentInventoryCategoryData> Categories => categories;

        public ContentInventoryPageData(
            IReadOnlyList<ContentInventoryCategoryData> categories)
        {
            if (categories == null || categories.Count == 0)
            {
                this.categories = Array.Empty<ContentInventoryCategoryData>();
                return;
            }

            this.categories = new ContentInventoryCategoryData[categories.Count];
            for (int index = 0; index < categories.Count; index++)
            {
                this.categories[index] = categories[index];
            }
        }
    }
}
