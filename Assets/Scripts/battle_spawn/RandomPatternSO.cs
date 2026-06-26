using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomPattern", menuName = "BS/Spawn/RandomPattern")]
public class RandomPatternSO : SpawnPattern
{
    [SerializeField] private SpawnAreaShape shape = SpawnAreaShape.Circle;
    [SerializeField] private Vector2 areaSize = new Vector2(1f, 1f); // Circle: X = radius. Rectangle: X = width, Y = height.

    public SpawnAreaShape Shape => shape;
    public Vector2 AreaSize => areaSize;

    public void Initialize(string id, string name, SpawnAreaShape shapeVal, Vector2 areaSizeVal)
    {
        InitializeBase(id, name);
        shape = shapeVal;
        areaSize = areaSizeVal;
    }

    public override List<SpawnPatternSlot> GetSlots()
    {
        // Random은 정적 슬롯을 갖지 않음
        return new List<SpawnPatternSlot>();
    }

    public override void ScaleAreaSize(float scale)
    {
        areaSize = areaSize * scale;
    }
}
