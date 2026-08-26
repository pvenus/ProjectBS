using Skill;
using UnityEngine;

namespace Character.Skill
{
    /// <summary>
    /// 캐릭터 상태 머신에서 기본 공격과 액티브 스킬의 사용 간격을 관리한다.
    /// </summary>
    public sealed class CharacterStateService
    {
        private const int RequiredBasicAttackCount = 1;
        private const float ActiveSkillWaitSeconds = 2f;

        private int successfulBasicAttackCount;
        private bool hasUsedActiveSkill;
        private float lastActiveSkillUsedTime;

        public bool CanUseActiveSkill
        {
            get
            {
                if (successfulBasicAttackCount >= RequiredBasicAttackCount)
                {
                    return true;
                }

                // 첫 액티브는 시간으로 우회하지 않고 기본 공격 1회를 반드시 요구한다.
                return hasUsedActiveSkill &&
                       Time.time - lastActiveSkillUsedTime >= ActiveSkillWaitSeconds;
            }
        }

        public void RecordSuccessfulSkillUse(
            EquipmentSkillRuntimeData runtime,
            CharacterSkillManager skillManager)
        {
            if (runtime == null || skillManager == null)
            {
                return;
            }

            EquipmentSkillRuntimeData basicAttack =
                skillManager.SkillPool?.GetRuntimeByKey(
                    SkillPoolSlotKeys.BasicAttack);

            if (runtime == basicAttack)
            {
                successfulBasicAttackCount++;
                return;
            }

            successfulBasicAttackCount = 0;
            hasUsedActiveSkill = true;
            lastActiveSkillUsedTime = Time.time;
        }
    }
}
