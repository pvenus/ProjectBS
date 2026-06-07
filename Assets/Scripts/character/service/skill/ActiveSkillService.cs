using Character.Helper.Skill;
using Character.Runtime.Skill;
using Skill.Service.Helper;
using UnityEngine;
using Skill;
using System.Collections;

namespace Character.Skill
{
    /// <summary>
    /// Character skill selection service.
    ///
    /// Current policy:
    /// - Select the first skill whose cooldown is ready.
    ///
    /// Future expansion:
    /// - Priority based selection
    /// - Distance based selection
    /// - HP based selection
    /// - Target count based selection
    /// - AI/Brain based selection
    /// - Tactical scoring
    ///
    /// Keep all selection logic inside this service.
    /// </summary>
    public class ActiveSkillService
    {
        /// <summary>
        /// Selects an active skill that is ready to use.
        /// Passive skills are excluded from this selection flow.
        /// </summary>
        public EquipmentSkillRuntimeData SelectActiveSkill(
            CharacterSkillManager skillManager)
        {
            if (skillManager == null)
            {
                return null;
            }

            EquipmentSkillRuntimeData[] runtimes =
                skillManager.GetActiveRuntimes();

            if (runtimes == null || runtimes.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < runtimes.Length; i++)
            {
                EquipmentSkillRuntimeData runtime = runtimes[i];

                if (runtime == null)
                {
                    continue;
                }

                string skillId = CharacterSkillHelper.GetSkillId(runtime);

                if (!IsCooldownReady(skillManager.SkillRuntimeData, skillId))
                {
                    continue;
                }

                return runtime;
            }

            return null;
        }
        public EquipmentSkillRuntimeData SelectReadySkill(
            CharacterSkillManager skillManager)
        {
            return SelectActiveSkill(skillManager);
        }
        /// <summary>
        /// Fires a specific runtime skill through SkillExecutorMono.
        /// Cooldown starts only when the executor successfully fires the skill.
        /// </summary>
        public bool FireSkill(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            if (skillManager == null || caster == null)
            {
                return false;
            }

            if (!CanFireRuntime(runtime, caster))
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);

            if (!IsCooldownReady(skillManager.SkillRuntimeData, skillId))
            {
                return false;
            }

            return StartSkillUseRoutine(
                skillManager,
                runtime,
                caster,
                target,
                false,
                Vector2.zero);
        }
        /// <summary>
        /// Marks a skill as actually used.
        /// Call this only after the executor successfully fires the skill.
        /// </summary>
        public bool UseSkill(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime)
        {
            if (skillManager == null || runtime == null)
            {
                return false;
            }

            CharacterSkillRuntimeData runtimeData = skillManager.SkillRuntimeData;

            if (runtimeData == null)
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);

            if (string.IsNullOrEmpty(skillId))
            {
                return false;
            }

            StartCooldownInternal(
                runtimeData,
                skillId,
                CharacterSkillHelper.GetCooldown(runtime));

