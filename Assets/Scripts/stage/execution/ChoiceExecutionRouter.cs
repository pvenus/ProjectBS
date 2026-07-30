using System.Collections.Generic;

namespace Stage
{
    /// <summary>
    /// Choice 실행 타입을 executor로 전달하고 선택 실행 ID별 중복 호출을 방지한다.
    /// </summary>
    public sealed class ChoiceExecutionRouter
    {
        private readonly Dictionary<
            ChoiceExecutionType,
            IChoiceExecutionExecutor> executors = new();

        private readonly HashSet<string> completedExecutionIds = new();

        public ChoiceExecutionRouter(
            IEnumerable<IChoiceExecutionExecutor> executors)
        {
            if (executors == null)
            {
                return;
            }

            foreach (IChoiceExecutionExecutor executor in executors)
            {
                if (executor == null
                    || executor.ExecutionType
                        == ChoiceExecutionType.None)
                {
                    continue;
                }

                this.executors[executor.ExecutionType] = executor;
            }
        }

        public static ChoiceExecutionRouter CreateNextEventOnly()
        {
            return new ChoiceExecutionRouter(
                new IChoiceExecutionExecutor[]
                {
                    new NextEventChoiceExecutionExecutor()
                });
        }

        public static ChoiceExecutionRouter CreateDefault()
        {
            return new ChoiceExecutionRouter(
                new IChoiceExecutionExecutor[]
                {
                    new NextEventChoiceExecutionExecutor(),
                    new BattleChoiceExecutionExecutor(),
                    new ShopChoiceExecutionExecutor(),
                    new ShrineChoiceExecutionExecutor(),
                    new CompleteEventChoiceExecutionExecutor()
                });
        }

        public ChoiceExecutionResult TryExecute(
            string executionId,
            ChoiceExecutionConfig config,
            ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(executionId))
            {
                error =
                    "EXECUTION_ID_REQUIRED: executionId is required.";
                return ChoiceExecutionResult.InvalidRequest;
            }

            if (completedExecutionIds.Contains(executionId))
            {
                return ChoiceExecutionResult.AlreadyExecuted;
            }

            List<string> validationErrors =
                ChoiceExecutionConfigValidator.Validate(config);

            if (validationErrors.Count > 0)
            {
                error = string.Join(" | ", validationErrors);
                return ChoiceExecutionResult.InvalidConfig;
            }

            if (!executors.TryGetValue(
                    config.executionType,
                    out IChoiceExecutionExecutor executor))
            {
                error =
                    $"EXECUTION_TYPE_UNSUPPORTED: "
                    + $"{config.executionType} is not connected.";
                return ChoiceExecutionResult.UnsupportedType;
            }

            if (!executor.TryExecute(config.data, context, out error))
            {
                return ChoiceExecutionResult.ExecutionFailed;
            }

            completedExecutionIds.Add(executionId);
            return ChoiceExecutionResult.Success;
        }

        public void ClearHistory()
        {
            completedExecutionIds.Clear();
        }
    }
}
