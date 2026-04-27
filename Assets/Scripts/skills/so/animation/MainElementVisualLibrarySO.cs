

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 메인 속성 비주얼 라이브러리 허브.
/// 실제 데이터는 속성별 Group SO로 분리해서 관리하고,
/// 이 객체는 각 속성 그룹을 찾아 다시 조회하는 역할만 담당한다.
/// </summary>
[CreateAssetMenu(fileName = "MainElementVisualLibrarySO", menuName = "BS/Skills/Visual/MainElementVisualLibrarySO")]
public class MainElementVisualLibrarySO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private List<MainElementVisualGroupSO> elementGroups = new();

    public IReadOnlyList<MainElementVisualGroupSO> ElementGroups => elementGroups;
    public MainElementVisualGroupSO GetGroup(ElementType element)
    {
        return elementGroups.Find(group => group != null && group.Element == element);
    }
    public MainElementVisualEntry Get(ElementType element, EquipmentGrade grade)
    {
        MainElementVisualGroupSO group = GetGroup(element);
        return group != null ? group.Get(grade) : null;
    }
}