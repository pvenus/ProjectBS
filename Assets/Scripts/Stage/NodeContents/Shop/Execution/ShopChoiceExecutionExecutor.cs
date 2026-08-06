namespace Stage
{
    public sealed class ShopChoiceExecutionExecutor
        : IChoiceExecutionExecutor
    {
        public ChoiceExecutionType ExecutionType =>
            ChoiceExecutionType.Shop;

        public bool TryExecute(
            ChoiceExecutionData data,
            ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;

            if (data is not ShopExecutionData shopData)
            {
                error =
                    "SHOP_DATA_INVALID: "
                    + "ShopExecutionData is required.";
                return false;
            }

            if (context?.OpenShop == null)
            {
                error =
                    "SHOP_CONTEXT_INVALID: "
                    + "OpenShop action is required.";
                return false;
            }

            if (!context.OpenShop(shopData))
            {
                error =
                    "SHOP_OPEN_FAILED: "
                    + "Shop runtime rejected the request.";
                return false;
            }

            return true;
        }
    }
}