            return true;
        }

        private bool StartSkillUseRoutine(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            if (skillManager == null || runtime == null || caster == null)
            {
                return false;
            }

            bool isSelfOrNoTargetSkill = IsSelfOrNoTargetSkill(runtime);
            Vector2 resolvedTargetPoint = ResolveTargetPoint(
                runtime,
                caster,
                target,
                usePoint,
                targetPoint);

            Vector2 spawnPosition = caster.position;
            Vector2 direction = ResolveDirection(
                spawnPosition,
                caster,
                isSelfOrNoTargetSkill ? null : target,
                isSelfOrNoTargetSkill || usePoint,
                resolvedTargetPoint);

            if (ShouldPlayAttackAnimation(runtime))
            {
                ApplyAnimationDirection(caster, direction);
                PlayAttackAnimation(caster);

                skillManager.StartCoroutine(
                    FireSkillAtAttackTiming(
                        skillManager,
                        runtime,
                        caster,
                        isSelfOrNoTargetSkill ? null : target,
                        isSelfOrNoTargetSkill || usePoint,
                        resolvedTargetPoint));

                return true;
            }

            bool fired = SkillUseHelper.UseSkillProjectilesAndSelfEffects(
                runtime,
                caster,
                isSelfOrNoTargetSkill ? null : target,
                isSelfOrNoTargetSkill || usePoint,
                resolvedTargetPoint);

            if (fired)
            {
                UseSkill(skillManager, runtime);
            }

            return fired;
        }

        private bool ShouldPlayAttackAnimation(
            EquipmentSkillRuntimeData runtime)
        {
            return runtime != null &&
                   runtime.sourceEquipment != null &&
                   !runtime.skipAttackAnimation &&
                   !HasCastMove(runtime);
        }

        private bool HasCastMove(
            EquipmentSkillRuntimeData runtime)
        {
            return runtime != null &&
                   runtime.castSo != null &&
                   runtime.castSo.CastMove != null &&
                   runtime.castSo.CastMove.HasMove;
        }

        private IEnumerator FireSkillAtAttackTiming(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            yield return null;

            AnimationMono animationMono = ResolveAnimation(caster);
            float delay = ResolveAttackFireDelay(animationMono);

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            bool fired = SkillUseHelper.UseSkillProjectilesAndSelfEffects(
                runtime,
                caster,
                target,
                usePoint,
                targetPoint);

            if (fired)
            {
                UseSkill(skillManager, runtime);
            }
        }

        private float ResolveAttackFireDelay(
            AnimationMono animationMono)
        {
            if (animationMono == null)
            {
                return 0f;
            }

            return animationMono.GetAttackFireDelay(2f / 3f);
        }

        private Vector2 ResolveDirection(
            Vector2 spawnPosition,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            Vector2 direction = caster != null
                ? (Vector2)caster.right
                : Vector2.right;

            if (target != null)
            {
                Vector2 toTarget = (Vector2)target.position - spawnPosition;

                if (toTarget.sqrMagnitude > 0.0001f)
                {
                    direction = toTarget.normalized;
                }
            }
            else if (usePoint)
            {
                Vector2 toPoint = targetPoint - spawnPosition;

                if (toPoint.sqrMagnitude > 0.0001f)
                {
                    direction = toPoint.normalized;
                }
            }

            return direction;
        }

        private Vector2 ResolveTargetPoint(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint)
        {
            if (runtime == null || runtime.castSo == null)
            {
                return usePoint
                    ? targetPoint
                    : caster != null
                        ? (Vector2)caster.position
                        : Vector2.zero;
            }

            TargetingType targetingType = runtime.castSo.TargetingType;

            if (targetingType == TargetingType.Self ||
                targetingType == TargetingType.None)
            {
                return caster != null
                    ? (Vector2)caster.position
                    : Vector2.zero;
            }

            if (usePoint)
            {
                return targetPoint;
            }

            if (target != null)
            {
                return target.position;
            }

            return caster != null
                ? (Vector2)caster.position
                : Vector2.zero;
        }

        private bool IsSelfOrNoTargetSkill(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null || runtime.castSo == null)
            {
                return false;
            }

            TargetingType targetingType = runtime.castSo.TargetingType;

            return targetingType == TargetingType.Self ||
                   targetingType == TargetingType.None;
        }

        private void ApplyAnimationDirection(
            Transform caster,
            Vector2 direction)
        {
            if (caster == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            AnimationMono animationMono =
                ResolveAnimation(caster);

            animationMono?.SetDirectionFromVector(direction);
        }

        private void PlayAttackAnimation(
            Transform caster)
        {
            AnimationMono animationMono =
                ResolveAnimation(caster);

            animationMono?.PlayAttack();
        }

        private AnimationMono ResolveAnimation(
            Transform caster)
        {
            if (caster == null)
            {
                return null;
            }

            return caster.GetComponent<AnimationMono>()
                ?? caster.GetComponentInParent<AnimationMono>()
                ?? caster.GetComponentInChildren<AnimationMono>();
        }



        private bool CanFireRuntime(
            EquipmentSkillRuntimeData runtime,
            Transform caster)
        {
            if (runtime == null || runtime.sourceEquipment == null || caster == null)
            {
                return false;
            }

            CharacterManager characterManager =
                caster.GetComponent<CharacterManager>()
                ?? caster.GetComponentInParent<CharacterManager>()
                ?? caster.GetComponentInChildren<CharacterManager>();

            AnimationMono animationMono =
                ResolveAnimation(caster);

            if (!runtime.skipAttackAnimation
                && animationMono != null
                && animationMono.IsPlayingAttack())
            {
                return false;
            }

            return characterManager == null || characterManager.CanUseSkill;
        }

        private bool IsCooldownReady(
            CharacterSkillRuntimeData runtimeData,
            string skillId)
        {
            if (runtimeData == null || string.IsNullOrEmpty(skillId))
            {
                return false;
            }

            if (!runtimeData.cooldownEndTimes.TryGetValue(skillId, out float cooldownEndTime))
            {
                return true;
            }

            return Time.time >= cooldownEndTime;
        }

        private void StartCooldownInternal(
            CharacterSkillRuntimeData runtimeData,
            string skillId,
            float cooldown)
        {
            if (runtimeData == null || string.IsNullOrEmpty(skillId))
            {
                return;
            }

            runtimeData.cooldownEndTimes[skillId] =
                Time.time + Mathf.Max(0f, cooldown);
        }
    }
}