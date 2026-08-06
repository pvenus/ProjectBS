namespace Stage
{
    public sealed class CompleteEventChoiceExecutionExecutor
        : IChoiceExecutionExecutor
    {
        public ChoiceExecutionType ExecutionType =>
            ChoiceExecutionType.CompleteEvent;

        public bool TryExecute(
            ChoiceExecutionData data,
            ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;

            if (data is not CompleteEventExecutionData)
            {
                error =
                    "COMPLETE_EVENT_DATA_INVALID: "
                    + "CompleteEventExecutionData is required.";
                return false;
            }

            if (context?.CompleteEvent == null)
            {
                error =
                    "COMPLETE_EVENT_CONTEXT_INVALID: "
                    + "CompleteEvent action is required.";
                return false;
            }

            if (!context.CompleteEvent())
            {
                error =
                    "COMPLETE_EVENT_FAILED: "
                    + "Stage runtime rejected the request.";
                return false;
            }

            return true;
        }
    }
}
