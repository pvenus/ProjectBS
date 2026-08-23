using System;
using System.Collections.Generic;
using Effect;
using Presentation;
using Shrine;

namespace Bless
{
    public sealed class BlessPresentationResolver
    {
        private readonly EffectPresentationResolver effectResolver = new();
        private readonly EffectPresentationGroupResolver effectGroupResolver = new();

        public BlessPresentationData ResolveData(
            BlessSO bless,
            PresentationContext context)
        {
            return ResolveCore(bless, null, context ?? PresentationContext.Preview);
        }

        public BlessPresentationData ResolveData(
            BlessRuntimeData.BlessEntry runtime,
            PresentationContext context)
        {
            return ResolveCore(
                runtime?.source,
                runtime,
                context ?? PresentationContext.Runtime);
        }

        public ContentPresentationData Resolve(
            BlessSO bless,
            PresentationContext context)
        {
            return CreateContent(ResolveData(bless, context));
        }

        public ContentPresentationData Resolve(
            BlessRuntimeData.BlessEntry runtime,
            PresentationContext context)
        {
            return CreateContent(ResolveData(runtime, context));
        }

        public ContentPresentationData ResolveForPlayerDisplay(
            BlessSO bless,
            PresentationContext context)
        {
            return CreateContent(ResolveData(bless, context), true);
        }

        public ContentPresentationData ResolveForPlayerDisplay(
            BlessRuntimeData.BlessEntry runtime,
            PresentationContext context)
        {
            return CreateContent(ResolveData(runtime, context), true);
        }

        private BlessPresentationData ResolveCore(
            BlessSO bless,
            BlessRuntimeData.BlessEntry runtime,
            PresentationContext context)
        {
            if (bless == null)
            {
                return new BlessPresentationData(
                    new PresentationIdentityData(string.Empty, string.Empty),
                    string.Empty,
                    BlessCategory.None,
                    ShrineGodType.None,
                    BlessDurationType.Permanent,
                    Array.Empty<string>(),
                    Array.Empty<EffectPresentationData>(),
                    null,
                    null,
                    null,
                    null,
                    null,
                    new PresentationProvenanceData(PresentationProvenanceKind.Unknown),
                    ContentPresentationStatus.Unsupported);
            }

            bool useRuntime = runtime != null
                && context.Mode == PresentationContextMode.Runtime;
            PresentationProvenanceKind kind = useRuntime
                ? PresentationProvenanceKind.RuntimeResolved
                : PresentationProvenanceKind.AuthoredAsset;

            return new BlessPresentationData(
                new PresentationIdentityData(
                    bless.BlessingId,
                    PresentationLocalizedTextResolver.ResolveName(
                        bless.name,
                        bless.LocalizationMainKey),
                    bless.Icon),
                ResolveDescription(bless),
                bless.Category,
                bless.GodType,
                bless.DurationType,
                bless.Tags,
                ResolveEffects(bless, context),
                bless.DurationType == BlessDurationType.BattleCount
                    ? Number(bless, bless.DurationBattleCount, PresentationValueUnit.Count, "DurationBattleCount")
                    : null,
                useRuntime
                    ? RuntimeNumber(bless, runtime.level, PresentationValueUnit.Count, "level")
                    : null,
                useRuntime && runtime.isTemporary
                    ? RuntimeNumber(bless, runtime.remainingBattleCount, PresentationValueUnit.Count, "remainingBattleCount")
                    : null,
                useRuntime ? runtime.isEquipped : null,
                useRuntime ? runtime.isLocked : null,
                Provenance(bless, kind),
                ContentPresentationStatus.Supported);
        }

        private static string ResolveDescription(BlessSO bless)
        {
            if (bless == null)
            {
                return string.Empty;
            }

            return PresentationLocalizedTextResolver.ResolveRequired(
                "desc",
                bless.LocalizationMainKey);
        }

        private IReadOnlyList<EffectPresentationData> ResolveEffects(
            BlessSO bless,
            PresentationContext context)
        {
            List<EffectPresentationData> result = new();
            foreach (EffectEntrySO entry in bless.EffectEntries)
            {
                result.Add(effectResolver.Resolve(entry, context));
            }

            return result;
        }

        private ContentPresentationData CreateContent(
            BlessPresentationData data,
            bool playerDisplay = false)
        {
            if (data == null)
            {
                return null;
            }

            List<string> classifications = new()
            {
                $"Bless.Category.{data.Category}",
                $"Bless.Duration.{data.DurationType}",
            };
            if (data.GodType != ShrineGodType.None)
            {
                classifications.Add($"Bless.God.{data.GodType}");
            }
            classifications.AddRange(data.Tags);
            if (playerDisplay)
            {
                classifications.RemoveAll(key =>
                    !PresentationDisplayCatalog.IsPlayerVisibleTag(key));
            }

            List<PresentationGroupData> groups = new();
            List<PresentationEntryData> durationEntries = new();
            AddValue(durationEntries, "Bless.Duration.Battles", data.DurationBattleCount);
            if (durationEntries.Count > 0)
            {
                groups.Add(new PresentationGroupData("Bless.Duration", durationEntries));
            }

            List<PresentationEntryData> runtimeEntries = new();
            AddValue(runtimeEntries, "Bless.Runtime.Level", data.Level);
            AddValue(runtimeEntries, "Bless.Runtime.RemainingBattles", data.RemainingBattleCount);
            AddNullableToken(runtimeEntries, "Bless.Runtime.Equipped", data.IsEquipped);
            AddNullableToken(runtimeEntries, "Bless.Runtime.Locked", data.IsLocked);
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
                groups.Add(new PresentationGroupData("Bless.Runtime", runtimeEntries));
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

        private static void AddValue(
            ICollection<PresentationEntryData> entries,
            string key,
            PresentationValueData value)
        {
            if (value != null)
            {
                entries.Add(new PresentationEntryData(key, new[] { value }));
            }
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

        private static PresentationValueData Number(
            BlessSO bless,
            double value,
            PresentationValueUnit unit,
            string field)
        {
            return PresentationValueData.Number(
                value,
                unit,
                Provenance(bless, PresentationProvenanceKind.AuthoredAsset, field));
        }

        private static PresentationValueData RuntimeNumber(
            BlessSO bless,
            double value,
            PresentationValueUnit unit,
            string field)
        {
            return PresentationValueData.Number(
                value,
                unit,
                Provenance(bless, PresentationProvenanceKind.RuntimeResolved, field));
        }

        private static PresentationProvenanceData Provenance(
            BlessSO bless,
            PresentationProvenanceKind kind,
            string field = null)
        {
            return new PresentationProvenanceData(
                kind,
                bless != null ? bless.BlessingId : string.Empty,
                sourceField: field);
        }
    }
}
