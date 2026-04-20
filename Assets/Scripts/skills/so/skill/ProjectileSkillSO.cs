using System.Collections.Generic;
using UnityEngine;
using Skills.Dto;

/// <summary>
/// BasicThrowSkill
/// - Configurable projectile-skill asset.
/// - Supports nearest-target or explicit-target execution.
/// - Creates projectile instances from a prefab and configures runtime sub-monos.
/// </summary>
[CreateAssetMenu(menuName = "BS/Skills/Projectile Skill", fileName = "ProjectileSkill")]
public class ProjectileSkill : BattleSkillBase
{
    private static CoroutineRunner _runner;
    public enum ProjectileTargetingMode
    {
        NearestNpc,
        ExplicitTargetOnly,
        RandomPointInRadius,
        SpawnAtCasterOnly
    }

    [Header("Projectile (Targeting)")]
    [SerializeField] private ProjectileTargetingMode targetingMode = ProjectileTargetingMode.NearestNpc;
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private bool ignoreSameRoot = true;

    [Header("Projectile (Prefab)")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpawnOffset = 0.6f;
    [SerializeField, Min(0.01f)] private float projectileScale = 1f;

    [Header("Projectile (Move)")]
    [SerializeField] private SkillMoveSO moveConfig;

    [Header("Projectile (Lifetime)")]
    [SerializeField] private float projectileLifetime = 2.5f;

    [Header("Projectile (Runtime DTO)")]
    [SerializeField, Min(1)] private int projectileCount = 1;
    [SerializeField] private SkillProjectileFireType fireType = SkillProjectileFireType.Targeting;

    public int ProjectileCount => Mathf.Max(1, projectileCount);
    public float ProjectileScale => Mathf.Max(0.01f, projectileScale);

    [Header("Projectile (Spawn Timing)")]
    [SerializeField, Min(0f)] private float spawnInterval = 0f;

    [Header("Projectile (Runtime Upgrade Refresh)")]
    [SerializeField] private bool useRuntimeUpgradeRefresh = false;

    [Header("Projectile (Random Radius Pattern)")]
    [SerializeField, Range(0f, 180f)] private float randomAngleJitter = 18f;

    private struct ResolvedProjectileValues
    {
        public float range;
        public float projectileScale;
        public float projectileLifetime;
        public int maxProjectileCount;
    }

    public override float EvaluateBrainScore(object context, int roleBias = 0)
    {
        return BasePriority + roleBias;
    }

    /// <summary>
    /// Executes the throw from caster toward nearest NpcMono target.
    /// Returns true if fired.
    /// </summary>
    public bool Execute(Transform caster)
    {
        if (caster == null) return false;

        ResolvedProjectileValues resolved = ResolveRuntimeValues(caster);

        if (targetingMode == ProjectileTargetingMode.RandomPointInRadius)
        {
            FireRandomInRadius(caster, resolved);
            return true;
        }

        if (targetingMode == ProjectileTargetingMode.SpawnAtCasterOnly)
        {
            FireAtPosition(caster, caster.position, true, resolved);
            return true;
        }

        Transform target = ResolveAutoTarget(caster, resolved.range);
        if (target == null) return false;

        FireAt(caster, target, resolved);
        return true;
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null) return false;

        ResolvedProjectileValues resolved = ResolveRuntimeValues(caster);

        if (targetingMode == ProjectileTargetingMode.SpawnAtCasterOnly)
        {
            FireAtPosition(caster, caster.position, true, resolved);
            return true;
        }

        if (target == null)
        {
            if (targetingMode == ProjectileTargetingMode.ExplicitTargetOnly)
                return false;

            target = ResolveAutoTarget(caster, resolved.range);
        }

        if (target == null) return false;

        if (targetingMode == ProjectileTargetingMode.RandomPointInRadius)
        {
            FireRandomInRadius(caster, resolved);
            return true;
        }

        FireAt(caster, target, resolved);
        return true;
    }

    private Transform ResolveAutoTarget(Transform caster, float resolvedRange)
    {
        if (caster == null)
            return null;

        switch (targetingMode)
        {
            case ProjectileTargetingMode.ExplicitTargetOnly:
            case ProjectileTargetingMode.RandomPointInRadius:
            case ProjectileTargetingMode.SpawnAtCasterOnly:
                return null;
            case ProjectileTargetingMode.NearestNpc:
            default:
                var npc = FindNearestNpc(caster.position, resolvedRange);
                return npc != null ? npc.transform : null;
        }
    }

