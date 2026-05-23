using System;
using Stage;

namespace Session
{
    [Serializable]
    public class StageSession
    {
        public StageRuntimeData RuntimeData;

        public void Initialize(
            StageRuntimeData runtimeData)
        {
            RuntimeData = runtimeData;
        }

        public void ResetRuntime()
        {
            RuntimeData = new StageRuntimeData();
        }

        public void Clear()
        {
            RuntimeData = null;
        }
    }
}
