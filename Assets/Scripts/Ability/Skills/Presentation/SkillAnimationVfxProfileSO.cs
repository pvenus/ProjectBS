using UnityEngine;

public enum SkillAnimationVfxPreset
{
    ImpactAttack,
    ProjectileFlow,
    HealSupport,
    GuardControl
}

public enum SkillAnimationVfxSpatialPattern
{
    Directional,
    StaggeredLanes,
    RadialConvergence,
    BilateralGround
}

[System.Serializable]
public sealed class SkillAnimationVfxPaletteBinding
{
    [SerializeField] private string paletteCode;
    [SerializeField] private Color signatureColor = Color.white;
    [SerializeField] private Color auxiliaryColor = Color.black;
    [SerializeField] private Color neutralPeakColor = Color.white;
    [SerializeField, Range(.7f, .9f)] private float signatureHoldCoverage = .8f;
    [SerializeField, Range(0f, .2f)] private float auxiliaryHoldCoverage = .18f;
    [SerializeField, Range(0f, .3f)] private float auxiliaryPeakCoverage = .28f;
    [SerializeField, Range(0f, .06f)] private float neutralPeakCoverage = .04f;
    [SerializeField, Range(.01f, .05f)] private float neutralPeakDuration = .05f;

    public string PaletteCode => paletteCode;
    public Color SignatureColor => signatureColor;
    public Color AuxiliaryColor => auxiliaryColor;
    public Color NeutralPeakColor => neutralPeakColor;
    public float SignatureHoldCoverage => signatureHoldCoverage;
    public float AuxiliaryHoldCoverage => auxiliaryHoldCoverage;
    public float AuxiliaryPeakCoverage => auxiliaryPeakCoverage;
    public float NeutralPeakCoverage => neutralPeakCoverage;
    public float NeutralPeakDuration => neutralPeakDuration;
}

[CreateAssetMenu(fileName = "SkillAnimationVfxProfile", menuName = "BS/Skills/Visual/Skill Animation VFX Profile")]
public sealed class SkillAnimationVfxProfileSO : ScriptableObject
{
    [SerializeField] private string profileId;
    [SerializeField] private SkillAnimationVfxPreset preset;
    [SerializeField] private Material material;
    [SerializeField] private Color tintColor = Color.white;
    [SerializeField, Range(0f, .5f)] private float tintStrength = .22f;
    [SerializeField] private Color colorA = Color.white;
    [SerializeField] private Color colorB = Color.cyan;
    [SerializeField, Range(0f, .35f)] private float colorShiftStrength = .16f;
    [SerializeField] private Color emissionColor = Color.cyan;
    [SerializeField, Range(0f, 1.1f)] private float emissionIntensity = .38f;
    [SerializeField, Range(0f, .18f)] private float desaturate = .08f;
    [SerializeField] private Color outlineColor = new(0f, 0f, 0f, .78f);
    [SerializeField, Range(.75f, 2f)] private float outlineWidth = 1f;
    [SerializeField, Min(.01f)] private float fadeIn = .08f;
    [SerializeField, Min(0f)] private float hold = .08f;
    [SerializeField, Min(.01f)] private float fadeOut = .2f;
    [SerializeField, Range(0f, 3f)] private float phaseSpeed = 2f;
    [SerializeField] private Vector2 flowSpeed;
    [Header("Spatial highlight")]
    [SerializeField] private Color rimColor = Color.white;
    [SerializeField] private SkillAnimationVfxSpatialPattern spatialPattern;
    [SerializeField, Range(0f, .6f)] private float rimPulseStrength;
    [SerializeField] private Color sweepColor = Color.white;
    [SerializeField] private Vector2 sweepDirection = Vector2.right;
    [SerializeField, Range(0f, .4f)] private float sweepStrength;
    [SerializeField, Range(.02f, .2f)] private float sweepWidth = .1f;
    [SerializeField, Range(.01f, .12f)] private float sweepSoftness = .05f;
    [SerializeField] private Color glintColor = Color.white;
    [SerializeField, Range(0f, .3f)] private float glintStrength;
    [SerializeField, Range(.01f, .08f)] private float glintWidth = .03f;
    [Header("Event envelopes")]
    [SerializeField] private Color peakColor = Color.white;
    [SerializeField, Range(0f, 1f)] private float peakIntensity;
    [SerializeField, Min(.01f)] private float peakDuration = .05f;
    [SerializeField] private Color afterglowColor = Color.black;
    [SerializeField, Range(0f, .4f)] private float afterglowIntensity;
    [SerializeField, Min(0f)] private float afterglowDuration = .18f;
    [Header("Ink + controlled neon")]
    [SerializeField, Range(0f, .6f)] private float inkDensity = .3f;
    [SerializeField, Range(0f, 1f)] private float inkBreakup = .5f;
    [SerializeField, Range(4f, 64f)] private float inkScale = 24f;
    [SerializeField] private Vector2 inkFlow = Vector2.right;
    [SerializeField, Range(0f, 1f)] private float neonRimStrength = .5f;
    [SerializeField, Range(0f, 1.5f)] private float neonPeakGain = 1f;
    [SerializeField, Range(0f, 1f)] private float neonAfterglowGain = .5f;
    [SerializeField, Range(0f, 1f)] private float bodyOpacityGain;
    [SerializeField, Range(0f, .2f)] private float localizedGlowAlpha;
    [Header("Procedural ground field (default off)")]
    [SerializeField] private bool proceduralGroundField;
    [SerializeField, Min(.01f)] private float fieldRadiusWorld = 3f;
    [SerializeField] private Vector2 footprintAspect = new(1f, .62f);
    [SerializeField, Range(.01f, .4f)] private float edgeSoftness = .18f;
    [SerializeField, Range(0f, .3f)] private float edgeIrregularity = .1f;
    [SerializeField, Range(0f, 1f)] private float fieldInkDensity = .62f;
    [SerializeField, Range(0f, .3f)] private float supportContrast = .14f;
    [SerializeField, Range(2, 3)] private int supportLobes = 2;
    [SerializeField, Range(1, 4)] private int pressureScarCount = 3;
    [SerializeField, Range(0f, .04f)] private float rustCoverage = .025f;
    [SerializeField] private uint deterministicSeed = 0xA2C20002u;

