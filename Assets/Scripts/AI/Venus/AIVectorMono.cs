using UnityEngine;

// DangerPlotVector is a subjective danger map felt by the NPC itself.
// immediate : danger directly felt as an immediate threat to self.
// linked    : danger internalized from others (e.g. ally crisis becoming my burden).
// ambient   : danger felt from battlefield instability or surrounding pressure.
[System.Serializable]
public struct DangerPlotVector
{
    [Range(0f, 100f)] public float immediate;
    [Range(0f, 100f)] public float linked;
    [Range(0f, 100f)] public float ambient;

    public DangerPlotVector(float immediate, float linked, float ambient)
    {
        this.immediate = immediate;
        this.linked = linked;
        this.ambient = ambient;
    }

    public float MaxValue => Mathf.Max(immediate, Mathf.Max(linked, ambient));
    public float Sum => immediate + linked + ambient;

    public DangerPlotVector Clamped(float min = 0f, float max = 100f)
    {
        return new DangerPlotVector(
            Mathf.Clamp(immediate, min, max),
            Mathf.Clamp(linked, min, max),
            Mathf.Clamp(ambient, min, max));
    }

    public DangerPlotVector Normalized01()
    {
        float max = MaxValue;
        if (max <= 0.0001f)
            return new DangerPlotVector(0f, 0f, 0f);

        return new DangerPlotVector(immediate / max, linked / max, ambient / max);
    }

    public Vector3 ToVector3()
    {
        return new Vector3(immediate, linked, ambient);
    }

    public float GetAxisValue(DangerAxis axis)
    {
        switch (axis)
        {
            case DangerAxis.Immediate:
                return immediate;
            case DangerAxis.Linked:
                return linked;
            case DangerAxis.Ambient:
                return ambient;
            default:
                return 0f;
        }
    }

    public DangerAxis GetDominantAxis()
    {
        if (immediate >= linked && immediate >= ambient)
            return DangerAxis.Immediate;

        if (linked >= immediate && linked >= ambient)
            return DangerAxis.Linked;

        return DangerAxis.Ambient;
    }

    public static DangerPlotVector operator +(DangerPlotVector a, DangerPlotVector b)
    {
        return new DangerPlotVector(a.immediate + b.immediate, a.linked + b.linked, a.ambient + b.ambient);
    }

    public static DangerPlotVector operator -(DangerPlotVector a, DangerPlotVector b)
    {
        return new DangerPlotVector(a.immediate - b.immediate, a.linked - b.linked, a.ambient - b.ambient);
    }

    public static DangerPlotVector operator *(DangerPlotVector a, float scalar)
    {
        return new DangerPlotVector(a.immediate * scalar, a.linked * scalar, a.ambient * scalar);
    }

    public override string ToString()
    {
        return $"(Immediate:{immediate:0.00}, Linked:{linked:0.00}, Ambient:{ambient:0.00})";
    }
}

public enum DangerAxis
{
    None,
    Immediate,
    Linked,
    Ambient
}

public class AIVectorMono : MonoBehaviour
{
    [Header("Subjective Danger Plot")]
    [SerializeField] private DangerPlotVector currentDanger;
    [SerializeField] private float maxDangerPerAxis = 100f;

    public DangerPlotVector CurrentDanger => currentDanger;

    public void SetDanger(DangerPlotVector value)
    {
        currentDanger = value.Clamped(0f, maxDangerPerAxis);
    }

    public void AddDanger(DangerPlotVector delta)
    {
        currentDanger = (currentDanger + delta).Clamped(0f, maxDangerPerAxis);
    }

    public void ReduceDanger(DangerPlotVector delta)
    {
        currentDanger = (currentDanger - delta).Clamped(0f, maxDangerPerAxis);
    }

    public void ClearDanger()
    {
        currentDanger = new DangerPlotVector(0f, 0f, 0f);
    }

    public float GetDanger(DangerAxis axis)
    {
        return currentDanger.GetAxisValue(axis);
    }

    public DangerAxis GetDominantDangerAxis()
    {
        if (currentDanger.MaxValue <= 0.0001f)
            return DangerAxis.None;

        return currentDanger.GetDominantAxis();
    }

    public DangerPlotVector GetNormalizedDanger01()
    {
        return currentDanger.Normalized01();
    }

    public Vector3 GetDangerVector3()
    {
        return currentDanger.ToVector3();
    }
}
