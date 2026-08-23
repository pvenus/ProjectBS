using System;
using System.Collections.Generic;

namespace Presentation
{
    public enum ContentPresentationStatus
    {
        Supported = 0,
        DescriptionOnly = 100,
        Unsupported = 200,
    }

    [Serializable]
    public sealed class ContentPresentationData
    {
        private readonly string[] classificationKeys;
        private readonly PresentationGroupData[] groups;

        public PresentationIdentityData Identity { get; }
        public string Description { get; }
        public IReadOnlyList<string> ClassificationKeys => classificationKeys;
        public IReadOnlyList<PresentationGroupData> Groups => groups;
        public PresentationProvenanceData Provenance { get; }
        public ContentPresentationStatus Status { get; }

        public ContentPresentationData(
            PresentationIdentityData identity,
            string description,
            IReadOnlyList<string> classificationKeys,
            IReadOnlyList<PresentationGroupData> groups,
            PresentationProvenanceData provenance,
            ContentPresentationStatus status = ContentPresentationStatus.Supported)
        {
            Identity = identity;
            Description = description ?? string.Empty;
            this.classificationKeys = Copy(classificationKeys);
            this.groups = Copy(groups);
            Provenance = provenance;
            Status = status;
        }

        private static string[] Copy(IReadOnlyList<string> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<string>();
            }

            string[] copy = new string[source.Count];

            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index] ?? string.Empty;
            }

            return copy;
        }

        private static PresentationGroupData[] Copy(
            IReadOnlyList<PresentationGroupData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<PresentationGroupData>();
            }

            PresentationGroupData[] copy =
                new PresentationGroupData[source.Count];

            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }
}
