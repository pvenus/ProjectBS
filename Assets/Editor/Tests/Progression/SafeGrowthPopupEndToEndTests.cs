using NUnit.Framework;
using Stage;

public sealed class SafeGrowthPopupEndToEndTests
{
    [TestCase(SafeGrowthPresentationActionIntent.OpenGrowthOffer, true,
        SafeGrowthTerminalNavigationDecision.OpenGrowthOffer)]
    [TestCase(SafeGrowthPresentationActionIntent.OpenGrowthOffer, false,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    [TestCase(SafeGrowthPresentationActionIntent.ContinueStage, false,
        SafeGrowthTerminalNavigationDecision.CloseStagePopup)]
    [TestCase(SafeGrowthPresentationActionIntent.ConfirmObserve, true,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    [TestCase(SafeGrowthPresentationActionIntent.ConfirmDecline, true,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    [TestCase(SafeGrowthPresentationActionIntent.RetrySameChoice, true,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    [TestCase(SafeGrowthPresentationActionIntent.RecheckEligibility, true,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    [TestCase(SafeGrowthPresentationActionIntent.CancelPreconfirm, true,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    [TestCase(SafeGrowthPresentationActionIntent.None, true,
        SafeGrowthTerminalNavigationDecision.KeepPopup)]
    public void TerminalNavigationIsExplicitAndNeverClosesBeforeSuccessfulIntent(
        SafeGrowthPresentationActionIntent intent, bool offerOpened,
        SafeGrowthTerminalNavigationDecision expected)
    {
        Assert.That(SafeGrowthTerminalNavigationPolicy.Resolve(intent, offerOpened), Is.EqualTo(expected));
    }
}
