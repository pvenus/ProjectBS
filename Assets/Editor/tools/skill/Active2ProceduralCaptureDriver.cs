using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Active2ProceduralCaptureDriver
{
    private const int NativeSize = 768;
    private static readonly float[] Phases = { 0f, .2f, .4f, .6f, .8f };
    private static readonly string ProjectRoot = Directory.GetParent(Application.dataPath).FullName;
    private static readonly string OutputRoot = Path.Combine(ProjectRoot,
        "Artifacts/GraphicsRemediation/Active2ProceduralCapture/review-01");

    [MenuItem("BS/Evidence/Capture Active2 Procedural GIF8 Sources")]
    public static void Capture()
    {
        string session = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string output = Path.Combine(OutputRoot, session);
        Directory.CreateDirectory(output);
        var manifest = new StringBuilder();
        manifest.AppendLine("status=REVIEW_EVIDENCE_ONLY_RUNTIME_PASS_NOT_INFERRED");
        manifest.AppendLine("captureUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
        manifest.AppendLine("nativeSize=768x768");
        manifest.AppendLine("phases=0,.2,.4,.6,.8");

        GameObject root = null;
        GameObject cameraObject = null;
        RenderTexture target = null;
        Texture2D readback = null;
        RenderTexture previousActive = RenderTexture.active;
        try
        {
            root = new GameObject("Active2ProceduralCapture_Temporary") { hideFlags = HideFlags.HideAndDontSave };
            ProjectileVisual visual = root.AddComponent<ProjectileVisual>();
            cameraObject = new GameObject("Active2ProceduralCaptureCamera_Temporary") { hideFlags = HideFlags.HideAndDontSave };
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.42f, .43f, .42f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            target = new RenderTexture(NativeSize, NativeSize, 24, RenderTextureFormat.ARGB32)
            {
                name = "Active2ProceduralCaptureRT",
                hideFlags = HideFlags.HideAndDontSave
            };
            target.Create();
            camera.targetTexture = target;
            readback = new Texture2D(NativeSize, NativeSize, TextureFormat.RGBA32, false, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            CaptureGrade(visual, camera, readback, output, 2,
                "Assets/Contents/Skill/so/skill.character.seojin.2.active_2.crane_wing_formation.visual.asset",
                "skill.character.seojin.2.active_2.crane_wing_formation.visual", manifest);
            CaptureGrade(visual, camera, readback, output, 3,
                "Assets/Contents/Skill/so/skill.character.seojin.3.active_2.crane_wing_formation.visual.asset",
                "skill.character.seojin.3.active_2.crane_wing_formation.visual", manifest);
            File.WriteAllText(Path.Combine(output, "capture-manifest.txt"), manifest.ToString(), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(OutputRoot, "latest.txt"), output, new UTF8Encoding(false));
            Debug.Log("[Active2Capture] COMPLETE " + output);
        }
        catch (Exception exception)
        {
            File.WriteAllText(Path.Combine(output, "capture-error.txt"), exception.ToString(), new UTF8Encoding(false));
            Debug.LogException(exception);
            throw;
        }
        finally
        {
            RenderTexture.active = previousActive;
            if (root != null)
            {
                ProjectileVisual visual = root.GetComponent<ProjectileVisual>();
                visual?.EditorStopProceduralCapture();
            }
            if (target != null) target.Release();
            if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
            if (target != null) UnityEngine.Object.DestroyImmediate(target);
            if (cameraObject != null) UnityEngine.Object.DestroyImmediate(cameraObject);
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void CaptureGrade(ProjectileVisual visual, Camera camera, Texture2D readback,
        string output, int grade, string assetPath, string visualId, StringBuilder manifest)
    {
        BaseVisualSO baseVisual = AssetDatabase.LoadAssetAtPath<BaseVisualSO>(assetPath);
        if (baseVisual == null || !visual.EditorInitializeProceduralCapture(baseVisual))
            throw new InvalidOperationException("Active2 capture binding failed: " + assetPath);
        SkillAnimationVfxProfileSO profile = baseVisual.AnimationVfxProfile;
        manifest.AppendLine($"grade{grade}.visualId={visualId}");
        manifest.AppendLine($"grade{grade}.assetPath={assetPath}");
        manifest.AppendLine($"grade{grade}.profileId={profile.ProfileId}");
        manifest.AppendLine($"grade{grade}.profileGuid={AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(profile))}");
        manifest.AppendLine($"grade{grade}.seed=0x{profile.DeterministicSeed:X8}");
        manifest.AppendLine($"grade{grade}.radius={profile.FieldRadiusWorld.ToString("0.###", CultureInfo.InvariantCulture)}");
        foreach (string view in new[] { "topdown-neutral", "gameplay-angle" })
        {
            ConfigureCamera(camera, view);
            manifest.AppendLine($"grade{grade}.{view}.cameraPosition={Format(camera.transform.position)}");
            manifest.AppendLine($"grade{grade}.{view}.cameraRotation={Format(camera.transform.eulerAngles)}");
            for (int i = 0; i < Phases.Length; i++)
            {
                visual.EditorApplyProceduralCapturePhase(Phases[i]);
                camera.Render();
                RenderTexture.active = camera.targetTexture;
                readback.ReadPixels(new Rect(0, 0, NativeSize, NativeSize), 0, 0, false);
                readback.Apply(false, false);
                byte[] png = readback.EncodeToPNG();
                string name = $"g{grade}-{view}-t{i}-{Phases[i]:0.00}-native.png";
                string path = Path.Combine(output, name);
                File.WriteAllBytes(path, png);
                manifest.AppendLine(name + ".sha256=" + Sha256(png));
            }
        }
        visual.EditorStopProceduralCapture();
    }

    private static void ConfigureCamera(Camera camera, string view)
    {
        if (view == "topdown-neutral") camera.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
        else
        {
            camera.transform.position = new Vector3(0f, -5.5f, -10f);
            camera.transform.LookAt(Vector3.zero, Vector3.up);
        }
    }

    private static string Format(Vector3 value) => string.Format(CultureInfo.InvariantCulture,
        "{0:0.###},{1:0.###},{2:0.###}", value.x, value.y, value.z);

    private static string Sha256(byte[] bytes)
    {
        using SHA256 sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
    }
}
