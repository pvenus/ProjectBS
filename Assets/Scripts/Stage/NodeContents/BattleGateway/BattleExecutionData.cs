using System;
using Battle;

namespace Stage
{
    [Serializable]
    public sealed class BattleExecutionData : ChoiceExecutionData
    {
        public BattleSO battle;

        // Optional event-scoped completion identity. Existing Battle assets leave
        // every field empty and retain the legacy node-only completion path.
        public string eventId;
        public string nodeId;
        public string sourcePopupId;
        public string reservationId;
        public string choiceId;
        public string expectedVictoryResultId;
    }
}
