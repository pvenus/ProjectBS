using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MainElementVisualEntry
{
    [SerializeField] private EquipmentGrade grade;

    [Header("Visual Override")]
    [SerializeField] private AnimationClip attackClipOverride;
    [SerializeField] private AnimationClip projectileClipOverride;
    [SerializeField] private Sprite mainSprite;
    [SerializeField] private Material mainMaterial;

    public EquipmentGrade Grade => grade;
    public AnimationClip AttackClipOverride => attackClipOverride;
    public AnimationClip ProjectileClipOverride => projectileClipOverride;
    public Sprite MainSprite => mainSprite;
    public Material MainMaterial => mainMaterial;

    public bool Matches(EquipmentGrade targetGrade)
    {
        return grade == targetGrade;
    }
}

/// <summary>
/// 속성 하나(ElementType 1개) + 무기 타입 하나(AttackArchetype 1개)에 대한 메인 비주얼 묶음.
/// 예: Fire + Ranged 전용, Ice + Magic 전용.
/// 내부에서는 등급별 비주얼 엔트리를 관리한다.
/// </summary>
[CreateAssetMenu(fileName = "MainElementVisualGroupSO", menuName = "BS/Skills/Visual/MainElementVisualGroupSO")]
public class MainElementVisualGroupSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private ElementType element;
    [SerializeField] private AttackArchetype archetype;

    [Header("Entries")]
    [SerializeField] private List<MainElementVisualEntry> entries = new();

    public ElementType Element => element;
    public AttackArchetype Archetype => archetype;
    public IReadOnlyList<MainElementVisualEntry> Entries => entries;

    public MainElementVisualEntry Get(EquipmentGrade grade)
    {
        return entries.Find(entry => entry != null && entry.Matches(grade));
    }
}