using System.Collections.Generic;
using UnityEngine;
using Skill;
[System.Serializable]
public class ImpactVisualEntry
{
    [SerializeField] private ImpactType impactType;

    [Header("Effect")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private AudioClip sound;

    [Header("Optional")]
    [SerializeField] private float scale = 1f;
    [SerializeField] private float intensity = 1f;

    public ImpactType ImpactType => impactType;
    public GameObject EffectPrefab => effectPrefab;
    public AudioClip Sound => sound;
    public float Scale => scale;
    public float Intensity => intensity;
}
[CreateAssetMenu(fileName = "ImpactVisualSO", menuName = "BS/Skills/Visual/ImpactVisualSO")]
public class ImpactVisualSO : ScriptableObject
{
    [SerializeField] private List<ImpactVisualEntry> entries;
}