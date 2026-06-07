using System.Collections.Generic;
using Effect;

namespace Character.Skill
{
    /// <summary>
    /// Passive skill business logic.
    ///
    /// Passive skill rule:
    /// - Only EquipmentSkillRuntimeData with SkillType.Passive is handled.
    /// - Passive effects are read from runtime.hitSos[].buffEffects.
    /// - This service is responsible for selecting passive skill runtimes and
    ///   exposing the effect list to the caller that actually applies effects.
    ///
    /// Effect application/removal is intentionally not hard-wired here yet,
    /// because EffectManager target/source policy is still shared with relic/bless logic.
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
                   runtime.skillType == global::Skill.SkillType.Passive;
        }

        public bool HasPassiveEffects(
            EquipmentSkillRuntimeData runtime)
        {
            if (!IsPassiveSkill(runtime) ||
                runtime.hitSos == null ||
                runtime.hitSos.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < runtime.hitSos.Length; i++)
            {
                if (runtime.hitSos[i] == null)
                {
                    continue;
                }

                SkillProjectileHitDto hitDto =
                    runtime.hitSos[i].CreateDto(
                        resolvedDamageProfile: null);

                if (hitDto != null &&
                    hitDto.buffEffects != null &&
                    hitDto.buffEffects.Length > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public List<SkillProjectileHitEffectEntry> GetPassiveEffects(
            EquipmentSkillRuntimeData runtime)
        {
            List<SkillProjectileHitEffectEntry> result = new();

            if (!IsPassiveSkill(runtime) ||
                runtime.hitSos == null ||
                runtime.hitSos.Length == 0)
            {
                return result;
            }

            for (int hitIndex = 0; hitIndex < runtime.hitSos.Length; hitIndex++)
            {
                if (runtime.hitSos[hitIndex] == null)
                {
                    continue;
                }

                SkillProjectileHitDto hitDto =
                    runtime.hitSos[hitIndex].CreateDto(
                        resolvedDamageProfile: null);

                if (hitDto == null || hitDto.buffEffects == null)
                {
                    continue;
                }

                for (int i = 0; i < hitDto.buffEffects.Length; i++)
                {
                    SkillProjectileHitEffectEntry effectEntry =
                        hitDto.buffEffects[i];

                    if (effectEntry == null)
                    {
                        continue;
                    }

                    result.Add(effectEntry);
                }
            }

            return result;
        }

        public List<SkillProjectileHitEffectEntry> GetAllPassiveEffects(
            CharacterSkillManager skillManager)
        {
            List<SkillProjectileHitEffectEntry> result = new();
            List<EquipmentSkillRuntimeData> passiveSkills =
                GetPassiveSkills(skillManager);

            for (int i = 0; i < passiveSkills.Count; i++)
            {
                List<SkillProjectileHitEffectEntry> effects =
                    GetPassiveEffects(passiveSkills[i]);

                for (int j = 0; j < effects.Count; j++)
                {
                    result.Add(effects[j]);
                }
            }

            return result;
        }
    }
}
