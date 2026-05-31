using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Item;
using Currency;

namespace Shop
{
    /// <summary>
    /// 상점 생성/열기/구매/닫기 흐름을 관리하는 런타임 매니저.
    /// 현재는 라운드 연결 전 단계이므로, ShopItemPoolSO에서 바로 상점을 생성해서 UI 검증이 가능하도록 구성한다.
    /// </summary>
    public class StageShopManager : MonoBehaviour
    {
        public static StageShopManager Instance { get; private set; }

        [Header("Shop Source")]
        [SerializeField] private List<ShopItemPoolSO> defaultPools = new();
        [SerializeField] private ShopType shopType = ShopType.Normal;

        [Header("Generation")]
        [SerializeField] private string shopId = "test_shop";
        [SerializeField] private int itemCount = 6;
        [SerializeField] private int maxPoolCount = 3;
        [SerializeField] private bool useFixedSeed = false;
        [SerializeField] private int seed = 0;

        [Header("Runtime")]
        [SerializeField] private ShopRuntimeData currentShop;

        public ShopRuntimeData CurrentShop => currentShop;
        public bool HasShop => currentShop != null;
        public bool IsOpened => currentShop != null && currentShop.isOpened;

        public event Action<ShopRuntimeData> OnShopOpened;
        public event Action<ShopRuntimeData> OnShopClosed;
        public event Action<ShopRuntimeData> OnShopRefreshed;
        public event Action<ShopRuntimeItem> OnItemPurchased;
        public event Action<int> OnGoldChanged;

        private System.Random fixedRandom;
        private ShopPurchaseService purchaseService;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            purchaseService = new ShopPurchaseService();
        }

        public void OpenDefaultShop()
        {
            OpenShop(defaultPools, itemCount, shopType);
        }

        public void OpenShop(List<ShopItemPoolSO> pools, int generateCount, ShopType targetShopType = ShopType.Normal)
        {
            if (pools == null
                || pools.Count == 0)
            {
                Debug.LogWarning("[StageShopManager] OpenShop failed. Pools are empty.");
                return;
            }

            fixedRandom = useFixedSeed ? new System.Random(seed) : null;

            currentShop = GenerateShop(pools, generateCount, targetShopType);
            if (currentShop == null)
            {
                Debug.LogWarning("[StageShopManager] OpenShop failed. Generated shop is null.");
                return;
            }

            currentShop.Open();
            OnShopOpened?.Invoke(currentShop);
            OnShopRefreshed?.Invoke(currentShop);
        }

        public void CloseShop()
        {
            if (currentShop == null)
            {
                return;
            }

            currentShop.Close();
            OnShopClosed?.Invoke(currentShop);
            OnShopRefreshed?.Invoke(currentShop);
        }

        public void ClearShop()
        {
            currentShop = null;
            OnShopRefreshed?.Invoke(null);
        }

        public bool TryPurchase(string runtimeItemId)
        {
            if (currentShop == null)
            {
                Debug.LogWarning("[StageShopManager] Purchase failed. Current shop is null.");
                return false;
            }

            ShopRuntimeItem item = FindItem(runtimeItemId);
            if (item == null)
            {
                Debug.LogWarning($"[StageShopManager] Purchase failed. Item not found. runtimeId={runtimeItemId}");
                return false;
            }

            if (purchaseService == null)
            {
                Debug.LogWarning(
                    "[StageShopManager] Purchase failed. PurchaseService is null.");

                return false;
            }

            if (!item.CanPurchase(CurrencyManager.Instance.Gold))
            {
                Debug.Log($"[StageShopManager] Cannot purchase item. item={item.DisplayName}, price={item.price}, gold={CurrencyManager.Instance.Gold}, state={item.state}");
                return false;
            }

            ShopProductSO product = item.product;

            if (product == null)
            {
                Debug.LogWarning(
                    $"[StageShopManager] Purchase failed. Product is null. item={item.DisplayName}");

                return false;
            }

            if (!purchaseService.TryPurchase(product))
            {
                Debug.LogWarning(
                    $"[StageShopManager] Purchase service failed. product={product.DisplayName}");

                return false;
            }

            item.MarkPurchased();

            Debug.Log($"[StageShopManager] Purchased: {item.DisplayName}, price={product.price}, remainingGold={CurrencyManager.Instance.Gold}");

            OnGoldChanged?.Invoke(CurrencyManager.Instance.Gold);
            OnItemPurchased?.Invoke(item);
            OnShopRefreshed?.Invoke(currentShop);
            return true;
        }

        public void SetGold(int gold)
        {
            CurrencyManager.Instance.SetGold(gold);
            OnGoldChanged?.Invoke(CurrencyManager.Instance.Gold);
            OnShopRefreshed?.Invoke(currentShop);
        }

