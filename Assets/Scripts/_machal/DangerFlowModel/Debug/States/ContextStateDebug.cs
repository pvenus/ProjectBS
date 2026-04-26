using UnityEngine;

public class ContextStateDebug : MonoBehaviour
{
	[Header("Runtime")]
	[SerializeField] private string presetName;
	[SerializeField] private bool useSituationOption;

	[Header("Context")]
	[SerializeField] private SemanticDangerContext context;

	public string PresetName => presetName;
	public bool UseSituationOption => useSituationOption;
	public SemanticDangerContext Context => context;

	public void Apply(string newPresetName, SemanticDangerContext newContext, bool useSituation)
	{
		presetName = newPresetName ?? string.Empty;
		context = newContext;
		useSituationOption = useSituation;
	}
}