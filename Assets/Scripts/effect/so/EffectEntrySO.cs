

using UnityEngine;

namespace Effect
{
    [CreateAssetMenu(
        fileName = "effect.entry",
        menuName = "BS/Effect/Effect Entry",
        order = 30)]
    public class EffectEntrySO : ScriptableObject
    {
        [Header("Effect")]
        [SerializeField] private EffectSO effectSO;

        [Header("Apply")]
        [SerializeField] private EffectLifetimeType lifetimeType;
        [SerializeField] private EffectCategoryType categoryType;
        [SerializeField] private float duration;
        [SerializeField] private int maxApplyCount = 1;

        [Header("Override")]
        [SerializeField] private bool hasValueOverride;
        [SerializeField] private float valueOverride;

        public EffectSO EffectSO => effectSO;
        public EffectLifetimeType LifetimeType => lifetimeType;
        public EffectCategoryType CategoryType => categoryType;
        public float Duration => duration;
        public int MaxApplyCount => maxApplyCount;
        public bool HasValueOverride => hasValueOverride;
        public float ValueOverride => valueOverride;

#if UNITY_EDITOR
        public void ApplyEditorData(
            EffectSO effectSO,
            EffectLifetimeType lifetimeType,
            EffectCategoryType categoryType,
            float duration,
            int maxApplyCount,
            bool hasValueOverride,
            float valueOverride)
        {
            this.effectSO = effectSO;
            this.lifetimeType = lifetimeType;
            this.categoryType = categoryType;
            this.duration = duration;
            this.maxApplyCount = maxApplyCount;
            this.hasValueOverride = hasValueOverride;
            this.valueOverride = valueOverride;
        }
#endif
    }
}