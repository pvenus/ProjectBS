using System;
using Item;
using Stage;

namespace Session
{
    [Serializable]
    public class StageSession
    {
        public StageRuntimeData RuntimeData;

        public StrategicSkillItemRuntimeData StrategicSkillItemRuntimeData;

        public void Initialize(
            StageRuntimeData runtimeData)
        {
            RuntimeData = runtimeData;
            
            StrategicSkillItemRuntimeData ??= new StrategicSkillItemRuntimeData();
        }

        public void ResetRuntime()
        {
            RuntimeData = new StageRuntimeData();

            StrategicSkillItemRuntimeData = new StrategicSkillItemRuntimeData();
        }

        public void Clear()
        {
            RuntimeData = null;

            StrategicSkillItemRuntimeData = null;
        }
    }
}
