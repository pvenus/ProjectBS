using UnityEngine;

[System.Serializable]
public struct SkillUseState
{
	public string skillId;
	public BattleSkillBase skill;
	public bool isUsable;
	public Transform target;
	public Vector3 point;
	public string blockReason;

	public static SkillUseState CreateUsable(
		string skillId,
		BattleSkillBase skill,
		Transform target = null,
		Vector3? point = null)
	{
		return new SkillUseState
		{
			skillId = skillId,
			skill = skill,
			isUsable = true,
			target = target,
			point = point ?? Vector3.zero,
			blockReason = string.Empty
		};
	}

	public static SkillUseState CreateBlocked(
		string skillId,
		BattleSkillBase skill,
		string blockReason)
	{
		return new SkillUseState
		{
			skillId = skillId,
			skill = skill,
			isUsable = false,
			target = null,
			point = Vector3.zero,
			blockReason = blockReason ?? string.Empty
		};
	}
}