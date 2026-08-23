using System;
using System.Collections.Generic;
using Effect;
using Presentation;

namespace Item
{
    public sealed class RelicPresentationResolver
    {
        private readonly EffectPresentationResolver effectResolver = new();
        private readonly EffectPresentationGroupResolver effectGroupResolver = new();

        public RelicPresentationData ResolveData(
            RelicSO relic,
            PresentationContext context)
        {
            return ResolveCore(relic, null, context ?? PresentationContext.Preview);
        }

        public RelicPresentationData ResolveData(
            RelicEntry runtime,
            PresentationContext context)
        {
            return ResolveCore(runtime?.relic, runtime, context ?? PresentationContext.Runtime);
        }

        public ContentPresentationData Resolve(
            RelicSO relic,
            PresentationContext context)
        {
            return CreateContent(ResolveData(relic, context));
        }

        public ContentPresentationData Resolve(
            RelicEntry runtime,
            PresentationContext context)
        {
            return CreateContent(ResolveData(runtime, context));
        }

        public ContentPresentationData ResolveForPlayerDisplay(
            RelicSO relic,
            PresentationContext context)
        {
            return CreateContent(ResolveData(relic, context), true);
        }

        public ContentPresentationData ResolveForPlayerDisplay(
            RelicEntry runtime,
            PresentationContext context)
        {
            return CreateContent(ResolveData(runtime, context), true);
        }

        private RelicPresentationData ResolveCore(
            RelicSO relic,
            RelicEntry runtime,
            PresentationContext context)
        {
            if (relic == null)
            {
                return new RelicPresentationData(
                    new PresentationIdentityData(string.Empty, string.Empty),
                    string.Empty,
                    RelicRarity.Common,
                    string.Empty,
                    string.Empty,
                    Array.Empty<EffectPresentationData>(),
                    null,
                    null,
                    new PresentationProvenanceData(PresentationProvenanceKind.Unknown),
                    ContentPresentationStatus.Unsupported);
            }

            bool useRuntime = runtime != null
                && context.Mode == PresentationContextMode.Runtime;
            List<EffectPresentationData> effects = new();
            if (relic.effectEntries != null)
            {
                foreach (EffectEntrySO entry in relic.effectEntries)
                {
                    effects.Add(effectResolver.Resolve(entry, context));
                }
            }

            return new RelicPresentationData(
                new PresentationIdentityData(
                    relic.relicId,
                    PresentationLocalizedTextResolver.ResolveName(
                        relic.name,
                        relic.LocalizationMainKey),
                    relic.icon),
                ResolveDescription(relic),
                relic.rarity,
                relic.category,
                relic.subCategory,
                effects,
                useRuntime ? runtime.isEquipped : null,
                useRuntime ? runtime.HasOwner : null,
                new PresentationProvenanceData(
                    useRuntime
                        ? PresentationProvenanceKind.RuntimeResolved
                        : PresentationProvenanceKind.AuthoredAsset,
                    relic.relicId),
                ContentPresentationStatus.Supported);
        }

        private static string ResolveDescription(RelicSO relic)
        {
            if (relic == null)
            {
                return string.Empty;
            }

            return PresentationLocalizedTextResolver.ResolveRequired(
                "desc",
                relic.LocalizationMainKey);
        }

        private ContentPresentationData CreateContent(
            RelicPresentationData data,
            bool playerDisplay = false)
        {
            if (data == null)
            {
                return null;
            }

            List<string> classifications = new() { $"Relic.Rarity.{data.Rarity}" };
            if (!string.IsNullOrWhiteSpace(data.Category))
            {
                classifications.Add($"Relic.Category.{data.Category}");
            }
            if (!string.IsNullOrWhiteSpace(data.SubCategory))
            {
                classifications.Add($"Relic.SubCategory.{data.SubCategory}");
            }
            if (playerDisplay)
            {
                classifications.RemoveAll(key =>
                    !PresentationDisplayCatalog.IsPlayerVisibleTag(key));
            }

            List<PresentationGroupData> groups = new();
            List<PresentationEntryData> runtimeEntries = new();
            AddNullableToken(runtimeEntries, "Relic.Runtime.Equipped", data.IsEquipped);
            AddNullableToken(runtimeEntries, "Relic.Runtime.HasOwner", data.HasOwner);
            if (runtimeEntries.Count > 0)
            {
                if (playerDisplay)
                {
                    runtimeEntries.RemoveAll(entry =>
                        !PresentationDisplayCatalog.IsPlayerVisibleEntry(entry.Key));
                }
            }
            if (runtimeEntries.Count > 0)
            {
                groups.Add(new PresentationGroupData("Relic.Runtime", runtimeEntries));
            }

            for (int index = 0; index < data.Effects.Count; index++)
            {
                PresentationGroupData group = playerDisplay
                    ? effectGroupResolver.ResolveForPlayerDisplay(data.Effects[index])
                    : effectGroupResolver.Resolve(data.Effects[index]);
                if (group != null)
                {
                    groups.Add(group);
                }
            }

            return new ContentPresentationData(
                data.Identity,
                data.Description,
                classifications,
                groups,
                data.Provenance,
                data.Status);
        }

        private static void AddNullableToken(
            ICollection<PresentationEntryData> entries,
            string key,
            bool? value)
        {
            if (value.HasValue)
            {
                entries.Add(new PresentationEntryData(
                    key,
                    new[] { PresentationValueData.SemanticToken(value.Value ? "Yes" : "No") }));
            }
        }
    }
}
