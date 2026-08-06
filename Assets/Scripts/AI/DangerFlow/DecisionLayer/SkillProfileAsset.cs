using UnityEngine;

[CreateAssetMenu(
	fileName = "SkillProfile",
	menuName = "Machal/Danger Flow/Skill Profile")]
public class SkillProfileAsset : ScriptableObject
{
	[Header("Identity")]
	[SerializeField] private string skillId;
	[SerializeField] private string displayName;

	[Header("Tactic Profile")]
	[SerializeField] private TacticWeights tacticWeights;

	[Header("Output")]
	[SerializeField] private SkillOutputTargetMode outputTargetMode = SkillOutputTargetMode.None;

	[Header("Static Selection Info")]
	[SerializeField] private bool enabledInDangerFlow = true;
	[SerializeField] private float basePriority = 1f;

	[TextArea(2, 4)]
	[SerializeField] private string note;

	public string SkillId => string.IsNullOrWhiteSpace(skillId) ? name : skillId;
	public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? SkillId : displayName;
	public TacticWeights TacticWeights => tacticWeights;
	public SkillOutputTargetMode OutputTargetMode => outputTargetMode;
	public bool EnabledInDangerFlow => enabledInDangerFlow;
	public float BasePriority => Mathf.Max(0f, basePriority);
	public string Note => note;
}