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

                case ShopRewardType.StrategicSkillItem:
                    return GiveStrategicSkillItem(
                        rewardData.strategicSkillItem);

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

        private bool GiveStrategicSkillItem(
            ScriptableObject strategicSkillItemObject)
        {
            if (strategicSkillItemObject == null)
            {
                return false;
            }

            if (itemManager == null)
            {
                Debug.LogWarning(
                    "[ShopPurchaseService] ItemManager is null.");

                return false;
            }

            StrategicSkillItemSO strategicSkillItem =
                strategicSkillItemObject as StrategicSkillItemSO;

            if (strategicSkillItem == null)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Invalid strategic skill item type. type={strategicSkillItemObject.GetType().Name}");

                return false;
            }

            bool added =
                itemManager.AddStrategicSkillItem(strategicSkillItem);

            if (!added)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Failed to add strategic skill item. item={strategicSkillItem.displayName}");

                return false;
            }

            Debug.Log(
                $"[ShopPurchaseService] Strategic skill item granted. item={strategicSkillItem.displayName}");

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
