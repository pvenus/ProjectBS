#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace ResourceTools.Character
{
    public static class CharacterAnimationV2FocusedTestRunner
    {
        private const string ReportPath =
            "/private/tmp/projectbs-character-animation-v2-focused-editmode.txt";

        private static TestRunnerApi api;
        private static Callback callback;

        [MenuItem("Assets/Resource Tools/Character Animation v2/Run Focused EditMode Tests", false, 2101)]
        public static void Run()
        {
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            callback = new Callback();
            api.RegisterCallbacks(callback);
            Filter filter = new Filter
            {
                testMode = TestMode.EditMode,
                testNames = new[]
                {
                    "CharacterAnimationSpecV2ContractTests",
                    "SkillSpecificBodyActionContractTests",
                    "CharacterLocomotionPlaybackRateTests",
                    "Episode1Npc2AnimationCastingContractTests",
                    "SkillCastingPhaseContractTests"
                }
            };
            ExecutionSettings settings = new ExecutionSettings(filter)
            {
                runSynchronously = true
            };
            api.Execute(settings);
        }

        private sealed class Callback : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }

            public void RunFinished(ITestResultAdaptor result)
            {
                string report = $"status={result.TestStatus}\npass={result.PassCount}\n" +
                    $"fail={result.FailCount}\nskip={result.SkipCount}\ninconclusive={result.InconclusiveCount}\n" +
                    $"duration={result.Duration:F3}\nmessage={result.Message}\n";
                File.WriteAllText(ReportPath, report);
                Debug.Log("[CharacterAnimationV2] FOCUSED_EDITMODE_COMPLETE " +
                    $"Status={result.TestStatus}, Pass={result.PassCount}, Fail={result.FailCount}, " +
                    $"Skip={result.SkipCount}, Report={ReportPath}");
                if (api != null) api.UnregisterCallbacks(this);
                if (api != null) Object.DestroyImmediate(api);
                api = null;
                callback = null;
            }
        }
    }
}
#endif
