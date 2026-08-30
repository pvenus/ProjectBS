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

            int identityFieldCount = CountContinuationIdentityFields(nextEventData);
            if (identityFieldCount != 0 && identityFieldCount != 8)
            {
                error = "NEXT_EVENT_TRANSACTION_IDENTITY_INVALID: "
                    + "Continuation identity must be all-empty or all-complete.";
                return false;
            }

            if (identityFieldCount == 8)
            {
                if (context?.OpenNextEventTransaction == null)
                {
                    error = "NEXT_EVENT_TRANSACTION_CONTEXT_INVALID: "
                        + "OpenNextEventTransaction action is required.";
                    return false;
                }

                return context.OpenNextEventTransaction(nextEventData, out error);
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

        private static int CountContinuationIdentityFields(NextEventExecutionData data)
        {
            int count = 0;
            if (!string.IsNullOrWhiteSpace(data.parentEventId)) count++;
            if (!string.IsNullOrWhiteSpace(data.parentNodeId)) count++;
            if (!string.IsNullOrWhiteSpace(data.parentChoiceId)) count++;
            if (!string.IsNullOrWhiteSpace(data.parentResultId)) count++;
            if (!string.IsNullOrWhiteSpace(data.parentReservationId)) count++;
            if (!string.IsNullOrWhiteSpace(data.childEventId)) count++;
            if (!string.IsNullOrWhiteSpace(data.childNodeId)) count++;
            if (!string.IsNullOrWhiteSpace(data.childReservationId)) count++;
            return count;
        }
    }
}
