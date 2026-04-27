using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비에 장착 가능한 룬 원형 데이터.
/// 룬 하나는 여러 스탯 modifier와 여러 Effect를 가질 수 있다.
/// </summary>
[CreateAssetMenu(fileName = "RuneSO", menuName = "BS/Skills/Rune/RuneSO")]
public class RuneSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string runeId;
    [SerializeField] private string displayName;
    [TextArea]
    [SerializeField] private string description;

    [Header("Classification")]
    [SerializeField] private ElementType elementType = ElementType.None;
    [SerializeField] private EquipmentGrade grade = EquipmentGrade.Common;
    [SerializeField] private List<string> tags = new();

    [Header("Runtime Modifiers")]
    [SerializeField] private List<SkillStatModifierRuntimeData> statModifiers = new();

    [Header("Effects")]
    [SerializeField] private List<SkillEffectSO> effects = new();

    public string RuneId => runeId;
    public string DisplayName => displayName;
    public string Description => description;

    public ElementType ElementType => elementType;
    public EquipmentGrade Grade => grade;
    public IReadOnlyList<string> Tags => tags;

    public IReadOnlyList<SkillStatModifierRuntimeData> StatModifiers => statModifiers;
    public IReadOnlyList<SkillEffectSO> Effects => effects;

    public bool HasAnyModifier => statModifiers != null && statModifiers.Count > 0;
    public bool HasAnyEffect => effects != null && effects.Count > 0;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(runeId))
        {
            runeId = name;
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = name;
        }
    }
#endif
}
