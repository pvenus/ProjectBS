

using UnityEngine;

/// <summary>
/// Allows a target to be hit repeatedly after a fixed interval.
/// Used for area damage, aura, DOT, etc.
/// </summary>
public class SkillHitIntervalPolicy : ISkillHitRepeatPolicy
{
    private readonly float _interval;

    /// <param name="interval">Minimum time between hits on the same target</param>
    public SkillHitIntervalPolicy(float interval)
    {
        _interval = Mathf.Max(0f, interval);
    }

    public bool CanHit(SkillProjectileHitTracker tracker, Collider2D target, float currentTime)
    {
        if (tracker == null || target == null)
            return false;

        // First time → always allowed
        if (!tracker.HasHit(target))
            return true;

        // Check interval
        return tracker.CanHitAgain(target, currentTime, _interval);
    }

    public void RecordHit(SkillProjectileHitTracker tracker, Collider2D target, float currentTime)
    {
        if (tracker == null || target == null)
            return;

        tracker.RecordHit(target, currentTime);
    }
}