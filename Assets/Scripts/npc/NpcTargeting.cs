using UnityEngine;

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
    }

    public void ClearForcedTarget(Transform target)
    {
        if (_forcedTarget != target)
            return;

        _forcedTarget = null;
        RefreshCurrentTargetIfNeeded(true);
    }

    public void RefreshCurrentTargetIfNeeded(bool force = false)
    {
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
        bool siegeTowerOnly = archetype == TargetingArchetype.Siege;
        bool flyingRandomTarget = archetype == TargetingArchetype.Flying;

        if (flyingRandomTarget)
            return FindRandomAvailableTarget();

        Vector3 selfPos = transform.position;
        Transform best = null;
        float bestScore = float.MaxValue;

        if (!siegeTowerOnly && includePartyMembersAsTargets)
        {
            PartyMovementMono[] members = FindObjectsByType<PartyMovementMono>(FindObjectsSortMode.None);
            for (int i = 0; i < members.Length; i++)
            {
                PartyMovementMono member = members[i];
                if (member == null || !member.gameObject.activeInHierarchy)
                    continue;

                ConsiderCandidate(member.transform, selfPos, ref best, ref bestScore, false);
            }
        }

        if (includeTowersAsTargets)
        {
            TowerPropMono[] towers = FindObjectsByType<TowerPropMono>(FindObjectsSortMode.None);
            for (int i = 0; i < towers.Length; i++)
            {
                TowerPropMono tower = towers[i];
                if (tower == null || !tower.gameObject.activeInHierarchy)
                    continue;

                if (tower.IsDead())
                    continue;

                ConsiderCandidate(tower.transform, selfPos, ref best, ref bestScore, true);
            }
        }

        return best;
    }

    private Transform FindRandomAvailableTarget()
    {
        System.Collections.Generic.List<Transform> candidates = new System.Collections.Generic.List<Transform>();

        if (includePartyMembersAsTargets)
        {
            PartyMovementMono[] members = FindObjectsByType<PartyMovementMono>(FindObjectsSortMode.None);
            for (int i = 0; i < members.Length; i++)
            {
                PartyMovementMono member = members[i];
                if (member == null || !member.gameObject.activeInHierarchy)
                    continue;

                if (member.transform == transform)
                    continue;

                candidates.Add(member.transform);
            }
        }

        if (includeTowersAsTargets)
        {
            TowerPropMono[] towers = FindObjectsByType<TowerPropMono>(FindObjectsSortMode.None);
            for (int i = 0; i < towers.Length; i++)
            {
                TowerPropMono tower = towers[i];
                if (tower == null || !tower.gameObject.activeInHierarchy)
                    continue;

                if (tower.IsDead())
                    continue;

                candidates.Add(tower.transform);
            }
        }

        if (candidates.Count == 0)
            return null;

        int index = Random.Range(0, candidates.Count);
        return candidates[index];
    }

    private void ConsiderCandidate(Transform candidate, Vector3 selfPos, ref Transform best, ref float bestScore, bool isTower)
    {
        if (candidate == null)
            return;

        if (candidate == transform)
            return;

        float distSqr = (candidate.position - selfPos).sqrMagnitude;
        float score = distSqr;

        if (archetype == TargetingArchetype.Siege && siegePrioritizeTowers)
        {
            if (isTower)
                score *= 0.35f;
            else
                score *= 1.65f;
        }

        if (score < bestScore)
        {
            best = candidate;
            bestScore = score;
        }
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