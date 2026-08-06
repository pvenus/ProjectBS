using UnityEngine;

public static class SituationApplier
{
	public static ActivationSet Evaluate(
		SemanticDangerContext context,
		ActivationSet directDanger,
		ActivationSet situation)
	{
		ActivationSet result = ActivationSet.CreateDangerSet();

		float directImmediate = directDanger.Get(SemanticDangerType.Immediate.ToString());
		float directLinked = directDanger.Get(SemanticDangerType.Linked.ToString());
		float directAmbient = directDanger.Get(SemanticDangerType.Ambient.ToString());

		float swarm = situation.Get(SemanticSituationType.Swarm.ToString());
		float weak = situation.Get(SemanticSituationType.Weak.ToString());
		float encircle = situation.Get(SemanticSituationType.Encircle.ToString());
		float allyFragile = situation.Get(SemanticSituationType.AllyFragile.ToString());
		float selfFragile = situation.Get(SemanticSituationType.SelfFragile.ToString());
		float boss = situation.Get(SemanticSituationType.Boss.ToString());

		float immediateBonus =
			(encircle * 0.22f) +
			(selfFragile * 0.12f) +
			(boss * 0.08f);

		float linkedBonus =
			(allyFragile * 0.20f) +
			(swarm * 0.05f);

		float ambientBonus =
			(swarm * 0.18f) +
			(weak * 0.08f) +
			(boss * 0.06f);

		float finalImmediate = Mathf.Clamp01(directImmediate + immediateBonus);
		float finalLinked = Mathf.Clamp01(directLinked + linkedBonus);
		float finalAmbient = Mathf.Clamp01(directAmbient + ambientBonus);

		result.Set(
			SemanticDangerType.Immediate.ToString(),
			finalImmediate,
			$"direct={directImmediate:0.00} + encircle({encircle:0.00}) + selfFragile({selfFragile:0.00}) + boss({boss:0.00})");

		result.Set(
			SemanticDangerType.Linked.ToString(),
			finalLinked,
			$"direct={directLinked:0.00} + allyFragile({allyFragile:0.00}) + swarm({swarm:0.00})");

		result.Set(
			SemanticDangerType.Ambient.ToString(),
			finalAmbient,
			$"direct={directAmbient:0.00} + swarm({swarm:0.00}) + weak({weak:0.00}) + boss({boss:0.00})");

		return result;
	}
}