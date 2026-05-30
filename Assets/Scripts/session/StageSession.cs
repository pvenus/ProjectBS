using System;
using Item;
using Currency;
using Stage;

namespace Session
{
    [Serializable]
    public class StageSession
    {
        public StageRuntimeData RuntimeData;

        public StrategicSkillItemRuntimeData StrategicSkillItemRuntimeData;

        public RelicRuntimeData RelicRuntimeData;

        public CurrencyRutimeData CurrencyRuntimeData;

        public void Initialize(
            StageRuntimeData runtimeData)
        {
            RuntimeData = runtimeData;
            
            StrategicSkillItemRuntimeData ??= new StrategicSkillItemRuntimeData();
            RelicRuntimeData ??= new RelicRuntimeData();
            CurrencyRuntimeData ??= new CurrencyRutimeData();
        }

        public void ResetRuntime()
        {
            RuntimeData = new StageRuntimeData();

            StrategicSkillItemRuntimeData = new StrategicSkillItemRuntimeData();
            RelicRuntimeData = new RelicRuntimeData();
            CurrencyRuntimeData = new CurrencyRutimeData();
        }

        public void Clear()
        {
            RuntimeData = null;

            StrategicSkillItemRuntimeData = null;
            RelicRuntimeData = null;
            CurrencyRuntimeData = null;
        }
    }
}
