using System.Collections.Generic;
using UnityEngine;

public class SkillSelectionStateDebug : MonoBehaviour
{
	[Header("Runtime")]
	[SerializeField] private string presetName;
	[SerializeField] private bool hasSkill;
	[SerializeField] private string selectedSkillId;
	[SerializeField] private string selectedSkillName;
	[SerializeField] private float selectedScore;
	[SerializeField][TextArea(2, 5)] private string selectedReason;

	[Header("Candidates")]
	[SerializeField] private List<SkillSelectionCandidate> candidates = new List<SkillSelectionCandidate>();

	public void Apply(string newPresetName, DangerFlowDecision decision)
	{
		presetName = newPresetName ?? string.Empty;
		candidates.Clear();

		if (decision == null)
		{
			hasSkill = false;
			selectedSkillId = string.Empty;
			selectedScore = 0f;
			selectedReason = "Decision is null";
			return;
		}

		hasSkill = decision.HasSkill;
		selectedSkillId = decision.skillId ?? string.Empty;
		selectedScore = decision.score;
		selectedReason = decision.reason ?? string.Empty;

		if (decision.selectionDebug != null && decision.selectionDebug.candidates != null)
		{
			for (int i = 0; i < decision.selectionDebug.candidates.Count; i++)
			{
				SkillSelectionCandidate src = decision.selectionDebug.candidates[i];
				candidates.Add(new SkillSelectionCandidate
				{
					skillId = src.skillId,
					displayName = src.displayName,
					isUsable = src.isUsable,
					tacticScore = src.tacticScore,
					reason = src.reason
				});
			}
		}
	}
}