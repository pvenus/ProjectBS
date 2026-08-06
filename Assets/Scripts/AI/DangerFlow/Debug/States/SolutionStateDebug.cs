using UnityEngine;

public class SolutionStateDebug : MonoBehaviour
{
	[Header("Runtime")]
	[SerializeField] private bool useSituationOption;

	[Header("Solution")]
	[SerializeField] private ActivationLayerDebugData data = new ActivationLayerDebugData();

	public bool UseSituationOption => useSituationOption;
	public ActivationLayerDebugData Data => data;

	public void Apply(ActivationSet finalSet, ActivationSet directSet, bool useSituation)
	{
		useSituationOption = useSituation;
		data.CopyFrom(finalSet, useSituation ? directSet : null);
	}
}