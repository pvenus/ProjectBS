using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Progression;
using UnityEngine;

namespace Stage
{
    [Serializable]
    public sealed class SafeGrowthEvidenceStateRecord
    {
        public string planVersion;
        public string planSha;
        public string caseId;
        public string lane;
        public int width;
        public int height;
        public string expectedState;
        public string unityVersion;
        public string platform;
        public string graphicsDevice;
        public string graphicsFormat;
        public int sourceTextureWidth;
        public int sourceTextureHeight;
        public float canvasScaleFactor;
        public Rect safeArea;
        public Vector3[] popupWorldCorners;
        public string catalogResource;
        public string semanticDigest;
        public string definitionFingerprint;
        public string stateAuthority;
        public string evidenceSchema;
        public string payloadSha;
        public string tokenSha;
        public bool provesVisualPresentation;
        public bool provesDomainBehavior;
        public bool provesLedgerBehavior;
        public bool provesTerminalLifecycle;
    }

    public sealed class SafeGrowthPlayerEvidenceCaptureDriver : MonoBehaviour
    {
        private SafeGrowthPlayerEvidencePlan plan;
        private SafeGrowthEvidenceLaunchContext context;

        public void Initialize(SafeGrowthPlayerEvidencePlan value, SafeGrowthEvidenceLaunchContext launch)
        {
            plan = value;
            context = launch;
            StartCoroutine(RunOrdered());
        }

        private IEnumerator RunOrdered()
        {
            StagePopupEventManager manager = null;
            for (int frame = 0; frame < 300; frame++)
            {
                if (TryBindAuthorities(out _, out manager, out _)) break;
                yield return null;
            }
            if (manager == null) yield break;
            List<string> captured = new();
            foreach (SafeGrowthPlayerEvidenceCase evidenceCase in plan.Cases)
            {
                bool selected = Application.isEditor
                    ? evidenceCase.Lane == SafeGrowthEvidenceLane.EditorG2
                    : evidenceCase.Lane == SafeGrowthEvidenceLane.MacG2;
                if (!selected) continue;
                Screen.SetResolution(evidenceCase.Width, evidenceCase.Height, false);
                yield return new WaitForEndOfFrame();
                if (!manager.TryRenderSafeGrowthEvidenceProjection(evidenceCase,
                        SafeGrowthPlayerEvidencePlan.Token, plan.Sha256,
                        out SafeGrowthPresentationSnapshot snapshot, out string payloadSha))
                    yield break;
                yield return new WaitForEndOfFrame();
                if (!TryCapturePair(evidenceCase, snapshot, payloadSha, out _, out _)) yield break;
                captured.Add(evidenceCase.Id);
            }
            int expected = Application.isEditor ? plan.EditorG2Count : plan.MacG2Count;
            if (captured.Count != expected) yield break;
            string marker = Path.Combine(context.OutputRoot,
                Application.isEditor ? "editor-g2.complete.json" : "mac-g2.complete.json");
            if (File.Exists(marker)) yield break;
            string body = JsonUtility.ToJson(new CompletionRecord
            { planSha = plan.Sha256, count = captured.Count, caseIds = captured.ToArray() }, true);
            WriteAtomic(marker, body);
            if (!Application.isEditor) Application.Quit(0);
        }

        public bool TryBindAuthorities(out EventPopupView view, out StagePopupEventManager manager,
            out RandomGrowthPresentationCopyAsset catalog)
        {
            view = FindFirstObjectByType<EventPopupView>(FindObjectsInactive.Include);
            manager = FindFirstObjectByType<StagePopupEventManager>(FindObjectsInactive.Include);
            catalog = Resources.Load<RandomGrowthPresentationCopyAsset>(
                SafeGrowthPlayerEvidencePlan.PresentationCatalogResource);
            return plan != null && view != null && manager != null && catalog != null
                && catalog.Fields.Count == 31;
        }

