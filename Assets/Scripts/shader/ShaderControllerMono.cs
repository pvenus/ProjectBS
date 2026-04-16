using UnityEngine;

/// <summary>
/// ShaderControllerMono
///
/// ShaderMono를 이용해 시간 기반 셰이더 연출을 관리하는 컨트롤러.
/// - 기능 단위(Feature Unit)로 상태를 나눈다.
/// - 외부에서는 `PlayHitFlash()` 같은 단일 함수만 호출하면 된다.
/// - 실제 셰이더 값 적용은 ShaderMono가 담당하고,
///   이 컴포넌트는 시간 흐름과 보간만 담당한다.
/// </summary>
[DisallowMultipleComponent]
public class ShaderControllerMono : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private ShaderMono shaderMono;

    [Header("Feature - Hit Flash")]
    [SerializeField] private HitFlashFeature hitFlash = new HitFlashFeature();

    [Header("Feature - Spawn Reveal")]
    [SerializeField] private SpawnRevealFeature spawnReveal = new SpawnRevealFeature();

    [Header("Feature - Death Dissolve")]
    [SerializeField] private DeathDissolveFeature deathDissolve = new DeathDissolveFeature();

    [Header("Debug")]
    [SerializeField] private bool debugSpawnReveal = true;
    [SerializeField] private bool debugDeathDissolve = true;

    public ShaderMono ShaderMono => shaderMono;
    public bool IsHitFlashPlaying => hitFlash != null && hitFlash.isPlaying;
    public bool IsSpawnRevealPlaying => spawnReveal != null && spawnReveal.isPlaying;
    public bool IsDeathDissolvePlaying => deathDissolve != null && deathDissolve.isPlaying;

    private void Reset()
    {
        shaderMono = GetComponent<ShaderMono>();
        if (shaderMono == null)
            shaderMono = gameObject.AddComponent<ShaderMono>();
    }

    private void Awake()
    {
        if (shaderMono == null)
            shaderMono = GetComponent<ShaderMono>();
        if (shaderMono == null)
            shaderMono = gameObject.AddComponent<ShaderMono>();

        if (hitFlash != null)
            hitFlash.EnsureDefaults();

        if (spawnReveal != null)
            spawnReveal.EnsureDefaults();

        if (deathDissolve != null)
            deathDissolve.EnsureDefaults();

        ApplyBaseState(forceApply: true);
    }

    private void Update()
    {
        if (shaderMono == null || !shaderMono.HasValidRenderer())
            return;

        bool changed = false;

        if (spawnReveal != null && spawnReveal.enabled)
        {
            bool wasPlaying = spawnReveal.isPlaying;
            spawnReveal.Tick(Time.deltaTime);
            if (wasPlaying || spawnReveal.isPlaying)
            {
                ApplySpawnRevealFeature();
                changed = true;

                if (wasPlaying && !spawnReveal.isPlaying && spawnReveal.resetToBaseWhenFinished)
                {
                    bool isHitPlaying = hitFlash != null && hitFlash.enabled && hitFlash.isPlaying;
                    bool isDeathPlaying = deathDissolve != null && deathDissolve.enabled && deathDissolve.isPlaying;
                    if (!isHitPlaying && !isDeathPlaying)
                    {
                        ApplyBaseState(forceApply: false);
                        changed = true;
                    }
                }
            }
        }

        if (deathDissolve != null && deathDissolve.enabled)
        {
            bool wasPlaying = deathDissolve.isPlaying;

            if (debugDeathDissolve)
                Debug.Log($"[ShaderControllerMono] Update death block / wasPlaying={wasPlaying} / timer(before)={deathDissolve.timer:0.0000}", this);

            deathDissolve.Tick(Time.deltaTime);

            if (debugDeathDissolve)
                Debug.Log($"[ShaderControllerMono] Update death block / isPlaying(after Tick)={deathDissolve.isPlaying} / timer(after)={deathDissolve.timer:0.0000}", this);

            if (wasPlaying || deathDissolve.isPlaying)
            {
                ApplyDeathDissolveFeature();
                changed = true;
            }
        }

        if (hitFlash != null && hitFlash.enabled)
        {
            bool wasPlaying = hitFlash.isPlaying;
            hitFlash.Tick(Time.deltaTime);
            if (wasPlaying || hitFlash.isPlaying)
            {
                // Hit flash is applied after spawn reveal so that shared rim/pulse
                // values are overridden by damage feedback when both are active.
                ApplyHitFlashFeature();
                changed = true;
            }
        }

        if (changed)
            shaderMono.ApplyIfDirty();
    }

    /// <summary>
    /// 외부 피격 처리에서는 이 함수 하나만 호출하면 된다.
    /// </summary>
    public void PlayHitFlash()
    {
        if (hitFlash == null || !hitFlash.enabled)
            return;

        hitFlash.Play();
        ApplyHitFlashFeature();
        shaderMono?.ApplyIfDirty();
    }


    /// <summary>
    /// 외부 소환/등장 처리에서는 이 함수 하나만 호출하면 된다.
    /// </summary>
    public void PlaySpawnReveal()
    {
        if (debugSpawnReveal)
            Debug.Log($"[ShaderControllerMono] PlaySpawnReveal() called / hasSpawnReveal={(spawnReveal != null)} / enabled={(spawnReveal != null && spawnReveal.enabled)} / activeInHierarchy={gameObject.activeInHierarchy} / componentEnabled={enabled}", this);

        if (spawnReveal == null || !spawnReveal.enabled)
            return;

        spawnReveal.Play();

        ApplySpawnRevealFeature();
        shaderMono?.ApplyIfDirty();
    }

    /// <summary>
    /// 외부 사망 처리에서는 이 함수 하나만 호출하면 된다.
    /// </summary>
    public void PlayDeathDissolve()
    {
        if (debugDeathDissolve)
            Debug.Log($"[ShaderControllerMono] PlayDeathDissolve() called / hasDeathDissolve={(deathDissolve != null)} / enabled={(deathDissolve != null && deathDissolve.enabled)} / activeInHierarchy={gameObject.activeInHierarchy} / componentEnabled={enabled}", this);

        if (deathDissolve == null || !deathDissolve.enabled)
            return;

        if (spawnReveal != null)
            spawnReveal.StopImmediate();

        if (hitFlash != null)
            hitFlash.StopImmediate();

        deathDissolve.Play();

        if (debugDeathDissolve)
            Debug.Log($"[ShaderControllerMono] PlayDeathDissolve() after Play / isPlaying={deathDissolve.isPlaying} / timer={deathDissolve.timer:0.0000}", this);

        ApplyDeathDissolveFeature();
        shaderMono?.ApplyIfDirty();
    }

    public void StopHitFlash(bool restoreBaseState = true)
    {
        if (hitFlash == null)
            return;

        hitFlash.StopImmediate();

        if (restoreBaseState)
        {
            ApplyBaseState(forceApply: true);
        }
    }


    public void StopSpawnReveal(bool restoreBaseState = true)
    {
        if (spawnReveal == null)
            return;

        spawnReveal.StopImmediate();

        if (restoreBaseState)
            ApplyBaseState(forceApply: true);
    }

    public void StopDeathDissolve(bool restoreBaseState = true)
    {
        if (deathDissolve == null)
            return;

        deathDissolve.StopImmediate();

        if (restoreBaseState)
            ApplyBaseState(forceApply: true);
    }

    public void ApplyBaseState(bool forceApply = false)
    {
        if (shaderMono == null || !shaderMono.HasValidRenderer())
            return;

        if (hitFlash != null)
        {
            hitFlash.EnsureDefaults();
            shaderMono.SetRim(hitFlash.baseRimColor, hitFlash.baseRimIntensity);
            shaderMono.SetRimPower(hitFlash.baseRimPower);
            shaderMono.SetPulse(hitFlash.basePulseIntensity, hitFlash.basePulseSpeed);
        }

        if (spawnReveal != null)
        {
            spawnReveal.EnsureDefaults();
            shaderMono.SetRevealFeature(
                spawnReveal.endProgress,
                spawnReveal.softness,
                spawnReveal.edgeWidth,
                spawnReveal.edgeColor,
                0f,
                spawnReveal.noiseStrength
            );

            if (spawnReveal.boostRimAndPulse)
            {
                shaderMono.SetRim(spawnReveal.baseRimColor, spawnReveal.baseRimIntensity);
                shaderMono.SetRimPower(spawnReveal.baseRimPower);
                shaderMono.SetPulse(spawnReveal.basePulseIntensity, spawnReveal.basePulseSpeed);
            }
        }

        if (deathDissolve != null)
        {
            deathDissolve.EnsureDefaults();
            shaderMono.SetDeathFeature(
                deathDissolve.startProgress,
                deathDissolve.softness,
                deathDissolve.edgeWidth,
                deathDissolve.edgeColor,
                0f,
                deathDissolve.noiseStrength,
                deathDissolve.driftY
            );

            if (deathDissolve.fadeRimAndPulse)
            {
                shaderMono.SetRim(deathDissolve.baseRimColor, deathDissolve.baseRimIntensity);
                shaderMono.SetRimPower(deathDissolve.baseRimPower);
                shaderMono.SetPulse(deathDissolve.basePulseIntensity, deathDissolve.basePulseSpeed);
            }
        }

        if (forceApply)
            shaderMono.Apply();
        else
            shaderMono.ApplyIfDirty();
    }

    private void ApplyHitFlashFeature()
    {
        if (shaderMono == null || hitFlash == null)
            return;

        hitFlash.EnsureDefaults();

        float t = hitFlash.EvaluateBlend01();

        Color rimColor = Color.Lerp(hitFlash.baseRimColor, hitFlash.hitRimColor, t);
        float rimIntensity = Mathf.Lerp(hitFlash.baseRimIntensity, hitFlash.hitRimIntensity, t);
        float rimPower = Mathf.Lerp(hitFlash.baseRimPower, hitFlash.hitRimPower, t);
        float pulseIntensity = Mathf.Lerp(hitFlash.basePulseIntensity, hitFlash.hitPulseIntensity, t);
        float pulseSpeed = Mathf.Lerp(hitFlash.basePulseSpeed, hitFlash.hitPulseSpeed, t);

        shaderMono.SetRim(rimColor, rimIntensity);
        shaderMono.SetRimPower(rimPower);
        shaderMono.SetPulse(pulseIntensity, pulseSpeed);
    }

    private void ApplySpawnRevealFeature()
    {
        if (shaderMono == null || spawnReveal == null)
            return;

        spawnReveal.EnsureDefaults();

        float revealProgress = spawnReveal.isPlaying ? spawnReveal.EvaluateRevealProgress() : spawnReveal.endProgress;
        float boost01 = spawnReveal.isPlaying ? spawnReveal.EvaluateBoost01() : 0f;


        shaderMono.SetRevealFeature(
            revealProgress,
            spawnReveal.softness,
            spawnReveal.edgeWidth,
            spawnReveal.edgeColor,
            spawnReveal.edgeIntensity,
            spawnReveal.noiseStrength
        );

        if (spawnReveal.boostRimAndPulse)
        {
            Color rimColor = Color.Lerp(spawnReveal.baseRimColor, spawnReveal.revealRimColor, boost01);
            float rimIntensity = Mathf.Lerp(spawnReveal.baseRimIntensity, spawnReveal.revealRimIntensity, boost01);
            float rimPower = Mathf.Lerp(spawnReveal.baseRimPower, spawnReveal.revealRimPower, boost01);
            float pulseIntensity = Mathf.Lerp(spawnReveal.basePulseIntensity, spawnReveal.revealPulseIntensity, boost01);
            float pulseSpeed = Mathf.Lerp(spawnReveal.basePulseSpeed, spawnReveal.revealPulseSpeed, boost01);

            shaderMono.SetRim(rimColor, rimIntensity);
            shaderMono.SetRimPower(rimPower);
            shaderMono.SetPulse(pulseIntensity, pulseSpeed);
        }
    }

    private void ApplyDeathDissolveFeature()
    {
        if (shaderMono == null || deathDissolve == null)
            return;

        deathDissolve.EnsureDefaults();

        float deathProgress = deathDissolve.isPlaying ? deathDissolve.EvaluateDeathProgress() : deathDissolve.startProgress;
        float fade01 = deathDissolve.isPlaying ? deathDissolve.EvaluateFade01() : 1f;

        if (debugDeathDissolve)
            Debug.Log($"[ShaderControllerMono] ApplyDeathDissolveFeature / isPlaying={deathDissolve.isPlaying} / timer={deathDissolve.timer:0.0000} / deathProgress={deathProgress:0.0000} / fade01={fade01:0.0000}", this);

        shaderMono.SetDeathFeature(
            deathProgress,
            deathDissolve.softness,
            deathDissolve.edgeWidth,
            deathDissolve.edgeColor,
            deathDissolve.edgeIntensity,
            deathDissolve.noiseStrength,
            deathDissolve.driftY
        );

        if (deathDissolve.fadeRimAndPulse)
        {
            Color rimColor = Color.Lerp(deathDissolve.baseRimColor, deathDissolve.deathRimColor, 1f - fade01);
            float rimIntensity = Mathf.Lerp(deathDissolve.baseRimIntensity, deathDissolve.deathRimIntensity, 1f - fade01);
            float rimPower = Mathf.Lerp(deathDissolve.baseRimPower, deathDissolve.deathRimPower, 1f - fade01);
            float pulseIntensity = Mathf.Lerp(deathDissolve.basePulseIntensity, deathDissolve.deathPulseIntensity, 1f - fade01);
            float pulseSpeed = Mathf.Lerp(deathDissolve.basePulseSpeed, deathDissolve.deathPulseSpeed, 1f - fade01);

            shaderMono.SetRim(rimColor, rimIntensity);
            shaderMono.SetRimPower(rimPower);
            shaderMono.SetPulse(pulseIntensity, pulseSpeed);
        }
    }
}