using System;
using System.Collections.Generic;
using UnityEngine;
using Character;

[Serializable]
public sealed class PlatoonPosition
{
    [SerializeField] private int groupId;
    [SerializeField] private Vector2 localPosition;

    public PlatoonPosition(int groupId, Vector2 localPosition)
    {
        this.groupId = groupId;
        this.localPosition = localPosition;
    }

    public int GroupId => groupId;
    public Vector2 LocalPosition => localPosition;
}

[Serializable]
public sealed class PlatoonGroup
{
    [SerializeField] private int groupId;
    [SerializeField] private CharacterSO monster;

    public PlatoonGroup(int groupId, CharacterSO monster)
    {
        this.groupId = groupId;
        this.monster = monster;
    }

    public int GroupId => groupId;
    public CharacterSO Monster => monster;
}

[CreateAssetMenu(fileName = "Platoon", menuName = "BS/Spawn/Platoon")]
public class PlatoonSO : ScriptableObject
{
    [SerializeField] private string platoonId;
    [SerializeField] private string displayName;
    [SerializeField] private List<PlatoonPosition> positions = new List<PlatoonPosition>();
    [SerializeField] private List<PlatoonGroup> groups = new List<PlatoonGroup>();

    public string PlatoonId => platoonId;
    public string DisplayName => displayName;
    public IReadOnlyList<PlatoonPosition> Positions => positions;
    public IReadOnlyList<PlatoonGroup> Groups => groups;

    public CharacterSO GetMonster(int groupId)
    {
        if (groups == null) return null;
        for (int i = 0; i < groups.Count; i++)
        {
            if (groups[i] != null && groups[i].GroupId == groupId)
            {
                return groups[i].Monster;
            }
        }
        return null;
    }

    public Dictionary<int, CharacterSO> GetGroupsDictionary()
    {
        Dictionary<int, CharacterSO> dict = new Dictionary<int, CharacterSO>();
        if (groups == null) return dict;

        for (int i = 0; i < groups.Count; i++)
        {
            PlatoonGroup group = groups[i];
            if (group == null) continue;
            if (!dict.ContainsKey(group.GroupId))
            {
                dict.Add(group.GroupId, group.Monster);
            }
        }
        return dict;
    }

    public List<string> Validate()
    {
        List<string> errors = new List<string>();

        if (positions == null || positions.Count == 0)
        {
            errors.Add("Position 목록이 비어 있습니다.");
        }

        if (groups == null || groups.Count == 0)
        {
            errors.Add("Group 목록이 비어 있습니다.");
        }

        HashSet<int> definedGroupIds = new HashSet<int>();
        HashSet<int> duplicatedGroupIds = new HashSet<int>();

        if (groups != null)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                PlatoonGroup group = groups[i];
                if (group == null)
                {
                    errors.Add("Group 목록에 null 요소가 포함되어 있습니다.");
                    continue;
                }

                if (group.GroupId < 0)
                {
                    errors.Add($"음수 groupId가 존재합니다: {group.GroupId}");
                }

                if (group.Monster == null)
                {
                    errors.Add($"Group (ID: {group.GroupId})의 몬스터 참조가 비어 있습니다.");
                }

                if (!definedGroupIds.Add(group.GroupId))
                {
                    duplicatedGroupIds.Add(group.GroupId);
                }
            }
        }

        foreach (int dupId in duplicatedGroupIds)
        {
            errors.Add($"Group 목록에 동일한 groupId가 중복되어 존재합니다: {dupId}");
        }

        HashSet<int> usedGroupIds = new HashSet<int>();
        if (positions != null)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                PlatoonPosition pos = positions[i];
                if (pos == null)
                {
                    errors.Add("Position 목록에 null 요소가 포함되어 있습니다.");
                    continue;
                }

                if (pos.GroupId < 0)
                {
                    errors.Add($"Position의 groupId가 음수입니다: {pos.GroupId}");
                }

                if (!definedGroupIds.Contains(pos.GroupId))
                {
                    errors.Add($"Position이 참조하는 groupId가 Group 목록에 존재하지 않습니다: {pos.GroupId}");
                }
                else
                {
                    usedGroupIds.Add(pos.GroupId);
                }
            }
        }

        if (groups != null)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                PlatoonGroup group = groups[i];
                if (group != null && !usedGroupIds.Contains(group.GroupId))
                {
                    errors.Add($"[Warning] 사용되지 않는 Group이 존재합니다: {group.GroupId}");
                }
            }
        }

        return errors;
    }
}
