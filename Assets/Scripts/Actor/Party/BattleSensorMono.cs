

using UnityEngine;

public class BattleSensorMono : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIVectorMono aiVector;
    [SerializeField] private DangerPerceptionProfile perceptionProfile;
    [SerializeField] private Transform sensorCenter;

    [Header("Scan Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask allyLayer;

    [Header("Scan Range")]
    [SerializeField, Min(0.1f)] private float enemyScanRadius = 4f;
    [SerializeField, Min(0.1f)] private float allyScanRadius = 5f;

    [Header("Thresholds")]
    [SerializeField, Min(1)] private int clusteredEnemyCount = 3;
    [SerializeField, Min(1)] private int isolatedEnemyCount = 2;
    [SerializeField, Min(1)] private int regroupedAllyCount = 1;

    [Header("Danger Deltas")]
    [SerializeField] private DangerPlotVector enemyApproachDelta = new DangerPlotVector(6f, 0f, 2f);
    [SerializeField] private DangerPlotVector enemyClusterDelta = new DangerPlotVector(12f, 2f, 6f);
    [SerializeField] private DangerPlotVector isolatedDelta = new DangerPlotVector(4f, 4f, 8f);
    [SerializeField] private DangerPlotVector regroupedRecoveryDelta = new DangerPlotVector(2f, 3f, 5f);
    [SerializeField] private DangerPlotVector stabilizedRecoveryDelta = new DangerPlotVector(3f, 1f, 4f);

    [Header("Timing")]
    [SerializeField, Min(0.05f)] private float scanInterval = 0.25f;
    [SerializeField, Min(0.1f)] private float stabilizeInterval = 1.25f;

    [Header("Debug")]
    [SerializeField] private bool debugLog;
    [SerializeField] private bool drawGizmos = true;

    private float nextScanTime;
    private float nextStabilizeTime;

    private bool wasEnemyNear;
    private bool wasEnemyClustered;
    private bool wasIsolated;
    private bool wasRegrouped;

    private int lastEnemyCount;
    private int lastAllyCount;

    private void Reset()
    {
        sensorCenter = transform;
        aiVector = GetComponent<AIVectorMono>();
    }

    private void Awake()
    {
        if (sensorCenter == null)
            sensorCenter = transform;

        if (aiVector == null)
            aiVector = GetComponent<AIVectorMono>();
    }

    private void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + scanInterval;
            ScanBattlefield();
        }

        if (Time.time >= nextStabilizeTime)
        {
            nextStabilizeTime = Time.time + stabilizeInterval;
            ApplyPassiveStabilization();
        }
    }

    private void ScanBattlefield()
    {
        Vector2 center = sensorCenter.position;
        int enemyCount = CountNearby(center, enemyScanRadius, enemyLayer, includeSelf: false);
        int allyCount = CountNearby(center, allyScanRadius, allyLayer, includeSelf: false);

        bool hasEnemyNear = enemyCount > 0;
        bool isEnemyClustered = enemyCount >= clusteredEnemyCount;
        bool isIsolated = enemyCount >= isolatedEnemyCount && allyCount <= 0;
        bool isRegrouped = allyCount >= regroupedAllyCount;

        if (hasEnemyNear && !wasEnemyNear)
        {
            EmitEvent(new DangerEvent(
                DangerEventType.EnemyApproachedSelf,
                enemyApproachDelta,
                Mathf.Max(1f, enemyCount),
                $"Enemy approached self (count={enemyCount})",
                sensorCenter));
        }

        if (isEnemyClustered && !wasEnemyClustered)
        {
            float intensity = Mathf.Max(1f, enemyCount / Mathf.Max(1f, (float)clusteredEnemyCount));
            EmitEvent(new DangerEvent(
                DangerEventType.EnemyClusteredNearSelf,
                enemyClusterDelta,
                intensity,
                $"Enemies clustered near self (count={enemyCount})",
                sensorCenter));
        }

        if (isIsolated && !wasIsolated)
        {
            EmitEvent(new DangerEvent(
                DangerEventType.Isolated,
                isolatedDelta,
                1f,
                $"Isolated under pressure (enemies={enemyCount}, allies={allyCount})",
                sensorCenter));
        }

        if (isRegrouped && !wasRegrouped)
        {
            EmitEvent(new DangerEvent(
                DangerEventType.Regrouped,
                regroupedRecoveryDelta,
                1f,
                $"Regrouped with allies (allies={allyCount})",
                sensorCenter));
        }

        wasEnemyNear = hasEnemyNear;
        wasEnemyClustered = isEnemyClustered;
        wasIsolated = isIsolated;
        wasRegrouped = isRegrouped;

        lastEnemyCount = enemyCount;
        lastAllyCount = allyCount;

        if (debugLog)
        {
            Debug.Log($"[BattleSensor] enemies={enemyCount} allies={allyCount} danger={GetCurrentDangerString()}");
        }
    }

    private void ApplyPassiveStabilization()
    {
        if (aiVector == null)
            return;

        if (lastEnemyCount > 0)
            return;

        EmitEvent(new DangerEvent(
            DangerEventType.BattlefieldStabilized,
            stabilizedRecoveryDelta,
            1f,
            "No nearby enemies; passive stabilization",
            sensorCenter));
    }

    private void EmitEvent(DangerEvent dangerEvent)
    {
        if (aiVector == null)
            return;

        DangerPerceptionResolver.ApplyTo(aiVector, dangerEvent, perceptionProfile);

        if (debugLog)
        {
            Debug.Log($"[BattleSensor] event={dangerEvent} resolvedDanger={aiVector.CurrentDanger}");
        }
    }

    private int CountNearby(Vector2 center, float radius, LayerMask layerMask, bool includeSelf)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, layerMask);
        if (hits == null || hits.Length == 0)
            return 0;

        int count = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            if (!includeSelf && hit.transform == transform)
                continue;

            if (!includeSelf && sensorCenter != null && hit.transform == sensorCenter)
                continue;

            count++;
        }

        return count;
    }

    private string GetCurrentDangerString()
    {
        return aiVector == null ? "None" : aiVector.CurrentDanger.ToString();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Transform centerTransform = sensorCenter != null ? sensorCenter : transform;
        Vector3 center = centerTransform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, enemyScanRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, allyScanRadius);
    }
}