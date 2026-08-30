using System;
using System.IO;
using Stage;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SafeGrowthPlayerEvidenceBuildDriver
{
    private const string EditorG2ActiveKey = "ProjectBS.SafeGrowthEvidence.EditorG2.Active";
    private const string EditorG2RootKey = "ProjectBS.SafeGrowthEvidence.EditorG2.Root";
    private const string EditorG2DeadlineKey = "ProjectBS.SafeGrowthEvidence.EditorG2.Deadline";

    public static BuildPlayerOptions CreateOptions(string outputPath)
    {
        if (!SafeGrowthPlayerEvidencePlan.TryValidateOutputRoot(outputPath, out string root))
            throw new ArgumentException("SAFE_EVIDENCE_OUTPUT_ROOT_INVALID", nameof(outputPath));
        return new BuildPlayerOptions
        {
            scenes = new[] { SafeGrowthPlayerEvidencePlan.StageScenePath },
            locationPathName = Path.Combine(root, "build", "standalone", "ProjectBS.app"),
            target = BuildTarget.StandaloneOSX,
            targetGroup = BuildTargetGroup.Standalone,
            options = BuildOptions.Development
        };
    }

    public static BuildReport Build(string outputPath)
    {
        BuildPlayerOptions options = CreateOptions(outputPath);
        return BuildPipeline.BuildPlayer(options);
    }

    public static void RunFromCommandLine()
    {
        int exitCode = 1;
        try
        {
            if (!SafeGrowthPlayerEvidenceBootstrap.TryParse(Environment.GetCommandLineArgs(),
                    out SafeGrowthEvidenceLaunchContext context))
                throw new InvalidOperationException("SAFE_EVIDENCE_COMMAND_LINE_INVALID");
            BuildReport report = Build(context.OutputRoot);
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("SAFE_EVIDENCE_BUILD_FAILED");
            string path = Path.Combine(context.OutputRoot, "build-report.json");
            Directory.CreateDirectory(context.OutputRoot);
            string json = JsonUtility.ToJson(new BuildReceipt
            {
                result = report.summary.result.ToString(),
                outputPath = report.summary.outputPath,
                totalSize = report.summary.totalSize,
                totalErrors = report.summary.totalErrors,
                totalWarnings = report.summary.totalWarnings,
                planSha = context.PlanSha
            }, true);
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, json);
            File.Move(temporary, path);
            exitCode = 0;
        }
        catch (Exception exception)
        {
            Debug.LogError("[SafeGrowthPlayerEvidenceBuildDriver] " + exception);
        }
        finally
        {
            EditorApplication.Exit(exitCode);
        }
    }

    public static void RunEditorG2()
    {
        try
        {
            if (!SafeGrowthPlayerEvidenceBootstrap.TryParse(Environment.GetCommandLineArgs(),
                    out SafeGrowthEvidenceLaunchContext context))
                throw new InvalidOperationException("SAFE_EVIDENCE_COMMAND_LINE_INVALID");
            string marker = Path.Combine(context.OutputRoot, "editor-g2.complete.json");
            if (File.Exists(marker))
                throw new InvalidOperationException("SAFE_EVIDENCE_EDITOR_G2_OUTPUT_COLLISION");
            Directory.CreateDirectory(context.OutputRoot);
            SessionState.SetBool(EditorG2ActiveKey, true);
            SessionState.SetString(EditorG2RootKey, context.OutputRoot);
            SessionState.SetString(EditorG2DeadlineKey,
                DateTime.UtcNow.AddMinutes(10).Ticks.ToString());
            RegisterEditorG2Monitor();
            EditorSceneManager.OpenScene(SafeGrowthPlayerEvidencePlan.StageScenePath,
                OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogError("[SafeGrowthPlayerEvidenceBuildDriver] " + exception);
            ClearEditorG2Session();
            EditorApplication.Exit(1);
        }
    }

    [InitializeOnLoadMethod]
    private static void ResumeEditorG2AfterDomainReload()
    {
        if (SessionState.GetBool(EditorG2ActiveKey, false)) RegisterEditorG2Monitor();
    }

    private static void RegisterEditorG2Monitor()
    {
        EditorApplication.update -= MonitorEditorG2;
        EditorApplication.update += MonitorEditorG2;
    }

    private static void MonitorEditorG2()
    {
        if (!SessionState.GetBool(EditorG2ActiveKey, false))
        {
            EditorApplication.update -= MonitorEditorG2;
            return;
        }
        try
        {
            string deadlineText = SessionState.GetString(EditorG2DeadlineKey, string.Empty);
            if (!long.TryParse(deadlineText, out long deadline)
                || DateTime.UtcNow.Ticks > deadline)
                throw new TimeoutException("SAFE_EVIDENCE_EDITOR_G2_TIMEOUT");
            string root = SessionState.GetString(EditorG2RootKey, string.Empty);
            string marker = Path.Combine(root, "editor-g2.complete.json");
            if (!File.Exists(marker)) return;
            CompletionRecord record = JsonUtility.FromJson<CompletionRecord>(File.ReadAllText(marker));
            SafeGrowthPlayerEvidencePlan plan = SafeGrowthPlayerEvidencePlan.CreateCanonical();
            string lane = Path.Combine(root, "editorg2");
            int pngCount = Directory.Exists(lane) ? Directory.GetFiles(lane, "*.png").Length : 0;
            int stateCount = Directory.Exists(lane) ? Directory.GetFiles(lane, "*.state.json").Length : 0;
            if (record == null || record.count != plan.EditorG2Count
                || !string.Equals(record.planSha, plan.Sha256, StringComparison.Ordinal)
                || record.caseIds == null || record.caseIds.Length != plan.EditorG2Count
                || pngCount != plan.EditorG2Count || stateCount != plan.EditorG2Count)
                throw new InvalidDataException("SAFE_EVIDENCE_EDITOR_G2_COMPLETION_INVALID");
            ClearEditorG2Session();
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogError("[SafeGrowthPlayerEvidenceBuildDriver] " + exception);
            ClearEditorG2Session();
            EditorApplication.Exit(2);
        }
    }

    private static void ClearEditorG2Session()
    {
        EditorApplication.update -= MonitorEditorG2;
        SessionState.EraseBool(EditorG2ActiveKey);
        SessionState.EraseString(EditorG2RootKey);
        SessionState.EraseString(EditorG2DeadlineKey);
    }

    [Serializable]
    private sealed class BuildReceipt
    {
        public string result;
        public string outputPath;
        public ulong totalSize;
        public int totalErrors;
        public int totalWarnings;
        public string planSha;
    }

    [Serializable]
    private sealed class CompletionRecord
    {
        public string planSha;
        public int count;
        public string[] caseIds;
    }
}
