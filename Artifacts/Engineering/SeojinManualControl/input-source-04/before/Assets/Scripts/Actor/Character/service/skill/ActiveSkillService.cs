using Character.Helper.Skill;
using Character.Runtime.Skill;
using Skill.Service.Helper;
using UnityEngine;
using Skill;
using System.Collections;
using System.Collections.Generic;
using Battle.Presentation.SkillFocus;

namespace Character.Skill
{
    public enum OffensiveBlockReason
    {
        None,
        NoOffensiveRuntime,
        Cooldown,
        FailureRetry,
        Cadence,
        CrowdControl
    }

    public readonly struct OffensiveReadinessSnapshot
    {
        public readonly bool hasOffensiveRuntime;
        public readonly bool hasUsableOffensive;
        public readonly string selectedOrSoonestRuntimeId;
        public readonly OffensiveBlockReason blockReason;
        public readonly float remainingCooldownSeconds;
        public readonly bool validTarget;
        public readonly float resolvedRange;
        public readonly float sampleTime;

        public OffensiveReadinessSnapshot(
            bool hasRuntime, bool usable, string runtimeId,
            OffensiveBlockReason reason, float remaining,
            bool targetValid, float range, float time)
        {
            hasOffensiveRuntime = hasRuntime;
            hasUsableOffensive = usable;
            selectedOrSoonestRuntimeId = runtimeId ?? string.Empty;
            blockReason = reason;
            remainingCooldownSeconds = Mathf.Max(0f, remaining);
            validTarget = targetValid;
            resolvedRange = Mathf.Max(0f, range);
            sampleTime = time;
        }
    }

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
        private const float FailedSkillRetryDelay = 0.75f;
        private readonly Dictionary<string, float> failedSkillRetryEndTimes = new();
        private readonly HashSet<int> activeComboCasters = new();
        private static int nextComboToken;
        private readonly Dictionary<int,int> comboGenerations=new();
        public bool HasCombo(Transform caster)=>caster!=null&&activeComboCasters.Contains(caster.GetInstanceID());

        /// <summary>
        /// Selects an active skill that is ready to use.
        /// Passive skills are excluded from this selection flow.
        /// </summary>
        public EquipmentSkillRuntimeData SelectActiveSkill(
            CharacterSkillManager skillManager,
            bool allowActiveSkills = true)
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

            if (!allowActiveSkills)
            {
                EquipmentSkillRuntimeData basicAttack =
                    skillManager.SkillPool?.GetRuntimeByKey(
                        SkillPoolSlotKeys.BasicAttack);

                return IsRuntimeReady(skillManager, basicAttack)
                    ? basicAttack
                    : null;
            }

            for (int i = runtimes.Length - 1; i >= 0; i--)
            {
                EquipmentSkillRuntimeData runtime = runtimes[i];

                if (runtime == null)
                {
                    continue;
                }

                string skillId = CharacterSkillHelper.GetSkillId(runtime);

                if (IsTemporarilyBlockedAfterFailure(skillId))
                {
                    continue;
                }

                if (!IsCooldownReady(skillManager.SkillRuntimeData, skillId))
                {
                    continue;
                }

                return runtime;
            }

