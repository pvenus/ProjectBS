using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TacticWeights
{
	[Range(0f, 1f)] public float protect;
	[Range(0f, 1f)] public float recover;
	[Range(0f, 1f)] public float control;
	[Range(0f, 1f)] public float eliminate;
	[Range(0f, 1f)] public float retreat;
}

[System.Serializable]
public class SkillSelectionCandidate
{
	public string skillId;
	public string displayName;
	public bool isUsable;
	public float tacticScore;
	public string reason;
}

[System.Serializable]
public class SkillSelectionDebugInfo
{
	public ActivationSet finalTactic;
	public List<SkillSelectionCandidate> candidates = new List<SkillSelectionCandidate>();
	public string selectedSkillId;
	public string selectedReason;
}