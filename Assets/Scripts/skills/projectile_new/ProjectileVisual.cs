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

    [Header("Optional Direct Clips")]
    [SerializeField] private AnimationClip spawnClip;
    [SerializeField] private AnimationClip hitClip;
    [SerializeField] private AnimationClip despawnClip;

    [Header("Debug")]
    [SerializeField] private bool initialized;
    [SerializeField] private bool isClipPlaying;
    [SerializeField] private bool deactivateAfterClipFinished;

    private ProjectileEntity owner;
    private ProjectileRuntimeData runtimeData;

    private PlayableGraph playableGraph;
    private AnimationClipPlayable clipPlayable;
    private AnimationClip currentClip;
    private double currentClipDuration;
    private bool graphCreated;

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
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (materialTargetRenderer == null)
        {
            materialTargetRenderer = GetComponentInChildren<Renderer>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

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

        if (!ShouldUseAnimatorTriggers() && spawnClip != null)
        {
            PlayClip(spawnClip);
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

        if (!ShouldUseAnimatorTriggers() && hitClip != null)
        {
            PlayClip(hitClip);
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

        if (!ShouldUseAnimatorTriggers() && despawnClip != null)
        {
            PlayClip(despawnClip, true);
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
        clipPlayable.SetSpeed(1d);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(playableGraph, "ProjectileVisualOutput", animator);
        output.SetSourcePlayable(clipPlayable);

        currentClip = clip;
        currentClipDuration = clip.length;
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

    public void SetSpawnClip(AnimationClip clip)
    {
        spawnClip = clip;
    }

    public void SetHitClip(AnimationClip clip)
    {
        hitClip = clip;
    }

    public void SetDespawnClip(AnimationClip clip)
    {
        despawnClip = clip;
    }

    public void SetMaterial(Material material)
    {
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

        spawnClip = data.spawnClip;
        hitClip = data.hitClip;
        despawnClip = data.despawnClip;

        if (data.sprite != null)
        {
            SetSprite(data.sprite);
        }

        SetColor(data.color);

        if (data.material != null)
        {
            SetMaterial(data.material);
        }
    }

    private bool ShouldUseAnimatorTriggers()
    {
        return runtimeData != null && runtimeData.useAnimatorTriggers;
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = color;
    }

    public void SetFlipX(bool flipX)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.flipX = flipX;
    }

    public void SetSortingOrder(int sortingOrder)
    {
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