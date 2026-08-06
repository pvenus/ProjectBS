#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContextStateDebug))]
public class ContextStateDebugEditor : Editor
{
	public override void OnInspectorGUI()
	{
		ContextStateDebug state = (ContextStateDebug)target;

		EditorGUILayout.LabelField("Context State", EditorStyles.boldLabel);
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.TextField("Preset", state.PresetName);
			EditorGUILayout.Toggle("Use Situation Option", state.UseSituationOption);
		}

		EditorGUILayout.Space();

		var context = state.Context;
		DrawProgress("Self Hp", context.selfHp01);
		DrawProgress("Lowest Ally Hp", context.lowestAllyHp01);
		DrawBool("Has Heal Target", context.hasHealTarget);
		DrawIntLike01("Nearby Enemy Count", context.nearbyEnemyCount, 15f);
		DrawProgress("Enemy Cluster", context.enemyCluster01);
		DrawBool("Is Encircled", context.isEncircled);
		DrawBool("Has Boss Nearby", context.hasBossNearby);
		DrawProgress("Average Enemy Hp", context.averageEnemyHp01);
	}

	private void DrawProgress(string label, float value)
	{
		Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
		EditorGUI.ProgressBar(rect, Mathf.Clamp01(value), $"{label} ({value:0.00})");
		GUILayout.Space(2f);
	}

	private void DrawBool(string label, bool value)
	{
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.Toggle(label, value);
		}
	}

	private void DrawIntLike01(string label, int value, float max)
	{
		float normalized = max <= 0f ? 0f : Mathf.Clamp01(value / max);
		Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
		EditorGUI.ProgressBar(rect, normalized, $"{label} ({value})");
		GUILayout.Space(2f);
	}
}
#endif