using System;
using Battle;

namespace Stage
{
    [Serializable]
    public sealed class BattleExecutionData : ChoiceExecutionData
    {
        public BattleSO battle;
    }
}
