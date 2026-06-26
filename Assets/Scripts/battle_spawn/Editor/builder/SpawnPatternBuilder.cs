#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class JsonSpawnPatternSlot
{
    public Vector2 localPosition;
    public float rotation;
}

[Serializable]
public class JsonSpawnPattern
{
    public string patternId;
    public string patternType; // "FixedPosition" or "RangeRandom"
    public string displayName;
    
    public List<JsonSpawnPatternSlot> positions;
    
    public string shape; // "Circle" or "Rectangle"
    public Vector2 areaSize;
    public int quantity; // 레거시 지원용 (새 RandomPatternSO는 사용하지 않음)

    public float rotation;
    public float scale;
}

public static class SpawnPatternBuilder
{
    public static SpawnPattern Build(JsonSpawnPattern data, string baseDir)
    {
        if (string.IsNullOrEmpty(data.patternId)) return null;

        bool isFixedType = (data.patternType == "FixedPosition" || string.IsNullOrEmpty(data.patternType));
        string assetPath = $"{baseDir}/{data.patternId}.asset";

        if (isFixedType)
        {
            FixedPatternSO asset = AssetDatabase.LoadAssetAtPath<FixedPatternSO>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<FixedPatternSO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            List<SpawnPatternSlot> finalSlots = new List<SpawnPatternSlot>();
            float rotationVal = data.rotation;
            float scaleVal = data.scale <= 0f ? 1f : data.scale;

            if (data.positions != null)
            {
                foreach (var pos in data.positions)
                {
                    Vector2 rawPos = pos.localPosition;
                    Vector2 scaled = rawPos * scaleVal;
                    Vector2 rotated = SpawnCoordinateUtility.Rotate(scaled, rotationVal);

                    float finalSlotRot = pos.rotation + rotationVal;
                    finalSlotRot = (finalSlotRot % 360f + 360f) % 360f;

                    finalSlots.Add(new SpawnPatternSlot(rotated, finalSlotRot));
                }
            }

            asset.Initialize(data.patternId, data.displayName, finalSlots);
            EditorUtility.SetDirty(asset);
            return asset;
        }
        else
        {
            RandomPatternSO asset = AssetDatabase.LoadAssetAtPath<RandomPatternSO>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<RandomPatternSO>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            SpawnAreaShape shapeVal = SpawnAreaShape.Circle;
            if (!string.IsNullOrEmpty(data.shape) && data.shape.Equals("Rectangle", StringComparison.OrdinalIgnoreCase)) 
            {
                shapeVal = SpawnAreaShape.Rectangle;
            }

            float scaleVal = data.scale <= 0f ? 1f : data.scale;
            Vector2 scaledAreaSize = data.areaSize * scaleVal;

            asset.Initialize(data.patternId, data.displayName, shapeVal, scaledAreaSize);
            EditorUtility.SetDirty(asset);
            return asset;
        }
    }
}
#endif
