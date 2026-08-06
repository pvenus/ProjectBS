using UnityEngine;

public class DebugSkillExecutor : MonoBehaviour, ISkillExecutor
{
	[SerializeField] private bool logExecution = true;

	[Header("Last Execution")]
	[SerializeField] private bool hasSkill;
	[SerializeField] private string lastSkillName;
	[SerializeField] private float lastScore;
	[SerializeField][TextArea(2, 5)] private string lastReason;

	public bool Execute(SkillBrainOutput output, Transform caster)
	{
		hasSkill = output.skill != null;
		lastSkillName = output.skill != null ? output.skill.name : "";
		lastScore = output.score;
		lastReason = output.reason;

		if (!hasSkill)
		{
			Debug.LogWarning("[DebugSkillExecutor] No skill to execute.");
			return false;
		}

		if (logExecution)
		{
			Debug.Log(
				"[DebugSkillExecutor]\n" +
				$"Skill: {lastSkillName}\n" +
				$"Score: {lastScore:0.00}\n" +
				$"Reason: {lastReason}");
		}

		return true;
	}
}