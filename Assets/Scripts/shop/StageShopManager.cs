

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
        [SerializeField] private ShopItemPoolSO defaultPool;
        [SerializeField] private ShopType shopType = ShopType.Normal;

        [Header("Generation")]
        [SerializeField] private string shopId = "test_shop";
        [SerializeField] private string shopName = "Shop";
        [SerializeField] private int itemCount = 6;
        [SerializeField] private bool useFixedSeed = false;
        [SerializeField] private int seed = 0;

        [Header("Player Debug")]
        [SerializeField] private int currentGold = 500;

        [Header("Runtime")]
        [SerializeField] private ShopRuntimeData currentShop;

        public ShopRuntimeData CurrentShop => currentShop;
        public int CurrentGold => currentGold;
        public bool HasShop => currentShop != null;
        public bool IsOpened => currentShop != null && currentShop.isOpened;

        public event Action<ShopRuntimeData> OnShopOpened;
        public event Action<ShopRuntimeData> OnShopClosed;
        public event Action<ShopRuntimeData> OnShopRefreshed;
        public event Action<ShopRuntimeItem> OnItemPurchased;
        public event Action<int> OnGoldChanged;

        private System.Random fixedRandom;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void OpenDefaultShop()
        {
            OpenShop(defaultPool, itemCount, shopType);
        }

        public void OpenShop(ShopItemPoolSO pool, int generateCount, ShopType targetShopType = ShopType.Normal)
        {
            if (pool == null)
            {
                Debug.LogWarning("[StageShopManager] OpenShop failed. Pool is null.");
                return;
            }

            fixedRandom = useFixedSeed ? new System.Random(seed) : null;

            currentShop = GenerateShop(pool, generateCount, targetShopType);
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

            ShopRuntimeItem item = currentShop.GetItem(runtimeItemId);
            if (item == null)
            {
                Debug.LogWarning($"[StageShopManager] Purchase failed. Item not found. runtimeId={runtimeItemId}");
                return false;
            }

            if (!item.CanPurchase(currentGold))
            {
                Debug.Log($"[StageShopManager] Cannot purchase item. item={item.DisplayName}, price={item.price}, gold={currentGold}, state={item.state}");
                return false;
            }

            currentGold -= item.price;
            item.MarkPurchased();

            Debug.Log($"[StageShopManager] Purchased: {item.DisplayName}, price={item.price}, remainingGold={currentGold}");

            OnGoldChanged?.Invoke(currentGold);
            OnItemPurchased?.Invoke(item);
            OnShopRefreshed?.Invoke(currentShop);
            return true;
        }

        public void SetGold(int gold)
        {
            currentGold = Mathf.Max(0, gold);
            OnGoldChanged?.Invoke(currentGold);
            OnShopRefreshed?.Invoke(currentShop);
        }

        public void AddGold(int amount)
        {
            currentGold = Mathf.Max(0, currentGold + amount);
            OnGoldChanged?.Invoke(currentGold);
            OnShopRefreshed?.Invoke(currentShop);
        }

        public void RefreshDefaultShop()
        {
            OpenDefaultShop();
        }

        private ShopRuntimeData GenerateShop(ShopItemPoolSO pool, int generateCount, ShopType targetShopType)
        {
            List<ShopItemEntry> candidates = GetCandidates(pool, targetShopType);
            if (candidates.Count == 0)
            {
                Debug.LogWarning($"[StageShopManager] No candidates in pool. pool={pool.poolId}");
                return null;
            }

            ShopRuntimeData shop = new ShopRuntimeData(shopId, shopName, targetShopType)
            {
                generatedFromPoolId = pool.poolId,
                seed = seed
            };

            List<ShopItemEntry> workingCandidates = new List<ShopItemEntry>(candidates);
            int count = Mathf.Max(0, generateCount);

            for (int i = 0; i < count; i++)
            {
                if (workingCandidates.Count == 0)
                {
                    break;
                }

                ShopItemEntry selectedEntry = PickWeighted(workingCandidates);
                if (selectedEntry == null || selectedEntry.item == null)
                {
                    continue;
                }

                ShopRuntimeItem runtimeItem = new ShopRuntimeItem(
                    selectedEntry.item,
                    selectedEntry.GetPrice(),
                    i,
                    pool.poolId);

                shop.AddItem(runtimeItem);

                if (!pool.allowDuplicate)
                {
                    workingCandidates.Remove(selectedEntry);
                }
            }

            return shop;
        }

        private List<ShopItemEntry> GetCandidates(ShopItemPoolSO pool, ShopType targetShopType)
        {
            List<ShopItemEntry> entries = pool.GetAvailableEntries();

            return entries
                .Where(x => x != null)
                .Where(x => x.item != null)
                .Where(x => x.shopType == ShopType.Normal || x.shopType == targetShopType)
                .Where(x => x.weight > 0)
                .ToList();
        }

        private ShopItemEntry PickWeighted(List<ShopItemEntry> candidates)
        {
            if (candidates == null || candidates.Count == 0)
            {
                return null;
            }

            int totalWeight = candidates.Sum(x => Mathf.Max(0, x.weight));
            if (totalWeight <= 0)
            {
                return candidates[GetRandomRange(0, candidates.Count)];
            }

            int roll = GetRandomRange(1, totalWeight + 1);
            int accumulated = 0;

            foreach (ShopItemEntry candidate in candidates)
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