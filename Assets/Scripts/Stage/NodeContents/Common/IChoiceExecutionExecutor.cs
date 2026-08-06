namespace Stage
{
    public interface IChoiceExecutionExecutor
    {
        ChoiceExecutionType ExecutionType { get; }

        bool TryExecute(
            ChoiceExecutionData data,
            ChoiceExecutionContext context,
            out string error);
    }
}
