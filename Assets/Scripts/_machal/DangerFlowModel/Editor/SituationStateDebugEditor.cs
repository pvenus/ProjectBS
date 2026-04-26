#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SituationStateDebug))]
public class SituationStateDebugEditor : Editor
{
	public override void OnInspectorGUI()
	{
		SituationStateDebug state = (SituationStateDebug)target;

		EditorGUILayout.LabelField("Situation State", EditorStyles.boldLabel);
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.Toggle("Use Situation Option", state.UseSituationOption);
		}

		EditorGUILayout.Space();
		DrawLayer(state.FinalData, false);
	}

	private void DrawLayer(ActivationLayerDebugData data, bool showDirect)
	{
		if (data == null || data.values == null || data.values.Count == 0)
		{
			EditorGUILayout.HelpBox("No Data", MessageType.Info);
			return;
		}

		for (int i = 0; i < data.values.Count; i++)
		{
			var item = data.values[i];
			DrawBar(item.key, item.finalValue, item.directValue, showDirect);

			if (!string.IsNullOrEmpty(item.reason))
				EditorGUILayout.HelpBox(item.reason, MessageType.None);
		}
	}

	private void DrawBar(string label, float finalValue, float directValue, bool showDirect)
	{
		if (!showDirect)
		{
			Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
			EditorGUI.ProgressBar(rect, Mathf.Clamp01(finalValue), $"{label} ({finalValue:0.00})");
			GUILayout.Space(2f);
			return;
		}

		EditorGUILayout.LabelField($"{label}  Final:{finalValue:0.00} / Direct:{directValue:0.00}");
	}
}
#endif