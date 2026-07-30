namespace Stage
{
    public sealed class NextEventChoiceExecutionExecutor
        : IChoiceExecutionExecutor
    {
        public ChoiceExecutionType ExecutionType =>
            ChoiceExecutionType.NextEvent;

        public bool TryExecute(
            ChoiceExecutionData data,
            ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;

            if (data is not NextEventExecutionData nextEventData)
            {
                error =
                    "NEXT_EVENT_DATA_INVALID: "
                    + "NextEventExecutionData is required.";
                return false;
            }

            if (nextEventData.nextEvent == null)
            {
                error =
                    "NEXT_EVENT_NULL: NextEvent target is required.";
                return false;
            }

            if (context?.OpenNextEvent == null)
            {
                error =
                    "NEXT_EVENT_CONTEXT_INVALID: "
                    + "OpenNextEvent action is required.";
                return false;
            }

            context.OpenNextEvent(nextEventData.nextEvent);
            return true;
        }
    }
}
