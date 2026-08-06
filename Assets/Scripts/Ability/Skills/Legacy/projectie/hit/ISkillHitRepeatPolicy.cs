

using UnityEngine;

/// <summary>
/// Defines how repeated hits on the same target are handled.
///
/// This policy decides:
/// - Can this target be hit now?
/// - How to record the hit after it happens
///
/// It works together with HitTracker.
/// </summary>
public interface ISkillHitRepeatPolicy
{
    /// <summary>
    /// Returns true if the target can be hit at this time.
    /// </summary>
    bool CanHit(SkillProjectileHitTracker tracker, Collider2D target, float currentTime);

    /// <summary>
    /// Called after a successful hit to record state.
    /// </summary>
    void RecordHit(SkillProjectileHitTracker tracker, Collider2D target, float currentTime);
}