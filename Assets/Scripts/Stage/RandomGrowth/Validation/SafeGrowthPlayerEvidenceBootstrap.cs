using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Stage
{
    public readonly struct SafeGrowthEvidenceLaunchContext
    {
        public SafeGrowthEvidenceLaunchContext(string outputRoot, string planSha)
        { OutputRoot = outputRoot; PlanSha = planSha; }
        public string OutputRoot { get; }
        public string PlanSha { get; }
    }

    public static class SafeGrowthPlayerEvidenceBootstrap
    {
        public const string TokenArgument = "--projectbs-safe-evidence-token=";
        public const string PlanArgument = "--projectbs-safe-evidence-plan-sha=";
        public const string OutputArgument = "--projectbs-safe-evidence-output=";

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TryStart()
        {
            if (!string.Equals(SceneManager.GetActiveScene().path,
                    SafeGrowthPlayerEvidencePlan.StageScenePath, StringComparison.Ordinal)
                || !TryParse(Environment.GetCommandLineArgs(), out SafeGrowthEvidenceLaunchContext context))
                return;
            if (UnityEngine.Object.FindFirstObjectByType<SafeGrowthPlayerEvidenceCaptureDriver>(
                    FindObjectsInactive.Include) != null) return;
            GameObject host = new("__SafeGrowthMacPlayerEvidence");
            UnityEngine.Object.DontDestroyOnLoad(host);
            host.AddComponent<SafeGrowthPlayerEvidenceCaptureDriver>().Initialize(
                SafeGrowthPlayerEvidencePlan.CreateCanonical(), context);
        }
#endif

        public static bool TryParse(string[] args, out SafeGrowthEvidenceLaunchContext context)
        {
            context = default;
            string token = Find(args, TokenArgument), sha = Find(args, PlanArgument), root = Find(args, OutputArgument);
            SafeGrowthPlayerEvidencePlan plan = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            if (!string.Equals(token, SafeGrowthPlayerEvidencePlan.Token, StringComparison.Ordinal)
                || !string.Equals(sha, plan.Sha256, StringComparison.Ordinal)
                || !SafeGrowthPlayerEvidencePlan.TryValidateOutputRoot(root, out string normalized)) return false;
            context = new SafeGrowthEvidenceLaunchContext(normalized, sha); return true;
        }

        private static string Find(string[] args, string prefix)
        {
            string[] input = args ?? Array.Empty<string>();
            string found = null;
            foreach (string arg in input)
            {
                if (arg?.StartsWith(prefix, StringComparison.Ordinal) != true) continue;
                if (found != null) return null;
                found = arg.Substring(prefix.Length);
            }
            return found;
        }
    }
}
