using System;
using System.Collections.Generic;
using Presentation;

namespace Character
{
    [Serializable]
    public sealed class CharacterPresentationData
    {
        private readonly PresentationEntryData[] stats;
        private readonly PresentationEntryData[] skills;

        public PresentationIdentityData Identity { get; }
        public CharacterType CharacterType { get; }
        public CharacterJob Job { get; }
        public CharacterJobFamily JobFamily { get; }
        public CharacterJobTier JobTier { get; }
        public CharacterJobBranch JobBranch { get; }
        public IReadOnlyList<PresentationEntryData> Stats => stats;
        public IReadOnlyList<PresentationEntryData> Skills => skills;
        public bool? IsDead { get; }
        public PresentationProvenanceData Provenance { get; }
        public ContentPresentationStatus Status { get; }

        public CharacterPresentationData(
            PresentationIdentityData identity,
            CharacterType characterType,
            CharacterJob job,
            CharacterJobFamily jobFamily,
            CharacterJobTier jobTier,
            CharacterJobBranch jobBranch,
            IReadOnlyList<PresentationEntryData> stats,
            IReadOnlyList<PresentationEntryData> skills,
            bool? isDead,
            PresentationProvenanceData provenance,
            ContentPresentationStatus status)
        {
            Identity = identity;
            CharacterType = characterType;
            Job = job;
            JobFamily = jobFamily;
            JobTier = jobTier;
            JobBranch = jobBranch;
            this.stats = Copy(stats);
            this.skills = Copy(skills);
            IsDead = isDead;
            Provenance = provenance;
            Status = status;
        }

        private static PresentationEntryData[] Copy(
            IReadOnlyList<PresentationEntryData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<PresentationEntryData>();
            }

            PresentationEntryData[] result = new PresentationEntryData[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }
}
