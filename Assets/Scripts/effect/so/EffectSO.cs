using String;
using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "EffectSO",
        menuName = "Effect/Effect SO")]
    public class EffectSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string effectId;

        [Header("Visual")]
        [SerializeField] private Sprite icon;
        
        [Header("Config")]
        [SerializeReference] private EffectConfig config;

        public EffectConfig Config => config;

        public string LocalizationMainKey => effectId;

        public string DisplayName =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "name");

        public string Description =>
            StringManager.Instance.Get(
                LocalizationMainKey,
                "desc");

        public string EffectId => effectId;
        public Sprite Icon => icon;

#if UNITY_EDITOR
        public void ApplyEditorData(
            string effectId,
            Sprite icon,
            EffectConfig config)
        {
            this.effectId = effectId;
            this.icon = icon;
            this.config = config;
        }
#endif
    }
}
