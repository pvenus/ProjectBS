using Skills.Dto;
using UnityEngine;
using Skill;
[CreateAssetMenu(fileName = "SkillDamageSO", menuName = "Game/Skills/Damage/SkillDamageSO")]
public class SkillDamageSO : ScriptableObject
{
    [Header("Skill")]
    [SerializeField] private string skillId = "skill_damage";
    [SerializeField] private DamageType damageType = DamageType.Normal;

    [Header("Damage")]
    // 기본 데미지: Heat 스케일링 및 최종 데미지의 기준이 되는 값
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float firstHitBaseDamage = 0f;

    // 공격력 계수 기반 추가 데미지
    [SerializeField] private float attackPercentDamage = 1f;

    [Header("Critical / Modifiers")]
    [SerializeField] private bool canCritical = false;
    [SerializeField] private bool ignoreDefense = false;

    public string SkillId => skillId;
    public DamageType DamageType => damageType;
    public float BaseDamage => baseDamage;
    public float FirstHitBaseDamage => firstHitBaseDamage;
    public float AttackPercentDamage => attackPercentDamage;
    public bool CanCritical => canCritical;
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
            baseDamage = Mathf.Max(0f, baseDamage),
            firstHitBaseDamage = Mathf.Max(0f, firstHitBaseDamage),
            attackDamagePercent = Mathf.Max(0f, attackPercentDamage),
            canCritical = canCritical,
            ignoreDefense = ignoreDefense
        };
    }
}