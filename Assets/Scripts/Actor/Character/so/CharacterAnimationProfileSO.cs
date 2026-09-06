using System;
using System.Collections.Generic;
using UnityEngine;

namespace Character
{
    public enum CharacterAnimationSlot
    {
        Idle,
        Move,
        BasicAttack,
        Death,
        AttackDisabledCc,
        Hurt,
        Knockback
    }

    public enum CharacterAnimationFallbackPolicy
    {
        LegacyDirectional,
        BasicAttack,
        Idle,
        Rendererless
    }

    public enum CharacterAnimationInterruptPolicy { CancelToLocomotion, DeathOverride, UninterruptiblePresentation }
    public enum CharacterAnimationRestorePolicy { PreviousLocomotion, Idle, HoldTerminal }

    [Serializable]
    public sealed class CharacterAnimationStateEntry
    {
        [SerializeField] private CharacterAnimationSlot slot;
        [SerializeField] private string animationId;
        [SerializeField] private CharacterAnimationClipEntry[] directionalClips = Array.Empty<CharacterAnimationClipEntry>();
        [SerializeField] private bool loop;
        [SerializeField] private bool mirrorLeftFromRight = true;
        [SerializeField] private bool rootMotion;

        public CharacterAnimationSlot Slot => slot;
        public string AnimationId => animationId;
        public IReadOnlyList<CharacterAnimationClipEntry> DirectionalClips => directionalClips;
        public bool Loop => loop;
        public bool MirrorLeftFromRight => mirrorLeftFromRight;
        public bool RootMotion => rootMotion;

#if UNITY_EDITOR
        public void ApplyEditorData(string id, CharacterAnimationSlot value, CharacterAnimationClipEntry[] clips,
            bool shouldLoop, bool mirror, bool usesRootMotion)
        {
            animationId = id;
            slot = value;
            directionalClips = clips ?? Array.Empty<CharacterAnimationClipEntry>();
            loop = shouldLoop;
            mirrorLeftFromRight = mirror;
            rootMotion = usesRootMotion;
        }
#endif
    }

    [Serializable]
    public sealed class CharacterSkillAnimationEntry
    {
        [SerializeField] private string skillId;
        [SerializeField] private string animationId;
        [SerializeField] private AnimationClip clip;
        [SerializeField] private float playbackSpeed = 1f;
        [SerializeField] private bool mirrorWithFacing = true;
        [SerializeField] private CharacterAnimationFallbackPolicy fallback = CharacterAnimationFallbackPolicy.BasicAttack;
        [SerializeField] private string fallbackReceipt;
        [SerializeField] private string[] markerIntents = Array.Empty<string>();
        [SerializeField] private CharacterAnimationInterruptPolicy interruptPolicy = CharacterAnimationInterruptPolicy.CancelToLocomotion;
        [SerializeField] private CharacterAnimationRestorePolicy restorePolicy = CharacterAnimationRestorePolicy.PreviousLocomotion;
        [SerializeField] private string gradeReuseSource;
        [SerializeField] private string exceptionReceipt;

        public string SkillId => skillId;
        public string AnimationId => animationId;
        public AnimationClip Clip => clip;
        public float PlaybackSpeed => Mathf.Max(.01f, playbackSpeed);
        public bool MirrorWithFacing => mirrorWithFacing;
        public CharacterAnimationFallbackPolicy Fallback => fallback;
        public string FallbackReceipt => fallbackReceipt;
        public IReadOnlyList<string> MarkerIntents => markerIntents;
        public CharacterAnimationInterruptPolicy InterruptPolicy => interruptPolicy;
        public CharacterAnimationRestorePolicy RestorePolicy => restorePolicy;
        public string GradeReuseSource => gradeReuseSource;
        public string ExceptionReceipt => exceptionReceipt;

#if UNITY_EDITOR
        public void ApplyEditorData(string id, string actionAnimationId, AnimationClip value, float speed, bool mirror,
            CharacterAnimationFallbackPolicy fallbackPolicy, string receipt, string[] markers,
            CharacterAnimationInterruptPolicy interrupt, CharacterAnimationRestorePolicy restore,
            string reuseSource, string gradeExceptionReceipt)
        {
            skillId = id;
            animationId = actionAnimationId;
            clip = value;
            playbackSpeed = Mathf.Max(.01f, speed);
            mirrorWithFacing = mirror;
            fallback = fallbackPolicy;
            fallbackReceipt = receipt;
            markerIntents = markers ?? Array.Empty<string>();
            interruptPolicy = interrupt;
            restorePolicy = restore;
            gradeReuseSource = reuseSource;
            exceptionReceipt = gradeExceptionReceipt;
        }
#endif
    }

    [CreateAssetMenu(fileName = "CharacterAnimationProfile", menuName = "Character/Animation Profile v2")]
    public sealed class CharacterAnimationProfileSO : ScriptableObject
    {
        public const int CurrentSchemaVersion = 2;

        [SerializeField] private int schemaVersion = CurrentSchemaVersion;
        [SerializeField] private string generatorVersion;
        [SerializeField] private string sourceDigest;
        [SerializeField] private string contentContractReceipt;
        [SerializeField] private string productionGovernanceReceipt;
        [SerializeField] private bool attackDisabledCcReachable;
        [SerializeField] private string characterId;
        [SerializeField] private CharacterAnimationStateEntry[] states = Array.Empty<CharacterAnimationStateEntry>();
        [SerializeField] private CharacterSkillAnimationEntry[] skillActions = Array.Empty<CharacterSkillAnimationEntry>();

        public int SchemaVersion => schemaVersion;
        public string GeneratorVersion => generatorVersion;
        public string SourceDigest => sourceDigest;
        public string ContentContractReceipt => contentContractReceipt;
        public string ProductionGovernanceReceipt => productionGovernanceReceipt;
        public bool AttackDisabledCcReachable => attackDisabledCcReachable;
        public string CharacterId => characterId;
        public IReadOnlyList<CharacterAnimationStateEntry> States => states;
        public IReadOnlyList<CharacterSkillAnimationEntry> SkillActions => skillActions;

        public bool TryGetState(CharacterAnimationSlot slot, out CharacterAnimationStateEntry result)
        {
            result = null;
            if (states == null) return false;
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] != null && states[i].Slot == slot)
                {
                    result = states[i];
                    return true;
                }
            }
            return false;
        }

        public bool TryGetSkill(string skillId, out CharacterSkillAnimationEntry result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(skillId) || skillActions == null) return false;
            for (int i = 0; i < skillActions.Length; i++)
            {
                CharacterSkillAnimationEntry candidate = skillActions[i];
                if (candidate != null && string.Equals(candidate.SkillId, skillId, StringComparison.Ordinal))
                {
                    result = candidate;
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        public void ApplyEditorData(string id, string generator, string digest,
            string contentReceipt, string governanceReceipt,
            bool ccReachable, CharacterAnimationStateEntry[] stateEntries,
            CharacterSkillAnimationEntry[] actionEntries)
        {
            schemaVersion = CurrentSchemaVersion;
            characterId = id;
            generatorVersion = generator;
            sourceDigest = digest;
            contentContractReceipt = contentReceipt;
            productionGovernanceReceipt = governanceReceipt;
            attackDisabledCcReachable = ccReachable;
            states = stateEntries ?? Array.Empty<CharacterAnimationStateEntry>();
            skillActions = actionEntries ?? Array.Empty<CharacterSkillAnimationEntry>();
        }
#endif
    }
}
