using System;
using UnityEngine;
using Skill;

public enum ProjectilePresentationLayerRole { GroundField, PullFlow, SetupPulse, Residue }

[Serializable]
public sealed class ProjectilePresentationLayerEntry
{
    [SerializeField] private string layerId;
    [SerializeField] private ProjectilePresentationLayerRole role;
    [SerializeField] private bool required;
    [SerializeField] private AnimationClip clip;
    [SerializeField] private SkillAnimationVfxProfileSO profile;
    [SerializeField, Min(0f)] private float startTime;
    [SerializeField, Min(0f)] private float lifetime;
    [SerializeField] private bool loop;
    [SerializeField] private SkillSortingRelation sortingRelation = SkillSortingRelation.BelowOwner;
    [SerializeField] private int sortingOffset;
    [SerializeField] private bool worldFixed = true;
    [SerializeField] private bool followCaster;
    [SerializeField] private bool followTarget;

    public string LayerId => layerId;
    public ProjectilePresentationLayerRole Role => role;
    public bool Required => required;
    public AnimationClip Clip => clip;
    public SkillAnimationVfxProfileSO Profile => profile;
    public float StartTime => Mathf.Max(0f, startTime);
    public float Lifetime => Mathf.Max(0f, lifetime);
    public bool Loop => loop;
    public SkillSortingRelation SortingRelation => sortingRelation;
    public int SortingOffset => sortingOffset;
    public bool WorldFixed => worldFixed;
    public bool FollowCaster => followCaster;
    public bool FollowTarget => followTarget;
    public bool HasPresentation => clip != null || profile != null;
    public bool HasValidAnchor => (worldFixed ? 1 : 0) + (followCaster ? 1 : 0) +
        (followTarget ? 1 : 0) == 1;

#if UNITY_EDITOR
    public void ApplyEditorData(string id, ProjectilePresentationLayerRole layerRole,
        bool isRequired, AnimationClip animationClip, float begin, float duration,
        bool shouldLoop, SkillSortingRelation relation, int orderOffset)
    {
        layerId = id;
        role = layerRole;
        required = isRequired;
        clip = animationClip;
        profile = null;
        startTime = begin;
        lifetime = duration;
        loop = shouldLoop;
        sortingRelation = relation;
        sortingOffset = orderOffset;
        worldFixed = true;
        followCaster = false;
        followTarget = false;
    }
#endif
}

[CreateAssetMenu(fileName = "LayeredProjectilePresentationProfile",
    menuName = "BS/Skills/Visual/Layered Projectile Presentation Profile")]
public sealed class LayeredProjectilePresentationProfileSO : ScriptableObject
{
    [SerializeField] private string schemaVersion = "projectile_layers_v1";
    [SerializeField] private string profileId;
    [SerializeField] private ProjectilePresentationLayerEntry[] layers;
    [Header("Purpose combo phase hooks")]
    [SerializeField, Min(0f)] private float contactTime = .25f;
    [SerializeField, Min(0f)] private float sweetOpen = .8f;
    [SerializeField, Min(0f)] private float sweetClose = 1.1f;
    [SerializeField, Min(0f)] private float sustainEnd = 1.5f;

    public string SchemaVersion => schemaVersion;
    public string ProfileId => profileId;
    public ProjectilePresentationLayerEntry[] Layers => layers;
    public float ContactTime => Mathf.Max(0f, contactTime);
    public float SweetOpen => Mathf.Max(ContactTime, sweetOpen);
    public float SweetClose => Mathf.Max(SweetOpen, sweetClose);
    public float SustainEnd => Mathf.Max(SweetClose, sustainEnd);

    public bool HasRequiredGroundField()
    {
        if (schemaVersion != "projectile_layers_v1" || layers == null) return false;
        for (int i = 0; i < layers.Length; i++)
        {
            ProjectilePresentationLayerEntry entry = layers[i];
            if (entry != null && entry.Role == ProjectilePresentationLayerRole.GroundField &&
                entry.Required && entry.HasPresentation && entry.HasValidAnchor &&
                entry.Lifetime > 0f) return true;
        }
        return false;
    }

    public float ResolveMaximumLayerEnd()
    {
        if (!HasRequiredGroundField()) return 0f;
        float result = 0f;
        for (int i = 0; i < layers.Length; i++)
        {
            ProjectilePresentationLayerEntry entry = layers[i];
            if (entry != null && entry.HasPresentation)
                result = Mathf.Max(result, entry.StartTime + entry.Lifetime);
        }
        return result;
    }

#if UNITY_EDITOR
    public void ApplyEditorData(string id, ProjectilePresentationLayerEntry[] entries,
        float contact, float open, float close, float end)
    {
        schemaVersion = "projectile_layers_v1";
        profileId = id;
        layers = entries;
        contactTime = contact;
        sweetOpen = open;
        sweetClose = close;
        sustainEnd = end;
    }
#endif
}