            return null;
        }

        public OffensiveReadinessSnapshot QueryOffensiveReadiness(
            CharacterSkillManager skillManager,
            bool allowActiveSkills,
            bool canUseSkill,
            bool validTarget)
        {
            float now = Time.time;
            EquipmentSkillRuntimeData[] runtimes = skillManager != null
                ? skillManager.GetActiveRuntimes()
                : null;
            EquipmentSkillRuntimeData basic = skillManager?.SkillPool?.GetRuntimeByKey(
                SkillPoolSlotKeys.BasicAttack);
            bool hasRuntime = false;
            EquipmentSkillRuntimeData soonest = null;
            float soonestRemaining = float.MaxValue;
            OffensiveBlockReason soonestReason = OffensiveBlockReason.NoOffensiveRuntime;

            if (runtimes != null)
            {
                for (int i = runtimes.Length - 1; i >= 0; i--)
                {
                    EquipmentSkillRuntimeData runtime = runtimes[i];
                    if (!IsOffensiveRuntime(runtime)) continue;
                    hasRuntime = true;

                    bool cadenceBlocked = runtime != basic && !allowActiveSkills;
                    string id = CharacterSkillHelper.GetSkillId(runtime);
                    float cooldownRemaining = GetCooldownRemaining(
                        skillManager.SkillRuntimeData, id, now);
                    float failureRemaining = GetFailureRetryRemaining(id, now);
                    float remaining = Mathf.Max(cooldownRemaining, failureRemaining);
                    if (cadenceBlocked) remaining = Mathf.Max(remaining, 0.10f);

                    OffensiveBlockReason reason = !canUseSkill
                        ? OffensiveBlockReason.CrowdControl
                        : cadenceBlocked
                            ? OffensiveBlockReason.Cadence
                            : failureRemaining > 0f
                                ? OffensiveBlockReason.FailureRetry
                                : cooldownRemaining > 0f
                                    ? OffensiveBlockReason.Cooldown
                                    : OffensiveBlockReason.None;

                    if (reason == OffensiveBlockReason.None)
                    {
                        return new OffensiveReadinessSnapshot(
                            true, true, id, reason, 0f, validTarget,
                            runtime.resolvedRange, now);
                    }

                    if (remaining < soonestRemaining)
                    {
                        soonest = runtime;
                        soonestRemaining = remaining;
                        soonestReason = reason;
                    }
                }
            }

            return new OffensiveReadinessSnapshot(
                hasRuntime,
                false,
                CharacterSkillHelper.GetSkillId(soonest),
                hasRuntime ? soonestReason : OffensiveBlockReason.NoOffensiveRuntime,
                hasRuntime && soonestRemaining < float.MaxValue ? soonestRemaining : 0f,
                validTarget,
                soonest != null ? soonest.resolvedRange : 0f,
                now);
        }

        private static bool IsOffensiveRuntime(EquipmentSkillRuntimeData runtime)
        {
            SkillHitSO[] hits = runtime?.sourceEquipment?.HitSos;
            if (hits == null) return false;
            for (int i = 0; i < hits.Length; i++)
            {
                SkillHitSO hit = hits[i];
                if (hit == null || hit.TargetLayerMask.value == 0) continue;
                if (hit.BaseDamage > 0f || hit.AttackPercentDamage > 0f ||
                    (hit.DebuffEffects != null && hit.DebuffEffects.Length > 0))
                {
                    return true;
                }
            }
            return false;
        }

        private float GetFailureRetryRemaining(string skillId, float now)
        {
            return !string.IsNullOrEmpty(skillId) &&
                   failedSkillRetryEndTimes.TryGetValue(skillId, out float end)
                ? Mathf.Max(0f, end - now)
                : 0f;
        }

        private static float GetCooldownRemaining(
            CharacterSkillRuntimeData data, string skillId, float now)
        {
            return data != null && !string.IsNullOrEmpty(skillId) &&
                   data.cooldownEndTimes.TryGetValue(skillId, out float end)
                ? Mathf.Max(0f, end - now)
                : 0f;
        }

        public void MarkExecutionFailed(EquipmentSkillRuntimeData runtime)
        {
            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            if (!string.IsNullOrEmpty(skillId))
            {
                failedSkillRetryEndTimes[skillId] = Time.time + FailedSkillRetryDelay;
            }
        }

        public void ClearExecutionFailure(EquipmentSkillRuntimeData runtime)
        {
            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            if (!string.IsNullOrEmpty(skillId))
            {
                failedSkillRetryEndTimes.Remove(skillId);
            }
        }

        public void ResetExecutionFailures()
        {
            failedSkillRetryEndTimes.Clear();
        }

        public void CancelCombo(Transform caster)
        {
            if (caster != null)
            {
                activeComboCasters.Remove(caster.GetInstanceID());
                comboGenerations.Remove(caster.GetInstanceID());
            }
        }

        private bool IsTemporarilyBlockedAfterFailure(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) ||
                !failedSkillRetryEndTimes.TryGetValue(skillId, out float retryEndTime))
            {
                return false;
            }

            if (Time.time < retryEndTime)
            {
                return true;
            }

            failedSkillRetryEndTimes.Remove(skillId);
            return false;
        }
        public EquipmentSkillRuntimeData SelectReadySkill(
            CharacterSkillManager skillManager)
        {
            return SelectActiveSkill(skillManager);
        }

        public bool CanBeginCast(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster)
        {
            if (skillManager == null || !CanFireRuntime(runtime, caster))
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            return IsCooldownReady(skillManager.SkillRuntimeData, skillId);
        }

        public bool CanCommitCast(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster)
        {
            if (skillManager == null ||
                !CanFireRuntime(runtime, caster, skillManager, true))
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            return IsCooldownReady(skillManager.SkillRuntimeData, skillId);
        }

        internal bool IsRuntimeReady(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime)
        {
            if (skillManager == null || runtime == null)
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            return !IsTemporarilyBlockedAfterFailure(skillId) &&
                   IsCooldownReady(skillManager.SkillRuntimeData, skillId);
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

            if (!CanFireRuntime(runtime, caster, skillManager, true))
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);

            if (!IsCooldownReady(skillManager.SkillRuntimeData, skillId))
            {
                return false;
            }

            bool mobility = runtime.sourceEquipment?.BaseProfileSo != null &&
                runtime.sourceEquipment.BaseProfileSo.SkillComponentType == SkillComponentType.Mobility;
            bool deferredComboCooldown = runtime.comboProfile != null &&
                runtime.comboProfile.Enabled;
            if (!mobility && !deferredComboCooldown)
            {
                UseSkill(skillManager, runtime);
            }

            bool started = StartSkillUseRoutine(
                skillManager,
                runtime,
                caster,
                target,
                false,
                Vector2.zero);

            if (started)
            {
                ClearExecutionFailure(runtime);
            }

            return started;
        }

        public bool FireSkillAtSnapshotPoint(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Vector2 targetPoint)
        {
            if (skillManager == null || caster == null ||
                float.IsNaN(targetPoint.x) || float.IsInfinity(targetPoint.x) ||
                float.IsNaN(targetPoint.y) || float.IsInfinity(targetPoint.y) ||
                !CanFireRuntime(runtime, caster, skillManager, true))
            {
                return false;
            }

            string skillId = CharacterSkillHelper.GetSkillId(runtime);
            if (!IsCooldownReady(skillManager.SkillRuntimeData, skillId))
            {
                return false;
            }

            bool deferredComboCooldown = runtime.comboProfile != null && runtime.comboProfile.Enabled;
            if (!deferredComboCooldown)
            {
                UseSkill(skillManager, runtime);
            }

            bool started = StartSkillUseRoutine(
                skillManager, runtime, caster, null, true, targetPoint);
            if (started)
            {
                ClearExecutionFailure(runtime);
            }
            return started;
        }
        internal bool FireManualAim(CharacterSkillManager manager,EquipmentSkillRuntimeData runtime,Transform caster,ManualSkillAim aim)
        {
            if(!aim.IsValid||runtime?.sourceEquipment?.AimMode!=aim.Mode||!CanFireRuntime(runtime,caster,manager,true))return false;
            if(aim.Mode==SkillAimMode.Target&&!IsManualTargetValid(runtime,caster,aim.Target))return false;
            if(aim.Mode==SkillAimMode.GroundPoint&&Vector2.Distance(caster.position,aim.Point)>runtime.resolvedRange+.0001f)return false;
            bool mobility=runtime.sourceEquipment.BaseProfileSo.SkillComponentType==SkillComponentType.Mobility;
            if(!mobility&&!IsRuntimeReady(manager,runtime))return false;
            if(!mobility&&!(runtime.comboProfile!=null&&runtime.comboProfile.Enabled))UseSkill(manager,runtime);
            bool started=StartSkillUseRoutine(manager,runtime,caster,aim.Target,aim.Mode==SkillAimMode.GroundPoint,aim.Point,aim);
            if(started)ClearExecutionFailure(runtime);
            return started;
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
        public void ReduceAllCooldowns(
            CharacterSkillRuntimeData runtimeData,
            float percent,
            float seconds)
        {
            if (runtimeData == null ||
                runtimeData.cooldownEndTimes == null ||
                runtimeData.cooldownEndTimes.Count == 0)
            {
                return;
            }

            string[] skillIds = new string[runtimeData.cooldownEndTimes.Count];
            runtimeData.cooldownEndTimes.Keys.CopyTo(skillIds, 0);

            for (int i = 0; i < skillIds.Length; i++)
            {
                ReduceCooldown(
                    runtimeData,
                    skillIds[i],
                    percent,
                    seconds);
            }
        }

        private void ReduceCooldown(
            CharacterSkillRuntimeData runtimeData,
            string skillId,
            float percent,
            float seconds)
        {
            if (runtimeData == null || string.IsNullOrEmpty(skillId))
            {
                return;
            }

            if (!runtimeData.cooldownEndTimes.TryGetValue(
                    skillId,
                    out float cooldownEndTime))
            {
                return;
            }

            float remainingCooldown = cooldownEndTime - Time.time;

            if (remainingCooldown <= 0f)
            {
                return;
            }

            float reducedByPercent =
                remainingCooldown * percent;

            float reducedAmount =
                Mathf.Max(reducedByPercent, seconds);

            runtimeData.cooldownEndTimes[skillId] =
                Mathf.Max(Time.time, cooldownEndTime - reducedAmount);
        }

        private bool StartSkillUseRoutine(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint,ManualSkillAim? manualAim=null)
        {
            if (skillManager == null || runtime == null || caster == null)
            {
                return false;
            }

            return StartSkillUseRoutineCore(skillManager, runtime, caster, target, usePoint, targetPoint,manualAim);
        }

        private bool StartSkillUseRoutineCore(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint,ManualSkillAim? manualAim=null)
        {
            if (skillManager == null || runtime == null || caster == null)
            {
                return false;
            }

            SkillComboProfile combo = runtime.comboProfile;
            if (combo != null && combo.Enabled)
            {
                if (!combo.IsComplete || activeComboCasters.Contains(caster.GetInstanceID()))
                {
                    return false;
                }

                activeComboCasters.Add(caster.GetInstanceID());
                int generation=++nextComboToken;comboGenerations[caster.GetInstanceID()]=generation;
                skillManager.StartOwnedSkillRoutine(FireComboRoutine(skillManager, runtime, caster, target,usePoint,targetPoint,skillManager.ManualExecutionAuthorized?skillManager.ManualComboChain:null,generation,manualAim));
                return true;
            }

            bool isSelfOrNoTargetSkill = manualAim.HasValue?manualAim.Value.Mode==SkillAimMode.Self:IsSelfOrNoTargetSkill(runtime);
            Vector2 resolvedTargetPoint = ResolveTargetPoint(
                runtime,
                caster,
                target,
                usePoint,
                targetPoint);

            if(manualAim?.Mode==SkillAimMode.GroundPoint)resolvedTargetPoint=manualAim.Value.Point;
            Vector2 spawnPosition = caster.position;
            Vector2 direction = ResolveDirection(
                spawnPosition,
                caster,
                isSelfOrNoTargetSkill ? null : target,
                isSelfOrNoTargetSkill || usePoint,
                resolvedTargetPoint);
            if(manualAim?.Mode==SkillAimMode.Direction)direction=manualAim.Value.Direction;

            SkillCastSO castSo = ResolveCastSo(runtime);
            AnimationMono bodyAnimation = ResolveAnimation(caster);
            CharacterAnimationProfileSO animationProfile = null;
            CharacterManager animationOwner = caster != null
                ? caster.GetComponent<CharacterManager>() ?? caster.GetComponentInParent<CharacterManager>()
                : null;
            if (animationOwner != null && animationOwner.RuntimeData != null &&
                animationOwner.RuntimeData.characterSO != null)
            {
                animationProfile = animationOwner.RuntimeData.characterSO.AnimationProfile;
            }
            string executedSkillId = runtime.sourceEquipment != null
                ? runtime.sourceEquipment.EquipmentId
                : runtime.instanceData != null ? runtime.instanceData.equipmentId : null;
            AnimationClip resolvedBodyClip = CharacterAnimationResolver.ResolveSkillClip(
                animationProfile,
                executedSkillId,
                castSo != null ? castSo.BodyActionClip : null,
                out CharacterAnimationFallbackPolicy bodyFallback,
                out float bodyPlaybackSpeed,
                out bool bodyMirrorWithFacing);
            if (resolvedBodyClip != null)
            {
                ApplyAnimationDirection(caster, direction);
                float duration = castSo != null && castSo.BodyActionClip == resolvedBodyClip
                    ? castSo.BodyActionPlayback.Duration(resolvedBodyClip)
                    : Mathf.Max(.01f, resolvedBodyClip.length / bodyPlaybackSpeed);
                if (bodyAnimation == null ||
                    !bodyAnimation.PlaySkillBodyAction(resolvedBodyClip, duration, bodyMirrorWithFacing))
                {
                    if (bodyFallback == CharacterAnimationFallbackPolicy.Rendererless)
                    {
                        // Presentation is optional. Gameplay continues below.
                    }
                    else
                    {
                        PlayAttackAnimation(caster);
                    }
                }

                // Body playback is presentation-only. Existing cast/move/hit receipts
                // stay authoritative and are never delayed by an animation marker.
                float contactDelay = castSo != null && castSo.BodyActionClip == resolvedBodyClip
                    ? castSo.BodyActionPlayback.ContactTime
                    : 0f;
                skillManager.StartOwnedSkillRoutine(
                    FireSkillBodyActionTiming(
                        skillManager,
                        runtime,
                        caster,
                        isSelfOrNoTargetSkill ? null : target,
                        isSelfOrNoTargetSkill || usePoint,
                        resolvedTargetPoint,
                        contactDelay,manualAim));
                return true;
            }

            if (ShouldPlayAttackAnimation(runtime))
            {
                ApplyAnimationDirection(caster, direction);
                PlayAttackAnimation(caster);

                skillManager.StartOwnedSkillRoutine(
                    FireSkillAtAttackTiming(
                        skillManager,
                        runtime,
                        caster,
                        isSelfOrNoTargetSkill ? null : target,
                        isSelfOrNoTargetSkill || usePoint,
                        resolvedTargetPoint,manualAim));

                return true;
            }

            skillManager.StartOwnedSkillRoutine(
                FireSkillBurstRoutine(
                    skillManager,
                    runtime,
                    caster,
                    isSelfOrNoTargetSkill ? null : target,
                    isSelfOrNoTargetSkill || usePoint,
                    resolvedTargetPoint,manualAim));

            return true;
        }

        private IEnumerator FireSkillBodyActionTiming(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint,
            float contactDelay,ManualSkillAim? manualAim=null)
        {
            if (contactDelay > 0f)
            {
                yield return new WaitForSeconds(contactDelay);
            }

            yield return FireSkillBurstRoutine(
                skillManager, runtime, caster, target, usePoint, targetPoint,manualAim);
        }

        private IEnumerator FireSkillAtAttackTiming(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint,ManualSkillAim? manualAim=null)
        {
            yield return null;

            AnimationMono animationMono = ResolveAnimation(caster);
            float attackSpeed = ResolveAttackSpeed(caster);
            float delay = ResolveAttackFireDelay(
                animationMono,
                attackSpeed);

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            yield return FireSkillBurstRoutine(
                skillManager,
                runtime,
                caster,
                target,
                usePoint,
                targetPoint,manualAim);
        }

        private IEnumerator FireSkillBurstRoutine(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint,ManualSkillAim? manualAim=null)
        {
            bool successNotified = false;
            int burstCount = ResolveBurstCount(runtime);
            float burstInterval = ResolveBurstInterval(runtime);

            for (int burstIndex = 0; burstIndex < burstCount; burstIndex++)
            {
                if(manualAim?.Mode==SkillAimMode.Target&&!IsManualTargetValid(runtime,caster,manualAim.Value.Target)){MainCharacterSkillFocusFeature.CancelPending(caster);yield break;}
                if (!successNotified)
                {
                    MainCharacterSkillFocusFeature.NotifySkillExecuting(skillManager, runtime, caster);
                }
                bool burstFired = UseSkillOnce(
                    skillManager,
                    runtime,
                    caster,
                    target,
                    usePoint,
                    targetPoint,manualAim:manualAim);

                if (burstFired && !successNotified)
                {
                    successNotified = true;
                    skillManager.NotifySkillUseSucceeded(runtime);
                }
                else if (!burstFired && !successNotified)
                {
                    MainCharacterSkillFocusFeature.CancelPending(caster);
                }

                if (burstIndex < burstCount - 1 && burstInterval > 0f)
                {
                    yield return new WaitForSeconds(burstInterval);
                }
            }

            // Cooldown is started before the firing coroutine begins
            // to prevent duplicate coroutine scheduling in the same window.
        }

        private IEnumerator FireComboRoutine(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform initialTarget,bool usePoint,Vector2 point,System.Func<bool> continueChain,int generation,ManualSkillAim? manualAim=null)
        {
            int casterId = caster.GetInstanceID();
            string comboToken = $"seojin-basic-combo-{++nextComboToken}";
            bool sharedCritical = RollComboCritical(caster);
            float elapsed = 0f;
            bool anyStepFired = false;
            Transform target = initialTarget;
            bool freeAim=usePoint||manualAim?.Mode==SkillAimMode.Direction;
            SkillComboStep[] steps = runtime.comboProfile.Steps;
            bool continuousBody = runtime.comboProfile.HasCompleteSegmentedBodyRegistry;
            int completedSteps = 0;
            bool cooldownOnThirdStepEntry = runtime.sourceEquipment != null &&
                string.Equals(
                    runtime.sourceEquipment.EquipmentId,
                    "skill.character.seojin.1.basic_attack.basic_attack",
                    System.StringComparison.Ordinal);
            bool thirdStepCooldownCommitted = false;

            MainCharacterSkillFocusFeature.NotifySkillExecuting(skillManager, runtime, caster);
            AnimationMono continuousAnimation = ResolveAnimation(caster);
            if (continuousBody)
            {
                continuousAnimation?.RestartContinuousComboAction(
                    runtime.comboProfile.SegmentedBodyActionClip,
                    runtime.comboProfile,
                    null);
            }

            for (int i = 0; i < steps.Length; i++)
            {
                SkillComboStep step = steps[i];
                float waitToStart = Mathf.Max(0f, step.StartTime - elapsed);
                if (waitToStart > 0f)
                {
                    yield return new WaitForSeconds(waitToStart);
                    elapsed += waitToStart;
                }

                if (!CanContinueGeneration(caster,generation))
                {
                    break;
                }

                if(i>0&&continueChain!=null&&!continueChain())break;

                // G1 Basic commits its one-per-combo cooldown when the third step
                // actually begins (.780s), before target revalidation or its F4 hit.
                // If the combo never reaches index2 there is no cooldown; once reached,
                // later target loss/cancel cannot refund the committed cooldown.
                if (cooldownOnThirdStepEntry && step.ComboIndex == 2 &&
                    !thirdStepCooldownCommitted)
                {
                    thirdStepCooldownCommitted = UseSkill(skillManager, runtime);
                    if (thirdStepCooldownCommitted)
                    {
                        skillManager.NotifySkillUseSucceeded(runtime);
                    }
                }

                float targetRange = runtime.sourceEquipment.CastSo != null
                    ? runtime.sourceEquipment.CastSo.Range
                    : 0f;
                float continuationRange = targetRange + (i > 0 ? 0.25f : 0f);
                target = NormalizeComboTarget(target);
                if (!freeAim&&(!IsValidComboTarget(caster, target, step.Hit) ||
                    !IsComboTargetInRange(caster, target, continuationRange)))
                {
                    if(manualAim?.Mode==SkillAimMode.Target)break;
                    target = ResolveComboRetarget(caster, continuationRange, step.Hit);
                    float retargetElapsed = 0f;
                    while (retargetElapsed < 0.12f &&
                           (!IsValidComboTarget(caster, target, step.Hit) ||
                            !IsComboTargetInRange(caster, target, continuationRange)))
                    {
                        if(manualAim?.Mode==SkillAimMode.Target)break;
                    target = ResolveComboRetarget(caster, continuationRange, step.Hit);
                        if (IsValidComboTarget(caster, target, step.Hit) &&
                            IsComboTargetInRange(caster, target, continuationRange))
                        {
                            break;
                        }

                        yield return null;
                        retargetElapsed += Time.deltaTime;
                        elapsed += Time.deltaTime;
                    }
                }

                if (!freeAim&&(!IsValidComboTarget(caster, target, step.Hit) ||
                    !IsComboTargetInRange(caster, target, continuationRange)))
                {
                    break;
                }

                Vector2 direction = ResolveDirection(
                    caster.position,
                    caster,
                    target,
                    usePoint,
                    usePoint?point:(Vector2)caster.position);
                if(manualAim?.Mode==SkillAimMode.Direction)direction=manualAim.Value.Direction;
                ApplyAnimationDirection(caster, direction);
                AnimationMono comboAnimation = ResolveAnimation(caster);
                if (!continuousBody)
                {
                    if (runtime.comboProfile.UseCanonicalBodyChoreography)
                    {
                        comboAnimation?.RestartCanonicalComboChoreography(
                            step.ComboIndex,
                            Mathf.Max(0f, step.HitTime - step.StartTime),
                            Mathf.Max(0f, step.RecoveryEnd - step.HitTime));
                    }
                    else if (step.BodyActionClip != null)
                    {
                        comboAnimation?.RestartComboAction(
                            step.BodyActionClip,
                            Mathf.Max(0f, step.HitTime - step.StartTime),
                            Mathf.Max(0f, step.RecoveryEnd - step.HitTime),
                            step.BodyPresentationCalibration);
                    }
                    else
                    {
                        // Functional-pilot fallback: art absence never suppresses gameplay.
                        comboAnimation?.RestartAttack();
                    }
                }
                if (target != null &&
                    Vector2.Distance(caster.position, target.position) > 0.05f)
                {
                    ApplyCollisionSafeLunge(caster, target, direction, step.LungeDistance);
                }

                float waitToHit = Mathf.Max(0f, step.HitTime - elapsed);
                if (waitToHit > 0f)
                {
                    yield return new WaitForSeconds(waitToHit);
                    elapsed += waitToHit;
                }

                if (!CanContinueGeneration(caster,generation))
                {
                    break;
                }

                if (!freeAim&&(!IsValidComboTarget(caster, target, step.Hit) ||
                    !IsComboTargetInRange(caster, target, continuationRange)))
                {
                    if(manualAim?.Mode==SkillAimMode.Target)break;
                    target = ResolveComboRetarget(caster, continuationRange, step.Hit);
                    if (!IsValidComboTarget(caster, target, step.Hit) ||
                        !IsComboTargetInRange(caster, target, continuationRange))
                    {
                        break;
                    }

                    direction = ResolveDirection(
                        caster.position,
                        caster,
                        target,
                        false,
                        caster.position);
                    ApplyAnimationDirection(caster, direction);
                }

                if (continuousBody && step.ComboIndex == 2)
                {
                    // The body sampler and gameplay coroutine accumulate time independently.
                    // Pin the visible body to segment-3 F4 before spawning its sole VFX.
                    continuousAnimation?.SynchronizeContinuousComboContact(
                        runtime.comboProfile.SegmentedBodyActionClip,
                        step,
                        runtime.comboProfile.SegmentedBodyFrameCount);
                }

                bool fired = UseSkillOnce(
                    skillManager,
                    runtime,
                    caster,
                    target,
                    usePoint,
                    usePoint?point:target != null ? (Vector2)target.position : (Vector2)caster.position,
                    0,
                    step.VfxClip,
                    step.VfxProfileOverride,
                    step.VfxPresentationCalibration,
                    comboToken,
                    sharedCritical,
                    step.DamageWeight,
                    step.ComboIndex,
                    step.Hit,
                    step.ComboIndex < 2,
                    step.MinimumVisualLifetime,manualAim);
                anyStepFired |= fired;

                float waitToRecovery = Mathf.Max(0f, step.RecoveryEnd - elapsed);
                if (waitToRecovery > 0f)
                {
                    yield return new WaitForSeconds(waitToRecovery);
                    elapsed += waitToRecovery;
                }
                completedSteps++;
            }

            if(!comboGenerations.TryGetValue(casterId,out var terminalGeneration)||terminalGeneration!=generation)yield break;

            if (continuousBody && completedSteps != steps.Length)
            {
                continuousAnimation?.StopComboAction();
            }

            if (cooldownOnThirdStepEntry)
            {
                if (!thirdStepCooldownCommitted)
                {
                    MainCharacterSkillFocusFeature.CancelPending(caster);
                }
            }
            else if (anyStepFired && completedSteps == steps.Length)
            {
                UseSkill(skillManager, runtime);
                skillManager.NotifySkillUseSucceeded(runtime);
            }
            else
            {
                MainCharacterSkillFocusFeature.CancelPending(caster);
            }

            if(comboGenerations.TryGetValue(casterId,out var endingGeneration)&&endingGeneration==generation){activeComboCasters.Remove(casterId);comboGenerations.Remove(casterId);}
        }

        private bool UseSkillOnce(
            CharacterSkillManager skillManager,
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool usePoint,
            Vector2 targetPoint,
            int selectedHitIndex = -1,
            AnimationClip visualClipOverride = null,
            SkillAnimationVfxProfileSO animationVfxProfileOverride = null,
            SpritePresentationCalibrationProfileSO presentationCalibration = null,
            string comboToken = null,
            bool? criticalOverride = null,
            float damageWeight = 1f,
            int comboIndex = -1,
            SkillHitSO hitOverride = null,
            bool suppressVisual = false,
            float minimumVisualLifetime = 0f,ManualSkillAim? manualAim=null)
        {
            return SkillUseHelper.UseSkill(
                new SkillUseContext
                {
                    Runtime = runtime,
                    Caster = caster,
                    Target = target,
                    UsePoint = usePoint,
                    TargetPoint = targetPoint,
                    ManualAim = manualAim,
                    CoroutineRunner = skillManager,
                    SelectedHitIndex = selectedHitIndex,
                    VisualClipOverride = visualClipOverride,
                    AnimationVfxProfileOverride = animationVfxProfileOverride,
                    PresentationCalibration = presentationCalibration,
                    ComboToken = comboToken,
                    CriticalOverride = criticalOverride,
                    DamageWeight = damageWeight,
                    ComboIndex = comboIndex,
                    HitOverride = hitOverride,
                    SuppressVisual = suppressVisual,
                    MinimumVisualLifetime = minimumVisualLifetime
                });
        }

        private bool RollComboCritical(Transform caster)
        {
            CharacterManager manager = caster != null
                ? caster.GetComponent<CharacterManager>()
                  ?? caster.GetComponentInParent<CharacterManager>()
                  ?? caster.GetComponentInChildren<CharacterManager>()
                : null;
            float chance = manager != null ? manager.GetStatValue(Stat.StatType.CritChance) : 0f;
            return chance > 0f && Random.value <= chance / 100f;
        }

        private bool CanContinueGeneration(Transform caster,int generation)=>caster!=null&&comboGenerations.TryGetValue(caster.GetInstanceID(),out int current)&&current==generation&&CanContinueCombo(caster);
        private bool CanContinueCombo(Transform caster)
        {
            if (caster == null || !caster.gameObject.activeInHierarchy)
            {
                return false;
            }

            CharacterManager manager = caster.GetComponent<CharacterManager>()
                ?? caster.GetComponentInParent<CharacterManager>()
                ?? caster.GetComponentInChildren<CharacterManager>();
            return manager == null || (manager.CanUseSkill && manager.IsTargetable);
        }

        private Transform NormalizeComboTarget(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            CharacterManager manager = target.GetComponent<CharacterManager>()
                ?? target.GetComponentInParent<CharacterManager>()
                ?? target.GetComponentInChildren<CharacterManager>();
            return manager != null ? manager.transform : target;
        }

        private bool IsValidComboTarget(
            Transform caster,
            Transform target,
            SkillHitSO hit)
        {
            target = NormalizeComboTarget(target);
            if (caster == null || target == null || target.root == caster.root)
            {
                return false;
            }

            CharacterManager manager = target.GetComponent<CharacterManager>()
                ?? target.GetComponentInParent<CharacterManager>();
            if (manager == null || !manager.IsTargetable)
            {
                return false;
            }

            LayerMask mask = hit != null ? hit.TargetLayerMask : 0;
            return mask.value != 0 &&
                   (mask.value & (1 << manager.gameObject.layer)) != 0;
        }

        private Transform ResolveComboRetarget(
            Transform caster,
            float radius,
            SkillHitSO hit,Vector2? coneDirection=null)
        {
            if (caster == null || hit == null)
            {
                return null;
            }

            Collider2D[] candidates = Physics2D.OverlapCircleAll(caster.position, radius);
            var roots = new System.Collections.Generic.List<CharacterManager>();
            var rootIds = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < candidates.Length; i++)
            {
                CharacterManager manager = candidates[i] != null
                    ? candidates[i].GetComponentInParent<CharacterManager>()
                    : null;
                if (manager == null || !rootIds.Add(manager.transform.root.GetInstanceID()))
                {
                    continue;
                }

                Vector2 delta=(Vector2)manager.transform.position-(Vector2)caster.position;
                if(coneDirection.HasValue&&(delta.sqrMagnitude<=.0001f||Vector2.Dot(delta.normalized,coneDirection.Value.normalized)<.70710678f))continue;
                if (IsValidComboTarget(caster, manager.transform, hit))
                {
                    roots.Add(manager);
                }
            }

            roots.Sort((left, right) =>
            {
                float leftDistance = ((Vector2)left.transform.position - (Vector2)caster.position).sqrMagnitude;
                float rightDistance = ((Vector2)right.transform.position - (Vector2)caster.position).sqrMagnitude;
                int distanceOrder = leftDistance.CompareTo(rightDistance);
                return distanceOrder != 0
                    ? distanceOrder
                    : left.transform.root.GetInstanceID().CompareTo(right.transform.root.GetInstanceID());
            });

            return roots.Count > 0 ? roots[0].transform : null;
        }

        internal Transform ResolveManualTarget(EquipmentSkillRuntimeData runtime,Transform caster,Vector2 direction)
        {
            if(runtime?.sourceEquipment?.HitSos==null||direction.sqrMagnitude<=.0001f||float.IsNaN(runtime.resolvedRange)||float.IsInfinity(runtime.resolvedRange)||runtime.resolvedRange<0)return null;
            SkillHitSO hit=System.Array.Find(runtime.sourceEquipment.HitSos,h=>h!=null);
            return ResolveComboRetarget(caster,runtime.resolvedRange,hit,direction);
        }
        internal bool IsManualTargetValid(EquipmentSkillRuntimeData runtime,Transform caster,Transform target)
        {
            var hits=runtime?.sourceEquipment?.HitSos;
            return hits!=null&&IsValidComboTarget(caster,target,System.Array.Find(hits,h=>h!=null))&&IsComboTargetInRange(caster,target,runtime.resolvedRange);
        }
        private bool IsComboTargetInRange(Transform caster, Transform target, float range)
        {
            return caster != null && target != null &&
                   Vector2.Distance(caster.position, target.position) <= Mathf.Max(0f, range);
        }

        private void ApplyCollisionSafeLunge(
            Transform caster,
            Transform intendedTarget,
            Vector2 direction,
            float distance)
        {
            if (caster == null || distance <= 0f || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Rigidbody2D body = caster.GetComponent<Rigidbody2D>()
                ?? caster.GetComponentInParent<Rigidbody2D>();
            if (body == null)
            {
                return;
            }

            MovementMono movement = caster.GetComponent<MovementMono>()
                ?? caster.GetComponentInParent<MovementMono>();
            movement?.StopAllMotion(stopKnockback: false);

            ContactFilter2D filter = new ContactFilter2D
            {
                useTriggers = false,
                useLayerMask = false
            };
            RaycastHit2D[] hits = new RaycastHit2D[8];
            int count = body.Cast(direction.normalized, filter, hits, distance);
            float allowed = distance;
            for (int i = 0; i < count; i++)
            {
                Collider2D collider = hits[i].collider;
                if (collider == null || collider.transform.root == caster.root)
                {
                    continue;
                }

                float clearance = collider.transform.root == intendedTarget.root
                    ? 0.02f
                    : 0.01f;
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hits[i].distance - clearance));
            }

            body.MovePosition(body.position + direction.normalized * allowed);
        }

        private int ResolveBurstCount(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null)
            {
                return 1;
            }

            return Mathf.Max(1, runtime.resolvedBurstCount);
        }

        private float ResolveBurstInterval(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, runtime.resolvedBurstInterval);
        }

        private bool ShouldPlayAttackAnimation(
            EquipmentSkillRuntimeData runtime)
        {
            return runtime != null &&
                   !ShouldSkipAttackAnimation(runtime) &&
                   !HasCastMove(runtime);
        }

        private bool ShouldSkipAttackAnimation(
            EquipmentSkillRuntimeData runtime)
        {
            SkillCastSO castSo = ResolveCastSo(runtime);

            return castSo != null && castSo.SkipAttackAnimation;
        }

        private bool HasCastMove(
            EquipmentSkillRuntimeData runtime)
        {
            SkillCastSO castSo = ResolveCastSo(runtime);

            return castSo != null &&
                   castSo.CastMove != null &&
                   castSo.CastMove.MoveType != CastMoveType.None;
        }

        private float ResolveAttackFireDelay(
            AnimationMono animationMono,
            float attackSpeed)
        {
            if (animationMono == null)
            {
                return 0f;
            }

            return animationMono.GetAttackFireDelay(2f / 3f) /
                   Mathf.Max(0.01f, attackSpeed);
        }

        private float ResolveAttackSpeed(
            Transform caster)
        {
            if (caster == null)
            {
                return 1f;
            }

            CharacterManager characterManager =
                caster.GetComponent<CharacterManager>()
                ?? caster.GetComponentInParent<CharacterManager>()
                ?? caster.GetComponentInChildren<CharacterManager>();

            if (characterManager == null)
            {
                return 1f;
            }

            return Mathf.Max(
                0.01f,
                characterManager.GetStatValue(Stat.StatType.AttackSpeed));
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
            SkillCastSO castSo = ResolveCastSo(runtime);

            if (castSo == null)
            {
                return usePoint
                    ? targetPoint
                    : caster != null
                        ? (Vector2)caster.position
                        : Vector2.zero;
            }

            TargetingType targetingType = castSo.TargetingType;

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
            SkillCastSO castSo = ResolveCastSo(runtime);

            if (castSo == null)
            {
                return false;
            }

            TargetingType targetingType = castSo.TargetingType;

            return targetingType == TargetingType.Self ||
                   targetingType == TargetingType.None;
        }

        private SkillCastSO ResolveCastSo(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null || runtime.sourceEquipment == null)
            {
                return null;
            }

            return runtime.sourceEquipment.CastSo;
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
            Transform caster,
            CharacterSkillManager castOwner = null,
            bool allowOwnedCastPose = false)
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

            bool ownsCurrentCastPose = allowOwnedCastPose && castOwner != null &&
                castOwner.OwnsHeldCastPose(runtime, caster, animationMono);
            if (!ShouldSkipAttackAnimation(runtime) && animationMono != null &&
                animationMono.IsPlayingAttack() && !ownsCurrentCastPose)
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
