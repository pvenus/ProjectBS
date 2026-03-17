using UnityEngine;

/// <summary>
/// PerceptionMono
/// Handles environmental sensing for AI agents.
/// Collects nearby enemies and basic spatial awareness information
/// so other systems (SkillBrain, State, etc.) do not need to run
/// physics queries themselves.
/// </summary>
public class PerceptionMono : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private float detectRadius = 4f;

    [Header("Update")]
    [Tooltip("How often perception scans the environment")]
    [SerializeField] private float scanInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugDraw = false;

    public int EnemyCount { get; private set; }
    public Transform ClosestEnemy { get; private set; }

    private float scanTimer;

    void Update()
    {
        scanTimer -= Time.deltaTime;

        if (scanTimer > 0f)
            return;

        scanTimer = scanInterval;

        ScanEnemies();
    }

    void ScanEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyMask);

        EnemyCount = 0;
        ClosestEnemy = null;

        float closestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];

            if (h == null || h.isTrigger)
                continue;

            if (h.transform == transform || h.transform.IsChildOf(transform))
                continue;

            EnemyCount++;

            float d = Vector2.Distance(transform.position, h.transform.position);

            if (d < closestDist)
            {
                closestDist = d;
                ClosestEnemy = h.transform;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!debugDraw)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (ClosestEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, ClosestEnemy.position);
        }
    }
}