    private NpcMono FindNearestNpc(Vector2 origin, float resolvedRange)
    {
        float searchRange = Mathf.Max(0.1f, resolvedRange);
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, searchRange, enemyMask);
        float best = float.PositiveInfinity;
        NpcMono bestNpc = null;

        for (int i = 0; i < hits.Length; i++)
        {
            var c = hits[i];
            if (c == null) continue;
            if (c.isTrigger) continue;

            var npc = c.GetComponentInParent<NpcMono>();
            if (npc == null) continue;

            float d = ((Vector2)npc.transform.position - origin).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestNpc = npc;
            }
        }

        return bestNpc;
    }

    private CoroutineRunner GetRunner()
    {
        if (_runner == null)
        {
            var go = new GameObject("ProjectileCoroutineRunner");
            Object.DontDestroyOnLoad(go);
            _runner = go.AddComponent<CoroutineRunner>();
        }
        return _runner;
    }

    private void FireRandomInRadius(Transform caster, ResolvedProjectileValues resolved)
    {
        int count = Mathf.Max(1, projectileCount);
        float radius = Mathf.Max(0.1f, resolved.range);
        List<Vector2> targetPositions = BuildRandomRadiusPatternPositions((Vector2)caster.position, radius, count);

        if (spawnInterval > 0f)
        {
            GetRunner().StartCoroutine(FireRandomInRadiusCoroutine(caster, targetPositions, resolved));
            return;
        }

        for (int i = 0; i < targetPositions.Count; i++)
        {
            FireAtPosition(caster, targetPositions[i], false, resolved);
        }
    }

    private System.Collections.IEnumerator FireRandomInRadiusCoroutine(Transform caster, List<Vector2> targetPositions, ResolvedProjectileValues resolved)
    {
        for (int i = 0; i < targetPositions.Count; i++)
        {
            FireAtPosition(caster, targetPositions[i], false, resolved);

            if (spawnInterval > 0f && i < targetPositions.Count - 1)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void FireAtPosition(Transform caster, Vector2 targetPosition, bool useProjectileCount, ResolvedProjectileValues resolved)
    {
        Vector2 from = caster.position;
        Vector2 to = targetPosition;

        Vector2 dir = (to - from);
        bool hasDirection = dir.sqrMagnitude >= 0.0001f;
        if (!hasDirection)
            dir = Vector2.up;
        dir.Normalize();

        Vector2 spawnPos = hasDirection
            ? from + dir * projectileSpawnOffset * resolved.projectileScale
            : from;

        int spawnCount = useProjectileCount ? Mathf.Max(1, resolved.maxProjectileCount) : 1;

        if (spawnInterval > 0f && spawnCount > 1)
        {
            GetRunner().StartCoroutine(FireAtPositionCoroutine(caster, to, spawnPos, spawnCount, resolved));
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleProjectile(caster, spawnPos, to, resolved, i, spawnCount);
        }
    }

    private System.Collections.IEnumerator FireAtPositionCoroutine(Transform caster, Vector2 targetPosition, Vector2 spawnPos, int spawnCount, ResolvedProjectileValues resolved)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleProjectile(caster, spawnPos, targetPosition, resolved, i, spawnCount);

            if (spawnInterval > 0f)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnSingleProjectile(Transform caster, Vector2 spawnPos, Vector2 targetPosition, ResolvedProjectileValues resolved, int spawnOrder, int maxProjectileCount)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is required.", this);
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.localScale = Vector3.one * resolved.projectileScale;
        proj.name = projectilePrefab.name;

        var projectile = proj.GetComponent<SkillProjectileMono>() ?? proj.AddComponent<SkillProjectileMono>();

        SkillUpgradeMono upgradeMono = caster != null ? caster.GetComponentInParent<SkillUpgradeMono>() : null;
        SkillUpgradeMono.SkillUpgradeData upgradeData = upgradeMono != null ? upgradeMono.GetUpgradeData(this) : SkillUpgradeMono.SkillUpgradeData.Default;

        SkillProjectileDto dto = new SkillProjectileDto
        {
            moveConfig = moveConfig,
            lifetime = resolved.projectileLifetime,
            projectileCount = Mathf.Max(1, projectileCount),
            fireType = fireType,
            sourceSkill = this,
            useRuntimeUpgradeRefresh = useRuntimeUpgradeRefresh,
            runtimeUpgradeRefreshInterval = 0.2f
        };

        projectile.Initialize(
            dto,
            caster,
            upgradeData,
            spawnPos,
            targetPosition,
            resolved.projectileScale,
            resolved.projectileLifetime,
            spawnOrder,
            maxProjectileCount);
    }

    public void SpawnAdditionalOrbitProjectiles(Transform caster, int currentMaxProjectileCount, int desiredMaxProjectileCount)
    {
        if (caster == null)
            return;

        if (moveConfig == null)
            return;

        if (moveConfig.MoveType != SkillProjectileMoveDto.MoveType.Orbit)
            return;

        int safeCurrent = Mathf.Max(0, currentMaxProjectileCount);
        int safeDesired = Mathf.Max(1, desiredMaxProjectileCount);
        if (safeDesired <= safeCurrent)
            return;

        ResolvedProjectileValues resolved = ResolveRuntimeValues(caster);
        resolved.maxProjectileCount = safeDesired;

        Vector2 spawnPos = caster.position;
        Vector2 targetPos = caster.position;

        for (int spawnOrder = safeCurrent; spawnOrder < safeDesired; spawnOrder++)
        {
            SpawnSingleProjectile(caster, spawnPos, targetPos, resolved, spawnOrder, safeDesired);
        }
    }

    private void FireAt(Transform caster, Transform target, ResolvedProjectileValues resolved)
    {
        if (target == null)
            return;

        FireAtPosition(caster, target.position, true, resolved);
    }
    private ResolvedProjectileValues ResolveRuntimeValues(Transform caster)
    {
        ResolvedProjectileValues resolved = new ResolvedProjectileValues
        {
            range = Mathf.Max(0.1f, Range),
            projectileScale = Mathf.Max(0.01f, projectileScale),
            projectileLifetime = Mathf.Max(0.01f, projectileLifetime),
            maxProjectileCount = Mathf.Max(1, projectileCount)
        };

        if (caster == null)
            return resolved;

        SkillUpgradeMono upgradeMono = caster.GetComponentInParent<SkillUpgradeMono>();
        if (upgradeMono == null)
            return resolved;

        SkillUpgradeMono.SkillUpgradeData upgrade = upgradeMono.GetUpgradeData(this);

        resolved.range = Mathf.Max(0.1f, resolved.range + upgrade.rangeAdd);
        resolved.projectileScale = Mathf.Max(0.01f, resolved.projectileScale + upgrade.projectileScaleAdd);
        resolved.projectileLifetime = Mathf.Max(0.01f, resolved.projectileLifetime + upgrade.projectileLifetimeAdd);
        resolved.maxProjectileCount = Mathf.Max(1, resolved.maxProjectileCount + Mathf.RoundToInt(upgrade.projectileCountAdd));
        return resolved;
    }
    private class CoroutineRunner : MonoBehaviour { }
    private List<Vector2> BuildRandomRadiusPatternPositions(Vector2 center, float radius, int count)
    {
        List<Vector2> positions = new List<Vector2>(count);
        if (count <= 0)
            return positions;

        float innerRadiusMax = radius * 0.5f;
        int innerCount = Mathf.RoundToInt(count * 0.25f);
        innerCount = Mathf.Clamp(innerCount, 0, count);
        int outerCount = count - innerCount;

        List<float> angles = BuildShuffledAngles(count);

        int angleIndex = 0;

        for (int i = 0; i < innerCount; i++)
        {
            float angle = angles[angleIndex++];
            float sampledRadius = Random.Range(0f, innerRadiusMax);
            positions.Add(center + DirectionFromAngle(angle) * sampledRadius);
        }

        for (int i = 0; i < outerCount; i++)
        {
            float angle = angles[angleIndex++];
            float sampledRadius = Random.Range(innerRadiusMax, radius);
            positions.Add(center + DirectionFromAngle(angle) * sampledRadius);
        }

        ShufflePositions(positions);
        return positions;
    }

    private List<float> BuildShuffledAngles(int count)
    {
        List<float> angles = new List<float>(count);
        float step = 360f / Mathf.Max(1, count);
        float baseOffset = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            float angle = baseOffset + (step * i) + Random.Range(-randomAngleJitter, randomAngleJitter);
            angles.Add(angle);
        }

        for (int i = angles.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            float temp = angles[i];
            angles[i] = angles[swapIndex];
            angles[swapIndex] = temp;
        }

        return angles;
    }

    private static Vector2 DirectionFromAngle(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private static void ShufflePositions(List<Vector2> positions)
    {
        for (int i = positions.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            Vector2 temp = positions[i];
            positions[i] = positions[swapIndex];
            positions[swapIndex] = temp;
        }
    }
}