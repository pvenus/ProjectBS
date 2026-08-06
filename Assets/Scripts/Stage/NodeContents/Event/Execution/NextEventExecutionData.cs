using System;

namespace Stage
{
    [Serializable]
    public sealed class NextEventExecutionData : ChoiceExecutionData
    {
        public PopupEventSO nextEvent;
    }
}
