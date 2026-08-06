

using UnityEngine;

[System.Serializable]
public struct RoleVector
{
    [Range(0f, 1f)] public float dps;
    [Range(0f, 1f)] public float tank;
    [Range(0f, 1f)] public float support;

    public RoleVector(float dps, float tank, float support)
    {
        this.dps = dps;
        this.tank = tank;
        this.support = support;
    }

    public float Magnitude => Mathf.Sqrt(dps * dps + tank * tank + support * support);

    public RoleVector Normalized()
    {
        float mag = Magnitude;
        if (mag <= 0.0001f) return new RoleVector(0f, 0f, 0f);
        return new RoleVector(dps / mag, tank / mag, support / mag);
    }

    public static RoleVector operator +(RoleVector a, RoleVector b)
    {
        return new RoleVector(a.dps + b.dps, a.tank + b.tank, a.support + b.support);
    }

    public static RoleVector operator *(RoleVector a, float scalar)
    {
        return new RoleVector(a.dps * scalar, a.tank * scalar, a.support * scalar);
    }

    public override string ToString()
    {
        return $"(D:{dps:0.00}, T:{tank:0.00}, S:{support:0.00})";
    }
}