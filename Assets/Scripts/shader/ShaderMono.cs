using UnityEngine;

/// <summary>
/// ShaderMono
///
/// 셰이더 프로퍼티를 안전하게 읽고/쓰는 얇은 브리지 컴포넌트.
/// - 시간에 따른 연출 로직은 넣지 않는다.
/// - 실제 연출 타이머, 코루틴, 상태 머신은 별도 컴포넌트에서 처리한다.
/// - 이 컴포넌트는 SpriteRenderer + MaterialPropertyBlock 기반으로
///   개별 오브젝트 셰이더 값을 제어하는 역할만 담당한다.
/// </summary>
[DisallowMultipleComponent]
public class ShaderMono : MonoBehaviour
{
    private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
    private static readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");
    private static readonly int RimPowerId = Shader.PropertyToID("_RimPower");
    private static readonly int PulseIntensityId = Shader.PropertyToID("_PulseIntensity");
    private static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");
    private static readonly int SparkleIntensityId = Shader.PropertyToID("_SparkleIntensity");
    private static readonly int SparkleSpeedId = Shader.PropertyToID("_SparkleSpeed");
    private static readonly int FlowIntensityId = Shader.PropertyToID("_FlowIntensity");
    private static readonly int FlowSpeedXId = Shader.PropertyToID("_FlowSpeedX");
    private static readonly int FlowSpeedYId = Shader.PropertyToID("_FlowSpeedY");
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int RevealProgressId = Shader.PropertyToID("_RevealProgress");
    private static readonly int RevealSoftnessId = Shader.PropertyToID("_RevealSoftness");
    private static readonly int RevealEdgeWidthId = Shader.PropertyToID("_RevealEdgeWidth");
    private static readonly int RevealEdgeColorId = Shader.PropertyToID("_RevealEdgeColor");
    private static readonly int RevealEdgeIntensityId = Shader.PropertyToID("_RevealEdgeIntensity");
    private static readonly int RevealNoiseStrengthId = Shader.PropertyToID("_RevealNoiseStrength");
    private static readonly int DeathProgressId = Shader.PropertyToID("_DeathProgress");
    private static readonly int DeathSoftnessId = Shader.PropertyToID("_DeathSoftness");
    private static readonly int DeathEdgeWidthId = Shader.PropertyToID("_DeathEdgeWidth");
    private static readonly int DeathEdgeColorId = Shader.PropertyToID("_DeathEdgeColor");
    private static readonly int DeathEdgeIntensityId = Shader.PropertyToID("_DeathEdgeIntensity");
    private static readonly int DeathNoiseStrengthId = Shader.PropertyToID("_DeathNoiseStrength");
    private static readonly int DeathDriftYId = Shader.PropertyToID("_DeathDriftY");

    [Header("Reference")]
    [SerializeField] private SpriteRenderer targetRenderer;

    private MaterialPropertyBlock _propertyBlock;
    private bool _isDirty;

    public SpriteRenderer TargetRenderer => targetRenderer;

    private void Reset()
    {
        targetRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>();

        EnsurePropertyBlock();
    }

    /// <summary>
    /// 외부에서 대상 렌더러를 교체할 때 사용.
    /// </summary>
    public void SetTargetRenderer(SpriteRenderer renderer, bool reapply = true)
    {
        targetRenderer = renderer;

        if (reapply)
            Apply();
    }

    public bool HasValidRenderer()
    {
        return targetRenderer != null;
    }

    public void SetRim(Color color, float intensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(RimColorId, color);
        _propertyBlock.SetFloat(RimIntensityId, intensity);
        _isDirty = true;
    }

