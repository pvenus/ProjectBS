using UnityEngine;

[DisallowMultipleComponent]
public sealed class SkillAnimationVfxControllerMono : MonoBehaviour
{
    private static readonly int TintColor = Shader.PropertyToID("_VfxTintColor");
    private static readonly int TintStrength = Shader.PropertyToID("_VfxTintStrength");
    private static readonly int ColorA = Shader.PropertyToID("_VfxColorA");
    private static readonly int ColorB = Shader.PropertyToID("_VfxColorB");
    private static readonly int ColorPhase = Shader.PropertyToID("_VfxColorPhase");
    private static readonly int ColorShiftStrength = Shader.PropertyToID("_VfxColorShiftStrength");
    private static readonly int EmissionColor = Shader.PropertyToID("_VfxEmissionColor");
    private static readonly int EmissionIntensity = Shader.PropertyToID("_VfxEmissionIntensity");
    private static readonly int GlobalAlpha = Shader.PropertyToID("_VfxGlobalAlpha");
    private static readonly int Desaturate = Shader.PropertyToID("_VfxDesaturate");
    private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    private static readonly int FlowSpeedX = Shader.PropertyToID("_FlowSpeedX");
    private static readonly int FlowSpeedY = Shader.PropertyToID("_FlowSpeedY");
    private static readonly int RimColor = Shader.PropertyToID("_VfxRimColor");
    private static readonly int SpatialPattern = Shader.PropertyToID("_VfxSpatialPattern");
    private static readonly int RimPulseStrength = Shader.PropertyToID("_VfxRimPulseStrength");
    private static readonly int RimPulsePhase = Shader.PropertyToID("_VfxRimPulsePhase");
    private static readonly int SweepColor = Shader.PropertyToID("_VfxSweepColor");
    private static readonly int SweepDirection = Shader.PropertyToID("_VfxSweepDirection");
    private static readonly int SweepStrength = Shader.PropertyToID("_VfxSweepStrength");
    private static readonly int SweepWidth = Shader.PropertyToID("_VfxSweepWidth");
    private static readonly int SweepSoftness = Shader.PropertyToID("_VfxSweepSoftness");
    private static readonly int SweepPhase = Shader.PropertyToID("_VfxSweepPhase");
    private static readonly int GlintColor = Shader.PropertyToID("_VfxGlintColor");
    private static readonly int GlintStrength = Shader.PropertyToID("_VfxGlintStrength");
    private static readonly int GlintWidth = Shader.PropertyToID("_VfxGlintWidth");
    private static readonly int ImpactColor = Shader.PropertyToID("_VfxImpactColor");
    private static readonly int ImpactPeak = Shader.PropertyToID("_VfxImpactPeak");
    private static readonly int AfterglowColor = Shader.PropertyToID("_VfxAfterglowColor");
    private static readonly int Afterglow = Shader.PropertyToID("_VfxAfterglow");
    private static readonly int InkDensity = Shader.PropertyToID("_VfxInkDensity");
    private static readonly int InkBreakup = Shader.PropertyToID("_VfxInkBreakup");
    private static readonly int InkScale = Shader.PropertyToID("_VfxInkScale");
    private static readonly int InkFlow = Shader.PropertyToID("_VfxInkFlow");
    private static readonly int NeonRimStrength = Shader.PropertyToID("_VfxNeonRimStrength");
    private static readonly int NeonPeakGain = Shader.PropertyToID("_VfxNeonPeakGain");
    private static readonly int NeonAfterglowGain = Shader.PropertyToID("_VfxNeonAfterglowGain");
    private static readonly int SignatureColor = Shader.PropertyToID("_VfxSignatureColor");
    private static readonly int AuxiliaryColor = Shader.PropertyToID("_VfxAuxiliaryColor");
    private static readonly int NeutralPeakColor = Shader.PropertyToID("_VfxNeutralPeakColor");
    private static readonly int SignatureCoverage = Shader.PropertyToID("_VfxSignatureCoverage");
    private static readonly int AuxiliaryEnvelope = Shader.PropertyToID("_VfxAuxiliaryEnvelope");
    private static readonly int NeutralPeakEnvelope = Shader.PropertyToID("_VfxNeutralPeakEnvelope");
    private static readonly int BodyOpacityGain = Shader.PropertyToID("_VfxBodyOpacityGain");
    private static readonly int LocalizedGlowAlpha = Shader.PropertyToID("_VfxLocalizedGlowAlpha");
    private static readonly int SpriteUvRect = Shader.PropertyToID("_VfxSpriteUvRect");
    private static readonly int GroundFieldMode = Shader.PropertyToID("_VfxGroundFieldMode");
    private static readonly int FieldPhase = Shader.PropertyToID("_VfxFieldPhase");
    private static readonly int FieldAspect = Shader.PropertyToID("_VfxFieldAspect");
    private static readonly int FieldEdge = Shader.PropertyToID("_VfxFieldEdge");
    private static readonly int FieldInk = Shader.PropertyToID("_VfxFieldInk");
    private static readonly int FieldSupport = Shader.PropertyToID("_VfxFieldSupport");
    private static readonly int FieldScars = Shader.PropertyToID("_VfxFieldScars");
    private static readonly int FieldSeed = Shader.PropertyToID("_VfxFieldSeed");

