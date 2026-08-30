using System;
using System.Collections.Generic;
using Character;
using Progression;

namespace Character.ProgressionBridge
{
    public enum CharacterRuntimeProgressionFailure
    {
        None = 0,
        InvalidRosterDuplicateOwner,
        InvalidRosterDuplicateTarget,
        MissingOwner,
        MissingTarget,
        EmptyEquipmentId,
        CanonicalSkillMismatch,
        InactiveSkill,
        MaxLevel,
        StaleLevel,
        ApplyConflict,
        RestoreConflict
    }

    public sealed class CharacterSkillEligibilityDescriptor
    {
        public CharacterSkillEligibilityDescriptor(
            string ownerCharacterId,
            string equipmentId,
            string canonicalSkillId,
            int currentLevel,
            int maxLevel,
            bool isActive,
            bool isApplicable)
        {
            OwnerCharacterId = ownerCharacterId;
            EquipmentId = equipmentId;
            CanonicalSkillId = canonicalSkillId;
            CurrentLevel = currentLevel;
            MaxLevel = maxLevel;
            IsActive = isActive;
            IsApplicable = isApplicable;
        }

        public string OwnerCharacterId { get; }
        public string EquipmentId { get; }
        public string CanonicalSkillId { get; }
        public int CurrentLevel { get; }
        public int MaxLevel { get; }
        public bool IsActive { get; }
        public bool IsApplicable { get; }
    }

    public sealed class CharacterRuntimeSkillLevelGateway : IProgressionSkillLevelGateway
    {
        private readonly IReadOnlyList<CharacterRuntimeData> roster;
        private readonly IReadOnlyDictionary<string, CharacterSkillEligibilityDescriptor> descriptors;
        private readonly HashSet<string> rolledBackMutationIds = new(StringComparer.Ordinal);
        private long mutationSequence;

        public CharacterRuntimeSkillLevelGateway(
            IReadOnlyList<CharacterRuntimeData> roster,
            IEnumerable<CharacterSkillEligibilityDescriptor> descriptors)
        {
            this.roster = roster ?? throw new ArgumentNullException(nameof(roster));
            Dictionary<string, CharacterSkillEligibilityDescriptor> byKey =
                new(StringComparer.Ordinal);
            if (descriptors != null)
            {
                foreach (CharacterSkillEligibilityDescriptor descriptor in descriptors)
                {
                    if (descriptor != null)
                    {
                        byKey[DescriptorKey(descriptor.OwnerCharacterId, descriptor.EquipmentId)] = descriptor;
                    }
                }
            }

            this.descriptors = byKey;
        }

        public CharacterRuntimeProgressionFailure LastFailure { get; private set; }

        public bool TryGetCurrentLevel(ProgressionSkillMutationKey key, out int currentLevel)
        {
            currentLevel = 0;
            if (!TryResolve(key, out CharacterRuntimeData owner, out EquipmentSkillInstanceData target))
            {
                return false;
            }

            currentLevel = Math.Max(1, target.currentLevel);
            return true;
        }

