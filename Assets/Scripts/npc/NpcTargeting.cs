using UnityEngine;
using Npc.Service;

/// <summary>
/// Handles NPC target selection.
/// - Supports forced target (taunt-like behavior)
/// - Selects nearest valid party member or tower, except flying archetypes which choose a random valid target
/// - Siege archetypes target towers only
/// </summary>
public class NpcTargeting : MonoBehaviour
{
    public enum TargetingArchetype
    {
        Normal,
        Melee,
        Ranged,
        Siege,
        Flying
    }

    [Header("Target Search")]
    [SerializeField] private TargetingArchetype archetype = TargetingArchetype.Normal;
    [SerializeField] private bool includePartyMembersAsTargets = true;
    [SerializeField] private bool includeTowersAsTargets = true;
    [SerializeField] private float targetRefreshInterval = 0.25f;
    [SerializeField] private bool siegePrioritizeTowers = true;

    private Transform _forcedTarget;
    private float _forcedUntil;
    private Transform _currentTarget;
    private float _targetRefreshTimer;
    private readonly NpcTargetSearchService _targetSearchService =
        new NpcTargetSearchService();

    private void Awake()
    {
        RefreshCurrentTargetIfNeeded(true);
    }

    private void Update()
    {
        RefreshCurrentTargetIfNeeded();
    }

    public Transform GetCurrentTarget()
    {
        if (_forcedTarget != null && Time.time < _forcedUntil)
            return _forcedTarget;

        if (_forcedTarget != null && Time.time >= _forcedUntil)
            _forcedTarget = null;

        RefreshCurrentTargetIfNeeded();
        return _currentTarget;
    }

    public void ForceTarget(Transform target, float duration)
    {
        if (target == null)
            return;

        _forcedTarget = target;
        _forcedUntil = Mathf.Max(_forcedUntil, Time.time + Mathf.Max(0.05f, duration));
        _currentTarget = _forcedTarget;
        ForcePathingRepath();
    }

    public void ClearForcedTarget(Transform target)
    {
        if (_forcedTarget != target)
            return;

        _forcedTarget = null;
        ForcePathingRepath();
        RefreshCurrentTargetIfNeeded(true);
    }

    public void RefreshCurrentTargetIfNeeded(bool force = false)
    {
        if (_forcedTarget != null && Time.time < _forcedUntil)
        {
            _currentTarget = _forcedTarget;
            return;
        }

        if (_forcedTarget != null && Time.time >= _forcedUntil)
        {
            _forcedTarget = null;
            ForcePathingRepath();
        }
        if (!force)
        {
            _targetRefreshTimer -= Time.deltaTime;
            if (_targetRefreshTimer > 0f)
                return;
        }

        _targetRefreshTimer = Mathf.Max(0.05f, targetRefreshInterval);
        _currentTarget = FindNearestAvailableTarget();
    }

    public Transform FindNearestAvailableTarget()
    {
        return _targetSearchService.FindTarget(
            new NpcTargetSearchService.Context
            {
                self = transform,
                archetype = archetype,
                includePartyMembersAsTargets = includePartyMembersAsTargets,
                includeTowersAsTargets = includeTowersAsTargets,
                siegePrioritizeTowers = siegePrioritizeTowers
            });
    }

    public bool HasForcedTarget()
    {
        return _forcedTarget != null && Time.time < _forcedUntil;
    }

    public Transform GetForcedTarget()
    {
        return HasForcedTarget() ? _forcedTarget : null;
    }

    public TargetingArchetype GetArchetype()
    {
        return archetype;
    }

    public void SetArchetype(TargetingArchetype newArchetype)
    {
        archetype = newArchetype;
        RefreshCurrentTargetIfNeeded(true);
    }

    private void ForcePathingRepath()
    {
        NpcPathing pathing = GetComponent<NpcPathing>();

        if (pathing != null)
        {
            pathing.ForceRepath();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform target = Application.isPlaying ? GetCurrentTarget() : null;
        if (target == null)
            return;

        Gizmos.color = new Color(1f, 0.4f, 1f, 0.9f);
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.25f);
    }
}