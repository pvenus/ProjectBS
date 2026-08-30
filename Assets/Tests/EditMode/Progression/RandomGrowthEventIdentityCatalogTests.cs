using System.Linq;
using NUnit.Framework;

namespace Progression.Tests
{
    public sealed class RandomGrowthEventIdentityCatalogTests
    {
        [Test]
        public void EveryTypedRouteIsUniqueAndComplete()
        {
            Assert.That(RandomGrowthEventIdentityCatalog.Entries, Has.Count.EqualTo(9));
            Assert.That(RandomGrowthEventIdentityCatalog.Entries.Select(item => item.RouteId)
                .Distinct().Count(), Is.EqualTo(9));
            Assert.That(RandomGrowthEventIdentityCatalog.Entries.All(item =>
                !string.IsNullOrWhiteSpace(item.EventId)
                && !string.IsNullOrWhiteSpace(item.NodeId)
                && !string.IsNullOrWhiteSpace(item.ReservationId)
                && !string.IsNullOrWhiteSpace(item.ChoiceId)
                && !string.IsNullOrWhiteSpace(item.ResultId)
                && item.ChoiceId == item.SourceId), Is.True);
        }

        [TestCase("event.act1.random_growth.01.crying_bell_smithy_trial",
            "choice.act1.random_growth.01.crying_bell_smithy_trial.take_heated_talisman",
            RandomGrowthPayloadKind.Risk)]
        [TestCase("event.act1.random_growth.02.windworn_sword_marks",
            "choice.act1.random_growth.02.windworn_sword_marks.observe_sword_path",
            RandomGrowthPayloadKind.Safe)]
        [TestCase("event.act1.random_event.21.breath_between_water_drops",
            "choice.act1.random_event.21.breath_between_water_drops.follow_silent_rhythm",
            RandomGrowthPayloadKind.Safe)]
        [TestCase("event.act1.random_event.22.sleeping_hawk_watch",
            "choice.act1.random_event.22.sleeping_hawk_watch.keep_night_watch",
            RandomGrowthPayloadKind.Risk)]
        [TestCase("event.act1.random_event.23.temple_hundred_eight_steps",
            "choice.act1.random_event.23.temple_hundred_eight_steps.carry_stone_to_summit",
            RandomGrowthPayloadKind.Risk)]
        public void LegacyAndB1TypedIdentitiesResolveWithoutAlias(string eventId,
            string choiceId, RandomGrowthPayloadKind kind)
        {
            Assert.That(RandomGrowthEventIdentityCatalog.TryResolve(eventId, choiceId, kind,
                out RandomGrowthEventIdentity identity), Is.True);
            Assert.That(identity.EventId, Is.EqualTo(eventId));
            Assert.That(identity.ChoiceId, Is.EqualTo(choiceId));
        }
    }
}
