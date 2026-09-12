using Skill;

namespace Character.Helper.Skill
{
    public static class CharacterSkillHelper
    {
        public static string GetSkillId(
            EquipmentSkillRuntimeData skillRuntime)
        {
            if (skillRuntime == null)
            {
                return string.Empty;
            }

            if (skillRuntime.instanceData != null &&
                !string.IsNullOrEmpty(skillRuntime.instanceData.equipmentId))
            {
                return skillRuntime.instanceData.equipmentId;
            }

            if (skillRuntime.sourceEquipment != null &&
                !string.IsNullOrEmpty(skillRuntime.sourceEquipment.EquipmentId))
            {
                return skillRuntime.sourceEquipment.EquipmentId;
            }

            return skillRuntime.GetHashCode().ToString();
        }

        public static float GetCooldown(
            EquipmentSkillRuntimeData skillRuntime)
        {
            if (skillRuntime == null ||
                skillRuntime.sourceEquipment == null ||
                skillRuntime.sourceEquipment.CastSo == null)
            {
                return 0f;
            }

            float authored = skillRuntime.sourceEquipment.CastSo.Cooldown;
            if (skillRuntime.resolvedLevel < 4) return authored;
            string id = GetSkillId(skillRuntime);
            if (id == Character.Skill.SeojinJangdanRuntime.DungId) return 5.5f;
            if (id == Character.Skill.SeojinJangdanRuntime.GiId ||
                id == Character.Skill.SeojinJangdanRuntime.DeokId) return 3.5f;
            if (id == Character.Skill.SeojinJangdanRuntime.SequenceId) return 7.25f;
            return authored;
        }
    }
}
