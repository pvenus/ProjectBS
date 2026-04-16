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
        RandomPointInRadius
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

    [Header("Projectile (Spawn Timing)")]
    [SerializeField, Min(0f)] private float spawnInterval = 0f;

    [Header("Projectile (Random Radius Pattern)")]
    [SerializeField, Range(0f, 180f)] private float randomAngleJitter = 18f;

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

        Transform target = ResolveAutoTarget(caster);
        if (target == null) return false;

        if (targetingMode == ProjectileTargetingMode.RandomPointInRadius)
        {
            FireRandomInRadius(caster);
            return true;
        }

        FireAt(caster, target);
        return true;
    }

    public bool Execute(Transform caster, Transform target)
    {
        if (caster == null) return false;

        if (target == null)
        {
            if (targetingMode == ProjectileTargetingMode.ExplicitTargetOnly)
                return false;

            target = ResolveAutoTarget(caster);
        }

        if (target == null) return false;

        if (targetingMode == ProjectileTargetingMode.RandomPointInRadius)
        {
            FireRandomInRadius(caster);
            return true;
        }

        FireAt(caster, target);
        return true;
    }

    private Transform ResolveAutoTarget(Transform caster)
    {
        if (caster == null)
            return null;

        switch (targetingMode)
        {
            case ProjectileTargetingMode.ExplicitTargetOnly:
            case ProjectileTargetingMode.RandomPointInRadius:
                return null;
            case ProjectileTargetingMode.NearestNpc:
            default:
                var npc = FindNearestNpc(caster.position);
                return npc != null ? npc.transform : null;
        }
    }

    private NpcMono FindNearestNpc(Vector2 origin)
    {
        float searchRange = Mathf.Max(0.1f, Range);
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

    private void FireRandomInRadius(Transform caster)
    {
        int count = Mathf.Max(1, projectileCount);
        float radius = Mathf.Max(0.1f, Range);
        List<Vector2> targetPositions = BuildRandomRadiusPatternPositions((Vector2)caster.position, radius, count);

        if (spawnInterval > 0f)
        {
            GetRunner().StartCoroutine(FireRandomInRadiusCoroutine(caster, targetPositions));
            return;
        }

        for (int i = 0; i < targetPositions.Count; i++)
        {
            FireAtPosition(caster, targetPositions[i], false);
        }
    }

    private System.Collections.IEnumerator FireRandomInRadiusCoroutine(Transform caster, List<Vector2> targetPositions)
    {
        for (int i = 0; i < targetPositions.Count; i++)
        {
            FireAtPosition(caster, targetPositions[i], false);

            if (spawnInterval > 0f && i < targetPositions.Count - 1)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void FireAtPosition(Transform caster, Vector2 targetPosition, bool useProjectileCount)
    {
        Vector2 from = caster.position;
        Vector2 to = targetPosition;

        Vector2 dir = (to - from);
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
        dir.Normalize();

        Vector2 spawnPos = from + dir * projectileSpawnOffset * projectileScale;

        int spawnCount = useProjectileCount ? Mathf.Max(1, projectileCount) : 1;

        if (spawnInterval > 0f && spawnCount > 1)
        {
            GetRunner().StartCoroutine(FireAtPositionCoroutine(caster, to, spawnPos, spawnCount));
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleProjectile(caster, spawnPos, to);
        }
    }

    private System.Collections.IEnumerator FireAtPositionCoroutine(Transform caster, Vector2 targetPosition, Vector2 spawnPos, int spawnCount)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingleProjectile(caster, spawnPos, targetPosition);

            if (spawnInterval > 0f)
                yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnSingleProjectile(Transform caster, Vector2 spawnPos, Vector2 targetPosition)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is required.", this);
            return;
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        proj.transform.localScale = Vector3.one * projectileScale;
        proj.name = projectilePrefab.name;

        var projectile = proj.GetComponent<SkillProjectileMono>() ?? proj.AddComponent<SkillProjectileMono>();
        var hitMono = proj.GetComponentInChildren<SkillProjectileHitMono>(true);
        var moveMono = proj.GetComponent<SkillProjectileMoveMono>() ?? proj.AddComponent<SkillProjectileMoveMono>();
        var lifeMono = proj.GetComponent<SkillProjectileLifeTimeMono>() ?? proj.AddComponent<SkillProjectileLifeTimeMono>();

        SkillProjectileDto dto = new SkillProjectileDto
        {
            moveConfig = moveConfig,
            lifetime = projectileLifetime,
            projectileCount = Mathf.Max(1, projectileCount),
            fireType = fireType
        };

        projectile.Initialize(dto);

        if (hitMono != null)
            hitMono.SetOwner(caster);

        if (moveConfig != null)
        {
            moveMono.Initialize(moveConfig, proj.transform, spawnPos, targetPosition);
        }
        else
        {
            Debug.LogWarning("Projectile move config is not assigned.", this);
        }

        lifeMono.StartLife(projectileLifetime);
    }

    private void FireAt(Transform caster, Transform target)
    {
        if (target == null)
            return;

        FireAtPosition(caster, target.position, true);
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