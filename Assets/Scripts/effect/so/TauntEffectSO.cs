using UnityEngine;

namespace Effect
{
    /// <summary>
    /// 일정 시간 동안 대상의 타겟을 시전자 또는 지정된 유인 지점으로 고정시키는 도발 효과 설정입니다.
    /// 실제 이동/타겟 변경 처리는 EffectManager 또는 전투 AI 쪽에서 이 설정을 읽어 처리합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "TauntEffectSO", menuName = "Effect/Taunt Effect")]
    public class TauntEffectSO : EffectSO
    {
        [Header("Taunt")]
        [SerializeField]
        private bool forceTargetToCaster = true;

        [SerializeField]
        private bool blockRetargeting = true;

        [SerializeField]
        private bool allowAttack = true;

        [SerializeField]
        private bool allowMovement = true;

        [Header("Pull / Lure Option")]
        [SerializeField]
        private bool useLurePoint;

        [SerializeField]
        private float lureRadius = 0.75f;

        [SerializeField]
        private float lureMoveSpeedMultiplier = 1f;

        public bool ForceTargetToCaster => forceTargetToCaster;
        public bool BlockRetargeting => blockRetargeting;
        public bool AllowAttack => allowAttack;
        public bool AllowMovement => allowMovement;
        public bool UseLurePoint => useLurePoint;
        public float LureRadius => lureRadius;
        public float LureMoveSpeedMultiplier => lureMoveSpeedMultiplier;
    }
}
