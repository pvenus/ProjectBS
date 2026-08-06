using UnityEngine;

[System.Serializable]
public struct SkillRuntimeState
{
	public string skillId;
	public BattleSkillBase skill;
	public bool isUsable;
	public Transform target;
	public Vector3 point;
	public string blockReason;

	public static SkillRuntimeState Usable(
		string skillId,
		BattleSkillBase skill,
		Transform target = null,
		Vector3? point = null)
	{
		return new SkillRuntimeState
		{
			skillId = skillId,
			skill = skill,
			isUsable = true,
			target = target,
			point = point ?? Vector3.zero,
			blockReason = ""
		};
	}

	public static SkillRuntimeState Blocked(
		string skillId,
		BattleSkillBase skill,
		string reason)
	{
		return new SkillRuntimeState
		{
			skillId = skillId,
			skill = skill,
			isUsable = false,
			target = null,
			point = Vector3.zero,
			blockReason = reason ?? ""
		};
	}
}