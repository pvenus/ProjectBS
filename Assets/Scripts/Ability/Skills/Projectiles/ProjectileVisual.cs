using System.Collections;
using System.Collections.Generic;
using Skill;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Util;

/// <summary>
/// ProjectileEntity의 비주얼 표현을 담당하는 컴포넌트.
/// SpriteRenderer / Animator / optional VFX root를 관리하고,
/// Animator trigger 방식과 AnimationClip 직접 재생 방식을 모두 지원한다.
/// 실제 속성별 리소스 선택은 이후 VisualResolver가 담당한다.
/// </summary>
public class ProjectileVisual : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Renderer materialTargetRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform vfxRoot;

    [Header("Animator Parameters")]
    [SerializeField] private string spawnTriggerName = "Spawn";
    [SerializeField] private string hitTriggerName = "Hit";
    [SerializeField] private string despawnTriggerName = "Despawn";


    [Header("Rain Visual")]
    [SerializeField] private int rainBurstCount = 16;
    [SerializeField] private float rainSpawnRadius = 2f;
    [SerializeField] private float rainStartHeight = 2.5f;
    [SerializeField] private float rainFallDistance = 5f;
    [SerializeField] private float rainDuration = 0.45f;
    [SerializeField] private float rainSpawnInterval = 0.03f;
    [SerializeField] private float rainRandomXJitter = 0.25f;
    [SerializeField] private float rainRandomStartDelay = 0.08f;
    [SerializeField] private bool hideSourceRendererForRain = true;

    [Header("Debug")]
    [SerializeField] private bool initialized;
    [SerializeField] private bool isClipPlaying;
    [SerializeField] private bool deactivateAfterClipFinished;
    [SerializeField] private float currentClipPlaybackSpeed = 1f;

    private readonly ProjectileDirectionPresentationLease directionWrapper=new();
    private ProjectileEntity owner;
    private ProjectileRuntimeData runtimeData;
    private SkillAnimationVfxFeatureObject animationVfx;
    private LayeredProjectilePresentationController layeredPresentation;
    private bool layeredPresentationActive;
    private bool sourceRendererEnabledBeforeLayers;
    private Material baselineSharedMaterial;
    private bool baselineMaterialCaptured;
    private Transform rendererScaleTransform;
    private Vector3 baselineRendererLocalScale;
    private Vector3 baselineRendererLocalPosition;
    private bool baselineRendererScaleCaptured;
    private float effectiveRendererScale = 1f;
    private SpriteRenderer presentationProxyRenderer;
    private bool sourceRendererEnabledBeforeProxy;
    private MaterialPropertyBlock presentationPropertyBlock;

    private PlayableGraph playableGraph;
    private AnimationClipPlayable clipPlayable;
    private AnimationClip currentClip;
    private double currentClipDuration;
    private bool graphCreated;

    private readonly List<SpriteRenderer> rainRenderers = new();
    private Coroutine rainRoutine;
    private readonly List<PlayableGraph> rainGraphs = new();
    private readonly Dictionary<SpriteRenderer, PlayableGraph> rainGraphByRenderer = new();

    public bool IsInitialized => initialized;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;
    public Transform VfxRoot => vfxRoot;
    public AnimationClip CurrentClip => currentClip;
    public bool IsClipPlaying => isClipPlaying;

    public float GetRemainingCurrentClipPlaybackTime()
    {
        float layeredRemaining = layeredPresentation != null
            ? layeredPresentation.RemainingTime
            : 0f;
        if (!isClipPlaying || currentClip == null || currentClip.length <= 0f ||
            currentClipPlaybackSpeed <= 0f || !clipPlayable.IsValid())
        {
            return layeredRemaining;
        }

        double clipTime = clipPlayable.GetTime();
        double remainingClipTime;

        if (currentClip.isLooping)
        {
            double normalizedClipTime = clipTime % currentClip.length;
            remainingClipTime = currentClip.length - normalizedClipTime;

            if (remainingClipTime <= 0.0001d)
            {
                remainingClipTime = currentClip.length;
            }
        }
        else
        {
            remainingClipTime = System.Math.Max(0d, currentClip.length - clipTime);
        }

        return Mathf.Max((float)(remainingClipTime / currentClipPlaybackSpeed), layeredRemaining);
    }

    private void Reset()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        materialTargetRenderer = GetComponentInChildren<Renderer>();
        animator = GetComponentInChildren<Animator>();

        if (vfxRoot == null)
        {
            vfxRoot = transform;
        }
    }

    private void Awake()
    {
        EnsureVisualComponents();
    }

    private void EnsureVisualComponents()
    {
        EnsureSpriteRenderer();
        EnsureAnimator();
        EnsureMaterialTargetRenderer();
        CaptureBaselineMaterial();
        CaptureBaselineRendererScale();
        EnsureVfxRoot();
    }

    private void CaptureBaselineMaterial()
    {
        if (baselineMaterialCaptured || materialTargetRenderer == null) return;
        baselineSharedMaterial = materialTargetRenderer.sharedMaterial;
        baselineMaterialCaptured = true;
    }

    private void RestoreBaselineMaterial()
    {
        animationVfx?.StopImmediate();
        if (materialTargetRenderer != null && baselineMaterialCaptured)
        {
            materialTargetRenderer.sharedMaterial = baselineSharedMaterial;
        }
    }

    private void EnsureSpriteRenderer()
    {
        if (spriteRenderer != null)
        {
            return;
        }

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    private void EnsureAnimator()
    {
        if (animator != null)
        {
            return;
        }

        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }
    }

    private void EnsureMaterialTargetRenderer()
    {
        if (materialTargetRenderer != null)
        {
            return;
        }

        if (spriteRenderer != null)
        {
            materialTargetRenderer = spriteRenderer;
            return;
        }

        materialTargetRenderer = GetComponentInChildren<Renderer>();
    }

    private void EnsureVfxRoot()
    {
        if (vfxRoot == null)
        {
            vfxRoot = transform;
        }
    }

    private void CaptureBaselineRendererScale()
    {
        Transform candidate = presentationProxyRenderer != null && presentationProxyRenderer.enabled
            ? presentationProxyRenderer.transform
            : spriteRenderer != null
            ? spriteRenderer.transform
            : materialTargetRenderer != null
                ? materialTargetRenderer.transform
                : null;

        if (candidate == null || candidate == transform)
        {
            return;
        }

        if (!baselineRendererScaleCaptured || rendererScaleTransform != candidate)
        {
            rendererScaleTransform = candidate;
            baselineRendererLocalScale = candidate.localScale;
            baselineRendererLocalPosition = candidate.localPosition;
            baselineRendererScaleCaptured = true;
        }
    }

    private void ApplyRendererScale(float scale)
    {
        CaptureBaselineRendererScale();
        if (!baselineRendererScaleCaptured || rendererScaleTransform == null)
        {
            return;
        }

        float effectiveScale = EquipmentBaseProfileSO.NormalizeRendererScale(scale);
        effectiveRendererScale = effectiveScale;
        rendererScaleTransform.localScale = baselineRendererLocalScale * effectiveScale;
    }

    private void RestoreRendererScale()
    {
        if (baselineRendererScaleCaptured && rendererScaleTransform != null)
        {
            rendererScaleTransform.localScale = baselineRendererLocalScale;
            rendererScaleTransform.localPosition = baselineRendererLocalPosition;
        }
        effectiveRendererScale = 1f;
    }

    private void Update()
    {
        if (!isClipPlaying || !graphCreated || currentClip == null)
        {
            return;
        }

        if (currentClip.isLooping)
        {
            return;
        }

        if (clipPlayable.IsDone())
        {
            Stop();

            if (deactivateAfterClipFinished && owner != null)
            {
                owner.Despawn();
            }
        }
    }

    private void LateUpdate()
    {
        ApplyRelativeSortingOrder();
        ApplyPresentationCalibration();
        directionWrapper.Apply();
    }

    private void ApplyPresentationCalibration()
    {
        if (!initialized || runtimeData == null || runtimeData.presentationCalibration == null || spriteRenderer == null)
        {
            return;
        }
        if (presentationProxyRenderer != null && presentationProxyRenderer.enabled)
        {
            CopyRendererPresentation(spriteRenderer, presentationProxyRenderer);
            presentationProxyRenderer.sprite = spriteRenderer.sprite;
        }
        CaptureBaselineRendererScale();
        if (!baselineRendererScaleCaptured || rendererScaleTransform == null) return;
        if (runtimeData.presentationCalibration.TryResolve(spriteRenderer.sprite, out Vector2 scale, out Vector2 offset))
        {
            rendererScaleTransform.localScale = Vector3.Scale(
                baselineRendererLocalScale * effectiveRendererScale,
                new Vector3(scale.x, scale.y, 1f));
            rendererScaleTransform.localPosition = baselineRendererLocalPosition + new Vector3(offset.x, offset.y, 0f);
        }
        else
        {
            rendererScaleTransform.localScale = baselineRendererLocalScale * effectiveRendererScale;
            rendererScaleTransform.localPosition = baselineRendererLocalPosition;
        }
    }

    private void BeginPresentationProxyIfRequired()
    {
        EndPresentationProxy();
        if (runtimeData == null || runtimeData.presentationCalibration == null ||
            spriteRenderer == null || spriteRenderer.transform != transform) return;

        Transform child = transform.Find("__ProjectilePresentationProxy");
        if (child == null)
        {
            child = new GameObject("__ProjectilePresentationProxy").transform;
            child.SetParent(transform, false);
        }
        presentationProxyRenderer = child.GetComponent<SpriteRenderer>() ?? child.gameObject.AddComponent<SpriteRenderer>();
        CopyRendererPresentation(spriteRenderer, presentationProxyRenderer);
        presentationProxyRenderer.sprite = spriteRenderer.sprite;
        presentationProxyRenderer.enabled = true;
        sourceRendererEnabledBeforeProxy = spriteRenderer.enabled;
        spriteRenderer.enabled = false;
        baselineRendererScaleCaptured = false;
        CaptureBaselineRendererScale();
    }

    private void EndPresentationProxy()
    {
        if (presentationProxyRenderer == null) return;
        presentationProxyRenderer.enabled = false;
        presentationProxyRenderer.sprite = null;
        presentationProxyRenderer.transform.localScale = Vector3.one;
        presentationProxyRenderer.transform.localPosition = Vector3.zero;
        if (spriteRenderer != null) spriteRenderer.enabled = sourceRendererEnabledBeforeProxy;
        presentationProxyRenderer = null;
        baselineRendererScaleCaptured = false;
        CaptureBaselineRendererScale();
    }

    private void CopyRendererPresentation(SpriteRenderer source, SpriteRenderer destination)
    {
        if (source == null || destination == null) return;
        destination.sharedMaterial = source.sharedMaterial;
        destination.color = source.color;
        destination.flipX = source.flipX;
        destination.flipY = source.flipY;
        destination.sortingLayerID = source.sortingLayerID;
        destination.sortingOrder = source.sortingOrder;
        destination.maskInteraction = source.maskInteraction;
        presentationPropertyBlock ??= new MaterialPropertyBlock();
        source.GetPropertyBlock(presentationPropertyBlock);
        destination.SetPropertyBlock(presentationPropertyBlock);
    }

    private void ApplyRelativeSortingOrder()
    {
        if (!initialized || runtimeData == null || spriteRenderer == null)
        {
            return;
        }

        // AboveOwner is the character-overlay contract: keep it above every
        // character/body/effect renderer rather than only ownerOrder + 1.
        if (runtimeData.sortingRelation == SkillSortingRelation.AbsoluteTop ||
            runtimeData.sortingRelation == SkillSortingRelation.AboveOwner)
        {
            ApplyResolvedSortingOrder((int)SkillSortingRelation.AbsoluteTop);
            return;
        }

        GameObject sortingOwner = runtimeData.owner;
        if (sortingOwner == null || sortingOwner == gameObject)
        {
            return;
        }

        int ownerOrder;
        ProjectileEntity ownerProjectile =
            sortingOwner.GetComponent<ProjectileEntity>();
        if (ownerProjectile != null &&
            ownerProjectile.Visual != null &&
            ownerProjectile.Visual.SpriteRenderer != null)
        {
            ownerOrder = ownerProjectile.Visual.SpriteRenderer.sortingOrder;
        }
        else
        {
            SortingOrderMono ownerSorting =
                sortingOwner.GetComponentInChildren<SortingOrderMono>();
            if (ownerSorting != null)
            {
                ownerOrder = ownerSorting.CalculateSortingOrder();
            }
            else
            {
                SpriteRenderer ownerRenderer =
                    sortingOwner.GetComponentInChildren<SpriteRenderer>();
                if (ownerRenderer == null)
                {
                    return;
                }

                ownerOrder = ownerRenderer.sortingOrder;
            }
        }

        int resolvedOrder = ownerOrder + (int)runtimeData.sortingRelation;
        ApplyResolvedSortingOrder(resolvedOrder);
    }

    private void ApplyResolvedSortingOrder(int resolvedOrder)
    {
        spriteRenderer.sortingOrder = resolvedOrder;
        animationVfx?.SetSortingOrder(resolvedOrder);
        layeredPresentation?.SetBaseSortingOrder(resolvedOrder);

        for (int i = 0; i < rainRenderers.Count; i++)
        {
            if (rainRenderers[i] != null)
            {
                rainRenderers[i].sortingOrder = resolvedOrder + i + 1;
            }
        }
    }

    private void OnDestroy()
    {
        layeredPresentation?.StopImmediate();
        directionWrapper.Restore();
        StopRainRoutine();
        RestoreBaselineMaterial();
        DestroyPlayableGraph();
    }

    public void Initialize(ProjectileEntity ownerEntity, ProjectileRuntimeData data)
    {
        if (ownerEntity == null)
        {
            Debug.LogError("ProjectileVisual.Initialize failed: ownerEntity is null.", this);
            return;
        }

        if (data == null)
        {
            Debug.LogError("ProjectileVisual.Initialize failed: ProjectileRuntimeData is null.", this);
            return;
        }

        directionWrapper.Restore();
        owner = ownerEntity;
        runtimeData = data;
        EnsureVisualComponents();
        StopLayeredPresentation();
        initialized = true;

        RestoreRendererScale();
        ApplyRendererScale(data.rendererScale);
        ApplyRuntimeVisualData(data);
        BeginPresentationProxyIfRequired();

        if (data.suppressVisual)
        {
            animationVfx?.StopImmediate();
            EndPresentationProxy();
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            return;
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;

        BaseVisualSO baseVisual = data.sourceEquipment?.BaseVisualSo;
        layeredPresentation ??= GetComponent<LayeredProjectilePresentationController>()
            ?? gameObject.AddComponent<LayeredProjectilePresentationController>();
        layeredPresentationActive = layeredPresentation.Begin(
            baseVisual != null ? baseVisual.LayeredPresentationProfile : null,
            spriteRenderer,
            data);
        if (layeredPresentationActive && spriteRenderer != null)
        {
            sourceRendererEnabledBeforeLayers = spriteRenderer.enabled;
            spriteRenderer.enabled = false;
        }

        if(data.orientManualPresentation&&!data.suppressVisual&&animator!=null)
        {
            // Animator is below this wrapper: its root curves cannot touch gameplay rotation.
            directionWrapper.Begin(animator.transform,transform,spriteRenderer,
                data.NormalizedDirection,data.moveRuntime?.applyDirectionRotation==true,
                data.moveRuntime?.rotationOffset??0f);
        }
        OnSpawn();
        directionWrapper.Apply();
    }
    private void OnDisable()
    {
        StopLayeredPresentation();
        directionWrapper.Restore();RestoreRendererScale();EndPresentationProxy();
    }

    public void OnSpawn()
    {
        if (!initialized || IsVisualSuppressed())
        {
            return;
        }
        if (layeredPresentationActive) return;

        if (IsRainVisualType())
        {
            animationVfx?.Play();
            PlayRainVisual();
            return;
        }

        AnimationClip clip = ResolveAnimationClip(SkillAnimationClipType.ProjectileLoop);
        if (!ShouldUseAnimatorTriggers() && clip != null)
        {
            PlayClip(clip);
            animationVfx?.Play();
            return;
        }

        animationVfx?.Play();
        TriggerAnimation(spawnTriggerName);
    }

    public void OnHit()
    {
        if (!initialized || IsVisualSuppressed())
        {
            return;
        }
        if (layeredPresentationActive) return;

        if (animationVfx != null && animationVfx.RestartsOnHit)
        {
            animationVfx.Play();
        }

        if (IsRainVisualType())
        {
            return;
        }

        AnimationClip clip = ResolveAnimationClip(SkillAnimationClipType.Hit);
        if (!ShouldUseAnimatorTriggers() && clip != null)
        {
            PlayClip(clip);
            return;
        }

        TriggerAnimation(hitTriggerName);
    }

    public void OnDespawn()
    {
        directionWrapper.Restore();
        if (!initialized)
        {
            return;
        }

        animationVfx?.StopImmediate();
        bool wasLayered = layeredPresentationActive;
        StopLayeredPresentation();
        RestoreBaselineMaterial();
        RestoreRendererScale();
        EndPresentationProxy();

        if (wasLayered) return;

        if (IsVisualSuppressed())
        {
            return;
        }

        if (IsRainVisualType())
        {
            StopRainRoutine();
            return;
        }

        TriggerAnimation(despawnTriggerName);
    }

    private bool IsVisualSuppressed() =>
        runtimeData != null && runtimeData.suppressVisual;

    private void StopLayeredPresentation()
    {
        layeredPresentation?.StopImmediate();
        if (layeredPresentationActive && spriteRenderer != null)
            spriteRenderer.enabled = sourceRendererEnabledBeforeLayers;
        layeredPresentationActive = false;
    }

    public void PlayClip(AnimationClip clip, bool deactivateWhenFinished = false)
    {
        if (clip == null)
        {
            return;
        }

        if (animator == null)
        {
            return;
        }

        EnsurePlayableGraph();

        if (!graphCreated)
        {
            return;
        }

        if (clipPlayable.IsValid())
        {
            clipPlayable.Destroy();
        }
        clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
        clipPlayable.SetApplyFootIK(false);
        clipPlayable.SetApplyPlayableIK(false);
        clipPlayable.SetTime(0d);
        currentClipPlaybackSpeed = ResolveClipPlaybackSpeed(clip);
        clipPlayable.SetSpeed(currentClipPlaybackSpeed);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(playableGraph, "ProjectileVisualOutput", animator);
        output.SetSourcePlayable(clipPlayable);

        currentClip = clip;
        currentClipDuration = clip.length / currentClipPlaybackSpeed;
        deactivateAfterClipFinished = deactivateWhenFinished;
        isClipPlaying = true;

        if (!playableGraph.IsPlaying())
        {
            playableGraph.Play();
        }

        // The shared projectile prefab starts with no sprite. Evaluate the
        // first clip sample immediately so short-lived melee projectiles have
        // a visible source texture before VFX/material state is applied.
        playableGraph.Evaluate(0f);
    }

    public void RestartCurrentClip()
    {
        if (currentClip == null)
        {
            return;
        }

        PlayClip(currentClip, deactivateAfterClipFinished);
    }

    public void Stop()
    {
        isClipPlaying = false;
        deactivateAfterClipFinished = false;

        if (graphCreated && playableGraph.IsValid())
        {
            playableGraph.Stop();
        }
    }


    public void SetMaterial(Material material)
    {
        EnsureMaterialTargetRenderer();

        if (materialTargetRenderer == null || material == null)
        {
            return;
        }

        materialTargetRenderer.sharedMaterial = material;
    }

    private void ApplyRuntimeVisualData(ProjectileRuntimeData data)
    {
        if (data == null)
        {
            return;
        }

        // ProjectileEntity may be reused. Always remove the previous profile's
        // shared material/MPB before applying this spawn's optional visual data.
        animationVfx?.StopImmediate();
        CaptureBaselineMaterial();
        if (materialTargetRenderer != null && baselineMaterialCaptured)
        {
            materialTargetRenderer.sharedMaterial = baselineSharedMaterial;
        }

        SetColor(data.color);

        if (data.material != null)
        {
            SetMaterial(data.material);
        }

        BaseVisualSO baseVisual = data.sourceEquipment != null
            ? data.sourceEquipment.BaseVisualSo
            : null;
        SkillAnimationVfxProfileSO profile = data.animationVfxProfileOverride != null
            ? data.animationVfxProfileOverride
            : baseVisual != null
                ? baseVisual.AnimationVfxProfile
                : null;
        if (profile != null && spriteRenderer != null)
        {
            if (profile.Material != null)
            {
                SetMaterial(profile.Material);
            }
            animationVfx ??= GetComponent<SkillAnimationVfxFeatureObject>()
                ?? gameObject.AddComponent<SkillAnimationVfxFeatureObject>();
            animationVfx.Initialize(spriteRenderer, profile, baseVisual.AnimationVfxPalette);
        }
        else
        {
            animationVfx?.StopImmediate();
            SkillAnimationVfxMaterialAuthority.Ensure(spriteRenderer);
        }
    }

    private AnimationClip ResolveAnimationClip(
        SkillAnimationClipType clipType)
    {
        if (clipType == SkillAnimationClipType.ProjectileLoop &&
            runtimeData != null && runtimeData.visualClipOverride != null)
        {
            return runtimeData.visualClipOverride;
        }
        BaseVisualSO baseVisual = runtimeData != null &&
                                  runtimeData.sourceEquipment != null
            ? runtimeData.sourceEquipment.BaseVisualSo
            : null;

        if (baseVisual == null || baseVisual.AnimationClips == null)
        {
            return null;
        }

        AnimationClipEntry[] clips = baseVisual.AnimationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClipEntry entry = clips[i];
            if (entry != null && entry.ClipType == clipType)
            {
                return entry.Clip;
            }
        }

        return null;
    }

    private bool ShouldUseAnimatorTriggers()
    {
        return runtimeData != null && runtimeData.useAnimatorTriggers;
    }

    private bool IsRainVisualType()
    {
        return runtimeData != null &&
               runtimeData.projectileVisualType == ProjectileVisualType.Rain;
    }

