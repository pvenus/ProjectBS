using System;
using System.Collections.Generic;

namespace Presentation
{
    [Serializable]
    public sealed class PresentationEntryData
    {
        private readonly PresentationValueData[] values;

        public string Key { get; }
        public IReadOnlyList<PresentationValueData> Values => values;
        public string DetailContentId { get; }

        public PresentationEntryData(
            string key,
            IReadOnlyList<PresentationValueData> values,
            string detailContentId = null)
        {
            Key = key ?? string.Empty;
            this.values = Copy(values);
            DetailContentId = detailContentId ?? string.Empty;
        }

        private static PresentationValueData[] Copy(
            IReadOnlyList<PresentationValueData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<PresentationValueData>();
            }

            PresentationValueData[] copy =
                new PresentationValueData[source.Count];

            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }
}
