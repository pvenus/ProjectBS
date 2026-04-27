#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SkillProfileJsonImporter
{
	[MenuItem("Machal/Danger Flow/Import Skill Profiles From Json")]
	public static void ImportFromJsonFile()
	{
		string jsonPath = EditorUtility.OpenFilePanel(
			"Select Skill Profile Json",
			Application.dataPath,
			"json");

		if (string.IsNullOrEmpty(jsonPath))
			return;

		string json = File.ReadAllText(jsonPath);
		ImportFromJson(json, "Assets/_machal89/DangerFlowProfiles");
	}

	public static void ImportFromJson(string json, string outputFolder)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			Debug.LogError("SkillProfileJsonImporter: json is empty");
			return;
		}

		SkillProfileJsonRoot root = JsonUtility.FromJson<SkillProfileJsonRoot>(json);
		SkillProfileJsonItem[] entries = root != null ? root.GetEntries() : null;

		if (entries == null || entries.Length == 0)
		{
			Debug.LogError("SkillProfileJsonImporter: no skill entries");
			return;
		}

		EnsureFolder(outputFolder);

		for (int i = 0; i < entries.Length; i++)
		{
			SkillProfileJsonItem item = entries[i];

			if (item == null || string.IsNullOrWhiteSpace(item.skillId))
				continue;

			string assetPath = $"{outputFolder}/{item.skillId}.asset";
			SkillProfileAsset asset = AssetDatabase.LoadAssetAtPath<SkillProfileAsset>(assetPath);

			if (asset == null)
			{
				asset = ScriptableObject.CreateInstance<SkillProfileAsset>();
				AssetDatabase.CreateAsset(asset, assetPath);
			}

			SerializedObject so = new SerializedObject(asset);

			so.FindProperty("skillId").stringValue = item.skillId;
			so.FindProperty("displayName").stringValue =
				string.IsNullOrWhiteSpace(item.displayName) ? item.skillId : item.displayName;

			so.FindProperty("note").stringValue = item.note ?? string.Empty;

			SkillOutputTargetMode targetMode = ParseTargetMode(item.outputTargetMode);
			so.FindProperty("outputTargetMode").enumValueIndex = (int)targetMode;

			SerializedProperty enabledProp = so.FindProperty("enabledInDangerFlow");
			if (enabledProp != null)
				enabledProp.boolValue = true;

			SerializedProperty priorityProp = so.FindProperty("basePriority");
			if (priorityProp != null)
				priorityProp.floatValue = item.basePriority <= 0f ? 1f : item.basePriority;

			SerializedProperty tactic = so.FindProperty("tacticWeights");
			tactic.FindPropertyRelative("protect").floatValue = Mathf.Clamp01(item.protect);
			tactic.FindPropertyRelative("recover").floatValue = Mathf.Clamp01(item.recover);
			tactic.FindPropertyRelative("control").floatValue = Mathf.Clamp01(item.control);
			tactic.FindPropertyRelative("eliminate").floatValue = Mathf.Clamp01(item.eliminate);
			tactic.FindPropertyRelative("retreat").floatValue = Mathf.Clamp01(item.retreat);

			so.ApplyModifiedPropertiesWithoutUndo();
			EditorUtility.SetDirty(asset);
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log($"SkillProfileJsonImporter: imported {entries.Length} profiles");
	}

	private static SkillOutputTargetMode ParseTargetMode(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return SkillOutputTargetMode.None;

		if (System.Enum.TryParse(value, true, out SkillOutputTargetMode mode))
			return mode;

		return SkillOutputTargetMode.None;
	}

	private static void EnsureFolder(string folderPath)
	{
		if (AssetDatabase.IsValidFolder(folderPath))
			return;

		string[] parts = folderPath.Split('/');
		string current = parts[0];

		for (int i = 1; i < parts.Length; i++)
		{
			string next = current + "/" + parts[i];

			if (!AssetDatabase.IsValidFolder(next))
				AssetDatabase.CreateFolder(current, parts[i]);

			current = next;
		}
	}
}
#endif