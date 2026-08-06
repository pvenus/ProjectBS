using System;

namespace Effect
{
    [Serializable]
    public class TauntEffectConfig : EffectConfig
    {
#if UNITY_EDITOR
        public void ApplyEditorData()
        {
        }
#endif
    }
}
