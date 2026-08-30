using System.IO;
using NUnit.Framework;
using Stage;

namespace Progression.EditModeTests
{
    public sealed class SafeGrowthPlayerEvidencePlanTests
    {
        [Test] public void CanonicalPlanHasExactSeventeenG2AndTenG3Cases()
        { var p = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            Assert.That((p.EditorG2Count, p.MacG2Count, p.MacG3Count), Is.EqualTo((9, 8, 10))); }

        [Test] public void PlanContainsOnlyEditorAndMacStandaloneLanes()
        { var p = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            foreach (SafeGrowthPlayerEvidenceCase item in p.Cases)
                Assert.That(item.Lane == SafeGrowthEvidenceLane.EditorG2
                    || item.Lane == SafeGrowthEvidenceLane.MacG2
                    || item.Lane == SafeGrowthEvidenceLane.MacG3, Is.True); }

        [Test] public void CanonicalPlanShaIsStableAcrossConstructionAndCulture()
        { Assert.That(SafeGrowthPlayerEvidencePlan.CreateCanonical().Sha256,
            Is.EqualTo(SafeGrowthPlayerEvidencePlan.CreateCanonical().Sha256)); }

        [Test] public void OutputRootIsRestrictedToPrivateTmpUnitRoot()
        { Assert.That(SafeGrowthPlayerEvidencePlan.TryValidateOutputRoot(
            "/private/tmp/projectbs-safe-mac-player.fixture", out _), Is.True);
          Assert.That(SafeGrowthPlayerEvidencePlan.TryValidateOutputRoot("/private/tmp/other", out _), Is.False); }

        [Test] public void BootstrapRejectsWrongTokenShaAndDuplicateArguments()
        { var p = SafeGrowthPlayerEvidencePlan.CreateCanonical(); string root = "/private/tmp/projectbs-safe-mac-player.fixture";
          Assert.That(SafeGrowthPlayerEvidenceBootstrap.TryParse(new[] {
              SafeGrowthPlayerEvidenceBootstrap.TokenArgument + SafeGrowthPlayerEvidencePlan.Token,
              SafeGrowthPlayerEvidenceBootstrap.PlanArgument + p.Sha256,
              SafeGrowthPlayerEvidenceBootstrap.OutputArgument + root }, out SafeGrowthEvidenceLaunchContext valid), Is.True);
          Assert.That(valid.PlanSha, Is.EqualTo(p.Sha256));
          Assert.That(SafeGrowthPlayerEvidenceBootstrap.TryParse(new[] {
              SafeGrowthPlayerEvidenceBootstrap.TokenArgument + "wrong",
              SafeGrowthPlayerEvidenceBootstrap.PlanArgument + p.Sha256,
              SafeGrowthPlayerEvidenceBootstrap.OutputArgument + root }, out _), Is.False);
          Assert.That(SafeGrowthPlayerEvidenceBootstrap.TryParse(new[] {
              SafeGrowthPlayerEvidenceBootstrap.TokenArgument + SafeGrowthPlayerEvidencePlan.Token,
              SafeGrowthPlayerEvidenceBootstrap.PlanArgument + p.Sha256,
              SafeGrowthPlayerEvidenceBootstrap.OutputArgument + root,
              SafeGrowthPlayerEvidenceBootstrap.OutputArgument + root }, out _), Is.False); }
    }
}
