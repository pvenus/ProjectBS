using System.Collections.Generic;
using UnityEngine;

namespace Shop
{
    /// <summary>
    /// Provides inspector-driven entry points for testing the fixed shop UI.
    /// Runtime product generation and purchase integration are added incrementally.
    /// </summary>
    public class ShopTestController : AutoBindBehaviour
    {
        public const int ProductCountPerCategory = 3;
        public const int DefaultFallbackPrice = 100;

        [Header("Manager")]
        [SerializeField] private StageShopManager shopManager;

        [Header("SO Pools")]
        [SerializeField] private ShopItemPoolSO relicPool;
        [SerializeField] private ShopItemPoolSO strategicSkillPool;

        [Header("Test Settings")]
        [SerializeField, Min(1)] private int goldIncreaseAmount = 1000;
        [SerializeField, Min(1)] private int fallbackPrice = DefaultFallbackPrice;

        [ContextMenu("Generate Test Shop")]
        public void GenerateTestShop()
        {
            ResolveShopManager();

            if (shopManager == null)
            {
                Debug.LogWarning(
                    $"[{nameof(ShopTestController)}] Test shop generation failed because " +
                    $"{nameof(StageShopManager)} is missing.",
                    this);
                return;
            }

            if (relicPool == null || strategicSkillPool == null)
            {
                Debug.LogWarning(
                    $"[{nameof(ShopTestController)}] Test shop generation failed because " +
                    "both ShopItemPoolSO references must be assigned.",
                    this);
                return;
            }

            List<ShopItemPoolSO> pools = new()
            {
                relicPool,
                strategicSkillPool,
            };

            shopManager.OpenShop(
                pools,
                ProductCountPerCategory * pools.Count,
                ShopType.Normal,
                fallbackPrice);
        }

        [ContextMenu("Add Test Gold")]
        public void AddTestGold()
        {
            ResolveShopManager();

            if (shopManager == null)
            {
                Debug.LogWarning(
                    $"[{nameof(ShopTestController)}] Test gold could not be added because " +
                    $"{nameof(StageShopManager)} is missing.",
                    this);
                return;
            }

            shopManager.AddGold(goldIncreaseAmount);
        }

        private void ResolveShopManager()
        {
            if (shopManager != null)
            {
                return;
            }

            shopManager = StageShopManager.Instance;
            if (shopManager == null)
            {
                shopManager = FindFirstObjectByType<StageShopManager>();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            goldIncreaseAmount = Mathf.Max(1, goldIncreaseAmount);
            fallbackPrice = Mathf.Max(1, fallbackPrice);
        }
#endif
    }
}
