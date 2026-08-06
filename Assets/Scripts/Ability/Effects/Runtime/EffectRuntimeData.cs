using System;

namespace Effect
{
    [Serializable]
    public class EffectRuntimeData
    {
        public string RuntimeId;

        public bool IsActive = true;

        public virtual void OnApply()
        {
        }

        public virtual void OnRemove()
        {
        }
    }
}
