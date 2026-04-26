using UnityEngine;

public static class ContextToDanger
{
	public static ActivationSet Evaluate(SemanticDangerContext context)
	{
		ActivationSet danger = ActivationSet.CreateDangerSet();

		float selfFragile = 1f - Mathf.Clamp01(context.selfHp01);
		float allyFragile = context.hasHealTarget ? 1f - Mathf.Clamp01(context.lowestAllyHp01) : 0f;
		float enemyPressure = Mathf.Clamp01(context.nearbyEnemyCount / 10f);
		float clusterPressure = Mathf.Clamp01(context.enemyCluster01);

		float immediate = Mathf.Clamp01(
			(selfFragile * 0.65f) +
			((context.isEncircled ? 1f : 0f) * 0.25f) +
			((context.hasBossNearby ? 1f : 0f) * 0.10f));

		float linked = Mathf.Clamp01(
			(allyFragile * 0.80f) +
			((context.hasHealTarget ? 1f : 0f) * 0.10f) +
			(enemyPressure * 0.10f));

		float ambient = Mathf.Clamp01(
			(enemyPressure * 0.60f) +
			(clusterPressure * 0.30f) +
			((context.hasBossNearby ? 1f : 0f) * 0.10f));

		danger.Set(
			SemanticDangerType.Immediate.ToString(),
			immediate,
			$"selfFragile={selfFragile:0.00}, encircled={context.isEncircled}, boss={context.hasBossNearby}");

		danger.Set(
			SemanticDangerType.Linked.ToString(),
			linked,
			$"allyFragile={allyFragile:0.00}, hasHealTarget={context.hasHealTarget}, enemyPressure={enemyPressure:0.00}");

		danger.Set(
			SemanticDangerType.Ambient.ToString(),
			ambient,
			$"enemyPressure={enemyPressure:0.00}, cluster={clusterPressure:0.00}, boss={context.hasBossNearby}");

		return danger;
	}
}