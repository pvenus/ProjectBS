using UnityEngine;

public static class ContextToSituation
{
	public static ActivationSet Evaluate(SemanticDangerContext context)
	{
		ActivationSet situation = ActivationSet.CreateSituationSet();

		float swarm = EvaluateSwarm(context);
		float weak = EvaluateWeak(context);
		float encircle = context.isEncircled ? 1f : Mathf.Clamp01((context.enemyCluster01 * 0.4f) + (NormalizeEnemyCount(context.nearbyEnemyCount) * 0.2f));
		float allyFragile = context.hasHealTarget ? 1f - Mathf.Clamp01(context.lowestAllyHp01) : 0f;
		float selfFragile = 1f - Mathf.Clamp01(context.selfHp01);
		float boss = context.hasBossNearby ? 1f : 0f;

		situation.Set(
			SemanticSituationType.Swarm.ToString(),
			swarm,
			$"enemyCount={context.nearbyEnemyCount}, cluster={context.enemyCluster01:0.00}");

		situation.Set(
			SemanticSituationType.Weak.ToString(),
			weak,
			$"avgEnemyHp01={context.averageEnemyHp01:0.00}");

		situation.Set(
			SemanticSituationType.Encircle.ToString(),
			encircle,
			$"isEncircled={context.isEncircled}, cluster={context.enemyCluster01:0.00}");

		situation.Set(
			SemanticSituationType.AllyFragile.ToString(),
			allyFragile,
			$"hasHealTarget={context.hasHealTarget}, lowestAllyHp01={context.lowestAllyHp01:0.00}");

		situation.Set(
			SemanticSituationType.SelfFragile.ToString(),
			selfFragile,
			$"selfHp01={context.selfHp01:0.00}");

		situation.Set(
			SemanticSituationType.Boss.ToString(),
			boss,
			$"hasBossNearby={context.hasBossNearby}");

		return situation;
	}

	private static float EvaluateSwarm(SemanticDangerContext context)
	{
		float enemyCount01 = NormalizeEnemyCount(context.nearbyEnemyCount);
		float cluster01 = Mathf.Clamp01(context.enemyCluster01);
		return Mathf.Clamp01((enemyCount01 * 0.65f) + (cluster01 * 0.35f));
	}

	private static float EvaluateWeak(SemanticDangerContext context)
	{
		return 1f - Mathf.Clamp01(context.averageEnemyHp01);
	}

	private static float NormalizeEnemyCount(int count)
	{
		return Mathf.Clamp01(count / 10f);
	}
}