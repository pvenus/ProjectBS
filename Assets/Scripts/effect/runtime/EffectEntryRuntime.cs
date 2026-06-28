namespace Effect
{
    public class EffectEntryRuntime
    {
        public EffectRuntimeData RuntimeData { get; }
        public EffectLifetimeType LifetimeType { get; }
        public EffectCategoryType CategoryType { get; }
        public float Duration { get; }
        public int MaxApplyCount { get; }

        public EffectEntryRuntime(
            EffectRuntimeData runtimeData,
            EffectLifetimeType lifetimeType,
            EffectCategoryType categoryType,
            float duration,
            int maxApplyCount)
        {
            RuntimeData = runtimeData;
            LifetimeType = lifetimeType;
            CategoryType = categoryType;
            Duration = duration;
            MaxApplyCount = maxApplyCount;
        }
    }
}