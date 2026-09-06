using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Skill;
using Character.Runtime.Skill;
using Character.Skill;

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
        private readonly SkillUpgradeService skillUpgradeService = new();
        private Coroutine castRoutine;
        private EquipmentSkillRuntimeData castingRuntime;
        private Transform castingCaster;
        private CharacterSkillCastPresentationMono castPresentation;
        private CharacterCastWorldHudMono castWorldHud;
        private NpcCastGroundConeTelegraphMono npcCastGroundCone;
        private SkillCastPhaseClock castClock;
        private AnimationMono castPoseAnimation;
        private AnimationClip castPoseClip;
        private AnimationMono castFacingAnimation;
        private object castFacingOwner;
        private Transform castFacingTarget;
        private Coroutine castFacingReleaseRoutine;
        private bool hasCastTargetPointSnapshot;
        private Vector2 castTargetPointSnapshot;

        public CharacterSkillRuntimeData SkillRuntimeData => skillRuntimeData;
        public SkillPoolRuntimeData SkillPool => skillRuntimeData?.skillPool;
        public bool IsCasting => castRoutine != null;

        /// <summary>
        /// Fired only after the shared skill-use path reports a successful use.
        /// Selection, cooldown rejection, and failed execution do not raise it.
        /// </summary>
        public event Action<EquipmentSkillRuntimeData> SkillUseSucceeded;

        /// <summary>
        /// Raised synchronously when an interruptible cast crosses its commit
        /// boundary and the existing execution path accepts the skill. Cast
        /// acceptance itself never raises this receipt.
        /// </summary>
        public event Action<EquipmentSkillRuntimeData> CastCommitted;

        internal void NotifySkillUseSucceeded(
            EquipmentSkillRuntimeData runtime)
        {
            if (runtime == null)
            {
                return;
            }

            SkillUseSucceeded?.Invoke(runtime);
        }

        private void OnDisable()
        {
            CancelCasting();
            skillService.CancelCombo(transform);
            castMoveService.StopMove();
            AnimationMono animation = GetComponent<AnimationMono>()
                ?? GetComponentInParent<AnimationMono>()
                ?? GetComponentInChildren<AnimationMono>();
            animation?.StopSkillBodyAction();
        }

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
            skillService.ResetExecutionFailures();

            SkillPoolRuntimeData skillPool = new SkillPoolRuntimeData();

            IReadOnlyList<CharacterSkillEntry> skills = characterSO != null
                ? characterSO.Skills
                : null;

            if (skills == null || skills.Count == 0)
            {
                skillRuntimeData.skillPool = skillPool;
                return;
            }

            for (int i = 0; i < skills.Count; i++)
            {
                CharacterSkillEntry entry = skills[i];
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

        public bool TryUpgradeSkill(
            EquipmentSkillInstanceData skillInstance,
            int maxSkillLevel)
        {
            CharacterRuntimeData characterRuntimeData = ResolveCharacterRuntimeData();

            return skillUpgradeService.TryUpgradeSkill(
                characterRuntimeData,
                skillInstance,
                this,
                maxSkillLevel);
        }

        public void RefreshSkillRuntimes()
        {
            if (skillRuntimeData == null || skillRuntimeData.skillPool == null)
            {
                return;
            }

            CharacterRuntimeData characterRuntimeData = ResolveCharacterRuntimeData();
            if (characterRuntimeData == null)
            {
                Debug.LogError(
                    $"[CharacterSkillManager] CharacterRuntimeData not found. Character={name}");
                return;
            }

            EquipmentSkillResolver resolver = new EquipmentSkillResolver();
            skillRuntimeData.skillPool.ResolveAllSkills(
                resolver,
                characterRuntimeData);

            ApplyPassiveSkills();
        }

        public EquipmentSkillRuntimeData SelectReadyActiveSkill(
            bool allowActiveSkills = true)
        {
            return skillService.SelectActiveSkill(
                this,
                allowActiveSkills);
        }

        public OffensiveReadinessSnapshot QueryOffensiveReadiness(
            bool allowActiveSkills,
            bool canUseSkill,
            bool validTarget)
        {
            return skillService.QueryOffensiveReadiness(
                this, allowActiveSkills, canUseSkill, validTarget);
        }

        public void MarkSkillExecutionFailed(EquipmentSkillRuntimeData runtime)
        {
            skillService.MarkExecutionFailed(runtime);
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
            SkillCastSO castSo = runtime?.sourceEquipment?.CastSo;
            float castTime = ResolveCastTime(castSo);
            if (castTime > 0f)
            {
                return BeginCasting(runtime, caster, target, castTime);
            }

            return FireSkillImmediate(runtime, caster, target, true);
        }

        public void CancelCasting()
        {
            if (castRoutine != null)
            {
                StopCoroutine(castRoutine);
                castRoutine = null;
            }

            castClock = null;
            castPresentation?.CancelPresentation();
            castWorldHud?.HideAndReset();
            npcCastGroundCone?.CancelAndHide();
            ReleaseCastBodyPose();
            ReleaseCastFacing();
            castingRuntime = null;
            castingCaster = null;
            hasCastTargetPointSnapshot = false;
            castTargetPointSnapshot = Vector2.zero;
            EndSkillExecution();
        }

        private bool BeginCasting(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            float castTime)
        {
            if (castRoutine != null ||
                !skillService.CanBeginCast(this, runtime, caster) ||
                !IsCastTargetValid(runtime, caster, target))
            {
                return false;
            }

            BeginSkillExecution();
            castingRuntime = runtime;
            castingCaster = caster;
            SkillCastSO castSo = runtime?.sourceEquipment?.CastSo;
            hasCastTargetPointSnapshot = castSo != null && castSo.SnapshotTargetPointOnCast;
            if (hasCastTargetPointSnapshot)
            {
                castTargetPointSnapshot = target != null
                    ? (Vector2)target.position
                    : new Vector2(float.NaN, float.NaN);
                if (float.IsNaN(castTargetPointSnapshot.x) || float.IsInfinity(castTargetPointSnapshot.x) ||
                    float.IsNaN(castTargetPointSnapshot.y) || float.IsInfinity(castTargetPointSnapshot.y))
                {
                    Debug.LogWarning("[CharacterSkillManager] Invalid cast target snapshot; cast cancelled.", this);
                    hasCastTargetPointSnapshot = false;
                    castingRuntime = null;
                    castingCaster = null;
                    EndSkillExecution();
                    return false;
                }
            }
            BeginCastFacing(runtime, caster, target);
            BeginCastBodyPose(runtime?.sourceEquipment?.CastSo, caster);
            castPresentation = caster.GetComponent<CharacterSkillCastPresentationMono>()
                ?? caster.GetComponentInChildren<CharacterSkillCastPresentationMono>()
                ?? caster.gameObject.AddComponent<CharacterSkillCastPresentationMono>();
            castPresentation.BeginPresentation(castTime);
            if (NpcCastGroundConeTelegraphMono.Supports(runtime))
            {
                castWorldHud = caster.GetComponent<CharacterCastWorldHudMono>()
                    ?? caster.GetComponentInChildren<CharacterCastWorldHudMono>(true);
                castWorldHud?.HideAndReset();
                npcCastGroundCone = caster.GetComponent<NpcCastGroundConeTelegraphMono>()
                    ?? caster.gameObject.AddComponent<NpcCastGroundConeTelegraphMono>();
                Vector2 snapshotPoint = hasCastTargetPointSnapshot
                    ? castTargetPointSnapshot
                    : target != null ? (Vector2)target.position : (Vector2)caster.position;
                Vector2 snapshotDirection = snapshotPoint - (Vector2)caster.position;
                EquipmentBaseProfileSO baseProfile =
                    runtime.sourceEquipment.BaseProfileSo;
                float colliderRadius = baseProfile != null
                    ? baseProfile.ProjectileColliderRadius
                    : .01f;
                npcCastGroundCone.Begin(
                    caster.position,
                    snapshotDirection,
                    castSo.Range,
                    baseProfile != null ? baseProfile.ProjectileSpawnOffset : 0f,
                    baseProfile != null ? baseProfile.ProjectileScale : 1f,
                    colliderRadius);
            }
            else
            {
                npcCastGroundCone?.HideAndReset();
                castWorldHud = caster.GetComponent<CharacterCastWorldHudMono>()
                    ?? caster.GetComponentInChildren<CharacterCastWorldHudMono>(true)
                    ?? caster.gameObject.AddComponent<CharacterCastWorldHudMono>();
                castWorldHud.BeginCast(castTime);
            }
            castClock = new SkillCastPhaseClock(castTime);
            castRoutine = StartCoroutine(CastRoutine(runtime, caster, target, castTime));
            return true;
        }

        private System.Collections.IEnumerator CastRoutine(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            float castTime)
        {
            while (castClock != null && !castClock.Tick(Time.deltaTime))
            {
                if (!CanContinueCasting(runtime, caster, target))
                {
                    CancelCastingFromRoutine();
                    yield break;
                }

                RefreshCastFacing(caster);
                castPresentation?.SetProgress(castClock.Progress);
                castWorldHud?.SetCastProgress(
                    castClock.Progress,
                    castClock.RemainingSeconds);
                npcCastGroundCone?.SetProgress(castClock.Progress);
                yield return null;
            }

            if (!CanContinueCasting(runtime, caster, target) ||
                !skillService.CanCommitCast(this, runtime, caster))
            {
                CancelCastingFromRoutine();
                yield break;
            }

            castRoutine = null;
            castClock = null;
            RefreshCastFacing(caster);
            castPresentation?.CompletePresentation();
            castWorldHud?.HideAndReset();
            npcCastGroundCone?.CompleteAndHide();

            // Commit through the pre-existing immediate path exactly once. Cost,
            // cooldown, body action and gameplay objects are all downstream of this call.
            if (!FireSkillImmediate(runtime, caster, target, false))
            {
                ReleaseCastBodyPose();
                ReleaseCastFacing();
                castingRuntime = null;
                castingCaster = null;
                hasCastTargetPointSnapshot = false;
                castTargetPointSnapshot = Vector2.zero;
                EndSkillExecution();
                yield break;
            }

            // ActiveSkillService consumes a matching held pose synchronously.
            // A missing/mismatched body clip must never leave the caster frozen.
            if (castPoseAnimation != null &&
                castPoseAnimation.IsHoldingSkillBodyActionCastPose(castPoseClip))
            {
                ReleaseCastBodyPose();
            }
            else
            {
                castPoseAnimation = null;
                castPoseClip = null;
            }

            ScheduleCastFacingRelease();

            CastCommitted?.Invoke(runtime);
            castingRuntime = null;
            castingCaster = null;
            hasCastTargetPointSnapshot = false;
            castTargetPointSnapshot = Vector2.zero;
        }

        private void CancelCastingFromRoutine()
        {
            castRoutine = null;
            castClock = null;
            castPresentation?.CancelPresentation();
            castWorldHud?.HideAndReset();
            npcCastGroundCone?.CancelAndHide();
            ReleaseCastBodyPose();
            ReleaseCastFacing();
            castingRuntime = null;
            castingCaster = null;
            hasCastTargetPointSnapshot = false;
            castTargetPointSnapshot = Vector2.zero;
            EndSkillExecution();
        }

        private void BeginCastBodyPose(SkillCastSO castSo, Transform caster)
        {
            ReleaseCastBodyPose();
            AnimationClip clip = castSo != null ? castSo.BodyActionClip : null;
            if (clip == null || caster == null)
                return;

            AnimationMono animation = caster.GetComponent<AnimationMono>()
                ?? caster.GetComponentInParent<AnimationMono>()
                ?? caster.GetComponentInChildren<AnimationMono>();
            if (animation == null || !animation.HoldSkillBodyActionCastPose(clip, true))
                return;

            castPoseAnimation = animation;
            castPoseClip = clip;
        }

        private void ReleaseCastBodyPose()
        {
            castPoseAnimation?.ReleaseSkillBodyActionCastPose();
            castPoseAnimation = null;
            castPoseClip = null;
        }

        private void BeginCastFacing(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            ReleaseCastFacing();
            if (!NpcCastGroundConeTelegraphMono.Supports(runtime) || caster == null)
                return;

            AnimationMono animation = caster.GetComponent<AnimationMono>()
                ?? caster.GetComponentInParent<AnimationMono>()
                ?? caster.GetComponentInChildren<AnimationMono>();
            Vector2 point = hasCastTargetPointSnapshot
                ? castTargetPointSnapshot
                : target != null ? (Vector2)target.position : (Vector2)caster.position;
            Vector2 direction = point - (Vector2)caster.position;
            object owner = new object();
            if (animation == null || !animation.AcquireFacingLock(owner, direction))
                return;

            castFacingAnimation = animation;
            castFacingOwner = owner;
            castFacingTarget = target;
        }

        private void RefreshCastFacing(Transform caster)
        {
            if (castFacingAnimation == null || castFacingOwner == null || caster == null)
                return;
            Vector2 point = hasCastTargetPointSnapshot
                ? castTargetPointSnapshot
                : castFacingTarget != null
                    ? (Vector2)castFacingTarget.position
                    : (Vector2)caster.position;
            castFacingAnimation.RefreshFacingLock(
                castFacingOwner,
                point - (Vector2)caster.position);
        }

        private void ScheduleCastFacingRelease()
        {
            if (castFacingAnimation == null || castFacingOwner == null)
                return;
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                ReleaseCastFacing();
                return;
            }
            castFacingReleaseRoutine = StartCoroutine(ReleaseCastFacingAfterRecovery(
                castFacingAnimation, castFacingOwner));
        }

        private System.Collections.IEnumerator ReleaseCastFacingAfterRecovery(
            AnimationMono animation,
            object owner)
        {
            yield return null;
            while (animation != null && animation.isActiveAndEnabled && animation.IsPlayingOneShot)
                yield return null;
            if (animation != null)
                animation.ReleaseFacingLock(owner);
            if (ReferenceEquals(castFacingOwner, owner))
            {
                castFacingAnimation = null;
                castFacingOwner = null;
                castFacingTarget = null;
                castFacingReleaseRoutine = null;
            }
        }

        private void ReleaseCastFacing()
        {
            if (castFacingReleaseRoutine != null)
            {
                StopCoroutine(castFacingReleaseRoutine);
                castFacingReleaseRoutine = null;
            }
            castFacingAnimation?.ReleaseFacingLock(castFacingOwner);
            castFacingAnimation = null;
            castFacingOwner = null;
            castFacingTarget = null;
        }

        internal bool OwnsHeldCastPose(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            AnimationMono animation)
        {
            SkillCastSO castSo = runtime?.sourceEquipment?.CastSo;
            return runtime != null && ReferenceEquals(castingRuntime, runtime) &&
                caster != null && castingCaster == caster &&
                animation != null && castPoseAnimation == animation &&
                castSo != null && castPoseClip == castSo.BodyActionClip &&
                animation.IsHoldingSkillBodyActionCastPose(castPoseClip);
        }

        public sealed class SkillCastPhaseClock
        {
            private readonly float duration;
            private float elapsed;
            private bool committed;

            public SkillCastPhaseClock(float castTime)
            {
                duration = NormalizeCastTime(castTime);
            }

            public float Progress => duration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / duration);

            public float RemainingSeconds => Mathf.Max(0f, duration - elapsed);

            public bool Tick(float scaledDeltaTime)
            {
                if (committed)
                {
                    return false;
                }

                elapsed += Mathf.Max(0f, scaledDeltaTime);
                if (duration > 0f && elapsed < duration)
                {
                    return false;
                }

                committed = true;
                return true;
            }
        }

        private bool CanContinueCasting(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            if (runtime == null || caster == null || !caster.gameObject.activeInHierarchy)
            {
                return false;
            }

            CharacterManager character = caster.GetComponent<CharacterManager>()
                ?? caster.GetComponentInParent<CharacterManager>();
            if (character != null && (!character.IsTargetable || !character.CanUseSkill))
            {
                return false;
            }

            MovementController movement = caster.GetComponent<MovementController>()
                ?? caster.GetComponentInParent<MovementController>()
                ?? caster.GetComponentInChildren<MovementController>();
            if (movement != null && movement.CurrentMode != MovementController.MoveMode.None)
            {
                return false;
            }

            SkillCastSO castSo = runtime?.sourceEquipment?.CastSo;
            if (castSo != null && castSo.SnapshotTargetPointOnCast)
            {
                return hasCastTargetPointSnapshot;
            }

            return IsCastTargetValid(runtime, caster, target);
        }

        private static bool IsCastTargetValid(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            SkillCastSO castSo = runtime?.sourceEquipment?.CastSo;
            if (castSo == null || castSo.TargetingType == TargetingType.None ||
                castSo.TargetingType == TargetingType.Self)
            {
                return true;
            }

            if (caster == null || target == null || !target.gameObject.activeInHierarchy)
            {
                return false;
            }

            CharacterManager targetCharacter = target.GetComponent<CharacterManager>()
                ?? target.GetComponentInParent<CharacterManager>();
            if (targetCharacter != null && !targetCharacter.IsTargetable)
            {
                return false;
            }

            return castSo.Range <= 0f ||
                Vector2.Distance(caster.position, target.position) <= castSo.Range;
        }

        public static float NormalizeCastTime(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value) || value <= 0f
                ? 0f
                : value;
        }

        private static float ResolveCastTime(SkillCastSO castSo)
        {
            return NormalizeCastTime(castSo != null ? castSo.CastTime : 0f);
        }

        private bool FireSkillImmediate(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target,
            bool beginExecution)
        {
            if (beginExecution)
            {
                BeginSkillExecution();
            }

            bool mobility = IsMobility(runtime);
            bool movementStarted = TryStartCastMove(
                runtime,
                caster,
                target);

            if (mobility && !movementStarted)
            {
                EndSkillExecution();
                return false;
            }

            bool started = hasCastTargetPointSnapshot
                ? skillService.FireSkillAtSnapshotPoint(
                    this, runtime, caster, castTargetPointSnapshot)
                : skillService.FireSkill(
                    this, runtime, caster, target);

            if (!started)
            {
                EndSkillExecution();
            }

            return started;
        }

        private bool TryStartCastMove(
            EquipmentSkillRuntimeData runtime,
            Transform caster,
            Transform target)
        {
            CastMoveProfile castMove = ResolveCastMove(runtime);

            if (castMove == null || castMove.MoveType == CastMoveType.None)
            {
                return false;
            }

            Transform resolvedCaster = caster != null
                ? caster
                : transform;

            Vector2 castDirection = ResolveCastDirection(
                resolvedCaster,
                target);

            if (IsMobility(runtime))
            {
                bool usesBodyActionClip = runtime.sourceEquipment.CastSo.BodyActionClip != null;
                CharacterManager animationOwner = resolvedCaster.GetComponent<CharacterManager>()
                    ?? resolvedCaster.GetComponentInParent<CharacterManager>();
                CharacterAnimationProfileSO animationProfile = animationOwner != null &&
                    animationOwner.RuntimeData != null && animationOwner.RuntimeData.characterSO != null
                    ? animationOwner.RuntimeData.characterSO.AnimationProfile
                    : null;
                usesBodyActionClip = usesBodyActionClip ||
                    (animationProfile != null && animationProfile.TryGetSkill(
                        runtime.sourceEquipment.EquipmentId, out CharacterSkillAnimationEntry action) &&
                     action.Clip != null);
                global::Skill.CharacterMobilityBodyPresentationController bodyPresenter =
                    resolvedCaster.GetComponent<global::Skill.CharacterMobilityBodyPresentationController>()
                    ?? resolvedCaster.gameObject.AddComponent<global::Skill.CharacterMobilityBodyPresentationController>();
                bool bodyPresentationStarted = !usesBodyActionClip && bodyPresenter.Begin(
                    runtime.sourceEquipment.CastSo.MobilityBodyPresentation);
                global::Skill.MobilityCastPresentationControllerMono presentation =
                    resolvedCaster.GetComponentInChildren<global::Skill.MobilityCastPresentationControllerMono>();
                presentation?.Play(runtime.sourceEquipment.CastSo.MobilityVfxClip);
                return castMoveService.TryStartMove(
                    this, resolvedCaster, target, castDirection, castMove,
                    Character.Movement.CharacterMovementPriority.SwiftStep,
                    () => skillService.UseSkill(this, runtime),
                    receipt =>
                    {
                        if (receipt.Succeeded)
                        {
                            global::Skill.Service.Helper.SkillUseHelper.ApplyPostMoveSelfEffects(
                                runtime, resolvedCaster.gameObject);
                        }
                        else
                        {
                            bodyPresenter.Restore();
                        }
                    },
                    bodyPresentationStarted
                        ? () => bodyPresenter.PreparationFrameObserved
                        : null);
            }

            return castMoveService.TryStartMove(
                this, resolvedCaster, target, castDirection, castMove);
        }

        private static bool IsMobility(EquipmentSkillRuntimeData runtime)
        {
            return runtime?.sourceEquipment?.BaseProfileSo != null &&
                runtime.sourceEquipment.BaseProfileSo.SkillComponentType == SkillComponentType.Mobility;
        }

        private CastMoveProfile ResolveCastMove(
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
            AddRuntimeBySlotKey(result, SkillPoolSlotKeys.Active4);

            return result.ToArray();
        }

        public void ReduceAllCooldowns(
            float percent,
            float seconds)
        {
            skillService.ReduceAllCooldowns(
                skillRuntimeData,
                percent,
                seconds);
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
            CharacterManager ownerCharacter =
                GetComponent<CharacterManager>()
                ?? GetComponentInParent<CharacterManager>()
                ?? GetComponentInChildren<CharacterManager>();

            passiveSkillService.ApplyPassiveSkills(
                this,
                ownerCharacter);
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

        /// <summary>
        /// Runtime-created, world-space cast indicator. It deliberately has no
        /// prefab/scene dependency so pooled NPCs receive the same presentation.
        /// Failure to create presentation never affects cast gameplay.
        /// </summary>
        [DisallowMultipleComponent]
    public sealed class CharacterCastWorldHudMono : MonoBehaviour
        {
            private const string HudRootName = "__CharacterCastWorldHud";
            private const int TopSortingOrder = 32760;
            private static readonly Color BackColor = new(0.04f, 0.055f, 0.07f, 0.9f);
            private static readonly Color FillColor = new(0.84f, 0.42f, 0.27f, 1f);

            private GameObject hudRoot;
            private RectTransform fillRect;
            private TextMeshProUGUI remainingText;
            private bool creationFailed;
            private bool warningLogged;

            public bool IsVisible => hudRoot != null && hudRoot.activeSelf;

            public bool BeginCast(float duration)
            {
                if (duration <= 0f)
                {
                    HideAndReset();
                    return false;
                }

                if (!EnsureHud())
                {
                    WarnOnce("runtime world HUD could not be created; cast gameplay continues without it");
                    return false;
                }

                hudRoot.SetActive(true);
                SetCastProgress(0f, duration);
                return true;
            }

            public void SetCastProgress(float progress, float remainingSeconds)
            {
                if (hudRoot == null || fillRect == null || remainingText == null)
                {
                    return;
                }

                float normalized = Mathf.Clamp01(progress);
                fillRect.anchorMax = new Vector2(normalized, 1f);
                remainingText.text = FormatRemainingSeconds(remainingSeconds);
            }

            public void HideAndReset()
            {
                if (fillRect != null)
                {
                    fillRect.anchorMax = new Vector2(0f, 1f);
                }
                if (remainingText != null)
                {
                    remainingText.text = "0.0s";
                }
                if (hudRoot != null)
                {
                    hudRoot.SetActive(false);
                }
            }

            public static string FormatRemainingSeconds(float seconds)
            {
                return Mathf.Max(0f, seconds)
                    .ToString("0.0", CultureInfo.InvariantCulture) + "s";
            }

            private bool EnsureHud()
            {
                if (creationFailed)
                {
                    return false;
                }
                if (hudRoot != null && fillRect != null && remainingText != null)
                {
                    return true;
                }

                try
                {
                    Transform existing = transform.Find(HudRootName);
                    hudRoot = existing != null ? existing.gameObject : null;
                    if (hudRoot != null)
                    {
                        fillRect = hudRoot.transform.Find("Bar/Fill") as RectTransform;
                        remainingText = hudRoot.GetComponentInChildren<TextMeshProUGUI>(true);
                        if (fillRect != null && remainingText != null)
                        {
                            return true;
                        }
                        Destroy(hudRoot);
                    }

                    hudRoot = new GameObject(HudRootName, typeof(RectTransform), typeof(Canvas));
                    hudRoot.layer = gameObject.layer;
                    RectTransform rootRect = hudRoot.GetComponent<RectTransform>();
                    rootRect.SetParent(transform, false);
                    rootRect.localPosition = new Vector3(0f, 1.35f, 0f);
                    rootRect.localRotation = Quaternion.identity;
                    rootRect.localScale = Vector3.one * 0.01f;
                    rootRect.sizeDelta = new Vector2(160f, 36f);

                    Canvas canvas = hudRoot.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = TopSortingOrder;

                    RectTransform bar = CreateRect("Bar", rootRect, new Vector2(0f, 9f), new Vector2(150f, 12f));
                    Image background = bar.gameObject.AddComponent<Image>();
                    background.color = BackColor;
                    background.raycastTarget = false;

                    fillRect = CreateRect("Fill", bar, Vector2.zero, Vector2.zero);
                    fillRect.anchorMin = Vector2.zero;
                    fillRect.anchorMax = new Vector2(0f, 1f);
                    fillRect.offsetMin = new Vector2(2f, 2f);
                    fillRect.offsetMax = new Vector2(-2f, -2f);
                    Image fill = fillRect.gameObject.AddComponent<Image>();
                    fill.color = FillColor;
                    fill.raycastTarget = false;

                    RectTransform textRect = CreateRect("Remaining", rootRect, new Vector2(0f, -8f), new Vector2(150f, 20f));
                    remainingText = textRect.gameObject.AddComponent<TextMeshProUGUI>();
                    remainingText.alignment = TextAlignmentOptions.Center;
                    remainingText.fontSize = 16f;
                    remainingText.color = Color.white;
                    remainingText.raycastTarget = false;
                    remainingText.text = "0.0s";
                    hudRoot.SetActive(false);
                    return true;
                }
                catch (Exception exception)
                {
                    creationFailed = true;
                    if (hudRoot != null)
                    {
                        Destroy(hudRoot);
                        hudRoot = null;
                    }
                    WarnOnce($"runtime world HUD creation failed ({exception.GetType().Name}); cast gameplay continues");
                    return false;
                }
            }

            private static RectTransform CreateRect(
                string objectName,
                Transform parent,
                Vector2 anchoredPosition,
                Vector2 size)
            {
                GameObject child = new(objectName, typeof(RectTransform));
                RectTransform rect = child.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = new Vector2(.5f, .5f);
                rect.anchorMax = new Vector2(.5f, .5f);
                rect.pivot = new Vector2(.5f, .5f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = size;
                return rect;
            }

            private void WarnOnce(string message)
            {
                if (warningLogged)
                {
                    return;
                }
                warningLogged = true;
                Debug.LogWarning($"[CharacterCastWorldHud] {name}: {message}", this);
            }

            private void OnDisable() => HideAndReset();
            private void OnDestroy() => HideAndReset();
    }
}
