using System;
using UnityEngine;

[Serializable]
public class SkillProfileJsonRoot
{
	public SkillProfileJsonItem[] skills;
	public SkillProfileJsonItem[] SkillProfiles;

	public SkillProfileJsonItem[] GetEntries()
	{
		if (skills != null && skills.Length > 0)
			return skills;

		if (SkillProfiles != null && SkillProfiles.Length > 0)
			return SkillProfiles;

		return null;
	}
}

[Serializable]
public class SkillProfileJsonItem
{
	public string skillId;
	public string displayName;
	public string outputTargetMode;
	public string note;

	public float basePriority = 1f;

	[Range(0f, 1f)] public float protect;
	[Range(0f, 1f)] public float recover;
	[Range(0f, 1f)] public float control;
	[Range(0f, 1f)] public float eliminate;
	[Range(0f, 1f)] public float retreat;
}