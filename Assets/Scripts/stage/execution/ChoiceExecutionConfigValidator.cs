using System.Collections.Generic;
using Shop;
using Shrine;

namespace Stage
{
    /// <summary>
    /// 실행 config 자체의 구조와 필수 참조를 검사한다.
    /// 에셋 탐색과 NextEvent 그래프 순환 검사는 Editor validator가 담당한다.
    /// </summary>
    public static class ChoiceExecutionConfigValidator
    {
        public static List<string> Validate(ChoiceExecutionConfig config)
        {
            List<string> errors = new();
            CollectErrors(config, errors);
            return errors;
        }

        public static void CollectErrors(
            ChoiceExecutionConfig config,
            List<string> errors)
        {
            if (errors == null)
            {
                return;
            }

            if (config == null)
            {
                errors.Add("CONFIG_NULL: executionConfig is null.");
                return;
            }

            if (config.executionType == ChoiceExecutionType.None)
            {
                errors.Add("TYPE_NONE: executionType must not be None.");
            }

            if (config.data == null)
            {
                errors.Add("DATA_NULL: execution data is null.");
                return;
            }

            if (!IsTypeMatch(config.executionType, config.data))
            {
                errors.Add(
                    $"TYPE_MISMATCH: {config.executionType} cannot use "
                    + $"{config.data.GetType().Name}.");
                return;
            }

            switch (config.data)
            {
                case NextEventExecutionData nextEventData:
                    if (nextEventData.nextEvent == null)
                    {
                        errors.Add(
                            "NEXT_EVENT_NULL: NextEvent target is required.");
                    }
                    break;

                case BattleExecutionData battleData:
                    if (battleData.battle == null)
                    {
                        errors.Add(
                            "BATTLE_NULL: BattleSO reference is required.");
                    }
                    break;

                case ShopExecutionData shopData:
                    ValidateShop(shopData, errors);
                    break;

                case ShrineExecutionData shrineData:
                    ValidateShrine(shrineData, errors);
                    break;
            }
        }

        public static bool IsTypeMatch(
            ChoiceExecutionType executionType,
            ChoiceExecutionData data)
        {
            return executionType switch
            {
                ChoiceExecutionType.NextEvent =>
                    data is NextEventExecutionData,
                ChoiceExecutionType.Battle =>
                    data is BattleExecutionData,
                ChoiceExecutionType.Shop =>
                    data is ShopExecutionData,
                ChoiceExecutionType.Shrine =>
                    data is ShrineExecutionData,
                ChoiceExecutionType.CompleteEvent =>
                    data is CompleteEventExecutionData,
                _ => data == null
            };
        }

        private static void ValidateShop(
            ShopExecutionData data,
            List<string> errors)
        {
            if (data.pools == null || data.pools.Count == 0)
            {
                errors.Add(
                    "SHOP_POOLS_EMPTY: At least one ShopItemPoolSO is required.");
            }
            else
            {
                for (int i = 0; i < data.pools.Count; i++)
                {
                    ShopItemPoolSO pool = data.pools[i];

                    if (pool == null)
                    {
                        errors.Add(
                            $"SHOP_POOL_NULL: pools[{i}] is null.");
                        continue;
                    }

                    if (!HasValidProduct(pool))
                    {
                        errors.Add(
                            $"SHOP_POOL_EMPTY: pools[{i}] has no valid product.");
                    }
                }
            }

            if (data.itemCount <= 0)
            {
                errors.Add(
                    "SHOP_ITEM_COUNT: itemCount must be greater than zero.");
            }
        }

        private static bool HasValidProduct(ShopItemPoolSO pool)
        {
            if (pool.products == null)
            {
                return false;
            }

            foreach (ShopProductSO product in pool.products)
            {
                if (product != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateShrine(
            ShrineExecutionData data,
            List<string> errors)
        {
            if (data.config == null)
            {
                errors.Add(
                    "SHRINE_CONFIG_NULL: ShrineConfigSO reference is required.");
            }

            if (data.god == null)
            {
                errors.Add(
                    "SHRINE_GOD_NULL: ShrineGodSO reference is required.");
                return;
            }

            if (data.god.GodType == ShrineGodType.None)
            {
                errors.Add(
                    "SHRINE_GOD_NONE: ShrineGodType must not be None.");
            }

            if (data.config != null && !ContainsGod(data.config, data.god))
            {
                errors.Add(
                    "SHRINE_GOD_NOT_REGISTERED: "
                    + "ShrineConfigSO must contain the selected ShrineGodSO.");
            }
        }

        private static bool ContainsGod(
            ShrineConfigSO config,
            ShrineGodSO targetGod)
        {
            if (config.Gods == null)
            {
                return false;
            }

            foreach (ShrineGodSO god in config.Gods)
            {
                if (god == targetGod)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
