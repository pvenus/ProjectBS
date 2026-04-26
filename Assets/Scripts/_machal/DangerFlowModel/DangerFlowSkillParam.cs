using UnityEngine;

[System.Serializable]
public struct DangerFlowSkillParam
{
	public SkillProfileAsset profile;
	public SkillRuntimeState runtimeState;

	public bool IsUsable
	{
		get
		{
			return profile != null &&
				   profile.EnabledInDangerFlow &&
				   runtimeState.skill != null &&
				   runtimeState.isUsable &&
				   profile.SkillId == runtimeState.skillId;
		}
	}

	public string SkillId
	{
		get
		{
			if (profile != null)
				return profile.SkillId;

			return runtimeState.skillId;
		}
	}

	public string DisplayName
	{
		get
		{
			if (profile != null)
				return profile.DisplayName;

			return runtimeState.skillId;
		}
	}
}