    public void SetRimColor(Color color)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(RimColorId, color);
        _isDirty = true;
    }

    public void SetRimIntensity(float intensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RimIntensityId, intensity);
        _isDirty = true;
    }

    public void SetRimPower(float power)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RimPowerId, power);
        _isDirty = true;
    }

    public void SetPulse(float intensity, float speed)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(PulseIntensityId, intensity);
        _propertyBlock.SetFloat(PulseSpeedId, speed);
        _isDirty = true;
    }

    public void SetPulseIntensity(float intensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(PulseIntensityId, intensity);
        _isDirty = true;
    }

    public void SetPulseSpeed(float speed)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(PulseSpeedId, speed);
        _isDirty = true;
    }

    public void SetSparkle(float intensity, float speed)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(SparkleIntensityId, intensity);
        _propertyBlock.SetFloat(SparkleSpeedId, speed);
        _isDirty = true;
    }

    public void SetFlow(float intensity, Vector2 speed)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(FlowIntensityId, intensity);
        _propertyBlock.SetFloat(FlowSpeedXId, speed.x);
        _propertyBlock.SetFloat(FlowSpeedYId, speed.y);
        _isDirty = true;
    }

    public void SetOutline(Color color, float width)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(OutlineColorId, color);
        _propertyBlock.SetFloat(OutlineWidthId, width);
        _isDirty = true;
    }

    public void SetTint(Color color)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(ColorId, color);
        _isDirty = true;
    }

    public void SetReveal(float progress)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RevealProgressId, Mathf.Clamp01(progress));
        _isDirty = true;
    }

    public void SetRevealProgress(float progress)
    {
        SetReveal(progress);
    }

    public void SetRevealSoftness(float softness)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RevealSoftnessId, Mathf.Max(0.001f, softness));
        _isDirty = true;
    }

    public void SetRevealEdgeWidth(float edgeWidth)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RevealEdgeWidthId, Mathf.Max(0.001f, edgeWidth));
        _isDirty = true;
    }

    public void SetRevealEdgeColor(Color color)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(RevealEdgeColorId, color);
        _isDirty = true;
    }

    public void SetRevealEdgeIntensity(float intensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RevealEdgeIntensityId, Mathf.Max(0f, intensity));
        _isDirty = true;
    }

    public void SetRevealNoiseStrength(float noiseStrength)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RevealNoiseStrengthId, Mathf.Clamp01(noiseStrength));
        _isDirty = true;
    }

    public void SetRevealEdge(Color color, float edgeWidth, float edgeIntensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(RevealEdgeColorId, color);
        _propertyBlock.SetFloat(RevealEdgeWidthId, Mathf.Max(0.001f, edgeWidth));
        _propertyBlock.SetFloat(RevealEdgeIntensityId, Mathf.Max(0f, edgeIntensity));
        _isDirty = true;
    }

    public void SetRevealFeature(float progress, float softness, float edgeWidth, Color edgeColor, float edgeIntensity, float noiseStrength)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(RevealProgressId, Mathf.Clamp01(progress));
        _propertyBlock.SetFloat(RevealSoftnessId, Mathf.Max(0.001f, softness));
        _propertyBlock.SetFloat(RevealEdgeWidthId, Mathf.Max(0.001f, edgeWidth));
        _propertyBlock.SetColor(RevealEdgeColorId, edgeColor);
        _propertyBlock.SetFloat(RevealEdgeIntensityId, Mathf.Max(0f, edgeIntensity));
        _propertyBlock.SetFloat(RevealNoiseStrengthId, Mathf.Clamp01(noiseStrength));
        _isDirty = true;
    }

    public void SetDeathProgress(float progress)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathProgressId, Mathf.Clamp01(progress));
        _isDirty = true;
    }

    public void SetDeathSoftness(float softness)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathSoftnessId, Mathf.Max(0.001f, softness));
        _isDirty = true;
    }

    public void SetDeathEdgeWidth(float edgeWidth)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathEdgeWidthId, Mathf.Max(0.001f, edgeWidth));
        _isDirty = true;
    }

    public void SetDeathEdgeColor(Color color)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(DeathEdgeColorId, color);
        _isDirty = true;
    }

    public void SetDeathEdgeIntensity(float intensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathEdgeIntensityId, Mathf.Max(0f, intensity));
        _isDirty = true;
    }

    public void SetDeathNoiseStrength(float noiseStrength)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathNoiseStrengthId, Mathf.Clamp01(noiseStrength));
        _isDirty = true;
    }

    public void SetDeathDriftY(float driftY)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathDriftYId, Mathf.Max(0f, driftY));
        _isDirty = true;
    }

    public void SetDeathEdge(Color color, float edgeWidth, float edgeIntensity)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetColor(DeathEdgeColorId, color);
        _propertyBlock.SetFloat(DeathEdgeWidthId, Mathf.Max(0.001f, edgeWidth));
        _propertyBlock.SetFloat(DeathEdgeIntensityId, Mathf.Max(0f, edgeIntensity));
        _isDirty = true;
    }

    public void SetDeathFeature(float progress, float softness, float edgeWidth, Color edgeColor, float edgeIntensity, float noiseStrength, float driftY)
    {
        if (!HasValidRenderer())
            return;

        EnsureLoaded();
        _propertyBlock.SetFloat(DeathProgressId, Mathf.Clamp01(progress));
        _propertyBlock.SetFloat(DeathSoftnessId, Mathf.Max(0.001f, softness));
        _propertyBlock.SetFloat(DeathEdgeWidthId, Mathf.Max(0.001f, edgeWidth));
        _propertyBlock.SetColor(DeathEdgeColorId, edgeColor);
        _propertyBlock.SetFloat(DeathEdgeIntensityId, Mathf.Max(0f, edgeIntensity));
        _propertyBlock.SetFloat(DeathNoiseStrengthId, Mathf.Clamp01(noiseStrength));
        _propertyBlock.SetFloat(DeathDriftYId, Mathf.Max(0f, driftY));
        _isDirty = true;
    }

    public Color GetRimColor(Color fallback)
    {
        return TryGetColor(RimColorId, fallback);
    }

    public float GetRimIntensity(float fallback = 0f)
    {
        return TryGetFloat(RimIntensityId, fallback);
    }

    public float GetRimPower(float fallback = 0f)
    {
        return TryGetFloat(RimPowerId, fallback);
    }

    public float GetPulseIntensity(float fallback = 0f)
    {
        return TryGetFloat(PulseIntensityId, fallback);
    }

    public float GetPulseSpeed(float fallback = 0f)
    {
        return TryGetFloat(PulseSpeedId, fallback);
    }

    public float GetSparkleIntensity(float fallback = 0f)
    {
        return TryGetFloat(SparkleIntensityId, fallback);
    }

    public float GetSparkleSpeed(float fallback = 0f)
    {
        return TryGetFloat(SparkleSpeedId, fallback);
    }

    public float GetFlowIntensity(float fallback = 0f)
    {
        return TryGetFloat(FlowIntensityId, fallback);
    }

    public Vector2 GetFlowSpeed(Vector2 fallback)
    {
        float x = TryGetFloat(FlowSpeedXId, fallback.x);
        float y = TryGetFloat(FlowSpeedYId, fallback.y);
        return new Vector2(x, y);
    }

    public Color GetOutlineColor(Color fallback)
    {
        return TryGetColor(OutlineColorId, fallback);
    }

    public float GetOutlineWidth(float fallback = 0f)
    {
        return TryGetFloat(OutlineWidthId, fallback);
    }

    public Color GetTint(Color fallback)
    {
        return TryGetColor(ColorId, fallback);
    }

    public float GetRevealProgress(float fallback = 1f)
    {
        return TryGetFloat(RevealProgressId, fallback);
    }

    public float GetRevealSoftness(float fallback = 0.05f)
    {
        return TryGetFloat(RevealSoftnessId, fallback);
    }

    public float GetRevealEdgeWidth(float fallback = 0.08f)
    {
        return TryGetFloat(RevealEdgeWidthId, fallback);
    }

    public Color GetRevealEdgeColor(Color fallback)
    {
        return TryGetColor(RevealEdgeColorId, fallback);
    }

    public float GetRevealEdgeIntensity(float fallback = 1.5f)
    {
        return TryGetFloat(RevealEdgeIntensityId, fallback);
    }

    public float GetRevealNoiseStrength(float fallback = 0.15f)
    {
        return TryGetFloat(RevealNoiseStrengthId, fallback);
    }

    public float GetDeathProgress(float fallback = 0f)
    {
        return TryGetFloat(DeathProgressId, fallback);
    }

    public float GetDeathSoftness(float fallback = 0.06f)
    {
        return TryGetFloat(DeathSoftnessId, fallback);
    }

    public float GetDeathEdgeWidth(float fallback = 0.1f)
    {
        return TryGetFloat(DeathEdgeWidthId, fallback);
    }

    public Color GetDeathEdgeColor(Color fallback)
    {
        return TryGetColor(DeathEdgeColorId, fallback);
    }

    public float GetDeathEdgeIntensity(float fallback = 1.4f)
    {
        return TryGetFloat(DeathEdgeIntensityId, fallback);
    }

    public float GetDeathNoiseStrength(float fallback = 0.25f)
    {
        return TryGetFloat(DeathNoiseStrengthId, fallback);
    }

    public float GetDeathDriftY(float fallback = 0.35f)
    {
        return TryGetFloat(DeathDriftYId, fallback);
    }

    /// <summary>
    /// 현재 수정된 PropertyBlock 내용을 렌더러에 반영.
    /// 외부 연출 코드에서 값 여러 개를 바꾼 뒤 한 번만 호출하면 된다.
    /// </summary>
    public void Apply()
    {
        if (!HasValidRenderer())
            return;

        EnsurePropertyBlock();
        targetRenderer.SetPropertyBlock(_propertyBlock);
        _isDirty = false;
    }

    /// <summary>
    /// 값을 수정한 경우에만 Apply.
    /// 외부 Update/LateUpdate 루프에서 쓰기 좋다.
    /// </summary>
    public void ApplyIfDirty()
    {
        if (!_isDirty)
            return;

        Apply();
    }

    /// <summary>
    /// 현재 렌더러의 PropertyBlock을 다시 읽어온다.
    /// 외부에서 값을 바꿨을 때 동기화가 필요하면 사용.
    /// </summary>
    public void Reload()
    {
        if (!HasValidRenderer())
            return;

        EnsurePropertyBlock();
        targetRenderer.GetPropertyBlock(_propertyBlock);
        _isDirty = false;
    }

    /// <summary>
    /// 현재 오브젝트에 적용된 PropertyBlock 오버라이드를 제거.
    /// 공유 머터리얼 기본값으로 되돌리고 싶을 때 사용.
    /// </summary>
    public void ClearOverrides()
    {
        if (!HasValidRenderer())
            return;

        EnsurePropertyBlock();
        _propertyBlock.Clear();
        targetRenderer.SetPropertyBlock(_propertyBlock);
        _isDirty = false;
    }

    private void EnsureLoaded()
    {
        EnsurePropertyBlock();

        if (!HasValidRenderer())
            return;

        // Only reload from the renderer when we are not already editing a dirty block.
        // This prevents sequential setter calls in the same frame from wiping out
        // previously staged values before Apply()/ApplyIfDirty() is called.
        if (!_isDirty)
            targetRenderer.GetPropertyBlock(_propertyBlock);
    }

    private void EnsurePropertyBlock()
    {
        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();
    }

    private float TryGetFloat(int propertyId, float fallback)
    {
        if (!HasValidRenderer())
            return fallback;

        Material material = targetRenderer.sharedMaterial;
        if (material == null)
            return fallback;

        return material.HasProperty(propertyId) ? material.GetFloat(propertyId) : fallback;
    }

    private Color TryGetColor(int propertyId, Color fallback)
    {
        if (!HasValidRenderer())
            return fallback;

        Material material = targetRenderer.sharedMaterial;
        if (material == null)
            return fallback;

        return material.HasProperty(propertyId) ? material.GetColor(propertyId) : fallback;
    }
}