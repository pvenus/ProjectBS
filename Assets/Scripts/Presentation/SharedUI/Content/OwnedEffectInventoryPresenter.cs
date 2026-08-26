using System.Collections.Generic;
using Bless;
using Item;
using Presentation;
using UnityEngine;

namespace UI
{
    public sealed class OwnedEffectInventoryPresenter : UIComponent
    {
        private const string RelicCategoryId = "relic";
        private const string GeneralBlessCategoryId = "general_bless";
        private const string FaithBlessCategoryId = "faith_bless";

        [Header("View")]
        [SerializeField] private OwnedEffectInventoryView inventoryView;

        [Header("Configured Preview Sources")]
        [SerializeField] private List<RelicSO> relics = new();
        [SerializeField] private List<BlessSO> generalBlesses = new();
        [SerializeField] private List<BlessSO> faithBlesses = new();

        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;

        private readonly RelicPresentationResolver relicResolver = new();
        private readonly BlessPresentationResolver blessResolver = new();
        private bool hasExplicitlyBuilt;

        private void Start()
        {
            if (buildOnStart && !hasExplicitlyBuilt)
            {
                BuildConfiguredInventory();
            }
        }

        [ContextMenu("Build Configured Owned Effects")]
        public void BuildConfiguredInventory()
        {
            if (!CanBuild())
            {
                return;
            }

            hasExplicitlyBuilt = true;

            List<ContentInventoryCategoryData> categories = new();
            AddCategory(
                categories,
                RelicCategoryId,
                "presentation.inventory.category.relic",
                BuildRelicDefinitions(relics));
            AddCategory(
                categories,
                GeneralBlessCategoryId,
                "presentation.inventory.category.general_bless",
                BuildBlessDefinitions(generalBlesses, GeneralBlessCategoryId));
            AddCategory(
                categories,
                FaithBlessCategoryId,
                "presentation.inventory.category.faith_bless",
                BuildBlessDefinitions(faithBlesses, FaithBlessCategoryId));

            inventoryView.ShowInventory(new ContentInventoryPageData(categories));
        }

        public void ShowOwnedEffects(
            IReadOnlyList<RelicSO> ownedRelics,
            IReadOnlyList<BlessRuntimeData.BlessEntry> activeGeneralBlesses,
            IReadOnlyList<BlessRuntimeData.BlessEntry> activeFaithBlesses)
        {
            if (!CanBuild())
            {
                return;
            }

            hasExplicitlyBuilt = true;

            List<ContentInventoryCategoryData> categories = new();
            AddCategory(
                categories,
                RelicCategoryId,
                "presentation.inventory.category.relic",
                BuildRelicDefinitions(ownedRelics));
            AddCategory(
                categories,
                GeneralBlessCategoryId,
                "presentation.inventory.category.general_bless",
                BuildBlessRuntimeEntries(activeGeneralBlesses, GeneralBlessCategoryId));
            AddCategory(
                categories,
                FaithBlessCategoryId,
                "presentation.inventory.category.faith_bless",
                BuildBlessRuntimeEntries(activeFaithBlesses, FaithBlessCategoryId));

            inventoryView.ShowInventory(new ContentInventoryPageData(categories));
        }

        private bool CanBuild()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning(
                    "[OwnedEffectInventoryPresenter] Enter Play Mode before building owned effects.",
                    this);
                return false;
            }

            if (inventoryView == null)
            {
                inventoryView = GetComponent<OwnedEffectInventoryView>() ??
                                GetComponentInChildren<OwnedEffectInventoryView>(true);
            }

            if (inventoryView == null)
            {
                Debug.LogError(
                    "[OwnedEffectInventoryPresenter] OwnedEffectInventoryView is not assigned.",
                    this);
                return false;
            }

            return true;
        }

        private List<ContentInventoryItemData> BuildRelicDefinitions(
            IReadOnlyList<RelicSO> sources)
        {
            List<ContentInventoryItemData> items = new();
            if (sources == null)
            {
                return items;
            }

            for (int index = 0; index < sources.Count; index++)
            {
                RelicSO relic = sources[index];
                if (relic == null)
                {
                    continue;
                }

                ContentPresentationData content =
                    relicResolver.ResolveForPlayerDisplay(
                        relic,
                        PresentationContext.Preview);
                items.Add(new ContentInventoryItemData(
                    $"relic:{relic.relicId}",
                    ContentAcquisitionState.Owned,
                    ContentActivationState.Active,
                    content));
            }

            return items;
        }

        private List<ContentInventoryItemData> BuildBlessDefinitions(
            IReadOnlyList<BlessSO> sources,
            string categoryId)
        {
            List<ContentInventoryItemData> items = new();
            if (sources == null)
            {
                return items;
            }

            for (int index = 0; index < sources.Count; index++)
            {
                BlessSO bless = sources[index];
                if (bless == null)
                {
                    continue;
                }

                ContentPresentationData content =
                    blessResolver.ResolveForPlayerDisplay(
                        bless,
                        PresentationContext.Preview);
                items.Add(new ContentInventoryItemData(
                    $"bless:{categoryId}:{bless.BlessingId}:{index}",
                    ContentAcquisitionState.Owned,
                    ContentActivationState.Active,
                    content));
            }

            return items;
        }

        private List<ContentInventoryItemData> BuildBlessRuntimeEntries(
            IReadOnlyList<BlessRuntimeData.BlessEntry> sources,
            string categoryId)
        {
            List<ContentInventoryItemData> items = new();
            if (sources == null)
            {
                return items;
            }

            for (int index = 0; index < sources.Count; index++)
            {
                BlessRuntimeData.BlessEntry bless = sources[index];
                if (bless?.source == null)
                {
                    continue;
                }

                ContentPresentationData content =
                    blessResolver.ResolveForPlayerDisplay(
                        bless,
                        PresentationContext.Runtime);
                items.Add(new ContentInventoryItemData(
                    $"bless:{categoryId}:{bless.runtimeId}",
                    ContentAcquisitionState.Owned,
                    ContentActivationState.Active,
                    content));
            }

            return items;
        }

        private static void AddCategory(
            ICollection<ContentInventoryCategoryData> target,
            string categoryId,
            string titleLocalizationKey,
            IReadOnlyList<ContentInventoryItemData> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            target.Add(new ContentInventoryCategoryData(
                categoryId,
                titleLocalizationKey,
                ContentInventoryDisplayMode.OwnedOnly,
                items));
        }
    }
}
