using Skill.Service.Helper;
using System.Collections.Generic;

namespace Character.Skill
{
    /// <summary>
    /// Passive skill business logic.
    ///
    /// Passive skill rule:
    /// - Only runtime.sourceEquipment.BaseProfileSo.SkillType.Passive is handled.
    /// - Passive self effects are applied through the same cast self-effect path
    ///   used by active skills.
    /// - Legacy runtime.sourceEquipment.HitSos[].buffEffects is intentionally not
    ///   used here anymore.
    /// </summary>
    public class PassiveSkillService
    {
        public List<EquipmentSkillRuntimeData> GetPassiveSkills(
            CharacterSkillManager skillManager)
        {
            List<EquipmentSkillRuntimeData> result = new();

            if (skillManager == null)
            {
                return result;
            }

            EquipmentSkillRuntimeData[] runtimes =
                skillManager.GetAllRuntimes();

            if (runtimes == null || runtimes.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < runtimes.Length; i++)
            {
                EquipmentSkillRuntimeData runtime = runtimes[i];

                if (!IsPassiveSkill(runtime))
                {
                    continue;
                }

                result.Add(runtime);
            }

            return result;
        }

        public bool IsPassiveSkill(
            EquipmentSkillRuntimeData runtime)
        {
            return runtime != null &&
                   runtime.sourceEquipment != null &&
                   runtime.sourceEquipment.BaseProfileSo != null &&
                   runtime.sourceEquipment.BaseProfileSo.SkillType == global::Skill.SkillType.Passive;
        }

        public bool HasPassiveEffects(
            EquipmentSkillRuntimeData runtime)
        {
            if (!IsPassiveSkill(runtime) ||
                runtime.sourceEquipment == null ||
                runtime.sourceEquipment.CastSo == null ||
                runtime.sourceEquipment.CastSo.SelfEffects == null)
            {
                return false;
            }

            return runtime.sourceEquipment.CastSo.SelfEffects.Length > 0;
        }

        public void ApplyPassiveSkills(
            CharacterSkillManager skillManager,
            CharacterManager ownerCharacter)
        {
            if (skillManager == null || ownerCharacter == null)
            {
                return;
            }

            List<EquipmentSkillRuntimeData> passiveSkills =
                GetPassiveSkills(skillManager);

            for (int i = 0; i < passiveSkills.Count; i++)
            {
                EquipmentSkillRuntimeData runtime = passiveSkills[i];

                if (!HasPassiveEffects(runtime))
                {
                    continue;
                }

                SkillUseHelper.ApplyCastSelfEffects(runtime, ownerCharacter.gameObject);
            }
        }
    }
}