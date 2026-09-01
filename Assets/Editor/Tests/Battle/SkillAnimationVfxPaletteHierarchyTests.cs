using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class SkillAnimationVfxPaletteHierarchyTests
{
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
    private static string PalettePath => Path.Combine(Application.dataPath,
        "Contents/Skill/vfx/skill-vfx-palette-authority96.tsv");

    [Test]
    public void PaletteAuthority_ExactlyJoinsRoot95AndTypedChild1()
    {
        string[] rows = File.ReadAllLines(PalettePath).Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
        Assert.That(rows.Length, Is.EqualTo(96));
        Assert.That(rows.Select(line => line.Split('\t')[0]).Distinct().Count(), Is.EqualTo(96));
        Assert.That(rows.Count(line => line.Split('\t')[2] == "root"), Is.EqualTo(95));
        Assert.That(rows.Count(line => line.Split('\t')[2] == "typed_child"), Is.EqualTo(1));

        foreach (string row in rows)
        {
            string id = row.Split('\t')[0];
            string asset = Path.Combine(Application.dataPath, "Contents/Skill/so", id + ".asset");
            Assert.That(File.Exists(asset), Is.True, id);
            string yaml = File.ReadAllText(asset);
            Assert.That(yaml, Does.Contain("visualId: " + id));
            Assert.That(yaml, Does.Contain("animationVfxPalette:"), id);
        }
    }

    [Test]
    public void PaletteBindings_RespectStaticHierarchyBounds()
    {
        foreach (string asset in Directory.GetFiles(
                     Path.Combine(Application.dataPath, "Contents/Skill/so"), "*.asset"))
        {
            string yaml = File.ReadAllText(asset);
            if (!yaml.Contains("animationVfxPalette:")) continue;
            float signature = Scalar(yaml, "signatureHoldCoverage");
            float auxHold = Scalar(yaml, "auxiliaryHoldCoverage");
            float auxPeak = Scalar(yaml, "auxiliaryPeakCoverage");
            float neutral = Scalar(yaml, "neutralPeakCoverage");
            float duration = Scalar(yaml, "neutralPeakDuration");
            Assert.That(signature, Is.InRange(.7f, .9f), asset);
            Assert.That(auxHold, Is.InRange(0f, .2f), asset);
            Assert.That(auxPeak, Is.InRange(0f, .3f), asset);
            Assert.That(neutral, Is.InRange(0f, .06f), asset);
            Assert.That(duration, Is.InRange(.01f, .05f), asset);
        }
    }

    [Test]
    public void SeojinCharge_UsesOnlyNavyRustAndNeutralPeak()
    {
        foreach (string grade in new[] { "1", "2", "3" })
        {
            string path = Path.Combine(Application.dataPath, "Contents/Skill/so",
                $"skill.character.seojin.{grade}.active_1.{(grade == "1" ? "active_1" : "charge")}.visual.asset");
            string yaml = File.ReadAllText(path);
            Assert.That(yaml, Does.Contain("paletteCode: SJ"));
            AssertColor(yaml, "signatureColor", "#294A68");
            AssertColor(yaml, "auxiliaryColor", "#C65743");
            Assert.That(yaml, Does.Not.Contain("colorPhase"));
        }
    }

    [Test]
    public void Shader_HasNoTemporalHueInterpolationPath()
    {
        string shader = File.ReadAllText(Path.Combine(Application.dataPath,
            "Shaders/SkillAnimationVfx.shader"));
        Assert.That(shader, Does.Contain("_VfxSignatureColor"));
        Assert.That(shader, Does.Contain("_VfxAuxiliaryEnvelope"));
        Assert.That(shader, Does.Contain("_VfxNeutralPeakEnvelope"));
        Assert.That(shader, Does.Not.Contain("lerp(_VfxColorA.rgb,_VfxColorB.rgb"));
    }

    [Test]
    public void ImpactProfile_LowAlphaBodyAndLocalizedGlowRemainReadableWithoutBreakingTerminalZero()
    {
        string profile = File.ReadAllText(Path.Combine(Application.dataPath,
            "Contents/Skill/vfx/vfx-impact-attack-seojin-validation.asset"));
        float bodyGain = Scalar(profile, "bodyOpacityGain");
        float glowGain = Scalar(profile, "localizedGlowAlpha");
        Assert.That(bodyGain, Is.InRange(0f, 1f));
        Assert.That(glowGain, Is.InRange(0f, .2f));

        const float sourceAlpha = .2f;
        float readable = Mathf.Lerp(sourceAlpha, Mathf.Sqrt(sourceAlpha), bodyGain);
        float support = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.01f, .32f, sourceAlpha));
        float localizedGlow = support * glowGain;
        Assert.That(readable, Is.GreaterThan(sourceAlpha));
        Assert.That(localizedGlow, Is.GreaterThan(0f));
        Assert.That(readable * 0f, Is.EqualTo(0f));

        string shader = File.ReadAllText(Path.Combine(Application.dataPath,
            "Shaders/SkillAnimationVfx.shader"));
        Assert.That(shader, Does.Contain("sqrt(saturate(s.a))"));
        Assert.That(shader, Does.Contain("sourceSupport*_VfxLocalizedGlowAlpha*globalAlpha"));
        Assert.That(shader, Does.Contain("_VfxSpriteUvRect"));
        Assert.That(shader, Does.Contain("clamp(i.uv+float2(t.x,0),_VfxSpriteUvRect.xy,_VfxSpriteUvRect.zw)"));
    }

    private static float Scalar(string yaml, string key)
    {
        string line = yaml.Split('\n').First(value => value.TrimStart().StartsWith(key + ":"));
        return float.Parse(line.Substring(line.IndexOf(':') + 1), CultureInfo.InvariantCulture);
    }

    private static void AssertColor(string yaml, string key, string hex)
    {
        string line = yaml.Split('\n').First(value => value.TrimStart().StartsWith(key + ":"));
        ColorUtility.TryParseHtmlString(hex, out Color expected);
        foreach (string pair in line.Substring(line.IndexOf('{') + 1).TrimEnd('}').Split(','))
        {
            string[] parts = pair.Trim().Split(':');
            if (parts[0] == "a") continue;
            float actual = float.Parse(parts[1], CultureInfo.InvariantCulture);
            float target = parts[0] == "r" ? expected.r : parts[0] == "g" ? expected.g : expected.b;
            Assert.That(actual, Is.EqualTo(target).Within(.00001f), key + " " + parts[0]);
        }
    }
}
