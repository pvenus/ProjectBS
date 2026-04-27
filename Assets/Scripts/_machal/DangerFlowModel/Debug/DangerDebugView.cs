using UnityEngine;

public class DangerDebugView : MonoBehaviour
{
	[Header("Target")]
	[SerializeField] private DangerDebugRunner runner;

	[Header("GUI")]
	[SerializeField] private bool showGui = true;

	private void OnGUI()
	{
		if (!showGui)
			return;

		GUILayout.BeginArea(new Rect(20f, 20f, 380f, 260f), GUI.skin.box);

		GUILayout.Label("Danger Flow Debug");

		if (runner == null)
		{
			GUILayout.Label("Runner is null.");
			GUILayout.EndArea();
			return;
		}

		GUILayout.Label($"Context: {runner.LastPresetName}");
		GUILayout.Label($"Preset Index: {runner.SelectedPresetIndex + 1} / {runner.PresetCount}");
		GUILayout.Label($"Manual Context: {runner.UseManualContext}");

		DangerFlowDecision decision = runner.LastDecision;
		string selected = decision != null && decision.HasSkill
			? decision.displayName
			: "(none)";

		GUILayout.Label($"Selected Skill: {selected}");

		if (decision != null)
		{
			GUILayout.Label($"SkillId: {decision.skillId}");
			GUILayout.Label($"Score: {decision.score:0.00}");
		}

		GUILayout.Space(8f);

		if (GUILayout.Button("Evaluate Current"))
			runner.EvaluateNow();

		if (GUILayout.Button("Evaluate Manual Context"))
			runner.UseManualAndEvaluate();

		GUILayout.BeginHorizontal();

		if (GUILayout.Button("Prev Preset"))
			runner.SelectPreviousPreset();

		if (GUILayout.Button("Next Preset"))
			runner.SelectNextPreset();

		GUILayout.EndHorizontal();

		GUILayout.Space(8f);
		GUILayout.Label("Details:");
		GUILayout.Label("- DangerDebugRoot");
		GUILayout.Label("- SkillSelectionStateDebug");

		GUILayout.EndArea();
	}
}