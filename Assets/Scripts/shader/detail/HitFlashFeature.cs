using UnityEngine;

[System.Serializable]
public class HitFlashFeature
{
    [Header("Enabled")]
    public bool enabled = true;

    [Header("Rim")]
    public Color baseRimColor = Color.white;
    public float baseRimIntensity = 0f;
    public float baseRimPower = 3f;

    public Color hitRimColor = new Color(1f, 0.95f, 0.95f, 1f);
    public float hitRimIntensity = 2.5f;
    public float hitRimPower = 2f;

    [Header("Pulse")]
    public float basePulseIntensity = 0f;
    public float basePulseSpeed = 2f;
    public float hitPulseIntensity = 0.35f;
    public float hitPulseSpeed = 8f;

    [Header("Timing")]
    [Min(0.01f)] public float attackDuration = 0.03f;
    [Min(0.01f)] public float releaseDuration = 0.12f;
    public AnimationCurve blendCurve = null;

    [Header("Behavior")]
    public bool allowRefreshWhilePlaying = true;

    [HideInInspector] public bool isPlaying;
    [HideInInspector] public float timer;
    [HideInInspector] public float duration;
    [HideInInspector] public Phase phase = Phase.Idle;

    public enum Phase
    {
        Idle,
        Attack,
        Release
    }

    public void EnsureDefaults()
    {
        if (blendCurve == null || blendCurve.length == 0)
            blendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        attackDuration = Mathf.Max(0.01f, attackDuration);
        releaseDuration = Mathf.Max(0.01f, releaseDuration);
        baseRimPower = Mathf.Max(0f, baseRimPower);
        hitRimPower = Mathf.Max(0f, hitRimPower);
    }

    public void Play()
    {
        Debug.Log($"[HitFlashFeature] Play requested / enabled={enabled} / allowRefreshWhilePlaying={allowRefreshWhilePlaying} / isPlaying(before)={isPlaying}");
        if (!enabled)
            return;

        if (isPlaying && !allowRefreshWhilePlaying)
            return;

        EnsureDefaults();
        isPlaying = true;
        phase = Phase.Attack;
        timer = 0f;
        duration = attackDuration;
    }

    public void StopImmediate()
    {
        isPlaying = false;
        phase = Phase.Idle;
        timer = 0f;
        duration = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (!isPlaying)
            return;

        timer += Mathf.Max(0f, deltaTime);

        if (phase == Phase.Attack && timer >= duration)
        {
            phase = Phase.Release;
            timer = 0f;
            duration = releaseDuration;
            return;
        }

        if (phase == Phase.Release && timer >= duration)
        {
            StopImmediate();
        }
    }

    public float EvaluateBlend01()
    {
        if (!isPlaying)
            return 0f;

        float normalized = duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
        float curved = blendCurve != null ? blendCurve.Evaluate(normalized) : normalized;

        switch (phase)
        {
            case Phase.Attack:
                return curved;
            case Phase.Release:
                return 1f - curved;
            default:
                return 0f;
        }
    }
}