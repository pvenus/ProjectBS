

using UnityEngine;

[System.Serializable]
public struct DangerPerceptionWeights
{
    [Min(0f)] public float immediate;
    [Min(0f)] public float linked;
    [Min(0f)] public float ambient;

    public DangerPerceptionWeights(float immediate, float linked, float ambient)
    {
        this.immediate = immediate;
        this.linked = linked;
        this.ambient = ambient;
    }

    public static DangerPerceptionWeights One => new DangerPerceptionWeights(1f, 1f, 1f);

    public DangerPlotVector Apply(DangerPlotVector value)
    {
        return new DangerPlotVector(
            value.immediate * immediate,
            value.linked * linked,
            value.ambient * ambient);
    }

    public override string ToString()
    {
        return $"(Immediate:{immediate:0.00}, Linked:{linked:0.00}, Ambient:{ambient:0.00})";
    }
}

[CreateAssetMenu(fileName = "DangerPerceptionProfile", menuName = "BS/AI/Danger Perception Profile")]
public class DangerPerceptionProfile : ScriptableObject
{
    [Header("Axis Sensitivity")]
    [SerializeField] private DangerPerceptionWeights axisSensitivity = DangerPerceptionWeights.One;

    [Header("Event Response")]
    [SerializeField, Min(0f)] private float globalEventMultiplier = 1f;
    [SerializeField, Min(0f)] private float gainMultiplier = 1f;
    [SerializeField, Min(0f)] private float recoveryMultiplier = 1f;

    public DangerPerceptionWeights AxisSensitivity => axisSensitivity;
    public float GlobalEventMultiplier => globalEventMultiplier;
    public float GainMultiplier => gainMultiplier;
    public float RecoveryMultiplier => recoveryMultiplier;

    public DangerPlotVector ApplyGain(DangerPlotVector rawDelta)
    {
        DangerPlotVector scaled = axisSensitivity.Apply(rawDelta) * (globalEventMultiplier * gainMultiplier);
        return scaled;
    }

    public DangerPlotVector ApplyRecovery(DangerPlotVector rawDelta)
    {
        DangerPlotVector scaled = axisSensitivity.Apply(rawDelta) * (globalEventMultiplier * recoveryMultiplier);
        return scaled;
    }
}