    public string ProfileId => profileId;
    public SkillAnimationVfxPreset Preset => preset;
    public Material Material => material;
    public Color TintColor => tintColor;
    public float TintStrength => tintStrength;
    public Color ColorA => colorA;
    public Color ColorB => colorB;
    public float ColorShiftStrength => colorShiftStrength;
    public Color EmissionColor => emissionColor;
    public float EmissionIntensity => emissionIntensity;
    public float Desaturate => desaturate;
    public Color OutlineColor => outlineColor;
    public float OutlineWidth => outlineWidth;
    public float FadeIn => fadeIn;
    public float Hold => hold;
    public float FadeOut => fadeOut;
    public float PhaseSpeed => phaseSpeed;
    public Vector2 FlowSpeed => flowSpeed;
    public Color RimColor => rimColor;
    public SkillAnimationVfxSpatialPattern SpatialPattern => spatialPattern;
    public float RimPulseStrength => rimPulseStrength;
    public Color SweepColor => sweepColor;
    public Vector2 SweepDirection => sweepDirection;
    public float SweepStrength => sweepStrength;
    public float SweepWidth => sweepWidth;
    public float SweepSoftness => sweepSoftness;
    public Color GlintColor => glintColor;
    public float GlintStrength => glintStrength;
    public float GlintWidth => glintWidth;
    public Color PeakColor => peakColor;
    public float PeakIntensity => peakIntensity;
    public float PeakDuration => peakDuration;
    public Color AfterglowColor => afterglowColor;
    public float AfterglowIntensity => afterglowIntensity;
    public float AfterglowDuration => afterglowDuration;
    public float InkDensity => inkDensity;
    public float InkBreakup => inkBreakup;
    public float InkScale => inkScale;
    public Vector2 InkFlow => inkFlow;
    public float NeonRimStrength => neonRimStrength;
    public float NeonPeakGain => neonPeakGain;
    public float NeonAfterglowGain => neonAfterglowGain;
    public float BodyOpacityGain => bodyOpacityGain;
    public float LocalizedGlowAlpha => localizedGlowAlpha;
    public bool ProceduralGroundField => proceduralGroundField;
    public float FieldRadiusWorld => Mathf.Max(.01f, fieldRadiusWorld);
    public Vector2 FootprintAspect => new(Mathf.Max(.01f, footprintAspect.x), Mathf.Max(.01f, footprintAspect.y));
    public float EdgeSoftness => Mathf.Clamp(edgeSoftness, .01f, .4f);
    public float EdgeIrregularity => Mathf.Clamp(edgeIrregularity, 0f, .3f);
    public float FieldInkDensity => Mathf.Clamp01(fieldInkDensity);
    public float SupportContrast => Mathf.Clamp(supportContrast, 0f, .3f);
    public int SupportLobes => Mathf.Clamp(supportLobes, 2, 3);
    public int PressureScarCount => Mathf.Clamp(pressureScarCount, 1, 4);
    public float RustCoverage => Mathf.Clamp(rustCoverage, 0f, .04f);
    public uint DeterministicSeed => deterministicSeed;
}
