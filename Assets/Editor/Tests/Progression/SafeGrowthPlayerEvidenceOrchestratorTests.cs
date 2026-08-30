using System.Linq;
using System;
using System.Reflection;
using NUnit.Framework;
using Stage;
using UnityEditor;
using UnityEngine;

namespace Progression.EditModeTests
{
    public sealed class SafeGrowthPlayerEvidenceOrchestratorTests
    {
        private const string PopupPath =
            "Assets/Contents/Stage/so/node.act1.random_growth.02.windworn_sword_marks.intro.asset";
        private const string CatalogPath =
            "Assets/Resources/Stage/RandomGrowth/Presentation/event.act1.random_growth.02.windworn_sword_marks.ko-KR.asset";

        [Test]
        public void EditorNineProjectsCanonicalStatesThroughTypedSnapshots()
        {
            SafeGrowthPlayerEvidencePlan plan = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            foreach (SafeGrowthPlayerEvidenceCase evidenceCase in plan.Cases
                         .Where(x => x.Lane == SafeGrowthEvidenceLane.EditorG2))
            {
                Assert.That(Project(evidenceCase, out SafeGrowthPresentationSnapshot snapshot,
                    out string payload), Is.True, evidenceCase.Id);
                Assert.That(snapshot.State, Is.EqualTo(evidenceCase.ExpectedState));
                Assert.That(payload, Has.Length.EqualTo(64));
            }
        }

        [Test]
        public void MacEightProjectsOnlyVisualG2States()
        {
            SafeGrowthPlayerEvidencePlan plan = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            foreach (SafeGrowthPlayerEvidenceCase evidenceCase in plan.Cases
                         .Where(x => x.Lane == SafeGrowthEvidenceLane.MacG2))
                Assert.That(Project(evidenceCase, out _, out _), Is.True, evidenceCase.Id);
        }

        [Test]
        public void G3ProjectionIsRejected()
        {
            SafeGrowthPlayerEvidenceCase evidenceCase = SafeGrowthPlayerEvidencePlan.CreateCanonical()
                .Cases.First(x => x.Lane == SafeGrowthEvidenceLane.MacG3);
            Assert.That(Project(evidenceCase, out _, out _), Is.False);
        }

        [Test]
        public void WrongTokenAndPlanShaFailClosed()
        {
            SafeGrowthPlayerEvidencePlan plan = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            SafeGrowthPlayerEvidenceCase evidenceCase = plan.Cases[0];
            var orchestrator = new SafeGrowthPlayerEvidenceOrchestrator();
            Assert.That(orchestrator.TryProject(Popup(), Catalog(), evidenceCase, "wrong", plan.Sha256,
                out _, out _), Is.False);
            Assert.That(orchestrator.TryProject(Popup(), Catalog(), evidenceCase,
                SafeGrowthPlayerEvidencePlan.Token, new string('0', 64), out _, out _), Is.False);
        }

        [Test]
        public void CandidateTwoOneZeroRemainTypedAndDeterministic()
        {
            SafeGrowthPlayerEvidencePlan plan = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            foreach ((string marker, int count) in new[] { ("preconfirm-c2", 2), ("preconfirm-c1", 1), ("disabled-c0", 0) })
            {
                SafeGrowthPlayerEvidenceCase evidenceCase = plan.Cases.First(x => x.Id.Contains(marker));
                Assert.That(Project(evidenceCase, out SafeGrowthPresentationSnapshot snapshot,
                    out string first), Is.True);
                Assert.That(snapshot.DisplayCandidateCount, Is.EqualTo(count));
                Assert.That(Project(evidenceCase, out _, out string second), Is.True);
                Assert.That(second, Is.EqualTo(first));
            }
        }

