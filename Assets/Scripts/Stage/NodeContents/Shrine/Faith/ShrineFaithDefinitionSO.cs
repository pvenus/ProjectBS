using System;
using System.Collections.Generic;
using Bless;
using Character;
using String;
using UnityEngine;

namespace Shrine
{
    public enum FaithDefinitionSourceStatus
    {
        PendingSource = 0,
        ConfirmedDescriptionOnly = 100,
        RuntimeReady = 200
    }

    public enum FaithLockRequirement
    {
        Unspecified = 0,
        NotRequired = 100,
        Required = 200
    }

    [Serializable]
    public sealed class FaithBasicBlessLevelDefinition
    {
        [SerializeField, Range(1, 10)] private int faithLevel = 1;
        [SerializeField] private BlessSO blessing;

        public int FaithLevel => Mathf.Clamp(faithLevel, 1, 10);
        public BlessSO Blessing => blessing;

#if UNITY_EDITOR
        public FaithBasicBlessLevelDefinition(int faithLevel, BlessSO blessing)
        {
            this.faithLevel = Mathf.Clamp(faithLevel, 1, 10);
            this.blessing = blessing;
        }
#endif
    }

    [Serializable]
    public sealed class FaithBasicBlessDefinition
    {
        [SerializeField] private string featureId;
        [SerializeField] private FaithDefinitionSourceStatus sourceStatus;
        [SerializeField] private List<FaithBasicBlessLevelDefinition> levels = new();

        public string FeatureId => featureId;
        public FaithDefinitionSourceStatus SourceStatus => sourceStatus;
        public bool IsRuntimeReady => sourceStatus == FaithDefinitionSourceStatus.RuntimeReady;
        public IReadOnlyList<FaithBasicBlessLevelDefinition> Levels => levels;