#if UNITY_EDITOR
    public bool EditorInitializeProceduralCapture(BaseVisualSO baseVisual)
    {
        EnsureVisualComponents();
        SkillAnimationVfxProfileSO captureProfile = baseVisual != null
            ? baseVisual.AnimationVfxProfile
            : null;
        if (captureProfile == null || !captureProfile.ProceduralGroundField || spriteRenderer == null)
            return false;
        animationVfx ??= GetComponent<SkillAnimationVfxFeatureObject>()
            ?? gameObject.AddComponent<SkillAnimationVfxFeatureObject>();
        animationVfx.Initialize(spriteRenderer, captureProfile, baseVisual.AnimationVfxPalette);
        animationVfx.Play();
        return true;
    }

    public void EditorApplyProceduralCapturePhase(float phase01)
    {
        animationVfx?.EditorApplyCapturePhase(phase01);
    }

    public void EditorStopProceduralCapture()
    {
        animationVfx?.StopImmediate();
    }
#endif

    private void PlayRainVisual()
    {
        StopRainRoutine();
        ClearRainRenderers();

        if (ResolveAnimationClip(SkillAnimationClipType.ProjectileLoop) == null)
        {
            return;
        }

        if (hideSourceRendererForRain && spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        rainRoutine = StartCoroutine(PlayRainRoutine());
    }

    private IEnumerator PlayRainRoutine()
    {
        int burstCount = Mathf.Max(1, rainBurstCount);
        float lifetime = ResolveRainLifetime();
        float fallDuration = ResolveRainFallDuration();

        float spawnWindow = Mathf.Max(0.01f, lifetime - fallDuration);
        float interval = spawnWindow / burstCount;

        for (int i = 0; i < burstCount; i++)
        {
            SpriteRenderer rainRenderer = CreateRainRenderer(i, burstCount);

            if (rainRenderer != null)
            {
                StartCoroutine(AnimateRainRenderer(rainRenderer));
            }

            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private SpriteRenderer CreateRainRenderer(int index, int count)
    {
        AnimationClip rainClip = ResolveAnimationClip(SkillAnimationClipType.ProjectileLoop);
        if (rainClip == null)
        {
            return null;
        }

        Transform parent = vfxRoot != null
            ? vfxRoot
            : transform;

        GameObject rainObject = new GameObject($"RainProjectile_{index + 1:00}");
        rainObject.transform.SetParent(parent, false);

        SpriteRenderer rainRenderer = rainObject.AddComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            rainRenderer.color = spriteRenderer.color;
            rainRenderer.flipX = spriteRenderer.flipX;
            rainRenderer.flipY = spriteRenderer.flipY;
            rainRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            rainRenderer.sortingOrder = spriteRenderer.sortingOrder + index + 1;
            rainRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
        }

        Animator rainAnimator = rainObject.AddComponent<Animator>();
        PlayableGraph rainGraph = PlayRainClip(rainAnimator, rainClip);

        if (rainGraph.IsValid())
        {
            rainGraphByRenderer[rainRenderer] = rainGraph;
        }

        // New: spawn in circular area instead of horizontal line
        Vector2 randomCircle = Random.insideUnitCircle * Mathf.Max(0f, rainSpawnRadius);
        randomCircle.x += Random.Range(-rainRandomXJitter, rainRandomXJitter);

        rainObject.transform.localPosition = new Vector3(
            randomCircle.x,
            rainStartHeight + randomCircle.y,
            0f);
        if (spriteRenderer != null)
        {
            rainObject.transform.localRotation = spriteRenderer.transform.localRotation;
            rainObject.transform.localScale = spriteRenderer.transform.localScale;
        }

        rainRenderers.Add(rainRenderer);
        return rainRenderer;
    }

    private PlayableGraph PlayRainClip(Animator rainAnimator, AnimationClip clip)
    {
        if (rainAnimator == null || clip == null)
        {
            return default;
        }

        PlayableGraph graph = PlayableGraph.Create($"RainProjectileVisual_{name}");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, clip);
        playable.SetApplyFootIK(false);
        playable.SetApplyPlayableIK(false);
        float playbackSpeed = ResolveClipPlaybackSpeed(clip);
        playable.SetDuration(clip.length / playbackSpeed);
        playable.SetTime(0d);
        playable.SetSpeed(playbackSpeed);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(
            graph,
            "RainProjectileOutput",
            rainAnimator);
        output.SetSourcePlayable(playable);

        graph.Play();
        rainGraphs.Add(graph);

        return graph;
    }

    private IEnumerator AnimateRainRenderer(SpriteRenderer rainRenderer)
    {
        if (rainRenderer == null)
        {
            yield break;
        }

        float delay = Random.Range(0f, Mathf.Max(0f, rainRandomStartDelay));

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        Transform rainTransform = rainRenderer.transform;
        Vector3 start = rainTransform.localPosition;
        Vector3 end = start + Vector3.down * Mathf.Max(0f, rainFallDistance);
        float duration = ResolveRainFallDuration();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (rainRenderer == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rainTransform.localPosition = Vector3.Lerp(start, end, t);

            yield return null;
        }

        DestroyRainRenderer(rainRenderer);
    }
    private void DestroyRainRenderer(SpriteRenderer rainRenderer)
    {
        if (rainRenderer == null)
        {
            return;
        }

        if (rainGraphByRenderer.TryGetValue(rainRenderer, out PlayableGraph graph))
        {
            if (graph.IsValid())
            {
                graph.Destroy();
            }

            rainGraphs.Remove(graph);
            rainGraphByRenderer.Remove(rainRenderer);
        }

        rainRenderers.Remove(rainRenderer);
        Destroy(rainRenderer.gameObject);
    }

    private float ResolveRainLifetime()
    {
        if (runtimeData != null && runtimeData.lifetime > 0f)
        {
            return runtimeData.lifetime;
        }

        return Mathf.Max(ResolveRainFallDuration(), rainDuration);
    }

    private float ResolveRainFallDuration()
    {
        AnimationClip rainClip = ResolveAnimationClip(SkillAnimationClipType.ProjectileLoop);
        if (rainClip != null && rainClip.length > 0f)
        {
            return rainClip.length / ResolveClipPlaybackSpeed(rainClip);
        }

        return Mathf.Max(0.01f, rainDuration);
    }

    private float ResolveClipPlaybackSpeed(AnimationClip clip)
    {
        if (clip == null || clip.length <= 0f)
        {
            return 1f;
        }

        float availableLifetime = runtimeData != null
            ? Mathf.Max(runtimeData.lifetime, runtimeData.minimumVisualLifetime)
            : 0f;

        if (availableLifetime <= 0f || clip.length <= availableLifetime)
        {
            return 1f;
        }

        return clip.length / Mathf.Max(0.01f, availableLifetime);
    }

    private void StopRainRoutine()
    {
        if (rainRoutine != null)
        {
            StopCoroutine(rainRoutine);
            rainRoutine = null;
        }

        ClearRainRenderers();

        if (spriteRenderer != null && hideSourceRendererForRain)
        {
            spriteRenderer.enabled = true;
        }
    }

    private void ClearRainRenderers()
    {
        for (int i = rainGraphs.Count - 1; i >= 0; i--)
        {
            PlayableGraph graph = rainGraphs[i];

            if (graph.IsValid())
            {
                graph.Destroy();
            }
        }

        rainGraphByRenderer.Clear();
        rainGraphs.Clear();

        for (int i = rainRenderers.Count - 1; i >= 0; i--)
        {
            SpriteRenderer rainRenderer = rainRenderers[i];

            if (rainRenderer != null)
            {
                Destroy(rainRenderer.gameObject);
            }
        }

        rainRenderers.Clear();
    }

    public void SetSprite(Sprite sprite)
    {
        EnsureSpriteRenderer();

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        EnsureSpriteRenderer();

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = color;
    }

    public void SetFlipX(bool flipX)
    {
        EnsureSpriteRenderer();

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.flipX = flipX;
    }

    public void SetSortingOrder(int sortingOrder)
    {
        EnsureSpriteRenderer();

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sortingOrder = sortingOrder;
    }

    private void TriggerAnimation(string triggerName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        animator.SetTrigger(triggerName);
    }

    private void EnsurePlayableGraph()
    {
        if (graphCreated && playableGraph.IsValid())
        {
            return;
        }

        playableGraph = PlayableGraph.Create($"ProjectileVisual_{name}");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        graphCreated = true;
    }

    private void DestroyPlayableGraph()
    {
        if (!graphCreated)
        {
            return;
        }

        if (playableGraph.IsValid())
        {
            playableGraph.Destroy();
        }

        graphCreated = false;
        isClipPlaying = false;
        currentClip = null;
        currentClipDuration = 0d;
    }
}
