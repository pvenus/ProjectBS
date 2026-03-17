using UnityEngine;

[System.Serializable]
public struct BrainDecisionState
{
    public BrainPhase phase;
    public TacticalNeed need;
    public string label;
    public float priority;
    public Transform target;
    public Vector3 point;

    public static BrainDecisionState None => new BrainDecisionState
    {
        phase = BrainPhase.Idle,
        need = TacticalNeed.None,
        label = "None",
        priority = float.NegativeInfinity,
        target = null,
        point = Vector3.zero
    };
}
