using UnityEngine;

[CreateAssetMenu(
    fileName = "SkillAnimation",
    menuName = "BS/Skills/Animation/Skill Animation SO",
    order = 10)]
public class SkillAnimationSO : ScriptableObject
{
    [Header("Playback")]
    [SerializeField] private AnimationClip animationClip;
    [SerializeField] private Vector2 defaultDirection = Vector2.right;
    [SerializeField] private int sortingOrder;

    [Header("Renderer")]
    [SerializeField] private bool ensureSpriteRenderer = true;
    [SerializeField] private string sortingLayerName = "Default";

    public AnimationClip AnimationClip => animationClip;
    public Vector2 DefaultDirection => defaultDirection;
    public int SortingOrder => sortingOrder;
    public bool EnsureSpriteRenderer => ensureSpriteRenderer;
    public string SortingLayerName => sortingLayerName;

    public SkillProjectileVisualDto CreateVisualDto(Vector2? directionOverride = null)
    {
        Vector2 direction = directionOverride ?? defaultDirection;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.right;
        }

        return new SkillProjectileVisualDto
        {
            animationClip = animationClip,
            direction = direction.normalized,
            sortingOrder = sortingOrder,
            ensureSpriteRenderer = ensureSpriteRenderer,
            sortingLayerName = sortingLayerName,
        };
    }

    public void ApplyTo(SkillProjectileVisualMono visualMono, Vector2? directionOverride = null)
    {
        if (visualMono == null)
        {
            Debug.LogError("SkillProjectileVisualMono is null");
            return;
        }

        var dto = CreateVisualDto(directionOverride);
        visualMono.Initialize(dto);

        var sr = visualMono.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
        }
    }
}