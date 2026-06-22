using System.Collections.Generic;
using Character;
using UnityEngine;
using Skills.Dto;

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
            CharacterManager targetCharacter,
            EffectSO effectSo,
            EffectSourceType sourceType,
            string sourceId,
            EffectLifetimeType lifetimeType = EffectLifetimeType.Instant,
            float duration = 0f,
            EffectCategoryType categoryType = EffectCategoryType.Neutral,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default,
            CharacterManager sourceCharacter = null,
            SkillProjectileHitEffectEntry effectEntry = null)
        {
            if (effectManager == null || targetCharacter == null || effectSo == null)
            {
                return false;
            }

            EffectRuntimeData runtimeData = EffectResolveHelper.CreateRuntimeData(
                effectSo,
                sourceType,
                sourceId,
                targetCharacter,
                sourceTransform,
                projectileDirection,
                sourceCharacter,
                effectEntry);

            if (runtimeData == null)
            {
                return false;
            }

            effectManager.AddEffect(runtimeData, lifetimeType, duration, categoryType);
            return true;
        }

        public static int ApplyEffects(
            EffectManager effectManager,
            CharacterManager targetCharacter,
            IEnumerable<EffectSO> effects,
            EffectSourceType sourceType,
            string sourceId,
            EffectLifetimeType lifetimeType = EffectLifetimeType.Instant,
            float duration = 0f,
            EffectCategoryType categoryType = EffectCategoryType.Neutral,
            Transform sourceTransform = null,
            Vector2 projectileDirection = default,
            CharacterManager sourceCharacter = null,
            SkillProjectileHitEffectEntry effectEntry = null)
        {
            if (effects == null)
            {
                return 0;
            }

            int appliedCount = 0;

            foreach (EffectSO effectSo in effects)
            {
                bool applied = ApplyEffect(
                    effectManager,
                    targetCharacter,
                    effectSo,
                    sourceType,
                    sourceId,
                    lifetimeType,
                    duration,
                    categoryType,
                    sourceTransform,
                    projectileDirection,
                    sourceCharacter,
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
            EffectLifetimeType lifetimeType = EffectLifetimeType.Instant,
            float duration = 0f,
            EffectCategoryType categoryType = EffectCategoryType.Neutral)
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
            EffectLifetimeType lifetimeType = EffectLifetimeType.Instant,
            float duration = 0f,
            EffectCategoryType categoryType = EffectCategoryType.Neutral)
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