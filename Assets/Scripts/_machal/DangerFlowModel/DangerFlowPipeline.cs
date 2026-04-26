public static class DangerFlowPipeline
{
	public static SemanticDangerEvaluationResult Evaluate(
		SemanticDangerContext context,
		bool useSituationOption)
	{
		SemanticDangerEvaluationResult result = new SemanticDangerEvaluationResult();
		result.context = context;
		result.useSituation = useSituationOption;

		result.directDanger = ContextToDanger.Evaluate(context);
		result.directProblem = DangerToProblem.Evaluate(result.directDanger);
		result.directSolution = ProblemToSolution.Evaluate(result.directProblem);
		result.directTactic = SolutionToTactic.Evaluate(result.directSolution);

		if (!useSituationOption)
		{
			result.situation = ActivationSet.CreateSituationSet();
			result.finalDanger = result.directDanger;
			result.finalProblem = result.directProblem;
			result.finalSolution = result.directSolution;
			result.finalTactic = result.directTactic;
			return result;
		}

		result.situation = ContextToSituation.Evaluate(context);
		result.finalDanger = SituationApplier.Evaluate(
			context,
			result.directDanger,
			result.situation);

		result.finalProblem = DangerToProblem.Evaluate(result.finalDanger);
		result.finalSolution = ProblemToSolution.Evaluate(result.finalProblem);
		result.finalTactic = SolutionToTactic.Evaluate(result.finalSolution);

		return result;
	}
}