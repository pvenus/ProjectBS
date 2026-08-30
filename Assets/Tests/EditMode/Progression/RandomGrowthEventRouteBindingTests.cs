using NUnit.Framework;

namespace Progression.Tests
{
    public sealed class RandomGrowthEventRouteBindingTests
    {
        [Test]
        public void ExactB1RouteCreatesTypedBinding()
        {
            Assert.That(RandomGrowthEventRouteBinding.TryCreate(
                "event.act1.random_event.23.temple_hundred_eight_steps",
                "node.act1.random_event.23.temple_hundred_eight_steps.intro",
                "reservation.act1.chapter01.portfolio_b1.event23",
                "choice.act1.random_event.23.temple_hundred_eight_steps.carry_stone_to_summit",
                "result.act1.random_event.23.temple_hundred_eight_steps.risk_growth_granted",
                RandomGrowthPayloadKind.Risk, out RandomGrowthEventRouteBinding binding), Is.True);
            Assert.That(binding.TransactionDomain, Is.EqualTo("random-growth-risk"));
            Assert.That(binding.StableRouteKey, Does.Contain(binding.Identity.EventId));
        }

        [TestCase("node.act1.random_event.22.sleeping_hawk_watch.intro.alias")]
        [TestCase("node.act1.random_growth.01.crying_bell_smithy_trial.intro")]
        public void NodeAliasOrCrossEventIdentityFailsClosed(string nodeId)
        {
            Assert.That(RandomGrowthEventRouteBinding.TryCreate(
                "event.act1.random_event.22.sleeping_hawk_watch", nodeId,
                "reservation.act1.chapter01.portfolio_b1.event22",
                "choice.act1.random_event.22.sleeping_hawk_watch.keep_night_watch",
                "result.act1.random_event.22.sleeping_hawk_watch.risk_growth_granted",
                RandomGrowthPayloadKind.Risk, out _), Is.False);
        }
    }
}
