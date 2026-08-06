using UnityEngine;

[System.Serializable]
public class DangerFlowDecision
{
	public string skillId;
	public string displayName;
	public SkillOutputTargetMode targetMode;
	public float score;
	public string reason;

	public SemanticDangerEvaluationResult flowResult;
	public SkillSelectionDebugInfo selectionDebug;

	public bool HasSkill => !string.IsNullOrWhiteSpace(skillId);

	public static DangerFlowDecision None => new DangerFlowDecision
	{
		skillId = "",
		displayName = "",
		targetMode = SkillOutputTargetMode.None,
		score = float.NegativeInfinity,
		reason = "No decision",
		flowResult = null,
		selectionDebug = null
	};
}