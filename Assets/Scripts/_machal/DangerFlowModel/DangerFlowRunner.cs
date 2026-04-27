using UnityEngine;

public class DangerFlowRunner : MonoBehaviour
{
	[Header("Flow")]
	[SerializeField] private bool useSituationOption = true;

	[Header("Available Profiles")]
	[SerializeField] private SkillProfileAsset[] availableProfiles;

	public DangerFlowDecision EvaluateDecision(BrainContext brainContext)
	{
		DangerFlowContext context =
			DangerFlowContext.FromBrainContextTemporary(brainContext);

		return EvaluateDecision(context);
	}

	public DangerFlowDecision EvaluateDecision(DangerFlowContext context)
	{
		SemanticDangerEvaluationResult flowResult =
			DangerFlowPipeline.Evaluate(
				context.ToSemanticContext(),
				useSituationOption);

		SkillSelectionDebugInfo selectionDebug;
		DangerFlowDecision decision = TacticToSkillSelector.Select(
			flowResult.finalTactic,
			availableProfiles,
			flowResult,
			out selectionDebug);

		decision.flowResult = flowResult;
		decision.selectionDebug = selectionDebug;

		return decision;
	}
}