        public SkillLevelMutationResult TryApplyExactOne(
            ProgressionSkillMutationKey key,
            int expectedLevel,
            out ProgressionSkillLevelMutation mutation)
        {
            mutation = null;
            if (!TryResolve(key, out CharacterRuntimeData owner, out EquipmentSkillInstanceData target))
            {
                return MapFailure();
            }

            CharacterSkillEligibilityDescriptor descriptor =
                descriptors[DescriptorKey(key.OwnerCharacterId, key.SkillInstanceId)];
            int currentLevel = Math.Max(1, target.currentLevel);
            if (currentLevel != expectedLevel || descriptor.CurrentLevel != expectedLevel)
            {
                LastFailure = CharacterRuntimeProgressionFailure.StaleLevel;
                return SkillLevelMutationResult.RejectedExpectedLevel;
            }

            if (currentLevel >= descriptor.MaxLevel)
            {
                LastFailure = CharacterRuntimeProgressionFailure.MaxLevel;
                return SkillLevelMutationResult.RejectedMaxLevel;
            }

            CharacterSkillLevelCasResult applied = owner.TryCompareExchangeSkillLevel(
                key.SkillInstanceId,
                expectedLevel,
                expectedLevel + 1);
            if (applied != CharacterSkillLevelCasResult.Applied)
            {
                LastFailure = applied switch
                {
                    CharacterSkillLevelCasResult.DuplicateTarget => CharacterRuntimeProgressionFailure.InvalidRosterDuplicateTarget,
                    CharacterSkillLevelCasResult.MissingTarget => CharacterRuntimeProgressionFailure.MissingTarget,
                    _ => CharacterRuntimeProgressionFailure.ApplyConflict
                };
                return applied == CharacterSkillLevelCasResult.StaleLevel
                    ? SkillLevelMutationResult.RejectedExpectedLevel
                    : SkillLevelMutationResult.RejectedIdentity;
            }

            LastFailure = CharacterRuntimeProgressionFailure.None;
            mutationSequence++;
            mutation = new ProgressionSkillLevelMutation(
                key,
                expectedLevel,
                expectedLevel + 1,
                $"character-runtime.{mutationSequence}");
            return SkillLevelMutationResult.Applied;
        }

        public bool TryRollback(ProgressionSkillLevelMutation mutation)
        {
            if (mutation == null || string.IsNullOrWhiteSpace(mutation.MutationId))
            {
                LastFailure = CharacterRuntimeProgressionFailure.RestoreConflict;
                return false;
            }

            if (rolledBackMutationIds.Contains(mutation.MutationId))
            {
                LastFailure = CharacterRuntimeProgressionFailure.None;
                return true;
            }

            if (!TryResolve(mutation.Key, out CharacterRuntimeData owner, out _))
            {
                return false;
            }

            CharacterSkillLevelCasResult restored = owner.TryCompareExchangeSkillLevel(
                mutation.Key.SkillInstanceId,
                mutation.AppliedLevel,
                mutation.PreviousLevel);
            if (restored != CharacterSkillLevelCasResult.Applied)
            {
                LastFailure = CharacterRuntimeProgressionFailure.RestoreConflict;
                return false;
            }

            rolledBackMutationIds.Add(mutation.MutationId);
            LastFailure = CharacterRuntimeProgressionFailure.None;
            return true;
        }

        public bool TryRestoreExactLevel(
            ProgressionSkillMutationKey key,
            int expectedAppliedLevel,
            int restoreLevel)
        {
            if (!TryResolve(key, out CharacterRuntimeData owner, out _))
            {
                return false;
            }

            CharacterSkillLevelCasResult result = owner.TryCompareExchangeSkillLevel(
                key.SkillInstanceId,
                expectedAppliedLevel,
                restoreLevel);
            if (result != CharacterSkillLevelCasResult.Applied)
            {
                LastFailure = CharacterRuntimeProgressionFailure.RestoreConflict;
                return false;
            }

            LastFailure = CharacterRuntimeProgressionFailure.None;
            return true;
        }

        private bool TryResolve(
            ProgressionSkillMutationKey key,
            out CharacterRuntimeData owner,
            out EquipmentSkillInstanceData target)
        {
            owner = null;
            target = null;
            LastFailure = CharacterRuntimeProgressionFailure.None;

            if (!ValidateRoster())
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(key.SkillInstanceId))
            {
                LastFailure = CharacterRuntimeProgressionFailure.EmptyEquipmentId;
                return false;
            }

            if (!string.Equals(key.SkillInstanceId, key.CanonicalSkillId, StringComparison.Ordinal))
            {
                LastFailure = CharacterRuntimeProgressionFailure.CanonicalSkillMismatch;
                return false;
            }

            int ownerMatches = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                CharacterRuntimeData member = roster[i];
                if (member?.characterSO == null)
                {
                    continue;
                }

