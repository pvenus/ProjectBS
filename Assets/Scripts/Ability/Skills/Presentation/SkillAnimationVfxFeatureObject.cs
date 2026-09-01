using UnityEngine;

[DisallowMultipleComponent]
public sealed class SkillAnimationVfxFeatureObject : MonoBehaviour
{
    public static bool ReducedMotion { get; set; }
    private SkillAnimationVfxControllerMono controller;
    private SkillAnimationVfxProfileSO profile;
    private SkillAnimationVfxPaletteBinding palette;
    private float elapsed;
    private bool playing;
    private SpriteRenderer groundRenderer;
    private static Texture2D carrierTexture;
    private static Sprite carrierSprite;

    public void Initialize(SpriteRenderer renderer, SkillAnimationVfxProfileSO value,
        SkillAnimationVfxPaletteBinding paletteOverride = null)
    {
        controller ??= GetComponent<SkillAnimationVfxControllerMono>() ?? gameObject.AddComponent<SkillAnimationVfxControllerMono>();
        profile = value;
        palette = paletteOverride;
        controller.Bind(profile != null && profile.ProceduralGroundField
            ? EnsureGroundRenderer(renderer)
            : renderer);
        StopImmediate();
    }

    public void Play()
    {
        if (profile == null) return;
        elapsed = 0f;
        playing = true;
        if (groundRenderer != null) groundRenderer.enabled = profile.ProceduralGroundField;
        controller.Apply(profile, palette, 0f, profile.ProceduralGroundField ? 1f : 0f);
    }

    public void StopImmediate()
    {
        playing = false;
        elapsed = 0f;
        controller?.ResetState();
        if (groundRenderer != null) groundRenderer.enabled = false;
    }

    public void SetSortingOrder(int value)
    {
        if (groundRenderer != null) groundRenderer.sortingOrder = value;
    }

    public bool RestartsOnHit => profile != null && !profile.ProceduralGroundField;

#if UNITY_EDITOR
    public void EditorApplyCapturePhase(float phase01)
    {
        if (profile == null || controller == null) return;
        if (groundRenderer != null) groundRenderer.enabled = profile.ProceduralGroundField;
        controller.Apply(profile, palette, Mathf.Repeat(phase01, 1f), 1f);
    }
#endif

    private void Update()
    {
        if (!playing || profile == null) return;
        elapsed += Time.deltaTime;
        if (profile.ProceduralGroundField)
        {
            float phase = ReducedMotion ? 0f : Mathf.Repeat(elapsed, 1f);
            controller.Apply(profile, palette, phase, 1f);
            return;
        }
        float fadeIn = Mathf.Max(.01f, profile.FadeIn);
        float fadeOutStart = fadeIn + Mathf.Max(0f, profile.Hold);
        float end = fadeOutStart + Mathf.Max(.01f, profile.FadeOut);
        float alpha = elapsed < fadeIn ? elapsed / fadeIn :
            elapsed < fadeOutStart ? 1f : 1f - ((elapsed - fadeOutStart) / Mathf.Max(.01f, profile.FadeOut));
        float peakCenter = fadeIn;
        float peakDuration = palette != null ? palette.NeutralPeakDuration : profile.PeakDuration;
        float peak = 1f - Mathf.Clamp01(Mathf.Abs(elapsed - peakCenter) / Mathf.Max(.01f, peakDuration));
        float afterglow = elapsed < fadeOutStart ? 0f :
            1f - Mathf.Clamp01((elapsed - fadeOutStart) / Mathf.Max(.01f, profile.AfterglowDuration));
        float rimPulse = Mathf.Sin(Mathf.Clamp01(elapsed / fadeIn) * Mathf.PI);
        controller.Apply(profile, palette, elapsed * profile.PhaseSpeed, alpha, rimPulse, peak, afterglow);
        if (elapsed >= end) StopImmediate();
    }

    private void OnDisable() => StopImmediate();
    private void OnDestroy() => StopImmediate();

    private SpriteRenderer EnsureGroundRenderer(SpriteRenderer reference)
    {
        if (groundRenderer == null)
        {
            GameObject go = new GameObject("SkillAnimationVfxGroundField");
            go.transform.SetParent(transform, false);
            groundRenderer = go.AddComponent<SpriteRenderer>();
        }
        groundRenderer.sprite = ResolveCarrierSprite();
        groundRenderer.color = Color.white;
        SkillAnimationVfxMaterialAuthority.Ensure(groundRenderer, profile.Material);
        groundRenderer.sortingLayerID = reference != null ? reference.sortingLayerID : 0;
        groundRenderer.sortingOrder = reference != null ? reference.sortingOrder : 0;
        Vector2 aspect = profile.FootprintAspect;
        float diameter = profile.FieldRadiusWorld * 2f;
        groundRenderer.transform.localPosition = Vector3.zero;
        groundRenderer.transform.localRotation = Quaternion.identity;
        groundRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);
        return groundRenderer;
    }

    private static Sprite ResolveCarrierSprite()
    {
        if (carrierSprite != null) return carrierSprite;
        carrierTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "SkillAnimationVfxGroundCarrier",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        carrierTexture.SetPixel(0, 0, Color.white);
        carrierTexture.Apply(false, true);
        carrierSprite = Sprite.Create(carrierTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(.5f, .5f), 1f);
        carrierSprite.name = "SkillAnimationVfxGroundCarrier";
        carrierSprite.hideFlags = HideFlags.HideAndDontSave;
        return carrierSprite;
    }
}
