using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using NUnit.Framework;
using Stage;
using UnityEditor;

namespace Progression.EditModeTests
{
    public sealed class SafeGrowthPlayerEvidenceBuildDriverTests
    {
        private const string Root = "/private/tmp/projectbs-safe-mac-player.fixture";

        [Test] public void OptionsUseOnlyExplicitStageScene()
        { string[] before = ScenePaths();
          var o = SafeGrowthPlayerEvidenceBuildDriver.CreateOptions(Root);
          string[] after = ScenePaths();
          Assert.That(o.scenes, Is.EqualTo(new[] { SafeGrowthPlayerEvidencePlan.StageScenePath }));
          Assert.That(after, Is.EqualTo(before)); }

        [Test] public void OptionsAreMacStandaloneDevelopmentOnly()
        { var o = SafeGrowthPlayerEvidenceBuildDriver.CreateOptions(Root);
          Assert.That(o.target, Is.EqualTo(BuildTarget.StandaloneOSX));
          Assert.That(o.targetGroup, Is.EqualTo(BuildTargetGroup.Standalone));
          Assert.That(o.options, Is.EqualTo(BuildOptions.Development)); }

        [Test] public void OptionsStayInsideApprovedTemporaryRoot()
        { var o = SafeGrowthPlayerEvidenceBuildDriver.CreateOptions(Root);
          Assert.That(Path.GetFullPath(o.locationPathName).StartsWith(Root), Is.True); }

        [Test] public void InvalidOutputDoesNotProduceBuildOptions()
        { Assert.That(() => SafeGrowthPlayerEvidenceBuildDriver.CreateOptions("Assets/build"),
            Throws.ArgumentException);
          System.Action entrypoint = SafeGrowthPlayerEvidenceBuildDriver.RunFromCommandLine;
          Assert.That(entrypoint, Is.Not.Null); }

        [Test] public void ImporterMetaIsReadOnlyBaselineOnly()
        { using SHA256 sha = SHA256.Create();
          StringBuilder actual = new();
          foreach (byte value in sha.ComputeHash(File.ReadAllBytes(
              SafeGrowthPlayerEvidencePlan.ImporterMetaPath))) actual.Append(value.ToString("x2"));
          Assert.That(actual.ToString(), Is.EqualTo("2d067ec7b8b8b59870566708df6ea13eb516fce4906b42248e329d56a5fbeafe")); }

        [Test] public void EditorG2EntryPointIsPublicStaticAndParameterless()
        { MethodInfo method = typeof(SafeGrowthPlayerEvidenceBuildDriver).GetMethod(
              "RunEditorG2", BindingFlags.Public | BindingFlags.Static);
          Assert.That(method, Is.Not.Null);
          Assert.That(method.ReturnType, Is.EqualTo(typeof(void)));
          Assert.That(method.GetParameters(), Is.Empty); }

        private static string[] ScenePaths()
        { var scenes = EditorBuildSettings.scenes; string[] paths = new string[scenes.Length];
          for (int i = 0; i < scenes.Length; i++) paths[i] = scenes[i].path; return paths; }
    }
}
