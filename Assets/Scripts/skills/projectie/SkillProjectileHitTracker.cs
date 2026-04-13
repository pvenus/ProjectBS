

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores hit history per target so hit policies can decide whether
/// a target may be hit again.
///
/// First-step responsibilities:
/// - Resolve a stable target id from a collider/root
/// - Record last hit time per target
/// - Support both one-time hit checks and interval-based re-hit checks
/// </summary>
[System.Serializable]
public class SkillProjectileHitTracker
{
    private readonly Dictionary<int, float> _lastHitTimeByTarget = new Dictionary<int, float>();

    public int TrackedTargetCount => _lastHitTimeByTarget.Count;

    public void ResetState()
    {
        _lastHitTimeByTarget.Clear();
    }

    public bool HasHit(Collider2D other)
    {
        if (other == null)
            return false;

        int targetId = GetTargetId(other);
        return _lastHitTimeByTarget.ContainsKey(targetId);
    }

    public bool TryGetLastHitTime(Collider2D other, out float lastHitTime)
    {
        lastHitTime = 0f;

        if (other == null)
            return false;

        int targetId = GetTargetId(other);
        return _lastHitTimeByTarget.TryGetValue(targetId, out lastHitTime);
    }

    public bool CanHitAgain(Collider2D other, float currentTime, float requiredInterval)
    {
        if (other == null)
            return false;

        if (requiredInterval <= 0f)
            return true;

        if (!TryGetLastHitTime(other, out float lastHitTime))
            return true;

        return currentTime - lastHitTime >= requiredInterval;
    }

    public void RecordHit(Collider2D other, float currentTime)
    {
        if (other == null)
            return;

        int targetId = GetTargetId(other);
        _lastHitTimeByTarget[targetId] = currentTime;
    }

    public bool RemoveTarget(Collider2D other)
    {
        if (other == null)
            return false;

        int targetId = GetTargetId(other);
        return _lastHitTimeByTarget.Remove(targetId);
    }

    public static int GetTargetId(Collider2D other)
    {
        if (other == null)
            return 0;

        Transform otherRoot = other.transform.root;
        return otherRoot != null
            ? otherRoot.gameObject.GetInstanceID()
            : other.gameObject.GetInstanceID();
    }
}