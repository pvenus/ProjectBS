using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{
    [Serializable]
    public sealed class RandomGrowthPresentationCopyFieldData
    {
        [SerializeField] private string name;
        [SerializeField] [TextArea] private string value;

        public string Name => name ?? string.Empty;
        public string Value => value ?? string.Empty;
    }

    /// <summary>Generated, immutable-at-runtime semantic copy projection.</summary>
    public sealed class RandomGrowthPresentationCopyAsset : ScriptableObject
    {
        [SerializeField] private int schemaVersion;
        [SerializeField] private string contentContractVersion;
        [SerializeField] private string locale;
        [SerializeField] private string catalogId;
        [SerializeField] private string projectionKind;
        [SerializeField] private string semanticDomain;
        [SerializeField] private string definitionDomain;
        [SerializeField] private string eventId;
        [SerializeField] private string sourcePopupId;
        [SerializeField] private string semanticCopyDigest;
        [SerializeField] private string definitionFingerprint;
        [SerializeField] private List<RandomGrowthPresentationCopyFieldData> fields = new();

        public int SchemaVersion => schemaVersion;
        public string ContentContractVersion => contentContractVersion ?? string.Empty;
        public string Locale => locale ?? string.Empty;
        public string CatalogId => catalogId ?? string.Empty;
        public string ProjectionKind => projectionKind ?? string.Empty;
        public string SemanticDomain => semanticDomain ?? string.Empty;
        public string DefinitionDomain => definitionDomain ?? string.Empty;
        public string EventId => eventId ?? string.Empty;
        public string SourcePopupId => sourcePopupId ?? string.Empty;
        public string SemanticCopyDigest => semanticCopyDigest ?? string.Empty;
        public string DefinitionFingerprint => definitionFingerprint ?? string.Empty;
        public IReadOnlyList<RandomGrowthPresentationCopyFieldData> Fields =>
            Array.AsReadOnly((fields ?? new List<RandomGrowthPresentationCopyFieldData>()).ToArray());
    }
}
