using UnityEngine;

public static class ProblemToSolution
{
	public static ActivationSet Evaluate(ActivationSet problem)
	{
		ActivationSet solution = ActivationSet.CreateSolutionSet();

		float survival = problem.Get(SemanticProblemType.Survival.ToString());
		float rescue = problem.Get(SemanticProblemType.Rescue.ToString());
		float relief = problem.Get(SemanticProblemType.Relief.ToString());
		float opportunity = problem.Get(SemanticProblemType.Opportunity.ToString());

		float guard = Mathf.Clamp01(
			(survival * 0.75f) +
			(rescue * 0.20f));

		float control = Mathf.Clamp01(
			(relief * 0.75f) +
			(survival * 0.10f) +
			(opportunity * 0.05f));

		float support = Mathf.Clamp01(
			(rescue * 0.75f) +
			(survival * 0.10f));

		float burst = Mathf.Clamp01(
			(opportunity * 0.80f) +
			((1f - survival) * 0.10f));

		solution.Set(
			SemanticSolutionType.Guard.ToString(),
			guard,
			$"survival={survival:0.00}, rescue={rescue:0.00}");

		solution.Set(
			SemanticSolutionType.Control.ToString(),
			control,
			$"relief={relief:0.00}, survival={survival:0.00}, opportunity={opportunity:0.00}");

		solution.Set(
			SemanticSolutionType.Support.ToString(),
			support,
			$"rescue={rescue:0.00}, survival={survival:0.00}");

		solution.Set(
			SemanticSolutionType.Burst.ToString(),
			burst,
			$"opportunity={opportunity:0.00}, invSurvival={(1f - survival):0.00}");

		return solution;
	}
}