        public bool TryCapturePair(SafeGrowthPlayerEvidenceCase evidenceCase,
            SafeGrowthPresentationSnapshot snapshot, string payloadSha,
            out string pngPath, out string statePath)
        {
            pngPath = string.Empty; statePath = string.Empty;
            if (plan == null || evidenceCase == null || snapshot == null
                || !string.Equals(context.PlanSha, plan.Sha256, StringComparison.Ordinal)
                || snapshot.State != evidenceCase.ExpectedState
                || Screen.width != evidenceCase.Width || Screen.height != evidenceCase.Height
                || snapshot.SemanticCopyDigest != SafeGrowthPresentationCopyResolver.V2SemanticDigest
                || snapshot.DefinitionFingerprint != SafeGrowthPresentationCopyResolver.V2DefinitionFingerprint
                || !TryBindAuthorities(out EventPopupView view, out StagePopupEventManager manager, out _)
                || manager.CurrentEvent?.eventId != SafeGrowthTransactionIds.EventId
                || manager.CurrentEvent.mainImage?.texture == null
                || !SafeGrowthPlayerEvidencePlan.TryValidateOutputRoot(context.OutputRoot, out string root))
                return false;
            Texture sourceTexture = manager.CurrentEvent.mainImage.texture;
            string directory = Path.Combine(root, evidenceCase.Lane.ToString().ToLowerInvariant());
            Directory.CreateDirectory(directory);
            if ((new DirectoryInfo(directory).Attributes & FileAttributes.ReparsePoint) != 0) return false;
            pngPath = Path.Combine(directory, evidenceCase.Id + ".png");
            statePath = Path.Combine(directory, evidenceCase.Id + ".state.json");
            if (File.Exists(pngPath) || File.Exists(statePath)) return false;
            Canvas canvas = view.GetComponentInParent<Canvas>();
            RectTransform popup = view.transform as RectTransform;
            Vector3[] corners = new Vector3[4];
            popup?.GetWorldCorners(corners);
            var record = new SafeGrowthEvidenceStateRecord
            {
                planVersion = SafeGrowthPlayerEvidencePlan.Version, planSha = plan.Sha256,
                caseId = evidenceCase.Id, lane = evidenceCase.Lane.ToString(),
                width = Screen.width, height = Screen.height,
                expectedState = evidenceCase.ExpectedState.ToString(), unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(), graphicsDevice = SystemInfo.graphicsDeviceName,
                graphicsFormat = sourceTexture.graphicsFormat.ToString(),
                sourceTextureWidth = sourceTexture.width, sourceTextureHeight = sourceTexture.height,
                canvasScaleFactor = canvas != null ? canvas.scaleFactor : 0f,
                safeArea = Screen.safeArea, popupWorldCorners = corners,
                catalogResource = SafeGrowthPlayerEvidencePlan.PresentationCatalogResource,
                semanticDigest = snapshot.SemanticCopyDigest,
                definitionFingerprint = snapshot.DefinitionFingerprint,
                stateAuthority = SafeGrowthPlayerEvidenceOrchestrator.Authority,
                evidenceSchema = SafeGrowthPlayerEvidenceOrchestrator.Schema,
                payloadSha = payloadSha,
                tokenSha = SafeGrowthPlayerEvidenceOrchestrator.HashFields(SafeGrowthPlayerEvidencePlan.Token),
                provesVisualPresentation = true,
                provesDomainBehavior = false,
                provesLedgerBehavior = false,
                provesTerminalLifecycle = false
            };
            Texture2D screen = new(Screen.width, Screen.height, TextureFormat.RGBA32, false, false);
            try
            {
                screen.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
                screen.Apply(false, false);
                byte[] png = screen.EncodeToPNG();
                if (png == null || png.Length == 0) return false;
                WriteAtomic(pngPath, png);
                WriteAtomic(statePath, JsonUtility.ToJson(record, true));
                return true;
            }
            catch
            {
                // Preserve any partial evidence for diagnosis; cleanup is an explicit later gate.
                return false;
            }
            finally { Destroy(screen); }
        }

        private static void WriteAtomic(string path, string text) =>
            WriteAtomic(path, System.Text.Encoding.UTF8.GetBytes(text));

        private static void WriteAtomic(string path, byte[] bytes)
        {
            string temporary = path + ".tmp";
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path);
        }

        [Serializable]
        private sealed class CompletionRecord
        {
            public string planSha;
            public int count;
            public string[] caseIds;
        }
    }
}
