using System.Collections;
using UnityEngine;
using System;

namespace venus.eldawn.party
{
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
    public class AnimationMono : MonoBehaviour
    {
        public enum AnimationState
        {
            None,
            Idle,
            Move,
            Attack
        }

        public enum DiagonalDirection
        {
            UpRight,
            UpLeft,
            DownRight,
            DownLeft
        }

        [System.Serializable]
        public class DirectionalClipSet
        {
            public AnimationClip upRight;
            public AnimationClip upLeft;
            public AnimationClip downRight;
            public AnimationClip downLeft;

            public AnimationClip Get(DiagonalDirection direction)
            {
                return direction switch
                {
                    DiagonalDirection.UpRight => upRight,
                    DiagonalDirection.UpLeft => upLeft,
                    DiagonalDirection.DownRight => downRight,
                    DiagonalDirection.DownLeft => downLeft,
                    _ => null
                };
            }
        }

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private MovementController movementController;

        [Header("Default")]
        [SerializeField] private bool playIdleOnStart = true;
        [SerializeField] private DiagonalDirection defaultDirection = DiagonalDirection.DownRight;
        [SerializeField] private bool resetToPreviousLocomotionAfterAttack = true;

        [Header("Clips - Idle")]
        [SerializeField] private DirectionalClipSet idleClips;

        [Header("Clips - Move")]
        [SerializeField] private DirectionalClipSet moveClips;

        [Header("Clips - Attack")]
        [SerializeField] private DirectionalClipSet attackClips;

        private Coroutine _playRoutine;
        private Coroutine _oneShotRoutine;

        private AnimationState _currentState = AnimationState.None;
        private AnimationState _previousLocomotionState = AnimationState.Idle;
        private DiagonalDirection _currentDirection;
        private AnimationClip _currentClip;
        private bool _isPlayingOneShot;

        public AnimationState CurrentState => _currentState;
        public DiagonalDirection CurrentDirection => _currentDirection;
        public bool IsPlayingOneShot => _isPlayingOneShot;

        public bool IsPlayingAttack()
        {
            return _isPlayingOneShot && _currentState == AnimationState.Attack;
        }

        private void Reset()
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            movementController = GetComponent<MovementController>();
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
        }

        private void Awake()
        {
            EnsureAnimatorOnSpriteRenderer();
            if (movementController == null)
                movementController = GetComponent<MovementController>();
            _currentDirection = defaultDirection;
        }

        private void EnsureAnimatorOnSpriteRenderer()
        {
            if (targetSpriteRenderer == null)
            {
                targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
        }

        private IEnumerator Start()
        {
            yield return null;

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
            if (_currentDirection == direction)
                return;

            _currentDirection = direction;

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

        public void UpdateDirectionFromMovementController()
        {
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
            if (_isPlayingOneShot)
                return;

            PlayState(AnimationState.Idle);
        }

        public void PlayMove()
        {
            if (_isPlayingOneShot)
                return;

            UpdateDirectionFromMovementController();
            PlayState(AnimationState.Move);
        }

        public void PlayAttack()
        {
            AnimationClip clip = GetClip(AnimationState.Attack, _currentDirection);
            if (!CanPlayClip(clip, AnimationState.Attack, _currentDirection))
                return;

            if (_isPlayingOneShot && _currentState == AnimationState.Attack && _currentClip == clip)
                return;

            StopOneShotRoutine();
            StopPlayRoutine();
            _isPlayingOneShot = true;
            _currentState = AnimationState.Attack;
            _currentClip = clip;
            _playRoutine = StartCoroutine(PlayOneShotClipRoutine(clip));
            _oneShotRoutine = StartCoroutine(PlayAttackRoutine(clip));
        }

        public void StopAnimation()
        {
            StopOneShotRoutine();
            StopPlayRoutine();
            _currentState = AnimationState.None;
            _isPlayingOneShot = false;
            _currentClip = null;
        }

        private void PlayState(AnimationState state, bool restartIfSameState = false)
        {
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
            _playRoutine = StartCoroutine(PlayLoopClipRoutine(clip));
        }

        private IEnumerator PlayAttackRoutine(AnimationClip clip)
        {
            float duration = Mathf.Max(0.01f, clip.length);
            yield return new WaitForSeconds(duration);

            _oneShotRoutine = null;
            _isPlayingOneShot = false;
            _currentClip = null;

            if (resetToPreviousLocomotionAfterAttack)
            {
                AnimationState nextState = _previousLocomotionState == AnimationState.Move
                    ? AnimationState.Move
                    : AnimationState.Idle;
                PlayState(nextState, restartIfSameState: true);
            }
            else
            {
                _currentState = AnimationState.None;
            }
        }

        private AnimationClip GetClip(AnimationState state, DiagonalDirection direction)
        {
            return state switch
            {
                AnimationState.Idle => idleClips != null ? idleClips.Get(direction) : null,
                AnimationState.Move => moveClips != null ? moveClips.Get(direction) : null,
                AnimationState.Attack => attackClips != null ? attackClips.Get(direction) : null,
                _ => null
            };
        }

        private bool CanPlayClip(AnimationClip clip, AnimationState state, DiagonalDirection direction)
        {
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
                time += Time.deltaTime;

                if (time >= length)
                {
                    time %= length;
                }

                yield return null;
            }
        }

        private IEnumerator PlayOneShotClipRoutine(AnimationClip clip)
        {
            if (clip == null)
                yield break;

            Debug.Log($"[{nameof(AnimationMono)}] PlayOneShotClip -> {clip.name} on {name}", this);

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

        private void OnDestroy()
        {
            StopOneShotRoutine();
            StopPlayRoutine();
        }
    }
}