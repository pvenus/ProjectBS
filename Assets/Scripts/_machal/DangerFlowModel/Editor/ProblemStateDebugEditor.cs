#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProblemStateDebug))]
public class ProblemStateDebugEditor : Editor
{
	private bool showDirectCompare = false;

	public override void OnInspectorGUI()
	{
		ProblemStateDebug state = (ProblemStateDebug)target;

		EditorGUILayout.LabelField("Problem State", EditorStyles.boldLabel);
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.Toggle("Use Situation Option", state.UseSituationOption);
		}

		showDirectCompare = EditorGUILayout.Toggle("Show Direct Compare", showDirectCompare);

		EditorGUILayout.Space();
		DrawLayer(state.Data, showDirectCompare);
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
		Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
		EditorGUI.ProgressBar(rect, Mathf.Clamp01(finalValue), $"{label} Final ({finalValue:0.00})");
		GUILayout.Space(2f);

		if (showDirect)
		{
			Rect directRect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
			EditorGUI.ProgressBar(directRect, Mathf.Clamp01(directValue), $"{label} Direct ({directValue:0.00})");
			GUILayout.Space(2f);
		}
	}
}
#endif