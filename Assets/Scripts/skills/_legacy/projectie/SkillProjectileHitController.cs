using UnityEngine;

/// <summary>
/// First-step refactor for hit processing.
///
/// Responsibilities in this version:
/// - Enforce max hit count
/// - Reject same-root hits when configured
/// - Delegate repeated-hit rules to IHitRepeatPolicy
/// - Store per-target hit history through HitTracker
///
/// This keeps the controller small while allowing hit behavior to expand
/// through policy objects later.
/// </summary>
[System.Serializable]
public class SkillProjectileHitController
{
    [SerializeField, Min(1)] private int maxHitCount = 1;
    [SerializeField] private bool ignoreSameRoot = true;

    private readonly SkillProjectileHitTracker _hitTracker = new SkillProjectileHitTracker();
    private ISkillHitRepeatPolicy _repeatPolicy = new SkillHitOncePolicy();
    private int _hitCount;

    public int MaxHitCount => maxHitCount;
    public int HitCount => _hitCount;
    public bool IgnoreSameRoot => ignoreSameRoot;
    public bool HasReachedMaxHitCount => _hitCount >= maxHitCount;
    public SkillProjectileHitTracker Tracker => _hitTracker;
    public ISkillHitRepeatPolicy RepeatPolicy => _repeatPolicy;

    public void Configure(int maxHitCount, bool ignoreSameRoot)
    {
        this.maxHitCount = Mathf.Max(1, maxHitCount);
        this.ignoreSameRoot = ignoreSameRoot;
    }

    public void SetRepeatPolicy(ISkillHitRepeatPolicy repeatPolicy)
    {
        _repeatPolicy = repeatPolicy ?? new SkillHitOncePolicy();
    }

    public void UseHitOncePolicy()
    {
        _repeatPolicy = new SkillHitOncePolicy();
    }

    public void UseHitIntervalPolicy(float interval)
    {
        _repeatPolicy = new SkillHitIntervalPolicy(interval);
    }

    public void ResetState()
    {
        _hitTracker.ResetState();
        _hitCount = 0;
    }

    public bool CanHit(Transform owner, Collider2D other)
    {
        if (other == null)
            return false;

        if (HasReachedMaxHitCount)
            return false;

        Transform otherRoot = other.transform.root;
        Transform ownerRoot = owner != null ? owner.root : null;

        if (ignoreSameRoot && ownerRoot != null && otherRoot == ownerRoot)
            return false;

        if (_repeatPolicy == null)
            _repeatPolicy = new SkillHitOncePolicy();

        return _repeatPolicy.CanHit(_hitTracker, other, Time.time);
    }

    public bool TryRegisterHit(Transform owner, Collider2D other)
    {
        if (!CanHit(owner, other))
            return false;

        if (_repeatPolicy == null)
            _repeatPolicy = new SkillHitOncePolicy();

        _repeatPolicy.RecordHit(_hitTracker, other, Time.time);
        _hitCount++;
        return true;
    }
}