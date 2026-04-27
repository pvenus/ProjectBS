using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImpactVisualEntry
{
    [SerializeField] private ElementType element;
    [SerializeField] private ImpactType impactType;

    [Header("Effect")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private AudioClip sound;

    [Header("Optional")]
    [SerializeField] private float scale = 1f;
    [SerializeField] private float intensity = 1f;

    public ElementType Element => element;
    public ImpactType ImpactType => impactType;
    public GameObject EffectPrefab => effectPrefab;
    public AudioClip Sound => sound;
    public float Scale => scale;
    public float Intensity => intensity;

    public bool Matches(ElementType targetElement, ImpactType targetType)
    {
        return element == targetElement && impactType == targetType;
    }
}
[CreateAssetMenu(fileName = "ImpactVisualSO", menuName = "BS/Skills/Visual/ImpactVisualSO")]
public class ImpactVisualSO : ScriptableObject
{
    [SerializeField] private List<ImpactVisualEntry> entries;

    public ImpactVisualEntry Get(ElementType element, ImpactType type)
    {
        return entries.Find(e => e.Matches(element, type));
    }
}