                if (string.Equals(member.characterSO.CharacterId, key.OwnerCharacterId, StringComparison.Ordinal))
                {
                    owner = member;
                    ownerMatches++;
                }
            }

            if (ownerMatches == 0)
            {
                LastFailure = CharacterRuntimeProgressionFailure.MissingOwner;
                return false;
            }

            if (ownerMatches != 1)
            {
                LastFailure = CharacterRuntimeProgressionFailure.InvalidRosterDuplicateOwner;
                return false;
            }

            int targetMatches = owner.CountSkillInstances(key.SkillInstanceId);
            if (targetMatches == 0)
            {
                LastFailure = CharacterRuntimeProgressionFailure.MissingTarget;
                return false;
            }

            if (targetMatches != 1)
            {
                LastFailure = CharacterRuntimeProgressionFailure.InvalidRosterDuplicateTarget;
                return false;
            }

            if (!descriptors.TryGetValue(
                    DescriptorKey(key.OwnerCharacterId, key.SkillInstanceId),
                    out CharacterSkillEligibilityDescriptor descriptor))
            {
                LastFailure = CharacterRuntimeProgressionFailure.MissingTarget;
                return false;
            }

            if (!string.Equals(descriptor.OwnerCharacterId, key.OwnerCharacterId, StringComparison.Ordinal)
                || !string.Equals(descriptor.EquipmentId, key.SkillInstanceId, StringComparison.Ordinal)
                || !string.Equals(descriptor.CanonicalSkillId, key.CanonicalSkillId, StringComparison.Ordinal))
            {
                LastFailure = CharacterRuntimeProgressionFailure.CanonicalSkillMismatch;
                return false;
            }

            if (!descriptor.IsActive || !descriptor.IsApplicable)
            {
                LastFailure = CharacterRuntimeProgressionFailure.InactiveSkill;
                return false;
            }

            target = owner.GetSkillInstance(key.SkillInstanceId);
            return target != null;
        }

        private bool ValidateRoster()
        {
            HashSet<string> ownerIds = new(StringComparer.Ordinal);
            for (int i = 0; i < roster.Count; i++)
            {
                CharacterRuntimeData member = roster[i];
                string ownerId = member?.characterSO?.CharacterId;
                if (string.IsNullOrWhiteSpace(ownerId))
                {
                    continue;
                }

                if (!ownerIds.Add(ownerId))
                {
                    LastFailure = CharacterRuntimeProgressionFailure.InvalidRosterDuplicateOwner;
                    return false;
                }

                HashSet<string> targetIds = new(StringComparer.Ordinal);
                if (member.skillInstances == null)
                {
                    continue;
                }

                for (int j = 0; j < member.skillInstances.Count; j++)
                {
                    EquipmentSkillInstanceData instance = member.skillInstances[j];
                    if (instance == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(instance.equipmentId))
                    {
                        LastFailure = CharacterRuntimeProgressionFailure.EmptyEquipmentId;
                        return false;
                    }

                    if (!targetIds.Add(instance.equipmentId))
                    {
                        LastFailure = CharacterRuntimeProgressionFailure.InvalidRosterDuplicateTarget;
                        return false;
                    }
                }
            }

            return true;
        }

        private SkillLevelMutationResult MapFailure() => LastFailure switch
        {
            CharacterRuntimeProgressionFailure.MissingOwner or
            CharacterRuntimeProgressionFailure.MissingTarget => SkillLevelMutationResult.RejectedNotFound,
            CharacterRuntimeProgressionFailure.StaleLevel => SkillLevelMutationResult.RejectedExpectedLevel,
            CharacterRuntimeProgressionFailure.MaxLevel => SkillLevelMutationResult.RejectedMaxLevel,
            _ => SkillLevelMutationResult.RejectedIdentity
        };

        private static string DescriptorKey(string ownerCharacterId, string equipmentId) =>
            (ownerCharacterId ?? string.Empty) + "\u001f" + (equipmentId ?? string.Empty);
    }
}
