

using Skills.Dto;
using Status.Service;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDamageSO", menuName = "BS/Skills/Damage/SkillDamageSO")]
public class SkillDamageSO : ScriptableObject
{
    [Header("Skill")]
    [SerializeField] private string skillId = "skill_damage";
    [SerializeField] private DamageType damageType = DamageType.Normal;
    [SerializeField] private ElementType elementType = ElementType.None;

    [Header("Damage")]
    // 기본 데미지: Heat 스케일링 및 최종 데미지의 기준이 되는 값
    [SerializeField] private float baseDamage = 10f;

    // 고정 추가 데미지 (특수 보정용, 기본적으로는 최소 사용 권장)
    [SerializeField] private float flatBonusDamage = 0f;

    [Header("Fire / Heat")]
    // heatGain: Heat를 얼마나 잘 쌓는가 (Generator 성능)
    [SerializeField] private float heatGain = 0f;

    // heatCoefficient: 쌓인 Heat를 데미지로 얼마나 잘 변환하는가 (Finisher 성능)
    [SerializeField] private float heatCoefficient = 0f;

    [SerializeField] private bool canTriggerOverheat = true;

    [Header("Critical / Modifiers")]
    [SerializeField] private bool canCritical = false;
    [SerializeField] private float criticalMultiplier = 1.5f;
    [SerializeField] private bool ignoreDefense = false;

    public string SkillId => skillId;
    public DamageType DamageType => damageType;
    public ElementType ElementType => elementType;
    public float BaseDamage => baseDamage;
    public float FlatBonusDamage => flatBonusDamage;
    public float HeatCoefficient => heatCoefficient;
    public float HeatGain => heatGain;
    public bool CanTriggerOverheat => canTriggerOverheat;
    public bool CanCritical => canCritical;
    public float CriticalMultiplier => criticalMultiplier;
    public bool IgnoreDefense => ignoreDefense;

    /// <summary>
    /// SO 설정값을 전투용 데미지 프로필 DTO로 변환한다.
    /// 현재는 업그레이드 수치를 직접 반영하지 않고 원본 값을 그대로 전달한다.
    /// 필요 시 이후 upgradeData 기반 보정을 여기에 추가한다.
    /// </summary>
    public SkillDamageProfileDto CreateDto()
    {
        return new SkillDamageProfileDto
        {
            skillId = skillId,
            damageType = damageType,
            elementType = elementType,
            baseDamage = Mathf.Max(0f, baseDamage),
            flatBonusDamage = flatBonusDamage,
            heatCoefficient = Mathf.Max(0f, heatCoefficient),
            heatGain = Mathf.Max(0f, heatGain),
            canTriggerOverheat = canTriggerOverheat,
            canCritical = canCritical,
            criticalMultiplier = Mathf.Max(1f, criticalMultiplier),
            ignoreDefense = ignoreDefense
        };
    }
}