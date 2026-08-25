using System;
using System.Collections.Generic;
using Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [AutoBindPrefix("UI")]
    public sealed class ContentInventoryCategoryView : UIComponent
    {
        [Header("Header")]
        [AutoBind("TitleText")]
        [SerializeField] private TMP_Text categoryTitleText;

        [AutoBind("CountText")]
        [SerializeField] private TMP_Text categoryCountText;

        [Header("Items")]
        [AutoBind("InventoryRoot")]
        [SerializeField] private RectTransform itemRoot;

        [SerializeField] private GameObject itemPrefab;

        private readonly List<ContentInventoryItemView> spawnedItems = new();

        public ContentInventoryCategoryData Category { get; private set; }
        public IReadOnlyList<ContentInventoryItemView> SpawnedItems => spawnedItems;

        public void Bind(
            ContentInventoryCategoryData category,
            Action<ContentInventoryItemView> onItemSelected)
        {
            Category = category;
            ClearItems();

            if (categoryTitleText != null)
            {
                categoryTitleText.text =
                    PresentationLocalizedTextResolver.ResolveLabel(
                        category?.TitleLocalizationKey);
            }

            IReadOnlyList<ContentInventoryItemData> items = category?.Items;
            int itemCount = items?.Count ?? 0;

            if (categoryCountText != null)
            {
                categoryCountText.text = itemCount.ToString();
            }

            if (itemCount == 0)
            {
                RefreshLayout();
                return;
            }

            if (itemRoot == null || itemPrefab == null)
            {
                Debug.LogError(
                    "[ContentInventoryCategoryView] Item root or item prefab is not assigned.",
                    this);
                return;
            }

            for (int index = 0; index < items.Count; index++)
            {
                ContentInventoryItemData item = items[index];
                if (item == null)
                {
                    continue;
                }

                GameObject instance = Instantiate(itemPrefab, itemRoot);
                ContentInventoryItemView itemView =
                    instance.GetComponent<ContentInventoryItemView>();
                if (itemView == null)
                {
                    Debug.LogError(
                        "[ContentInventoryCategoryView] ContentInventoryItemView is missing from the item prefab.",
                        instance);
                    instance.SetActive(false);
                    Destroy(instance);
                    continue;
                }

                itemView.Bind(item, onItemSelected);
                spawnedItems.Add(itemView);
            }

            RefreshLayout();
        }

        public void ClearItems()
        {
            if (itemRoot != null)
            {
                for (int index = itemRoot.childCount - 1; index >= 0; index--)
                {
                    GameObject child = itemRoot.GetChild(index).gameObject;
                    child.SetActive(false);
                    Destroy(child);
                }
            }

            spawnedItems.Clear();
        }

        private void RefreshLayout()
        {
            if (itemRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(itemRoot);
            }

            RectTransform ownRect = transform as RectTransform;
            if (ownRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(ownRect);
            }
        }
    }
}
