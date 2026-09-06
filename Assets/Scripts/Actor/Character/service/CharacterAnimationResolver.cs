using System;
using UnityEngine;

namespace Character
{
    public static class CharacterAnimationResolver
    {
        public enum Priority
        {
            Idle = 0,
            Move = 10,
            BasicAttack = 20,
            SkillAction = 30,
            AttackDisabledCc = 40,
            Death = 50
        }

        public static Priority ResolvePriority(bool dead, bool attackDisabledCc, bool skillAction,
            bool basicAttack, bool moving)
        {
            if (dead) return Priority.Death;
            if (attackDisabledCc) return Priority.AttackDisabledCc;
            if (skillAction) return Priority.SkillAction;
            if (basicAttack) return Priority.BasicAttack;
            return moving ? Priority.Move : Priority.Idle;
        }

        public static AnimationClip ResolveSkillClip(CharacterAnimationProfileSO profile,
            string skillId, AnimationClip castOwnedClip, out CharacterAnimationFallbackPolicy fallback)
        {
            return ResolveSkillClip(profile, skillId, castOwnedClip, out fallback, out _, out _);
        }

        public static AnimationClip ResolveSkillClip(CharacterAnimationProfileSO profile,
            string skillId, AnimationClip castOwnedClip, out CharacterAnimationFallbackPolicy fallback,
            out float playbackSpeed, out bool mirrorWithFacing)
        {
            fallback = CharacterAnimationFallbackPolicy.LegacyDirectional;
            playbackSpeed = 1f;
            mirrorWithFacing = true;
            if (castOwnedClip != null) return castOwnedClip;
            if (profile != null && profile.TryGetSkill(skillId, out CharacterSkillAnimationEntry entry))
            {
                fallback = entry.Fallback;
                playbackSpeed = entry.PlaybackSpeed;
                mirrorWithFacing = entry.MirrorWithFacing;
                return entry.Clip;
            }
            return null;
        }
    }
}
