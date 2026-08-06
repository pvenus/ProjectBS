using System.Text;
using UnityEngine;

public class DangerDebugRunner : MonoBehaviour
{
	[Header("Runner")]
	[SerializeField] private DangerFlowRunner flowRunner;

	[Header("Context Source")]
	[SerializeField] private bool useManualContext = false;
	[SerializeField]
	private DangerFlowContext manualContext = new DangerFlowContext
	{
		selfHp01 = 0.5f,
		lowestAllyHp01 = 0.5f,
		hasHealTarget = false,
		nearbyEnemyCount = 5,
		enemyCluster01 = 0.5f,
		isEncircled = false,
		hasBossNearby = false,
		averageEnemyHp01 = 0.5f
	};

	[Header("Preset")]
	[SerializeField] private DangerContextPreset[] presets;
	[SerializeField] private int selectedPresetIndex = 0;

	[Header("Debug Target")]
	[SerializeField] private DangerDebugRoot debugRoot;
	[SerializeField] private SkillSelectionStateDebug selectionDebugState;

	[Header("Debug")]
	[SerializeField] private bool evaluateOnStart = true;
	[SerializeField] private bool evaluateEveryFrame = false;
	[SerializeField] private bool logToConsole = true;

	private DangerFlowDecision lastDecision;
	private string lastPresetName = "(none)";

	public DangerFlowDecision LastDecision => lastDecision;
	public string LastPresetName => lastPresetName;
	public int SelectedPresetIndex => selectedPresetIndex;
	public int PresetCount => presets != null ? presets.Length : 0;
	public bool UseManualContext => useManualContext;

	private void Start()
	{
		if (evaluateOnStart)
			EvaluateNow();
	}

	private void Update()
	{
		if (evaluateEveryFrame)
			EvaluateNow();

		if (Input.GetKeyDown(KeyCode.Space))
			EvaluateNow();
	}

	public void EvaluateNow()
	{
		if (flowRunner == null)
		{
			Debug.LogWarning("[DangerDebugRunner] flowRunner is null.");
			return;
		}

		DangerFlowContext context = BuildContext(out string contextName);
		lastPresetName = contextName;

		lastDecision = flowRunner.EvaluateDecision(context);

		if (debugRoot != null && lastDecision.flowResult != null)
			debugRoot.Apply(contextName, lastDecision.flowResult);

		if (selectionDebugState != null)
			selectionDebugState.Apply(contextName, lastDecision);

		if (logToConsole)
			PrintToConsole(lastDecision, contextName);
	}

	public void SelectPreset(int index)
	{
		if (presets == null || presets.Length == 0)
			return;

		selectedPresetIndex = Mathf.Clamp(index, 0, presets.Length - 1);
		useManualContext = false;
		EvaluateNow();
	}

	public void SelectNextPreset()
	{
		if (presets == null || presets.Length == 0)
			return;

		selectedPresetIndex++;
		if (selectedPresetIndex >= presets.Length)
			selectedPresetIndex = 0;

		useManualContext = false;
		EvaluateNow();
	}

	public void SelectPreviousPreset()
	{
		if (presets == null || presets.Length == 0)
			return;

		selectedPresetIndex--;
		if (selectedPresetIndex < 0)
			selectedPresetIndex = presets.Length - 1;

		useManualContext = false;
		EvaluateNow();
	}

	public void UseManualAndEvaluate()
	{
		useManualContext = true;
		EvaluateNow();
	}

	private DangerFlowContext BuildContext(out string contextName)
	{
		if (useManualContext)
		{
			contextName = "(manual)";
			return manualContext;
		}

		contextName = "(empty)";

		if (presets != null &&
			presets.Length > 0 &&
			selectedPresetIndex >= 0 &&
			selectedPresetIndex < presets.Length &&
			presets[selectedPresetIndex] != null)
		{
			contextName = presets[selectedPresetIndex].PresetId;
			return presets[selectedPresetIndex].Context;
		}

		return default;
	}

	private void PrintToConsole(DangerFlowDecision decision, string contextName)
	{
		string selectedSkill = decision != null && decision.HasSkill
			? decision.displayName
			: "(none)";

		StringBuilder sb = new StringBuilder();

		sb.AppendLine("[DangerDebugRunner]");
		sb.AppendLine($"Context: {contextName}");
		sb.AppendLine($"SelectedSkill: {selectedSkill}");
		sb.AppendLine($"SkillId: {(decision != null ? decision.skillId : "")}");
		sb.AppendLine($"Score: {(decision != null ? decision.score : 0f):0.00}");
		sb.AppendLine($"Reason: {(decision != null ? decision.reason : "")}");

		if (decision != null && decision.flowResult != null)
			sb.AppendLine(BuildSetText("FinalTactic", decision.flowResult.finalTactic));

		if (decision != null && decision.selectionDebug != null)
		{
			sb.AppendLine("Candidates:");

			if (decision.selectionDebug.candidates == null ||
				decision.selectionDebug.candidates.Count == 0)
			{
				sb.AppendLine("- No candidates");
			}
			else
			{
				for (int i = 0; i < decision.selectionDebug.candidates.Count; i++)
				{
					SkillSelectionCandidate candidate = decision.selectionDebug.candidates[i];

					sb.AppendLine(
						$"- {candidate.displayName} / " +
						$"id={candidate.skillId} / " +
						$"usable={candidate.isUsable} / " +
						$"score={candidate.tacticScore:0.00} / " +
						$"{candidate.reason}");
				}
			}
		}

		Debug.Log(sb.ToString());
	}

	private string BuildSetText(string title, ActivationSet set)
	{
		if (set == null)
			return $"{title}: null";

		StringBuilder sb = new StringBuilder();
		sb.AppendLine(title);

		for (int i = 0; i < set.Values.Count; i++)
		{
			ActivationValue item = set.Values[i];
			sb.AppendLine($"- {item.key}: {item.value:0.00} ({item.reason})");
		}

		return sb.ToString();
	}
}