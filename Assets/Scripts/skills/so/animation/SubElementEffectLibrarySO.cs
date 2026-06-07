

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubElementEffectEntry
{
    [Header("Overlay / Accent")]
    [SerializeField] private Sprite overlaySprite;
    [SerializeField] private Material materialOverride;
    [SerializeField, Min(0f)] private float intensity = 1f;

    [Header("Effect Prefabs")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private GameObject trailPrefab;

    [Header("Flags")]
    [SerializeField] private bool enableOverlay = true;
    [SerializeField] private bool enableParticle = true;
    [SerializeField] private bool enableTrail = false;

    public Sprite OverlaySprite => overlaySprite;
    public Material MaterialOverride => materialOverride;
    public float Intensity => intensity;
    public GameObject ParticlePrefab => particlePrefab;
    public GameObject TrailPrefab => trailPrefab;
    public bool EnableOverlay => enableOverlay;
    public bool EnableParticle => enableParticle;
    public bool EnableTrail => enableTrail;
}