using System;
using System.Collections.Generic;

namespace Presentation
{
    [Serializable]
    public sealed class PresentationGroupData
    {
        private readonly PresentationEntryData[] entries;

        public string Key { get; }
        public string Description { get; }
        public string SourceContentId { get; }
        public IReadOnlyList<PresentationEntryData> Entries => entries;

        public PresentationGroupData(
            string key,
            IReadOnlyList<PresentationEntryData> entries,
            string description = null,
            string sourceContentId = null)
        {
            Key = key ?? string.Empty;
            Description = description ?? string.Empty;
            SourceContentId = sourceContentId ?? string.Empty;
            this.entries = Copy(entries);
        }

        private static PresentationEntryData[] Copy(
            IReadOnlyList<PresentationEntryData> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<PresentationEntryData>();
            }

            PresentationEntryData[] copy =
                new PresentationEntryData[source.Count];

            for (int index = 0; index < source.Count; index++)
            {
                copy[index] = source[index];
            }

            return copy;
        }
    }
}
