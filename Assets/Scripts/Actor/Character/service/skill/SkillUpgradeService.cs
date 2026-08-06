using UnityEngine;

namespace Character.Skill
{
    /// <summary>
    /// 캐릭터 스킬 레벨업 처리 서비스.
    /// UI는 CharacterSkillManager를 호출하고,
    /// CharacterSkillManager가 이 서비스를 통해 레벨 증가와 런타임 갱신을 처리한다.
    /// </summary>
    public class SkillUpgradeService
    {
        public bool TryUpgradeSkill(
            CharacterRuntimeData characterRuntimeData,
            EquipmentSkillInstanceData skillInstance,
            CharacterSkillManager skillManager,
            int maxSkillLevel)
        {
            if (characterRuntimeData == null)
            {
                Debug.LogWarning("[SkillUpgradeService] CharacterRuntimeData is null.");
                return false;
            }

            if (skillInstance == null || string.IsNullOrWhiteSpace(skillInstance.equipmentId))
            {
                Debug.LogWarning("[SkillUpgradeService] SkillInstance is invalid.");
                return false;
            }

            int currentLevel = Mathf.Max(1, skillInstance.currentLevel);
            int resolvedMaxLevel = Mathf.Max(1, maxSkillLevel);

            if (currentLevel >= resolvedMaxLevel)
            {
                return false;
            }

            int nextLevel = Mathf.Min(resolvedMaxLevel, currentLevel + 1);

            characterRuntimeData.SetSkillLevel(
                skillInstance.equipmentId,
                nextLevel);

            skillManager?.RefreshSkillRuntimes();
            return true;
        }
    }
}
