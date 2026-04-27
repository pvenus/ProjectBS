using UnityEngine;

/// <summary>
/// 스킬이 언제, 어떤 방식으로 발동되는지를 정의하는 SO.
/// 데미지나 속성, 업그레이드 결과는 포함하지 않고
/// 순수하게 발동 구조만 담당한다.
/// </summary>
[CreateAssetMenu(fileName = "SkillCastSO", menuName = "BS/Skills/Cast/SkillCastSO")]
public class SkillCastSO : ScriptableObject
{
    [Header("Timing")]
    [SerializeField, Min(0f)] private float cooldown = 1f;
    [SerializeField, Min(0f)] private float castTime = 0f;
    [SerializeField, Min(0f)] private float range = 5f;

    [Header("Burst / Repeat")]
    [SerializeField, Min(1)] private int burstCount = 1;
    [SerializeField, Min(0f)] private float burstInterval = 0f;

    [Header("Cast Settings")]
    [SerializeField] private CastType castType = CastType.Instant;
    [SerializeField] private TargetingType targetingType = TargetingType.AutoTarget;

    [Header("Flags")]
    [SerializeField] private bool canMoveWhileCasting = true;
    [SerializeField] private bool canCancelCasting = true;
    [SerializeField] private bool autoCast = true;

    public float Cooldown => cooldown;
    public float CastTime => castTime;
    public float Range => range;
    public int BurstCount => burstCount;
    public float BurstInterval => burstInterval;

    public CastType CastType => castType;
    public TargetingType TargetingType => targetingType;

    public bool CanMoveWhileCasting => canMoveWhileCasting;
    public bool CanCancelCasting => canCancelCasting;
    public bool AutoCast => autoCast;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (burstCount < 1)
        {
            burstCount = 1;
        }

        if (cooldown < 0f)
        {
            cooldown = 0f;
        }

        if (castTime < 0f)
        {
            castTime = 0f;
        }

        if (burstInterval < 0f)
        {
            burstInterval = 0f;
        }

        if (range < 0f)
        {
            range = 0f;
        }
    }
#endif
}