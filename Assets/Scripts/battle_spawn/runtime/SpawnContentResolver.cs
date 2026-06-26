using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public readonly struct SpawnRequest
{
    public SpawnContentSO Content { get; }
    public Vector3 AnchorPosition { get; }
    public float AnchorRotation { get; }

    public SpawnRequest(
        SpawnContentSO content,
        Vector3 anchorPosition,
        float anchorRotation)
    {
        Content = content;
        AnchorPosition = anchorPosition;
        AnchorRotation = anchorRotation;
    }
}

public static class SpawnContentResolver
{
    public static SpawnPlan Resolve(SpawnRequest request)
    {
        List<SpawnCommand> commands = new List<SpawnCommand>();
        SpawnTransform rootTransform = new SpawnTransform(request.AnchorPosition, request.AnchorRotation);

        if (request.Content is SpawnSquadSO squad)
        {
            AppendSquad(squad, rootTransform, 0f, commands);
        }
        else if (request.Content is SpawnFormationSO formation)
        {
            AppendFormation(formation, rootTransform, 0f, commands);
        }

        // 모든 Command를 생성한 후 StartTime 오름차순으로 정렬
        commands.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        return new SpawnPlan(commands);
    }

    private static void AppendSquad(
        SpawnSquadSO squad,
        SpawnTransform parentTransform,
        float startTimeOffset,
        List<SpawnCommand> commands)
    {
        if (squad == null || squad.Groups == null || squad.Groups.Count == 0) return;

        // Group들을 Order 기준으로 그룹화하고 정렬
        var orderedGroups = squad.Groups
            .GroupBy(g => g.Order)
            .OrderBy(g => g.Key)
            .ToList();

        float currentOrderStartTime = startTimeOffset;

        for (int orderIndex = 0; orderIndex < orderedGroups.Count; orderIndex++)
        {
            var orderGroup = orderedGroups[orderIndex];
            float currentOrderDuration = 0f;

            foreach (var group in orderGroup)
            {
                if (group == null || group.Pattern == null) continue;
                
                // 가상 또는 실제 슬롯 리스트 확보
                List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
                if (group.Pattern is FixedPatternSO fixedPat)
                {
                    slots.AddRange(fixedPat.GetSlots());
                }
                else if (group.Pattern is RandomPatternSO randPat)
                {
                    System.Random rand = new System.Random(randPat.name.GetHashCode() + group.Order);
                    int qty = group.Quantity;
                    for (int i = 0; i < qty; i++)
                    {
                        Vector2 localPos = Vector2.zero;
                        if (randPat.Shape == SpawnAreaShape.Circle)
                        {
                            double angle = rand.NextDouble() * Math.PI * 2;
                            double r = Math.Sqrt(rand.NextDouble()) * randPat.AreaSize.x;
                            localPos = new Vector2((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
                        }
                        else if (randPat.Shape == SpawnAreaShape.Rectangle)
                        {
                            float rx = (float)(rand.NextDouble() - 0.5) * randPat.AreaSize.x;
                            float ry = (float)(rand.NextDouble() - 0.5) * randPat.AreaSize.y;
                            localPos = new Vector2(rx, ry);
                        }
                        slots.Add(new SpawnPatternSlot(localPos, 0f));
                    }
                }

                if (slots.Count == 0) continue;

                // 1. Group Transform 합성 (부모 Transform 기준)
                SpawnTransform groupTransform = SpawnCoordinateUtility.Compose(
                    parentTransform, 
                    group.LocalOffset, 
                    group.LocalRotation
                );

                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (slot == null) continue;

                    float spawnTime = currentOrderStartTime + (i * group.SlotInterval);

                    // 2. Pattern Slot Transform 합성 (Group Transform 기준)
                    SpawnTransform finalTransform = SpawnCoordinateUtility.Compose(
                        groupTransform,
                        slot.LocalPosition,
                        slot.LocalRotation
                    );

                    commands.Add(new SpawnCommand(group.Character, finalTransform.Position, finalTransform.Rotation, spawnTime));
                }

                float groupDuration = Mathf.Max(0f, (slots.Count - 1) * group.SlotInterval);
                if (groupDuration > currentOrderDuration)
                {
                    currentOrderDuration = groupDuration;
                }
            }

            // 다음 Order의 시작 시점 계산
            bool isLastOrder = (orderIndex == orderedGroups.Count - 1);
            if (!isLastOrder)
            {
                currentOrderStartTime = currentOrderStartTime + currentOrderDuration + squad.GroupInterval;
            }
            else
            {
                currentOrderStartTime = currentOrderStartTime + currentOrderDuration;
            }
        }
    }

    private static void AppendFormation(
        SpawnFormationSO formation,
        SpawnTransform parentTransform,
        float startTimeOffset,
        List<SpawnCommand> commands)
    {
        if (formation == null || formation.Squad == null || formation.Pattern == null) return;

        // 가상 또는 실제 슬롯 리스트 확보
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        if (formation.Pattern is FixedPatternSO fixedPat)
        {
            slots.AddRange(fixedPat.GetSlots());
        }
        else if (formation.Pattern is RandomPatternSO randPat)
        {
            System.Random rand = new System.Random(randPat.name.GetHashCode());
            int qty = formation.Quantity;
            for (int i = 0; i < qty; i++)
            {
                Vector2 localPos = Vector2.zero;
                if (randPat.Shape == SpawnAreaShape.Circle)
                {
                    double angle = rand.NextDouble() * Math.PI * 2;
                    double r = Math.Sqrt(rand.NextDouble()) * randPat.AreaSize.x;
                    localPos = new Vector2((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
                }
                else if (randPat.Shape == SpawnAreaShape.Rectangle)
                {
                    float rx = (float)(rand.NextDouble() - 0.5) * randPat.AreaSize.x;
                    float ry = (float)(rand.NextDouble() - 0.5) * randPat.AreaSize.y;
                    localPos = new Vector2(rx, ry);
                }
                slots.Add(new SpawnPatternSlot(localPos, 0f));
            }
        }

        if (slots.Count == 0) return;

        for (int j = 0; j < slots.Count; j++)
        {
            var slot = slots[j];
            if (slot == null) continue;

            float squadStartTime = startTimeOffset + (j * formation.SlotInterval);

            // 포메이션 슬롯의 최종 World Transform 합성 (부모 Transform 기준)
            SpawnTransform formationSlotTransform = SpawnCoordinateUtility.Compose(
                parentTransform,
                slot.LocalPosition,
                slot.LocalRotation
            );

            // 하위 스쿼드 해석 및 추가 (포메이션 슬롯의 Transform을 부모 기준으로 전달)
            AppendSquad(formation.Squad, formationSlotTransform, squadStartTime, commands);
        }
    }
}