    private SpriteRenderer target;
    private MaterialPropertyBlock block;

    public void Bind(SpriteRenderer renderer)
    {
        if (target != renderer) ResetState();
        target = renderer;
        block ??= new MaterialPropertyBlock();
    }

    public void Apply(SkillAnimationVfxProfileSO profile, SkillAnimationVfxPaletteBinding palette,
        float phase01, float alpha01,
        float rimPulse01 = 0f, float impact01 = 0f, float afterglow01 = 0f)
    {
        if (target == null || profile == null) return;
        block ??= new MaterialPropertyBlock();
        target.GetPropertyBlock(block);
        block.SetColor(TintColor, profile.TintColor);
        block.SetFloat(TintStrength, profile.TintStrength);
        block.SetColor(ColorA, profile.ColorA);
        block.SetColor(ColorB, profile.ColorB);
        block.SetFloat(ColorPhase, Mathf.Repeat(phase01, 1f));
        block.SetFloat(ColorShiftStrength, profile.ColorShiftStrength);
        block.SetColor(EmissionColor, profile.EmissionColor);
        block.SetFloat(EmissionIntensity, profile.EmissionIntensity);
        block.SetFloat(GlobalAlpha, Mathf.Clamp01(alpha01));
        block.SetFloat(Desaturate, profile.Desaturate);
        block.SetColor(OutlineColor, profile.OutlineColor);
        block.SetFloat(OutlineWidth, profile.OutlineWidth);
        block.SetFloat(FlowSpeedX, profile.FlowSpeed.x);
        block.SetFloat(FlowSpeedY, profile.FlowSpeed.y);
        block.SetColor(RimColor, profile.RimColor);
        block.SetFloat(SpatialPattern, (float)profile.SpatialPattern);
        block.SetFloat(RimPulseStrength, profile.RimPulseStrength);
        block.SetFloat(RimPulsePhase, Mathf.Clamp01(rimPulse01));
        block.SetColor(SweepColor, profile.SweepColor);
        block.SetVector(SweepDirection, profile.SweepDirection);
        block.SetFloat(SweepStrength, profile.SweepStrength);
        block.SetFloat(SweepWidth, profile.SweepWidth);
        block.SetFloat(SweepSoftness, profile.SweepSoftness);
        block.SetFloat(SweepPhase, Mathf.Repeat(phase01, 1f));
        block.SetColor(GlintColor, profile.GlintColor);
        block.SetFloat(GlintStrength, profile.GlintStrength);
        block.SetFloat(GlintWidth, profile.GlintWidth);
        block.SetColor(ImpactColor, profile.PeakColor);
        block.SetFloat(ImpactPeak, Mathf.Clamp01(impact01) * profile.PeakIntensity);
        block.SetColor(AfterglowColor, profile.AfterglowColor);
        block.SetFloat(Afterglow, Mathf.Clamp01(afterglow01) * profile.AfterglowIntensity);
        block.SetFloat(InkDensity, profile.InkDensity);
        block.SetFloat(InkBreakup, profile.InkBreakup);
        block.SetFloat(InkScale, profile.InkScale);
        block.SetVector(InkFlow, profile.InkFlow);
        block.SetFloat(NeonRimStrength, profile.NeonRimStrength);
        block.SetFloat(NeonPeakGain, profile.NeonPeakGain);
        block.SetFloat(NeonAfterglowGain, profile.NeonAfterglowGain);
        Color signature = palette != null ? palette.SignatureColor : profile.ColorA;
        Color auxiliary = palette != null ? palette.AuxiliaryColor : profile.ColorB;
        Color neutral = palette != null ? palette.NeutralPeakColor : profile.PeakColor;
        float signatureCoverage = palette != null ? palette.SignatureHoldCoverage : .8f;
        float auxiliaryEnvelope = palette != null
            ? Mathf.Lerp(palette.AuxiliaryHoldCoverage, palette.AuxiliaryPeakCoverage,
                Mathf.Clamp01(impact01))
            : .18f;
        float neutralEnvelope = palette != null
            ? palette.NeutralPeakCoverage * Mathf.Clamp01(impact01)
            : .04f * Mathf.Clamp01(impact01);
        block.SetColor(SignatureColor, signature);
        block.SetColor(AuxiliaryColor, auxiliary);
        block.SetColor(NeutralPeakColor, neutral);
        block.SetFloat(SignatureCoverage, Mathf.Clamp(signatureCoverage, .7f, .9f));
        block.SetFloat(AuxiliaryEnvelope, Mathf.Clamp(auxiliaryEnvelope, 0f, .3f));
        block.SetFloat(NeutralPeakEnvelope, Mathf.Clamp(neutralEnvelope, 0f, .06f));
        block.SetFloat(BodyOpacityGain, Mathf.Clamp01(profile.BodyOpacityGain));
        block.SetFloat(LocalizedGlowAlpha, Mathf.Clamp(profile.LocalizedGlowAlpha, 0f, .2f));
        block.SetVector(SpriteUvRect, ResolveSpriteUvRect(target));
        block.SetFloat(GroundFieldMode, profile.ProceduralGroundField ? 1f : 0f);
        block.SetFloat(FieldPhase, Mathf.Repeat(phase01, 1f));
        block.SetVector(FieldAspect, profile.FootprintAspect);
        block.SetVector(FieldEdge, new Vector4(profile.EdgeSoftness, profile.EdgeIrregularity, 0f, 0f));
        block.SetVector(FieldInk, new Vector4(profile.FieldInkDensity, .85f, 3.4f, .18f));
        block.SetVector(FieldSupport, new Vector4(profile.SupportContrast, profile.SupportLobes, 0f, 0f));
        block.SetVector(FieldScars, new Vector4(profile.PressureScarCount, profile.RustCoverage, 0f, 0f));
        block.SetFloat(FieldSeed, profile.DeterministicSeed & 0x00ffffffu);
        target.SetPropertyBlock(block);
    }

