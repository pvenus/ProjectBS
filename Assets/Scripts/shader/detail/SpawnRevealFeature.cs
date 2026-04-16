using UnityEngine;

[System.Serializable]
public class SpawnRevealFeature
{
    [Header("Enabled")]
    public bool enabled = true;

    [Header("Reveal")]
    [Range(0f, 1f)] public float startProgress = 0f;
    [Range(0f, 1f)] public float endProgress = 1f;
    [Min(0.001f)] public float softness = 0.02f;
    [Min(0.001f)] public float edgeWidth = 0.1f;
    public Color edgeColor = new Color(0.6f, 0.9f, 1.0f, 1.0f);
    [Min(0f)] public float edgeIntensity = 2.8f;
    [Range(0f, 1f)] public float noiseStrength = 0.2f;

    [Header("Linked Rim / Pulse Boost")]
    public bool boostRimAndPulse = true;
    public Color baseRimColor = Color.white;
    public float baseRimIntensity = 0f;
    public float baseRimPower = 3f;
    public float basePulseIntensity = 0f;
    public float basePulseSpeed = 2f;

    public Color revealRimColor = new Color(0.8f, 0.95f, 1f, 1f);
    public float revealRimIntensity = 2.2f;
    public float revealRimPower = 2.5f;
    public float revealPulseIntensity = 0.42f;
    public float revealPulseSpeed = 6f;

    [Header("Timing")]
    [Min(0.01f)] public float duration = 1.0f;
    public AnimationCurve progressCurve = null;
    public AnimationCurve boostCurve = null;

    [Header("Behavior")]
    public bool resetToBaseWhenFinished = true;
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
        startProgress = Mathf.Clamp01(startProgress);
        endProgress = Mathf.Clamp01(endProgress);
        baseRimPower = Mathf.Max(0f, baseRimPower);
        revealRimPower = Mathf.Max(0f, revealRimPower);

        if (progressCurve == null || progressCurve.length == 0)
        {
            progressCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0.9f, 1.8f),
                new Keyframe(0.7f, 0.82f, 0.8f, 0.5f),
                new Keyframe(1f, 1f, 0.15f, 0f)
            );
        }

        if (boostCurve == null || boostCurve.length == 0)
        {
            boostCurve = new AnimationCurve(
                new Keyframe(0f, 1f, 0f, -1.2f),
                new Keyframe(0.35f, 0.92f, -0.5f, -0.8f),
                new Keyframe(0.75f, 0.3f, -1.0f, -0.3f),
                new Keyframe(1f, 0f, -0.1f, 0f)
            );
        }
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
        Debug.Log($"[SpawnRevealFeature] Play started / timer={timer} / duration={duration}");
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
        Debug.Log($"[SpawnRevealFeature] Tick / deltaTime={deltaTime:0.0000} / timer={timer:0.0000} / duration={duration:0.0000} / normalized={(duration > 0f ? timer / duration : 0f):0.0000}");

        if (timer >= duration)
        {
            Debug.Log("[SpawnRevealFeature] Tick reached duration -> StopImmediate()");
            StopImmediate();
        }
    }

    public float EvaluateTime01()
    {
        if (!isPlaying)
            return 1f;

        return duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
    }

    public float EvaluateRevealProgress()
    {
        float t = EvaluateTime01();
        float curved = progressCurve != null ? progressCurve.Evaluate(t) : t;
        return Mathf.Lerp(startProgress, endProgress, curved);
    }

    public float EvaluateBoost01()
    {
        float t = EvaluateTime01();
        return boostCurve != null ? Mathf.Clamp01(boostCurve.Evaluate(t)) : (1f - t);
    }

}