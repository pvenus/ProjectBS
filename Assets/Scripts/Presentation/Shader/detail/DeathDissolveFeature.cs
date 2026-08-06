using UnityEngine;

[System.Serializable]
public class DeathDissolveFeature
{
    [Header("Enabled")]
    public bool enabled = true;

    [Header("Dissolve")]
    [Range(0f, 1f)] public float startProgress = 1f;
    [Range(0f, 1f)] public float endProgress = 0f;
    [Min(0.001f)] public float softness = 0.06f;
    [Min(0.001f)] public float edgeWidth = 0.1f;
    public Color edgeColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [Min(0f)] public float edgeIntensity = 1.4f;
    [Range(0f, 1f)] public float noiseStrength = 0.25f;
    [Min(0f)] public float driftY = 0.35f;

    [Header("Linked Rim / Pulse Fade")]
    public bool fadeRimAndPulse = true;
    public Color baseRimColor = Color.white;
    public float baseRimIntensity = 0f;
    public float baseRimPower = 3f;
    public float basePulseIntensity = 0f;
    public float basePulseSpeed = 2f;

    public Color deathRimColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public float deathRimIntensity = 0.6f;
    public float deathRimPower = 2.2f;
    public float deathPulseIntensity = 0.08f;
    public float deathPulseSpeed = 3f;

    [Header("Timing")]
    [Min(0.01f)] public float duration = 0.55f;
    public AnimationCurve progressCurve = null;
    public AnimationCurve fadeCurve = null;

    [Header("Behavior")]
    public bool allowRefreshWhilePlaying = true;

    [HideInInspector] public bool isPlaying;
    [HideInInspector] public float timer;

    public void EnsureDefaults()
    {
        duration = Mathf.Max(0.01f, duration);
        softness = Mathf.Max(0.001f, softness);
        edgeWidth = Mathf.Max(0.001f, edgeWidth);
        edgeIntensity = Mathf.Max(0f, edgeIntensity);
        noiseStrength = Mathf.Clamp01(noiseStrength);
        driftY = Mathf.Max(0f, driftY);
        startProgress = Mathf.Clamp01(startProgress);
        endProgress = Mathf.Clamp01(endProgress);
        baseRimPower = Mathf.Max(0f, baseRimPower);
        deathRimPower = Mathf.Max(0f, deathRimPower);

        if (progressCurve == null || progressCurve.length == 0)
            progressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        if (fadeCurve == null || fadeCurve.length == 0)
            fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    public void Play()
    {
        if (!enabled)
            return;

        if (isPlaying && !allowRefreshWhilePlaying)
            return;

        EnsureDefaults();
        isPlaying = true;
        timer = 0f;
    }

    public void StopImmediate()
    {
        isPlaying = false;
        timer = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (!isPlaying)
            return;

        timer += Mathf.Max(0f, deltaTime);
        if (timer >= duration)
            StopImmediate();
    }

    public float EvaluateTime01()
    {
        if (!isPlaying)
            return 1f;

        return duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
    }

    public float EvaluateDeathProgress()
    {
        float t = EvaluateTime01();
        float curved = progressCurve != null ? progressCurve.Evaluate(t) : t;
        return Mathf.Lerp(startProgress, endProgress, curved);
    }

    public float EvaluateFade01()
    {
        float t = EvaluateTime01();
        return fadeCurve != null ? Mathf.Clamp01(fadeCurve.Evaluate(t)) : (1f - t);
    }
}