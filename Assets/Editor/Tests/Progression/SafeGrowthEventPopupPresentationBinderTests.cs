using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Stage;

namespace Progression.EditModeTests
{
    public sealed class SafeGrowthEventPopupPresentationBinderTests
    {
        [TestCase(SafeGrowthPresentationActionIntent.RequestObservePreconfirm)]
        [TestCase(SafeGrowthPresentationActionIntent.ConfirmObserve)]
        [TestCase(SafeGrowthPresentationActionIntent.RetrySameChoice)]
        [TestCase(SafeGrowthPresentationActionIntent.OpenGrowthOffer)]
        [TestCase(SafeGrowthPresentationActionIntent.ContinueStage)]
        public void PrimaryActionsUseCanonicalSnapshotCta(SafeGrowthPresentationActionIntent intent)
        {
            SafeGrowthPresentationSnapshot snapshot = Snapshot();
            Assert.That(SafeGrowthEventPopupPresentationBinder.ResolveLabel(snapshot, intent),
                Is.EqualTo("canonical-cta"));
        }

        [Test]
        public void CancelAndDeclineUseCanonicalCancelCta()
        {
            SafeGrowthPresentationSnapshot snapshot = Snapshot();
            Assert.That(SafeGrowthEventPopupPresentationBinder.ResolveLabel(snapshot,
                SafeGrowthPresentationActionIntent.CancelPreconfirm), Is.EqualTo("canonical-cancel"));
            Assert.That(SafeGrowthEventPopupPresentationBinder.ResolveLabel(snapshot,
                SafeGrowthPresentationActionIntent.ConfirmDecline), Is.EqualTo("canonical-cancel"));
        }

        [Test]
        public void RecheckUsesCanonicalRecheckCta()
        {
            Assert.That(SafeGrowthEventPopupPresentationBinder.ResolveLabel(Snapshot(),
                SafeGrowthPresentationActionIntent.RecheckEligibility), Is.EqualTo("canonical-recheck"));
        }

        [Test]
        public void NoneHasNoListenerLabel()
        {
            Assert.That(SafeGrowthEventPopupPresentationBinder.ResolveLabel(Snapshot(),
                SafeGrowthPresentationActionIntent.None), Is.Empty);
        }

        private static SafeGrowthPresentationSnapshot Snapshot()
        {
            Type[] signature =
            {
                typeof(SafeGrowthPresentationState), typeof(SafeGrowthPresentationDisabledReason),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(SafeGrowthPresentationCopy), typeof(bool), typeof(bool), typeof(int),
                typeof(IReadOnlyList<SafeGrowthPresentationActionIntent>),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
                typeof(string), typeof(string), typeof(string), typeof(string), typeof(string)
            };
            ConstructorInfo ctor = typeof(SafeGrowthPresentationSnapshot).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic, null, signature, null);
            Assert.That(ctor, Is.Not.Null, "Approved internal 27-argument snapshot signature drifted.");
            return (SafeGrowthPresentationSnapshot)ctor.Invoke(new object[]
            {
                SafeGrowthPresentationState.Preconfirm, SafeGrowthPresentationDisabledReason.None,
                "event", "node", "popup", "instance", "observe", "decline", "result", "token",
                "revision", "fingerprint", null, true, true, 2,
                new[] { SafeGrowthPresentationActionIntent.ConfirmObserve },
                "title", "body", "method", "reward", "cap", "assist", "status",
                "canonical-cta", "canonical-cancel", "canonical-recheck"
            });
        }
    }
}
