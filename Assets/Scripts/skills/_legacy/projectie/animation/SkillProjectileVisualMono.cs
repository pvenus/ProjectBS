using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class SkillProjectileVisualMono : MonoBehaviour
{
    [Header("Optional Auto Play")]
    [SerializeField] private bool playOnStart;
    [SerializeField] private SkillAnimationSO animationConfig;
    [SerializeField] private Vector2 directionOverride = Vector2.right;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private PlayableGraph _playableGraph;
    private bool _hasPlayableGraph;
    private AnimationClip _currentClip;
    private bool _hideOnNonLoopClipFinished = true;
    private void Update()
    {
        if (!_hasPlayableGraph || _currentClip == null)
            return;

        if (_currentClip.isLooping)
            return;

        if (_playableGraph.IsValid() && _playableGraph.IsPlaying())
        {
            double time = _playableGraph.GetRootPlayable(0).GetTime();
            if (time >= _currentClip.length)
            {
                StopCurrentPlayable();

                if (_hideOnNonLoopClipFinished && _spriteRenderer != null)
                {
                    _spriteRenderer.sprite = null;
                    _spriteRenderer.enabled = false;
                }
            }
        }
    }
    private Vector2 _currentDirection = Vector2.right;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;
    public Vector2 CurrentDirection => _currentDirection;
    public SkillAnimationSO CurrentAnimationConfig => animationConfig;

    private void Awake()
    {
        PrepareComponents(null);
    }

    private void Start()
    {
        if (!playOnStart || animationConfig == null)
            return;

        Vector2? overrideDir = directionOverride.sqrMagnitude <= 0.0001f ? null : directionOverride;
        Initialize(animationConfig, overrideDir);
    }

    public void Initialize(SkillAnimationSO config, Vector2? directionOverride = null)
    {
        if (config == null)
        {
            Debug.LogError("SkillAnimationSO is null", this);
            return;
        }

        animationConfig = config;
        Initialize(config.CreateVisualDto(directionOverride));
    }

    public void Initialize(SkillProjectileVisualDto dto)
    {
        if (dto == null)
        {
            Debug.LogError("SkillProjectileVisualDto is null", this);
            return;
        }

        PrepareComponents(dto);

        _currentDirection = dto.direction.sqrMagnitude <= 0.0001f ? Vector2.right : dto.direction.normalized;

        ApplyVisualSettings(dto);

        Play(dto.animationClip);
        _isInitialized = true;
    }

    public void Play(AnimationClip clip)
    {
        if (clip == null)
            return;

        PrepareComponents(null);
        _currentClip = clip;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = true;
        }

        if (_animator == null)
        {
            Debug.LogWarning("Animator could not be prepared for SkillProjectileVisualMono.", this);
            return;
        }

        StopCurrentPlayable();

        _playableGraph = PlayableGraph.Create($"{name}_ProjectileVisualGraph");
        var output = AnimationPlayableOutput.Create(_playableGraph, "ProjectileVisual", _animator);
        var clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip);
        output.SetSourcePlayable(clipPlayable);
        _playableGraph.Play();
        _hasPlayableGraph = true;
    }

    public void Stop()
    {
        StopCurrentPlayable();
        _currentClip = null;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = null;
            _spriteRenderer.enabled = false;
        }
    }

    public void RestartCurrentClip()
    {
        if (_currentClip == null)
            return;

        Play(_currentClip);
    }

    public void UpdateDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        _currentDirection = direction.normalized;
    }

    public void SetSortingOrder(int order)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = order;
        }
    }

    private void PrepareComponents(SkillProjectileVisualDto dto)
    {
        bool ensureRenderer = dto?.ensureSpriteRenderer ?? animationConfig?.EnsureSpriteRenderer ?? true;

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_spriteRenderer == null && ensureRenderer)
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_animator == null)
        {
            _animator = gameObject.AddComponent<Animator>();
        }
    }

    private void ApplyVisualSettings(SkillProjectileVisualDto dto)
    {
        if (dto == null)
            return;


        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingLayerName = dto.sortingLayerName;
            _spriteRenderer.sortingOrder = dto.sortingOrder;
        }
    }

    private void StopCurrentPlayable()
    {
        if (!_hasPlayableGraph)
            return;

        _playableGraph.Destroy();
        _hasPlayableGraph = false;
        _currentClip = null;
    }

    private void OnDisable()
    {
        StopCurrentPlayable();
    }

    private void OnDestroy()
    {
        StopCurrentPlayable();
    }
}