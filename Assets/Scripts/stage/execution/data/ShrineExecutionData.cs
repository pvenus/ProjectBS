using System;
using Shrine;

namespace Stage
{
    [Serializable]
    public sealed class ShrineExecutionData : ChoiceExecutionData
    {
        public ShrineConfigSO config;
        public ShrineGodSO god;
    }
}
