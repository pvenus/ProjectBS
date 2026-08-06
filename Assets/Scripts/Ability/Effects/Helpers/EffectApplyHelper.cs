using System.Collections.Generic;
using UnityEngine;

namespace Effect.Helper
{
    /// <summary>
    /// 공통 이펙트 적용 헬퍼.
    /// 스킬, 유물, 소비 아이템처럼 서로 다른 시스템에서 EffectSO를 EffectManager에 등록할 때 사용한다.
    /// </summary>
    public static class EffectApplyHelper
    {
        public static bool ApplyEffect(
            EffectManager effectManager,
            EffectEntryRuntime effectEntry)
        {
            if (effectManager == null
                || effectEntry == null
                || effectEntry.RuntimeData == null)
            {
                return false;
            }

            effectManager.AddEffect(
                effectEntry.RuntimeData,
                effectEntry.LifetimeType,
                effectEntry.Duration,
                effectEntry.CategoryType);
            return true;
        }

        public static int ApplyEffects(
            EffectManager effectManager,
            IEnumerable<EffectEntryRuntime> effects)
        {
            if (effects == null)
            {
                return 0;
            }

            int appliedCount = 0;

            foreach (EffectEntryRuntime effectEntry in effects)
            {
                bool applied = ApplyEffect(
                    effectManager,
                    effectEntry);

                if (applied)
                {
                    appliedCount++;
                }
            }

            return appliedCount;
        }

        public static bool ApplyRuntimeData(
            EffectManager effectManager,
            EffectRuntimeData runtimeData,
            EffectLifetimeType lifetimeType,
            float duration,
            EffectCategoryType categoryType)
        {
            if (effectManager == null || runtimeData == null)
            {
                return false;
            }

            effectManager.AddEffect(runtimeData, lifetimeType, duration, categoryType);
            return true;
        }

        public static int ApplyRuntimeDataList(
            EffectManager effectManager,
            IEnumerable<EffectRuntimeData> runtimeDataList,
            EffectLifetimeType lifetimeType,
            float duration,
            EffectCategoryType categoryType)
        {
            if (runtimeDataList == null)
            {
                return 0;
            }

            int appliedCount = 0;

            foreach (EffectRuntimeData runtimeData in runtimeDataList)
            {
                bool applied = ApplyRuntimeData(
                    effectManager,
                    runtimeData,
                    lifetimeType,
                    duration,
                    categoryType);

                if (applied)
                {
                    appliedCount++;
                }
            }

            return appliedCount;
        }
    }
}