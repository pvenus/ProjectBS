using System;
using System.Collections.Generic;
using UnityEngine;
using Skill;
using Character.Runtime.Skill;
using Character.Skill;
using Effect;
using Effect.Helper;

namespace Character
{
    /// <summary>
    /// Character skill selection / cooldown manager.
    ///
    /// Current temporary policy:
    /// - SkillPoolRuntimeData is the source of truth.
    /// - Active skill selection is handled separately from passive skill application.
    /// - After a skill is used, start its cooldown.
    ///
    /// Later this class can become the main skill decision manager by replacing
    /// SelectReadyActiveSkill() with smarter AI / priority / context based logic.
    /// </summary>
    public class CharacterSkillManager : MonoBehaviour
    {
        [Header("Skill Runtime")]
        [SerializeField] private CharacterSkillRuntimeData skillRuntimeData = new();

        private readonly ActiveSkillService skillService = new();
        private readonly PassiveSkillService passiveSkillService = new();
        private readonly CastMoveService castMoveService = new();

        private int skillExecutionLockCount;

        public bool IsSkillExecuting => skillExecutionLockCount > 0;

        public CharacterSkillRuntimeData SkillRuntimeData => skillRuntimeData;
        public SkillPoolRuntimeData SkillPool => skillRuntimeData?.skillPool;

        public void SetSkillRuntimeData(CharacterSkillRuntimeData newSkillRuntimeData)
        {
            skillRuntimeData = newSkillRuntimeData ?? new CharacterSkillRuntimeData();

            if (skillRuntimeData.skillPool == null)
            {
                skillRuntimeData.skillPool = new SkillPoolRuntimeData();
            }

            EnsureRuntimeData();
        }

        public void SetSkillPool(SkillPoolRuntimeData newSkillPool)
        {
            EnsureRuntimeData();
            skillRuntimeData.skillPool = newSkillPool ?? new SkillPoolRuntimeData();
            EnsureRuntimeData();
        }

        public void InitializeSkills(CharacterSO characterSO)
        {
            EnsureRuntimeData();

            SkillPoolRuntimeData skillPool = new SkillPoolRuntimeData();

            SkillPoolOverrideSO skillOverrideSet = characterSO != null
                ? characterSO.SkillOverrideSet
                : null;

            if (skillOverrideSet == null)
            {
                return;
            }

            for (int i = 0; i < skillOverrideSet.overrides.Count; i++)
            {
                SkillPoolOverrideEntry entry = skillOverrideSet.overrides[i];
                if (entry == null || entry.skillSo == null)
                {
                    continue;
                }

                SkillPoolSlotData slot = new SkillPoolSlotData();
                slot.Configure(
                    entry.slotKey,
                    entry.skillSo);

                skillPool.AddSlot(slot);
            }

            skillRuntimeData.skillPool = skillPool ?? new SkillPoolRuntimeData();

            EquipmentSkillResolver resolver = new EquipmentSkillResolver();
            CharacterRuntimeData characterRuntimeData = ResolveCharacterRuntimeData();

            if (characterRuntimeData == null)
            {
                Debug.LogError(
                    $"[CharacterSkillManager] CharacterRuntimeData not found. Character={name}");
                return;
            }

            skillRuntimeData.skillPool.ResolveAllSkills(
                resolver,
                characterRuntimeData);

            ApplyPassiveSkills();
        }

        private CharacterRuntimeData ResolveCharacterRuntimeData()
        {
            CharacterManager characterManager =
                GetComponent<CharacterManager>()
                ?? GetComponentInChildren<CharacterManager>();

            return characterManager?.RuntimeData;
        }

        public EquipmentSkillRuntimeData SelectReadyActiveSkill()
        {
            return skillService.SelectActiveSkill(this);
        }

        public EquipmentSkillRuntimeData GetRuntimeBySkill(EquipmentSkillSO skillSO)
        {
            if (skillSO == null || SkillPool == null || SkillPool.Slots == null)
            {
                return null;
            }

            for (int i = 0; i < SkillPool.Slots.Count; i++)
            {
                SkillPoolSlotData slot = SkillPool.GetSlot(i);

                if (slot == null || slot.SkillSo == null || slot.RuntimeData == null)
                {
                    continue;
                }

                if (slot.SkillSo == skillSO)
                {
                    return slot.RuntimeData;
                }
            }

            return null;
        }

        public void BeginSkillExecution()
        {
            //skillExecutionLockCount++;
        }

        public void EndSkillExecution()
        {
            //skillExecutionLockCount = Mathf.Max(
            //    0,
            //    skillExecutionLockCount - 1);
        }

        public bool FireSkill(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            BeginSkillExecution();

            TryStartCastMove(
                runtime,
                caster,
                target);

            bool started = skillService.FireSkill(
                this,
                runtime,
                caster,
                target);

            if (!started)
            {
                EndSkillExecution();
            }

            return started;
        }

        private void TryStartCastMove(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            SkillCastMoveSO castMoveSo = ResolveCastMoveSo(runtime);

            if (castMoveSo == null || !castMoveSo.HasMove)
            {
                return;
            }

            Transform resolvedCaster = caster != null
                ? caster
                : transform;

            Vector2 castDirection = ResolveCastDirection(
                resolvedCaster,
                target);

            castMoveService.TryStartMove(
                this,
                resolvedCaster,
                target,
                castDirection,
                castMoveSo);
        }

