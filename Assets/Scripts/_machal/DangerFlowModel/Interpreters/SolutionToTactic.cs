using UnityEngine;

public static class SolutionToTactic
{
	public static ActivationSet Evaluate(ActivationSet solution)
	{
		ActivationSet tactic = ActivationSet.CreateTacticSet();

		float guard = solution.Get(SemanticSolutionType.Guard.ToString());
		float control = solution.Get(SemanticSolutionType.Control.ToString());
		float support = solution.Get(SemanticSolutionType.Support.ToString());
		float burst = solution.Get(SemanticSolutionType.Burst.ToString());

		float protect = Mathf.Clamp01(
			(guard * 0.70f) +
			(support * 0.20f));

		float recover = Mathf.Clamp01(
			(support * 0.80f) +
			(guard * 0.10f));

		float controlTactic = Mathf.Clamp01(
			(control * 0.85f) +
			(guard * 0.05f));

		float eliminate = Mathf.Clamp01(
			(burst * 0.85f) +
			(control * 0.05f));

		float retreat = Mathf.Clamp01(
			(guard * 0.50f) +
			((1f - burst) * 0.15f));

		tactic.Set(
			SemanticTacticType.Protect.ToString(),
			protect,
			$"guard={guard:0.00}, support={support:0.00}");

		tactic.Set(
			SemanticTacticType.Recover.ToString(),
			recover,
			$"support={support:0.00}, guard={guard:0.00}");

		tactic.Set(
			SemanticTacticType.Control.ToString(),
			controlTactic,
			$"control={control:0.00}, guard={guard:0.00}");

		tactic.Set(
			SemanticTacticType.Eliminate.ToString(),
			eliminate,
			$"burst={burst:0.00}, control={control:0.00}");

		tactic.Set(
			SemanticTacticType.Retreat.ToString(),
			retreat,
			$"guard={guard:0.00}, invBurst={(1f - burst):0.00}");

		return tactic;
	}
}