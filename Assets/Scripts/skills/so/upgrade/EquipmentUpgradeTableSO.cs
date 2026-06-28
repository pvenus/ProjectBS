using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 장비 레벨별 modifier 정의 테이블.
/// EquipmentUpgradeEntry 목록을 보관하고,
/// 현재 장비 레벨 이하의 entry들을 조회하는 역할을 한다.
/// </summary>
[CreateAssetMenu(fileName = "EquipmentUpgradeTableSO", menuName = "BS/Skills/Upgrade/EquipmentUpgradeTableSO")]
public class EquipmentUpgradeTableSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string upgradeTableId;

    [Header("Entries")]
    [SerializeField] private List<EquipmentUpgradeEntry> entries = new();

    public string UpgradeTableId => upgradeTableId;
    public IReadOnlyList<EquipmentUpgradeEntry> Entries => entries;

#if UNITY_EDITOR
    public void ApplyEditorData(
        string upgradeTableId,
        List<EquipmentUpgradeEntry> entries)
    {
        this.upgradeTableId = upgradeTableId;
        this.entries = entries ?? new List<EquipmentUpgradeEntry>();
    }
#endif
}