        [Test]
        public void ProjectionUsesActualPopupCatalogAndCanonicalDigests()
        {
            Assert.That(Popup(), Is.Not.Null);
            Assert.That(Catalog(), Is.Not.Null);
            Assert.That(Project(SafeGrowthPlayerEvidencePlan.CreateCanonical().Cases[0],
                out SafeGrowthPresentationSnapshot snapshot, out _), Is.True);
            Assert.That(snapshot.EventId, Is.EqualTo(SafeGrowthTransactionIds.EventId));
            Assert.That(snapshot.SemanticCopyDigest,
                Is.EqualTo(SafeGrowthPresentationCopyResolver.V2SemanticDigest));
            Assert.That(snapshot.DefinitionFingerprint,
                Is.EqualTo(SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint));

            AssertIdentityFailure(ClonePopup(p => p.eventId = "wrong.popup"), Catalog(),
                "PopupSourceIdentityMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).eventId = "wrong.event"), Catalog(),
                "PayloadPortfolioEventMismatch");
            AssertIdentityFailure(ClonePopup(p =>
            {
                p.eventId = "wrong.node";
                Data(p).sourcePopupId = "wrong.node";
            }), Catalog(), "PayloadNodeIdentityMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).stageNodeId = "wrong.stage"), Catalog(),
                "PayloadStageIdentityMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).presentationCatalogId = "wrong.catalog"), Catalog(),
                "CatalogIdentityMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).presentationLocale = "en-US"), Catalog(),
                "CatalogLocaleMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).presentationProjectionKind = "wrong.kind"), Catalog(),
                "CatalogProjectionKindMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).presentationTextDigestKo = new string('0', 64)),
                Catalog(), "SemanticDigestMismatch");
            AssertIdentityFailure(ClonePopup(p => Data(p).definitionFingerprint = new string('0', 64)),
                Catalog(), "DefinitionFingerprintMismatch");
            AssertIdentityFailure(ClonePopup(_ => { }), CloneCatalog("catalogId", "wrong.catalog"),
                "CatalogIdentityMismatch");
            AssertIdentityFailure(ClonePopup(_ => { }), CloneCatalog("locale", "en-US"),
                "CatalogLocaleMismatch");
            AssertIdentityFailure(ClonePopup(_ => { }), CloneCatalog("projectionKind", "wrong.kind"),
                "CatalogProjectionKindMismatch");
            AssertIdentityFailure(ClonePopup(_ => { }),
                CloneCatalog("semanticCopyDigest", new string('0', 64)), "SemanticDigestMismatch");
            AssertIdentityFailure(ClonePopup(_ => { }),
                CloneCatalog("definitionFingerprint", new string('0', 64)),
                "DefinitionFingerprintMismatch");
        }

        private static void AssertIdentityFailure(PopupEventSO popup,
            RandomGrowthPresentationCopyAsset catalog, string expected)
        {
            Type orchestrator = typeof(SafeGrowthPlayerEvidenceOrchestrator);
            MethodInfo method = orchestrator.GetMethod("TryValidateIdentity",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            object[] args = { popup, catalog, null, null, null };
            Assert.That((bool)method.Invoke(null, args), Is.False);
            Assert.That(args[4].ToString(), Is.EqualTo(expected));
            UnityEngine.Object.DestroyImmediate(popup);
            if (!AssetDatabase.Contains(catalog)) UnityEngine.Object.DestroyImmediate(catalog);
        }

        private static PopupEventSO ClonePopup(Action<PopupEventSO> mutate)
        {
            PopupEventSO popup = UnityEngine.Object.Instantiate(Popup());
            mutate(popup);
            return popup;
        }

        private static RandomGrowthPresentationCopyAsset CloneCatalog(string propertyName, string value)
        {
            RandomGrowthPresentationCopyAsset catalog = UnityEngine.Object.Instantiate(Catalog());
            var serialized = new SerializedObject(catalog);
            SerializedProperty property = serialized.FindProperty(propertyName);
            Assert.That(property, Is.Not.Null, propertyName);
            property.stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static RandomGrowthChoiceExecutionData Data(PopupEventSO popup) =>
            popup.choices[0].executionConfig.data as RandomGrowthChoiceExecutionData;

        private static bool Project(SafeGrowthPlayerEvidenceCase evidenceCase,
            out SafeGrowthPresentationSnapshot snapshot, out string payload) =>
            new SafeGrowthPlayerEvidenceOrchestrator().TryProject(Popup(), Catalog(), evidenceCase,
                SafeGrowthPlayerEvidencePlan.Token, SafeGrowthPlayerEvidencePlan.CreateCanonical().Sha256,
                out snapshot, out payload);

        private static PopupEventSO Popup() => AssetDatabase.LoadAssetAtPath<PopupEventSO>(PopupPath);
        private static RandomGrowthPresentationCopyAsset Catalog() =>
            AssetDatabase.LoadAssetAtPath<RandomGrowthPresentationCopyAsset>(CatalogPath);
    }
}
