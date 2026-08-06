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

        if (squad.HasFormationPattern)
        {
            List<SpawnPatternSlot> formationSlots = ResolveSlots(
                squad.FormationPattern,
                0,
                squad.FormationQuantity);

            for (int i = 0; i < formationSlots.Count; i++)
            {
                SpawnPatternSlot slot = formationSlots[i];
                if (slot == null) continue;

                SpawnTransform squadTransform = SpawnCoordinateUtility.Compose(
                    parentTransform,
                    slot.LocalPosition,
                    slot.LocalRotation);

                float squadStartTime = startTimeOffset + (i * squad.FormationSlotInterval);
                AppendSquadGroups(squad, squadTransform, squadStartTime, commands);
            }

            return;
        }

        AppendSquadGroups(squad, parentTransform, startTimeOffset, commands);
    }

    private static void AppendSquadGroups(
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
                if (group == null) continue;
                
                int quantity = ResolveQuantity(squad, group);
                float slotInterval = ResolveSlotInterval(squad, group);
                List<SpawnPatternSlot> slots = ResolveSlots(group, quantity);

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

                    float spawnTime = currentOrderStartTime + (i * slotInterval);

                    // 2. Pattern Slot Transform 합성 (Group Transform 기준)
                    SpawnTransform finalTransform = SpawnCoordinateUtility.Compose(
                        groupTransform,
                        slot.LocalPosition,
                        slot.LocalRotation
                    );

                    commands.Add(new SpawnCommand(
                        group.SpawnUnitKey,
                        group.SpawnRole,
                        finalTransform.Position,
                        finalTransform.Rotation,
                        spawnTime));
                }

                float groupDuration = Mathf.Max(0f, (slots.Count - 1) * slotInterval);
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

    private static int ResolveQuantity(SpawnSquadSO squad, SpawnSquadGroup group)
    {
        if (group != null && group.Quantity > 0)
        {
            return group.Quantity;
        }

        return Mathf.Max(1, squad != null ? squad.Quantity : 1);
    }

    private static float ResolveSlotInterval(SpawnSquadSO squad, SpawnSquadGroup group)
    {
        if (group != null && group.SlotInterval > 0f)
        {
            return group.SlotInterval;
        }

        return Mathf.Max(0f, squad != null ? squad.SlotInterval : 0f);
    }

    private static List<SpawnPatternSlot> ResolveSlots(SpawnSquadGroup group, int quantity)
    {
        if (group == null || !group.HasPattern)
        {
            return ResolveNoneSlots(quantity);
        }

        return ResolveSlots(
            group.PatternKind,
            group.PatternId,
            group.FixedConfig,
            group.RandomConfig,
            group.Order,
            quantity);
    }

    private static List<SpawnPatternSlot> ResolveSlots(SpawnPatternData pattern, int seedOffset, int quantity)
    {
        if (pattern == null || !pattern.HasPattern)
        {
            return ResolveNoneSlots(quantity);
        }

        return ResolveSlots(
            pattern.PatternKind,
            pattern.PatternId,
            pattern.FixedConfig,
            pattern.RandomConfig,
            seedOffset,
            quantity);
    }

    private static List<SpawnPatternSlot> ResolveNoneSlots(int quantity)
    {
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        int safeQuantity = Mathf.Max(1, quantity);
        for (int i = 0; i < safeQuantity; i++)
        {
            slots.Add(new SpawnPatternSlot(Vector2.zero, 0f));
        }
        return slots;
    }

    private static List<SpawnPatternSlot> ResolveSlots(
        SpawnPatternKind patternKind,
        string patternId,
        FixedSpawnPatternConfig fixedConfig,
        RandomSpawnPatternConfig randomConfig,
        int seedOffset,
        int quantity)
    {
        List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();
        int safeQuantity = Mathf.Max(1, quantity);

        if (patternKind.IsFixedSlotKind())
        {
            IReadOnlyList<SpawnPatternSlot> fixedSlots = fixedConfig != null ? fixedConfig.Slots : null;
            if (fixedSlots == null || fixedSlots.Count == 0)
            {
                return slots;
            }

            int spawnCount = quantity > 1 ? safeQuantity : fixedSlots.Count;
            for (int i = 0; i < spawnCount; i++)
            {
                slots.Add(fixedSlots[i % fixedSlots.Count]);
            }
            return slots;
        }

        if (patternKind.IsRandomAreaKind())
        {
            if (randomConfig == null)
            {
                return slots;
            }

            int seed = string.IsNullOrEmpty(patternId) ? 0 : patternId.GetHashCode();
            System.Random rand = new System.Random(seed + seedOffset);
            for (int i = 0; i < safeQuantity; i++)
            {
                Vector2 localPos = Vector2.zero;
                SpawnAreaShape shape = patternKind.ResolveAreaShape(randomConfig.Shape);
                if (shape == SpawnAreaShape.Circle)
                {
                    double angle = rand.NextDouble() * Math.PI * 2;
                    double r = Math.Sqrt(rand.NextDouble()) * randomConfig.AreaSize.x;
                    localPos = new Vector2((float)(Math.Cos(angle) * r), (float)(Math.Sin(angle) * r));
                }
                else if (shape == SpawnAreaShape.Rectangle)
                {
                    float rx = (float)(rand.NextDouble() - 0.5) * randomConfig.AreaSize.x;
                    float ry = (float)(rand.NextDouble() - 0.5) * randomConfig.AreaSize.y;
                    localPos = new Vector2(rx, ry);
                }
                slots.Add(new SpawnPatternSlot(localPos, 0f));
            }
        }

        return slots;
    }

}
