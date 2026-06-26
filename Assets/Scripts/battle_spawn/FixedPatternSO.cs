using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FixedPattern", menuName = "BS/Spawn/FixedPattern")]
public class FixedPatternSO : SpawnPattern
{
    [SerializeField] private List<SpawnPatternSlot> slots = new List<SpawnPatternSlot>();

    public List<SpawnPatternSlot> Slots => slots;

    public void Initialize(string id, string name, List<SpawnPatternSlot> slotsVal)
    {
        InitializeBase(id, name);
        this.slots = slotsVal ?? new List<SpawnPatternSlot>();
    }

    public override List<SpawnPatternSlot> GetSlots()
    {
        return slots;
    }

    public override void ScaleAreaSize(float scale)
    {
        if (slots == null || slots.Count == 0) return;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;
            slots[i] = new SpawnPatternSlot(slots[i].LocalPosition * scale, slots[i].LocalRotation);
        }
    }

    public void ApplyModifiers(float modifyRotation, float modifyScale, LookDirectionType lookDirType, bool flipDirection)
    {
        if (slots == null || slots.Count == 0) return;

        List<SpawnPatternSlot> newSlots = new List<SpawnPatternSlot>();
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            Vector2 originalPos = slot.LocalPosition;

            // 1. 스케일 적용
            Vector2 scaled = originalPos * modifyScale;

            // 2. 회전 적용
            Vector2 rotated = SpawnCoordinateUtility.Rotate(scaled, modifyRotation);

            // 3. 방향 적용
            float finalRot = slot.LocalRotation + modifyRotation;
            if (lookDirType != LookDirectionType.AxisY || flipDirection)
            {
                finalRot = SpawnCoordinateUtility.CalculateDirectionAngle(rotated, lookDirType, flipDirection);
            }
            finalRot = (finalRot % 360f + 360f) % 360f;

            newSlots.Add(new SpawnPatternSlot(rotated, finalRot));
        }

        slots = newSlots;
    }
}
