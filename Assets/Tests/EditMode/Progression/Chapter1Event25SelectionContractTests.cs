using System.Linq;
using NUnit.Framework;
using Progression.Portfolio;

namespace Progression.Tests
{
    public sealed class Chapter1Event25SelectionContractTests
    {
        [Test]
        public void Event18AndEvent25ShareBattleReuseExclusionGroup()
        {
            PortfolioEventDescriptor event18 =
                Chapter1Event25SelectionContract.CreateOriginalEvent18();
            PortfolioEventDescriptor event25 =
                Chapter1Event25SelectionContract.CreateEvent25();

            Assert.That(event18.EventId,
                Is.EqualTo(Chapter1Event25SelectionContract.OriginalEvent18Id));
            Assert.That(event25.EventId,
                Is.EqualTo(Chapter1Event25SelectionContract.Event25Id));
            Assert.That(event18.ExclusionGroup,
                Is.EqualTo(Chapter1Event25SelectionContract.BattleReuseExclusionGroup));
            Assert.That(event25.ExclusionGroup, Is.EqualTo(event18.ExclusionGroup));
        }

        [Test]
        public void SelectorNeverReturnsBothBattleSharingEvents()
        {
            var registry = Enumerable.Range(0, 48)
                .Select(index => new PortfolioEventDescriptor(
                    "event.fixture.event25." + index,
                    (PortfolioPurpose)(index % 8 * 10),
                    "motif.event25." + index))
                .Concat(new[]
                {
                    Chapter1Event25SelectionContract.CreateOriginalEvent18(),
                    Chapter1Event25SelectionContract.CreateEvent25()
                });

            for (int run = 0; run < 64; run++)
            {
                Chapter1PortfolioManifest manifest = new Chapter1PortfolioManifestBuilder().Build(
                    "run.event25." + run, "stage.chapter1", registry);
                Assert.That(manifest.Status, Is.EqualTo(PortfolioManifestStatus.Ready));
                Assert.That(manifest.Encounters.Count(item =>
                    item.ExclusionGroup == Chapter1Event25SelectionContract.BattleReuseExclusionGroup),
                    Is.LessThanOrEqualTo(1));
            }
        }

        [Test]
        public void SelectorCanonicalizesRawProductionDescriptors()
        {
            var registry = Enumerable.Range(0, 48)
                .Select(index => new PortfolioEventDescriptor(
                    "event.fixture.raw." + index,
                    (PortfolioPurpose)(index % 8 * 10),
                    "motif.raw." + index))
                .Concat(new[]
                {
                    new PortfolioEventDescriptor(
                        Chapter1Event25SelectionContract.OriginalEvent18Id,
                        PortfolioPurpose.World, "raw.event18"),
                    new PortfolioEventDescriptor(
                        Chapter1Event25SelectionContract.Event25Id,
                        PortfolioPurpose.Relic, "raw.event25")
                });

            for (int run = 0; run < 64; run++)
            {
                Chapter1PortfolioManifest manifest = new Chapter1PortfolioManifestBuilder().Build(
                    "run.raw.event25." + run, "stage.chapter1", registry);
                Assert.That(manifest.Encounters.Count(item =>
                    item.EventId == Chapter1Event25SelectionContract.OriginalEvent18Id
                    || item.EventId == Chapter1Event25SelectionContract.Event25Id),
                    Is.LessThanOrEqualTo(1));
            }
        }
    }
}
