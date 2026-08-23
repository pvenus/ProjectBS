using System;

namespace Presentation
{
    public enum PresentationProvenanceKind
    {
        Unknown = 0,
        AuthoredAsset = 100,
        RuntimeResolved = 200,
        AuthoringSource = 300,
        AuthoredDescriptionFallback = 400,
    }

    [Serializable]
    public sealed class PresentationProvenanceData
    {
        public PresentationProvenanceKind Kind { get; }
        public string SourceId { get; }
        public string SourcePath { get; }
        public string SourceField { get; }

        public bool IsRuntimeApplied =>
            Kind == PresentationProvenanceKind.RuntimeResolved;

        public PresentationProvenanceData(
            PresentationProvenanceKind kind,
            string sourceId = null,
            string sourcePath = null,
            string sourceField = null)
        {
            Kind = kind;
            SourceId = sourceId ?? string.Empty;
            SourcePath = sourcePath ?? string.Empty;
            SourceField = sourceField ?? string.Empty;
        }
    }
}
