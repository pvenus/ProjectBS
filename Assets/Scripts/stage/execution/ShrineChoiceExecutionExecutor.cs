namespace Stage
{
    public sealed class ShrineChoiceExecutionExecutor
        : IChoiceExecutionExecutor
    {
        public ChoiceExecutionType ExecutionType =>
            ChoiceExecutionType.Shrine;

        public bool TryExecute(
            ChoiceExecutionData data,
            ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;

            if (data is not ShrineExecutionData shrineData)
            {
                error =
                    "SHRINE_DATA_INVALID: "
                    + "ShrineExecutionData is required.";
                return false;
            }

            if (context?.OpenShrine == null)
            {
                error =
                    "SHRINE_CONTEXT_INVALID: "
                    + "OpenShrine action is required.";
                return false;
            }

            if (!context.OpenShrine(shrineData))
            {
                error =
                    "SHRINE_OPEN_FAILED: "
                    + "Shrine runtime rejected the request.";
                return false;
            }

            return true;
        }
    }
}