        public BlessSO GetExactLevelBlessing(int faithLevel)
        {
            if (levels == null || faithLevel < 1 || faithLevel > 10)
            {
                return null;
            }

            for (int index = 0; index < levels.Count; index++)
            {
                FaithBasicBlessLevelDefinition entry = levels[index];
                if (entry != null && entry.FaithLevel == faithLevel)
                {
                    return entry.Blessing;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public FaithBasicBlessDefinition(
            string featureId,
            FaithDefinitionSourceStatus sourceStatus,
            List<FaithBasicBlessLevelDefinition> levels)
        {
            this.featureId = featureId ?? string.Empty;
            this.sourceStatus = sourceStatus;
            this.levels = levels ?? new List<FaithBasicBlessLevelDefinition>();
        }
#endif
    }

    [Serializable]
    public sealed class FaithJobTransitionDefinition
    {
        [SerializeField] private CharacterJob fromJob;
        [SerializeField] private CharacterJob toJob;
        [SerializeField] private string fromJobLocalizationKey;
        [SerializeField] private string toJobLocalizationKey;

        public CharacterJob FromJob => fromJob;
        public CharacterJob ToJob => toJob;
        public string FromJobLocalizationKey => fromJobLocalizationKey;
        public string ToJobLocalizationKey => toJobLocalizationKey;
        public CharacterJobFamily JobFamily => CharacterJobHelper.GetFamily(fromJob);

        public bool IsValid =>
            fromJob != CharacterJob.None
            && toJob != CharacterJob.None
            && CharacterJobHelper.GetFamily(fromJob) == CharacterJobHelper.GetFamily(toJob)
            && CharacterJobHelper.GetTier(toJob) > CharacterJobHelper.GetTier(fromJob);

#if UNITY_EDITOR
        public FaithJobTransitionDefinition(
            CharacterJob fromJob,
            CharacterJob toJob,
            string fromJobLocalizationKey,
            string toJobLocalizationKey)
        {
            this.fromJob = fromJob;
            this.toJob = toJob;
            this.fromJobLocalizationKey = fromJobLocalizationKey ?? string.Empty;
            this.toJobLocalizationKey = toJobLocalizationKey ?? string.Empty;
        }
#endif
    }

    [Serializable]
    public sealed class FaithJobChangeDefinition
    {
        [SerializeField] private string featureId;
        [SerializeField] private FaithDefinitionSourceStatus sourceStatus;
        [SerializeField, Range(0, 10)] private int unlockLevel;
        [SerializeField] private FaithLockRequirement faithLockRequirement;
        [SerializeField] private List<FaithJobTransitionDefinition> transitions = new();

        public string FeatureId => featureId;
        public FaithDefinitionSourceStatus SourceStatus => sourceStatus;
        public bool IsRuntimeReady => sourceStatus == FaithDefinitionSourceStatus.RuntimeReady;
        public bool HasUnlockLevel => unlockLevel > 0;
        public int UnlockLevel => Mathf.Clamp(unlockLevel, 0, 10);
        public FaithLockRequirement FaithLockRequirement => faithLockRequirement;
        public IReadOnlyList<FaithJobTransitionDefinition> Transitions => transitions;

        public bool TryGetTransition(
            CharacterJob currentJob,
            out FaithJobTransitionDefinition transition)
        {
            if (transitions != null)
            {
                for (int index = 0; index < transitions.Count; index++)
                {
                    FaithJobTransitionDefinition candidate = transitions[index];
                    if (candidate != null
                        && candidate.IsValid
                        && candidate.FromJob == currentJob)
                    {
                        transition = candidate;
                        return true;
                    }
                }
            }

            transition = null;
            return false;
        }

#if UNITY_EDITOR
        public FaithJobChangeDefinition(
            string featureId,
            FaithDefinitionSourceStatus sourceStatus,
            int unlockLevel,
            FaithLockRequirement faithLockRequirement,
            List<FaithJobTransitionDefinition> transitions)
        {
            this.featureId = featureId ?? string.Empty;
            this.sourceStatus = sourceStatus;
            this.unlockLevel = Mathf.Clamp(unlockLevel, 0, 10);
            this.faithLockRequirement = faithLockRequirement;
            this.transitions = transitions ?? new List<FaithJobTransitionDefinition>();
        }
#endif
    }

    [Serializable]
    public sealed class FaithExclusiveBlessDefinition
    {
        [SerializeField] private string featureId;
        [SerializeField] private FaithDefinitionSourceStatus sourceStatus;
        [SerializeField, Range(0, 10)] private int unlockLevel;
        [SerializeField] private FaithLockRequirement faithLockRequirement;
        [SerializeField] private BlessSO blessing;

        public string FeatureId => featureId;
        public FaithDefinitionSourceStatus SourceStatus => sourceStatus;
        public bool IsRuntimeReady => sourceStatus == FaithDefinitionSourceStatus.RuntimeReady;
        public bool HasUnlockLevel => unlockLevel > 0;
        public int UnlockLevel => Mathf.Clamp(unlockLevel, 0, 10);
        public FaithLockRequirement FaithLockRequirement => faithLockRequirement;
        public BlessSO Blessing => blessing;

        public bool MeetsAuthoredUnlockCondition(int faithLevel, bool isFaithLocked)
        {
            if (sourceStatus == FaithDefinitionSourceStatus.PendingSource
                || blessing == null
                || HasUnlockLevel && faithLevel < UnlockLevel)
            {
                return false;
            }

            return faithLockRequirement != FaithLockRequirement.Required
                   || isFaithLocked;
        }

#if UNITY_EDITOR
        public FaithExclusiveBlessDefinition(
            string featureId,
            FaithDefinitionSourceStatus sourceStatus,
            int unlockLevel,
            FaithLockRequirement faithLockRequirement,
            BlessSO blessing)
        {
            this.featureId = featureId ?? string.Empty;
            this.sourceStatus = sourceStatus;
            this.unlockLevel = Mathf.Clamp(unlockLevel, 0, 10);
            this.faithLockRequirement = faithLockRequirement;
            this.blessing = blessing;
        }
#endif
    }

    [CreateAssetMenu(
        fileName = "ShrineFaithDefinition",
        menuName = "Shrine/Faith Definition")]
    public sealed class ShrineFaithDefinitionSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string faithId;
        [SerializeField] private ShrineGodType godType;
        [SerializeField] private ShrineGodSO shrineGod;

        [Header("Source")]
        [SerializeField] private string sourceReference;

        [Header("Faith Features")]
        [SerializeField] private FaithBasicBlessDefinition basicBlessing;
        [SerializeField] private FaithJobChangeDefinition exclusiveJobChange;
        [SerializeField] private FaithExclusiveBlessDefinition exclusiveBless1;
        [SerializeField] private FaithExclusiveBlessDefinition exclusiveBless2;

        public string FaithId => faithId;
        public string LocalizationMainKey => faithId;
        public ShrineGodType GodType => godType;
        public ShrineGodSO ShrineGod => shrineGod;
        public string SourceReference => sourceReference;
        public FaithBasicBlessDefinition BasicBlessing => basicBlessing;
        public FaithJobChangeDefinition ExclusiveJobChange => exclusiveJobChange;
        public FaithExclusiveBlessDefinition ExclusiveBless1 => exclusiveBless1;
        public FaithExclusiveBlessDefinition ExclusiveBless2 => exclusiveBless2;

        public string DisplayName => StringManager.Instance.Get(faithId, "name");
        public string Description => StringManager.Instance.Get(faithId, "desc");

#if UNITY_EDITOR
        public void ApplyEditorData(
            string faithId,
            ShrineGodType godType,
            ShrineGodSO shrineGod,
            string sourceReference,
            FaithBasicBlessDefinition basicBlessing,
            FaithJobChangeDefinition exclusiveJobChange,
            FaithExclusiveBlessDefinition exclusiveBless1,
            FaithExclusiveBlessDefinition exclusiveBless2)
        {
            this.faithId = faithId ?? string.Empty;
            this.godType = godType;
            this.shrineGod = shrineGod;
            this.sourceReference = sourceReference ?? string.Empty;
            this.basicBlessing = basicBlessing;
            this.exclusiveJobChange = exclusiveJobChange;
            this.exclusiveBless1 = exclusiveBless1;
            this.exclusiveBless2 = exclusiveBless2;
        }
#endif
    }
}