    public void Apply(SkillAnimationVfxProfileSO profile, float phase01, float alpha01,
        float rimPulse01 = 0f, float impact01 = 0f, float afterglow01 = 0f)
    {
        Apply(profile, null, phase01, alpha01, rimPulse01, impact01, afterglow01);
    }

    public void ResetState()
    {
        if (target == null) return;
        block ??= new MaterialPropertyBlock();
        block.Clear();
        target.SetPropertyBlock(block);
    }

    private void OnDisable() => ResetState();
    private void OnDestroy() => ResetState();

    public static Vector4 ResolveSpriteUvRect(SpriteRenderer renderer)
    {
        Sprite sprite = renderer != null ? renderer.sprite : null;
        Texture2D texture = sprite != null ? sprite.texture : null;
        if (sprite == null || texture == null || texture.width <= 0 || texture.height <= 0)
            return new Vector4(0f, 0f, 1f, 1f);

        Rect rect = sprite.textureRect;
        float halfX = .5f / texture.width;
        float halfY = .5f / texture.height;
        return new Vector4(
            Mathf.Clamp01(rect.xMin / texture.width + halfX),
            Mathf.Clamp01(rect.yMin / texture.height + halfY),
            Mathf.Clamp01(rect.xMax / texture.width - halfX),
            Mathf.Clamp01(rect.yMax / texture.height - halfY));
    }
}

public static class SkillAnimationVfxMaterialAuthority
{
    public const string ShaderName = "Custom/SkillAnimationVfx";
    private static Material runtimeFallback;

    public static bool Ensure(SpriteRenderer renderer, Material preferred = null)
    {
        if (renderer == null) return false;
        Material current = renderer.sharedMaterial;
        if (preferred != null)
        {
            renderer.sharedMaterial = preferred;
            return preferred.shader != null && preferred.shader.name == ShaderName;
        }
        if (current != null && current.shader != null
            && current.shader.name != "Sprites/Default")
        {
            return current.shader.name == ShaderName;
        }
        runtimeFallback ??= CreateRuntimeFallback();
        if (runtimeFallback == null) return false;
        renderer.sharedMaterial = runtimeFallback;
        return true;
    }

    private static Material CreateRuntimeFallback()
    {
        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            Debug.LogError($"[SkillAnimationVfx] Required shader not found: {ShaderName}");
            return null;
        }
        return new Material(shader)
        {
            name = "SkillAnimationVfx_RuntimeFallback",
            hideFlags = HideFlags.HideAndDontSave
        };
    }
}
