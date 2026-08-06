using UnityEngine;

[CreateAssetMenu(
	fileName = "DangerContextPreset",
	menuName = "Machal/Danger Flow/Danger Context Preset")]
public class DangerContextPreset : ScriptableObject
{
	[SerializeField] private string presetId = "NewPreset";
	[SerializeField][TextArea(2, 5)] private string description = "";
	[SerializeField] private DangerFlowContext context;

	public string PresetId => presetId;
	public string Description => description;
	public DangerFlowContext Context => context;
}