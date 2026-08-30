using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Character.ProgressionBridge;
using Party;
using Progression;
using Skill;
using UIFramework.Data;

namespace Stage
{
    public sealed class SafeGrowthPartyWideOfferViewProjection
    {
        internal SafeGrowthPartyWideOfferViewProjection(SkillUpgradeViewData data,
            IReadOnlyList<ProgressionSkillCandidateSnapshot> candidates,
            IReadOnlyList<CharacterSkillEligibilityDescriptor> descriptors)
        { ViewData = data; Candidates = candidates; Descriptors = descriptors; }
        public SkillUpgradeViewData ViewData { get; }
        public IReadOnlyList<ProgressionSkillCandidateSnapshot> Candidates { get; }
        public IReadOnlyList<CharacterSkillEligibilityDescriptor> Descriptors { get; }
    }

    public sealed class SafeGrowthPartyWideOfferViewDataBuilder
    {
        public bool TryBuild(PartyRuntimeData party, IEnumerable<EquipmentSkillSO> catalog,
            ProgressionOpportunitySnapshot opportunity, out SafeGrowthPartyWideOfferViewProjection projection)
        {
            projection = null;
            if (party?.Members == null || opportunity?.Offer?.Candidates == null
                || opportunity.Offer.TargetCount != 2 || opportunity.Offer.Candidates.Count is < 1 or > 2)
                return false;
            IGrouping<string, EquipmentSkillSO>[] groups = (catalog ?? Array.Empty<EquipmentSkillSO>())
                .Where(x => x != null).GroupBy(x => x.EquipmentId, StringComparer.Ordinal).ToArray();
            if (groups.Any(x => string.IsNullOrWhiteSpace(x.Key) || x.Count() != 1)) return false;
            Dictionary<string, EquipmentSkillSO> skills = groups
                .ToDictionary(x => x.Key, x => x.Single(), StringComparer.Ordinal);
            List<SkillUpgradeOptionData> options = new();
            List<ProgressionSkillCandidateSnapshot> candidates = new();
            List<CharacterSkillEligibilityDescriptor> descriptors = new();
            foreach (ProgressionSkillCandidateSnapshot candidate in opportunity.Offer.Candidates)
            {
                CharacterRuntimeData owner = party.Members.SingleOrDefault(x =>
                    string.Equals(x?.characterSO?.CharacterId, candidate.OwnerCharacterId, StringComparison.Ordinal));
                EquipmentSkillInstanceData instance = owner?.skillInstances?.SingleOrDefault(x =>
                    string.Equals(x?.equipmentId, candidate.SkillInstanceId, StringComparison.Ordinal));
                if (owner == null || instance == null || !skills.TryGetValue(candidate.CanonicalSkillId, out EquipmentSkillSO skill)
                    || instance.currentLevel != candidate.CurrentLevel) return false;
                SkillUpgradeOptionData option = SkillUpgradeViewDataBuilder.BuildFixedOfferOption(owner, instance, skill);
                if (option == null) return false;
                options.Add(option); candidates.Add(candidate);
                descriptors.Add(new CharacterSkillEligibilityDescriptor(candidate.OwnerCharacterId,
                    candidate.SkillInstanceId, candidate.CanonicalSkillId, candidate.CurrentLevel,
                    candidate.MaxLevel, true, true));
            }
            projection = new SafeGrowthPartyWideOfferViewProjection(
                new SkillUpgradeViewData { options = options },
                Array.AsReadOnly(candidates.ToArray()), Array.AsReadOnly(descriptors.ToArray()));
            return true;
        }
    }
}