        private SkillCastMoveSO ResolveCastMoveSo(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null ||
                runtime.sourceEquipment == null ||
                runtime.sourceEquipment.CastSo == null)
            {
                return null;
            }

            return runtime.sourceEquipment.CastSo.CastMove;
        }

        private Vector2 ResolveCastDirection(
            Transform caster,
            Transform target)
        {
            if (caster != null && target != null)
            {
                Vector2 direction = target.position - caster.position;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    return direction.normalized;
                }
            }

            if (caster != null)
            {
                Vector2 right = caster.right;

                if (right.sqrMagnitude > 0.0001f)
                {
                    return right.normalized;
                }
            }

            return Vector2.right;
        }

        public List<SkillProjectileHitEffectEntry> GetPassiveEffects()
        {
            return passiveSkillService.GetAllPassiveEffects(this);
        }

        public List<EquipmentSkillRuntimeData> GetPassiveSkills()
        {
            return passiveSkillService.GetPassiveSkills(this);
        }

        public EquipmentSkillRuntimeData[] GetAllRuntimes()
        {
            if (SkillPool == null || SkillPool.Slots == null)
            {
                return Array.Empty<EquipmentSkillRuntimeData>();
            }

            List<EquipmentSkillRuntimeData> result = new();

            for (int i = 0; i < SkillPool.Slots.Count; i++)
            {
                SkillPoolSlotData slot = SkillPool.GetSlot(i);

                if (slot == null || slot.RuntimeData == null)
                {
                    continue;
                }

                result.Add(slot.RuntimeData);
            }

            return result.ToArray();
        }

        public EquipmentSkillRuntimeData[] GetActiveRuntimes()
        {
            if (SkillPool == null || SkillPool.Slots == null)
            {
                return Array.Empty<EquipmentSkillRuntimeData>();
            }

            List<EquipmentSkillRuntimeData> result = new();

            AddRuntimeBySlotKey(result, SkillPoolSlotKeys.BasicAttack);
            AddRuntimeBySlotKey(result, SkillPoolSlotKeys.Active1);
            AddRuntimeBySlotKey(result, SkillPoolSlotKeys.Active2);
            AddRuntimeBySlotKey(result, SkillPoolSlotKeys.Active3);

            return result.ToArray();
        }

        private void AddRuntimeBySlotKey(
            List<EquipmentSkillRuntimeData> result,
            string slotKey)
        {
            if (result == null || string.IsNullOrEmpty(slotKey) || SkillPool == null)
            {
                return;
            }

            for (int i = 0; i < SkillPool.Slots.Count; i++)
            {
                SkillPoolSlotData slot = SkillPool.GetSlot(i);

                if (slot == null || slot.RuntimeData == null)
                {
                    continue;
                }

                if (string.Equals(slot.SlotKey, slotKey, StringComparison.Ordinal))
                {
                    result.Add(slot.RuntimeData);
                    return;
                }
            }
        }

        private void ApplyPassiveSkills()
        {
            List<EquipmentSkillRuntimeData> passiveSkills =
                passiveSkillService.GetPassiveSkills(this);

            if (passiveSkills == null || passiveSkills.Count == 0)
            {
                return;
            }

            CharacterManager characterManager =
                GetComponent<CharacterManager>();

            if (characterManager == null)
            {
                return;
            }

            EffectManager effectManager =
                GetComponent<EffectManager>()
                ?? GetComponentInChildren<EffectManager>();

            if (effectManager == null)
            {
                return;
            }

            for (int i = 0; i < passiveSkills.Count; i++)
            {
                EquipmentSkillRuntimeData skillRuntime =
                    passiveSkills[i];

                if (skillRuntime == null)
                {
                    continue;
                }

                List<SkillProjectileHitEffectEntry> passiveEffects =
                    passiveSkillService.GetPassiveEffects(skillRuntime);

                if (passiveEffects == null || passiveEffects.Count == 0)
                {
                    continue;
                }

                string sourceId = skillRuntime.sourceEquipment != null
                    ? skillRuntime.sourceEquipment.EquipmentId
                    : "PassiveSkill";

                for (int j = 0; j < passiveEffects.Count; j++)
                {
                    SkillProjectileHitEffectEntry entry =
                        passiveEffects[j];

                    if (entry == null || entry.effectSo == null)
                    {
                        continue;
                    }

                    EffectApplyHelper.ApplyEffect(
                        effectManager,
                        characterManager,
                        entry.effectSo,
                        EffectSourceType.Skill,
                        sourceId,
                        entry.lifetimeType,
                        entry.duration,
                        entry.categoryType,
                        transform,
                        Vector2.zero,
                        characterManager);
                }
            }
        }

        private void Awake()
        {
            EnsureRuntimeData();
        }

        private void EnsureRuntimeData()
        {
            if (skillRuntimeData == null)
            {
                skillRuntimeData = new CharacterSkillRuntimeData();
            }

            if (skillRuntimeData.skillPool == null)
            {
                skillRuntimeData.skillPool = new SkillPoolRuntimeData();
            }
        }
    }
}