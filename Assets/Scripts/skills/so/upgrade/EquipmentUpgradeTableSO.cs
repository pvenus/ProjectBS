using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 장비 등급별 modifier 정의 테이블.
/// EquipmentUpgradeEntry 목록을 보관하고,
/// 현재 장비 등급 이하의 entry들을 조회하는 역할을 한다.
/// </summary>
[CreateAssetMenu(fileName = "EquipmentUpgradeTableSO", menuName = "BS/Skills/Upgrade/EquipmentUpgradeTableSO")]
public class EquipmentUpgradeTableSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string upgradeTableId;
    [SerializeField] private string displayName;

    [Header("Entries")]
    [SerializeField] private List<EquipmentUpgradeEntry> entries = new();

    public string UpgradeTableId => upgradeTableId;
    public string DisplayName => displayName;
    public IReadOnlyList<EquipmentUpgradeEntry> Entries => entries;

    public EquipmentUpgradeEntry GetEntry(EquipmentGrade grade)
    {
        if (entries == null || entries.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            EquipmentUpgradeEntry entry = entries[i];
            if (entry != null && entry.Grade == grade)
            {
                return entry;
            }
        }

        return null;
    }

    public List<EquipmentUpgradeEntry> GetEntriesUpToGrade(EquipmentGrade grade)
    {
        var result = new List<EquipmentUpgradeEntry>();

        if (entries == null || entries.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            EquipmentUpgradeEntry entry = entries[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.Grade <= grade)
            {
                result.Add(entry);
            }
        }

        return result;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(upgradeTableId))
        {
            upgradeTableId = name;
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = name;
        }
    }
#endif
}