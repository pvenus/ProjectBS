using Item;
using Currency;
using UnityEngine;

namespace Shop
{
    public class ShopPurchaseService
    {

        public ShopPurchaseService()
        {
        }

        public void SetGold(int amount)
        {
            CurrencyManager.Instance.SetGold(amount);
        }

        public void AddGold(int amount)
        {
            CurrencyManager.Instance.AddGold(amount);
        }

        public bool CanPurchase(
            ShopProductSO product)
        {
            return CanPurchase(
                product,
                product != null ? product.price : 0);
        }

        public bool CanPurchase(
            ShopProductSO product,
            int price)
        {
            if (product == null)
            {
                return false;
            }

            return CurrencyManager.Instance.CanSpendGold(
                Mathf.Max(0, price));
        }

        public bool TryPurchase(
            ShopProductSO product)
        {
            return TryPurchase(
                product,
                product != null ? product.price : 0);
        }

        public bool TryPurchase(
            ShopProductSO product,
            int price)
        {
            int purchasePrice = Mathf.Max(0, price);

            if (!CanPurchase(product, purchasePrice))
            {
                return false;
            }

            if (!ApplyReward(product.rewardData))
            {
                return false;
            }

            CurrencyManager.Instance.TrySpendGold(purchasePrice);

            Debug.Log(
                $"[ShopPurchaseService] Purchase success. product={product.DisplayName}, remainGold={CurrencyManager.Instance.Gold}");

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
                        rewardData.Relic);

                case ShopRewardType.StrategicSkillItem:
                    return GiveStrategicSkillItem(
                        rewardData.StrategicSkillItem);

                case ShopRewardType.AIFunction:
                    return GiveAIFunction(
                        rewardData.reward);
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

            if (ItemManager.Instance == null)
            {
                Debug.LogWarning(
                    "[ShopPurchaseService] ItemManager is null.");

                return false;
            }

            ItemManager.Instance.AddRelic(relic);

            Debug.Log(
                $"[ShopPurchaseService] Relic granted. relic={relic.name}");

            return true;
        }

        private bool GiveStrategicSkillItem(
            StrategicSkillItemSO strategicSkillItem)
        {
            if (strategicSkillItem == null)
            {
                return false;
            }

            if (ItemManager.Instance == null)
            {
                Debug.LogWarning(
                    "[ShopPurchaseService] ItemManager is null.");

                return false;
            }

            bool added =
                ItemManager.Instance.AddStrategicSkillItem(strategicSkillItem);

            if (!added)
            {
                Debug.LogWarning(
                    $"[ShopPurchaseService] Failed to add strategic skill item. item={strategicSkillItem.DisplayName}");

                return false;
            }

            Debug.Log(
                $"[ShopPurchaseService] Strategic skill item granted. item={strategicSkillItem.DisplayName}");

            return true;
        }

        private bool GiveAIFunction(
            ScriptableObject aiFunction)
        {
            if (aiFunction == null)
            {
                return false;
            }

            if (ItemManager.Instance == null)
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
                ItemManager.Instance.AddAIFunction(function);

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
