using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Stage
{
    public enum RandomGrowthPresentationCopyMismatch
    {
        None = 0, Missing = 10, WrongSchema = 20, WrongVersion = 30,
        WrongLocale = 40, WrongIdentity = 50, WrongProjectionKind = 60,
        SubsetOrOrderMismatch = 70, TupleMismatch = 80, DigestMismatch = 90,
        FingerprintMismatch = 100
    }

    public sealed class RandomGrowthPresentationCopyExpectation
    {
        public RandomGrowthPresentationCopyExpectation(int schemaVersion,
            string contentContractVersion, string locale, string catalogId,
            string projectionKind, string semanticDomain, string definitionDomain,
            string eventId, string sourcePopupId, string semanticCopyDigest,
            string definitionFingerprint, IEnumerable<string> orderedFieldNames)
        {
            SchemaVersion = schemaVersion;
            ContentContractVersion = contentContractVersion ?? string.Empty;
            Locale = locale ?? string.Empty; CatalogId = catalogId ?? string.Empty;
            ProjectionKind = projectionKind ?? string.Empty;
            SemanticDomain = semanticDomain ?? string.Empty;
            DefinitionDomain = definitionDomain ?? string.Empty;
            EventId = eventId ?? string.Empty; SourcePopupId = sourcePopupId ?? string.Empty;
            SemanticCopyDigest = semanticCopyDigest ?? string.Empty;
            DefinitionFingerprint = definitionFingerprint ?? string.Empty;
            OrderedFieldNames = Array.AsReadOnly((orderedFieldNames ?? Array.Empty<string>()).ToArray());
        }

        public int SchemaVersion { get; }
        public string ContentContractVersion { get; }
        public string Locale { get; }
        public string CatalogId { get; }
        public string ProjectionKind { get; }
        public string SemanticDomain { get; }
        public string DefinitionDomain { get; }
        public string EventId { get; }
        public string SourcePopupId { get; }
        public string SemanticCopyDigest { get; }
        public string DefinitionFingerprint { get; }
        public IReadOnlyList<string> OrderedFieldNames { get; }
    }

    public sealed class RandomGrowthResolvedPresentationCopy
    {
        internal RandomGrowthResolvedPresentationCopy(RandomGrowthPresentationCopyExpectation identity,
            IReadOnlyDictionary<string, string> values)
        { Identity = identity; Values = values; }
        public RandomGrowthPresentationCopyExpectation Identity { get; }
        public IReadOnlyDictionary<string, string> Values { get; }
        public string Get(string name) => Values.TryGetValue(name ?? string.Empty, out string value)
            ? value : string.Empty;
    }

    public static class RandomGrowthPresentationCopyResolver
    {
        public static bool TryResolve(RandomGrowthPresentationCopyAsset asset,
            RandomGrowthPresentationCopyExpectation expected,
            out RandomGrowthResolvedPresentationCopy copy,
            out RandomGrowthPresentationCopyMismatch mismatch)
        {
            copy = null; mismatch = RandomGrowthPresentationCopyMismatch.Missing;
            if (asset == null || expected == null) return false;
            if (asset.SchemaVersion != expected.SchemaVersion)
                return Fail(RandomGrowthPresentationCopyMismatch.WrongSchema, out mismatch);
            if (!Eq(asset.ContentContractVersion, expected.ContentContractVersion)
                || !Eq(asset.SemanticDomain, expected.SemanticDomain)
                || !Eq(asset.DefinitionDomain, expected.DefinitionDomain))
                return Fail(RandomGrowthPresentationCopyMismatch.WrongVersion, out mismatch);
            if (!Eq(asset.Locale, expected.Locale))
                return Fail(RandomGrowthPresentationCopyMismatch.WrongLocale, out mismatch);
            if (!Eq(asset.CatalogId, expected.CatalogId)
                || !Eq(asset.EventId, expected.EventId)
                || !Eq(asset.SourcePopupId, expected.SourcePopupId))
                return Fail(RandomGrowthPresentationCopyMismatch.WrongIdentity, out mismatch);
            if (!Eq(asset.ProjectionKind, expected.ProjectionKind))
                return Fail(RandomGrowthPresentationCopyMismatch.WrongProjectionKind, out mismatch);

            IReadOnlyList<RandomGrowthPresentationCopyFieldData> fields = asset.Fields;
            if (fields.Count != expected.OrderedFieldNames.Count)
                return Fail(RandomGrowthPresentationCopyMismatch.SubsetOrOrderMismatch, out mismatch);
            Dictionary<string, string> values = new(StringComparer.Ordinal);
            List<string> tuple = new();
            for (int i = 0; i < fields.Count; i++)
            {
                RandomGrowthPresentationCopyFieldData field = fields[i];
                if (field == null || !Eq(field.Name, expected.OrderedFieldNames[i])
                    || !Eq(field.Value, Normalize(field.Value)) || !values.TryAdd(field.Name, field.Value))
                    return Fail(RandomGrowthPresentationCopyMismatch.SubsetOrOrderMismatch, out mismatch);
                tuple.Add(field.Name); tuple.Add(field.Value);
            }
            string digest = ComputeDigest(asset.SemanticDomain, tuple);
            if (!Eq(digest, asset.SemanticCopyDigest) || !Eq(digest, expected.SemanticCopyDigest))
                return Fail(RandomGrowthPresentationCopyMismatch.DigestMismatch, out mismatch);
            if (!Eq(asset.DefinitionFingerprint, expected.DefinitionFingerprint))
                return Fail(RandomGrowthPresentationCopyMismatch.FingerprintMismatch, out mismatch);

            mismatch = RandomGrowthPresentationCopyMismatch.None;
            copy = new RandomGrowthResolvedPresentationCopy(expected,
                new ReadOnlyDictionary<string, string>(values));
            return true;
        }

        public static string ComputeDigest(string domain, IEnumerable<string> orderedNameValues)
        {
            using MemoryStream stream = new();
            using (BinaryWriter writer = new(stream, new UTF8Encoding(false), true))
            {
                Write(writer, domain);
                foreach (string value in orderedNameValues ?? Array.Empty<string>()) Write(writer, value);
            }
            using SHA256 sha = SHA256.Create();
            return string.Concat(sha.ComputeHash(stream.ToArray()).Select(x => x.ToString("x2")));
        }

        private static void Write(BinaryWriter writer, string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            writer.Write((byte)(bytes.Length >> 24)); writer.Write((byte)(bytes.Length >> 16));
            writer.Write((byte)(bytes.Length >> 8)); writer.Write((byte)bytes.Length); writer.Write(bytes);
        }
        private static string Normalize(string value) => (value ?? string.Empty)
            .Normalize(NormalizationForm.FormC).Replace("\r\n", "\n").Replace("\r", "\n");
        private static bool Eq(string a, string b) => string.Equals(a, b, StringComparison.Ordinal);
        private static bool Fail(RandomGrowthPresentationCopyMismatch value,
            out RandomGrowthPresentationCopyMismatch mismatch) { mismatch = value; return false; }
    }
}
