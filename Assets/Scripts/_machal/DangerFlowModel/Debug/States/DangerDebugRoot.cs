using UnityEngine;

public class DangerDebugRoot : MonoBehaviour
{
	[Header("Children")]
	[SerializeField] private ContextStateDebug contextState;
	[SerializeField] private SituationStateDebug situationState;
	[SerializeField] private DangerStateDebug dangerState;
	[SerializeField] private ProblemStateDebug problemState;
	[SerializeField] private SolutionStateDebug solutionState;
	[SerializeField] private TacticStateDebug tacticState;

	public void Apply(string presetName, SemanticDangerEvaluationResult result)
	{
		if (result == null)
			return;

		if (contextState != null)
			contextState.Apply(presetName, result.context, result.useSituation);

		if (situationState != null)
			situationState.Apply(result.situation, result.useSituation);

		if (dangerState != null)
			dangerState.Apply(result.finalDanger, result.directDanger, result.useSituation);

		if (problemState != null)
			problemState.Apply(result.finalProblem, result.directProblem, result.useSituation);

		if (solutionState != null)
			solutionState.Apply(result.finalSolution, result.directSolution, result.useSituation);

		if (tacticState != null)
			tacticState.Apply(result.finalTactic, result.directTactic, result.useSituation);
	}
}