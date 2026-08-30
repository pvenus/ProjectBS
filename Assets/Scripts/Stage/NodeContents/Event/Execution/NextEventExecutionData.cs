using System;

namespace Stage
{
    [Serializable]
    public sealed class NextEventExecutionData : ChoiceExecutionData
    {
        public PopupEventSO nextEvent;
        public string parentEventId;
        public string parentNodeId;
        public string parentChoiceId;
        public string parentResultId;
        public string parentReservationId;
        public string childEventId;
        public string childNodeId;
        public string childReservationId;
    }
}
