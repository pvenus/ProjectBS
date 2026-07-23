using System.Collections;
using System.Collections.Generic;
using Skill;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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

    private ProjectileEntity owner;
    private ProjectileRuntimeData runtimeData;

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
        EnsureVfxRoot();
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

    private void OnDestroy()
    {
        StopRainRoutine();
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

        owner = ownerEntity;
        runtimeData = data;
        EnsureVisualComponents();
        initialized = true;

        ApplyRuntimeVisualData(data);

        OnSpawn();
    }

    public void OnSpawn()
    {
        if (!initialized)
        {
            return;
        }

        if (IsRainVisualType())
        {
            PlayRainVisual();
            return;
        }

        AnimationClip clip = ResolveAnimationClip(SkillAnimationClipType.ProjectileLoop);
        if (!ShouldUseAnimatorTriggers() && clip != null)
        {
            PlayClip(clip);
            return;
        }

        TriggerAnimation(spawnTriggerName);
    }

    public void OnHit()
    {
        if (!initialized)
        {
            return;
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
        if (!initialized)
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

        materialTargetRenderer.material = material;
    }

    private void ApplyRuntimeVisualData(ProjectileRuntimeData data)
    {
        if (data == null)
        {
            return;
        }

        SetColor(data.color);

        if (data.material != null)
        {
            SetMaterial(data.material);
        }
    }

    private AnimationClip ResolveAnimationClip(
        SkillAnimationClipType clipType)
    {
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
            ? runtimeData.lifetime
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
