using System.Collections.Generic;
using Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [AutoBindPrefix("UI")]
    public sealed class OwnedEffectInventoryView : UIView
    {
        [Header("Page")]
        [SerializeField] private TMP_Text titleText;

        [AutoBind]
        [SerializeField] private ScrollRect scrollRect;

        [AutoBind]
        [SerializeField] private RectTransform categoryRoot;

        [SerializeField] private GameObject categoryPrefab;

        [Header("Detail")]
        [SerializeField] private UIContentInfoView contentInfoView;

        private readonly List<ContentInventoryCategoryView> spawnedCategories = new();
        private ContentInventoryItemView selectedItemView;

        public IReadOnlyList<ContentInventoryCategoryView> SpawnedCategories =>
            spawnedCategories;

        public void ShowInventory(ContentInventoryPageData data)
        {
            base.Show();
            ClearSelection();
            ClearCategories();

            if (titleText != null)
            {
                titleText.text = PresentationLocalizedTextResolver.ResolveLabel(
                    "presentation.inventory.owned_effects");
            }

            if (categoryRoot == null || categoryPrefab == null)
            {
                Debug.LogError(
                    "[OwnedEffectInventoryView] Category root or category prefab is not assigned.",
                    this);
                return;
            }

            IReadOnlyList<ContentInventoryCategoryData> categories = data?.Categories;
            if (categories != null)
            {
                for (int index = 0; index < categories.Count; index++)
                {
                    ContentInventoryCategoryData category = categories[index];
                    if (category?.Items == null || category.Items.Count == 0)
                    {
                        continue;
                    }

                    GameObject instance = Instantiate(categoryPrefab, categoryRoot);
                    ContentInventoryCategoryView categoryView =
                        instance.GetComponent<ContentInventoryCategoryView>();
                    if (categoryView == null)
                    {
                        Debug.LogError(
                            "[OwnedEffectInventoryView] ContentInventoryCategoryView is missing from the category prefab.",
                            instance);
                        instance.SetActive(false);
                        Destroy(instance);
                        continue;
                    }

                    categoryView.Bind(category, SelectItem);
                    spawnedCategories.Add(categoryView);
                }
            }

            RefreshLayout();
        }

        public void ClearSelection()
        {
            selectedItemView?.SetSelected(false);
            selectedItemView = null;

            if (contentInfoView != null)
            {
                contentInfoView.gameObject.SetActive(true);
                contentInfoView.SetFormatter(
                    PresentationTextFormatter.CreatePlayerFormatter(
                        PresentationLocalizedTextResolver.ResolveLabel));
                contentInfoView.Bind(null);
            }
        }

        public override void Clear()
        {
            ClearSelection();
            ClearCategories();
        }

        private void SelectItem(ContentInventoryItemView itemView)
        {
            if (itemView?.Item?.Content == null)
            {
                return;
            }

            selectedItemView?.SetSelected(false);
            selectedItemView = itemView;
            selectedItemView.SetSelected(true);

            if (contentInfoView == null)
            {
                Debug.LogError(
                    "[OwnedEffectInventoryView] ContentInfoView is not assigned.",
                    this);
                return;
            }

            contentInfoView.gameObject.SetActive(true);
            contentInfoView.SetFormatter(
                PresentationTextFormatter.CreatePlayerFormatter(
                    PresentationLocalizedTextResolver.ResolveLabel));
            contentInfoView.Bind(itemView.Item.Content);
        }

        private void ClearCategories()
        {
            if (categoryRoot != null)
            {
                for (int index = categoryRoot.childCount - 1; index >= 0; index--)
                {
                    GameObject child = categoryRoot.GetChild(index).gameObject;
                    child.SetActive(false);
                    Destroy(child);
                }
            }

            spawnedCategories.Clear();
        }

        private void RefreshLayout()
        {
            if (categoryRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(categoryRoot);
            }

            Canvas.ForceUpdateCanvases();

            if (scrollRect == null)
            {
                return;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                scrollRect.transform as RectTransform);
            Canvas.ForceUpdateCanvases();
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
