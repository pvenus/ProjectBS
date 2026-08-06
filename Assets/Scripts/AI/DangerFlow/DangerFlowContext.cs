using UnityEngine;

[System.Serializable]
public struct DangerFlowContext
{
	[Header("Self")]
	[Range(0f, 1f)] public float selfHp01;

	[Header("Allies")]
	[Range(0f, 1f)] public float lowestAllyHp01;
	public bool hasHealTarget;

	[Header("Enemies")]
	public int nearbyEnemyCount;
	[Range(0f, 1f)] public float enemyCluster01;
	public bool isEncircled;
	public bool hasBossNearby;
	[Range(0f, 1f)] public float averageEnemyHp01;

	public SemanticDangerContext ToSemanticContext()
	{
		return new SemanticDangerContext
		{
			selfHp01 = selfHp01,
			lowestAllyHp01 = lowestAllyHp01,
			hasHealTarget = hasHealTarget,
			nearbyEnemyCount = nearbyEnemyCount,
			enemyCluster01 = enemyCluster01,
			isEncircled = isEncircled,
			hasBossNearby = hasBossNearby,
			averageEnemyHp01 = averageEnemyHp01
		};
	}

	public static DangerFlowContext FromBrainContextTemporary(BrainContext context)
	{
		return new DangerFlowContext
		{
			selfHp01 = context.selfHp01,
			lowestAllyHp01 = context.lowestAllyHp01,
			hasHealTarget = context.hasHealTarget,
			nearbyEnemyCount = context.nearbyEnemyCount,

			enemyCluster01 = Mathf.Clamp01(context.nearbyEnemyCount / 10f),
			isEncircled = false,
			hasBossNearby = false,
			averageEnemyHp01 = 0.5f
		};
	}
}