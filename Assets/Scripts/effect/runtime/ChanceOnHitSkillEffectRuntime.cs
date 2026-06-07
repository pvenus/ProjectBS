using UnityEngine;
using Character;
using Character.Skill;

namespace Effect
{
    /// <summary>
    /// 공격 적중 시 확률적으로 스킬을 발동한다.
    /// chance 값은 0~100(%) 기준이다.
    /// 실제 OnHit 이벤트 연결은 Damage/Hit 시스템에서 호출한다.
    /// </summary>
    public class ChanceOnHitSkillEffectRuntime : EffectRuntimeData
    {
        private readonly ChanceOnHitSkillEffectSO effectSO;
        private readonly CharacterManager targetCharacter;
        private readonly CharacterManager sourceCharacter;

        private EquipmentSkillRuntimeData cachedSkillRuntime;

        public ChanceOnHitSkillEffectRuntime(
            ChanceOnHitSkillEffectSO effectSO,
            CharacterManager targetCharacter,
            CharacterManager sourceCharacter)
        {
            this.effectSO = effectSO;
            this.targetCharacter = targetCharacter;
            this.sourceCharacter = sourceCharacter;

            RuntimeId =
                $"ChanceOnHitSkill_{effectSO.effectId}_{GetTargetRuntimeId()}";
        }

        private string GetTargetRuntimeId()
        {
            if (targetCharacter == null)
            {
                return "None";
            }

            return targetCharacter.GetInstanceID().ToString();
        }

        public override void OnApply()
        {
            if (effectSO == null || effectSO.skillSo == null)
            {
                IsActive = false;
                return;
            }

            cachedSkillRuntime = ResolveSkillRuntime();

            if (cachedSkillRuntime == null)
            {
                IsActive = false;
            }
        }

        public override void OnRemove()
        {
            cachedSkillRuntime = null;
        }

        /// <summary>
        /// 공격 적중 시 호출.
        /// </summary>
        public void OnHit(
            CharacterManager attacker,
            CharacterManager target,
            bool isCritical)
        {
            if (!IsActive)
            {
                return;
            }

            if (effectSO == null || effectSO.skillSo == null)
            {
                return;
            }

            if (effectSO.requireCriticalHit && !isCritical)
            {
                return;
            }

            float chance = Mathf.Clamp(effectSO.chance, 0f, 100f);

            if (chance <= 0f)
            {
                return;
            }

            if (Random.Range(0f, 100f) > chance)
            {
                return;
            }

            CharacterManager caster =
                attacker != null
                    ? attacker
                    : sourceCharacter;

            if (caster == null)
            {
                return;
            }

            CharacterSkillManager skillManager =
                caster.GetComponent<CharacterSkillManager>()
                ?? caster.GetComponentInChildren<CharacterSkillManager>();

            if (skillManager == null)
            {
                return;
            }

            EquipmentSkillRuntimeData skillRuntime =
                cachedSkillRuntime ?? ResolveSkillRuntime();

            if (skillRuntime == null)
            {
                return;
            }

            cachedSkillRuntime = skillRuntime;

            skillManager.FireSkill(
                skillRuntime,
                caster.transform,
                target != null ? target.transform : null);
        }

        private EquipmentSkillRuntimeData ResolveSkillRuntime()
        {
            if (effectSO == null || effectSO.skillSo == null)
            {
                return null;
            }

            EquipmentSkillResolver resolver =
                new EquipmentSkillResolver();

            return resolver.Resolve(
                effectSO.skillSo,
                new EquipmentSkillInstanceData
                {
                    equipmentId = effectSO.skillSo.EquipmentId,
                    projectilePrefab = null,
                    projectileLifetimeOverride = -1f
                });
        }
    }
}
