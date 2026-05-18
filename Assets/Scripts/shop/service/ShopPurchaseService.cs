using UnityEngine;
using Item;

namespace Shop
{
    public class ShopPurchaseService
    {
        private readonly ItemManager itemManager;

        private int currentGold;

        public int CurrentGold => currentGold;

        public ShopPurchaseService(
            ItemManager itemManager,
            int startGold = 0)
        {
            this.itemManager = itemManager;
            currentGold = Mathf.Max(0, startGold);
        }

        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentGold += amount;
        }

        public bool CanPurchase(
            ShopProductSO product)
        {
            if (product == null)
            {
                return false;
            }

            if (currentGold < product.price)
            {
                return false;
            }

            return true;
        }

        public bool TryPurchase(
            ShopProductSO product)
        {
            if (!CanPurchase(product))
            {
                return false;
            }

            if (!ApplyReward(product.rewardData))
            {
                return false;
            }

            currentGold -= product.price;

            Debug.Log(
                $"[ShopPurchaseService] Purchase success. product={product.displayName}, remainGold={currentGold}");

            return true;
        }

        private bool ApplyReward(
            ShopRewardData rewardData)
        {
            if (rewardData == null)
            {
                return false;
            }

            switch (rewardData.rewardType)
            {
                case ShopRewardType.Relic:
                    return GiveRelic(
                        rewardData.relic);

                case ShopRewardType.Consumable:
                    return GiveConsumable(
                        rewardData.consumable);

                case ShopRewardType.AIFunction:
                    return GiveAIFunction(
                        rewardData.aiFunction);
            }

            return false;
        }

        private bool GiveRelic(
            RelicSO relic)
        {
            if (relic == null)
            {
                return false;
            }

            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[ShopPurchaseService] ItemManager is null.");

                return false;
            }

            itemManager.AddRelic(relic);

            Debug.Log(
                $"[ShopPurchaseService] Relic granted. relic={relic.name}");

            return true;
        }

        private bool GiveConsumable(
            ScriptableObject consumable)
        {
            if (consumable == null)
            {
                return false;
            }

            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[ShopPurchaseService] ItemManager is null.");

                return false;
            }

            ConsumeSO consume =
                consumable as ConsumeSO;

            if (consume == null)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Invalid consume type. type={consumable.GetType().Name}");

                return false;
            }

            bool added =
                itemManager.AddConsume(consume);

            if (!added)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Failed to add consume. consume={consume.displayName}");

                return false;
            }

            Debug.Log(
                $"[ShopPurchaseService] Consumable granted. consumable={consume.displayName}");

            return true;
        }

        private bool GiveAIFunction(
            ScriptableObject aiFunction)
        {
            if (aiFunction == null)
            {
                return false;
            }

            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[ShopPurchaseService] ItemManager is null.");

                return false;
            }

            AIFunctionSO function =
                aiFunction as AIFunctionSO;

            if (function == null)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Invalid AI function type. type={aiFunction.GetType().Name}");

                return false;
            }

            bool added =
                itemManager.AddAIFunction(function);

            if (!added)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Failed to add AI function. function={function.displayName}");

                return false;
            }

            Debug.Log(
                $"[ShopPurchaseService] AI Function granted. ai={function.displayName}");

            return true;
        }
    }
}
