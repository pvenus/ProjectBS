using System;
using System.Collections.Generic;
using System.IO;
using Common.SO;
using Stage;
using UnityEditor;
using UnityEngine;

public static class LibraryGenerator
{
    private const string MenuRoot = "Assets/Library";

    [MenuItem(MenuRoot + "/Generate Libraries From Json", false, 2000)]
    public static void GenerateFromSelectedJson()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
        {
            Debug.LogWarning("[LibraryGenerator] Select a library json file first.");
            return;
        }

        string jsonPath = AssetDatabase.GetAssetPath(selected);

        if (string.IsNullOrEmpty(jsonPath) ||
            !jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning($"[LibraryGenerator] Selected asset is not a json file. path={jsonPath}");
            return;
        }

        GenerateFromJsonPath(jsonPath);
    }

    [MenuItem(MenuRoot + "/Generate Libraries From Json", true)]
    public static bool ValidateGenerateFromSelectedJson()
    {
        UnityEngine.Object selected = Selection.activeObject;

        if (selected == null)
        {
            return false;
        }

        string jsonPath = AssetDatabase.GetAssetPath(selected);

        return !string.IsNullOrEmpty(jsonPath) &&
            jsonPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
    }

    public static void GenerateFromJsonPath(string jsonPath)
    {
        TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);

        if (jsonAsset == null)
        {
            Debug.LogWarning($"[LibraryGenerator] Json asset not found. path={jsonPath}");
            return;
        }

        LibraryJsonData data = JsonUtility.FromJson<LibraryJsonData>(jsonAsset.text);

        if (data == null)
        {
            Debug.LogWarning($"[LibraryGenerator] Invalid library json. path={jsonPath}");
            return;
        }

        string outputFolder = Path.GetDirectoryName(jsonPath);

        RewardVisualLibrarySO rewardVisualLibrary = GenerateRewardVisualLibrary(
            data,
            outputFolder);

        NodeTypeIconLibrarySO nodeTypeIconLibrary = GenerateNodeTypeIconLibrary(
            data,
            outputFolder);

        EditorUtility.SetDirty(rewardVisualLibrary);
        EditorUtility.SetDirty(nodeTypeIconLibrary);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[LibraryGenerator] Generated libraries. reward={rewardVisualLibrary?.name} node={nodeTypeIconLibrary?.name}");
    }

    private static RewardVisualLibrarySO GenerateRewardVisualLibrary(
        LibraryJsonData data,
        string outputFolder)
    {
        string assetPath = NormalizeAssetPath(
            Path.Combine(outputFolder, "RewardVisualLibrary.asset"));

        RewardVisualLibrarySO library =
            AssetDatabase.LoadAssetAtPath<RewardVisualLibrarySO>(assetPath);

        if (library == null)
        {
            library = ScriptableObject.CreateInstance<RewardVisualLibrarySO>();
            AssetDatabase.CreateAsset(library, assetPath);
        }

        List<RewardVisualLibrarySO.RewardVisualEntry> entries = new();

        if (data.rewardVisuals != null)
        {
            for (int i = 0; i < data.rewardVisuals.Count; i++)
            {
                RewardVisualJsonData item = data.rewardVisuals[i];

                if (item == null)
                {
                    continue;
                }

                entries.Add(new RewardVisualLibrarySO.RewardVisualEntry
                {
                    rewardType = ParseEnum(
                        item.rewardType,
                        PopupEventRewardType.None),
                    localizationMainKey = item.localizationMainKey,
                    icon = FindSprite(item.icon)
                });
            }
        }

        SetSerializedList(
            library,
            "visuals",
            entries);

        EditorUtility.SetDirty(library);
        return library;
    }

    private static NodeTypeIconLibrarySO GenerateNodeTypeIconLibrary(
        LibraryJsonData data,
        string outputFolder)
    {
        string assetPath = NormalizeAssetPath(
            Path.Combine(outputFolder, "NodeTypeIconLibrary.asset"));

        NodeTypeIconLibrarySO library =
            AssetDatabase.LoadAssetAtPath<NodeTypeIconLibrarySO>(assetPath);

        if (library == null)
        {
            library = ScriptableObject.CreateInstance<NodeTypeIconLibrarySO>();
            AssetDatabase.CreateAsset(library, assetPath);
        }

        List<NodeTypeIconLibrarySO.NodeTypeIconEntry> entries = new();

        if (data.nodeTypeIcons != null)
        {
            for (int i = 0; i < data.nodeTypeIcons.Count; i++)
            {
                NodeTypeIconJsonData item = data.nodeTypeIcons[i];

                if (item == null)
                {
                    continue;
                }

                entries.Add(new NodeTypeIconLibrarySO.NodeTypeIconEntry
                {
                    nodeType = ParseEnum(
                        item.nodeType,
                        RoundNodeType.None),
                    icon = FindSprite(item.icon)
                });
            }
        }

        SetSerializedList(
            library,
            "icons",
            entries);

        EditorUtility.SetDirty(library);
        return library;
    }

    private static void SetSerializedList<T>(
        ScriptableObject target,
        string fieldName,
        List<T> values)
    {
        if (target == null)
        {
            return;
        }

        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(fieldName);

        if (property == null || !property.isArray)
        {
            Debug.LogWarning(
                $"[LibraryGenerator] Serialized list not found. target={target.name} field={fieldName}");
            return;
        }

        property.ClearArray();

        for (int i = 0; i < values.Count; i++)
        {
            property.InsertArrayElementAtIndex(i);
            SerializedProperty element = property.GetArrayElementAtIndex(i);

            SetSerializedElement(element, values[i]);
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedElement<T>(
        SerializedProperty element,
        T value)
    {
        if (element == null || value == null)
        {
            return;
        }

        Type valueType = value.GetType();
        System.Reflection.FieldInfo[] fields = valueType.GetFields(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic);

        for (int i = 0; i < fields.Length; i++)
        {
            System.Reflection.FieldInfo field = fields[i];
            SerializedProperty child = element.FindPropertyRelative(field.Name);

            if (child == null)
            {
                continue;
            }

            object fieldValue = field.GetValue(value);
            SetSerializedPropertyValue(child, fieldValue);
        }
    }

    private static void SetSerializedPropertyValue(
        SerializedProperty property,
        object value)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Enum:
                SetEnumSerializedPropertyValue(property, value);
                break;

            case SerializedPropertyType.String:
                property.stringValue = value as string ?? string.Empty;
                break;

            case SerializedPropertyType.ObjectReference:
                property.objectReferenceValue = value as UnityEngine.Object;
                break;
        }
    }

    private static void SetEnumSerializedPropertyValue(
        SerializedProperty property,
        object value)
    {
        if (property == null || value == null)
        {
            return;
        }

        string enumName = value.ToString();

        for (int i = 0; i < property.enumNames.Length; i++)
        {
            if (property.enumNames[i].Equals(
                    enumName,
                    StringComparison.OrdinalIgnoreCase))
            {
                property.enumValueIndex = i;
                return;
            }
        }

        Debug.LogWarning(
            $"[LibraryGenerator] Enum value not found in SerializedProperty. property={property.name} value={enumName}");
    }

    private static Sprite FindSprite(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        string[] guids = AssetDatabase.FindAssets(key);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);

            Sprite directSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (IsMatchingSprite(directSprite, key, path))
            {
                return directSprite;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            for (int j = 0; j < assets.Length; j++)
            {
                Sprite sprite = assets[j] as Sprite;

                if (IsMatchingSprite(sprite, key, path))
                {
                    return sprite;
                }
            }
        }

        Debug.LogWarning($"[LibraryGenerator] Sprite not found. key={key}");
        return null;
    }

    private static bool IsMatchingSprite(
        Sprite sprite,
        string key,
        string path)
    {
        if (sprite == null || string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (sprite.name.Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(path) &&
            Path.GetFileNameWithoutExtension(path).Equals(key, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static TEnum ParseEnum<TEnum>(
        string value,
        TEnum fallback)
        where TEnum : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            return fallback;
        }

        if (Enum.TryParse(value, true, out TEnum result))
        {
            return result;
        }

        Debug.LogWarning($"[LibraryGenerator] Failed to parse enum. type={typeof(TEnum).Name} value={value}");
        return fallback;
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Replace("\\", "/");
    }

    [Serializable]
    private class LibraryJsonData
    {
        public List<RewardVisualJsonData> rewardVisuals = new();
        public List<NodeTypeIconJsonData> nodeTypeIcons = new();
    }

    [Serializable]
    private class RewardVisualJsonData
    {
        public string rewardType;
        public string localizationMainKey;
        public string icon;
    }

    [Serializable]
    private class NodeTypeIconJsonData
    {
        public string nodeType;
        public string icon;
    }
}