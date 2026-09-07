using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Stat;
using Util;
using Skill;

namespace Character
{
    [DisallowMultipleComponent]
    public sealed class ComboBodyPresentationProxyMarker : MonoBehaviour
    {
    }

    /// <summary>
    /// Animation clip based controller for party members.
    ///
    /// - Uses AnimationClip instead of sprite frame arrays.
    /// - Supports state + direction selection.
    /// - Direction is based on 4 diagonal facings.
    /// - Attack plays once, then returns to the previous locomotion state.
    ///
    /// This component does not require an Animator Controller asset.
    /// It uses Playables directly on an Animator component.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class AnimationMono : MonoBehaviour
    {
        public enum AnimationState
        {
            None,
            Idle,
            Move,
            Attack,
            AttackDisabledCc,
            Death
        }

        public enum DiagonalDirection
        {
            UpRight,
            UpLeft,
            DownRight,
            DownLeft
        }


        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private MovementController movementController;
        [SerializeField] private CharacterManager characterManager;
        [SerializeField] private SortingOrderMono sortingOrderMono;

        [Header("Default")]
        [SerializeField] private bool playIdleOnStart = true;
        [SerializeField] private DiagonalDirection defaultDirection = DiagonalDirection.DownRight;
        [SerializeField] private bool resetToPreviousLocomotionAfterAttack = true;
        private const float AttackRecoveryDelay = 0.3f;
        public const float LocomotionReferenceMoveSpeed = 3f;
        public const float LocomotionMinimumPlaybackRate = 0.5f;
        public const float LocomotionMaximumPlaybackRate = 1.5f;

        [Header("Animation Clips")]
        [SerializeField] private List<CharacterAnimationClipEntry> animationClips = new();
        private CharacterAnimationProfileSO _animationProfile;

        private readonly SkillDirectionPresentationLease directedBody=new();
        private Coroutine _playRoutine;
        private Coroutine _oneShotRoutine;

        private AnimationState _currentState = AnimationState.None;
        private AnimationState _previousLocomotionState = AnimationState.Idle;
        private DiagonalDirection _currentDirection;
        private AnimationClip _currentClip;
        private bool _isPlayingOneShot;
        private bool _isDead;
        private SpriteRenderer _comboPresentationRenderer;
        private Sprite _comboBaselineSprite;
        private bool _comboBaselineEnabled;
        private bool _comboPresentationActive;
        private SpritePresentationCalibrationProfileSO _comboCalibration;
        private MaterialPropertyBlock _comboPropertyBlock;
        private bool _comboMirrorsCanonicalAnimation;
        private float _continuousComboMinimumNormalizedFrame;
        private bool _playingAttackDisabledCc;
        private bool _holdingSkillCastPose;
        private float _currentLocomotionPlaybackRate = 1f;
        private object _facingLockOwner;
        private DiagonalDirection _facingLockDirection;

        public AnimationState CurrentState => _currentState;
        public DiagonalDirection CurrentDirection => _currentDirection;
        public bool IsPlayingOneShot => _isPlayingOneShot;
        public bool IsDead => _isDead;
        public float CurrentLocomotionPlaybackRate => _currentLocomotionPlaybackRate;

        public static float CalculateLocomotionPlaybackRate(float movementSpeed)
        {
            if (float.IsNaN(movementSpeed))
            {
                return 1f;
            }

            if (movementSpeed <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp(
                movementSpeed / LocomotionReferenceMoveSpeed,
                LocomotionMinimumPlaybackRate,
                LocomotionMaximumPlaybackRate);
        }
        public float GetAttackDuration()
        {
            AnimationClip clip = GetClip(
                AnimationState.Attack,
                _currentDirection);

            if (clip == null)
            {
                return 0f;
            }

            return clip.length /
                Mathf.Max(0.01f, GetAttackSpeed());
        }

        public float GetAttackFireDelay(float normalizedFireTime = 2f / 3f)
        {
            float duration = GetAttackDuration();

            if (duration <= 0f)
            {
                return 0f;
            }

            return duration * Mathf.Clamp01(normalizedFireTime);
        }

        public bool IsPlayingAttack()
        {
            return _isPlayingOneShot && _currentState == AnimationState.Attack;
        }




        private void Reset()
        {
            ResolveRequiredComponents();
        }

        private void Awake()
        {
            ResolveRequiredComponents();
            _currentDirection = defaultDirection;
        }
        
        private void ResolveRequiredComponents()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }

            if (targetSpriteRenderer == null)
            {
                targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (targetSpriteRenderer == null)
            {
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                targetSpriteRenderer = spriteRenderer != null
                    ? spriteRenderer
                    : gameObject.AddComponent<SpriteRenderer>();
            }

            if (sortingOrderMono == null)
            {
                sortingOrderMono = GetComponent<SortingOrderMono>()
                    ?? gameObject.AddComponent<SortingOrderMono>();
            }

            if (movementController == null)
            {
                movementController =
                    GetComponent<MovementController>()
                    ?? GetComponentInParent<MovementController>()
                    ?? gameObject.AddComponent<MovementController>();
            }

            if (characterManager == null)
            {
                characterManager =
                    GetComponent<CharacterManager>()
                    ?? GetComponentInParent<CharacterManager>()
                    ?? gameObject.AddComponent<CharacterManager>();
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            
            CharacterSO definition = GetComponent<CharacterManager>().RuntimeData.characterSO;
            _animationProfile = definition != null ? definition.AnimationProfile : null;
            if (definition != null && definition.AnimationClips != null)
            {
                animationClips.AddRange(definition.AnimationClips);
            }

            if (playIdleOnStart)
            {
                PlayIdle();
            }
        }

        /// <summary>
        /// Updates the facing direction only.
        /// If a locomotion state is already playing, the clip is refreshed for the new direction.
        /// </summary>
        public void SetDirection(DiagonalDirection direction)
        {
            if (_isDead)
                return;

            if (_facingLockOwner != null && direction != _facingLockDirection)
                return;

            if (_currentDirection == direction)
                return;

            _currentDirection = direction;

            // Direction is renderer state, not root motion. New user-authored body
            // action clips deliberately omit flip curves so the same identity clip
            // can follow the established diagonal mirroring policy.
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.flipX = direction == DiagonalDirection.UpLeft ||
                    direction == DiagonalDirection.DownLeft;
            }

            if (_isPlayingOneShot)
                return;

            if (_currentState == AnimationState.Idle || _currentState == AnimationState.Move)
            {
                PlayState(_currentState, restartIfSameState: true);
            }
        }

        /// <summary>
        /// Convenience method for converting a movement vector into one of the 4 diagonal directions.
        /// </summary>
        public void SetDirectionFromVector(Vector2 direction)
        {
            if (_isDead)
                return;

            if (direction.sqrMagnitude <= 0.0001f)
                return;

            float x = direction.x;
            float y = direction.y;

            if (Mathf.Abs(x) <= 0.0001f)
            {
                bool useRight = _currentDirection == DiagonalDirection.UpRight || _currentDirection == DiagonalDirection.DownRight;

                if (y > 0f)
                    SetDirection(useRight ? DiagonalDirection.UpRight : DiagonalDirection.UpLeft);
                else
                    SetDirection(useRight ? DiagonalDirection.DownRight : DiagonalDirection.DownLeft);
                return;
            }

            if (Mathf.Abs(y) <= 0.0001f)
            {
                if (x >= 0f)
                    SetDirection(DiagonalDirection.DownRight);
                else
                    SetDirection(DiagonalDirection.DownLeft);
                return;
            }

            if (x >= 0f && y > 0f)
                SetDirection(DiagonalDirection.UpRight);
            else if (x < 0f && y > 0f)
                SetDirection(DiagonalDirection.UpLeft);
            else if (x >= 0f)
                SetDirection(DiagonalDirection.DownRight);
            else
                SetDirection(DiagonalDirection.DownLeft);
        }

        public bool AcquireFacingLock(object owner, Vector2 direction)
        {
            if (owner == null || direction.sqrMagnitude <= .0001f || _isDead)
                return false;
            if (_facingLockOwner != null && !ReferenceEquals(_facingLockOwner, owner))
                return false;

            _facingLockOwner = null;
            SetDirectionFromVector(direction);
            _facingLockDirection = _currentDirection;
            _facingLockOwner = owner;
            return true;
        }

        public bool RefreshFacingLock(object owner, Vector2 direction)
        {
            if (owner == null || !ReferenceEquals(_facingLockOwner, owner) ||
                direction.sqrMagnitude <= .0001f || _isDead)
                return false;

            _facingLockOwner = null;
            SetDirectionFromVector(direction);
            _facingLockDirection = _currentDirection;
            _facingLockOwner = owner;
            return true;
        }

        public void ReleaseFacingLock(object owner)
        {
            if (owner == null || !ReferenceEquals(_facingLockOwner, owner))
                return;
            _facingLockOwner = null;
        }

        public void UpdateDirectionFromMovementController()
        {
            if (_isDead)
                return;

            if (movementController == null)
                return;

            Vector2 direction = GetDirectionFromMovementController();
            if (direction.sqrMagnitude <= 0.0001f)
                return;

            SetDirectionFromVector(direction);
        }

        public Vector2 GetDirectionFromMovementController()
        {
            if (movementController == null)
                return Vector2.zero;

            Vector2 direction = movementController.CurrentDirection;
            if (direction.sqrMagnitude > 0.0001f)
                return direction;

            Vector2 velocity = movementController.CurrentVelocity;
            if (velocity.sqrMagnitude > 0.0001f)
                return velocity.normalized;

            return Vector2.zero;
        }

        public void PlayIdle()
        {
            if (!EnsurePresentationActive()) return;
            if (_isDead || IsAttackDisabledByCc())
                return;

            if (_isPlayingOneShot)
                return;

            PlayState(AnimationState.Idle);
        }

        public void PlayMove()
        {
            if (!EnsurePresentationActive()) return;
            if (_isDead || IsAttackDisabledByCc())
                return;

            if (_isPlayingOneShot)
                return;

            if (ResolveLocomotionPlaybackRate() <= 0f)
            {
                PlayIdle();
                return;
            }

            UpdateDirectionFromMovementController();
            PlayState(AnimationState.Move);
        }

        public void PlayAttack()
        {
            if (_isDead || IsAttackDisabledByCc())
                return;

            AnimationClip clip = GetClip(AnimationState.Attack, _currentDirection);
            if (!CanPlayClip(clip, AnimationState.Attack, _currentDirection))
                return;

            if (_isPlayingOneShot && _currentState == AnimationState.Attack && _currentClip == clip)
                return;

            StopOneShotRoutine();
            StopPlayRoutine();
            float attackSpeed = GetAttackSpeed();

            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = clip;
            _playRoutine = StartPresentationRoutine(PlayOneShotClipRoutine(clip, attackSpeed));
            _oneShotRoutine = StartPresentationRoutine(PlayAttackRoutine(clip, attackSpeed));
        }

        public void RestartAttack()
        {
            if (_isDead)
                return;

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = false;
            _currentState = AnimationState.None;
            _currentClip = null;
            PlayAttack();
        }

        // Read-only target for the battle transition's late sprite sampler; no gameplay/CC state is acquired.
        internal SpriteRenderer TransitionBodyRenderer => targetSpriteRenderer;

        public bool PlayDirectedSkillBodyAction(AnimationClip fallback,float duration,Vector2 snapshot,SkillDirectionPresentationProfile profile)
        {
            if(!EnsurePresentationActive()||_isDead||IsAttackDisabledByCc()||targetSpriteRenderer==null||!SkillDirectionMath.Valid(snapshot))return false;
            profile??=new SkillDirectionPresentationProfile();
            AnimationClip clip=profile.ResolveBody(snapshot,fallback,out float angle,out bool flip);
            if(clip==null)return false; // Missing visual never acquires gameplay state.
            StopOneShotRoutine();StopPlayRoutine();
            SetDirectionFromVector(snapshot);
            _holdingSkillCastPose=false;_isPlayingOneShot=true;_currentState=AnimationState.Attack;_currentClip=clip;
            directedBody.Begin(targetSpriteRenderer,angle,flip);
            _playRoutine=StartPresentationRoutine(PlaySkillBodyActionRoutine(clip,Mathf.Max(.01f,duration),true));
            return true;
        }
        public void CancelDirectedSkillPresentation()
        {
            if(!directedBody.Active)return;
            StopOneShotRoutine();StopPlayRoutine();_isPlayingOneShot=false;_currentClip=null;_currentState=AnimationState.None;
        }

        public bool PlaySkillBodyAction(AnimationClip clip, float duration)
        {
            return PlaySkillBodyAction(clip, duration, true);
        }

        public bool PlaySkillBodyAction(AnimationClip clip, float duration, bool mirrorWithFacing)
        {
            if (!EnsurePresentationActive()) return false;
            if (_isDead || IsAttackDisabledByCc() || clip == null || targetSpriteRenderer == null)
                return false;

            if (_holdingSkillCastPose && _currentClip == clip)
            {
                _holdingSkillCastPose = false;
                _playRoutine = StartPresentationRoutine(PlaySkillBodyActionRoutine(
                    clip, Mathf.Max(.01f, duration), mirrorWithFacing));
                return true;
            }

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = clip;
            if (!mirrorWithFacing) targetSpriteRenderer.flipX = false;
            _playRoutine = StartPresentationRoutine(PlaySkillBodyActionRoutine(
                clip, Mathf.Max(.01f, duration), mirrorWithFacing));
            return true;
        }

        public bool HoldSkillBodyActionCastPose(AnimationClip clip, bool mirrorWithFacing)
        {
            if (_isDead || IsAttackDisabledByCc() || clip == null || targetSpriteRenderer == null)
                return false;

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = clip;
            _holdingSkillCastPose = true;
            if (!mirrorWithFacing)
                targetSpriteRenderer.flipX = false;
            clip.SampleAnimation(gameObject, 0f);
            ApplyFacingLockAfterAnimationSample();
            return true;
        }

        public bool IsHoldingSkillBodyActionCastPose(AnimationClip clip)
        {
            return _holdingSkillCastPose && clip != null && _currentClip == clip;
        }

        public void ReleaseSkillBodyActionCastPose()
        {
            if (!_holdingSkillCastPose)
                return;

            _holdingSkillCastPose = false;
            _isPlayingOneShot = false;
            _currentClip = null;
            _currentState = AnimationState.None;
            RestoreFacingMirror();
            PlayIdle();
        }

        public void StopSkillBodyAction()
        {
            if (!_isPlayingOneShot && !_holdingSkillCastPose)
                return;
            _holdingSkillCastPose = false;
            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = false;
            _currentClip = null;
            _currentState = AnimationState.None;
            RestoreFacingMirror();
            PlayIdle();
        }

        public bool RestartComboAction(
            AnimationClip clip,
            float startToHitDuration,
            float hitToRecoveryDuration,
            SpritePresentationCalibrationProfileSO calibration = null)
        {
            if (_isDead || clip == null || targetSpriteRenderer == null)
            {
                return false;
            }

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = clip;
            BeginComboPresentation(calibration);
            _playRoutine = StartPresentationRoutine(PlayComboActionRoutine(
                clip,
                Mathf.Max(.001f, startToHitDuration),
                Mathf.Max(.001f, hitToRecoveryDuration)));
            return true;
        }

        public bool RestartContinuousComboAction(
            AnimationClip clip,
            SkillComboProfile combo,
            SpritePresentationCalibrationProfileSO calibration = null)
        {
            if (_isDead || clip == null || targetSpriteRenderer == null ||
                combo == null || !combo.HasCompleteSegmentedBodyRegistry)
            {
                return false;
            }

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = clip;
            _continuousComboMinimumNormalizedFrame = 0f;
            BeginComboPresentation(calibration);
            _playRoutine = StartPresentationRoutine(PlayContinuousComboActionRoutine(clip, combo));
            return true;
        }

        public bool SynchronizeContinuousComboContact(
            AnimationClip clip,
            SkillComboStep step,
            int totalFrames)
        {
            if (!_isPlayingOneShot || _currentClip != clip || clip == null || step == null ||
                targetSpriteRenderer == null || totalFrames < 2 ||
                !step.HasBodySegment(step.BodySegmentStartFrame, step.BodySegmentEndFrame, totalFrames))
            {
                return false;
            }

            int contactFrame = step.BodySegmentStartFrame + 4;
            float normalizedContact = contactFrame / (totalFrames - 1f);
            _continuousComboMinimumNormalizedFrame = Mathf.Max(
                _continuousComboMinimumNormalizedFrame, normalizedContact);
            clip.SampleAnimation(gameObject, clip.length * normalizedContact);
            ApplyComboPresentation();
            return true;
        }

        public bool RestartCanonicalComboChoreography(
            int comboIndex,
            float startToHitDuration,
            float hitToRecoveryDuration)
        {
            if (_isDead || comboIndex < 0 || comboIndex > 2 || targetSpriteRenderer == null)
            {
                return false;
            }

            AnimationClip attackClip = GetClip(AnimationState.Attack, _currentDirection);
            if (attackClip == null)
            {
                // Leave the canonical renderer visible. A missing clip must never turn an
                // idle snapshot into a fake attack or suppress gameplay.
                EndComboPresentation();
                RestartAttack();
                return false;
            }

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = attackClip;
            // Sample F0 before hiding the canonical renderer so the proxy never flashes
            // the previous idle frame.
            attackClip.SampleAnimation(gameObject, 0f);
            BeginComboPresentation(null, true, true);
            _playRoutine = StartPresentationRoutine(PlayCanonicalComboChoreographyRoutine(
                attackClip,
                comboIndex,
                Mathf.Max(.001f, startToHitDuration),
                Mathf.Max(.001f, hitToRecoveryDuration)));
            return true;
        }

        public void StopComboAction()
        {
            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = false;
            _currentClip = null;
        }

        public void StopAnimation()
        {
            StopOneShotRoutine();
            StopPlayRoutine();
            _currentState = AnimationState.None;
            _isPlayingOneShot = false;
            _currentClip = null;
        }

        public void PlayDeath()
        {
            PlayDeath(0f);
        }

        public void PlayDeath(float presentationDuration)
        {
            if (_isDead && _currentState == AnimationState.Death)
                return;

            _isDead = true;
            _facingLockOwner = null;
            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = false;

            AnimationClip clip = GetClip(AnimationState.Death, _currentDirection);
            if (clip == null)
            {
                _currentState = AnimationState.None;
                _currentClip = null;
                return;
            }

            if (!CanPlayClip(clip, AnimationState.Death, _currentDirection))
                return;

            _currentState = AnimationState.Death;
            _currentClip = clip;
            _playRoutine = presentationDuration > 0f
                ? StartPresentationRoutine(PlayDeathDurationRoutine(clip, presentationDuration))
                : StartPresentationRoutine(PlayHoldLastFrameClipRoutine(clip));
        }

        private IEnumerator PlayDeathDurationRoutine(AnimationClip clip, float duration)
        {
            float elapsed = 0f;
            duration = Mathf.Max(.01f, duration);
            while (elapsed < duration)
            {
                clip.SampleAnimation(gameObject, clip.length * Mathf.Clamp01(elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }

            clip.SampleAnimation(gameObject, clip.length);
            _playRoutine = null;
        }

        public void SetDead(bool dead)
        {
            if (!dead)
            {
                _isDead = false;
                return;
            }

            PlayDeath();
        }

        private void PlayState(AnimationState state, bool restartIfSameState = false)
        {
            if (!EnsurePresentationActive()) return;
            if (_isDead && state != AnimationState.Death)
                return;

            if (_isPlayingOneShot && state != AnimationState.Attack)
                return;

            AnimationClip clip = GetClip(state, _currentDirection);
            if (!CanPlayClip(clip, state, _currentDirection))
                return;

            if (!restartIfSameState && _currentState == state && _currentClip == clip && !_isPlayingOneShot)
                return;

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = false;
            _currentState = state;

            if (state == AnimationState.Idle || state == AnimationState.Move)
            {
                _previousLocomotionState = state;
            }

            _currentClip = clip;
            _playRoutine = StartPresentationRoutine(PlayLoopClipRoutine(clip));
        }

        private float GetAttackSpeed()
        {
            if (characterManager == null)
            {
                characterManager = GetComponent<CharacterManager>();

                if (characterManager == null)
                    characterManager = GetComponentInParent<CharacterManager>();
            }

            if (characterManager == null)
            {
                return 1f;
            }

            float attackSpeed =
                characterManager.GetStatValue(StatType.AttackSpeed);

            if (attackSpeed <= 0f)
            {
                return 1f;
            }

            return attackSpeed;
        }

        private IEnumerator PlayAttackRoutine(AnimationClip clip, float attackSpeed)
        {
            float animationDuration =
                Mathf.Max(0.01f, clip.length / Mathf.Max(0.01f, attackSpeed));

            yield return new WaitForSeconds(animationDuration);

            AnimationState nextState = _previousLocomotionState == AnimationState.Move
                ? AnimationState.Move
                : AnimationState.Idle;

            if (resetToPreviousLocomotionAfterAttack)
            {
                PlayLocomotionVisualOnly(AnimationState.Idle);
            }

            if (AttackRecoveryDelay > 0f)
            {
                float recoveryDelay =
                    AttackRecoveryDelay /
                    Mathf.Max(0.01f, attackSpeed);

                yield return new WaitForSeconds(recoveryDelay);
            }

            _oneShotRoutine = null;
            _isPlayingOneShot = false;
            _currentClip = null;

            if (resetToPreviousLocomotionAfterAttack)
            {
                PlayState(nextState, restartIfSameState: true);
            }
            else
            {
                _currentState = AnimationState.None;
            }
        }

        private void PlayLocomotionVisualOnly(AnimationState state)
        {
            if (state != AnimationState.Idle && state != AnimationState.Move)
            {
                return;
            }

            AnimationClip clip = GetClip(state, _currentDirection);

            if (!CanPlayClip(clip, state, _currentDirection))
            {
                return;
            }

            StopPlayRoutine();
            _currentClip = clip;
            _playRoutine = StartPresentationRoutine(PlayLoopClipRoutine(clip));
        }

        private AnimationClip GetClip(AnimationState state, DiagonalDirection direction)
        {
            CharacterAnimationClipType clipType = ToAnimationClipType(
                state,
                direction);

            CharacterAnimationSlot slot;
            switch (state)
            {
                case AnimationState.Move: slot = CharacterAnimationSlot.Move; break;
                case AnimationState.Attack: slot = CharacterAnimationSlot.BasicAttack; break;
                case AnimationState.AttackDisabledCc: slot = CharacterAnimationSlot.AttackDisabledCc; break;
                case AnimationState.Death: slot = CharacterAnimationSlot.Death; break;
                default: slot = CharacterAnimationSlot.Idle; break;
            }

            if (_animationProfile != null &&
                _animationProfile.TryGetState(slot, out CharacterAnimationStateEntry stateEntry) &&
                stateEntry.DirectionalClips != null)
            {
                foreach (CharacterAnimationClipEntry entry in stateEntry.DirectionalClips)
                {
                    if (entry != null && entry.clipType == clipType && entry.clip != null)
                    {
                        return entry.clip;
                    }
                }
            }

            if (animationClips == null)
            {
                return null;
            }

            foreach (CharacterAnimationClipEntry entry in animationClips)
            {
                if (entry != null && entry.clipType == clipType)
                {
                    return entry.clip;
                }
            }

            return null;
        }

        private static CharacterAnimationClipType ToAnimationClipType(
            AnimationState state,
            DiagonalDirection direction)
        {
            switch (state)
            {
                case AnimationState.Idle:
                    switch (direction)
                    {
                        case DiagonalDirection.UpRight:
                            return CharacterAnimationClipType.IdleUpRight;
                        case DiagonalDirection.UpLeft:
                            return CharacterAnimationClipType.IdleUpLeft;
                        case DiagonalDirection.DownLeft:
                            return CharacterAnimationClipType.IdleDownLeft;
                        default:
                            return CharacterAnimationClipType.IdleDownRight;
                    }

                case AnimationState.Move:
                    switch (direction)
                    {
                        case DiagonalDirection.UpRight:
                            return CharacterAnimationClipType.MoveUpRight;
                        case DiagonalDirection.UpLeft:
                            return CharacterAnimationClipType.MoveUpLeft;
                        case DiagonalDirection.DownLeft:
                            return CharacterAnimationClipType.MoveDownLeft;
                        default:
                            return CharacterAnimationClipType.MoveDownRight;
                    }

                case AnimationState.Attack:
                    switch (direction)
                    {
                        case DiagonalDirection.UpRight:
                            return CharacterAnimationClipType.AttackUpRight;
                        case DiagonalDirection.UpLeft:
                            return CharacterAnimationClipType.AttackUpLeft;
                        case DiagonalDirection.DownLeft:
                            return CharacterAnimationClipType.AttackDownLeft;
                        default:
                            return CharacterAnimationClipType.AttackDownRight;
                    }

                case AnimationState.Death:
                    switch (direction)
                    {
                        case DiagonalDirection.UpRight:
                            return CharacterAnimationClipType.DeathUpRight;
                        case DiagonalDirection.UpLeft:
                            return CharacterAnimationClipType.DeathUpLeft;
                        case DiagonalDirection.DownLeft:
                            return CharacterAnimationClipType.DeathDownLeft;
                        default:
                            return CharacterAnimationClipType.DeathDownRight;
                    }

                case AnimationState.AttackDisabledCc:
                    switch (direction)
                    {
                        case DiagonalDirection.UpRight: return CharacterAnimationClipType.IdleUpRight;
                        case DiagonalDirection.UpLeft: return CharacterAnimationClipType.IdleUpLeft;
                        case DiagonalDirection.DownLeft: return CharacterAnimationClipType.IdleDownLeft;
                        default: return CharacterAnimationClipType.IdleDownRight;
                    }

                default:
                    return CharacterAnimationClipType.IdleDownRight;
            }
        }

        private bool CanPlayClip(AnimationClip clip, AnimationState state, DiagonalDirection direction)
        {
            if (!EnsurePresentationActive()) return false;
            if (targetSpriteRenderer == null)
            {
                Debug.LogWarning($"[{nameof(AnimationMono)}] SpriteRenderer is not assigned or found under {name}.", this);
                return false;
            }

            if (clip == null)
            {
                Debug.LogWarning($"[{nameof(AnimationMono)}] Missing clip for state {state} / direction {direction} on {name}.", this);
                return false;
            }

            return true;
        }

        private IEnumerator PlayLoopClipRoutine(AnimationClip clip)
        {
            if (clip == null)
                yield break;

            Debug.Log($"[{nameof(AnimationMono)}] PlayLoopClip -> {clip.name} on {name}", this);

            float length = Mathf.Max(0.01f, clip.length);
            float time = 0f;

            while (true)
            {
                clip.SampleAnimation(gameObject, time);
                float playbackRate = _currentState == AnimationState.Move
                    ? ResolveLocomotionPlaybackRate()
                    : 1f;
                _currentLocomotionPlaybackRate = playbackRate;
                time += Time.deltaTime * playbackRate;

                if (time >= length)
                {
                    time %= length;
                }

                yield return null;
            }
        }

        private float ResolveLocomotionPlaybackRate()
        {
            // Use the locomotion owner's configured speed rather than raw
            // Rigidbody velocity. This includes MoveSpeed buffs and tactical
            // multipliers while excluding knockback, dash and forced motion.
            float movementSpeed = movementController != null
                ? movementController.MoveSpeed
                : LocomotionReferenceMoveSpeed;
            return CalculateLocomotionPlaybackRate(movementSpeed);
        }

        private IEnumerator PlayOneShotClipRoutine(AnimationClip clip, float speedMultiplier = 1f)
        {
            if (clip == null)
                yield break;

            Debug.Log($"[{nameof(AnimationMono)}] PlayOneShotClip -> {clip.name} on {name}", this);

            float length = Mathf.Max(0.01f, clip.length);
            float time = 0f;
            float speed = Mathf.Max(0.01f, speedMultiplier);

            while (time < length)
            {
                clip.SampleAnimation(gameObject, time);
                time += Time.deltaTime * speed;
                yield return null;
            }

            clip.SampleAnimation(gameObject, length);
            _playRoutine = null;
        }

        private IEnumerator PlaySkillBodyActionRoutine(AnimationClip clip, float duration, bool mirrorWithFacing)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float normalized = Mathf.Clamp01(elapsed / duration);
                clip.SampleAnimation(gameObject, clip.length * normalized);
                directedBody.Apply();
                elapsed += Time.deltaTime;
                yield return null;
            }

            clip.SampleAnimation(gameObject, clip.length);
            directedBody.Restore();
            _playRoutine = null;
            _isPlayingOneShot = false;
            _currentClip = null;
            _currentState = AnimationState.None;
            if (!mirrorWithFacing && targetSpriteRenderer != null)
            {
                targetSpriteRenderer.flipX = _currentDirection == DiagonalDirection.UpLeft ||
                    _currentDirection == DiagonalDirection.DownLeft;
            }
            PlayIdle();
        }

        private IEnumerator PlayComboActionRoutine(
            AnimationClip clip,
            float startToHitDuration,
            float hitToRecoveryDuration)
        {
            const float hitNormalizedTime = .8f; // F4 of exact6.
            float elapsed = 0f;
            while (elapsed < startToHitDuration)
            {
                float normalized = Mathf.Clamp01(elapsed / startToHitDuration) * hitNormalizedTime;
                clip.SampleAnimation(gameObject, clip.length * normalized);
                ApplyComboPresentation();
                elapsed += Time.deltaTime;
                yield return null;
            }

            clip.SampleAnimation(gameObject, clip.length * hitNormalizedTime);
            ApplyComboPresentation();
            elapsed = 0f;
            while (elapsed < hitToRecoveryDuration)
            {
                float normalized = Mathf.Lerp(
                    hitNormalizedTime,
                    1f,
                    Mathf.Clamp01(elapsed / hitToRecoveryDuration));
                clip.SampleAnimation(gameObject, clip.length * normalized);
                ApplyComboPresentation();
                elapsed += Time.deltaTime;
                yield return null;
            }

            clip.SampleAnimation(gameObject, clip.length);
            ApplyComboPresentation();
            EndComboPresentation();
            _playRoutine = null;
            _isPlayingOneShot = false;
            _currentClip = null;
        }

        private IEnumerator PlayContinuousComboActionRoutine(
            AnimationClip clip,
            SkillComboProfile combo)
        {
            float elapsed = 0f;
            SkillComboStep[] steps = combo.Steps;
            while (elapsed < combo.Duration)
            {
                float normalizedFrame = ResolveContinuousComboFrame(
                    elapsed, steps, combo.SegmentedBodyFrameCount);
                normalizedFrame = Mathf.Max(
                    normalizedFrame, _continuousComboMinimumNormalizedFrame);
                clip.SampleAnimation(gameObject, clip.length * normalizedFrame);
                ApplyComboPresentation();
                elapsed += Time.deltaTime;
                yield return null;
            }

            clip.SampleAnimation(gameObject, clip.length);
            ApplyComboPresentation();
            EndComboPresentation();
            _playRoutine = null;
            _isPlayingOneShot = false;
            _currentClip = null;
            _continuousComboMinimumNormalizedFrame = 0f;
        }

        private IEnumerator PlayCanonicalComboChoreographyRoutine(
            AnimationClip attackClip,
            int comboIndex,
            float startToHitDuration,
            float hitToRecoveryDuration)
        {
            float elapsed = 0f;
            while (elapsed < startToHitDuration)
            {
                float normalized = .8f * Mathf.Clamp01(elapsed / startToHitDuration);
                attackClip.SampleAnimation(gameObject, attackClip.length * normalized);
                ApplyCanonicalComboPose(comboIndex, normalized);
                elapsed += Time.deltaTime;
                yield return null;
            }

            attackClip.SampleAnimation(gameObject, attackClip.length * .8f);
            ApplyCanonicalComboPose(comboIndex, .8f);
            elapsed = 0f;
            while (elapsed < hitToRecoveryDuration)
            {
                float normalized = Mathf.Lerp(.8f, 1f, Mathf.Clamp01(elapsed / hitToRecoveryDuration));
                attackClip.SampleAnimation(gameObject, attackClip.length * normalized);
                ApplyCanonicalComboPose(comboIndex, normalized);
                elapsed += Time.deltaTime;
                yield return null;
            }

            attackClip.SampleAnimation(gameObject, attackClip.length);
            ApplyCanonicalComboPose(comboIndex, 1f);
            EndComboPresentation();
            _playRoutine = null;
            _isPlayingOneShot = false;
            _currentClip = null;
        }

        private void ApplyCanonicalComboPose(int comboIndex, float normalized)
        {
            ApplyComboPresentation();
            if (!_comboPresentationActive || _comboPresentationRenderer == null) return;

            Vector2 preparationPosition;
            Vector2 contactPosition;
            Vector2 preparationScale;
            Vector2 contactScale;
            float preparationRotation;
            float contactRotation;
            switch (comboIndex)
            {
                case 0:
                    preparationPosition = new Vector2(-.02f, 0f);
                    contactPosition = new Vector2(.055f, -.008f);
                    preparationScale = new Vector2(1.02f, .99f);
                    contactScale = new Vector2(1.10f, .93f);
                    preparationRotation = -4f;
                    contactRotation = 7f;
                    break;
                case 1:
                    preparationPosition = new Vector2(-.045f, .01f);
                    contactPosition = new Vector2(.085f, -.005f);
                    preparationScale = new Vector2(.98f, 1.02f);
                    contactScale = new Vector2(1.14f, .91f);
                    preparationRotation = 6f;
                    contactRotation = -11f;
                    break;
                default:
                    preparationPosition = new Vector2(-.055f, -.025f);
                    contactPosition = new Vector2(.12f, -.025f);
                    preparationScale = new Vector2(1.03f, .96f);
                    contactScale = new Vector2(1.18f, .86f);
                    preparationRotation = -7f;
                    contactRotation = 13f;
                    break;
            }

            Vector2 position;
            Vector2 scale;
            float rotation;
            if (normalized <= .8f)
            {
                float t = Mathf.SmoothStep(0f, 1f, normalized / .8f);
                position = Vector2.Lerp(preparationPosition, contactPosition, t);
                scale = Vector2.Lerp(preparationScale, contactScale, t);
                rotation = Mathf.Lerp(preparationRotation, contactRotation, t);
            }
            else if (comboIndex == 2 && normalized <= .9f)
            {
                float t = Mathf.InverseLerp(.8f, .9f, normalized);
                position = Vector2.Lerp(contactPosition, new Vector2(.145f, -.02f), t);
                scale = Vector2.Lerp(contactScale, new Vector2(1.20f, .84f), t);
                rotation = Mathf.Lerp(contactRotation, 16f, t);
            }
            else
            {
                float recoveryStart = comboIndex == 2 ? .9f : .8f;
                Vector2 recoveryPosition = comboIndex == 2 ? new Vector2(.145f, -.02f) : contactPosition;
                Vector2 recoveryScale = comboIndex == 2 ? new Vector2(1.20f, .84f) : contactScale;
                float recoveryRotation = comboIndex == 2 ? 16f : contactRotation;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(recoveryStart, 1f, normalized));
                position = Vector2.Lerp(recoveryPosition, Vector2.zero, t);
                scale = Vector2.Lerp(recoveryScale, Vector2.one, t);
                rotation = Mathf.Lerp(recoveryRotation, 0f, t);
            }

            Transform proxy = _comboPresentationRenderer.transform;
            proxy.localPosition = new Vector3(position.x, position.y, 0f);
            proxy.localScale = new Vector3(scale.x, scale.y, 1f);
            proxy.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private static float ResolveContinuousComboFrame(
            float elapsed,
            SkillComboStep[] steps,
            int totalFrames)
        {
            if (steps == null || steps.Length == 0 || totalFrames < 2) return 0f;
            float denominator = totalFrames - 1f;
            for (int i = 0; i < steps.Length; i++)
            {
                SkillComboStep step = steps[i];
                if (step == null || elapsed > step.RecoveryEnd) continue;
                if (elapsed < step.StartTime)
                {
                    int holdFrame = i == 0
                        ? step.BodySegmentStartFrame
                        : steps[i - 1].BodySegmentEndFrame;
                    return holdFrame / denominator;
                }
                float start = step.BodySegmentStartFrame / denominator;
                float contact = (step.BodySegmentStartFrame + 4f) / denominator;
                float end = step.BodySegmentEndFrame / denominator;
                if (elapsed <= step.HitTime)
                {
                    return Mathf.Lerp(start, contact,
                        Mathf.InverseLerp(step.StartTime, step.HitTime, elapsed));
                }
                return Mathf.Lerp(contact, end,
                    Mathf.InverseLerp(step.HitTime, step.RecoveryEnd, elapsed));
            }
            return 1f;
        }

        private IEnumerator PlayHoldLastFrameClipRoutine(AnimationClip clip)
        {
            if (clip == null)
                yield break;

            float length = Mathf.Max(0.01f, clip.length);
            float time = 0f;

            while (time < length)
            {
                clip.SampleAnimation(gameObject, time);
                time += Time.deltaTime;
                yield return null;
            }

            clip.SampleAnimation(gameObject, length);
            _playRoutine = null;
        }

        private void StopPlayRoutine()
        {
            directedBody.Restore();
            EndComboPresentation();
            _currentLocomotionPlaybackRate = 1f;
            if (_playRoutine != null)
            {
                _currentClip = null;
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }
        }

        private void StopOneShotRoutine()
        {
            if (_oneShotRoutine != null)
            {
                StopCoroutine(_oneShotRoutine);
                _oneShotRoutine = null;
            }
        }

        // Final gate for every presentation coroutine, including recovery/CC paths.
        private Coroutine StartPresentationRoutine(IEnumerator routine)
        {
            if (!EnsurePresentationActive()) return null;
            return StartCoroutine(routine);
        }

        private bool EnsurePresentationActive()
        {
            if (_presentationTeardownDepth == 0 && isActiveAndEnabled && gameObject.activeInHierarchy) return true;
            ResetInactivePresentation();
            return false;
        }

        private int _presentationTeardownDepth;
        internal void BeginSynchronousTeardown()
        {
            _presentationTeardownDepth++;
            ResetInactivePresentation();
        }
        internal void EndSynchronousTeardown()
        {
            if (_presentationTeardownDepth > 0) _presentationTeardownDepth--;
        }

        private void OnDestroy() => ResetInactivePresentation();
        private void OnDisable() => ResetInactivePresentation();

        private void ResetInactivePresentation()
        {
            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = false;
            _currentClip = null;
            _currentState = AnimationState.None;
            _playingAttackDisabledCc = false;
            _holdingSkillCastPose = false;
            _facingLockOwner = null;
            RestoreFacingMirror();
            EndComboPresentation();
        }

        private void BeginComboPresentation(
            SpritePresentationCalibrationProfileSO calibration,
            bool allowCanonicalSprite = false,
            bool mirrorCanonicalAnimation = false)
        {
            EndComboPresentation();
            if ((!allowCanonicalSprite && calibration == null) || targetSpriteRenderer == null) return;

            _comboBaselineSprite = targetSpriteRenderer.sprite;
            _comboBaselineEnabled = targetSpriteRenderer.enabled;
            _comboCalibration = calibration;
            _comboMirrorsCanonicalAnimation = mirrorCanonicalAnimation;

            _comboPresentationRenderer = AcquireComboPresentationRenderer();
            if (_comboPresentationRenderer == null ||
                !CopyRendererPresentation(targetSpriteRenderer, _comboPresentationRenderer))
            {
                _comboPresentationRenderer = null;
                _comboCalibration = null;
                _comboMirrorsCanonicalAnimation = false;
                targetSpriteRenderer.enabled = _comboBaselineEnabled;
                return;
            }
            _comboPresentationRenderer.enabled = true;
            targetSpriteRenderer.enabled = false;
            _comboPresentationActive = true;
        }

        private void LateUpdate()
        {
            directedBody.Apply();
            UpdateAttackDisabledCcPresentation();
            ApplyFacingLockAfterAnimationSample();
            // Animation clips are sampled before this point. Mirror the current sampled
            // sprite, not the sprite captured when presentation began.
            if (!_comboPresentationActive || !_comboMirrorsCanonicalAnimation ||
                _comboPresentationRenderer == null || targetSpriteRenderer == null)
                return;

            _comboPresentationRenderer.sprite = targetSpriteRenderer.sprite;
            CopyRendererPresentation(targetSpriteRenderer, _comboPresentationRenderer);
        }

        private void ApplyFacingLockAfterAnimationSample()
        {
            if (_facingLockOwner == null || targetSpriteRenderer == null)
                return;
            targetSpriteRenderer.flipX =
                _facingLockDirection == DiagonalDirection.UpLeft ||
                _facingLockDirection == DiagonalDirection.DownLeft;
            if(_comboPresentationActive&&_comboPresentationRenderer!=null)
                _comboPresentationRenderer.flipX=targetSpriteRenderer.flipX;
        }

        private bool IsAttackDisabledByCc()
        {
            return characterManager != null && characterManager.IsStunned;
        }

        private void RestoreFacingMirror()
        {
            if (targetSpriteRenderer == null) return;
            targetSpriteRenderer.flipX = _currentDirection == DiagonalDirection.UpLeft ||
                _currentDirection == DiagonalDirection.DownLeft;
        }

        private void UpdateAttackDisabledCcPresentation()
        {
            if (_isDead) return;
            bool disabled = IsAttackDisabledByCc();
            if (disabled == _playingAttackDisabledCc) return;
            _playingAttackDisabledCc = disabled;
            if (disabled)
            {
                AnimationClip clip = GetClip(AnimationState.AttackDisabledCc, _currentDirection);
                if (_animationProfile == null ||
                    !_animationProfile.TryGetState(CharacterAnimationSlot.AttackDisabledCc, out _) ||
                    clip == null)
                {
                    StopOneShotRoutine();
                    StopPlayRoutine();
                    _isPlayingOneShot = false;
                    return;
                }
                StopOneShotRoutine();
                StopPlayRoutine();
                _isPlayingOneShot = false;
                _currentState = AnimationState.AttackDisabledCc;
                _currentClip = clip;
                _playRoutine = StartPresentationRoutine(PlayLoopClipRoutine(clip));
            }
            else
            {
                StopPlayRoutine();
                _currentState = AnimationState.None;
                _currentClip = null;
                PlayIdle();
            }
        }

        private SpriteRenderer AcquireComboPresentationRenderer()
        {
            if (targetSpriteRenderer == null) return null;
            Transform owner = targetSpriteRenderer.transform;
            ComboBodyPresentationProxyMarker[] markers =
                owner.GetComponentsInChildren<ComboBodyPresentationProxyMarker>(true);
            Array.Sort(markers, CompareProxyMarkers);

            ComboBodyPresentationProxyMarker selected = null;
            for (int i = 0; i < markers.Length; i++)
            {
                ComboBodyPresentationProxyMarker marker = markers[i];
                if (marker == null || marker.transform.parent != owner) continue;
                if (selected == null)
                {
                    selected = marker;
                    continue;
                }
                DisableStaleProxy(marker.gameObject);
            }

            if (selected == null)
            {
                Transform legacy = null;
                for (int i = 0; i < owner.childCount; i++)
                {
                    Transform child = owner.GetChild(i);
                    if (child != null && child.name == "__ComboBodyPresentationProxy")
                    {
                        if (legacy == null) legacy = child;
                        else DisableStaleProxy(child.gameObject);
                    }
                }
                if (legacy == null)
                {
                    legacy = new GameObject("__ComboBodyPresentationProxy").transform;
                    legacy.SetParent(owner, false);
                }
                selected = legacy.GetComponent<ComboBodyPresentationProxyMarker>();
                if (selected == null)
                {
                    try { selected = legacy.gameObject.AddComponent<ComboBodyPresentationProxyMarker>(); }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"[AnimationMono] Cannot claim combo presentation proxy: {exception.Message}");
                        return null;
                    }
                }
            }

