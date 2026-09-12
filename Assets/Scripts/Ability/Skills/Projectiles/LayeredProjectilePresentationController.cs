using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public sealed class LayeredProjectilePresentationController : MonoBehaviour
{
    private sealed class LayerRuntime
    {
        internal ProjectilePresentationLayerEntry entry;
        internal GameObject root;
        internal SpriteRenderer renderer;
        internal Animator animator;
        internal SkillAnimationVfxFeatureObject feature;
        internal PlayableGraph graph;
        internal AnimationClipPlayable playable;
        internal bool activated;
    }

    private readonly List<LayerRuntime> layers = new();
    private ProjectileRuntimeData data;
    private Vector3 worldAnchor;
    private float elapsed;
    private int generation;
    private int baseSortingOrder;
    private int sortingLayerId;
    public bool IsPlaying { get; private set; }
    public float RemainingTime { get; private set; }

    public bool Begin(LayeredProjectilePresentationProfileSO profile,
        SpriteRenderer reference, ProjectileRuntimeData runtimeData)
    {
        StopImmediate();
        if (profile == null || !profile.HasRequiredGroundField() || runtimeData == null) return false;
        generation++;
        data = runtimeData;
        worldAnchor = runtimeData.spawnPosition;
        elapsed = 0f;
        baseSortingOrder = reference != null ? reference.sortingOrder : 0;
        sortingLayerId = reference != null ? reference.sortingLayerID : 0;
        ProjectilePresentationLayerEntry[] entries = profile.Layers;
        for (int i = 0; i < entries.Length; i++)
        {
            ProjectilePresentationLayerEntry entry = entries[i];
            if (entry == null || !entry.HasPresentation || !entry.HasValidAnchor ||
                entry.Lifetime <= 0f) continue;
            layers.Add(CreateLayer(entry, reference, i));
        }
        if (layers.Count == 0) return false;
        RemainingTime = profile.ResolveMaximumLayerEnd();
        IsPlaying = true;
        TickLayers();
        return true;
    }

    public void SetBaseSortingOrder(int value)
    {
        baseSortingOrder = value;
        for (int i = 0; i < layers.Count; i++) ApplySorting(layers[i]);
    }

    private LayerRuntime CreateLayer(ProjectilePresentationLayerEntry entry,
        SpriteRenderer reference, int index)
    {
        GameObject root = new($"__ProjectilePresentationLayer_{index}_{entry.LayerId}");
        root.transform.SetParent(transform, false);
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.enabled = false;
        renderer.sortingLayerID = sortingLayerId;
        if (reference != null)
        {
            renderer.sprite = reference.sprite;
            renderer.color = reference.color;
            renderer.sharedMaterial = reference.sharedMaterial;
            renderer.flipX = reference.flipX;
            renderer.flipY = reference.flipY;
        }
        LayerRuntime layer = new() { entry = entry, root = root, renderer = renderer };
        ApplySorting(layer);
        return layer;
    }

    private void Update()
    {
        if (!IsPlaying) return;
        elapsed += Time.deltaTime;
        TickLayers();
    }

    private void LateUpdate()
    {
        if (!IsPlaying) return;
        for (int i = 0; i < layers.Count; i++)
        {
            LayerRuntime layer = layers[i];
            if (layer.root == null) continue;
            if (layer.entry.FollowCaster && data?.owner != null)
                layer.root.transform.position = data.owner.transform.position;
            else if (layer.entry.FollowTarget && data?.target != null)
                layer.root.transform.position = data.target.transform.position;
            else if (layer.entry.WorldFixed)
                layer.root.transform.position = worldAnchor;
        }
    }

    private void TickLayers()
    {
        RemainingTime = 0f;
        bool pending = false;
        for (int i = 0; i < layers.Count; i++)
        {
            LayerRuntime layer = layers[i];
            float localTime = elapsed - layer.entry.StartTime;
            RemainingTime = Mathf.Max(RemainingTime,
                layer.entry.StartTime + layer.entry.Lifetime - elapsed);
            if (localTime < 0f) { pending = true; continue; }
            // Keep a key authored exactly at the layer boundary visible for the
            // boundary render sample; retire it on the following update.
            if (localTime > layer.entry.Lifetime)
            {
                if (layer.renderer != null) layer.renderer.enabled = false;
                layer.feature?.StopImmediate();
                continue;
            }
            pending = true;
            if (!layer.activated) ActivateLayer(layer);
            if (layer.entry.Loop && layer.entry.Clip != null && layer.playable.IsValid() &&
                layer.entry.Clip.length > 0f)
            {
                layer.playable.SetTime(localTime % layer.entry.Clip.length);
                if (layer.graph.IsValid()) layer.graph.Evaluate(0f);
            }
        }
        if (!pending) StopImmediate();
    }

    private void ActivateLayer(LayerRuntime layer)
    {
        layer.activated = true;
        if (layer.renderer != null) layer.renderer.enabled = true;
        if (layer.entry.Profile != null && layer.renderer != null)
        {
            layer.feature = layer.root.AddComponent<SkillAnimationVfxFeatureObject>();
            layer.feature.Initialize(layer.renderer, layer.entry.Profile);
            layer.feature.Play();
        }
        if (layer.entry.Clip == null || layer.renderer == null) return;
        layer.animator = layer.root.AddComponent<Animator>();
        layer.graph = PlayableGraph.Create($"ProjectileLayer_{generation}_{layer.entry.LayerId}");
        layer.graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        layer.playable = AnimationClipPlayable.Create(layer.graph, layer.entry.Clip);
        layer.playable.SetApplyFootIK(false);
        layer.playable.SetApplyPlayableIK(false);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(layer.graph, "LayerOutput", layer.animator);
        output.SetSourcePlayable(layer.playable);
        layer.graph.Play();
        layer.graph.Evaluate(0f);
    }

    private void ApplySorting(LayerRuntime layer)
    {
        if (layer?.renderer == null) return;
        layer.renderer.sortingLayerID = sortingLayerId;
        layer.renderer.sortingOrder = layer.entry.SortingRelation == Skill.SkillSortingRelation.AbsoluteTop
            ? (int)Skill.SkillSortingRelation.AbsoluteTop + layer.entry.SortingOffset
            : baseSortingOrder + (int)layer.entry.SortingRelation + layer.entry.SortingOffset;
        layer.feature?.SetSortingOrder(layer.renderer.sortingOrder);
    }

    public void StopImmediate()
    {
        generation++;
        IsPlaying = false;
        RemainingTime = 0f;
        data = null;
        for (int i = 0; i < layers.Count; i++)
        {
            LayerRuntime layer = layers[i];
            layer.feature?.StopImmediate();
            if (layer.graph.IsValid()) layer.graph.Destroy();
            if (layer.root != null) Destroy(layer.root);
        }
        layers.Clear();
    }

    private void OnDisable() => StopImmediate();
    private void OnDestroy() => StopImmediate();
}
