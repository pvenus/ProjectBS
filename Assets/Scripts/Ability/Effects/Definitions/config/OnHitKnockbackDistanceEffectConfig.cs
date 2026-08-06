using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class OnHitKnockbackDistanceEffectConfig : EffectConfig
    {
        [SerializeField] private float chancePercent = 100f;
        [SerializeField] private float distanceMeters;
        [SerializeField] private KnockbackDirectionType directionType =
            KnockbackDirectionType.PushAwayFromSource;

        public float ChancePercent => chancePercent;
        public float DistanceMeters => distanceMeters;
        public KnockbackDirectionType DirectionType => directionType;

        public void ApplyEditorData(
            float chancePercent,
            float distanceMeters,
            KnockbackDirectionType directionType)
        {
            this.chancePercent = chancePercent;
            this.distanceMeters = distanceMeters;
            this.directionType = directionType;
        }
    }
}
