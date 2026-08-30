using System;
using System.Collections.Generic;

namespace Progression
{
    public sealed class ProgressionSourceRegistry
    {
        public const string FixedRescueSegment =
            "progress.segment.act1.chapter01.after_episode01_rescue";
        public const string FixedPartyUnitedSegment =
            "progress.segment.act1.chapter01.after_episode04_party_united";
        public const string RandomGrowthSegment =
            "progress.segment.act1.chapter01.random_before_episode06";
        public const string OptionalRandomGrowthSegment =
            "progress.segment.act1.chapter01.optional_random_growth";

        public const string RescueBattleSource =
            "battle.act1.chapter01.01.rescue_villagers";
        public const string PartyUnitedJihanSource =
            "choice.act1.chapter01.episode04_1.shared_north_trail.follow_bloody_trace_with_jihan";
        public const string PartyUnitedYujinSource =
            "choice.act1.chapter01.episode04_2.jihan_testimony.inspect_erased_trace_with_yujin";
        public const string RandomGrowthRiskSource =
            "choice.act1.random_growth.01.crying_bell_smithy_trial.take_heated_talisman";
        public const string RandomGrowthSafeSource =
            "choice.act1.random_growth.02.windworn_sword_marks.observe_sword_path";
        public const string PortfolioB1Event21SafeSource =
            "choice.act1.random_event.21.breath_between_water_drops.follow_silent_rhythm";
        public const string PortfolioB1Event22RiskSource =
            "choice.act1.random_event.22.sleeping_hawk_watch.keep_night_watch";
        public const string PortfolioB1Event23RiskSource =
            "choice.act1.random_event.23.temple_hundred_eight_steps.carry_stone_to_summit";

        private readonly HashSet<SourceKey> allowedSources = new();

        public ProgressionSourceRegistry()
        {
            Add(
                FixedRescueSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.BattleVictory,
                RescueBattleSource);
            Add(
                FixedPartyUnitedSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.MajorStoryResolution,
                PartyUnitedJihanSource);
            Add(
                FixedPartyUnitedSegment,
                ProgressionSourceCategory.Fixed,
                ProgressionSourceType.MajorStoryResolution,
                PartyUnitedYujinSource);
            Add(
                RandomGrowthSegment,
                ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventRisk,
                RandomGrowthRiskSource);
            Add(
                OptionalRandomGrowthSegment,
                ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventSafe,
                RandomGrowthSafeSource);
            Add(OptionalRandomGrowthSegment, ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventSafe, PortfolioB1Event21SafeSource);
            Add(OptionalRandomGrowthSegment, ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventRisk, PortfolioB1Event22RiskSource);
            Add(OptionalRandomGrowthSegment, ProgressionSourceCategory.Random,
                ProgressionSourceType.RandomEventRisk, PortfolioB1Event23RiskSource);
        }

        public bool IsAllowed(ProgressionEarnRequest request)
        {
            if (request == null || !request.IsStructurallyValid)
            {
                return false;
            }

            return allowedSources.Contains(
                new SourceKey(
                    request.SegmentId,
                    request.SourceCategory,
                    request.SourceType,
                    request.SourceId));
        }

        private void Add(
            string segmentId,
            ProgressionSourceCategory category,
            ProgressionSourceType sourceType,
            string sourceId)
        {
            allowedSources.Add(
                new SourceKey(segmentId, category, sourceType, sourceId));
        }

        private readonly struct SourceKey : IEquatable<SourceKey>
        {
            public SourceKey(
                string segmentId,
                ProgressionSourceCategory category,
                ProgressionSourceType sourceType,
                string sourceId)
            {
                SegmentId = segmentId;
                Category = category;
                SourceType = sourceType;
                SourceId = sourceId;
            }

            private string SegmentId { get; }
            private ProgressionSourceCategory Category { get; }
            private ProgressionSourceType SourceType { get; }
            private string SourceId { get; }

            public bool Equals(SourceKey other) =>
                string.Equals(SegmentId, other.SegmentId, StringComparison.Ordinal)
                && Category == other.Category
                && SourceType == other.SourceType
                && string.Equals(SourceId, other.SourceId, StringComparison.Ordinal);

            public override bool Equals(object obj) =>
                obj is SourceKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = StringComparer.Ordinal.GetHashCode(SegmentId ?? string.Empty);
                    hash = (hash * 397) ^ (int)Category;
                    hash = (hash * 397) ^ (int)SourceType;
                    hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(SourceId ?? string.Empty);
                    return hash;
                }
            }
        }
    }
}
