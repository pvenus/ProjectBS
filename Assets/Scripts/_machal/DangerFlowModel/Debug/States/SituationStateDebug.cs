using UnityEngine;

public class SituationStateDebug : MonoBehaviour
{
	[Header("Runtime")]
	[SerializeField] private bool useSituationOption;

	[Header("Situation Final")]
	[SerializeField] private ActivationLayerDebugData finalData = new ActivationLayerDebugData();

	public bool UseSituationOption => useSituationOption;
	public ActivationLayerDebugData FinalData => finalData;

	public void Apply(ActivationSet finalSet, bool useSituation)
	{
		useSituationOption = useSituation;
		finalData.CopyFrom(finalSet, null);
	}
}