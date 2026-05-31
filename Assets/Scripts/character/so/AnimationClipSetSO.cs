using System;
using UnityEngine;

namespace Character
{
    [CreateAssetMenu(
        fileName = "AnimationClipSetSO",
        menuName = "Character/Animation Clip Set")]
    public class AnimationClipSetSO : ScriptableObject
    {
        [Header("Idle")]
        public DirectionalAnimationClips idleClips;

        [Header("Move")]
        public DirectionalAnimationClips moveClips;

        [Header("Attack")]
        public DirectionalAnimationClips attackClips;

        [Header("Death")]
        public DirectionalAnimationClips deathClips;

        public DirectionalAnimationClips GetClips(AnimationMono.AnimationState state)
        {
            switch (state)
            {
                case AnimationMono.AnimationState.Idle:
                    return idleClips;

                case AnimationMono.AnimationState.Move:
                    return moveClips;

                case AnimationMono.AnimationState.Attack:
                    return attackClips;

                case AnimationMono.AnimationState.Death:
                    return deathClips;

                default:
                    return null;
            }
        }
    }

    [Serializable]
    public class DirectionalAnimationClips
    {
        public AnimationClip upRight;
        public AnimationClip upLeft;
        public AnimationClip downRight;
        public AnimationClip downLeft;

        public AnimationClip GetClip(AnimationMono.DiagonalDirection direction)
        {
            switch (direction)
            {
                case AnimationMono.DiagonalDirection.UpRight:
                    return upRight;

                case AnimationMono.DiagonalDirection.UpLeft:
                    return upLeft;

                case AnimationMono.DiagonalDirection.DownRight:
                    return downRight;

                case AnimationMono.DiagonalDirection.DownLeft:
                    return downLeft;

                default:
                    return downRight;
            }
        }
    }
}