using System;
using System.Collections.Generic;
using System.Linq;

namespace Progression
{
    public enum RandomGrowthPayloadKind
    {
        Safe = 10,
        Risk = 20,
        Decline = 30
    }

    public sealed class RandomGrowthEventIdentity
    {
        public RandomGrowthEventIdentity(string eventId, string nodeId, string reservationId,
            string choiceId, string resultId, string segmentId, string sourceId,
            RandomGrowthPayloadKind payloadKind)
        {
            EventId = eventId;
            NodeId = nodeId;
            ReservationId = reservationId;
            ChoiceId = choiceId;
            ResultId = resultId;
            SegmentId = segmentId;
            SourceId = sourceId;
            PayloadKind = payloadKind;
        }

        public string EventId { get; }
        public string NodeId { get; }
        public string ReservationId { get; }
        public string ChoiceId { get; }
        public string ResultId { get; }
        public string SegmentId { get; }
        public string SourceId { get; }
        public RandomGrowthPayloadKind PayloadKind { get; }
        public string RouteId => string.Join("\n", EventId, NodeId, ReservationId, ChoiceId,
            ResultId, SegmentId, SourceId, PayloadKind.ToString());
    }

    public static class RandomGrowthEventIdentityCatalog
    {
        private const string OptionalSegment = ProgressionSourceRegistry.OptionalRandomGrowthSegment;
        private const string LegacyRiskSegment = ProgressionSourceRegistry.RandomGrowthSegment;

        private static readonly IReadOnlyList<RandomGrowthEventIdentity> entries =
            Array.AsReadOnly(new[]
            {
                Identity("event.act1.random_growth.01.crying_bell_smithy_trial",
                    "node.act1.random_growth.01.crying_bell_smithy_trial.intro",
                    "reservation.act1.chapter01.random_growth.before_episode06",
                    "take_heated_talisman", "risk_growth_granted", LegacyRiskSegment,
                    RandomGrowthPayloadKind.Risk),
                Identity("event.act1.random_growth.01.crying_bell_smithy_trial",
                    "node.act1.random_growth.01.crying_bell_smithy_trial.intro",
                    "reservation.act1.chapter01.random_growth.before_episode06",
                    "leave_forge", "declined", LegacyRiskSegment,
                    RandomGrowthPayloadKind.Decline),
                Identity("event.act1.random_growth.02.windworn_sword_marks",
                    "node.act1.random_growth.02.windworn_sword_marks.intro",
                    "reservation.act1.chapter01.random_growth.after_episode02",
                    "observe_sword_path", "safe_growth_granted", OptionalSegment,
                    RandomGrowthPayloadKind.Safe),
                Identity("event.act1.random_growth.02.windworn_sword_marks",
                    "node.act1.random_growth.02.windworn_sword_marks.intro",
                    "reservation.act1.chapter01.random_growth.after_episode02",
                    "leave_training_ground", "declined", OptionalSegment,
                    RandomGrowthPayloadKind.Decline),
                Identity("event.act1.random_event.21.breath_between_water_drops",
                    "node.act1.random_event.21.breath_between_water_drops.intro",
                    "reservation.act1.chapter01.portfolio_b1.event21",
                    "follow_silent_rhythm", "safe_growth_granted", OptionalSegment,
                    RandomGrowthPayloadKind.Safe),
                Identity("event.act1.random_event.21.breath_between_water_drops",
                    "node.act1.random_event.21.breath_between_water_drops.intro",
                    "reservation.act1.chapter01.portfolio_b1.event21",
                    "leave_cave_unchanged", "declined", OptionalSegment,
                    RandomGrowthPayloadKind.Decline),
                Identity("event.act1.random_event.22.sleeping_hawk_watch",
                    "node.act1.random_event.22.sleeping_hawk_watch.intro",
                    "reservation.act1.chapter01.portfolio_b1.event22",
                    "keep_night_watch", "risk_growth_granted", OptionalSegment,
                    RandomGrowthPayloadKind.Risk),
                Identity("event.act1.random_event.22.sleeping_hawk_watch",
                    "node.act1.random_event.22.sleeping_hawk_watch.intro",
                    "reservation.act1.chapter01.portfolio_b1.event22",
                    "leave_hawk_sleeping", "declined", OptionalSegment,
                    RandomGrowthPayloadKind.Decline),
                Identity("event.act1.random_event.23.temple_hundred_eight_steps",
                    "node.act1.random_event.23.temple_hundred_eight_steps.intro",
                    "reservation.act1.chapter01.portfolio_b1.event23",
                    "carry_stone_to_summit", "risk_growth_granted", OptionalSegment,
                    RandomGrowthPayloadKind.Risk)
            });

        public static IReadOnlyList<RandomGrowthEventIdentity> Entries => entries;

        public static bool TryResolve(string eventId, string choiceId,
            RandomGrowthPayloadKind payloadKind, out RandomGrowthEventIdentity identity)
        {
            identity = entries.SingleOrDefault(item => item.PayloadKind == payloadKind
                && string.Equals(item.EventId, eventId, StringComparison.Ordinal)
                && string.Equals(item.ChoiceId, choiceId, StringComparison.Ordinal));
            return identity != null;
        }

        private static RandomGrowthEventIdentity Identity(string eventId, string nodeId,
            string reservationId, string choiceSuffix, string resultSuffix, string segmentId,
            RandomGrowthPayloadKind payloadKind)
        {
            string choiceId = $"choice.{eventId.Substring("event.".Length)}.{choiceSuffix}";
            string resultId = $"result.{eventId.Substring("event.".Length)}.{resultSuffix}";
            return new RandomGrowthEventIdentity(eventId, nodeId, reservationId, choiceId,
                resultId, segmentId, choiceId, payloadKind);
        }
    }
}
