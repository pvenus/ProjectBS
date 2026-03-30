

using UnityEngine;

public enum DangerEventType
{
    None,
    EnemyClusteredNearSelf,
    EnemyApproachedSelf,
    SelfDamaged,
    SelfRecovered,
    AllyDamaged,
    AllyRecovered,
    BattlefieldUnstable,
    BattlefieldStabilized,
    Isolated,
    Regrouped
}

[System.Serializable]
public struct DangerEvent
{
    public DangerEventType eventType;
    public DangerPlotVector rawDelta;
    public float intensity;
    public string reason;
    public Transform source;

    public DangerEvent(
        DangerEventType eventType,
        DangerPlotVector rawDelta,
        float intensity = 1f,
        string reason = "",
        Transform source = null)
    {
        this.eventType = eventType;
        this.rawDelta = rawDelta;
        this.intensity = intensity;
        this.reason = reason;
        this.source = source;
    }

    public DangerPlotVector GetScaledDelta()
    {
        float clampedIntensity = Mathf.Max(0f, intensity);
        return rawDelta * clampedIntensity;
    }

    public bool IsRecoveryEvent()
    {
        switch (eventType)
        {
            case DangerEventType.SelfRecovered:
            case DangerEventType.AllyRecovered:
            case DangerEventType.BattlefieldStabilized:
            case DangerEventType.Regrouped:
                return true;
            default:
                return false;
        }
    }

    public override string ToString()
    {
        return $"[{eventType}] intensity={intensity:0.00} rawDelta={rawDelta} reason={reason}";
    }
}