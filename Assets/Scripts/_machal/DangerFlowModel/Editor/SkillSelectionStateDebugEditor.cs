#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillSelectionStateDebug))]
public class SkillSelectionStateDebugEditor : Editor
{
	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.LabelField("Skill Selection State", EditorStyles.boldLabel);
		EditorGUILayout.Space();

		DrawReadonlyProperty("presetName", "Preset");
		DrawReadonlyProperty("hasSkill", "Has Skill");
		DrawReadonlyProperty("selectedSkillId", "Selected Skill Id");
		DrawReadonlyProperty("selectedSkillName", "Selected Skill Name");
		DrawReadonlyProperty("selectedScore", "Selected Score");
		DrawReadonlyProperty("selectedReason", "Selected Reason");

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Candidates", EditorStyles.boldLabel);

		SerializedProperty candidatesProp = serializedObject.FindProperty("candidates");
		if (candidatesProp == null || candidatesProp.arraySize == 0)
		{
			EditorGUILayout.HelpBox("No Candidates", MessageType.Info);
		}
		else
		{
			for (int i = 0; i < candidatesProp.arraySize; i++)
			{
				SerializedProperty item = candidatesProp.GetArrayElementAtIndex(i);
				string displayName = item.FindPropertyRelative("displayName").stringValue;
				bool isUsable = item.FindPropertyRelative("isUsable").boolValue;
				float tacticScore = item.FindPropertyRelative("tacticScore").floatValue;
				string reason = item.FindPropertyRelative("reason").stringValue;

				Rect rect = GUILayoutUtility.GetRect(18f, 18f, "TextField");
				EditorGUI.ProgressBar(rect, Mathf.Clamp01(tacticScore), $"{displayName} ({tacticScore:0.00}) {(isUsable ? "" : "[BLOCKED]")}");
				GUILayout.Space(2f);

				if (!string.IsNullOrEmpty(reason))
					EditorGUILayout.HelpBox(reason, MessageType.None);
			}
		}

		serializedObject.ApplyModifiedProperties();
	}

	private void DrawReadonlyProperty(string propertyName, string label)
	{
		SerializedProperty prop = serializedObject.FindProperty(propertyName);
		using (new EditorGUI.DisabledScope(true))
		{
			EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
		}
	}
}
#endif