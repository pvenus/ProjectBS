using UnityEngine;

/// <summary>
/// Allows a target to be hit only once for the lifetime of the object.
/// This matches the current default projectile behavior.
/// </summary>
public class SkillHitOncePolicy : ISkillHitRepeatPolicy
{
    public bool CanHit(SkillProjectileHitTracker tracker, Collider2D target, float currentTime)
    {
        if (tracker == null || target == null)
            return false;

        // Already hit → cannot hit again
        return !tracker.HasHit(target);
    }

    public void RecordHit(SkillProjectileHitTracker tracker, Collider2D target, float currentTime)
    {
        if (tracker == null || target == null)
            return;

        tracker.RecordHit(target, currentTime);
    }
}