            SpriteRenderer renderer = selected.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                try { renderer = selected.gameObject.AddComponent<SpriteRenderer>(); }
                catch (Exception exception)
                {
                    Debug.LogWarning($"[AnimationMono] Cannot repair combo presentation renderer: {exception.Message}");
                    selected.gameObject.SetActive(false);
                    return null;
                }
            }
            if (renderer == null) return null;
            selected.gameObject.SetActive(true);
            return renderer;
        }

        private static int CompareProxyMarkers(
            ComboBodyPresentationProxyMarker left,
            ComboBodyPresentationProxyMarker right)
        {
            if (left == right) return 0;
            if (left == null) return 1;
            if (right == null) return -1;
            int sibling = left.transform.GetSiblingIndex().CompareTo(right.transform.GetSiblingIndex());
            return sibling != 0 ? sibling : left.GetInstanceID().CompareTo(right.GetInstanceID());
        }

        private static void DisableStaleProxy(GameObject proxy)
        {
            if (proxy == null) return;
            SpriteRenderer staleRenderer = proxy.GetComponent<SpriteRenderer>();
            if (staleRenderer != null)
            {
                staleRenderer.enabled = false;
                staleRenderer.sprite = null;
                staleRenderer.transform.localPosition = Vector3.zero;
                staleRenderer.transform.localRotation = Quaternion.identity;
                staleRenderer.transform.localScale = Vector3.one;
            }
            proxy.SetActive(false);
        }

        private void ApplyComboPresentation()
        {
            ApplyFacingLockAfterAnimationSample();
            if (!_comboPresentationActive) return;
            if (_comboPresentationRenderer == null || targetSpriteRenderer == null)
            {
                EndComboPresentation();
                return;
            }
            _comboPresentationRenderer.sprite = targetSpriteRenderer.sprite;
            if (!CopyRendererPresentation(targetSpriteRenderer, _comboPresentationRenderer))
            {
                EndComboPresentation();
                return;
            }
            if (_comboCalibration != null && _comboCalibration.TryResolve(targetSpriteRenderer.sprite, out Vector2 scale, out Vector2 offset))
            {
                _comboPresentationRenderer.transform.localScale = new Vector3(scale.x, scale.y, 1f);
                _comboPresentationRenderer.transform.localPosition = new Vector3(offset.x, offset.y, 0f);
            }
            else
            {
                _comboPresentationRenderer.transform.localScale = Vector3.one;
                _comboPresentationRenderer.transform.localPosition = Vector3.zero;
                _comboPresentationRenderer.transform.localRotation = Quaternion.identity;
            }
        }

        private void EndComboPresentation()
        {
            if (!_comboPresentationActive) return;
            if (targetSpriteRenderer != null)
            {
                if (!_comboMirrorsCanonicalAnimation)
                    targetSpriteRenderer.sprite = _comboBaselineSprite;
                targetSpriteRenderer.enabled = _comboBaselineEnabled;
            }
            if (_comboPresentationRenderer != null)
            {
                _comboPresentationRenderer.enabled = false;
                _comboPresentationRenderer.sprite = null;
                _comboPresentationRenderer.transform.localScale = Vector3.one;
                _comboPresentationRenderer.transform.localPosition = Vector3.zero;
                _comboPresentationRenderer.transform.localRotation = Quaternion.identity;
            }
            _comboCalibration = null;
            _comboMirrorsCanonicalAnimation = false;
            _comboPresentationActive = false;
        }

        private bool CopyRendererPresentation(SpriteRenderer source, SpriteRenderer destination)
        {
            if (source == null || destination == null) return false;
            _comboPropertyBlock ??= new MaterialPropertyBlock();
            destination.sharedMaterial = source.sharedMaterial;
            destination.color = source.color;
            destination.flipX = source.flipX;
            destination.flipY = source.flipY;
            destination.sortingLayerID = source.sortingLayerID;
            destination.sortingOrder = source.sortingOrder;
            destination.maskInteraction = source.maskInteraction;
            source.GetPropertyBlock(_comboPropertyBlock);
            destination.SetPropertyBlock(_comboPropertyBlock);
            return true;
        }
    }
}
