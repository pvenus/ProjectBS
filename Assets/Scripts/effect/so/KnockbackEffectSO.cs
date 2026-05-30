

using UnityEngine;

namespace Effect
{
    public enum KnockbackDirectionType
    {
        /// <summary>
        /// 중심에서 바깥 방향으로 밀어냄.
        /// </summary>
        PushAwayFromSource = 0,

        /// <summary>
        /// 중심점 방향으로 끌어당김.
        /// </summary>
        PullToSource = 1,

        ProjectileDirection = 2,
        CustomDirection = 3,
    }

    [CreateAssetMenu(
        fileName = "KnockbackEffect",
        menuName = "Effect/Knockback Effect")]
    public class KnockbackEffectSO : EffectSO
    {
        [Header("Knockback")]
        [Min(0f)]
        public float force = 5f;

        [Tooltip("PushAwayFromSource = 바깥 방향 넉백, PullToSource = 중심점 끌어당김")]
        public KnockbackDirectionType directionType =
            KnockbackDirectionType.PushAwayFromSource;

        [Tooltip("directionType이 CustomDirection일 때 사용할 방향")]
        public Vector2 customDirection = Vector2.up;

        [Tooltip("방향 벡터를 정규화해서 사용할지 여부")]
        public bool normalizeDirection = true;

        [Tooltip("넉백 방향 계산에 사용할 source가 없을 때 fallback으로 projectile direction을 사용할지 여부")]
        public bool fallbackToProjectileDirection = true;
    }
}