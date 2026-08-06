using UnityEngine;

public static class TacticToSkillSelector
{
	public static DangerFlowDecision Select(
		ActivationSet finalTactic,
		SkillProfileAsset[] availableProfiles,
		SemanticDangerEvaluationResult flowResult,
		out SkillSelectionDebugInfo debugInfo)
	{
		debugInfo = new SkillSelectionDebugInfo();
		debugInfo.finalTactic = finalTactic;

		if (availableProfiles == null || availableProfiles.Length == 0)
		{
			debugInfo.selectedSkillId = "";
			debugInfo.selectedReason = "No available profiles";
			return DangerFlowDecision.None;
		}

		float bestScore = float.NegativeInfinity;
		DangerFlowDecision bestDecision = DangerFlowDecision.None;

		for (int i = 0; i < availableProfiles.Length; i++)
		{
			SkillProfileAsset profile = availableProfiles[i];
			if (profile == null)
				continue;

			float tacticScore = CalculateTacticScore(finalTactic, profile.TacticWeights);
			float finalScore = tacticScore * profile.BasePriority;

			bool usable = profile.EnabledInDangerFlow;

			SkillSelectionCandidate candidate = new SkillSelectionCandidate
			{
				skillId = profile.SkillId,
				displayName = profile.DisplayName,
				isUsable = usable,
				tacticScore = finalScore,
				reason = BuildReason(profile, tacticScore, finalScore)
			};

			debugInfo.candidates.Add(candidate);

			if (!usable)
				continue;

			if (finalScore > bestScore)
			{
				bestScore = finalScore;
				bestDecision = new DangerFlowDecision
				{
					skillId = profile.SkillId,
					displayName = profile.DisplayName,
					targetMode = profile.OutputTargetMode,
					score = finalScore,
					reason = candidate.reason,
					flowResult = flowResult,
					selectionDebug = debugInfo
				};
			}
		}

		if (bestDecision.HasSkill)
		{
			debugInfo.selectedSkillId = bestDecision.skillId;
			debugInfo.selectedReason = bestDecision.reason;
			bestDecision.selectionDebug = debugInfo;
			return bestDecision;
		}

		debugInfo.selectedSkillId = "";
		debugInfo.selectedReason = "No usable profile";
		return DangerFlowDecision.None;
	}

	private static float CalculateTacticScore(ActivationSet tactic, TacticWeights weights)
	{
		return
			tactic.Get(SemanticTacticType.Protect.ToString()) * weights.protect +
			tactic.Get(SemanticTacticType.Recover.ToString()) * weights.recover +
			tactic.Get(SemanticTacticType.Control.ToString()) * weights.control +
			tactic.Get(SemanticTacticType.Eliminate.ToString()) * weights.eliminate +
			tactic.Get(SemanticTacticType.Retreat.ToString()) * weights.retreat;
	}

	private static string BuildReason(
		SkillProfileAsset profile,
		float tacticScore,
		float finalScore)
	{
		string usableText = profile.EnabledInDangerFlow
			? "usable"
			: "profile disabled";

		return
			$"usable={usableText}, " +
			$"tacticScore={tacticScore:0.00}, " +
			$"basePriority={profile.BasePriority:0.00}, " +
			$"finalScore={finalScore:0.00}";
	}
}