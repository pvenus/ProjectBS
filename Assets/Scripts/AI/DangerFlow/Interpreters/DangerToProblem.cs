using UnityEngine;

public static class DangerToProblem
{
	public static ActivationSet Evaluate(ActivationSet danger)
	{
		ActivationSet problem = ActivationSet.CreateProblemSet();

		float immediate = danger.Get(SemanticDangerType.Immediate.ToString());
		float linked = danger.Get(SemanticDangerType.Linked.ToString());
		float ambient = danger.Get(SemanticDangerType.Ambient.ToString());

		float survival = Mathf.Clamp01(
			(immediate * 0.75f) +
			(ambient * 0.10f));

		float rescue = Mathf.Clamp01(
			(linked * 0.80f) +
			(immediate * 0.10f));

		float relief = Mathf.Clamp01(
			(ambient * 0.70f) +
			(immediate * 0.15f) +
			(linked * 0.10f));

		float opportunity = Mathf.Clamp01(
			((1f - immediate) * 0.35f) +
			((1f - linked) * 0.15f) +
			(ambient * 0.15f));

		problem.Set(
			SemanticProblemType.Survival.ToString(),
			survival,
			$"immediate={immediate:0.00}, ambient={ambient:0.00}");

		problem.Set(
			SemanticProblemType.Rescue.ToString(),
			rescue,
			$"linked={linked:0.00}, immediate={immediate:0.00}");

		problem.Set(
			SemanticProblemType.Relief.ToString(),
			relief,
			$"ambient={ambient:0.00}, immediate={immediate:0.00}, linked={linked:0.00}");

		problem.Set(
			SemanticProblemType.Opportunity.ToString(),
			opportunity,
			$"invImmediate={(1f - immediate):0.00}, invLinked={(1f - linked):0.00}, ambient={ambient:0.00}");

		return problem;
	}
}