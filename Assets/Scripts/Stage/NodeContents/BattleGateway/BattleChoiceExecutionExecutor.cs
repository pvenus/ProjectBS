namespace Stage
{
    public sealed class BattleChoiceExecutionExecutor
        : IChoiceExecutionExecutor
    {
        public ChoiceExecutionType ExecutionType =>
            ChoiceExecutionType.Battle;

        public bool TryExecute(
            ChoiceExecutionData data,
            ChoiceExecutionContext context,
            out string error)
        {
            error = string.Empty;

            if (data is not BattleExecutionData battleData)
            {
                error =
                    "BATTLE_DATA_INVALID: "
                    + "BattleExecutionData is required.";
                return false;
            }

            if (battleData.battle == null)
            {
                error =
                    "BATTLE_NULL: BattleSO reference is required.";
                return false;
            }

            if (context?.BeginBattle == null)
            {
                error =
                    "BATTLE_CONTEXT_INVALID: "
                    + "BeginBattle action is required.";
                return false;
            }

            if (!context.BeginBattle(battleData.battle))
            {
                error =
                    "BATTLE_OPEN_FAILED: "
                    + "Battle runtime rejected the request.";
                return false;
            }

            return true;
        }
    }
}
