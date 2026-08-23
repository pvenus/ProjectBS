using System;
using System.Collections.Generic;
using Effect;
using Presentation;

namespace Item
{
    [Serializable]
    public sealed class RelicPresentationData
    {
        private readonly EffectPresentationData[] effects;

        public PresentationIdentityData Identity { get; }
        public string Description { get; }
        public RelicRarity Rarity { get; }
        public string Category { get; }
        public string SubCategory { get; }
        public IReadOnlyList<EffectPresentationData> Effects => effects;
        public bool? IsEquipped { get; }
        public bool? HasOwner { get; }
        public PresentationProvenanceData Provenance { get; }
        public ContentPresentationStatus Status { get; }

        public RelicPresentationData(
            PresentationIdentityData identity,
            string description,
            RelicRarity rarity,
            string category,
            string subCategory,
            IReadOnlyList<EffectPresentationData> effects,
            bool? isEquipped,
            bool? hasOwner,
            PresentationProvenanceData provenance,
            ContentPresentationStatus status)
        {
            Identity = identity;
            Description = description ?? string.Empty;
            Rarity = rarity;
            Category = category ?? string.Empty;
            SubCategory = subCategory ?? string.Empty;
            this.effects = Copy(effects);
            IsEquipped = isEquipped;
            HasOwner = hasOwner;
            Provenance = provenance;
            Status = status;
        }

        private static EffectPresentationData[] Copy(
            IReadOnlyList<EffectPresentationData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<EffectPresentationData>();
            }

            EffectPresentationData[] result = new EffectPresentationData[source.Count];
            for (int index = 0; index < source.Count; index++)
            {
                result[index] = source[index];
            }

            return result;
        }
    }
}