        public void AddGold(int amount)
        {
            CurrencyManager.Instance.AddGold(amount);
            OnGoldChanged?.Invoke(CurrencyManager.Instance.Gold);
            OnShopRefreshed?.Invoke(currentShop);
        }

        public void RefreshDefaultShop()
        {
            OpenDefaultShop();
        }

        private ShopRuntimeItem FindItem(string runtimeItemId)
        {
            if (currentShop?.groups == null)
            {
                return null;
            }

            foreach (ShopRuntimeGroup group in currentShop.groups)
            {
                if (group?.items == null)
                {
                    continue;
                }

                ShopRuntimeItem item =
                    group.items.FirstOrDefault(
                        x => x != null && x.runtimeId == runtimeItemId);

                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        private ShopRuntimeData GenerateShop(
            List<ShopItemPoolSO> pools,
            int generateCount,
            ShopType targetShopType)
        {
            List<ShopItemPoolSO> selectedPools =
                pools
                    .Where(x => x != null)
                    .Take(Mathf.Clamp(maxPoolCount, 1, 3))
                    .ToList();

            if (selectedPools.Count <= 0)
            {
                Debug.LogWarning(
                    "[StageShopManager] No selected pools.");

                return null;
            }


            ShopRuntimeData shop =
                new ShopRuntimeData(
                    shopId,
                    targetShopType)
                {
                    generatedFromPoolId = string.Join(",",
                        selectedPools.Select(x => x.poolId)),
                    seed = seed
                };

            Dictionary<string, ShopRuntimeGroup> runtimeGroups =
                new Dictionary<string, ShopRuntimeGroup>();

            foreach (ShopItemPoolSO pool in selectedPools)
            {
                if (pool == null)
                {
                    continue;
                }

                ShopRuntimeGroup group =
                    shop.AddGroup(
                        pool.poolId,
                        pool.DisplayName);

                runtimeGroups[pool.poolId] = group;
            }

            int runtimeIndex = 0;

            int countPerPool =
                Mathf.Max(1,
                    generateCount / selectedPools.Count);

            for (int poolIndex = 0;
                 poolIndex < selectedPools.Count;
                 poolIndex++)
            {
                ShopItemPoolSO pool =
                    selectedPools[poolIndex];

                if (pool == null)
                {
                    continue;
                }

                List<ShopProductSO> candidates =
                    GetCandidates(pool);

                if (candidates.Count <= 0)
                {
                    continue;
                }

                List<ShopProductSO> workingCandidates =
                    new List<ShopProductSO>(candidates);

                for (int i = 0; i < countPerPool; i++)
                {
                    if (workingCandidates.Count <= 0)
                    {
                        break;
                    }

                    ShopProductSO selectedProduct =
                        PickWeighted(workingCandidates);

                    if (selectedProduct == null)
                    {
                        continue;
                    }

                    ShopRuntimeItem runtimeItem =
                        new ShopRuntimeItem(
                            selectedProduct,
                            selectedProduct.price,
                            runtimeIndex,
                            pool.poolId);

                    runtimeIndex++;

                    if (runtimeGroups.TryGetValue(
                            pool.poolId,
                            out ShopRuntimeGroup group))
                    {
                        shop.AddItemToGroup(
                            group,
                            runtimeItem);
                    }

                    if (!pool.allowDuplicate)
                    {
                        workingCandidates.Remove(selectedProduct);
                    }
                }
            }

            return shop;
        }

        private List<ShopProductSO> GetCandidates(
            ShopItemPoolSO pool)
        {
            List<ShopProductSO> result = new();

            if (pool == null)
            {
                return result;
            }

            List<ShopProductSO> products =
                pool.GetAvailableProducts();

            if (products == null)
            {
                return result;
            }

            for (int i = 0; i < products.Count; i++)
            {
                ShopProductSO product = products[i];

                if (product == null)
                {
                    continue;
                }

                if (result.Contains(product))
                {
                    continue;
                }

                result.Add(product);
            }

            return result;
        }

        private ShopProductSO PickWeighted(
            List<ShopProductSO> candidates)
        {
            if (candidates == null
                || candidates.Count == 0)
            {
                return null;
            }

            int totalWeight =
                candidates.Sum(x => Mathf.Max(0, x.weight));

            if (totalWeight <= 0)
            {
                return candidates[
                    GetRandomRange(0, candidates.Count)];
            }

            int roll =
                GetRandomRange(1, totalWeight + 1);

            int accumulated = 0;

            foreach (ShopProductSO candidate in candidates)
            {
                accumulated += Mathf.Max(0, candidate.weight);

                if (roll <= accumulated)
                {
                    return candidate;
                }
            }

            return candidates[^1];
        }

        private int GetRandomRange(int minInclusive, int maxExclusive)
        {
            if (fixedRandom != null)
            {
                return fixedRandom.Next(minInclusive, maxExclusive);
            }

            return Random.Range(minInclusive, maxExclusive);
        }
    }
}