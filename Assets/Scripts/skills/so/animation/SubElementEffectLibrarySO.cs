

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SubElementEffectEntry
{
    [SerializeField] private ElementType element;

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

    public ElementType Element => element;
    public Sprite OverlaySprite => overlaySprite;
    public Material MaterialOverride => materialOverride;
    public float Intensity => intensity;
    public GameObject ParticlePrefab => particlePrefab;
    public GameObject TrailPrefab => trailPrefab;
    public bool EnableOverlay => enableOverlay;
    public bool EnableParticle => enableParticle;
    public bool EnableTrail => enableTrail;

    public bool Matches(ElementType targetElement)
    {
        return element == targetElement;
    }
}

/// <summary>
/// 메인 속성 외에 추가로 붙는 서브 속성 효과 라이브러리.
/// 혼합 속성 조합 시 메인 이미지를 교체하지 않고,
/// 오버레이 / 파티클 / 트레일 같은 보조 효과를 덧붙이기 위한 데이터만 보관한다.
/// </summary>
[CreateAssetMenu(fileName = "SubElementEffectLibrarySO", menuName = "BS/Skills/Visual/SubElementEffectLibrarySO")]
public class SubElementEffectLibrarySO : ScriptableObject
{
    [SerializeField] private List<SubElementEffectEntry> entries = new();

    public IReadOnlyList<SubElementEffectEntry> Entries => entries;

    public SubElementEffectEntry Get(ElementType element)
    {
        return entries.Find(entry => entry != null && entry.Matches(element));
    }
}