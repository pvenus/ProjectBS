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

            return skillRuntime.sourceEquipment.CastSo.Cooldown;
        }
    }
}