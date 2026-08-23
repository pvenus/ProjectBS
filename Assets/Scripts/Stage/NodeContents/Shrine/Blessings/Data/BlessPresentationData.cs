using System;
using System.Collections.Generic;
using Effect;
using Presentation;
using Shrine;

namespace Bless
{
    [Serializable]
    public sealed class BlessPresentationData
    {
        private readonly string[] tags;
        private readonly EffectPresentationData[] effects;

        public PresentationIdentityData Identity { get; }
        public string Description { get; }
        public BlessCategory Category { get; }
        public ShrineGodType GodType { get; }
        public BlessDurationType DurationType { get; }
        public IReadOnlyList<string> Tags => tags;
        public IReadOnlyList<EffectPresentationData> Effects => effects;
        public PresentationValueData DurationBattleCount { get; }
        public PresentationValueData Level { get; }
        public PresentationValueData RemainingBattleCount { get; }
        public bool? IsEquipped { get; }
        public bool? IsLocked { get; }
        public PresentationProvenanceData Provenance { get; }
        public ContentPresentationStatus Status { get; }

        public BlessPresentationData(
            PresentationIdentityData identity,
            string description,
            BlessCategory category,
            ShrineGodType godType,
            BlessDurationType durationType,
            IReadOnlyList<string> tags,
            IReadOnlyList<EffectPresentationData> effects,
            PresentationValueData durationBattleCount,
            PresentationValueData level,
            PresentationValueData remainingBattleCount,
            bool? isEquipped,
            bool? isLocked,
            PresentationProvenanceData provenance,
            ContentPresentationStatus status)
        {
            Identity = identity;
            Description = description ?? string.Empty;
            Category = category;
            GodType = godType;
            DurationType = durationType;
            this.tags = Copy(tags);
            this.effects = Copy(effects);
            DurationBattleCount = durationBattleCount;
            Level = level;
            RemainingBattleCount = remainingBattleCount;
            IsEquipped = isEquipped;
            IsLocked = isLocked;
            Provenance = provenance;
            Status = status;
        }

        private static T[] Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<T>();
            }

            T[] result = new T[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }
}
