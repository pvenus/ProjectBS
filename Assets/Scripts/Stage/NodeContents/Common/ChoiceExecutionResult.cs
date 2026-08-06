namespace Stage
{
    public enum ChoiceExecutionResult
    {
        Success = 0,
        AlreadyExecuted = 10,
        InvalidRequest = 20,
        InvalidConfig = 30,
        UnsupportedType = 40,
        ExecutionFailed = 50
    }
}
