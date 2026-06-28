using System;
using UnityEngine;

namespace Effect
{
    [Serializable]
    public class KnockbackEffectConfig : EffectConfig
    {
        [Header("Knockback")]
        [SerializeField, Min(0f)]
        private float force = 5f;

        [Tooltip("PushAwayFromSource = 바깥 방향 넉백, PullToSource = 중심점 끌어당김")]
        [SerializeField]
        private KnockbackDirectionType directionType =
            KnockbackDirectionType.PushAwayFromSource;

        [Tooltip("directionType이 CustomDirection일 때 사용할 방향")]
        [SerializeField]
        private Vector2 customDirection = Vector2.up;

        [Tooltip("방향 벡터를 정규화해서 사용할지 여부")]
        [SerializeField]
        private bool normalizeDirection = true;

        [Tooltip("넉백 방향 계산에 사용할 source가 없을 때 fallback으로 projectile direction을 사용할지 여부")]
        [SerializeField]
        private bool fallbackToProjectileDirection = true;

        public float Force => force;
        public KnockbackDirectionType DirectionType => directionType;
        public Vector2 CustomDirection => customDirection;
        public bool NormalizeDirection => normalizeDirection;
        public bool FallbackToProjectileDirection => fallbackToProjectileDirection;

#if UNITY_EDITOR
        public void ApplyEditorData(
            float force,
            KnockbackDirectionType directionType,
            Vector2 customDirection,
            bool normalizeDirection,
            bool fallbackToProjectileDirection)
        {
            this.force = force;
            this.directionType = directionType;
            this.customDirection = customDirection;
            this.normalizeDirection = normalizeDirection;
            this.fallbackToProjectileDirection = fallbackToProjectileDirection;
        }
#endif
    }
}
